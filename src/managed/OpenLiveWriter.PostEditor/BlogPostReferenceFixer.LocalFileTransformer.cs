// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using CoreServices;
    using HtmlParser.Parser;

    internal partial class BlogPostReferenceFixer
    {
        /// <summary>
        /// The LocalFileTransformer class.
        /// </summary>
        private class LocalFileTransformer
        {
            /// <summary>
            /// The file service
            /// </summary>
            private readonly ISupportingFileService fileService;

            /// <summary>
            /// The reference fixer
            /// </summary>
            private readonly BlogPostReferenceFixer referenceFixer;

            /// <summary>
            /// The uploader
            /// </summary>
            private readonly BlogFileUploader uploader;

            /// <summary>
            /// Initializes a new instance of the <see cref="LocalFileTransformer"/> class.
            /// </summary>
            /// <param name="referenceFixer">The reference fixer.</param>
            /// <param name="uploader">The uploader.</param>
            public LocalFileTransformer(BlogPostReferenceFixer referenceFixer, BlogFileUploader uploader)
            {
                this.referenceFixer = referenceFixer;
                this.fileService = this.referenceFixer.uploadContext.EditingContext.SupportingFileService;
                this.uploader = uploader;
            }

            /// <summary>
            /// Transforms the specified tag.
            /// </summary>
            /// <param name="tag">The tag.</param>
            /// <param name="reference">The reference.</param>
            /// <returns>System.String.</returns>
            internal string Transform(BeginTag tag, string reference)
            {
                if (!UrlHelper.IsUrl(reference))
                {
                    return reference;
                }

                var localReferenceUri = new Uri(reference);

                /*
                     * If we need to drop a hint to the photo uploader about
                     * whether Lightbox-like preview is enabled, so that we know to link to
                     * the image itself rather than the photo "self" page on photos.live.com;
                     * this is where we would figure that out (by looking at the tag) and
                     * pass that info through to the DoUploadWork call.
                     */
                var isLightboxCloneEnabled = false;

                this.referenceFixer.fileUploadWorker.DoUploadWork(reference, this.uploader,
                                                                  isLightboxCloneEnabled);

                var supportingFile = this.fileService.GetFileByUri(localReferenceUri);
                if (supportingFile == null)
                {
                    return reference;
                }

                var uploadUri = supportingFile.GetUploadInfo(this.uploader.DestinationContext).UploadUri;
                return uploadUri == null ? reference : UrlHelper.SafeToAbsoluteUri(uploadUri);
            }
        }
    }
}
