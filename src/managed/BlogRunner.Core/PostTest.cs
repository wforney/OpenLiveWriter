// <copyright file="PostTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;
    using System.Text;
    using System.Threading;
    using BlogRunner.Core.Config;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class PostTest.
    /// Implements the <see cref="BlogRunner.Core.Test" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.Test" />
    public abstract class PostTest : Test
    {
        /// <summary>
        /// Gets the duration of the timeout.
        /// </summary>
        /// <value>The duration of the timeout.</value>
        protected virtual TimeSpan TimeoutDuration => TimeSpan.FromMinutes(2.0);

        /// <summary>
        /// Does the test.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="results">The results.</param>
        public sealed override void DoTest(Blog blog, IBlogClient blogClient, ITestResults results)
        {
            var blogPost = new BlogPost();
            bool? publish = null;
            this.PreparePost(blogPost, ref publish);

            var token = BlogUtil.ShortGuid;
            blogPost.Title = token + ":" + blogPost.Title;

            try
            {
                RetryUntilTimeout(
                    this.TimeoutDuration,
                    () =>
                    {
                        using (var response = HttpRequestHelper.SendRequest(blog.HomepageUrl))
                        {
                            using (var stream = response.GetResponseStream())
                            {
                                var html = Encoding.ASCII.GetString(StreamHelper.AsBytes(stream));

                                if (html.Contains(token))
                                {
                                    this.HandleResult(html, results);
                                    return true;
                                }

                                Thread.Sleep(1000);
                                return false;
                            }
                        }
                    });
            }
            catch (TimeoutException te)
            {
                if (!this.HandleTimeout(te, results))
                {
                    throw;
                }
            }

            var postId = blogClient.NewPost(blog.BlogId, blogPost, null, publish ?? true, out var etag, out var remotePost);
            if (postId != null && CleanUpPosts)
            {
                blogClient.DeletePost(blog.BlogId, postId, false);
            }
        }

        /// <summary>
        /// Prepares the post.
        /// </summary>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected internal abstract void PreparePost(BlogPost blogPost, ref bool? publish);

        /// <summary>
        /// Handles the result.
        /// </summary>
        /// <param name="homepageHtml">The homepage HTML.</param>
        /// <param name="results">The results.</param>
        protected internal abstract void HandleResult(string homepageHtml, ITestResults results);

        /// <summary>
        /// Return true if the timeout condition was handled. False means
        /// the caller should deal with the timeout (by throwing TimeoutException).
        /// </summary>
        /// <param name="te">The timeout exception.</param>
        /// <param name="results">The test results.</param>
        /// <returns>
        /// True if the timeout condition was handled. False means
        /// the caller should deal with the timeout (by throwing TimeoutException).
        /// </returns>
        protected internal virtual bool HandleTimeout(TimeoutException te, ITestResults results) => false;
    }
}
