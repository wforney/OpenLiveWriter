// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    /// <summary>
    /// Possible result codes from check-word call
    /// </summary>
    public enum SpellCheckResult
    {
        /// <summary>
        /// Word is correctly spelled
        /// </summary>
        Correct,

        /// <summary>
        /// Word has an auto-replace value (value returned in otherWord parameter)
        /// </summary>
        AutoReplace,

        /// <summary>
        /// Word has a conditional-replace value (value returned in otherWord parameter)
        /// </summary>
        ConditionalReplace,

        /// <summary>
        /// Word is incorrectly capitalized
        /// </summary>
        Capitalization,

        /// <summary>
        /// Word is misspelled
        /// </summary>
        Misspelled
    }
}
