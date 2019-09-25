// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using BlogClient;
    using CoreServices;
    using CoreServices.Threading;
    using Extensibility.BlogClient;
    using Localization;

    /// <summary>
    /// The UpdateWeblogAsyncOperation class.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.AsyncOperation" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.AsyncOperation" />
    public partial class UpdateWeblogAsyncOperation : AsyncOperation
    {
        /// <summary>
        /// The publish
        /// </summary>
        private readonly bool publish;

        /// <summary>
        /// The publishing context
        /// </summary>
        private readonly IBlogPostPublishingContext publishingContext;

        /// <summary>
        /// The UI context
        /// </summary>
        private readonly IBlogClientUIContext uiContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWeblogAsyncOperation"/> class.
        /// </summary>
        /// <param name="uiContext">The UI context.</param>
        /// <param name="publishingContext">The publishing context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        public UpdateWeblogAsyncOperation(IBlogClientUIContext uiContext, IBlogPostPublishingContext publishingContext,
                                          bool publish)
            : base(uiContext)
        {
            this.uiContext = uiContext;
            this.publishingContext = publishingContext;
            this.publish = publish;
        }

        /// <inheritdoc />
        protected override void DoWork()
        {
            using (new BlogClientUIContextScope(this.uiContext))
            {
                // NOTE: LocalSupportingFileUploader temporarily modifies the contents of the BlogPost.Contents to
                // have the correct remote references to embedded images, etc. When it is disposed it returns the
                // value of BlogPost.Contents to its original value.
                using (var supportingFileUploader = new LocalSupportingFileUploader(this.publishingContext))
                {
                    //hook to publish files before the post is published
                    supportingFileUploader.UploadFilesBeforePublish();

                    // now submit the post
                    using (var blog = new Blog(this.publishingContext.EditingContext.BlogId))
                    {
                        var blogPost = this.publishingContext.GetBlogPostForPublishing();

                        var postResult = blogPost.IsNew
                                             ? blog.NewPost(blogPost, this.publishingContext, this.publish)
                                             : blog.EditPost(blogPost, this.publishingContext, this.publish);

                        // try to get published post hash and permalink (but failure shouldn't
                        // stop publishing -- if we allow this then the user will end up
                        // "double-posting" content because the actual publish did succeed
                        // whereas Writer's status would indicate it hadn't)
                        string publishedPostHash = null, permaLink = null, slug = null;
                        try
                        {
                            var publishedPost = blog.GetPost(postResult.PostId, blogPost.IsPage);
                            publishedPostHash = BlogPost.CalculateContentsSignature(publishedPost);
                            permaLink = publishedPost.Permalink;
                            slug = publishedPost.Slug;
                        }
                        catch (Exception ex)
                        {
                            Trace.WriteLine($"Unexpected error retrieving published post: {ex}");
                        }

                        var publishingResult = new BlogPostPublishingResult();

                        // Hook to publish files after the post is published (note that if this
                        // fails it is not a fatal error since the publish itself already
                        // succeeded. In the case of a failure note the exception so that the
                        // UI layer can inform/prompt the user as appropriate
                        try
                        {
                            supportingFileUploader.UploadFilesAfterPublish(postResult.PostId);
                        }
                        catch (Exception ex)
                        {
                            publishingResult.AfterPublishFileUploadException = ex;
                        }

                        // populate the publishing result
                        publishingResult.PostResult = postResult;
                        publishingResult.PostPermalink = permaLink;
                        publishingResult.PostContentHash = publishedPostHash;
                        publishingResult.Slug = slug;
                        publishingResult.PostPublished = this.publish;

                        // set the post result
                        this.publishingContext.SetPublishingPostResult(publishingResult);

                        // send pings if appropriate
                        if (this.publish && PostEditorSettings.Ping && !blogPost.IsTemporary)
                        {
                            this.SafeAsyncSendPings(blog.Name, blog.HomepageUrl);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Safes the asynchronous send pings.
        /// </summary>
        /// <param name="blogName">Name of the blog.</param>
        /// <param name="blogHomepageUrl">The blog homepage URL.</param>
        private void SafeAsyncSendPings(string blogName, string blogHomepageUrl)
        {
            try
            {
                var pingUrls = PostEditorSettings.PingUrls;

                if (pingUrls.Length <= 0)
                {
                    return;
                }

                var ph = new PingHelper(blogName, blogHomepageUrl, pingUrls);
                var t = ThreadHelper.NewThread(ph.ThreadStart, "AsyncPing", true, false, false);
                t.Start();

                this.UpdateProgress(
                    -1, -1, Res.Get(pingUrls.Length == 1 ? StringId.SendingPing : StringId.SendingPings));
                Thread.Sleep(750);
            }
            catch (Exception e)
            {
                Trace.Fail($"Exception while running ping logic: {e}");
            }
        }
    }
}
