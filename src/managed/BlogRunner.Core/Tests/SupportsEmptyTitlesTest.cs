// <copyright file="SupportsEmptyTitlesTest.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Tests
{
    using System;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Class SupportsEmptyTitlesTest.
    /// Implements the <see cref="BlogRunner.Core.Test" />.
    /// </summary>
    /// <seealso cref="BlogRunner.Core.Test" />
    public class SupportsEmptyTitlesTest : Test
    {
        /// <summary>
        /// Does the test.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="results">The results.</param>
        public override void DoTest(Config.Blog blog, IBlogClient blogClient, ITestResults results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            var post = new BlogPost
            {
                Contents = "foo",
                Title = string.Empty,
            };

            try
            {
                if (blog == null)
                {
                    throw new ArgumentNullException(nameof(blog));
                }

                if (blogClient == null)
                {
                    throw new ArgumentNullException(nameof(blogClient));
                }

                var newPostId = blogClient.NewPost(blog.BlogId, post, null, true, out var etag, out var remotePost);
                results.AddResult("supportsEmptyTitles", YES);

                if (CleanUpPosts)
                {
                    blogClient.DeletePost(blog.BlogId, newPostId, true);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                results.AddResult("supportsEmptyTitles", NO);
            }
        }
    }
}
