// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK

namespace OpenLiveWriter.BlogClient.Detection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using mshtml;

    using OpenLiveWriter.BlogClient.Clients;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.Progress;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.HtmlParser.Parser;
    using OpenLiveWriter.Localization;

    public class BlogServiceDetector : BlogServiceDetectorBase
    {
        private IBlogSettingsAccessor _blogSettings;

        public BlogServiceDetector(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, IBlogSettingsAccessor blogSettings, IBlogCredentialsAccessor credentials)
            : base(uiContext, hiddenBrowserParentControl, blogSettings.Id, blogSettings.HomepageUrl, credentials)
        {
            this._blogSettings = blogSettings;
        }

        protected override object DetectBlogService(IProgressHost progressHost)
        {
            using (var uiContextScope = new BlogClientUIContextSilentMode()) //suppress prompting for credentials
            {
                try
                {
                    // get the weblog homepage and rsd service description if available
                    var weblogDOM = this.GetWeblogHomepageDOM(progressHost);

                    // while we have the DOM available, scan for a writer manifest url
                    if (this.manifestDownloadInfo == null)
                    {
                        var manifestUrl = WriterEditingManifest.DiscoverUrl(this.homepageUrl, weblogDOM);
                        if (manifestUrl != string.Empty)
                        {
                            this.manifestDownloadInfo = new WriterEditingManifestDownloadInfo(manifestUrl);
                        }
                    }

                    var html = weblogDOM != null ? HTMLDocumentHelper.HTMLDocToString(weblogDOM) : null;

                    var detectionSucceeded = false;

                    if (!detectionSucceeded)
                    {
                        detectionSucceeded = this.AttemptGenericAtomLinkDetection(this.homepageUrl, html, !ApplicationDiagnostics.PreferAtom);
                    }

                    if (!detectionSucceeded && this._blogSettings.IsGoogleBloggerBlog)
                    {
                        detectionSucceeded = this.AttemptBloggerDetection(this.homepageUrl, html);
                    }

                    if (!detectionSucceeded)
                    {
                        var rsdServiceDescription = this.GetRsdServiceDescription(progressHost, weblogDOM);

                        // if there was no rsd service description or we fail to auto-configure from the
                        // rsd description then move on to other auto-detection techniques
                        if (!(detectionSucceeded = this.AttemptRsdBasedDetection(progressHost, rsdServiceDescription)))
                        {
                            // try detection by analyzing the homepage url and contents
                            this.UpdateProgress(progressHost, 75, Res.Get(StringId.ProgressAnalyzingHomepage));
                            if (weblogDOM != null)
                            {
                                detectionSucceeded = this.AttemptHomepageBasedDetection(this.homepageUrl, html);
                            }
                            else
                            {
                                detectionSucceeded = this.AttemptUrlBasedDetection(this.homepageUrl);
                            }

                            // if we successfully detected then see if we can narrow down
                            // to a specific weblog
                            if (detectionSucceeded)
                            {
                                if (!BlogProviderParameters.UrlContainsParameters(this.postApiUrl))
                                {
                                    // we detected the provider, now see if we can detect the weblog id
                                    // (or at lease the list of the user's weblogs)
                                    this.UpdateProgress(progressHost, 80, Res.Get(StringId.ProgressAnalyzingWeblogList));
                                    this.AttemptUserBlogDetection();
                                }
                            }
                        }
                    }

                    if (!detectionSucceeded && html != null)
                    {
                        this.AttemptGenericAtomLinkDetection(this.homepageUrl, html, false);
                    }

                    // finished
                    this.UpdateProgress(progressHost, 100, string.Empty);
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    this.Cancel();
                    // WasCancelled == true
                }
                catch (BlogAccountDetectorException ex)
                {
                    if (ApplicationDiagnostics.AutomationMode)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                    else
                    {
                        Trace.Fail(ex.ToString());
                    }
                    // ErrorOccurred == true
                }
                catch (Exception ex)
                {
                    // ErrorOccurred == true
                    Trace.Fail(ex.Message, ex.ToString());
                    this.ReportError(MessageId.WeblogDetectionUnexpectedError, ex.Message);
                }

                return this;
            }
        }

        private bool AttemptGenericAtomLinkDetection(string url, string html, bool preferredOnly)
        {
            const string GENERIC_ATOM_PROVIDER_ID = "D48F1B5A-06E6-4f0f-BD76-74F34F520792";

            if (html == null)
            {
                return false;
            }

            var ex = new HtmlExtractor(html);
            if (ex
                .SeekWithin("<head>", "<body>")
                .SeekWithin("<link href rel='service' type='application/atomsvc+xml'>", "</head>")
                .Success)
            {
                var atomProvider = BlogProviderManager.FindProvider(GENERIC_ATOM_PROVIDER_ID);

                var bt = ex.Element as BeginTag;

                if (preferredOnly)
                {
                    var classes = bt.GetAttributeValue("class");
                    if (classes == null)
                    {
                        return false;
                    }

                    if (!Regex.IsMatch(classes, @"\bpreferred\b"))
                    {
                        return false;
                    }
                }

                var linkUrl = bt.GetAttributeValue("href");

                Debug.WriteLine("Atom service link detected in the blog homepage");

                this.providerId = atomProvider.Id;
                this.serviceName = atomProvider.Name;
                this.clientType = atomProvider.ClientType;
                this.blogName = string.Empty;
                this.postApiUrl = this.GetAbsoluteUrl(url, linkUrl);

                var client = BlogClientManager.CreateClient(atomProvider.ClientType, this.postApiUrl, this.credentials);
                client.VerifyCredentials();
                this.usersBlogs = client.GetUsersBlogs();
                if (this.usersBlogs.Length == 1)
                {
                    this.hostBlogId = this.usersBlogs[0].Id;
                    this.blogName = this.usersBlogs[0].Name;
                    /*
                                        if (_usersBlogs[0].HomepageUrl != null && _usersBlogs[0].HomepageUrl.Length > 0)
                                            _homepageUrl = _usersBlogs[0].HomepageUrl;
                    */
                }

                // attempt to read the blog name from the homepage title
                if (this.blogName == null || this.blogName.Length == 0)
                {
                    var ex2 = new HtmlExtractor(html);
                    if (ex2.Seek("<title>").Success)
                    {
                        this.blogName = ex2.CollectTextUntil("title");
                    }
                }

                return true;
            }
            return false;
        }

        private string GetAbsoluteUrl(string url, string linkUrl)
        {
            Uri absoluteUrl;
            if (Uri.TryCreate(linkUrl, UriKind.Absolute, out absoluteUrl))
            {
                return linkUrl;
            }

            var baseUrl = new Uri(url);
            absoluteUrl = new Uri(baseUrl, linkUrl);
            return absoluteUrl.AbsoluteUri;
        }

        private class BloggerGeneratorCriterion : IElementPredicate
        {
            public bool IsMatch(Element e)
            {
                var tag = e as BeginTag;
                if (tag == null)
                {
                    return false;
                }

                if (!tag.NameEquals("meta"))
                {
                    return false;
                }

                if (tag.GetAttributeValue("name") != "generator")
                {
                    return false;
                }

                var generator = tag.GetAttributeValue("content");
                if (generator == null || CaseInsensitiveComparer.DefaultInvariant.Compare("blogger", generator) != 0)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Do special Blogger-specific detection logic.  We want to
        /// use the Blogger Atom endpoints specified in the HTML, not
        /// the Blogger endpoint in the RSD.
        /// </summary>
        private bool AttemptBloggerDetection(string homepageUrl, string html)
        {
            Debug.Assert(string.IsNullOrEmpty(homepageUrl), "Google Blogger blogs don't know the homepageUrl");
            Debug.Assert(string.IsNullOrEmpty(html), "Google Blogger blogs don't know the homepageUrl");

            const string BLOGGER_V3_PROVIDER_ID = "343F1D83-1098-43F4-AE86-93AFC7602855";
            var bloggerProvider = BlogProviderManager.FindProvider(BLOGGER_V3_PROVIDER_ID);
            if (bloggerProvider == null)
            {
                Trace.Fail("Couldn't retrieve Blogger provider");
                return false;
            }

            var blogAccountDetector = new BlogAccountDetector(bloggerProvider.ClientType, bloggerProvider.PostApiUrl, this.credentials);
            if (blogAccountDetector.ValidateService())
            {
                this.CopySettingsFromProvider(bloggerProvider);

                this.usersBlogs = blogAccountDetector.UsersBlogs;
                if (this.usersBlogs.Length == 1)
                {
                    this.hostBlogId = this.usersBlogs[0].Id;
                    this.blogName = this.usersBlogs[0].Name;
                    this.homepageUrl = this.usersBlogs[0].HomepageUrl;
                }

                // If we didn't find the specific blog, we'll prompt the user with the list of blogs
                return true;
            }
            else
            {
                this.AuthenticationErrorOccurred = blogAccountDetector.Exception is BlogClientAuthenticationException;
                this.ReportErrorAndFail(blogAccountDetector.ErrorMessageType, blogAccountDetector.ErrorMessageParams);
                return false;
            }
        }

        private bool AttemptRsdBasedDetection(IProgressHost progressHost, RsdServiceDescription rsdServiceDescription)
        {
            // always return alse for null description
            if (rsdServiceDescription == null)
            {
                return false;
            }

            var providerId = string.Empty;
            BlogAccount blogAccount = null;

            // check for a match on rsd engine link
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
            {
                blogAccount = provider.DetectAccountFromRsdHomepageLink(rsdServiceDescription);
                if (blogAccount != null)
                {
                    providerId = provider.Id;
                    break;
                }
            }

            // if none found on engine link, match on engine name
            if (blogAccount == null)
            {
                foreach (IBlogProvider provider in BlogProviderManager.Providers)
                {
                    blogAccount = provider.DetectAccountFromRsdEngineName(rsdServiceDescription);
                    if (blogAccount != null)
                    {
                        providerId = provider.Id;
                        break;
                    }
                }
            }

            // No provider associated with the RSD file, try to gin one up (will only
            // work if the RSD file contains an API for one of our supported client types)
            if (blogAccount == null)
            {
                // try to create one from RSD
                blogAccount = BlogAccountFromRsdServiceDescription.Create(rsdServiceDescription);
            }

            // if we have an rsd-detected weblog
            if (blogAccount != null)
            {
                // confirm that the credentials are OK
                this.UpdateProgress(progressHost, 65, Res.Get(StringId.ProgressVerifyingInterface));
                var blogAccountDetector = new BlogAccountDetector(
                    blogAccount.ClientType, blogAccount.PostApiUrl, this.credentials);

                if (blogAccountDetector.ValidateService())
                {
                    // copy basic account info
                    this.providerId = providerId;
                    this.serviceName = blogAccount.ServiceName;
                    this.clientType = blogAccount.ClientType;
                    this.hostBlogId = blogAccount.BlogId;
                    this.postApiUrl = blogAccount.PostApiUrl;

                    // see if we can improve on the blog name guess we already
                    // have from the <title> element of the homepage
                    var blogInfo = blogAccountDetector.DetectAccount(this.homepageUrl, this.hostBlogId);
                    if (blogInfo != null)
                    {
                        this.blogName = blogInfo.Name;
                    }
                }
                else
                {
                    // report user-authorization error
                    this.ReportErrorAndFail(blogAccountDetector.ErrorMessageType, blogAccountDetector.ErrorMessageParams);
                }

                // success!
                return true;
            }
            else
            {
                // couldn't do it
                return false;
            }
        }

        private bool AttemptUrlBasedDetection(string url)
        {
            // matched provider
            IBlogProvider blogAccountProvider = null;

            // do url-based matching
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
            {
                if (provider.IsProviderForHomepageUrl(url))
                {
                    blogAccountProvider = provider;
                    break;
                }
            }

            if (blogAccountProvider != null)
            {
                this.CopySettingsFromProvider(blogAccountProvider);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AttemptContentBasedDetection(string homepageContent)
        {
            // matched provider
            IBlogProvider blogAccountProvider = null;

            // do url-based matching
            foreach (IBlogProvider provider in BlogProviderManager.Providers)
            {
                if (provider.IsProviderForHomepageContent(homepageContent))
                {
                    blogAccountProvider = provider;
                    break;
                }
            }

            if (blogAccountProvider != null)
            {
                this.CopySettingsFromProvider(blogAccountProvider);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool AttemptHomepageBasedDetection(string homepageUrl, string homepageContent)
        {
            if (this.AttemptUrlBasedDetection(homepageUrl))
            {
                return true;
            }
            else
            {
                return this.AttemptContentBasedDetection(homepageContent);
            }
        }

        private RsdServiceDescription GetRsdServiceDescription(IProgressHost progressHost, IHTMLDocument2 weblogDOM)
        {
            if (weblogDOM != null)
            {
                // try to download an RSD description
                this.UpdateProgress(progressHost, 50, Res.Get(StringId.ProgressAnalyzingInterface));
                return RsdServiceDetector.DetectFromWeblog(this.homepageUrl, weblogDOM);
            }
            else
            {
                return null;
            }
        }

        private class BlogAccountFromRsdServiceDescription : BlogAccount
        {
            public static BlogAccount Create(RsdServiceDescription rsdServiceDescription)
            {
                try
                {
                    return new BlogAccountFromRsdServiceDescription(rsdServiceDescription);
                }
                catch (NoSupportedRsdClientTypeException)
                {
                    return null;
                }
            }

            private BlogAccountFromRsdServiceDescription(RsdServiceDescription rsdServiceDescription)
            {
                // look for supported apis from highest fidelity to lowest
                var rsdApi = rsdServiceDescription.ScanForApi("WordPress");
                if (rsdApi == null)
                {
                    rsdApi = rsdServiceDescription.ScanForApi("MovableType");
                }

                if (rsdApi == null)
                {
                    rsdApi = rsdServiceDescription.ScanForApi("MetaWeblog");
                }

                if (rsdApi != null)
                {
                    this.Init(rsdServiceDescription.EngineName, rsdApi.Name, rsdApi.ApiLink, rsdApi.BlogId);
                    return;
                }
                else
                {
                    // couldn't find a supported api type so we fall through to here
                    throw new NoSupportedRsdClientTypeException();
                }
            }

        }

        private class NoSupportedRsdClientTypeException : ApplicationException
        {
            public NoSupportedRsdClientTypeException()
                : base("No supported Rsd client-type")
            {
            }
        }
    }

    /// <summary>
    /// Blog settings detector for SharePoint blogs.
    /// </summary>
    public class SharePointBlogDetector : BlogServiceDetectorBase
    {
        private IBlogCredentials blogCredentials;
        public SharePointBlogDetector(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, string localBlogId, string homepageUrl, IBlogCredentialsAccessor credentials, IBlogCredentials blogCredentials)
            : base(uiContext, hiddenBrowserParentControl, localBlogId, homepageUrl, credentials)
        {
            this.blogCredentials = blogCredentials;
        }

        protected override object DetectBlogService(IProgressHost progressHost)
        {
            using (var uiContextScope = new BlogClientUIContextSilentMode()) //suppress prompting for credentials
            {
                try
                {
                    // copy basic account info
                    var provider = BlogProviderManager.FindProvider("4AA58E69-8C24-40b1-BACE-3BB14237E8F9");
                    this.providerId = provider.Id;
                    this.serviceName = provider.Name;
                    this.clientType = provider.ClientType;

                    //calculate the API url based on the homepage Url.
                    //  API URL Format: <blogurl>/_layouts/metaweblog.aspx
                    var homepagePath = UrlHelper.SafeToAbsoluteUri(new Uri(this.homepageUrl)).Split('?')[0];
                    if (homepagePath == null)
                    {
                        homepagePath = "/";
                    }

                    //trim off any file information included in the URL (ex: /default.aspx)
                    var lastPathPartIndex = homepagePath.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    if (lastPathPartIndex != -1)
                    {
                        var lastPathPart = homepagePath.Substring(lastPathPartIndex);
                        if (lastPathPart.IndexOf('.') != -1)
                        {
                            homepagePath = homepagePath.Substring(0, lastPathPartIndex);
                            if (homepagePath == string.Empty)
                            {
                                homepagePath = "/";
                            }
                        }
                    }
                    if (homepagePath != "/" && homepagePath.EndsWith("/", StringComparison.OrdinalIgnoreCase)) //trim off trailing slash
                    {
                        homepagePath = homepagePath.Substring(0, homepagePath.Length - 1);
                    }

                    //Update the homepage url
                    this.homepageUrl = homepagePath;

                    this.postApiUrl = string.Format(CultureInfo.InvariantCulture, "{0}/_layouts/metaweblog.aspx", homepagePath);

                    if (VerifyCredentialsAndDetectAuthScheme(this.postApiUrl, this.blogCredentials, this.credentials))
                    {
                        this.AuthenticationErrorOccurred = false;
                        //detect the user's blog ID.
                        if (!BlogProviderParameters.UrlContainsParameters(this.postApiUrl))
                        {
                            // we detected the provider, now see if we can detect the weblog id
                            // (or at lease the list of the user's weblogs)
                            this.UpdateProgress(progressHost, 80, Res.Get(StringId.ProgressAnalyzingWeblogList));
                            this.AttemptUserBlogDetection();
                        }
                    }
                    else
                    {
                        this.AuthenticationErrorOccurred = true;
                    }
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    this.Cancel();
                    // WasCancelled == true
                }
                catch (BlogAccountDetectorException)
                {
                    // ErrorOccurred == true
                }
                catch (BlogClientAuthenticationException)
                {
                    this.AuthenticationErrorOccurred = true;
                    // ErrorOccurred == true
                }
                catch (Exception ex)
                {
                    // ErrorOccurred == true
                    this.ReportError(MessageId.WeblogDetectionUnexpectedError, ex.Message);
                }
            }
            return this;
        }

        /*private string DiscoverPostApiUrl(string baseUrl, string blogPath)
        {

        }*/

        /// <summary>
        /// Verifies the user credentials and determines whether SharePoint is configure to use HTTP or MetaWeblog authentication
        /// </summary>
        /// <param name="postApiUrl"></param>
        /// <param name="blogCredentials"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        private static bool VerifyCredentialsAndDetectAuthScheme(string postApiUrl, IBlogCredentials blogCredentials, IBlogCredentialsAccessor credentials)
        {
            var blogClientAttr = (BlogClientAttribute)typeof(SharePointClient).GetCustomAttributes(typeof(BlogClientAttribute), false)[0];
            var client = (SharePointClient)BlogClientManager.CreateClient(blogClientAttr.TypeName, postApiUrl, credentials);

            return SharePointClient.VerifyCredentialsAndDetectAuthScheme(blogCredentials, client);
        }
    }

    public abstract class BlogServiceDetectorBase : MultipartAsyncOperation, ITemporaryBlogSettingsDetectionContext
    {
        public BlogServiceDetectorBase(IBlogClientUIContext uiContext, Control hiddenBrowserParentControl, string localBlogId, string homepageUrl, IBlogCredentialsAccessor credentials)
            : base(uiContext)
        {
            // save references
            this.uiContext = uiContext;
            this.localBlogId = localBlogId;
            this.homepageUrl = homepageUrl;
            this.credentials = credentials;

            // add blog service detection
            this.AddProgressOperation(
                new ProgressOperation(this.DetectBlogService),
                35);

            // add settings downloading (note: this operation will be a no-op
            // in the case where we don't successfully detect a weblog)
            this.AddProgressOperation(
                new ProgressOperation(this.DetectWeblogSettings),
                new ProgressOperationCompleted(this.DetectWeblogSettingsCompleted),
                30);

            // add template downloading (note: this operation will be a no-op in the
            // case where we don't successfully detect a weblog)
            this.blogEditingTemplateDetector = new BlogEditingTemplateDetector(uiContext, hiddenBrowserParentControl);
            this.AddProgressOperation(
                new ProgressOperation(this.blogEditingTemplateDetector.DetectTemplate),
                35);
        }

        public BlogInfo[] UsersBlogs => this.usersBlogs;

        public string ProviderId => this.providerId;

        public string ServiceName => this.serviceName;

        public string ClientType => this.clientType;

        public string PostApiUrl => this.postApiUrl;

        public string HostBlogId => this.hostBlogId;

        public string BlogName => this.blogName;

        public IDictionary<string, string> OptionOverrides => this.optionOverrides;

        public IDictionary<string, string> HomePageOverrides => this.homePageOverrides;

        public IDictionary<string, string> UserOptionOverrides => null;

        public IBlogProviderButtonDescription[] ButtonDescriptions { get; private set; } = null;

        public BlogPostCategory[] Categories { get; private set; } = null;

        public BlogPostKeyword[] Keywords { get; private set; } = null;

        public byte[] FavIcon { get; private set; } = null;

        public byte[] Image { get; private set; } = null;

        public byte[] WatermarkImage { get; private set; } = null;

        public BlogEditingTemplateFile[] BlogTemplateFiles => this.blogEditingTemplateDetector.BlogTemplateFiles;

        public Color? PostBodyBackgroundColor => this.blogEditingTemplateDetector.PostBodyBackgroundColor;

        public bool WasCancelled => this.CancelRequested;

        public bool ErrorOccurred => this.errorMessageType != MessageId.None;

        public bool AuthenticationErrorOccurred { get; set; } = false;

        public bool TemplateDownloadFailed => this.blogEditingTemplateDetector.ExceptionOccurred;

        IBlogCredentialsAccessor IBlogSettingsDetectionContext.Credentials => this.credentials;

        string IBlogSettingsDetectionContext.HomepageUrl => this.homepageUrl;

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo
        {
            get => this.manifestDownloadInfo;
            set => this.manifestDownloadInfo = value;
        }

        string IBlogSettingsDetectionContext.ClientType
        {
            get => this.clientType;
            set => this.clientType = value;
        }

        byte[] IBlogSettingsDetectionContext.FavIcon
        {
            get => this.FavIcon;
            set => this.FavIcon = value;
        }

        byte[] IBlogSettingsDetectionContext.Image
        {
            get => this.Image;
            set => this.Image = value;
        }

        byte[] IBlogSettingsDetectionContext.WatermarkImage
        {
            get => this.WatermarkImage;
            set => this.WatermarkImage = value;
        }

        BlogPostCategory[] IBlogSettingsDetectionContext.Categories
        {
            get => this.Categories;
            set => this.Categories = value;
        }

        BlogPostKeyword[] IBlogSettingsDetectionContext.Keywords
        {
            get => this.Keywords;
            set => this.Keywords = value;
        }

        IDictionary<string, string> IBlogSettingsDetectionContext.OptionOverrides
        {
            get => this.optionOverrides;
            set => this.optionOverrides = value;
        }

        IDictionary<string, string> IBlogSettingsDetectionContext.HomePageOverrides
        {
            get => this.homePageOverrides;
            set => this.homePageOverrides = value;
        }

        IBlogProviderButtonDescription[] IBlogSettingsDetectionContext.ButtonDescriptions
        {
            get => this.ButtonDescriptions;
            set => this.ButtonDescriptions = value;
        }

        public BlogInfo[] AvailableImageEndpoints { get; set; }

        public void ShowLastError(IWin32Window owner)
        {
            if (this.ErrorOccurred)
            {
                DisplayMessage.Show(this.errorMessageType, owner, this.errorMessageParams);
            }
            else
            {
                Trace.Fail("Called ShowLastError when no error occurred");
            }
        }

        public static byte[] SafeDownloadFavIcon(string homepageUrl)
        {
            try
            {
                var favIconUrl = UrlHelper.UrlCombine(homepageUrl, "favicon.ico");
                using (var favIconStream = HttpRequestHelper.SafeDownloadFile(favIconUrl))
                using (var memoryStream = new MemoryStream())
                {
                    StreamHelper.Transfer(favIconStream, memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }

        protected abstract object DetectBlogService(IProgressHost progressHost);

        protected void AttemptUserBlogDetection()
        {
            var blogAccountDetector = new BlogAccountDetector(
                this.clientType, this.postApiUrl, this.credentials);

            if (blogAccountDetector.ValidateService())
            {
                var blogInfo = blogAccountDetector.DetectAccount(this.homepageUrl, this.hostBlogId);
                if (blogInfo != null)
                {
                    // save the detected info
                    // TODO: Commenting out next line for Spaces demo tomorrow.
                    // need to decide whether to keep it commented out going forward.
                    // _homepageUrl = blogInfo.HomepageUrl;
                    this.hostBlogId = blogInfo.Id;
                    this.blogName = blogInfo.Name;
                }

                // always save the list of user's blogs
                this.usersBlogs = blogAccountDetector.UsersBlogs;
            }
            else
            {
                this.AuthenticationErrorOccurred = blogAccountDetector.Exception is BlogClientAuthenticationException;
                this.ReportErrorAndFail(blogAccountDetector.ErrorMessageType, blogAccountDetector.ErrorMessageParams);
            }
        }

        protected IHTMLDocument2 GetWeblogHomepageDOM(IProgressHost progressHost)
        {
            // try download the weblog home page
            this.UpdateProgress(progressHost, 25, Res.Get(StringId.ProgressAnalyzingHomepage));
            var weblogDOM = HTMLDocumentHelper.SafeGetHTMLDocumentFromUrl(this.homepageUrl, out var responseUri);
            if (responseUri != null && responseUri != this.homepageUrl)
            {
                this.homepageUrl = responseUri;
            }

            if (weblogDOM != null)
            {
                // default the blog name to the title of the document
                if (weblogDOM.title != null)
                {
                    this.blogName = weblogDOM.title;

                    // drop anything to the right of a "|", as it usually is a site name
                    var index = this.blogName.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                    if (index > 0)
                    {
                        var newname = this.blogName.Substring(0, index).Trim();
                        if (newname != string.Empty)
                        {
                            this.blogName = newname;
                        }
                    }
                }
            }

            return weblogDOM;
        }

        protected void CopySettingsFromProvider(IBlogProvider blogAccountProvider)
        {
            this.providerId = blogAccountProvider.Id;
            this.serviceName = blogAccountProvider.Name;
            this.clientType = blogAccountProvider.ClientType;
            this.postApiUrl = this.ProcessPostUrlMacros(blogAccountProvider.PostApiUrl);
        }

        private string ProcessPostUrlMacros(string postApiUrl) => postApiUrl.Replace("<username>", this.credentials.Username);

        private object DetectWeblogSettings(IProgressHost progressHost)
        {
            using (var uiContextScope = new BlogClientUIContextSilentMode()) //suppress prompting for credentials
            {
                // no-op if we don't have a blog-id to work with
                if (this.HostBlogId == string.Empty)
                {
                    return this;
                }

                try
                {
                    // detect settings
                    var blogSettingsDetector = new BlogSettingsDetector(this);
                    blogSettingsDetector.DetectSettings(progressHost);
                }
                catch (OperationCancelledException)
                {
                    // WasCancelled == true
                }
                catch (BlogClientOperationCancelledException)
                {
                    this.Cancel();
                    // WasCancelled == true
                }
                catch (Exception ex)
                {
                    Trace.Fail("Unexpected error occurred while detecting weblog settings: " + ex.ToString());
                }

                return this;
            }
        }

        private void DetectWeblogSettingsCompleted(object result)
        {
            // no-op if we don't have a blog detected
            if (this.HostBlogId == string.Empty)
            {
                return;
            }

            // get the editing template directory
            var blogTemplateDir = BlogEditingTemplate.GetBlogTemplateDir(this.localBlogId);

            // set context for template detector
            var blogAccount = new BlogAccount(this.ServiceName, this.ClientType, this.PostApiUrl, this.HostBlogId);
            this.blogEditingTemplateDetector.SetContext(
                blogAccount,
                this.credentials,
                this.homepageUrl,
                blogTemplateDir,
                this.manifestDownloadInfo,
                false,
                this.providerId,
                this.optionOverrides,
                null,
                this.homePageOverrides);
        }

        protected void UpdateProgress(IProgressHost progressHost, int percent, string message)
        {
            if (this.CancelRequested)
            {
                throw new OperationCancelledException();
            }

            progressHost.UpdateProgress(percent, 100, message);
        }

        protected void ReportError(MessageId errorMessageType, params object[] errorMessageParams)
        {
            this.errorMessageType = errorMessageType;
            this.errorMessageParams = errorMessageParams;
        }

        protected void ReportErrorAndFail(MessageId errorMessageType, params object[] errorMessageParams)
        {
            this.ReportError(errorMessageType, errorMessageParams);
            throw new BlogAccountDetectorException();
        }

        protected class BlogAccountDetectorException : ApplicationException
        {
            public BlogAccountDetectorException() : base("Blog account detector did not succeed")
            {
            }
        }

        /// <summary>
        /// Blog account we are scanning
        /// </summary>
        protected string localBlogId;
        protected string homepageUrl;
        protected WriterEditingManifestDownloadInfo manifestDownloadInfo = null;
        protected IBlogCredentialsAccessor credentials;

        // BlogTemplateDetector
        private BlogEditingTemplateDetector blogEditingTemplateDetector;

        /// <summary>
        /// Results of scanning
        /// </summary>
        protected string providerId = string.Empty;
        protected string serviceName = string.Empty;
        protected string clientType = string.Empty;
        protected string postApiUrl = string.Empty;
        protected string hostBlogId = string.Empty;
        protected string blogName = string.Empty;

        protected BlogInfo[] usersBlogs = new BlogInfo[] { };

        // if we are unable to detect these values then leave them null
        // as an indicator that their values are "unknown" vs. "empty"
        // callers can then choose to not overwrite any existing settings
        // in this case
        protected IDictionary<string, string> homePageOverrides = null;
        protected IDictionary<string, string> optionOverrides = null;

        // error info
        private MessageId errorMessageType;
        private object[] errorMessageParams;
        protected IBlogClientUIContext uiContext;
    }
}
