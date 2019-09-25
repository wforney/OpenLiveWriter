namespace OpenLiveWriter.SpellChecker
{
    using System.Linq;

    /// <summary>
    /// The WordRangeHelper class.
    /// </summary>
    public static class WordRangeHelper
    {
        /// <summary>
        /// Determines whether [contains only symbols] [the specified current word].
        /// </summary>
        /// <param name="currentWord">The current word.</param>
        /// <returns><c>true</c> if [contains only symbols] [the specified current word]; otherwise, <c>false</c>.</returns>
        public static bool ContainsOnlySymbols(string currentWord) =>
            // http://en.wikipedia.org/wiki/CJK_Unified_Ideographs
            // http://en.wikipedia.org/wiki/Japanese_writing_system

            // Look to see if the word is only chinese and japanese characters
            currentWord.All(c => !WordRangeHelper.IsNonSymbolChar(c));

        /// <summary>
        /// Determines whether [is non symbol character] [the specified c].
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if [is non symbol character] [the specified c]; otherwise, <c>false</c>.</returns>
        public static bool IsNonSymbolChar(char c) =>
            // Found a latin char, we should spell check this word
            (c < 19968 || c > 40959) && (c < 12352 || c > 12543);
    }
}
