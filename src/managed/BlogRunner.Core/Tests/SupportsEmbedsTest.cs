// <copyright file="SupportsEmbedsTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System;

    /// <summary>
    /// Class SupportsEmbedsTest.
    /// Implements the <see cref="BlogRunner.Core.BodyContentPostTest" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.BodyContentPostTest" />
    public class SupportsEmbedsTest : BodyContentPostTest
    {
        /// <summary>
        /// Gets the body content string.
        /// </summary>
        /// <value>The body content string.</value>
        public override string BodyContentString =>
            @"<embed src=""http://s3.amazonaws.com/slideshare/ssplayer2.swf?doc=inconvenient-truth-posters1319"" type=""application/x-shockwave-flash"" allowscriptaccess=""always"" allowfullscreen=""true"" width=""425"" height=""355"" />";

        /// <summary>
        /// Handles the content result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="results">The results.</param>
        /// <exception cref="System.InvalidOperationException">Embed test markers were not found.</exception>
        public override void HandleContentResult(string result, ITestResults results)
        {
            if (result == null)
            {
                throw new InvalidOperationException("Embed test markers were not found!");
            }
            else if (result.ToLowerInvariant().Contains("<embed"))
            {
                results.AddResult("supportsEmbeds", YES);
            }
            else
            {
                results.AddResult("supportsEmbeds", NO);
            }
        }
    }
}
