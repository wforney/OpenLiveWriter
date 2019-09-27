// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Text;

    using OpenLiveWriter.HtmlParser.Parser;

    /// <summary>
    /// <para>
    /// Generates HTML for an object tag which will be included on the page only if
    /// script is enabled in the environment where the page is rendered. If script is
    /// not enabled then a preview image which links to an external web-page is included
    /// on the page.
    /// </para>
    /// <para>
    /// This class is useful when you don't know the exact rendering capabilities of
    /// target publishing environment or if you expect the content to be viewed in
    /// more than one environment (e.g. weblog article within the browser and RSS
    /// feed item within a feed reader).
    /// </para>
    /// </summary>
    public class AdaptiveHtmlObject
    {
        /// <summary>
        /// Create a new AdaptiveHtmlObject.
        /// </summary>
        /// <param name="objectHtml">Object tag to generate adaptive HTML for.</param>
        /// <param name="previewLink">Link to navigate to when users click the preview image.</param>
        public AdaptiveHtmlObject(string objectHtml, string previewLink)
        {
            this.objectHtml = string.Format(CultureInfo.InvariantCulture, "<div>{0}</div>", objectHtml);
            this.PreviewLink = previewLink;
        }

        /// <summary>
        /// Create a new AdaptiveHtmlObject.
        /// </summary>
        /// <param name="objectHtml">Object tag to generate adaptive HTML for.</param>
        /// <param name="previewImageSrc">HREF for image file to use as a preview for the object tag.</param>
        /// <param name="previewLink">Link to navigate to when users click the preview image.</param>
        public AdaptiveHtmlObject(string objectHtml, string previewImageSrc, string previewLink)
        {
            this.objectHtml = string.Format(CultureInfo.InvariantCulture, "<div>{0}</div>", objectHtml);
            this.PreviewLink = previewLink;
            this.PreviewImageSrc = previewImageSrc;
        }

        /// <summary>
        /// HREF for image file to use as a preview for the object tag.
        /// </summary>
        public string PreviewImageSrc { get; set; }

        /// <summary>
        /// Link to navigate to when users click the preview image.
        /// </summary>
        public string PreviewLink { get; set; }

        /// <summary>
        /// <para>Size which the preview-image should be rendered at.</para>
        /// <para>This property should only be specified if you wish to render the image at
        /// size different from its actual size (the image will be scaled by the browser to
        /// the specified size.</para>
        /// </summary>
        public Size PreviewImageSize { get; set; } = Size.Empty;

        /// <summary>
        /// Open the preview link in a new browser window (defaults to false).
        /// </summary>
        public bool OpenPreviewInNewWindow { get; set; } = false;

        /// <summary>
        /// Generate the specified type of Html.
        /// </summary>
        /// <param name="type">Html type.</param>
        /// <returns>Generated Html.</returns>
        public string GenerateHtml(HtmlType type)
        {
            switch (type)
            {
                case HtmlType.PreviewHtml:
                    return this.GeneratePreviewHtml();
                case HtmlType.ObjectHtml:
                    return this.objectHtml;
                case HtmlType.AdaptiveHtml:
                    return this.GenerateAdaptiveHtml();
                default:
                    Debug.Fail("Unexpected SmartHtmlType");
                    return this.GeneratePreviewHtml();
            }
        }

        private string GeneratePreviewHtml() => this.GenerateHtml(string.Empty);

        private string GenerateAdaptiveHtml()
        {
            // unique id for the div to contain the downlevel and uplevel HTML
            var containerId = Guid.NewGuid().ToString();

            // surround the upgraded content with a DIV (required for dynamic substitution) and javascript-escape it
            var jsEscapedHtmlContent = LiteralElementMethods.JsEscape(this.objectHtml, '"');

            // generate and html escape the onLoad attribute
            var onLoadScript = string.Format(CultureInfo.InvariantCulture, "var downlevelDiv = document.getElementById('{0}'); downlevelDiv.innerHTML = \"{1}\";", containerId, jsEscapedHtmlContent);
            onLoadScript = HtmlServices.HtmlEncode(onLoadScript);
            var onLoadAttribute = string.Format(CultureInfo.InvariantCulture, "onload=\"{0}\"", onLoadScript);

            // generate the upgradable image html
            var downgradableHtml = new StringBuilder();
            downgradableHtml.AppendFormat(CultureInfo.InvariantCulture, "<div id=\"{0}\" style=\"margin: 0px; padding: 0px; display: inline;\">", containerId);
            downgradableHtml.Append(this.GenerateHtml(onLoadAttribute));
            downgradableHtml.AppendFormat(CultureInfo.InvariantCulture, "</div>");
            return downgradableHtml.ToString();
        }

        private string GenerateHtml(string onLoadAttribute)
        {
            // see if the caller wants a new window
            var newWindowAttribute = this.OpenPreviewInNewWindow ? "target=\"_new\"" : string.Empty;

            // see if the user has requested a size override
            var imageHtmlSize = !this.PreviewImageSize.IsEmpty ? string.Format(CultureInfo.InvariantCulture, "width=\"{0}\" height=\"{1}\"", this.PreviewImageSize.Width, this.PreviewImageSize.Height) : string.Empty;

            // return the preview html
            return string.Format(CultureInfo.InvariantCulture, "<div><a href=\"{0}\" {1}><img src=\"{2}\" style=\"border-style: none\"  galleryimg=\"no\" {3} {4} alt=\"\"></a></div>",
                                 HtmlUtils.EscapeEntities(this.PreviewLink),
                                 newWindowAttribute,
                                 HtmlUtils.EscapeEntities(this.PreviewImageSrc),
                                 imageHtmlSize,
                                 onLoadAttribute);

        }

        private readonly string objectHtml;
    }
}

