// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// Class Utf8StringWriter. This class cannot be inherited.
    /// Implements the <see cref="System.IO.StringWriter" />
    /// </summary>
    /// <seealso cref="System.IO.StringWriter" />
    internal sealed class Utf8StringWriter : StringWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Utf8StringWriter"/> class.
        /// </summary>
        /// <param name="sb">The <see cref="T:System.Text.StringBuilder" /> object to write to.</param>
        public Utf8StringWriter(StringBuilder sb) : base(sb)
        {

        }

        /// <summary>
        /// Gets the <see cref="T:System.Text.Encoding" /> in which the output is written.
        /// </summary>
        /// <value>The encoding.</value>
        public override Encoding Encoding => Encoding.UTF8;
    }
}
