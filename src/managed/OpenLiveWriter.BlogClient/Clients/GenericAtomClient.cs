// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

/*
 * TODO
 *
 * Make sure all required fields are filled out.
 * Remove the HTML title from the friendly error message
 * Test ETags where HEAD not supported
 * Test experience when no media collection configured
 * Add command line option for preferring Atom over RSD
 * See if blogproviders.xml can override Atom vs. RSD preference
 */

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    using OpenLiveWriter.BlogClient.Detection;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    [BlogClient("Atom", "Atom")]
    public class GenericAtomClient : AtomClient, ISelfConfiguringClient, IBlogClientForCategorySchemeHack
    {
        static GenericAtomClient() => AuthenticationManager.Register(new GoogleLoginAuthenticationModule());

        private string defaultCategoryScheme_HACK;

        public GenericAtomClient(Uri postApiUrl, IBlogCredentialsAccessor credentials) : base(AtomProtocolVersion.V10, postApiUrl, credentials)
        {
        }

        protected override void ConfigureClientOptions(OpenLiveWriter.BlogClient.Providers.BlogClientOptions clientOptions)
        {
            base.ConfigureClientOptions(clientOptions);

            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsExcerpt = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsFileUpload = true;
        }

        public virtual void DetectSettings(IBlogSettingsDetectionContext context, BlogSettingsDetector detector)
        {
            if (detector.IncludeOptionOverrides)
            {
                if (detector.IncludeCategoryScheme)
                {
                    Debug.Assert(!detector.UseManifestCache,
                                 "This code will not run correctly under the manifest cache, due to option overrides not being set");

                    var optionOverrides = context.OptionOverrides;
                    if (optionOverrides == null)
                    {
                        optionOverrides = new Dictionary<string, string>();
                    }

                    var hasNewCategories = optionOverrides.Keys.Contains(BlogClientOptions.SUPPORTS_NEW_CATEGORIES);
                    var hasScheme = optionOverrides.Keys.Contains(BlogClientOptions.CATEGORY_SCHEME);
                    if (!hasNewCategories || !hasScheme)
                    {
                        this.GetCategoryInfo(
                            context.HostBlogId,
                            optionOverrides[BlogClientOptions.CATEGORY_SCHEME] as string, // may be null
                            out var scheme,
                            out var supportsNewCategories);

                        if (scheme == null)
                        {
                            // no supported scheme was found or provided
                            optionOverrides[BlogClientOptions.SUPPORTS_CATEGORIES] = false.ToString();
                        }
                        else
                        {
                            if (!optionOverrides.Keys.Contains(BlogClientOptions.SUPPORTS_NEW_CATEGORIES))
                            {
                                optionOverrides.Add(BlogClientOptions.SUPPORTS_NEW_CATEGORIES, supportsNewCategories.ToString());
                            }

                            if (!optionOverrides.Keys.Contains(BlogClientOptions.CATEGORY_SCHEME))
                            {
                                optionOverrides.Add(BlogClientOptions.CATEGORY_SCHEME, scheme);
                            }
                        }

                        context.OptionOverrides = optionOverrides;
                    }
                }

                // GetFeaturesXml(context.HostBlogId);
            }
        }

        private void GetFeaturesXml(string blogId)
        {
            var uri = this.FeedServiceUrl;
            var serviceDoc = xmlRestRequestHelper.Get(ref uri, this.RequestFilter);

            foreach (XmlElement entryEl in serviceDoc.SelectNodes("app:service/app:workspace/app:collection", this.NamespaceManager))
            {
                var href = XmlHelper.GetUrl(entryEl, "@href", uri);
                if (blogId == href)
                {
                    var results = new XmlDocument();
                    var rootElement = results.CreateElement("featuresInfo");
                    results.AppendChild(rootElement);
                    foreach (XmlElement featuresNode in entryEl.SelectNodes("f:features", this.NamespaceManager))
                    {
                        this.AddFeaturesXml(featuresNode, rootElement, uri);
                    }
                    return;
                }
            }
            Trace.Fail("Couldn't find collection in service document:\r\n" + serviceDoc.OuterXml);
        }

        private void AddFeaturesXml(XmlElement featuresNode, XmlElement containerNode, Uri baseUri)
        {
            if (featuresNode.HasAttribute("href"))
            {
                var href = XmlHelper.GetUrl(featuresNode, "@href", baseUri);
                if (href != null && href.Length > 0)
                {
                    var uri = new Uri(href);
                    if (baseUri == null || !uri.Equals(baseUri)) // detect simple cycles
                    {
                        var doc = xmlRestRequestHelper.Get(ref uri, this.RequestFilter);
                        var features = (XmlElement)doc.SelectSingleNode(@"f:features", this.NamespaceManager);
                        if (features != null)
                        {
                            this.AddFeaturesXml(features, containerNode, uri);
                        }
                    }
                }
            }
            else
            {
                foreach (XmlElement featureEl in featuresNode.SelectNodes("f:feature"))
                {
                    containerNode.AppendChild(containerNode.OwnerDocument.ImportNode(featureEl, true));
                }
            }

        }

        string IBlogClientForCategorySchemeHack.DefaultCategoryScheme
        {
            set { this.defaultCategoryScheme_HACK = value; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="inScheme">The scheme that should definitely be used (i.e. from wlwmanifest), or null.
        /// If inScheme is non-null, then outScheme will equal inScheme.</param>
        /// <param name="outScheme">The scheme that should be used, or null if categories are not supported.</param>
        /// <param name="supportsNewCategories">Ignore this value if outScheme == null.</param>
        private void GetCategoryInfo(string blogId, string inScheme, out string outScheme, out bool supportsNewCategories)
        {
            var xmlDoc = this.GetCategoryXml(ref blogId);
            foreach (XmlElement categoriesNode in xmlDoc.DocumentElement.SelectNodes("app:categories", this.NamespaceManager))
            {
                var hasScheme = categoriesNode.HasAttribute("scheme");
                var scheme = categoriesNode.GetAttribute("scheme");
                var isFixed = categoriesNode.GetAttribute("fixed") == "yes";

                // <categories fixed="no" />
                if (!hasScheme && inScheme == null && !isFixed)
                {
                    outScheme = "";
                    supportsNewCategories = true;
                    return;
                }

                // <categories scheme="inScheme" fixed="yes|no" />
                if (hasScheme && scheme == inScheme)
                {
                    outScheme = inScheme;
                    supportsNewCategories = !isFixed;
                    return;
                }

                // <categories scheme="" fixed="yes|no" />
                if (hasScheme && inScheme == null && scheme == "")
                {
                    outScheme = "";
                    supportsNewCategories = !isFixed;
                    return;
                }
            }

            outScheme = inScheme; // will be null if no scheme was externally specified
            supportsNewCategories = false;
        }

        /*
                protected override OpenLiveWriter.CoreServices.HttpRequestFilter RequestFilter
                {
                    get
                    {
                        return new HttpRequestFilter(WordPressCookieFilter);
                    }
                }

                private void WordPressCookieFilter(HttpWebRequest request)
                {
                    request.CookieContainer = new CookieContainer();
                    string COOKIE_STRING =
                        "wordpressuser_6c27d03220bea936360daa76ec007cd7=admin; wordpresspass_6c27d03220bea936360daa76ec007cd7=696d29e0940a4957748fe3fc9efd22a3; __utma=260458847.291972184.1164155988.1176250147.1176308376.43; __utmz=260458847.1164155988.1.1.utmccn=(direct)|utmcsr=(direct)|utmcmd=(none); dbx-postmeta=grabit:0+|1+|2+|3+|4+|5+&advancedstuff:0-|1-|2-; dbx-pagemeta=grabit=0+,1+,2+,3+,4+,5+&advancedstuff=0+";
                    foreach (string cookie in StringHelper.Split(COOKIE_STRING, ";"))
                    {
                        string[] pair = cookie.Split('=');
                        request.CookieContainer.Add(new Cookie(pair[0], pair[1], "/wp22test/", "www.unknown.com"));
                    }
                }
        */

        protected override string CategoryScheme
        {
            get
            {
                var scheme = this.Options.CategoryScheme;
                if (scheme == null)
                {
                    scheme = this.defaultCategoryScheme_HACK;
                }

                return scheme;
            }
        }

        protected override void VerifyCredentials(TransientCredentials tc)
        {
            // This sucks. We really want to authenticate against the actual feed,
            // not just the service document.
            var uri = this.FeedServiceUrl;
            xmlRestRequestHelper.Get(ref uri, this.RequestFilter);
        }

        protected void EnsureLoggedIn()
        {

        }

        #region image upload support

        public override void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            var uploader = new AtomMediaUploader(this.NamespaceManager, this.RequestFilter, this.Options.ImagePostingUrl, this.Options);
            return uploader.DoBeforePublishUploadWork(uploadContext);
        }

        public override BlogInfo[] GetImageEndpoints()
        {
            this.EnsureLoggedIn();

            var serviceDocUri = this.FeedServiceUrl;
            var xmlDoc = xmlRestRequestHelper.Get(ref serviceDocUri, this.RequestFilter);

            var blogInfos = new ArrayList();
            foreach (XmlElement coll in xmlDoc.SelectNodes("/app:service/app:workspace/app:collection", this.NamespaceManager))
            {
                // does this collection accept entries?
                var acceptNodes = coll.SelectNodes("app:accept", this.NamespaceManager);
                var acceptTypes = new string[acceptNodes.Count];
                for (var i = 0; i < acceptTypes.Length; i++)
                {
                    acceptTypes[i] = acceptNodes[i].InnerText;
                }

                if (AcceptsImages(acceptTypes))
                {
                    var feedUrl = XmlHelper.GetUrl(coll, "@href", serviceDocUri);
                    if (feedUrl == null || feedUrl.Length == 0)
                    {
                        continue;
                    }

                    // form title
                    var titleBuilder = new StringBuilder();
                    foreach (var titleContainerNode in new XmlElement[] { coll.ParentNode as XmlElement, coll })
                    {
                        Debug.Assert(titleContainerNode != null);
                        if (titleContainerNode != null)
                        {
                            var titleNode = titleContainerNode.SelectSingleNode("atom:title", this.NamespaceManager) as XmlElement;
                            if (titleNode != null)
                            {
                                var titlePart = this.atomVersion.TextNodeToPlaintext(titleNode);
                                if (titlePart.Length != 0)
                                {
                                    Res.LOCME("loc the separator between parts of the blog name");
                                    if (titleBuilder.Length != 0)
                                    {
                                        titleBuilder.Append(" - ");
                                    }

                                    titleBuilder.Append(titlePart);
                                }
                            }
                        }
                    }

                    blogInfos.Add(new BlogInfo(feedUrl, titleBuilder.ToString().Trim(), ""));
                }
            }

            return (BlogInfo[])blogInfos.ToArray(typeof(BlogInfo));
        }

        private static bool AcceptsImages(string[] contentTypes)
        {
            bool acceptsPng = false, acceptsJpeg = false, acceptsGif = false;

            foreach (var contentType in contentTypes)
            {
                var values = MimeHelper.ParseContentType(contentType, true);
                var mainType = values[""] as string;

                switch (mainType)
                {
                    case "*/*":
                    case "image/*":
                        return true;
                    case "image/png":
                        acceptsPng = true;
                        break;
                    case "image/gif":
                        acceptsGif = true;
                        break;
                    case "image/jpeg":
                        acceptsJpeg = true;
                        break;
                }
            }

            return acceptsPng && acceptsJpeg && acceptsGif;
        }

        #endregion
    }

    public class GoogleLoginAuthenticationModule : IAuthenticationModule
    {
        private static GDataCredentials _gdataCred = new GDataCredentials();

        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            if (!challenge.StartsWith("GoogleLogin ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var httpRequest = (HttpWebRequest)request;

            string service;
            string realm;
            this.ParseChallenge(challenge, out realm, out service);
            if (realm != "http://www.google.com/accounts/ClientLogin")
            {
                return null;
            }

            var cred = credentials.GetCredential(request.RequestUri, this.AuthenticationType);

            var auth = _gdataCred.GetCredentialsIfValid(cred.UserName, cred.Password, service);
            if (auth != null)
            {
                return new Authorization(auth, true);
            }
            else
            {
                try
                {
                    _gdataCred.EnsureLoggedIn(cred.UserName, cred.Password, service, !BlogClientUIContext.SilentModeForCurrentThread);
                    auth = _gdataCred.GetCredentialsIfValid(cred.UserName, cred.Password, service);
                    if (auth != null)
                    {
                        return new Authorization(auth, true);
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    return null;
                }
            }
        }

        private void ParseChallenge(string challenge, out string realm, out string service)
        {
            var m = Regex.Match(challenge, @"\brealm=""([^""]*)""");
            realm = m.Groups[1].Value;
            var m2 = Regex.Match(challenge, @"\bservice=""([^""]*)""");
            service = m2.Groups[1].Value;
        }

        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials)
        {
            throw new NotImplementedException();
        }

        public bool CanPreAuthenticate
        {
            get { return false; }
        }

        public string AuthenticationType
        {
            get { return "GoogleLogin"; }
        }
    }
}
