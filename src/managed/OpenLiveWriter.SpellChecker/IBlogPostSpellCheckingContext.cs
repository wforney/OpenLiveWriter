// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;

    /// <summary>
    /// Spelling checking services for the HTML Editor.
    /// </summary>
    public interface IBlogPostSpellCheckingContext
    {
        /// <summary>
        /// Gets a value indicating whether this instance can spell check.
        /// </summary>
        /// <value><c>true</c> if this instance can spell check; otherwise, <c>false</c>.</value>
        bool CanSpellCheck { get; }

        /// <summary>
        /// Gets the spelling checker.
        /// </summary>
        /// <value>The spelling checker.</value>
        ISpellingChecker SpellingChecker { get; }

        /// <summary>
        /// Gets the automatic correct lexicon file path.
        /// </summary>
        /// <value>The automatic correct lexicon file path.</value>
        string AutoCorrectLexiconFilePath { get; }

        /// <summary>
        /// Occurs when [spelling options changed].
        /// </summary>
        event EventHandler SpellingOptionsChanged;
    }
}
