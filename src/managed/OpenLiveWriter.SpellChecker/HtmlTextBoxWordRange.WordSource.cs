// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{

    using HtmlParser.Parser;

    public partial class HtmlTextBoxWordRange
    {
        /// <summary>
        /// The WordSource class.
        /// </summary>
        private partial class WordSource
        {
            /// <summary>
            /// The current value
            /// </summary>
            private TextWithOffsetAndLen current;

            /// <summary>
            /// The offset
            /// </summary>
            private int offset;

            /// <summary>
            /// The source
            /// </summary>
            private readonly HtmlTextSource source;

            /// <summary>
            /// Initializes a new instance of the <see cref="WordSource"/> class.
            /// </summary>
            /// <param name="src">The source.</param>
            public WordSource(HtmlTextSource src)
            {
                this.source = src;
                this.current = src.Next();
            }

            /// <summary>
            /// Moves to the next item.
            /// </summary>
            /// <returns>TextWithOffsetAndLen.</returns>
            public TextWithOffsetAndLen Next()
            {
                while (true)
                {
                    // No chunks left.
                    if (this.current == null)
                    {
                        return null;
                    }

                    // Advance until we get to the next potential start of word.
                    // Note that this may not turn out to be an actual word, e.g.
                    // if it is all numbers.
                    this.AdvanceUntilWordStart();

                    if (this.Eos()) // Reached end of this chunk
                    {
                        this.offset = 0;
                        this.current = this.source.Next();
                        continue; // Try again with new chunk (or null, in which case we exit)
                    }

                    // Move to the end of the word.  Note that BoundaryWordBreak
                    // characters may not end the word.  For example, for the
                    // string "'that's'" (including single quotes), the word is
                    // "that's" (note outer single quotes dropped).
                    var start = this.offset;
                    var endOfWord = this.offset;
                    do
                    {
                        var charClass = WordSource.ClassifyChar(this.current.Text, this.offset, out var charsToConsume);
                        if (WordSource.Test(charClass, CharClass.Break))
                        {
                            break;
                        }

                        this.offset += charsToConsume;
                        if (WordSource.Test(charClass, CharClass.IncludedBreakChar))
                        {
                            endOfWord = this.offset;
                            break;
                        }

                        if (WordSource.Test(charClass, CharClass.LetterOrNumber))
                        {
                            endOfWord = this.offset;
                        }
                    } while (!this.Eos());

                    var substring = this.current.Text.Substring(start, endOfWord - start);
                    if (substring.Length > 0)
                    {
                        return new TextWithOffsetAndLen(
                            HtmlUtils.UnEscapeEntities(substring, HtmlUtils.UnEscapeMode.NonMarkupText),
                            this.current.Offset + start,
                            substring.Length);
                    }
                }
            }

            /// <summary>
            /// Advances the until word start.
            /// </summary>
            private void AdvanceUntilWordStart()
            {
                while (!this.Eos() && !WordSource.Test(WordSource.ClassifyChar(this.current.Text, this.offset, out var charsToConsume),
                                                 CharClass.LetterOrNumber))
                {
                    this.offset += charsToConsume;
                }
            }

            /// <summary>
            /// Tests the specified value.
            /// </summary>
            /// <param name="val">The value.</param>
            /// <param name="comparand">The comparand.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            private static bool Test(CharClass val, CharClass comparand) => (val & comparand) == comparand;

            /// <summary>
            /// Determines the type of character that is currently pointed to by _offset,
            /// </summary>
            /// <param name="stringValue">The string value.</param>
            /// <param name="offset">The offset.</param>
            /// <param name="charsToConsume">The chars to consume.</param>
            /// <returns>CharClass.</returns>
            private static CharClass ClassifyChar(string stringValue, int offset, out int charsToConsume)
            {
                charsToConsume = 1;
                var currentChar = stringValue[offset];

                if (currentChar == '&')
                {
                    var nextSemi = stringValue.IndexOf(';', offset + 1);
                    if (nextSemi != -1)
                    {
                        var code = HtmlUtils.DecodeEntityReference(stringValue.Substring(offset + 1, nextSemi - offset - 1));
                        if (code != -1)
                        {
                            charsToConsume = nextSemi - offset + 1;
                            currentChar = (char)code;
                        }
                    }
                }

                return
                    !WordRangeHelper.IsNonSymbolChar(currentChar) ? CharClass.Break :
                    char.IsLetter(currentChar) ? CharClass.Letter :
                    char.IsNumber(currentChar) ? CharClass.Number :
                    currentChar == '\'' ? CharClass.BoundaryBreak :
                    currentChar == '’' ? CharClass.BoundaryBreak :
                    currentChar == '.' ? CharClass.IncludedBreakChar :
                    CharClass.Break;
            }

            /// <summary>
            /// Whether we are at the end of the string
            /// </summary>
            /// <returns><c>true</c> if at the end, <c>false</c> otherwise.</returns>
            private bool Eos() => this.offset >= this.current.Text.Length;
        }
    }
}
