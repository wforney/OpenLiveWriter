// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using BlogClient;
    using CoreServices.HTML;

    public partial class UpdateWeblogAsyncOperation
    {
        /// <summary>
        /// Class which manages resolution of local file references by uploading
        /// the files to the publishing target, modifying the post contents to
        /// reflect the appropriate target URLs, and finally restoring the post
        /// contents to point at the local files for additional local editing).
        /// Implements the <see cref="System.IDisposable" />
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        private class LocalSupportingFileUploader : IDisposable
        {
            /// <summary>
            /// The blog
            /// </summary>
            private readonly Blog blog;

            /// <summary>
            /// The original post contents
            /// </summary>
            private readonly string originalPostContents;

            /// <summary>
            /// The publishing context
            /// </summary>
            private readonly IBlogPostPublishingContext publishingContext;

            /// <summary>
            /// The reference fixer
            /// </summary>
            private BlogPostReferenceFixer referenceFixer;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalSupportingFileUploader"/> class.
            /// </summary>
            /// <param name="publishingContext">The publishing context.</param>
            public LocalSupportingFileUploader(IBlogPostPublishingContext publishingContext)
            {
                // save references to parameters/post contents
                this.publishingContext = publishingContext;
                this.originalPostContents = this.publishingContext.EditingContext.BlogPost.Contents;
                this.blog = new Blog(this.publishingContext.EditingContext.BlogId);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                try
                {
                    this.blog.Dispose();

                    // restore the contents of the BlogPost
                    this.publishingContext.EditingContext.BlogPost.Contents = this.originalPostContents;
                }
                catch (Exception ex)
                {
                    Trace.Fail($"Unexpected exception occurred during Dispose of UpdateWeblogAsyncOperation:{ex}");
                }

                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Uploads the files before publish.
            /// </summary>
            public void UploadFilesBeforePublish()
            {
                // create a file uploader
                using (var fileUploader =
                    BlogFileUploader.CreateFileUploader(
                        this.blog, this.publishingContext.EditingContext.ServerSupportingFileDirectory))
                {
                    // connect to the file uploader
                    fileUploader.Connect();

                    // upload the files and fixup references within the contents of the blog post
                    var htmlContents = this.publishingContext.EditingContext.BlogPost.Contents;

                    this.referenceFixer = new BlogPostReferenceFixer(htmlContents, this.publishingContext);
                    this.referenceFixer.Parse();

                    var fixedHtml =
                        HtmlReferenceFixer.FixLocalFileReferences(htmlContents,
                                                                  this.referenceFixer.GetFileUploadReferenceFixer(
                                                                      fileUploader));
                    this.publishingContext.EditingContext.BlogPost.Contents = fixedHtml;
                }
            }

            /// <summary>
            /// Uploads the files after publish.
            /// </summary>
            /// <param name="postId">The post identifier.</param>
            public void UploadFilesAfterPublish(string postId)
            {
                // create a file uploader
                using (var fileUploader =
                    BlogFileUploader.CreateFileUploader(
                        this.blog, this.publishingContext.EditingContext.ServerSupportingFileDirectory))
                {
                    // connect to the file uploader
                    fileUploader.Connect();

                    this.referenceFixer.UploadFilesAfterPublish(postId, fileUploader);
                }
            }

            /// <summary>
            /// Finalizes an instance of the <see cref="LocalSupportingFileUploader"/> class.
            /// </summary>
            ~LocalSupportingFileUploader()
            {
                Debug.Fail("Failed to dispose LocalSupportingFileUploader");
            }
        }
    }
}
