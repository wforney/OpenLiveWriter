// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception thrown by the CreateContent methods of the ContentSource and SmartContentSource classes
    /// when they are unable to create content due to an error. Exceptions of this type are caught
    /// and displayed using a standard content-creation error dialog.
    /// </summary>
    public class ContentCreationException : ApplicationException
    {
        /// <summary>
        /// Create a new ContentCreationException
        /// </summary>
        /// <param name="title">Title of exception (used as the caption of the error dialog).</param>
        /// <param name="description">Description of exception (used to provide additional details within the error dialog).</param>
        public ContentCreationException(string title, string description)
            : base(string.Format(CultureInfo.CurrentCulture, "{0}: {1}", title, description))
        {
            this.Title = title;
            this.Description = description;
        }

        /// <summary>
        /// Title of exception (used as the caption of the error dialog)
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Description of exception (used to provide additional details within the error dialog).
        /// </summary>
        public string Description { get; }
    }
}
