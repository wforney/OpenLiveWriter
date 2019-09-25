// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Suggestion for a misspelled word
    /// </summary>
    public struct SpellingSuggestion
    {
        /// <summary>
        /// Initialize with hte appropriate suggestion and score
        /// </summary>
        /// <param name="suggestion">suggestion</param>
        /// <param name="score">score</param>
        public SpellingSuggestion(string suggestion, short score)
        {
            this.Suggestion = suggestion;
            this.Score = score;
        }

        /// <summary>
        /// Suggested word
        /// </summary>
        public readonly string Suggestion;

        /// <summary>
        /// Score (0-100 percent where 100 indicates an exact match)
        /// </summary>
        public readonly short Score;
    }
}
