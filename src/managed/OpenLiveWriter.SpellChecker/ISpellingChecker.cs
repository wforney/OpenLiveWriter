// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;

    /// <summary>
    /// Generic interface implemented by spell checking engines
    /// </summary>
    public interface ISpellingChecker : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        bool IsInitialized { get; }

        /// <summary>
        /// Notify the spell checker that we are going to start checking a document
        /// and that we would like the user's Ignore All and Replace All commands
        /// to be persisted in a context-bound dictionary
        /// </summary>
        void StartChecking();

        /// <summary>
        /// Notify the spell checker that we have stopped checking the document
        /// </summary>
        void StopChecking();

        /// <summary>
        /// Check the spelling of the specified word
        /// </summary>
        /// <param name="word">word to check</param>
        /// <param name="otherWord">auto or conditional replace word (returned only
        /// for certain SpellCheckResult values)</param>
        /// <returns>check-word result</returns>
        SpellCheckResult CheckWord(string word, out string otherWord, out int offset, out int length);

        /// <summary>
        /// Suggest alternate spellings for the specified word
        /// </summary>
        /// <param name="word">word to get suggestions for</param>
        /// <param name="maxSuggestions">maximum number of suggestions to return</param>
        /// <param name="depth">depth of search -- 0 to 100 where larger values
        /// indicated a deeper (and longer) search</param>
        /// <returns>array of spelling suggestions (up to maxSuggestions long)</returns>
        SpellingSuggestion[] Suggest(string word, short maxSuggestions, short depth);

        /// <summary>
        /// Add a word to the permanent user-dictionary
        /// </summary>
        /// <param name="word"></param>
        void AddToUserDictionary(string word);

        /// <summary>
        /// Occurs when [word added].
        /// </summary>
        event EventHandler WordAdded;

        /// <summary>
        /// Ignore all subsequent instances of the specified word
        /// </summary>
        /// <param name="word">word to ignore</param>
        void IgnoreAll(string word);

        /// <summary>
        /// Occurs when [word ignored].
        /// </summary>
        event EventHandler WordIgnored;

        /// <summary>
        /// Replace all subsequent instances of the specified word
        /// </summary>
        /// <param name="word">word to replace</param>
        /// <param name="replaceWith">replacement word</param>
        void ReplaceAll(string word, string replaceWith);
    }
}
