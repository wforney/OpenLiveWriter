// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove obsolete code.

namespace OpenLiveWriter.PostEditor
{
    using System.Drawing;
    using ApplicationFramework;
    using BlogClient;
    using CoreServices;

    internal partial class WeblogCommandManager
    {
        /// <summary>
        /// The SwitchWeblogMenuCommand class.
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IMenuCommandObject" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.IMenuCommandObject" />
        private class SwitchWeblogMenuCommand : IMenuCommandObject
        {

            /// <summary>
            /// The caption
            /// </summary>
            private readonly string caption;

            /// <summary>
            /// The latched
            /// </summary>
            private readonly bool latched;

            /// <summary>
            /// Initializes a new instance of the <see cref="SwitchWeblogMenuCommand"/> class.
            /// </summary>
            /// <param name="blogId">The blog identifier.</param>
            /// <param name="latched">if set to <c>true</c> [latched].</param>
            public SwitchWeblogMenuCommand(string blogId, bool latched)
            {
                this.BlogId = blogId;
                using (var settings = BlogSettings.ForBlogId(this.BlogId))
                {
                    this.caption = StringHelper.Ellipsis($"{settings.BlogName}", 65);
                }

                this.latched = latched;
            }

            /// <summary>
            /// Gets the blog identifier.
            /// </summary>
            /// <value>The blog identifier.</value>
            public string BlogId { get; }

            /// <inheritdoc />
            Bitmap IMenuCommandObject.Image => null;

            /// <inheritdoc />
            string IMenuCommandObject.Caption => this.caption;

            /// <inheritdoc />
            string IMenuCommandObject.CaptionNoMnemonic => this.caption;

            /// <inheritdoc />
            bool IMenuCommandObject.Latched => this.latched;

            /// <inheritdoc />
            bool IMenuCommandObject.Enabled => true;
        }
    }
}
