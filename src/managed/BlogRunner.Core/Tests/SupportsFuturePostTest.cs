// <copyright file="SupportsFuturePostTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class SupportsFuturePostTest.
    /// Implements the <see cref="BlogRunner.Core.PostTest" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.PostTest" />
    public class SupportsFuturePostTest : PostTest
    {
        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            if (blogPost == null)
            {
                throw new ArgumentNullException(nameof(blogPost));
            }

            blogPost.Title = "Future post test";
            blogPost.Contents = "foo bar";
            blogPost.DatePublishedOverride = DateTime.Now.AddDays(12.0);
        }

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="homepageHtml">The homepage HTML.</param>
        /// <param name="results">The results.</param>
        protected internal override void HandleResult(string homepageHtml, ITestResults results) =>
            (results ?? throw new ArgumentNullException(nameof(results))).AddResult("futurePublishDateWarning", YES);

        /// <summary>
        /// Return true if the timeout condition was handled. False means
        /// the caller should deal with the timeout (by throwing TimeoutException).
        /// </summary>
        /// <param name="te">The te.</param>
        /// <param name="results">The results.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected internal override bool HandleTimeout(TimeoutException te, ITestResults results)
        {
            (results ?? throw new ArgumentNullException(nameof(results))).AddResult("futurePublishDateWarning", NO);
            return true;
        }
    }
}
