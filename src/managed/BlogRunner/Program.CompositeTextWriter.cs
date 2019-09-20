// <copyright file="Program.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner
{
    using System.IO;
    using System.Text;

    public static partial class Program
    {
        /// <summary>
        /// Class CompositeTextWriter.
        /// Implements the <see cref="TextWriter" />.
        /// </summary>
        /// <seealso cref="TextWriter" />
        private class CompositeTextWriter : TextWriter
        {
            /// <summary>
            /// The text writer array.
            /// </summary>
            private readonly TextWriter[] writers;

            /// <summary>
            /// Initializes a new instance of the <see cref="CompositeTextWriter"/> class.
            /// </summary>
            /// <param name="writers">The writers.</param>
            public CompositeTextWriter(params TextWriter[] writers) => this.writers = writers;

            /// <summary>
            /// Gets the character encoding in which the output is written when overridden in a derived class.
            /// </summary>
            /// <value>The encoding.</value>
            public override Encoding Encoding => Encoding.Unicode;

            /// <summary>
            /// Writes a character to the text string or stream.
            /// </summary>
            /// <param name="value">The character to write to the text stream.</param>
            public override void Write(char value)
            {
                foreach (var writer in this.writers)
                {
                    writer.Write(value);
                    writer.Flush();
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
                foreach (var writer in this.writers)
                {
                    writer.Write(buffer, index, count);
                    writer.Flush();
                }
            }
        }
    }
}
