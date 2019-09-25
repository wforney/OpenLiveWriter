// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// The SpellingLanguageEntry class.
    /// </summary>
    public class SpellingLanguageEntry
    {
        /// <summary>
        /// The BCP47 code
        /// </summary>
        public readonly string BCP47Code;

        /// <summary>
        /// The display name
        /// </summary>
        public readonly string DisplayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellingLanguageEntry"/> class.
        /// </summary>
        /// <param name="bcp47Code">The BCP47 code.</param>
        /// <param name="displayName">The display name.</param>
        public SpellingLanguageEntry(string bcp47Code, string displayName)
        {
            this.BCP47Code = bcp47Code;
            this.DisplayName = displayName;
        }

        /// <inheritdoc />
        public override string ToString() => this.DisplayName;
    }
}
