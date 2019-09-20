// <copyright file="BlogUtil.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;

    /// <summary>
    /// The blog utility class.
    /// </summary>
    public static class BlogUtil
    {
        /// <summary>
        /// Gets the short unique identifier.
        /// </summary>
        /// <value>The short unique identifier.</value>
        public static string ShortGuid
        {
            get
            {
                var bytes = Guid.NewGuid().ToByteArray();
                var longVal = BitConverter.ToInt64(bytes, 0) ^ BitConverter.ToInt64(bytes, 8);
                return Convert.ToBase64String(BitConverter.GetBytes(longVal)).TrimEnd('=');
            }
        }
    }
}
