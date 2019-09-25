// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.Linq;
    using PlatformSpellCheck;

    /// <summary>
    /// The WinSpellingChecker class.
    /// Implements the <see cref="OpenLiveWriter.SpellChecker.ISpellingChecker" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.SpellChecker.ISpellingChecker" />
    /// <seealso cref="System.IDisposable" />
    public class WinSpellingChecker : ISpellingChecker, IDisposable
    {
        /// <summary>
        /// The BCP47 code
        /// </summary>
        private string bcp47Code;

        /// <summary>
        /// The speller
        /// </summary>
        private SpellChecker speller;

        /// <summary>
        /// Gets a value indicating whether this instance is initialized.
        /// </summary>
        /// <value><c>true</c> if this instance is initialized; otherwise, <c>false</c>.</value>
        public bool IsInitialized => this.speller != null;

        /// <summary>
        /// Occurs when [word added].
        /// </summary>
        public event EventHandler WordAdded;

        /// <summary>
        /// Occurs when [word ignored].
        /// </summary>
        public event EventHandler WordIgnored;

        /// <inheritdoc />
        public void AddToUserDictionary(string word)
        {
            this.CheckInitialized();
            this.speller.Add(word);

            this.WordAdded?.Invoke(word, EventArgs.Empty);
        }

        /// <inheritdoc />
        public SpellCheckResult CheckWord(string word, out string otherWord, out int offset, out int length)
        {
            this.CheckInitialized();
            otherWord = null;

            if (string.IsNullOrEmpty(word))
            {
                offset = 0;
                length = 0;
                return SpellCheckResult.Correct;
            }

            var spellerStatus = this.speller.Check(word).FirstOrDefault();

            if (spellerStatus == null)
            {
                offset = 0;
                length = word.Length;
                return SpellCheckResult.Correct;
            }

            offset = (int) spellerStatus.StartIndex;
            length = (int) spellerStatus.Length;

            switch (spellerStatus.RecommendedAction)
            {
                case RecommendedAction.Delete:
                    otherWord = "";
                    return SpellCheckResult.AutoReplace;

                case RecommendedAction.Replace:
                    otherWord = spellerStatus.RecommendedReplacement;
                    return SpellCheckResult.AutoReplace;

                case RecommendedAction.GetSuggestions:
                    return SpellCheckResult.Misspelled;

                case RecommendedAction.None:
                default:
                    return SpellCheckResult.Correct;
            }
        }

        /// <inheritdoc />
        public void Dispose() => this.StopChecking();

        /// <inheritdoc />
        public void IgnoreAll(string word)
        {
            this.CheckInitialized();
            this.speller.Ignore(word);

            this.WordIgnored?.Invoke(word, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void ReplaceAll(string word, string replaceWith)
        {
            this.CheckInitialized();
            this.speller.AutoCorrect(word, replaceWith);
        }

        /// <inheritdoc />
        public void StartChecking()
        {
            if (!SpellChecker.IsPlatformSupported() ||
                string.IsNullOrEmpty(this.bcp47Code))
            {
                this.StopChecking();
                return;
            }

            this.speller = new SpellChecker(this.bcp47Code);
        }

        /// <inheritdoc />
        public void StopChecking()
        {
            this.speller?.Dispose();
            this.speller = null;
        }

        /// <inheritdoc />
        public SpellingSuggestion[] Suggest(string word, short maxSuggestions, short depth)
        {
            this.CheckInitialized();

            return this.speller.Suggestions(word)
                       .Take(maxSuggestions)
                       .Select(suggestion => new SpellingSuggestion(suggestion, 1))
                       .ToArray();
        }

        /// <summary>
        /// Sets the options.
        /// </summary>
        /// <param name="bcp47Code">The BCP47 code.</param>
        public void SetOptions(string bcp47Code) => this.bcp47Code = bcp47Code;

        /// <summary>
        /// Gets the installed languages.
        /// </summary>
        /// <returns>System.String[].</returns>
        public static string[] GetInstalledLanguages() =>
            SpellChecker.IsPlatformSupported() ? SpellChecker.SupportedLanguages.ToArray() : new string[0];

        /// <summary>
        /// Determines whether [is language supported] [the specified BCP47 code].
        /// </summary>
        /// <param name="bcp47Code">The BCP47 code.</param>
        /// <returns><c>true</c> if [is language supported] [the specified BCP47 code]; otherwise, <c>false</c>.</returns>
        public static bool IsLanguageSupported(string bcp47Code) =>
            !string.IsNullOrEmpty(bcp47Code) &&
            (SpellChecker.IsPlatformSupported() && SpellChecker.IsLanguageSupported(bcp47Code));

        /// <summary>
        /// Checks the initialized.
        /// </summary>
        /// <exception cref="InvalidOperationException">Operation attempted on an uninitialized WinSpellingChecker</exception>
        private void CheckInitialized()
        {
            if (!this.IsInitialized)
            {
                throw new InvalidOperationException("Operation attempted on an uninitialized WinSpellingChecker");
            }
        }
    }
}
