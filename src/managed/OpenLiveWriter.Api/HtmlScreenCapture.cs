// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Drawing;
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Provides the ability to capture HTML content into a Bitmap.
    /// </summary>
    public class HtmlScreenCapture
    {
        /// <summary>
        /// Initialize a screen capture for an HTML page located at a URL.
        /// </summary>
        /// <param name="url">Url of HTML page to capture.</param>
        /// <param name="contentWidth">Width of content.</param>
        public HtmlScreenCapture(Uri url, int contentWidth)
        {
            this.htmlScreenCapture = new HtmlScreenCaptureCore(url, contentWidth);
            this.SubscribeToEvents();
        }

        /// <summary>
        /// Initialize a screen capture for a snippet of HTML content.
        /// </summary>
        /// <param name="htmlContent">HTML snippet to capture.</param>
        /// <param name="contentWidth">Width of content.</param>
        public HtmlScreenCapture(string htmlContent, int contentWidth)
        {
            this.htmlScreenCapture = new HtmlScreenCaptureCore(htmlContent, contentWidth);
            this.SubscribeToEvents();
        }

        /// <summary>
        /// Maximum height to capture. If this value is not specified then
        /// the height will be determined by the size of the page or HTML snippet.
        /// </summary>
        public int MaximumHeight
        {
            get => this.htmlScreenCapture.MaximumHeight;
            set => this.htmlScreenCapture.MaximumHeight = value;
        }

        /// <summary>
        /// Indicates that the HTML document to be captured is available. This event
        /// allows subscribers to examine the document in order to determine whether
        /// the page is fully loaded and ready for capture).
        /// </summary>
        public event HtmlDocumentAvailableHandler HtmlDocumentAvailable;

        /// <summary>
        /// Indicates that a a candidate screen capture is available. This event
        /// allows subscribers to examine the screen capture bitmap in order to determine
        /// whether the page is fully loaded and ready for capture.
        /// </summary>
        public event HtmlScreenCaptureAvailableHandler HtmlScreenCaptureAvailable;

        /// <summary>
        /// Perform an HTML screen capture.
        /// </summary>
        /// <param name="timeoutMs">Timeout (in ms). A timeout value greater than 0 must be specified for all screen captures.</param>
        /// <returns>Bitmap containing captured HTML (or null if a timeout occurred).</returns>
        public Bitmap CaptureHtml(int timeoutMs) => this.htmlScreenCapture.CaptureHtml(timeoutMs);

        private void SubscribeToEvents()
        {
            this.htmlScreenCapture.HtmlDocumentAvailable += new HtmlDocumentAvailableHandlerCore(this._htmlScreenCapture_HtmlDocumentAvailable);
            this.htmlScreenCapture.HtmlScreenCaptureAvailable += new HtmlScreenCaptureAvailableHandlerCore(this._htmlScreenCapture_HtmlScreenCaptureAvailable);
        }

        private void _htmlScreenCapture_HtmlDocumentAvailable(object sender, HtmlDocumentAvailableEventArgsCore e)
        {
            if (HtmlDocumentAvailable != null)
            {
                var ea = new HtmlDocumentAvailableEventArgs(e.Document);
                HtmlDocumentAvailable(this, ea);
                e.DocumentReady = ea.DocumentReady;
            }
        }

        private void _htmlScreenCapture_HtmlScreenCaptureAvailable(object sender, HtmlScreenCaptureAvailableEventArgsCore e)
        {
            if (HtmlScreenCaptureAvailable != null)
            {
                var ea = new HtmlScreenCaptureAvailableEventArgs(e.Bitmap);
                HtmlScreenCaptureAvailable(this, ea);
                e.CaptureCompleted = ea.CaptureCompleted;
            }
        }

        private readonly HtmlScreenCaptureCore htmlScreenCapture;
    }

    /// <summary>
    /// Represents the method that will handle the HtmlScreenCaptureAvailable event of the HtmlScreenCapture class.
    /// </summary>
    public delegate void HtmlScreenCaptureAvailableHandler(object sender, HtmlScreenCaptureAvailableEventArgs e);

    /// <summary>
    /// Represents the method that will handle the HtmlDocumentAvailable event of the HtmlScreenCapture class.
    /// </summary>
    public delegate void HtmlDocumentAvailableHandler(object sender, HtmlDocumentAvailableEventArgs e);
}
