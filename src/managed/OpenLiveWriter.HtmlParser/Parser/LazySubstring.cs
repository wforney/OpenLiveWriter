// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// String.Substring is very expensive, so we avoid calling it
    /// until the caller demands it.
    /// </summary>
    internal class LazySubstring
    {
        private readonly string baseString;
        private string substring;

        /// <summary>
        /// Initializes a new instance of the <see cref="LazySubstring"/> class.
        /// </summary>
        /// <param name="baseString">The base string.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public LazySubstring(string baseString, int offset, int length)
        {
            this.baseString = baseString;
            this.Offset = offset;
            this.Length = length;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value
        {
            get
            {
                if (this.substring == null)
                {
                    this.substring = this.baseString.Substring(this.Offset, this.Length);
                }

                return this.substring;
            }
        }

        /// <summary>
        /// Gets the offset.
        /// </summary>
        /// <value>The offset.</value>
        public int Offset { get; }

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>The length.</value>
        public int Length { get; }

        /// <summary>
        /// Maybes the create.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>LazySubstring.</returns>
        public static LazySubstring MaybeCreate(string val) => val == null ? null : new LazySubstring(val, 0, val.Length);
    }
}
