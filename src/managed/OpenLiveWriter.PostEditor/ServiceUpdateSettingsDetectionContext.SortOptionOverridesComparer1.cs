// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;

    internal partial class ServiceUpdateSettingsDetectionContext
    {
        /// <summary>
        /// The SortOptionOverridesComparer class.
        /// Implements the <see cref="IComparer" />
        /// </summary>
        /// <seealso cref="IComparer" />
        private class SortOptionOverridesComparer : IComparer
        {
            /// <inheritdoc />
            public int Compare(object x, object y)
            {
                var xKey = (x is DictionaryEntry xk ? xk : default).Key.ToString();
                var yKey = (y is DictionaryEntry yk ? yk : default).Key.ToString();
                return string.Compare(xKey, yKey, StringComparison.Ordinal);
            }
        }
    }
}
