// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using OpenLiveWriter.HtmlParser.Parser;

namespace OpenLiveWriter.Api
{
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
            _objectHtml = string.Format(CultureInfo.InvariantCulture, "<div>{0}</div>", objectHtml);
            PreviewLink = previewLink;
        }

        /// <summary>
        /// Create a new AdaptiveHtmlObject.
        /// </summary>
        /// <param name="objectHtml">Object tag to generate adaptive HTML for.</param>
        /// <param name="previewImageSrc">HREF for image file to use as a preview for the object tag.</param>
        /// <param name="previewLink">Link to navigate to when users click the preview image.</param>
        public AdaptiveHtmlObject(string objectHtml, string previewImageSrc, string previewLink)
        {
            _objectHtml = string.Format(CultureInfo.InvariantCulture, "<div>{0}</div>", objectHtml);
            PreviewLink = previewLink;
            PreviewImageSrc = previewImageSrc;
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
        public Size PreviewImageSize
        {
            get { return _previewImageSize; }
            set { _previewImageSize = value; }
        }
        private Size _previewImageSize = Size.Empty;

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
                    return GeneratePreviewHtml();
                case HtmlType.ObjectHtml:
                    return _objectHtml;
                case HtmlType.AdaptiveHtml:
                    return GenerateAdaptiveHtml();
                default:
                    Debug.Fail("Unexpected SmartHtmlType");
                    return GeneratePreviewHtml();
            }
        }

        private string GeneratePreviewHtml()
        {
            return GenerateHtml(string.Empty);
        }

        private string GenerateAdaptiveHtml()
        {
            // unique id for the div to contain the downlevel and uplevel HTML
            string containerId = Guid.NewGuid().ToString();

            // surround the upgraded content with a DIV (required for dynamic substitution) and javascript-escape it
            string jsEscapedHtmlContent = LiteralElementMethods.JsEscape(_objectHtml, '"');

            // generate and html escape the onLoad attribute
            string onLoadScript = string.Format(CultureInfo.InvariantCulture, "var downlevelDiv = document.getElementById('{0}'); downlevelDiv.innerHTML = \"{1}\";", containerId, jsEscapedHtmlContent);
            onLoadScript = HtmlServices.HtmlEncode(onLoadScript);
            string onLoadAttribute = string.Format(CultureInfo.InvariantCulture, "onload=\"{0}\"", onLoadScript);

            // generate the upgradable image html
            StringBuilder downgradableHtml = new StringBuilder();
            downgradableHtml.AppendFormat(CultureInfo.InvariantCulture, "<div id=\"{0}\" style=\"margin: 0px; padding: 0px; display: inline;\">", containerId);
            downgradableHtml.Append(GenerateHtml(onLoadAttribute));
            downgradableHtml.AppendFormat(CultureInfo.InvariantCulture, "</div>");
            return downgradableHtml.ToString();
        }

        private string GenerateHtml(string onLoadAttribute)
        {
            // see if the caller wants a new window
            string newWindowAttribute = OpenPreviewInNewWindow ? "target=\"_new\"" : string.Empty;

            // see if the user has requested a size override
            string imageHtmlSize = !PreviewImageSize.IsEmpty ? string.Format(CultureInfo.InvariantCulture, "width=\"{0}\" height=\"{1}\"", PreviewImageSize.Width, PreviewImageSize.Height) : string.Empty;

            // return the preview html
            return string.Format(CultureInfo.InvariantCulture, "<div><a href=\"{0}\" {1}><img src=\"{2}\" style=\"border-style: none\"  galleryimg=\"no\" {3} {4} alt=\"\"></a></div>",
                                 HtmlUtils.EscapeEntities(PreviewLink),
                                 newWindowAttribute,
                                 HtmlUtils.EscapeEntities(PreviewImageSrc),
                                 imageHtmlSize,
                                 onLoadAttribute);

        }

        private readonly string _objectHtml;
    }

    /// <summary>
    /// Types of Html which can be generated by an AdaptiveHtmlObject.
    /// </summary>
    public enum HtmlType
    {
        /// <summary>
        /// Html which contains only a preview-image that navigates to the preview-link when clicked.
        /// </summary>
        PreviewHtml,

        /// <summary>
        /// Html which contains only the object tag which was passed to the constructor of the AdaptiveHtmlObject.
        /// </summary>
        ObjectHtml,

        /// <summary>
        /// Adaptive Html which attempts to render the object tag but falls back to PreviewHtml
        /// if the rendering environment does not support script.
        /// </summary>
        AdaptiveHtml
    }
}

