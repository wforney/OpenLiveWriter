// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;

    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromLiveClipboard method to enable creation of new content from Live
    /// Clipboard data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LiveClipboardContentSourceAttribute : Attribute
    {
        private string contentType = string.Empty;
        private string description = string.Empty;
        private string imagePath = string.Empty;
        private string name = string.Empty;
        private string type = string.Empty;

        /// <summary>
        /// Initialize a new instance of a LiveClipboardContentSourceAttribute
        /// </summary>
        /// <param name="name">End-user presentable name of data format handled by this ContentSource.</param>
        /// <param name="contentType">MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the &lt;lc:format&gt; tag)</param>
        public LiveClipboardContentSourceAttribute(string name, string contentType)
        {
            this.Name = name;
            this.ContentType = contentType;
        }

        /// <summary>
        /// MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the &lt;lc:format&gt; tag)
        /// </summary>
        public string ContentType
        {
            get => this.contentType;
            set => this.contentType = value ?? throw new ArgumentNullException("LiveClipboardContentSource.ContentType");
        }

        /// <summary>
        /// End-user presentable name of data format handled by this ContentSource.
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.name = value ?? throw new ArgumentNullException("LiveClipboardContentSource.Name");
        }

        /// <summary>
        /// Path to embedded image resource used to represent this format within the Live Clipboard
        /// Preferences panel. The embedded image should be 20x18. If this attribute is not specified
        /// then the image specified in the WriterPlugin attribute is used.
        /// </summary>
        public string ImagePath
        {
            get => this.imagePath;
            set => this.imagePath = value ?? throw new ArgumentNullException("LiveClipboardContentSource.ImagePath");
        }

        /// <summary>
        /// End-user presentable description of the data format handled by this ContentSource.
        /// (used within the Live Clipboard Preferences panel (Optional).
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.description = value ?? throw new ArgumentNullException("LiveClipboardContentSource.Description");
        }

        /// <summary>
        /// Content sub-type handled by this content source. (corresponds to the
        /// type attribute of the &lt;lc:format&gt; tag). Optional (required only
        /// for formats which require additional disambiguation of the contentType
        /// attribute).
        /// </summary>
        public string Type
        {
            get => this.type;
            set => this.type = value ?? throw new ArgumentNullException("LiveClipboardContentSource.Type");
        }
    }
}
