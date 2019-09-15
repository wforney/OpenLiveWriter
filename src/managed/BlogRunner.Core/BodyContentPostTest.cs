// <copyright file="BodyContentPostTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System.Text.RegularExpressions;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class BodyContentPostTest.
    /// Implements the <see cref="BlogRunner.Core.PostTest" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.PostTest" />
    public abstract class BodyContentPostTest : PostTest
    {
        private string guid1;
        private string guid2;

        /// <summary>
        /// Gets the body content string.
        /// </summary>
        /// <value>The body content string.</value>
        public abstract string BodyContentString { get; }

        /// <summary>
        /// Handles the content result.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="results">The results.</param>
        public abstract void HandleContentResult(string result, ITestResults results);

        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected internal sealed override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            this.guid1 = BlogUtil.ShortGuid;
            this.guid2 = BlogUtil.ShortGuid;

            blogPost.Contents += $"\r\n<br />\r\n{this.guid1}{this.BodyContentString}{this.guid2}";
        }

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="homepageHtml">The homepage HTML.</param>
        /// <param name="results">The results.</param>
        protected internal sealed override void HandleResult(string homepageHtml, ITestResults results)
        {
            var regex = new Regex($"{Regex.Escape(this.guid1)}(.*?){Regex.Escape(this.guid2)}");
            var m = regex.Match(homepageHtml);
            string result;
            if (!m.Success)
            {
                result = null;
            }
            else
            {
                result = m.Groups[1].Value;
            }

            this.HandleContentResult(result, results);
        }
    }
}
