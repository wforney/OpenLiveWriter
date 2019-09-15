// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;

    /// <summary>
    /// Provides date for the HtmlDocumentAvailable event.
    /// </summary>
    public class HtmlDocumentAvailableEventArgs : EventArgs
    {
        /// <summary>
        /// Initialize a new instance of HtmlDocumentAvailableEventArgs with the specified document.
        /// </summary>
        /// <param name="document">HTML document (guaranteed to be castable to an IHTMLDocument2)</param>
        public HtmlDocumentAvailableEventArgs(object document) => this.Document = document;

        /// <summary>
        /// HTML document (guaranteed to be castable to an IHTMLDocument2)
        /// </summary>
        public object Document { get; }

        /// <summary>
        /// Value indicating whether the document is ready for a screen capture. Set this value
        /// to false to indicate that the document is not yet ready. This is useful for HTML
        /// documents that load in stages, such as documents that use embedded JavaScript to
        /// fetch and render additional content after the main document has loaded.
        /// </summary>
        public bool DocumentReady { get; set; } = true;
    }
}
