// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Detection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;

    using OpenLiveWriter.BlogClient.Clients;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Progress;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.HtmlParser.Parser;
    using OpenLiveWriter.Localization;

    public interface ISelfConfiguringClient : IBlogClient
    {
        void DetectSettings(IBlogSettingsDetectionContext context, BlogSettingsDetector detector);
    }

    public interface IBlogSettingsDetectionContext
    {
        string HomepageUrl { get; }
        string HostBlogId { get; }
        string PostApiUrl { get; }
        IBlogCredentialsAccessor Credentials { get; }
        string ProviderId { get; }
        IDictionary<string, string> UserOptionOverrides { get; }

        WriterEditingManifestDownloadInfo ManifestDownloadInfo { get; set; }
        string ClientType { get; set; }
        byte[] FavIcon { get; set; }
        byte[] Image { get; set; }
        byte[] WatermarkImage { get; set; }
        BlogPostCategory[] Categories { get; set; }
        BlogPostKeyword[] Keywords { get; set; }
        IDictionary<string, string> OptionOverrides { get; set; }
        IDictionary<string, string> HomePageOverrides { get; set; }
        IBlogProviderButtonDescription[] ButtonDescriptions { get; set; }
    }

    public interface ITemporaryBlogSettingsDetectionContext : IBlogSettingsDetectionContext
    {
        BlogInfo[] AvailableImageEndpoints { get; set; }
    }

    public interface IBlogSettingsDetectionContextForCategorySchemeHack : IBlogSettingsDetectionContext
    {
        string InitialCategoryScheme { get; }
    }

    internal interface IBlogClientForCategorySchemeHack : IBlogClient
    {
        string DefaultCategoryScheme { set; }
    }

    public class BlogSettingsDetector
    {
        /// <summary>
        /// Create a blog client based on all of the context we currently have available
        /// </summary>
        /// <returns></returns>
        private IBlogClient CreateBlogClient() =>
            BlogClientManager.CreateClient(
                this.context.ClientType,
                this.context.PostApiUrl,
                this.context.Credentials,
                this.context.ProviderId,
                this.context.OptionOverrides,
                this.context.UserOptionOverrides,
                this.context.HomePageOverrides);

        private HttpWebResponse ExecuteHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter) => this.CreateBlogClient().SendAuthenticatedHttpRequest(requestUri, timeoutMs, filter);

        public BlogSettingsDetector(IBlogSettingsDetectionContext context)
        {
            // save the context
            this.context = context;

            this.homepageAccessor = new LazyHomepageDownloader(this.context.HomepageUrl, new HttpRequestHandler(this.ExecuteHttpRequest));
        }

        public bool SilentMode { get; set; } = false;

        public bool IncludeFavIcon
        {
            get => this._includeFavIcon && this.context.Image == null;
            set => this._includeFavIcon = value;
        }
        private bool _includeFavIcon = true;

        public bool IncludeImageEndpoints { get; set; } = true;

        public bool IncludeCategories { get; set; } = true;

        public bool IncludeCategoryScheme { get; set; } = true;

        public bool IncludeOptionOverrides { get; set; } = true;

        public bool IncludeInsecureOperations { get; set; } = true;

        public bool IncludeHomePageSettings { get; set; } = true;

        public bool IncludeImages { get; set; } = true;

        public bool IncludeButtons { get; set; } = true;

        public bool UseManifestCache { get; set; } = false;

        private readonly LazyHomepageDownloader homepageAccessor;

        public object DetectSettings(IProgressHost progressHost)
        {
            var canRemoteDetect = this.CreateBlogClient().RemoteDetectionPossible;

            using (this.SilentMode ? new BlogClientUIContextSilentMode() : null)
            {
                if ((this.IncludeButtons || this.IncludeOptionOverrides || this.IncludeImages) && canRemoteDetect)
                {
                    using (new ProgressContext(progressHost, 40, Res.Get(StringId.ProgressDetectingWeblogSettings)))
                    {
                        // attempt to download editing manifest
                        var editingManifest = this.SafeDownloadEditingManifest();

                        if (editingManifest != null)
                        {
                            // always update the download info
                            if (editingManifest.DownloadInfo != null)
                            {
                                this.context.ManifestDownloadInfo = editingManifest.DownloadInfo;
                            }

                            // images
                            if (this.IncludeImages)
                            {
                                // image if provided
                                if (editingManifest.Image != null)
                                {
                                    this.context.Image = editingManifest.Image;
                                }

                                // watermark if provided
                                if (editingManifest.Watermark != null)
                                {
                                    this.context.WatermarkImage = editingManifest.Watermark;
                                }
                            }

                            // buttons if provided
                            if (this.IncludeButtons && (editingManifest.ButtonDescriptions != null))
                            {
                                this.context.ButtonDescriptions = editingManifest.ButtonDescriptions;
                            }

                            // option overrides if provided
                            if (this.IncludeOptionOverrides)
                            {
                                if (editingManifest.ClientType != null)
                                {
                                    this.context.ClientType = editingManifest.ClientType;
                                }

                                if (editingManifest.OptionOverrides != null)
                                {
                                    this.context.OptionOverrides = editingManifest.OptionOverrides;
                                }
                            }
                        }
                    }
                }

                using (new ProgressContext(progressHost, 40, Res.Get(StringId.ProgressDetectingWeblogCharSet)))
                {
                    if (this.IncludeOptionOverrides && this.IncludeHomePageSettings && canRemoteDetect)
                    {
                        this.DetectHomePageSettings();
                    }
                }

                var blogClient = this.CreateBlogClient();
                if (this.IncludeInsecureOperations || blogClient.IsSecure)
                {
                    if (blogClient is ISelfConfiguringClient)
                    {
                        // This must happen before categories detection but after manifest!!
                        ((ISelfConfiguringClient)blogClient).DetectSettings(this.context, this);
                    }

                    // detect categories
                    if (this.IncludeCategories)
                    {
                        using (
                            new ProgressContext(progressHost, 20, Res.Get(StringId.ProgressDetectingWeblogCategories)))
                        {
                            var categories = this.SafeDownloadCategories();
                            if (categories != null)
                            {
                                this.context.Categories = categories;
                            }

                            var keywords = this.SafeDownloadKeywords();
                            if (keywords != null)
                            {
                                this.context.Keywords = keywords;
                            }
                        }
                    }

                    // detect favicon (only if requested AND we don't have a PNG already
                    // for the small image size)
                    if (this.IncludeFavIcon && canRemoteDetect)
                    {
                        using (new ProgressContext(progressHost, 10, Res.Get(StringId.ProgressDetectingWeblogIcon)))
                        {
                            var favIcon = this.SafeDownloadFavIcon();
                            if (favIcon != null)
                            {
                                this.context.FavIcon = favIcon;
                            }
                        }
                    }

                    if (this.IncludeImageEndpoints)
                    {
                        Debug.WriteLine("Detecting image endpoints");
                        var tempContext =
                            this.context as ITemporaryBlogSettingsDetectionContext;
                        Debug.Assert(tempContext != null,
                                     "IncludeImageEndpoints=true but non-temporary context (type " +
                                     this.context.GetType().Name + ") was used");
                        if (tempContext != null)
                        {
                            tempContext.AvailableImageEndpoints = null;
                            try
                            {
                                var imageEndpoints = blogClient.GetImageEndpoints();
                                tempContext.AvailableImageEndpoints = imageEndpoints;
                                Debug.WriteLine(imageEndpoints.Length + " image endpoints detected");
                            }
                            catch (NotImplementedException)
                            {
                                Debug.WriteLine("Image endpoints not implemented");
                            }
                            catch (Exception e)
                            {
                                Trace.Fail("Exception detecting image endpoints: " + e.ToString());
                            }
                        }
                    }
                }
                // completed
                progressHost.UpdateProgress(100, 100, Res.Get(StringId.ProgressCompletedSettingsDetection));
            }

            return this;
        }

        /// <summary>
        /// Any setting that is derived from the homepage html needs to be in this function.  This function is turned
        /// on and off when detecting blog settings through the IncludeHomePageSettings.  None of these checks will be run
        /// if the internet is not active.  As each check is made, it does not need to be applied back the _content until the end
        /// at which time it will write the settings back to the registry.
        /// </summary>
        private void DetectHomePageSettings()
        {
            if (this.homepageAccessor.HtmlDocument == null)
            {
                return;
            }

            var homepageSettings = new Dictionary<string, string>();

            Debug.Assert(!this.UseManifestCache, "This code will not run correctly under the manifest cache, due to option overrides not being set");

            var metaData = new LightWeightHTMLMetaData(this.homepageAccessor.HtmlDocument);
            if (metaData.Charset != null)
            {
                try
                {
                    homepageSettings.Add(BlogClientOptions.CHARACTER_SET, metaData.Charset);
                }
                catch (NotSupportedException)
                {
                    //not an actual encoding
                }

            }

            var docType = new LightWeightHTMLMetaData(this.homepageAccessor.HtmlDocument).DocType;
            if (docType != null)
            {
                var xhtml = docType.IndexOf("xhtml", StringComparison.OrdinalIgnoreCase) >= 0;
                if (xhtml)
                {
                    homepageSettings.Add(BlogClientOptions.REQUIRES_XHTML, true.ToString(CultureInfo.InvariantCulture));
                }
            }

            //checking whether blog is rtl
            var extractor = new HtmlExtractor(this.homepageAccessor.HtmlDocument.RawHtml);
            if (extractor.Seek(new OrPredicate(
                new SmartPredicate("<html dir>"),
                new SmartPredicate("<body dir>"))).Success)
            {
                var tag = (BeginTag)extractor.Element;
                var dir = tag.GetAttributeValue("dir");
                if (string.Compare(dir, "rtl", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    homepageSettings.Add(BlogClientOptions.TEMPLATE_IS_RTL, true.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (this.homepageAccessor.HtmlDocument != null)
            {
                var html = this.homepageAccessor.OriginalHtml;
                var viewer = DhtmlImageViewers.DetectImageViewer(html, this.context.HomepageUrl);
                if (viewer != null)
                {
                    homepageSettings.Add(BlogClientOptions.DHTML_IMAGE_VIEWER, viewer.Name);
                }
            }

            this.context.HomePageOverrides = homepageSettings;
        }

        private byte[] SafeDownloadFavIcon() => this.SafeProbeForFavIconFromFile() ?? this.SafeProbeForFavIconFromLinkTag();

        private byte[] SafeProbeForFavIconFromFile()
        {
            try
            {
                var favIconUrl = UrlHelper.UrlCombine(this.context.HomepageUrl, "favicon.ico");
                using (var favIconStream = this.SafeDownloadFavIcon(favIconUrl))
                {
                    return favIconStream == null ? null : this.FavIconArrayFromStream(favIconStream);
                }
            }
            catch (Exception ex)
            {
                this.ReportException("attempting to download favicon", ex);
                return null;
            }
        }

        private byte[] SafeProbeForFavIconFromLinkTag()
        {
            try
            {
                if (this.homepageAccessor.HtmlDocument != null)
                {
                    var linkTags = this.homepageAccessor.HtmlDocument.GetTagsByName(HTMLTokens.Link);
                    foreach (var linkTag in linkTags)
                    {
                        var rel = linkTag.BeginTag.GetAttributeValue("rel");
                        var href = linkTag.BeginTag.GetAttributeValue("href");
                        if (rel != null && rel.Trim().ToUpperInvariant() == "SHORTCUT ICON" && href != null)
                        {
                            // now we have the favicon url, try to download it
                            var favIconUrl = UrlHelper.UrlCombineIfRelative(this.context.HomepageUrl, href);
                            using (var favIconStream = this.SafeDownloadFavIcon(favIconUrl))
                            {
                                if (favIconStream != null)
                                {
                                    return this.FavIconArrayFromStream(favIconStream);
                                }
                            }
                        }
                    }
                }

                // didn't find the favicon this way
                return null;
            }
            catch (Exception ex)
            {
                this.ReportException("attempting to download favicon from link tag", ex);
                return null;
            }
        }

        private byte[] FavIconArrayFromStream(Stream favIconStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                // copy it to a memory stream
                StreamHelper.Transfer(favIconStream, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                // validate that it is indeed an icon
                try
                {
                    var icon = new Icon(memoryStream);
                    (icon as IDisposable).Dispose();

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.ToArray();
                }
                catch
                {
                    return null;
                }
            }
        }

        private Stream SafeDownloadFavIcon(string favIconUrl)
        {
            try
            {
                var favIconPath = UrlDownloadToFile.Download(favIconUrl, 3000);
                return new FileStream(favIconPath, FileMode.Open);
            }
            catch
            {
                return null;
            }
        }

        private BlogPostCategory[] SafeDownloadCategories()
        {
            try
            {
                var blogClient = this.CreateBlogClient();

                if (blogClient is IBlogClientForCategorySchemeHack && this.context is IBlogSettingsDetectionContextForCategorySchemeHack)
                {
                    ((IBlogClientForCategorySchemeHack)blogClient).DefaultCategoryScheme =
                        ((IBlogSettingsDetectionContextForCategorySchemeHack)this.context).InitialCategoryScheme;
                }

                return blogClient.GetCategories(this.context.HostBlogId);
            }
            catch (Exception ex)
            {
                this.ReportException("attempting to download categories", ex);
                return null;
            }
        }

        private BlogPostKeyword[] SafeDownloadKeywords()
        {
            try
            {
                var blogClient = this.CreateBlogClient();
                if (blogClient.Options.SupportsGetKeywords)
                {
                    return blogClient.GetKeywords(this.context.HostBlogId);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                this.ReportException("attempting to download keywords", ex);
                return null;
            }
        }

        private WriterEditingManifest SafeDownloadEditingManifest()
        {
            WriterEditingManifest editingManifest = null;
            try
            {
                // create a blog client
                var blogClient = this.CreateBlogClient();

                // can we get one based on cached download info
                var credentialsToUse = (this.IncludeInsecureOperations || blogClient.IsSecure) ? this.context.Credentials : null;
                if (this.context.ManifestDownloadInfo != null)
                {
                    var manifestUrl = this.context.ManifestDownloadInfo.SourceUrl;
                    if (this.UseManifestCache)
                    {
                        editingManifest = WriterEditingManifest.FromDownloadInfo(this.context.ManifestDownloadInfo, blogClient, credentialsToUse, true);
                    }
                    else
                    {
                        editingManifest = WriterEditingManifest.FromUrl(new Uri(manifestUrl), blogClient, credentialsToUse, true);
                    }
                }

                // if we don't have one yet then probe for one
                if (editingManifest == null)
                {
                    editingManifest = WriterEditingManifest.FromHomepage(this.homepageAccessor, new Uri(this.context.HomepageUrl), blogClient, credentialsToUse);
                }
            }
            catch (Exception ex)
            {
                this.ReportException("attempting to download editing manifest", ex);
            }

            // return whatever we found
            return editingManifest;
        }

        private void ReportException(string context, Exception ex)
        {
            var error = string.Format(CultureInfo.InvariantCulture, "Exception occurred {0} for weblog {1}: {2}", context, this.context.HomepageUrl, ex.ToString());

            if (this.SilentMode)
            {
                Trace.WriteLine(error);
            }
            else
            {
                Trace.Fail(error);
            }
        }

        // detection context
        private readonly IBlogSettingsDetectionContext context;

        // helper class for wrapping progress around steps
        private class ProgressContext : IDisposable
        {
            public ProgressContext(IProgressHost progressHost, int complete, string message)
            {
                this.progressHost = progressHost;
                this.progressHost.UpdateProgress(complete, 100, message);
            }

            public void Dispose()
            {
                if (this.progressHost.CancelRequested)
                {
                    throw new OperationCancelledException();
                }
            }

            private readonly IProgressHost progressHost;
        }
    }
}
