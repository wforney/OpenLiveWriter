// <copyright file="CompositePostTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System.Collections.Generic;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class CompositePostTest.
    /// Implements the <see cref="BlogRunner.Core.PostTest" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.PostTest" />
    public class CompositePostTest : PostTest
    {
        private readonly List<PostTest> tests = new List<PostTest>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositePostTest"/> class.
        /// </summary>
        /// <param name="tests">The tests.</param>
        public CompositePostTest(params PostTest[] tests) => this.tests.AddRange(tests);

        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            foreach (var test in this.tests)
            {
                test.PreparePost(blogPost, ref publish);
            }
        }

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="homepageHtml">The homepage HTML.</param>
        /// <param name="results">The results.</param>
        protected internal override void HandleResult(string homepageHtml, ITestResults results)
        {
            foreach (var test in this.tests)
            {
                test.HandleResult(homepageHtml, results);
            }
        }
    }
}
