// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;

    using PostHtmlEditing.ImageEditing.Decorators;

    public partial class RecentPostSynchronizer
    {
        /// <summary>
        /// The ContextImageReferenceFixer class.
        /// </summary>
        private class ContextImageReferenceFixer
        {
            /// <summary>
            /// The editing context
            /// </summary>
            private readonly IBlogPostEditingContext editingContext;

            /// <summary>
            /// Initializes a new instance of the <see cref="ContextImageReferenceFixer"/> class.
            /// </summary>
            /// <param name="editingContext">The editing context.</param>
            internal ContextImageReferenceFixer(IBlogPostEditingContext editingContext) => this.editingContext = editingContext;

            /// <summary>
            /// References the fixed callback.
            /// </summary>
            /// <param name="oldReference">The old reference.</param>
            /// <param name="newReference">The new reference.</param>
            public void ReferenceFixedCallback(string oldReference, string newReference)
            {
                var file = this.editingContext.SupportingFileService.GetFileByUri(new Uri(newReference));
                if (file == null)
                {
                    return;
                }

                foreach (BlogPostImageData imageData in this.editingContext.ImageDataList)
                {
                    // We can no longer trust the target settings for this file, so we must remove them
                    // this means the first time the object is clicked it will read the settings from the DOM
                    if (file.FileId != imageData.InlineImageFile.SupportingFile.FileId)
                    {
                        continue;
                    }

                    var settings = imageData.ImageDecoratorSettings.GetSubSettings(HtmlImageTargetDecorator.Id);
                    foreach (var settingName in HtmlImageTargetDecoratorSettings.ImageReferenceFixedStaleProperties)
                    {
                        settings.Remove(settingName);
                    }
                }
            }
        }
    }
}
