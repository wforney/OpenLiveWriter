// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections.Generic;

    internal partial class ServiceUpdateSettingsDetectionContext
    {
        /// <summary>
        /// The SortOptionOverridesComparer class.
        /// Implements the <see cref="IComparer{KeyValuePair{TKey, TValue}}" />
        /// </summary>
        /// <typeparam name="TKey">The type of the t key.</typeparam>
        /// <typeparam name="TValue">The type of the t value.</typeparam>
        /// <seealso cref="IComparer{KeyValuePair{TKey, TValue}}" />
        private class SortOptionOverridesComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            /// <inheritdoc />
            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) =>
                string.Compare(x.Key.ToString(), y.Key.ToString(), StringComparison.Ordinal);
        }
    }
}
