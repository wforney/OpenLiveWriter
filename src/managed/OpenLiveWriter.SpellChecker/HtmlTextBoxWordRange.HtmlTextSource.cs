// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{

    using HtmlParser.Parser;

    public partial class HtmlTextBoxWordRange
    {
        /// <summary>
        /// The HtmlTextSource class.
        /// </summary>
        private class HtmlTextSource
        {
            /// <summary>
            /// The parser
            /// </summary>
            private readonly SimpleHtmlParser parser;

            /// <summary>
            /// Initializes a new instance of the <see cref="HtmlTextSource"/> class.
            /// </summary>
            /// <param name="parser">The parser.</param>
            public HtmlTextSource(SimpleHtmlParser parser) => this.parser = parser;

            /// <summary>
            /// Moves to the next item.
            /// </summary>
            /// <returns>TextWithOffsetAndLen.</returns>
            public TextWithOffsetAndLen Next()
            {
                Element e;
                while (null != (e = this.parser.Next()))
                {
                    if (e is Text)
                    {
                        return new TextWithOffsetAndLen(e.RawText, e.Offset, e.Length);
                    }
                }

                return null;
            }
        }
    }
}
