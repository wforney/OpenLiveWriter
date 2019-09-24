// <copyright file="GenericAtomClient.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.BlogClient.Clients
{
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml;

    using OpenLiveWriter.BlogClient.Detection;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The GenericAtomClient class.
    /// Implements the <see cref="AtomClient" />
    /// Implements the <see cref="ISelfConfiguringClient" />
    /// Implements the <see cref="IBlogClientForCategorySchemeHack" />
    /// </summary>
    /// <seealso cref="AtomClient" />
    /// <seealso cref="ISelfConfiguringClient" />
    /// <seealso cref="IBlogClientForCategorySchemeHack" />
    [BlogClient("Atom", "Atom")]
    public class GenericAtomClient : AtomClient, ISelfConfiguringClient, IBlogClientForCategorySchemeHack
    {
        /// <summary>
        /// The default category scheme hack
        /// </summary>
        private string defaultCategoryScheme_HACK;

        /// <summary>
        /// Initializes static members of the <see cref="GenericAtomClient"/> class.
        /// </summary>
        static GenericAtomClient()
        {
            AuthenticationManager.Register(new GoogleLoginAuthenticationModule());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericAtomClient"/> class.
        /// </summary>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        public GenericAtomClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(AtomProtocolVersion.V10, postApiUrl, credentials)
        {
        }

        /// <summary>
        /// Sets the default category scheme.
        /// </summary>
        /// <value>The default category scheme.</value>
        string IBlogClientForCategorySchemeHack.DefaultCategoryScheme
        {
            set => this.defaultCategoryScheme_HACK = value;
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

        /// <summary>
        /// Gets the category scheme.
        /// </summary>
        /// <value>The category scheme.</value>
        protected override string CategoryScheme => this.Options.CategoryScheme ?? this.defaultCategoryScheme_HACK;

        /// <summary>
        /// Detects the settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="detector">The detector.</param>
        public virtual void DetectSettings(IBlogSettingsDetectionContext context, BlogSettingsDetector detector)
        {
            if (!detector.IncludeOptionOverrides || !detector.IncludeCategoryScheme)
            {
                return;
            }

            Debug.Assert(
                !detector.UseManifestCache,
                "This code will not run correctly under the manifest cache, due to option overrides not being set");

            var optionOverrides = context.OptionOverrides ?? new Dictionary<string, string>();

            var hasNewCategories = optionOverrides.Keys.Contains(BlogClientOptions.SUPPORTS_NEW_CATEGORIES);
            var hasScheme = optionOverrides.Keys.Contains(BlogClientOptions.CATEGORY_SCHEME);
            if (hasNewCategories && hasScheme)
            {
                return;
            }

            this.GetCategoryInfo(
                context.HostBlogId,
                optionOverrides[BlogClientOptions.CATEGORY_SCHEME], // may be null
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

            // GetFeaturesXml(context.HostBlogId);
        }

        /// <summary>
        /// Does the after publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        public override void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result.</returns>
        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            var uploader = new AtomMediaUploader(
                this.NamespaceManager,
                this.RequestFilter,
                this.Options.ImagePostingUrl,
                this.Options);
            return uploader.DoBeforePublishUploadWork(uploadContext);
        }

        /// <summary>
        /// Gets the image endpoints.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        public override BlogInfo[] GetImageEndpoints()
        {
            this.EnsureLoggedIn();

            var serviceDocUri = this.FeedServiceUrl;
            var xmlDoc = AtomClient.xmlRestRequestHelper.Get(ref serviceDocUri, this.RequestFilter);

            var blogInfos = new List<BlogInfo>();
            foreach (var coll in xmlDoc.SelectNodes("/app:service/app:workspace/app:collection", this.NamespaceManager)
                                      ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                // does this collection accept entries?
                var acceptNodes = coll.SelectNodes("app:accept", this.NamespaceManager);
                if (acceptNodes == null)
                {
                    continue;
                }

                var acceptTypes = new string[acceptNodes.Count];
                for (var i = 0; i < acceptTypes.Length; i++)
                {
                    acceptTypes[i] = acceptNodes[i].InnerText;
                }

                if (!GenericAtomClient.AcceptsImages(acceptTypes))
                {
                    continue;
                }

                var feedUrl = XmlHelper.GetUrl(coll, "@href", serviceDocUri);
                if (string.IsNullOrEmpty(feedUrl))
                {
                    continue;
                }

                // form title
                var titleBuilder = new StringBuilder();
                foreach (var titleContainerNode in new[] { coll.ParentNode as XmlElement, coll })
                {
                    Debug.Assert(titleContainerNode != null, "title container node is not null");
                    if (!(titleContainerNode?.SelectSingleNode("atom:title", this.NamespaceManager) is XmlElement
                              titleNode))
                    {
                        continue;
                    }

                    var titlePart = this.atomVersion.TextNodeToPlaintext(titleNode);
                    if (titlePart.Length == 0)
                    {
                        continue;
                    }

                    Res.LOCME("loc the separator between parts of the blog name");
                    if (titleBuilder.Length != 0)
                    {
                        titleBuilder.Append(" - ");
                    }

                    titleBuilder.Append(titlePart);
                }

                blogInfos.Add(new BlogInfo(feedUrl, titleBuilder.ToString().Trim(), string.Empty));
            }

            return blogInfos.ToArray();
        }

        /// <summary>
        /// Configures the client options.
        /// </summary>
        /// <param name="clientOptions">The client options.</param>
        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
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

        /// <summary>
        /// Ensures the logged in.
        /// </summary>
        protected void EnsureLoggedIn()
        {
        }

        /// <inheritdoc />
        protected override void VerifyCredentials(TransientCredentials tc)
        {
            // This sucks. We really want to authenticate against the actual feed,
            // not just the service document.
            var uri = this.FeedServiceUrl;
            AtomClient.xmlRestRequestHelper.Get(ref uri, this.RequestFilter);
        }

        /// <summary>
        /// Accepts the images.
        /// </summary>
        /// <param name="contentTypes">The content types.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        private static bool AcceptsImages(IEnumerable<string> contentTypes)
        {
            bool acceptsPng = false, acceptsJpeg = false, acceptsGif = false;

            foreach (var contentType in contentTypes)
            {
                var values = MimeHelper.ParseContentType(contentType, true);
                var mainType = values[string.Empty] as string;

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

        /// <summary>
        /// Adds the features XML.
        /// </summary>
        /// <param name="featuresNode">The features node.</param>
        /// <param name="containerNode">The container node.</param>
        /// <param name="baseUri">The base URI.</param>
        private void AddFeaturesXml(XmlElement featuresNode, XmlElement containerNode, Uri baseUri)
        {
            while (true)
            {
                if (featuresNode.HasAttribute("href"))
                {
                    var href = XmlHelper.GetUrl(featuresNode, "@href", baseUri);
                    if (string.IsNullOrEmpty(href))
                    {
                        return;
                    }

                    var uri = new Uri(href);
                    if (baseUri != null && uri.Equals(baseUri))
                    {
                        return;
                    }

                    // detect simple cycles
                    var doc = AtomClient.xmlRestRequestHelper.Get(ref uri, this.RequestFilter);
                    var features = (XmlElement)doc.SelectSingleNode(@"f:features", this.NamespaceManager);
                    if (features != null)
                    {
                        featuresNode = features;
                        baseUri = uri;
                        continue;
                    }
                }
                else
                {
                    foreach (var featureEl in featuresNode.SelectNodes("f:feature")?.Cast<XmlElement>().ToList()
                                           ?? new List<XmlElement>())
                    {
                        if (containerNode.OwnerDocument != null)
                        {
                            containerNode.AppendChild(containerNode.OwnerDocument.ImportNode(featureEl, true));
                        }
                    }
                }

                break;
            }
        }

        /// <summary>
        /// Gets the category information.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// ReSharper disable once CommentTypo
        /// <param name="inScheme">The scheme that should definitely be used (i.e. from wlwmanifest), or null.
        /// If inScheme is non-null, then outScheme will equal inScheme.</param>
        /// <param name="outScheme">The scheme that should be used, or null if categories are not supported.</param>
        /// <param name="supportsNewCategories">Ignore this value if outScheme == null.</param>
        /// ReSharper disable once StyleCop.SA1650
        private void GetCategoryInfo(
            string blogId,
            string inScheme,
            out string outScheme,
            out bool supportsNewCategories)
        {
            var xmlDoc = this.GetCategoryXml(ref blogId);
            foreach (var categoriesNode in xmlDoc.DocumentElement?.SelectNodes("app:categories", this.NamespaceManager)
                                                ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                var hasScheme = categoriesNode.HasAttribute("scheme");
                var scheme = categoriesNode.GetAttribute("scheme");
                var isFixed = categoriesNode.GetAttribute("fixed") == "yes";

                // <categories fixed="no" />
                if (!hasScheme && inScheme == null && !isFixed)
                {
                    outScheme = string.Empty;
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
                if (hasScheme && inScheme == null && scheme == string.Empty)
                {
                    outScheme = string.Empty;
                    supportsNewCategories = !isFixed;
                    return;
                }
            }

            outScheme = inScheme; // will be null if no scheme was externally specified
            supportsNewCategories = false;
        }

        /// <summary>
        /// Gets the features XML.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        private void GetFeaturesXml(string blogId)
        {
            var uri = this.FeedServiceUrl;
            var serviceDoc = AtomClient.xmlRestRequestHelper.Get(ref uri, this.RequestFilter);

            foreach (var entryEl in serviceDoc.SelectNodes(
                                        "app:service/app:workspace/app:collection",
                                        this.NamespaceManager)?.Cast<XmlElement>().ToList()
                                 ?? new List<XmlElement>())
            {
                var href = XmlHelper.GetUrl(entryEl, "@href", uri);
                if (blogId != href)
                {
                    continue;
                }

                var results = new XmlDocument();
                var rootElement = results.CreateElement("featuresInfo");
                results.AppendChild(rootElement);
                foreach (var featuresNode in entryEl.SelectNodes("f:features", this.NamespaceManager)
                                                   ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
                {
                    this.AddFeaturesXml(featuresNode, rootElement, uri);
                }

                return;
            }

            Trace.Fail($"Couldn't find collection in service document:\r\n{serviceDoc.OuterXml}");
        }
    }
}
