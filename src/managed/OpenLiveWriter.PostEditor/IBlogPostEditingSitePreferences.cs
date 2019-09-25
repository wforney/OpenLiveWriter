// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// The IBlogPostEditingSitePreferences interface.
    /// </summary>
    internal interface IBlogPostEditingSitePreferences
    {
        /// <summary>
        /// Gets or sets the editing site.
        /// </summary>
        /// <value>The editing site.</value>
        IBlogPostEditingSite EditingSite { get; set; }
    }
}
