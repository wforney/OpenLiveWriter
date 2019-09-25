// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using Mshtml;

    /// <summary>
    /// Summary description for MisspelledWordInfo.
    /// </summary>
    public class MisspelledWordInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MisspelledWordInfo"/> class.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="word">The word.</param>
        public MisspelledWordInfo(MarkupRange range, string word)
        {
            this.WordRange = range;
            this.Word = word;
        }

        /// <summary>
        /// Gets the word range.
        /// </summary>
        /// <value>The word range.</value>
        public MarkupRange WordRange { get; }

        /// <summary>
        /// Gets the word.
        /// </summary>
        /// <value>The word.</value>
        public string Word { get; }
    }
}
