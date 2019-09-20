// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    /// <summary>
    /// Class Values.
    /// </summary>
    public class Values
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Values"/> class.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="comment">The comment.</param>
        public Values(object val, string comment)
        {
            this.Val = val;
            this.Comment = comment;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object Val { get; }

        /// <summary>
        /// Gets the comment.
        /// </summary>
        /// <value>The comment.</value>
        public string Comment { get; }
    }
}
