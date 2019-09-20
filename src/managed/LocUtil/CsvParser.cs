// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    /// <summary>
    /// The CSV parser class.
    /// Implements the <see cref="System.Collections.IEnumerable{Array{string}}" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.Collections.IEnumerable{Array{string}}" />
    /// <seealso cref="System.IDisposable" />
    public partial class CsvParser : IEnumerable<string[]>, IDisposable
    {
        /// <summary>
        /// The input
        /// </summary>
        private readonly TextReader input;
        /// <summary>
        /// At line end
        /// </summary>
        private bool atLineEnd = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvParser"/> class.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="skipFirstLine">if set to <c>true</c> [skip first line].</param>
        public CsvParser(TextReader input, bool skipFirstLine)
        {
            this.input = input;
            if (skipFirstLine)
            {
                while (this.NextWord() != null)
                {
                }

                this.NextLine();
            }
        }

        /// <summary>
        /// Nexts the word.
        /// </summary>
        /// <returns>System.String.</returns>
        private string NextWord()
        {
            if (this.atLineEnd)
            {
                return null;
            }

            this.EatSpaces();
            switch (this.input.Peek())
            {
                case -1:
                    return null;
                case '"':
                    return this.MatchQuoted();
                case '\r':
                case '\n':
                    this.EatLineEndings();
                    return null;
                default:
                    return this.MatchUnquoted();
            }
        }

        /// <summary>
        /// Matches the quoted.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <exception cref="System.ArgumentException">Malformed CSV: unexpected character after string \"" + sb.ToString() + "\"</exception>
        private string MatchQuoted()
        {
            Debug.Assert(this.input.Read() == '"');
            var sb = new StringBuilder();
            for (var c = this.input.Read(); c != -1; c = this.input.Read())
            {
                if (c == '"')
                {
                    if (this.input.Peek() == '"')
                    {
                        this.input.Read();
                        sb.Append('"');
                    }
                    else
                    {
                        this.EatSpaces();
                        switch (this.input.Peek())
                        {
                            case ',':
                                this.input.Read();
                                return sb.ToString();
                            case '\r':
                            case '\n':
                            case -1:
                                this.atLineEnd = true;
                                this.EatLineEndings();
                                return sb.ToString();
                            default:
                                throw new ArgumentException($"Malformed CSV: unexpected character after string \"{sb.ToString()}\"");
                        }
                    }

                }
                else
                {
                    sb.Append((char)c);
                }
            }

            Debug.Fail($"Unterminated quoted string: {sb.ToString()}");
            return sb.ToString();
        }

        /// <summary>
        /// Matches the unquoted.
        /// </summary>
        /// <returns>System.String.</returns>
        private string MatchUnquoted()
        {
            var sb = new StringBuilder();
            for (var c = this.input.Read(); c != -1; c = this.input.Read())
            {
                switch (c)
                {
                    case ',':
                        return sb.ToString();
                    case '\r':
                    case '\n':
                        this.atLineEnd = true;
                        this.EatLineEndings();
                        return sb.ToString();
                    default:
                        sb.Append((char)c);
                        break;
                }
            }

            // EOF
            this.atLineEnd = true;
            return sb.ToString();
        }

        /// <summary>
        /// Eats the spaces.
        /// </summary>
        private void EatSpaces()
        {
            while (this.input.Peek() == ' ')
            {
                this.input.Read();
            }
        }

        /// <summary>
        /// Eats the line endings.
        /// </summary>
        private void EatLineEndings()
        {
            while (true)
            {
                switch (this.input.Peek())
                {
                    case '\r':
                    case '\n':
                        this.input.Read();
                        break;
                    default:
                        return;
                }
            }
        }

        /// <summary>
        /// Nexts the line.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool NextLine()
        {
            if (this.input.Peek() != -1)
            {
                this.atLineEnd = false;
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() => this.input.Close();

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator{Array{string}}" /> object that can be used to iterate through the collection.</returns>
        public IEnumerator<string[]> GetEnumerator() => new LineEnumerator(this);

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }
}
