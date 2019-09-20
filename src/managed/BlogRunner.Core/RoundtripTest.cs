// <copyright file="RoundtripTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;
    using BlogRunner.Core.Config;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class RoundtripTest.
    /// Implements the <see cref="BlogRunner.Core.Test" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.Test" />
    public abstract class RoundtripTest : Test
    {
        /// <summary>
        /// Does the test.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="results">The results.</param>
        public sealed override void DoTest(Blog blog, IBlogClient blogClient, ITestResults results)
        {
            if (blog == null)
            {
                throw new ArgumentNullException(nameof(blog));
            }

            if (blogClient == null)
            {
                throw new ArgumentNullException(nameof(blogClient));
            }

            var blogPost = new BlogPost();
            bool? publish = null;
            this.PreparePost(blog, blogClient, blogPost, ref publish);

            var token = BlogUtil.ShortGuid;
            blogPost.Title = $"{token}:{blogPost.Title}";

            var postId = blogClient.NewPost(blog.BlogId, blogPost, null, publish ?? true, out _, out _);
            var newPost = blogClient.GetPost(blog.BlogId, postId);
            this.HandleResult(newPost, results);
            if (postId != null && CleanUpPosts)
            {
                blogClient.DeletePost(blog.BlogId, postId, false);
            }
        }

        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected internal abstract void PreparePost(Blog blog, IBlogClient blogClient, BlogPost blogPost, ref bool? publish);

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="results">The results.</param>
        protected internal abstract void HandleResult(BlogPost blogPost, ITestResults results);
    }
}
