// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;

    using CoreServices;

    using HtmlParser.Parser;

    public partial class RecentPostSynchronizer
    {
        /// <summary>
        /// The ImageReferenceFixer class.
        /// </summary>
        private class ImageReferenceFixer
        {
            /// <summary>
            /// The URL fixup table
            /// </summary>
            private readonly Hashtable urlFixupTable = new Hashtable();

            /// <summary>
            /// Initializes a new instance of the <see cref="ImageReferenceFixer"/> class.
            /// </summary>
            /// <param name="list">The list.</param>
            /// <param name="blogId">The blog identifier.</param>
            internal ImageReferenceFixer(BlogPostImageDataList list, string blogId)
            {
                var uploadDestinationContext = BlogFileUploader.GetFileUploadDestinationContext(blogId);
                foreach (BlogPostImageData imageData in list)
                {
                    if (imageData.InlineImageFile == null ||
                        imageData.InlineImageFile.GetPublishedUri(uploadDestinationContext) == null)
                    {
                        continue;
                    }

                    this.urlFixupTable[imageData.InlineImageFile.GetPublishedUri(uploadDestinationContext)] =
                        imageData.InlineImageFile.Uri;
                    if (imageData.LinkedImageFile != null)
                    {
                        this.urlFixupTable[imageData.LinkedImageFile.GetPublishedUri(uploadDestinationContext)] =
                            imageData.LinkedImageFile.Uri;
                    }
                }
            }

            /// <summary>
            /// Fixes the image references.
            /// </summary>
            /// <param name="tag">The tag.</param>
            /// <param name="reference">The reference.</param>
            /// <returns>System.String.</returns>
            internal string FixImageReferences(BeginTag tag, string reference)
            {
                if (!UrlHelper.IsUrl(reference))
                {
                    //Warning: fixing of relative paths is not currently supported.  This would only be
                    //a problem for post synchronization if blog servers returned relative paths when uploading
                    //(which they can't since this interface requires a URI), or if the blog service re-wrote the
                    //URL when the content was published (which is likely to cause the URL to be unmatchable
                    //anyhow). Sharepoint images cause this path to be hit, but image synchronization is not
                    //supported for SharePoint anyhow since the URLs are always re-written by the server.
                    Debug.WriteLine("warning: relative image URLs cannot be resolved for edited posts");
                }
                else
                {
                    var fixedImageUri = (Uri) this.urlFixupTable[new Uri(reference)];
                    if (fixedImageUri != null)
                    {
                        Debug.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                                      "RecentPostSyncrhonizer: converting remote image reference [{0}] to local reference",
                                                      reference));
                        reference = fixedImageUri.ToString();
                    }
                }

                return reference;
            }
        }
    }
}
