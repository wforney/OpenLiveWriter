// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Reflection;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromLiveClipboard method to enable creation of new content from Live
    /// Clipboard data.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LiveClipboardContentSourceAttribute : Attribute
    {
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
        /// End-user presentable name of data format handled by this ContentSource.
        /// </summary>
        public string Name
        {
            get => this._name;
            set => this._name = value ?? throw new ArgumentNullException("LiveClipboardContentSource.Name");
        }
        private string _name = string.Empty;

        /// <summary>
        /// MIME content-type handled by this ContentSource (corresponds to the
        /// contentType attribute of the &lt;lc:format&gt; tag)
        /// </summary>
        public string ContentType
        {
            get => this._contentType;
            set => this._contentType = value ?? throw new ArgumentNullException("LiveClipboardContentSource.ContentType");
        }
        private string _contentType = string.Empty;

        /// <summary>
        /// Path to embedded image resource used to represent this format within the Live Clipboard
        /// Preferences panel. The embedded image should be 20x18. If this attribute is not specified
        /// then the image specified in the WriterPlugin attribute is used.
        /// </summary>
        public string ImagePath
        {
            get => this._imagePath;
            set => this._imagePath = value ?? throw new ArgumentNullException("LiveClipboardContentSource.ImagePath");
        }
        private string _imagePath = string.Empty;

        /// <summary>
        /// End-user presentable description of the data format handled by this ContentSource.
        /// (used within the Live Clipboard Preferences panel (Optional).
        /// </summary>
        public string Description
        {
            get => this._description;
            set => this._description = value ?? throw new ArgumentNullException("LiveClipboardContentSource.Description");
        }
        private string _description = string.Empty;

        /// <summary>
        /// Content sub-type handled by this content source. (corresponds to the
        /// type attribute of the &lt;lc:format&gt; tag). Optional (required only
        /// for formats which require additional disambiguation of the contentType
        /// attribute).
        /// </summary>
        public string Type
        {
            get => this._type;
            set => this._type = value ?? throw new ArgumentNullException("LiveClipboardContentSource.Type");
        }
        private string _type = string.Empty;
    }
}
