// <copyright file="Program.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner
{
    using System;
    using System.IO;
    using System.Text;

    public static partial class Program
    {
        /// <summary>
        /// Class ColorChangeTextWriter.
        /// Implements the <see cref="TextWriter" />.
        /// </summary>
        /// <seealso cref="TextWriter" />
        private class ColorChangeTextWriter : TextWriter
        {
            private readonly TextWriter tw;
            private readonly ConsoleColor color;

            /// <summary>
            /// Initializes a new instance of the <see cref="ColorChangeTextWriter"/> class.
            /// </summary>
            /// <param name="tw">The text writer.</param>
            /// <param name="color">The color.</param>
            public ColorChangeTextWriter(TextWriter tw, ConsoleColor color)
            {
                this.tw = tw;
                this.color = color;
            }

            /// <summary>
            /// Gets the character encoding in which the output is written when overridden in a derived class.
            /// </summary>
            /// <value>The encoding.</value>
            public override Encoding Encoding => this.tw.Encoding;

            /// <summary>
            /// Writes a character to the text string or stream.
            /// </summary>
            /// <param name="value">The character to write to the text stream.</param>
            public override void Write(char value)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = this.color;
                try
                {
                    this.tw.Write(value);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
            }

            /// <summary>
            /// Writes a subarray of characters to the text string or stream.
            /// </summary>
            /// <param name="buffer">The character array to write data from.</param>
            /// <param name="index">The character position in the buffer at which to start retrieving data.</param>
            /// <param name="count">The number of characters to write.</param>
            public override void Write(char[] buffer, int index, int count)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = this.color;
                try
                {
                    this.tw.Write(buffer, index, count);
                }
                finally
                {
                    Console.ForegroundColor = oldColor;
                }
            }
        }
    }
}
