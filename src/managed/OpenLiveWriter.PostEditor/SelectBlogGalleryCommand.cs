// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove obsolete code.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Drawing;
    using BlogClient;
    using Commands;
    using CoreServices;
    using Interop.Com.Ribbon;
    using Localization;

    /// <summary>
    /// The SelectBlogGalleryCommand class.
    /// Implements the <see cref="OpenLiveWriter.PostEditor.Commands.SelectGalleryCommand{System.String}" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.PostEditor.Commands.SelectGalleryCommand{System.String}" />
    public class SelectBlogGalleryCommand : SelectGalleryCommand<string>
    {
        /// <summary>
        /// The editing manager
        /// </summary>
        private readonly IBlogPostEditingManager editingManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectBlogGalleryCommand"/> class.
        /// </summary>
        /// <param name="editingManager">The editing manager.</param>
        public SelectBlogGalleryCommand(IBlogPostEditingManager editingManager) : base(CommandId.SelectBlog)
        {
            this._invalidateGalleryRepresentation = true;
            this.editingManager = editingManager;
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        public override void LoadItems()
        {
            this.items.Clear();

            var i = 0;
            var currentBlogId = this.editingManager.CurrentBlog();
            foreach (var blogDescriptor in BlogSettings.GetBlogs(true))
            {
                using (var blog = new Blog(blogDescriptor.Id))
                {
                    var blogName = SelectBlogGalleryCommand.GetShortenedBlogName(blog.Name);

                    if (blog.Image == null)
                    {
                        var defaultIcon = ResourceHelper.LoadAssemblyResourceBitmap("OpenPost.Images.BlogAccount.png");
                        this.items.Add(new GalleryItem(blogName, defaultIcon.Clone() as Bitmap, blog.Id));
                    }
                    else
                    {
                        this.items.Add(new GalleryItem(blogName, new Bitmap(blog.Image), blog.Id));
                    }

                    if (currentBlogId == blog.Id)
                    {
                        this.selectedIndex = i;
                    }
                }

                i++;
            }

            base.LoadItems();
        }

        /// <summary>
        /// Invalidates this instance.
        /// </summary>
        public override void Invalidate()
        {
            if (this.items.Count == 0)
            {
                this.LoadItems();
                this.UpdateInvalidationState(PropertyKeys.ItemsSource, InvalidationState.Pending);
                this.UpdateInvalidationState(PropertyKeys.Categories, InvalidationState.Pending);
                this.UpdateInvalidationState(PropertyKeys.SelectedItem, InvalidationState.Pending);
                this.UpdateInvalidationState(PropertyKeys.StringValue, InvalidationState.Pending);
                this.UpdateInvalidationState(PropertyKeys.Label, InvalidationState.Pending);
                this.OnStateChanged(EventArgs.Empty);
            }
            else
            {
                this.UpdateSelectedIndex();
            }
        }

        /// <summary>
        /// Reloads the and invalidate.
        /// </summary>
        internal void ReloadAndInvalidate()
        {
            this.items.Clear();
            this.Invalidate();
        }

        /// <summary>
        /// Updates the index of the selected.
        /// </summary>
        private void UpdateSelectedIndex()
        {
            this.selectedIndex = GalleryCommand<string>.INVALID_INDEX;
            this.SetSelectedItem(this.editingManager.CurrentBlog());
        }

        /// <summary>
        /// Gets the name of the shortened blog.
        /// </summary>
        /// <param name="blogName">Name of the blog.</param>
        /// <returns>System.String.</returns>
        public static string GetShortenedBlogName(string blogName) =>
            TextHelper.GetTitleFromText(blogName, 100, TextHelper.Units.Characters);
    }
}
