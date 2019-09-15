// <copyright file="SupportsMultipleCategoriesTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System;
    using BlogRunner.Core.Config;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class SupportsMultipleCategoriesTest.
    /// Implements the <see cref="RoundtripTest" />.
    /// </summary>
    /// <seealso cref="RoundtripTest" />
    public class SupportsMultipleCategoriesTest : RoundtripTest
    {
        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <exception cref="InvalidOperationException">Blog " + blog.HomepageUrl + " does not have enough categories for the SupportsMultipleCategories test to be performed.</exception>
        protected internal override void PreparePost(Blog blog, IBlogClient blogClient, BlogPost blogPost, ref bool? publish)
        {
            var categories = blogClient.GetCategories(blog.BlogId);
            if (categories.Length < 2)
            {
                throw new InvalidOperationException($"Blog {blog.HomepageUrl} does not have enough categories for the SupportsMultipleCategories test to be performed");
            }

            var newCategories = new BlogPostCategory[2];
            newCategories[0] = categories[0];
            newCategories[1] = categories[1];
            blogPost.Categories = newCategories;
        }

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="results">The results.</param>
        protected internal override void HandleResult(BlogPost blogPost, ITestResults results)
        {
            if (blogPost.Categories.Length == 2)
            {
                results.AddResult("supportsMultipleCategories", YES);
            }
            else
            {
                results.AddResult("supportsMultipleCategories", NO);
            }
        }
    }
}
