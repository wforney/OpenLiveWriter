// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;

    using BlogClient;

    using CoreServices;

    using Extensibility.BlogClient;

    public partial class RecentPostSynchronizer
    {
        /// <summary>
        /// The GetRecentPostOperation class.
        /// Implements the <see cref="OpenLiveWriter.CoreServices.AsyncOperation" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.CoreServices.AsyncOperation" />
        private class GetRecentPostOperation : AsyncOperation
        {
            /// <summary>
            /// The blog identifier
            /// </summary>
            private readonly string blogId;

            /// <summary>
            /// The blog post
            /// </summary>
            private readonly BlogPost blogPost;

            /// <summary>
            /// The UI context
            /// </summary>
            private readonly IBlogClientUIContext uiContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="GetRecentPostOperation"/> class.
            /// </summary>
            /// <param name="uiContext">The UI context.</param>
            /// <param name="blogId">The blog identifier.</param>
            /// <param name="blogPost">The blog post.</param>
            public GetRecentPostOperation(IBlogClientUIContext uiContext, string blogId, BlogPost blogPost)
                : base(uiContext)
            {
                this.uiContext = uiContext;
                this.blogId = blogId;
                this.blogPost = blogPost;
            }

            /// <summary>
            /// Gets the server blog post.
            /// </summary>
            /// <value>The server blog post.</value>
            public BlogPost ServerBlogPost { get; private set; }

            /// <summary>
            /// Was a post available on the remote server? Distinguishes between the case
            /// where BlogPost is null due to an error vs. null because we couldn't get
            /// a remote copy of the post
            /// </summary>
            /// <value><c>true</c> if [no post available]; otherwise, <c>false</c>.</value>
            public bool NoPostAvailable { get; private set; }

            /// <summary>
            /// Gets a value indicating whether [was cancelled].
            /// </summary>
            /// <value><c>true</c> if [was cancelled]; otherwise, <c>false</c>.</value>
            public bool WasCancelled => this.CancelRequested;

            /// <inheritdoc />
            protected override void DoWork()
            {
                using (var uiScope = new BlogClientUIContextScope(this.uiContext))
                {
                    using (var blog = new Blog(this.blogId))
                    {
                        // Fix bug 457160 - New post created with a new category
                        // becomes without a category when opened in WLW
                        //
                        // See also PostEditorPostSource.GetPost(string)
                        try
                        {
                            blog.RefreshCategories();
                        }
                        catch (Exception e)
                        {
                            Trace.Fail($"Exception while attempting to refresh categories: {e}");
                        }

                        this.ServerBlogPost = blog.GetPost(this.blogPost.Id, this.blogPost.IsPage);
                        if (this.ServerBlogPost == null)
                        {
                            this.NoPostAvailable = true;
                        }
                    }
                }
            }
        }
    }
}
