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
    using System.Xml;

    using mshtml;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;


    internal class WriterEditingManifest
    {
        public static WriterEditingManifest FromHomepage(LazyHomepageDownloader homepageDownloader, Uri homepageUri, IBlogClient blogClient, IBlogCredentialsAccessor credentials)
        {
            if (homepageUri == null)
            {
                return null;
            }

            WriterEditingManifest editingManifest = null;
            try
            {
                // compute the "by-convention" url for the manifest
                var homepageUrl = UrlHelper.InsureTrailingSlash(UrlHelper.SafeToAbsoluteUri(homepageUri));
                var manifestUrl = UrlHelper.UrlCombine(homepageUrl, "wlwmanifest.xml");

                // test to see whether this url exists and has a valid manifest
                editingManifest = FromUrl(new Uri(manifestUrl), blogClient, credentials, false);

                // if we still don't have one then scan homepage contents for a link tag
                if (editingManifest == null)
                {
                    var manifestLinkTagUrl = ScanHomepageContentsForManifestLink(homepageUri, homepageDownloader);
                    if (manifestLinkTagUrl != null)
                    {
                        // test to see whether this url exists and has a valid manifest
                        try
                        {
                            editingManifest = FromUrl(new Uri(manifestLinkTagUrl), blogClient, credentials, true);
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine("Error attempting to download manifest from " + manifestLinkTagUrl + ": " + ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Unexpected exception attempting to discover manifest from " + UrlHelper.SafeToAbsoluteUri(homepageUri) + ": " + ex.ToString());
            }

            // return whatever editing manifest we found
            return editingManifest;
        }

        public static WriterEditingManifest FromUrl(Uri manifestUri, IBlogClient blogClient, IBlogCredentialsAccessor credentials, bool expectedAvailable) =>
            FromDownloadInfo(new WriterEditingManifestDownloadInfo(UrlHelper.SafeToAbsoluteUri(manifestUri)), blogClient, credentials, expectedAvailable);

        public static WriterEditingManifest FromDownloadInfo(WriterEditingManifestDownloadInfo downloadInfo, IBlogClient blogClient, IBlogCredentialsAccessor credentials, bool expectedAvailable)
        {
            if (downloadInfo == null)
            {
                return null;
            }

            try
            {
                // if the manifest is not yet expired then don't try a download at all
                if (downloadInfo.Expires > DateTimeHelper.UtcNow)
                {
                    return new WriterEditingManifest(downloadInfo);
                }

                // execute the download
                HttpWebResponse response = null;
                try
                {
                    if (credentials != null)
                    {
                        response = blogClient.SendAuthenticatedHttpRequest(downloadInfo.SourceUrl, REQUEST_TIMEOUT, new HttpRequestFilter(new EditingManifestFilter(downloadInfo).Filter));
                    }
                    else
                    {
                        response = HttpRequestHelper.SendRequest(downloadInfo.SourceUrl, new HttpRequestFilter(new EditingManifestFilter(downloadInfo).Filter));
                    }
                }
                catch (WebException ex)
                {
                    // Not modified -- return ONLY an updated downloadInfo (not a document)
                    if (ex.Response is HttpWebResponse errorResponse && errorResponse.StatusCode == HttpStatusCode.NotModified)
                    {
                        return new WriterEditingManifest(
                            new WriterEditingManifestDownloadInfo(
                                downloadInfo.SourceUrl,
                                HttpRequestHelper.GetExpiresHeader(errorResponse),
                                downloadInfo.LastModified,
                                HttpRequestHelper.GetETagHeader(errorResponse)));
                    }
                    else
                    {
                        throw;
                    }
                }

                // read headers
                var expires = HttpRequestHelper.GetExpiresHeader(response);
                var lastModified = response.LastModified;
                var eTag = HttpRequestHelper.GetETagHeader(response);

                // read document
                using (var stream = response.GetResponseStream())
                {
                    var manifestXmlDocument = new XmlDocument();
                    manifestXmlDocument.Load(stream);

                    // return the manifest
                    return new WriterEditingManifest(
                        new WriterEditingManifestDownloadInfo(downloadInfo.SourceUrl, expires, lastModified, eTag),
                        manifestXmlDocument,
                        blogClient,
                        credentials);
                }
            }
            catch (Exception ex)
            {
                if (expectedAvailable)
                {
                    Trace.WriteLine("Error attempting to download manifest from " + downloadInfo.SourceUrl + ": " + ex.ToString());
                }
                return null;
            }
        }

        private class EditingManifestFilter
        {
            private readonly WriterEditingManifestDownloadInfo previousDownloadInfo;

            public EditingManifestFilter(WriterEditingManifestDownloadInfo previousDownloadInfo) => this.previousDownloadInfo = previousDownloadInfo;

            public void Filter(HttpWebRequest contentRequest)
            {
                if (this.previousDownloadInfo.LastModified != DateTime.MinValue)
                {
                    contentRequest.IfModifiedSince = this.previousDownloadInfo.LastModified;
                }

                if (this.previousDownloadInfo.ETag != string.Empty)
                {
                    contentRequest.Headers.Add("If-None-Match", this.previousDownloadInfo.ETag);
                }
            }
        }

        public static WriterEditingManifest FromResource(string resourcePath)
        {
            try
            {
                using (var manifestStream = new MemoryStream())
                {
                    ResourceHelper.SaveAssemblyResourceToStream(resourcePath, manifestStream);
                    manifestStream.Seek(0, SeekOrigin.Begin);
                    var manifestXmlDocument = new XmlDocument();
                    manifestXmlDocument.Load(manifestStream);
                    return new WriterEditingManifest(new WriterEditingManifestDownloadInfo("Clients.Manifests"), manifestXmlDocument, null, null);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception attempting to load manifest from resource: " + ex.ToString());
                return null;
            }
        }

        public static string DiscoverUrl(string homepageUrl, IHTMLDocument2 weblogDOM)
        {
            if (weblogDOM == null)
            {
                return string.Empty;
            }

            try
            {
                // look in the first HEAD tag
                var headElements = ((IHTMLDocument3)weblogDOM).getElementsByTagName("HEAD");
                if (headElements.length > 0)
                {
                    // get the first head element
                    var firstHeadElement = (IHTMLElement2)headElements.item(0, 0);

                    // look for link tags within the head
                    foreach (IHTMLElement element in firstHeadElement.getElementsByTagName("LINK"))
                    {
                        if (element is IHTMLLinkElement linkElement)
                        {
                            var linkRel = linkElement.rel;
                            if (linkRel != null && (linkRel.ToUpperInvariant().Equals("WLWMANIFEST")))
                            {
                                if (linkElement.href != null && linkElement.href != string.Empty)
                                {
                                    return UrlHelper.UrlCombineIfRelative(homepageUrl, linkElement.href);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to discover manifest URL: " + ex.ToString());
            }

            // couldn't find one
            return string.Empty;
        }

        public WriterEditingManifestDownloadInfo DownloadInfo { get; }

        public string ClientType { get; } = null;

        public byte[] Image { get; } = null;

        public byte[] Watermark { get; } = null;

        public IDictionary<string, string> OptionOverrides { get; } = null;

        public IBlogProviderButtonDescription[] ButtonDescriptions { get; } = null;

        public string WebLayoutUrl { get; }

        public string WebPreviewUrl { get; }

        private readonly IBlogClient blogClient = null;
        private readonly IBlogCredentialsAccessor credentials = null;

        private WriterEditingManifest(WriterEditingManifestDownloadInfo downloadInfo)
            : this(downloadInfo, null, null, null)
        {
        }

        private WriterEditingManifest(
            WriterEditingManifestDownloadInfo downloadInfo,
            XmlDocument xmlDocument,
            IBlogClient blogClient,
            IBlogCredentialsAccessor credentials)
        {
            // record blog client and credentials
            this.blogClient = blogClient;
            this.credentials = credentials;

            // record download info
            if (UrlHelper.IsUrl(downloadInfo.SourceUrl))
            {
                this.DownloadInfo = downloadInfo;
            }

            // only process an xml document if we got one
            if (xmlDocument == null)
            {
                return;
            }

            // create namespace manager
            var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
            nsmgr.AddNamespace("m", "http://schemas.microsoft.com/wlw/manifest/weblog");

            // throw if the root element is not manifest
            if (xmlDocument.DocumentElement.LocalName.ToUpperInvariant() != "MANIFEST")
            {
                throw new ArgumentException("Not a valid writer editing manifest");
            }

            // get button descriptions
            this.ButtonDescriptions = new IBlogProviderButtonDescription[] { };
            var buttonsNode = xmlDocument.SelectSingleNode("//m:buttons", nsmgr);
            if (buttonsNode != null)
            {
                var buttons = new ArrayList();

                foreach (XmlNode buttonNode in buttonsNode.SelectNodes("m:button", nsmgr))
                {
                    try
                    {
                        // id
                        var id = XmlHelper.NodeText(buttonNode.SelectSingleNode("m:id", nsmgr));
                        if (id == string.Empty)
                        {
                            throw new ArgumentException("Missing id field");
                        }

                        // title
                        var description = XmlHelper.NodeText(buttonNode.SelectSingleNode("m:text", nsmgr));
                        if (description == string.Empty)
                        {
                            throw new ArgumentException("Missing text field");
                        }

                        // imageUrl
                        var imageUrl = XmlHelper.NodeText(buttonNode.SelectSingleNode("m:imageUrl", nsmgr));
                        if (imageUrl == string.Empty)
                        {
                            throw new ArgumentException("Missing imageUrl field");
                        }

                        // download the image
                        var image = this.DownloadImage(imageUrl, downloadInfo.SourceUrl);

                        // clickUrl
                        var clickUrl = BlogClientHelper.GetAbsoluteUrl(XmlHelper.NodeText(buttonNode.SelectSingleNode("m:clickUrl", nsmgr)), downloadInfo.SourceUrl);

                        // contentUrl
                        var contentUrl = BlogClientHelper.GetAbsoluteUrl(XmlHelper.NodeText(buttonNode.SelectSingleNode("m:contentUrl", nsmgr)), downloadInfo.SourceUrl);

                        // contentDisplaySize
                        var contentDisplaySize = XmlHelper.NodeSize(buttonNode.SelectSingleNode("m:contentDisplaySize", nsmgr), Size.Empty);

                        // button must have either clickUrl or hasContent
                        if (clickUrl == string.Empty && contentUrl == string.Empty)
                        {
                            throw new ArgumentException("Must either specify a clickUrl or contentUrl");
                        }

                        // notificationUrl
                        var notificationUrl = BlogClientHelper.GetAbsoluteUrl(XmlHelper.NodeText(buttonNode.SelectSingleNode("m:notificationUrl", nsmgr)), downloadInfo.SourceUrl);

                        // add the button
                        buttons.Add(new BlogProviderButtonDescription(id, imageUrl, image, description, clickUrl, contentUrl, contentDisplaySize, notificationUrl));
                    }
                    catch (Exception ex)
                    {
                        // buttons fail silently and are not "all or nothing"
                        Trace.WriteLine("Error occurred reading custom button description: " + ex.Message);
                    }
                }

                this.ButtonDescriptions = buttons.ToArray(typeof(IBlogProviderButtonDescription)) as IBlogProviderButtonDescription[];
            }

            // get options
            this.OptionOverrides = new Dictionary<string, string>();
            this.AddOptionsFromNode(xmlDocument.SelectSingleNode("//m:weblog", nsmgr), this.OptionOverrides);
            this.AddOptionsFromNode(xmlDocument.SelectSingleNode("//m:options", nsmgr), this.OptionOverrides);
            this.AddOptionsFromNode(xmlDocument.SelectSingleNode("//m:apiOptions[@name='" + this.blogClient.ProtocolName + "']", nsmgr), this.OptionOverrides);
            var defaultViewNode = xmlDocument.SelectSingleNode("//m:views/m:default", nsmgr);
            if (defaultViewNode != null)
            {
                this.OptionOverrides["defaultView"] = XmlHelper.NodeText(defaultViewNode);
            }

            // separate out client type
            const string CLIENT_TYPE = "clientType";
            if (this.OptionOverrides.Keys.Contains(CLIENT_TYPE))
            {
                var type = this.OptionOverrides[CLIENT_TYPE].ToString();
                if (this.ValidateClientType(type))
                {
                    this.ClientType = type;
                }

                this.OptionOverrides.Remove(CLIENT_TYPE);
            }

            // separate out image
            const string IMAGE_URL = "imageUrl";
            this.Image = this.GetImageBytes(IMAGE_URL, this.OptionOverrides, downloadInfo.SourceUrl, new Size(16, 16));
            this.OptionOverrides.Remove(IMAGE_URL);

            // separate out watermark image
            const string WATERMARK_IMAGE_URL = "watermarkImageUrl";
            this.Watermark = this.GetImageBytes(WATERMARK_IMAGE_URL, this.OptionOverrides, downloadInfo.SourceUrl, new Size(84, 84));
            this.OptionOverrides.Remove(WATERMARK_IMAGE_URL);

            // get templates
            var webLayoutUrlNode = xmlDocument.SelectSingleNode("//m:views/m:view[@type='WebLayout']/@src", nsmgr);
            if (webLayoutUrlNode != null)
            {
                var webLayoutUrl = XmlHelper.NodeText(webLayoutUrlNode);
                if (webLayoutUrl != string.Empty)
                {
                    this.WebLayoutUrl = BlogClientHelper.GetAbsoluteUrl(webLayoutUrl, downloadInfo.SourceUrl);
                }
            }
            var webPreviewUrlNode = xmlDocument.SelectSingleNode("//m:views/m:view[@type='WebPreview']/@src", nsmgr);
            if (webPreviewUrlNode != null)
            {
                var webPreviewUrl = XmlHelper.NodeText(webPreviewUrlNode);
                if (webPreviewUrl != string.Empty)
                {
                    this.WebPreviewUrl = BlogClientHelper.GetAbsoluteUrl(webPreviewUrl, downloadInfo.SourceUrl);
                }
            }
        }

        private void AddOptionsFromNode(XmlNode optionsNode, IDictionary<string, string> optionOverrides)
        {
            if (optionsNode != null)
            {
                foreach (XmlNode node in optionsNode.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        optionOverrides.Add(node.LocalName, node.InnerText.Trim());
                    }
                }
            }
        }

        private bool ValidateClientType(string clientType)
        {
            clientType = clientType.Trim().ToUpperInvariant();
            return clientType == "METAWEBLOG" || clientType == "MOVABLETYPE" || clientType == "WORDPRESS";
        }

        private byte[] GetImageBytes(string elementName, IDictionary<string, string> options, string basePath, Size requiredSize)
        {
            byte[] imageBytes = null;
            if (options[elementName] is string imageUrl && imageUrl != string.Empty)
            {
                try
                {
                    var bitmap = this.DownloadImage(imageUrl, basePath);
                    var bitmapFormat = bitmap.RawFormat;
                    if (requiredSize != Size.Empty && bitmap.Size != requiredSize)
                    {
                        // shrink or grow the bitmap as appropriate
                        var correctedBitmap = new Bitmap(bitmap, requiredSize);

                        // dispose the original
                        bitmap.Dispose();

                        // return corrected
                        bitmap = correctedBitmap;
                    }

                    imageBytes = ImageHelper.GetBitmapBytes(bitmap, bitmapFormat);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Unexpected error downloading image from {0}: {1}", imageUrl, ex.ToString()));
                }
            }
            else
            {
                // indicates that no attempt to provide us an image was made
                imageBytes = new byte[0];
            }
            return imageBytes;
        }

        private Bitmap DownloadImage(string imageUrl, string basePath)
        {
            // non-url base path means embedded resource
            if (!UrlHelper.IsUrl(basePath))
            {
                var imagePath = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", basePath, imageUrl);
                var image = ResourceHelper.LoadAssemblyResourceBitmap(imagePath);
                if (image != null)
                {
                    return image;
                }
                else
                {
                    throw new ArgumentException("Invalid Image Resource Path: " + imageUrl);
                }
            }
            else
            {
                // calculate the image url
                imageUrl = UrlHelper.UrlCombineIfRelative(basePath, imageUrl);

                // try to get the credentials context
                WinInetCredentialsContext credentialsContext = null;
                try
                {
                    credentialsContext = BlogClientHelper.GetCredentialsContext(this.blogClient, this.credentials, imageUrl);
                }
                catch (BlogClientOperationCancelledException)
                {
                }

                // download the image
                return ImageHelper.DownloadBitmap(imageUrl, credentialsContext);
            }
        }

        private static string ScanHomepageContentsForManifestLink(Uri homepageUri, LazyHomepageDownloader homepageDownloader)
        {
            if (homepageDownloader.HtmlDocument != null)
            {
                var linkTags = homepageDownloader.HtmlDocument.GetTagsByName(HTMLTokens.Link);
                foreach (var linkTag in linkTags)
                {
                    var rel = linkTag.BeginTag.GetAttributeValue("rel");
                    var href = linkTag.BeginTag.GetAttributeValue("href");

                    if (rel != null && (rel.Trim().ToUpperInvariant().Equals("WLWMANIFEST") && href != null))
                    {
                        return UrlHelper.UrlCombineIfRelative(UrlHelper.SafeToAbsoluteUri(homepageUri), href);
                    }
                }
            }

            // didn't find it
            return null;
        }

        // 20-second request timeout
        private const int REQUEST_TIMEOUT = 20000;

    }

}
