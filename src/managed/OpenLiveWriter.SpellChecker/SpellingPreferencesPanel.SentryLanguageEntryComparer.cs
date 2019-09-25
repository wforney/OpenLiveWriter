// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Collections;
    using System.Globalization;

    public partial class SpellingPreferencesPanel
    {
        /// <summary>
        /// The SentryLanguageEntryComparer class.
        /// Implements the <see cref="System.Collections.IComparer" />
        /// </summary>
        /// <seealso cref="System.Collections.IComparer" />
        private class SentryLanguageEntryComparer : IComparer
        {
            /// <summary>
            /// The culture information
            /// </summary>
            private readonly CultureInfo cultureInfo;

            /// <summary>
            /// Initializes a new instance of the <see cref="SentryLanguageEntryComparer"/> class.
            /// </summary>
            /// <param name="cultureInfo">The culture information.</param>
            public SentryLanguageEntryComparer(CultureInfo cultureInfo) => this.cultureInfo = cultureInfo;

            /// <inheritdoc />
            public int Compare(object x, object y) =>
                string.Compare(
                    ((SpellingLanguageEntry)x)?.DisplayName,
                    ((SpellingLanguageEntry)y)?.DisplayName,
                    true, this.cultureInfo);
        }
    }
}
