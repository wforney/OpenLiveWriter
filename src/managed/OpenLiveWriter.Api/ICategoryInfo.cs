// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Provides read-only information about a category.
    /// </summary>
    public interface ICategoryInfo
    {
        /// <summary>
        /// The ID of the category.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The name of the category.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// True if the category is newly created and does not exist
        /// on the server yet.
        /// </summary>
        bool IsNew { get; }
    }
}
