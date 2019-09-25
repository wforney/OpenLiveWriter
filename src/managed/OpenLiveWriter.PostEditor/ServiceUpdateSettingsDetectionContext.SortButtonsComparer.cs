// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;

    using Extensibility.BlogClient;

    internal partial class ServiceUpdateSettingsDetectionContext
    {
        /// <summary>
        /// The SortButtonsComparer class.
        /// Implements the <see cref="System.Collections.IComparer" />
        /// </summary>
        /// <seealso cref="System.Collections.IComparer" />
        private class SortButtonsComparer : IComparer
        {
            /// <inheritdoc />
            public int Compare(object x, object y) =>
                string.Compare((x as IBlogProviderButtonDescription)?.Id, (y as IBlogProviderButtonDescription)?.Id,
                               StringComparison.Ordinal);
        }
    }
}
