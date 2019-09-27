// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Progress
{
    using System;

    /// <summary>
    /// Summary description for IProgressCategoryProvider.
    /// </summary>
    public interface IProgressCategoryProvider
    {
        /// <summary>
        /// Should the user-interface show categories
        /// </summary>
        bool ShowCategories { get; }

        /// <summary>
        /// The current category that is being processed
        /// </summary>
        ProgressCategory CurrentCategory { get; }

        /// <summary>
        /// category changed
        /// </summary>
        event EventHandler ProgressCategoryChanged;
    }
}
