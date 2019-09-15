// <copyright file="SupportsScriptsTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System.Diagnostics;

    /// <summary>
    /// Class SupportsScriptsTest.
    /// Implements the <see cref="BlogRunner.Core.BodyContentPostTest" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.BodyContentPostTest" />
    public class SupportsScriptsTest : BodyContentPostTest
    {
        /// <summary>
        /// Gets the body content string.
        /// </summary>
        /// <value>The body content string.</value>
        public override string BodyContentString =>
            @"<script language=""javascript"">document.write('foo!');</script>";

        /// <summary>
        /// Handles the content result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="results">The results.</param>
        public override void HandleContentResult(string result, ITestResults results)
        {
            if (result == null)
            {
                Debug.Fail("Scripts gone");
                results.AddResult("supportsScripts", "Unknown");
            }
            else if (result.ToLowerInvariant().Contains("script"))
            {
                results.AddResult("supportsScripts", YES);
            }
            else
            {
                results.AddResult("supportsScripts", NO);
            }
        }
    }
}
