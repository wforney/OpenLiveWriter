// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    public partial class HtmlTextBoxWordRange
    {
        /// <summary>
        /// The TextWithOffsetAndLen class.
        /// </summary>
        private class TextWithOffsetAndLen
        {
            /// <summary>
            /// The length
            /// </summary>
            public readonly int Len;

            /// <summary>
            /// The offset
            /// </summary>
            public readonly int Offset;

            /// <summary>
            /// The text
            /// </summary>
            public readonly string Text;

            /// <summary>
            /// Initializes a new instance of the <see cref="TextWithOffsetAndLen"/> class.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="offset">The offset.</param>
            /// <param name="len">The length.</param>
            public TextWithOffsetAndLen(string text, int offset, int len)
            {
                this.Text = text;
                this.Offset = offset;
                this.Len = len;
            }
        }
    }
}
