// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Attribute which should be applied to all classes derived from WriterPlugin.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class WriterPluginAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of WriterPluginAttribute.
        /// </summary>
        /// <param name="id">Unique ID for the plugin (must be a GUID without leading and trailing braces)</param>
        /// <param name="name">Plugin name (this will appear in the Plugins preferences panel)</param>
        public WriterPluginAttribute(string id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        /// <summary>
        /// Initialize a new instance of WriterPluginAttribute.
        /// </summary>
        /// <param name="id">Unique ID for the plugin (must be a GUID without leading and trailing braces)</param>
        /// <param name="name">Plugin name (this will appear in the Plugins preferences panel)</param>
        /// <param name="imagePath">Path to embedded image resource used to represent this plugin within the
        /// Open Live Writer UI (menu bitmap, sidebar bitmap, etc.). The size of the embedded image must be 20x18.</param>
        [Obsolete("This method is for compatibility with pre-beta plugins, and will be removed in the future.")]
        public WriterPluginAttribute(string id, string name, string imagePath)
        {
            this.Id = id;
            this.Name = name;
            this.ImagePath = imagePath;
        }

        /// <summary>
        /// Unique ID for the plugin (should be a GUID without leading and trailing braces)
        /// </summary>
        public string Id
        {
            get => this.id;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("WriterPlugin.Id");
                }

                if (!Guid.TryParse(value, out _))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The value specifed ({0}) was not a GUID", value), "WriterPlugin.Id");
                }

                this.id = value;
            }
        }
        private string id;

        /// <summary>
        /// Plugin name (this will appear in the Plugins preferences panel)
        /// </summary>
        public string Name
        {
            get => this.name;
            set => this.name = value ?? throw new ArgumentNullException("WriterPlugin.Name");
        }
        private string name;

        /// <summary>
        /// Path to embedded image resource used to represent this plugin within the
        /// Open Live Writer UI (menu bitmap, sidebar bitmap, etc.). The size of
        /// the embedded image must be 20x18 or 16x16 pixels.
        /// </summary>
        /// <remarks>
        /// Early Beta versions of Open Live Writer required icons to be 20x18, but
        /// later versions prefer 16x16. Later versions of Writer will scale 20x18 images
        /// to 16x16, or, if only the center 16x16 pixels of the 20x18 are non-transparent,
        /// the image will simply be cropped to 16x16.
        /// </remarks>
        public string ImagePath { get; set; }

        /// <summary>
        /// Short description (1-2 sentences) of the plugin (displayed in the Plugins Preferences panel). Optional.
        /// </summary>
        public string Description
        {
            get => this.description;
            set => this.description = value ?? throw new ArgumentNullException("WriterPlugin.Description");
        }
        private string description = string.Empty;

        /// <summary>
        /// URL of the publisher for the Plugin (linked to from Plugins Preferences panel). Optional.
        /// </summary>
        public string PublisherUrl
        {
            get => this.publisherUrl;
            set => this.publisherUrl = value ?? throw new ArgumentNullException("WriterPlugin.PublisherUrl");
        }
        private string publisherUrl = string.Empty;

        /// <summary>
        /// Indicates whether the Plugin has editable options. That is, whether it overrides the EditOptions
        /// method of the WriterPlugin base class. Default is false.
        /// </summary>
        public bool HasEditableOptions { get; set; } = false;
    }
}
