// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Detection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;

    using mshtml;

    using OpenLiveWriter.BlogClient.Clients;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Progress;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Mshtml;

    public class BlogEditingTemplateFile
    {
        public BlogEditingTemplateFile(BlogEditingTemplateType type, string file)
        {
            this.TemplateType = type;
            this.TemplateFile = file;
        }
        public readonly BlogEditingTemplateType TemplateType;
        public readonly string TemplateFile;
    }
    public enum BlogEditingTemplateType { Normal, Styled, Framed, Webpage };

    /// <summary>
    /// Manages the creation of a blog editor template based on the styles in a weblog.
    /// This downloader will publish an temporary post to a weblog, and then re-download
    /// the post so that the surrounding HTML can be parsed back out.
    /// </summary>
    public class BlogEditingTemplateDetector
    {

        public static BlogEditingTemplateFile[] DetectTemplate(IBlogClientUIContext uiContext, Control parentControl, IBlogSettingsAccessor blogSettings, bool probeForManifest, out Color? postBodyBackgroundColor)
        {
            postBodyBackgroundColor = null;

            try
            {
                // create a new detector
                BlogEditingTemplateDetector detector = new BlogEditingTemplateDetector(uiContext, parentControl, blogSettings, probeForManifest);

                // execute with a progress dialog
                ProgressHelper.ExecuteWithProgress(
                    Res.Get(StringId.DownloadingWeblogStyle),
                    new ProgressOperation(detector.DetectTemplate),
                    uiContext,
                    uiContext);

                // propagate exception
                if (detector.ExceptionOccurred)
                    throw detector.Exception;

                postBodyBackgroundColor = detector.PostBodyBackgroundColor;
                // return the template
                return detector.BlogTemplateFiles;
            }
            catch (OperationCancelledException)
            {
                return new BlogEditingTemplateFile[0];
            }
            catch (BlogClientOperationCancelledException)
            {
                Debug.WriteLine("BlogClient operation cancelled");
                return new BlogEditingTemplateFile[0];
            }
            catch (Exception e)
            {
                Trace.Fail("Error occurred while downloading weblog style" + e.ToString());
                DisplayMessage.Show(MessageId.TemplateDownloadFailed);
                return new BlogEditingTemplateFile[0];
            }

        }

        private IBlogClientUIContext _uiContext;

        public BlogEditingTemplateDetector(IBlogClientUIContext uiContext, Control parentControl, IBlogSettingsAccessor blogSettings, bool probeForManifest)
            : this(uiContext, parentControl)
        {
            BlogAccount blogAccount = new BlogAccount(blogSettings.ServiceName, blogSettings.ClientType, blogSettings.PostApiUrl, blogSettings.HostBlogId);
            string blogTemplateDir = BlogEditingTemplate.GetBlogTemplateDir(blogSettings.Id);
            this.SetContext(blogAccount, blogSettings.Credentials, blogSettings.HomepageUrl, blogTemplateDir, blogSettings.ManifestDownloadInfo, probeForManifest, blogSettings.ProviderId, blogSettings.OptionOverrides, blogSettings.UserOptionOverrides, blogSettings.HomePageOverrides);
        }

        /// <summary>
        /// Initialize BlogTemplateDetector without providing context (if you do not call
        /// one of the SetContext methods prior to executing the Start method then the
        /// BlogTemplateDetector will be a no-op that does not detect and download the template)
        /// </summary>
        /// <param name="parentControl"></param>
        public BlogEditingTemplateDetector(IBlogClientUIContext uiContext, Control parentControl)
        {
            this._uiContext = uiContext;
            this._parentControl = parentControl;
        }

        /// <summary>
        /// SetContext using a weblog account
        /// </summary>
        public void SetContext(
            BlogAccount blogAccount,
            IBlogCredentialsAccessor credentials,
            string blogHomepageUrl,
            string blogTemplateDir,
            WriterEditingManifestDownloadInfo manifestDownloadInfo,
            bool probeForManifest,
            string providerId,
            IDictionary<string, string> optionOverrides,
            IDictionary<string, string> userOptionOverrides,
            IDictionary<string, string> homepageOptionOverrides)
        {
            // note context set
            this.contextSet = true;

            // create a blog client
            this.blogAccount = blogAccount;
            this.credentials = credentials;
            this.blogClient = BlogClientManager.CreateClient(
                blogAccount.ClientType,
                blogAccount.PostApiUrl,
                credentials,
                providerId,
                optionOverrides,
                userOptionOverrides,
                homepageOptionOverrides);

            // set other context that we've got
            this._blogHomepageUrl = blogHomepageUrl;
            this._blogTemplateDir = blogTemplateDir;
            this._manifestDownloadInfo = manifestDownloadInfo;
            this._probeForManifest = probeForManifest;
        }

        public BlogEditingTemplateFile[] BlogTemplateFiles
        {
            get
            {
                return this._blogTemplateFiles;
            }
        }

        public Color? PostBodyBackgroundColor
        {
            get
            {
                return this._postBodyBackgroundColor;
            }
        }

        public bool ExceptionOccurred
        {
            get { return this.Exception != null; }
        }

        public Exception Exception
        {
            get { return this._exception; }
        }
        private Exception _exception;

        private string _nextTryPostUrl;

        public object DetectTemplate(IProgressHost progress)
        {
            // if our context has not been set then just return without doing anything
            // (supports this being an optional step at the end of a chain of
            // other progress operations)
            if (this.contextSet == false)
                return this;

            using (BlogClientUIContextScope uiContextScope = new BlogClientUIContextScope(this._uiContext))
            {
                // initial progress
                progress.UpdateProgress(Res.Get(StringId.ProgressDetectingWeblogEditingStyle));

                // build list of detected templates
                ArrayList blogTemplateFiles = new ArrayList();

                // build list of template types that we need to auto-detect
                ArrayList detectionTargetTypes = new ArrayList();
                ArrayList detectionTargetStrategies = new ArrayList();

                // try explicit detection of templates
                BlogEditingTemplateFiles templateFiles = this.SafeGetTemplates(new ProgressTick(progress, 50, 100));

                // see if we got the FramedTemplate
                if (templateFiles.FramedTemplate != null)
                    blogTemplateFiles.Add(templateFiles.FramedTemplate);
                else
                {
                    detectionTargetTypes.Add(BlogEditingTemplateType.Framed);
                    detectionTargetStrategies.Add(BlogEditingTemplateStrategies.GetTemplateStrategy(BlogEditingTemplateStrategies.StrategyType.NoSiblings));
                }

                // see if we got the WebPageTemplate
                if (templateFiles.WebPageTemplate != null)
                    blogTemplateFiles.Add(templateFiles.WebPageTemplate);
                else
                {
                    detectionTargetTypes.Add(BlogEditingTemplateType.Webpage);
                    detectionTargetStrategies.Add(BlogEditingTemplateStrategies.GetTemplateStrategy(BlogEditingTemplateStrategies.StrategyType.Site));
                }

                // perform detection if we have detection targets
                if (detectionTargetTypes.Count > 0)
                {
                    BlogEditingTemplateFile[] detectedBlogTemplateFiles = this.DetectTemplates(new ProgressTick(progress, 50, 100),
                        detectionTargetTypes.ToArray(typeof(BlogEditingTemplateType)) as BlogEditingTemplateType[],
                        detectionTargetStrategies.ToArray(typeof(BlogEditingTemplateStrategy)) as BlogEditingTemplateStrategy[]);
                    if (detectedBlogTemplateFiles != null)
                        blogTemplateFiles.AddRange(detectedBlogTemplateFiles);
                }

                // updates member if we succeeded
                if (blogTemplateFiles.Count > 0)
                {
                    // capture template files
                    this._blogTemplateFiles = blogTemplateFiles.ToArray(typeof(BlogEditingTemplateFile)) as BlogEditingTemplateFile[];

                    // if we got at least one template by some method then clear any exception
                    // that occurs so we can at least update that template
                    this._exception = null;
                }

                foreach (BlogEditingTemplateFile file in blogTemplateFiles)
                {
                    if (file.TemplateType == BlogEditingTemplateType.Webpage)
                    {
                        this._postBodyBackgroundColor = BackgroundColorDetector.DetectColor(UrlHelper.SafeToAbsoluteUri(new Uri(file.TemplateFile)), this._postBodyBackgroundColor);
                    }
                }

                // return
                return this;
            }
        }

        private class BlogEditingTemplateFiles
        {
            public BlogEditingTemplateFile FramedTemplate;
            public BlogEditingTemplateFile WebPageTemplate;
        }

        private BlogEditingTemplateFiles SafeGetTemplates(IProgressHost progress)
        {
            WriterEditingManifest editingManifest = null;
            BlogEditingTemplateFiles templateFiles = new BlogEditingTemplateFiles();
            try
            {
                // if we have a manifest url then try to get our manifest
                if (this._manifestDownloadInfo != null)
                {
                    // try to get the editing manifest
                    string manifestUrl = this._manifestDownloadInfo.SourceUrl;
                    editingManifest = WriterEditingManifest.FromUrl(
                        new Uri(manifestUrl),
                        this.blogClient,
                        this.credentials,
                        true);

                    // progress
                    this.CheckCancelRequested(progress);
                    progress.UpdateProgress(20, 100);
                }

                // if we have no editing manifest then probe (if allowed)
                if ((editingManifest == null) && this._probeForManifest)
                {
                    editingManifest = WriterEditingManifest.FromHomepage(
                        new LazyHomepageDownloader(this._blogHomepageUrl, new HttpRequestHandler(this.blogClient.SendAuthenticatedHttpRequest)),
                        new Uri(this._blogHomepageUrl),
                        this.blogClient,
                        this.credentials);
                }

                // progress
                this.CheckCancelRequested(progress);
                progress.UpdateProgress(40, 100);

                // if we got one then return templates from it as-appropriate
                if (editingManifest != null)
                {
                    if (editingManifest.WebLayoutUrl != null)
                    {
                        string webLayoutTemplate = this.DownloadManifestTemplate(new ProgressTick(progress, 10, 100), editingManifest.WebLayoutUrl);
                        if (BlogEditingTemplate.ValidateTemplate(webLayoutTemplate))
                        {
                            // download supporting files
                            string templateFile = this.DownloadTemplateFiles(webLayoutTemplate, this._blogHomepageUrl, new ProgressTick(progress, 20, 100));

                            // return the template
                            templateFiles.FramedTemplate = new BlogEditingTemplateFile(BlogEditingTemplateType.Framed, templateFile);
                        }
                        else
                        {
                            Trace.WriteLine("Invalid webLayoutTemplate specified in manifest");
                        }
                    }

                    if (editingManifest.WebPreviewUrl != null)
                    {
                        string webPreviewTemplate = this.DownloadManifestTemplate(new ProgressTick(progress, 10, 100), editingManifest.WebPreviewUrl);
                        if (BlogEditingTemplate.ValidateTemplate(webPreviewTemplate))
                        {
                            // download supporting files
                            string templateFile = this.DownloadTemplateFiles(webPreviewTemplate, this._blogHomepageUrl, new ProgressTick(progress, 20, 100));

                            // return the template
                            templateFiles.WebPageTemplate = new BlogEditingTemplateFile(BlogEditingTemplateType.Webpage, templateFile);
                        }
                        else
                        {
                            Trace.WriteLine("Invalid webPreviewTemplate specified in manifest");
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                progress.UpdateProgress(100, 100);
            }

            return templateFiles;
        }

        private string DownloadManifestTemplate(IProgressHost progress, string manifestTemplateUrl)
        {
            try
            {
                // update progress
                progress.UpdateProgress(0, 100, Res.Get(StringId.ProgressDownloadingEditingTemplate));

                // process any parameters within the url
                string templateUrl = BlogClientHelper.FormatUrl(manifestTemplateUrl, this._blogHomepageUrl, this.blogAccount.PostApiUrl, this.blogAccount.BlogId);

                // download the url
                using (StreamReader streamReader = new StreamReader(this.blogClient.SendAuthenticatedHttpRequest(templateUrl, 20000, null).GetResponseStream()))
                    return streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception occurred while attempting to download template from " + manifestTemplateUrl + " :" + ex.ToString());
                return null;
            }
            finally
            {
                progress.UpdateProgress(100, 100);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="targetTemplateTypes"></param>
        /// <param name="templateStrategies">equivalent strategies for manipulating the blog homepage DOM into an editing template type</param>
        /// <returns></returns>
        private BlogEditingTemplateFile[] DetectTemplates(IProgressHost progress, BlogEditingTemplateType[] targetTemplateTypes, BlogEditingTemplateStrategy[] templateStrategies)
        {
            RecentPostRegionLocatorStrategy recentPostLocatorStrategy =
                new RecentPostRegionLocatorStrategy(this.blogClient, this.blogAccount, this.credentials, this._blogHomepageUrl,
                                                    new PageDownloader(this.RequestPageDownload));

            TemporaryPostRegionLocatorStrategy tempPostLocatorStrategy =
                new TemporaryPostRegionLocatorStrategy(this.blogClient, this.blogAccount, this.credentials, this._blogHomepageUrl,
                                                       new PageDownloader(this.RequestPageDownload), new BlogPostRegionLocatorBooleanCallback(recentPostLocatorStrategy.HasBlogPosts));

            //setup the strategies for locating the title/body regions in the blog homepage.
            BlogPostRegionLocatorStrategy[] regionLocatorStrategies = new BlogPostRegionLocatorStrategy[]
                {
                    recentPostLocatorStrategy,
                    tempPostLocatorStrategy
                };

            // template files to return
            BlogEditingTemplateFile[] blogTemplateFiles = null;

            // try each strategy as necessary
            for (int i = 0; i < regionLocatorStrategies.Length && blogTemplateFiles == null; i++)
            {
                this.CheckCancelRequested(progress);

                //reset the progress for each iteration
                BlogPostRegionLocatorStrategy regionLocatorStrategy = regionLocatorStrategies[i];
                try
                {
                    blogTemplateFiles = this.GetBlogTemplateFiles(progress, regionLocatorStrategy, templateStrategies, targetTemplateTypes, this._blogHomepageUrl);
                    progress.UpdateProgress(100, 100);

                    //if any exception occurred along the way, clear them since one of the template strategies
                    //was successful.
                    this._exception = null;
                }
                catch (OperationCancelledException)
                {
                    // cancel just means our template will be String.Empty
                    this._exception = null;
                }
                catch (BlogClientOperationCancelledException e)
                {
                    // cancel just means our template will be String.Empty
                    // (setting this exception here means that at least the user
                    // will be notified that they won't be able to edit with style)
                    this._exception = e;
                }
                catch (BlogClientAbortGettingTemplateException e)
                {
                    this._exception = e;
                    //Do not proceed with the other strategies if getting the template was aborted.
                    break;
                }
                catch (WebException e)
                {
                    this._exception = e;
                    Trace.WriteLine("Error occurred while downloading weblog style: " + e.ToString());
                    if (e.Response != null)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, "Blogpost homepage request failed: {0}", this._blogHomepageUrl));
                        //Debug.WriteLine(HttpRequestHelper.DumpResponse((HttpWebResponse)e.Response));
                    }
                }
                catch (Exception e)
                {
                    this._exception = e;
                    Trace.WriteLine("Error occurred while downloading weblog style: " + e.ToString());
                }

            }

            // return the detected templates
            return blogTemplateFiles;
        }

        /// <summary>
        /// Creates a set of BlogTemplateFiles using a specific region locator strategy.
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="regionLocatorStrategy"></param>
        /// <param name="templateStrategies"></param>
        /// <param name="templateTypes"></param>
        /// <param name="targetUrl">
        /// The URL to analyze. If a post can be located, but not the body, this is used
        /// to reiterate into the post it fetch it's content directly.
        /// </param>
        /// <returns></returns>
        private BlogEditingTemplateFile[] GetBlogTemplateFiles(IProgressHost progress, BlogPostRegionLocatorStrategy regionLocatorStrategy, BlogEditingTemplateStrategy[] templateStrategies, BlogEditingTemplateType[] templateTypes, string targetUrl)
        {
            BlogEditingTemplateFile[] blogTemplateFiles = null;
            try
            {
                regionLocatorStrategy.PrepareRegions(new ProgressTick(progress, 25, 100));

                ArrayList templateFiles = new ArrayList();
                ProgressTick tick = new ProgressTick(progress, 50, 100);
                for (int i = 0; i < templateTypes.Length; i++)
                {
                    ProgressTick parseTick = new ProgressTick(tick, 1, templateTypes.Length);
                    try
                    {
                        this.CheckCancelRequested(parseTick);
                        this.templateStrategy = templateStrategies[i];

                        // Clear _nextTryPostUrl flag
                        this._nextTryPostUrl = null;

                        // Parse the blog post HTML into an editing template.
                        // Note: we can't use MarkupServices to parse the document from a non-UI thread,
                        // so we have to execute the parsing portion of the template download operation on the UI thread.
                        string editingTemplate = this.ParseWebpageIntoEditingTemplate_OnUIThread(this._parentControl, regionLocatorStrategy, new ProgressTick(parseTick, 1, 5), targetUrl);

                        // If there's no editing template, there should be a URL to try next
                        Debug.Assert(editingTemplate != null || (editingTemplate == null && this._nextTryPostUrl != null));

                        // If the homepage has just been analysed and the _nextTryPostUrl flag is set
                        if (targetUrl == this._blogHomepageUrl && this._nextTryPostUrl != null && regionLocatorStrategy.CanRefetchPage)
                        {
                            // Try fetching the URL that has been specified, and reparse
                            progress.UpdateProgress(Res.Get(StringId.ProgressDownloadingWeblogEditingStyleDeep));
                            // Fetch the post page
                            regionLocatorStrategy.FetchTemporaryPostPage(SilentProgressHost.Instance, this._nextTryPostUrl);
                            // Parse out the template
                            editingTemplate = this.ParseWebpageIntoEditingTemplate_OnUIThread(this._parentControl, regionLocatorStrategy, new ProgressTick(parseTick, 1, 5), this._nextTryPostUrl);
                        }

                        // check for cancel
                        this.CheckCancelRequested(parseTick);

                        string baseUrl = HTMLDocumentHelper.GetBaseUrl(editingTemplate, this._blogHomepageUrl);

                        // Download the template stylesheets and embedded resources (this lets the editing template load faster..and works offline!)
                        string templateFile = this.DownloadTemplateFiles(editingTemplate, baseUrl, new ProgressTick(parseTick, 4, 5));
                        templateFiles.Add(new BlogEditingTemplateFile(templateTypes[i], templateFile));

                    }
                    catch (BlogClientAbortGettingTemplateException)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Failed to download template {0}.  Aborting getting further templates", templateTypes[i].ToString()));
                        throw;
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(String.Format(CultureInfo.CurrentCulture, "Failed to download template {0}: {1}", templateTypes[i].ToString(), e.ToString()));
                    }
                }
                if (templateFiles.Count > 0)
                    blogTemplateFiles = (BlogEditingTemplateFile[])templateFiles.ToArray(typeof(BlogEditingTemplateFile));
            }
            finally
            {
                regionLocatorStrategy.CleanupRegions(new ProgressTick(progress, 25, 100));
            }
            return blogTemplateFiles;
        }

        private void CheckCancelRequested(IProgressHost progress)
        {
            if (progress.CancelRequested)
                throw new OperationCancelledException();
        }

        private void NoCacheFilter(HttpWebRequest request)
        {
            request.Headers.Add("Pragma", "no-cache");
        }

        private HttpWebResponse RequestPageDownload(string url, int timeoutMs)
        {
            return this.blogClient.SendAuthenticatedHttpRequest(url, timeoutMs, new HttpRequestFilter(this.NoCacheFilter));
        }

        private void ApplyCredentials(PageAndReferenceDownloader downloader, string url)
        {
            WinInetCredentialsContext credentialsContext = this.CreateCredentialsContext(url);
            if (credentialsContext != null)
                downloader.CredentialsContext = credentialsContext;
        }

        private void ApplyCredentials(PageDownloadContext downloadContext, string url)
        {
            WinInetCredentialsContext credentialsContext = this.CreateCredentialsContext(url);
            if (credentialsContext != null && credentialsContext.CookieString != null)
                downloadContext.CookieString = credentialsContext.CookieString.Cookies;
        }

        private WinInetCredentialsContext CreateCredentialsContext(string url)
        {
            try
            {
                return BlogClientHelper.GetCredentialsContext(this.blogClient, this.credentials, url);
            }
            catch (BlogClientOperationCancelledException)
            {
                return null;
            }
        }

        /// <summary>
        /// Parses a webpage into an editing template using a marshalled callback on the UI context's thread.
        /// </summary>
        /// <param name="uiContext"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        private string ParseWebpageIntoEditingTemplate_OnUIThread(Control uiContext, BlogPostRegionLocatorStrategy regionLocator, IProgressHost progress, string postUrl)
        {
            BlogEditingTemplate blogEditingTemplate = (BlogEditingTemplate)uiContext.Invoke(
                new TemplateParser(this.ParseBlogPostIntoTemplate),
                new object[] {
                    regionLocator,
                    new ProgressTick(progress, 1, 100),
                    postUrl });
            return blogEditingTemplate?.Template;
        }
        private delegate BlogEditingTemplate TemplateParser(BlogPostRegionLocatorStrategy regionLocator, IProgressHost progress, string postUrl);

        private BlogEditingTemplate ParseBlogPostIntoTemplate(BlogPostRegionLocatorStrategy regionLocator, IProgressHost progress, string postUrl)
        {
            progress.UpdateProgress(Res.Get(StringId.ProgressCreatingEditingTemplate));

            BlogPostRegions regions = regionLocator.LocateRegionsOnUIThread(progress, postUrl);
            IHTMLElement primaryTitleRegion = GetPrimaryEditableTitleElement(regions.BodyRegion, regions.Document, regions.TitleRegions);

            // IF
            //   - primaryTitleRegion is not null (title found)
            //   - BodyRegion is null (no post body found)
            //   - AND primaryTitleRegion is a link
            if (primaryTitleRegion != null && regions.BodyRegion == null && primaryTitleRegion.tagName.ToLower() == "a")
            {
                // Title region was detected, but body region was not.
                // It is possible that only titles are shown on the homepage
                // Try requesting the post itself, and loading regions from the post itself

                // HACK Somewhere the 'about:' protocol replaces http/https, replace it again with the correct protocol
                var pathMatch = new Regex("^about:(.*)$").Match((primaryTitleRegion as IHTMLAnchorElement).href);
                Debug.Assert(pathMatch.Success); // Assert that this URL is to the format we expect
                var newPostPath = pathMatch.Groups[1].Value; // Grab the path from the URL
                var homepageUri = new Uri(this._blogHomepageUrl);
                var newPostUrl = $"{homepageUri.Scheme}://{homepageUri.Host}{newPostPath}"; // Recreate the full post URL

                // Set the NextTryPostUrl flag in the region locater
                // This will indicate to the other thread that another page should be parsed
                this._nextTryPostUrl = newPostUrl;
                return null;
            }

            BlogEditingTemplate template = this.GenerateBlogTemplate((IHTMLDocument3)regions.Document, primaryTitleRegion, regions.TitleRegions, regions.BodyRegion);

            progress.UpdateProgress(100, 100);
            return template;
        }

        /// <summary>
        /// Disambiguates a set of title regions to determine which should be editable based on proximity to the main post body element.
        /// </summary>
        /// <param name="bodyElement"></param>
        /// <param name="doc"></param>
        /// <param name="titleElements"></param>
        /// <returns>The title region in closest proximity to the post body element.</returns>
        protected static IHTMLElement GetPrimaryEditableTitleElement(IHTMLElement bodyElement, IHTMLDocument doc, IHTMLElement[] titleElements)
        {
            IHTMLDocument2 doc2 = (IHTMLDocument2)doc;
            IHTMLElement titleElement = titleElements[0];
            if (titleElements.Length > 1)
            {
                try
                {
                    MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)doc2);
                    MarkupRange bodyRange = markupServices.CreateMarkupRange(bodyElement, true);
                    MarkupPointer titlePointer = null;
                    MarkupPointer tempPointer = markupServices.CreateMarkupPointer();
                    foreach (IHTMLElement title in titleElements)
                    {
                        tempPointer.MoveAdjacentToElement(title, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                        if (titlePointer == null)
                            titlePointer = markupServices.CreateMarkupPointer(title, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                        else
                        {
                            tempPointer.MoveAdjacentToElement(title, _ELEMENT_ADJACENCY.ELEM_ADJ_AfterBegin);
                            if (tempPointer.IsLeftOf(bodyRange.End) && tempPointer.IsRightOf(titlePointer))
                            {
                                //the temp pointer is closer to the body element, so assume it is more appropriate
                                //to use as the title.
                                titleElement = title;
                                titlePointer.MoveToPointer(tempPointer);
                            }
                        }
                    }
                }
                catch (COMException ex)
                {
                    Trace.WriteLine("Failed to differentiate between multiple nodes with title text, using the first node.  Exception: " + ex);
                }
                catch (InvalidCastException ex)
                {
                    Trace.WriteLine("Failed to differentiate between multiple nodes with title text, using the first node.  Exception: " + ex);
                }

            }
            return titleElement;
        }

        private string DownloadTemplateFiles(string templateContents, string templateUrl, IProgressHost progress)
        {
            progress.UpdateProgress(Res.Get(StringId.ProgressDownloadingSupportingFiles));
            FileBasedSiteStorage files = new FileBasedSiteStorage(this._blogTemplateDir);

            // convert the string to a stream
            MemoryStream templateStream = new MemoryStream();
            StreamWriter writer = new StreamWriter(templateStream, Encoding.UTF8);
            writer.Write(templateContents);
            writer.Flush();
            templateStream.Seek(0, SeekOrigin.Begin);

            //read the stream into a lightweight HTML.  Note that we use from LightWeightHTMLDocument.FromIHTMLDocument2
            //instead of LightWeightHTMLDocument.FromStream because from stream improperly shoves a saveFrom declaration
            //above the docType (bug 289357)
            IHTMLDocument2 doc = HTMLDocumentHelper.StreamToHTMLDoc(templateStream, templateUrl, true);
            LightWeightHTMLDocument ldoc = LightWeightHTMLDocument.FromIHTMLDocument2(doc, templateUrl, true, false);

            PageDownloadContext downloadContext = new PageDownloadContext(0);
            this.ApplyCredentials(downloadContext, templateUrl);
            using (PageToDownloadFactory downloadFactory = new PageToDownloadFactory(ldoc, downloadContext, this._parentControl))
            {
                //calculate the dependent styles and resources
                ProgressTick tick = new ProgressTick(progress, 50, 100);
                downloadFactory.CreatePagesToDownload(tick);
                tick.UpdateProgress(100, 100);

                //download the dependent styles and resources
                tick = new ProgressTick(progress, 50, 100);
                PageAndReferenceDownloader downloader = new PageAndReferenceDownloader(downloadFactory.PagesToDownload, files);
                this.ApplyCredentials(downloader, templateUrl);
                downloader.Download(tick);
                tick.UpdateProgress(100, 100);

                //Expand out the relative paths in the downloaded HTML file with absolute paths.
                //Note: this is necessary so that template resources are not improperly resolved relative
                //      to the location of the file the editor is editing.
                string blogTemplateFile = Path.Combine(this._blogTemplateDir, files.RootFile);
                string origFile = blogTemplateFile + ".token";
                File.Move(blogTemplateFile, origFile);
                string absPath = String.Format(CultureInfo.InvariantCulture, "file:///{0}/{1}", this._blogTemplateDir.Replace('\\', '/'), downloader.PathToken);
                TextHelper.ReplaceInFile(origFile, downloader.PathToken, blogTemplateFile, absPath);
                File.Delete(origFile);

                //fix up the files
                this.FixupDownloadedFiles(blogTemplateFile, files, downloader.PathToken);

                //complete the progress.
                progress.UpdateProgress(100, 100);

                File.WriteAllText(blogTemplateFile + ".path", absPath);
                return blogTemplateFile;
            }
        }

        /// <summary>
        /// Fixes up the downloaded supporting files that were in the template.
        /// </summary>
        /// <param name="blogTemplateFile"></param>
        /// <param name="storage"></param>
        /// <param name="supportingFilesDir"></param>
        protected internal virtual void FixupDownloadedFiles(string blogTemplateFile, FileBasedSiteStorage storage, string supportingFilesDir)
        {
            this.templateStrategy.FixupDownloadedFiles(blogTemplateFile, storage, supportingFilesDir);
        }

        private BlogEditingTemplate GenerateBlogTemplate(IHTMLDocument3 doc, IHTMLElement titleElement, IHTMLElement[] allTitleElements, IHTMLElement bodyElement)
        {
            return this.templateStrategy.GenerateBlogTemplate(doc, titleElement, allTitleElements, bodyElement);
        }

        BlogEditingTemplateStrategy templateStrategy = BlogEditingTemplateStrategies.GetTemplateStrategy(BlogEditingTemplateStrategies.StrategyType.FramedWysiwyg);

        // execution context
        private Control _parentControl;
        private bool contextSet = false;
        private BlogAccount blogAccount;
        private IBlogClient blogClient;
        private string _blogHomepageUrl;
        private string _blogTemplateDir;
        private WriterEditingManifestDownloadInfo _manifestDownloadInfo;
        private bool _probeForManifest = false;
        private IBlogCredentialsAccessor credentials;

        // return value
        private BlogEditingTemplateFile[] _blogTemplateFiles = new BlogEditingTemplateFile[0];
        private Color? _postBodyBackgroundColor;
    }

    public delegate HttpWebResponse PageDownloader(string url, int timeoutMs);
}
