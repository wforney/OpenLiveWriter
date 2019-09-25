// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;

    public partial class HtmlTextBoxWordRange
    {

        private partial class WordSource
        {
            /// <summary>
            /// The CharClass enumeration.
            /// </summary>
            [Flags]
            private enum CharClass
            {
                /// <summary>
                /// The break
                /// </summary>
                Break = 1,

                /// <summary>
                /// The boundary break
                /// </summary>
                BoundaryBreak = 2, // only counts as break if at start or end of word

                /// <summary>
                /// The letter or number
                /// </summary>
                LetterOrNumber = 4,

                /// <summary>
                /// The letter
                /// </summary>
                Letter = CharClass.LetterOrNumber | 8,

                /// <summary>
                /// The number
                /// </summary>
                Number = CharClass.LetterOrNumber | 0x10,

                /// <summary>
                /// The included break character
                /// </summary>
                IncludedBreakChar = 0x20 // counts as a break, but is also included in the word
            }
        }
    }
}
