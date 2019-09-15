// <copyright file="TitleEncodingTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System;
    using System.Text.RegularExpressions;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.HtmlParser.Parser;

    /// <summary>
    /// Class TitleEncodingTest.
    /// Implements the <see cref="BlogRunner.Core.PostTest" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.PostTest" />
    public class TitleEncodingTest : PostTest
    {
        private const string TestString = "<b>&amp;&amp;amp;</b>";
        private string guid1;
        private string guid2;

        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected internal override void PreparePost(BlogPost blogPost, ref bool? publish)
        {
            this.guid1 = BlogUtil.ShortGuid;
            this.guid2 = BlogUtil.ShortGuid;

            blogPost.Title = this.guid1 + TestString + this.guid2;
            blogPost.Contents = "foo";
        }

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="homepageHtml">The homepage HTML.</param>
        /// <param name="results">The results.</param>
        /// <exception cref="System.InvalidOperationException">Title encoding test failed--title was not detected.</exception>
        protected internal override void HandleResult(string homepageHtml, ITestResults results)
        {
            var regex = new Regex(Regex.Escape(this.guid1) + "(.*?)" + Regex.Escape(this.guid2));

            var parser = new SimpleHtmlParser(homepageHtml);
            for (var e = parser.Next(); e != null; e = parser.Next())
            {
                if (e is Text)
                {
                    var m = regex.Match(e.ToString());
                    if (m.Success)
                    {
                        var str = m.Groups[1].Value;
                        if (str == HtmlUtils.EscapeEntities(TestString))
                        {
                            results.AddResult("requiresHtmlTitles", YES);
                        }
                        else if (str == HtmlUtils.EscapeEntities(HtmlUtils.EscapeEntities(TestString)))
                        {
                            results.AddResult("requiresHtmlTitles", NO);
                        }
                        else
                        {
                            results.AddResult("requiresHtmlTitles", $"[ERROR] (value was: {str})");
                        }

                        return;
                    }
                }
            }

            throw new InvalidOperationException("Title encoding test failed--title was not detected");
        }
    }
}
