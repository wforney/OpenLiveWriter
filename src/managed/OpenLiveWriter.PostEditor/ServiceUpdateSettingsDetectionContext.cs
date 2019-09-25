// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using BlogClient;
    using BlogClient.Detection;
    using BlogClient.Providers;
    using Configuration;
    using Extensibility.BlogClient;

    /// <summary>
    /// The ServiceUpdateSettingsDetectionContext class.
    /// Implements the <see cref="IBlogSettingsDetectionContextForCategorySchemeHack" />
    /// </summary>
    /// <seealso cref="IBlogSettingsDetectionContextForCategorySchemeHack" />
    internal partial class ServiceUpdateSettingsDetectionContext : IBlogSettingsDetectionContextForCategorySchemeHack
    {
        /// <summary>
        /// The blog identifier
        /// </summary>
        private readonly string blogId;

        /// <summary>
        /// The initial blog settings contents
        /// </summary>
        private readonly string initialBlogSettingsContents;

        /// <summary>
        /// The initial categories contents
        /// </summary>
        private readonly string initialCategoriesContents;

        /// <summary>
        /// The initial category scheme
        /// </summary>
        private readonly string initialCategoryScheme;

        /// <summary>
        /// The credentials
        /// </summary>
        private readonly IBlogCredentials credentials = new TemporaryBlogCredentials();

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceUpdateSettingsDetectionContext"/> class.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        public ServiceUpdateSettingsDetectionContext(string blogId)
        {
            using (var settings = BlogSettings.ForBlogId(blogId))
            {
                this.blogId = blogId;
                this.HomepageUrl = settings.HomepageUrl;
                this.ProviderId = settings.ProviderId;
                this.ManifestDownloadInfo = settings.ManifestDownloadInfo;
                this.HostBlogId = settings.HostBlogId;
                this.PostApiUrl = settings.PostApiUrl;
                this.ClientType = settings.ClientType;
                this.UserOptionOverrides = settings.UserOptionOverrides;
                this.HomePageOverrides = settings.HomePageOverrides;

                this.initialCategoryScheme = settings.OptionOverrides?[BlogClientOptions.CATEGORY_SCHEME];

                BlogCredentialsHelper.Copy(settings.Credentials, this.credentials);

                using (var blog = new Blog(settings))
                {
                    this.BlogSupportsCategories = blog.ClientOptions.SupportsCategories;
                }

                this.initialBlogSettingsContents = this.GetBlogSettingsContents(settings);
                this.initialCategoriesContents = this.GetCategoriesContents(settings.Categories);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance has updates.
        /// </summary>
        /// <value><c>true</c> if this instance has updates; otherwise, <c>false</c>.</value>
        public bool HasUpdates
        {
            get
            {
                // If all of our manifest fields are null that means we either
                // don't support manifests or we are using a cached manifest.
                // In this case just compare categories.
                if (this.Image == null && this.WatermarkImage == null && this.ButtonDescriptions == null &&
                    this.OptionOverrides == null && this.HomePageOverrides == null)
                {
                    var updatedCategories = this.GetCategoriesContents(this.Categories);
                    return this.initialCategoriesContents != updatedCategories;
                }

                var updatedSettingsContents = this.GetSettingsContents();
                return this.initialBlogSettingsContents != updatedSettingsContents;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [blog supports categories].
        /// </summary>
        /// <value><c>true</c> if [blog supports categories]; otherwise, <c>false</c>.</value>
        public bool BlogSupportsCategories { get; }

        /// <summary>
        /// Gets the post API URL.
        /// </summary>
        /// <value>The post API URL.</value>
        public string PostApiUrl { get; }

        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        public IBlogCredentialsAccessor Credentials => new BlogCredentialsAccessor(this.blogId, this.credentials);

        /// <summary>
        /// Gets the homepage URL.
        /// </summary>
        /// <value>The homepage URL.</value>
        public string HomepageUrl { get; }

        /// <summary>
        /// Gets the provider identifier.
        /// </summary>
        /// <value>The provider identifier.</value>
        public string ProviderId { get; }

        /// <summary>
        /// Gets the host blog identifier.
        /// </summary>
        /// <value>The host blog identifier.</value>
        public string HostBlogId { get; }

        /// <summary>
        /// Gets or sets the manifest download information.
        /// </summary>
        /// <value>The manifest download information.</value>
        public WriterEditingManifestDownloadInfo ManifestDownloadInfo { get; set; }

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>The type of the client.</value>
        public string ClientType { get; set; }

        /// <summary>
        /// Gets or sets the fav icon.
        /// </summary>
        /// <value>The fav icon.</value>
        public byte[] FavIcon { get; set; }

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        public byte[] Image { get; set; }

        /// <summary>
        /// Gets or sets the watermark image.
        /// </summary>
        /// <value>The watermark image.</value>
        public byte[] WatermarkImage { get; set; }

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        public BlogPostCategory[] Categories { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>The keywords.</value>
        public BlogPostKeyword[] Keywords { get; set; }

        /// <summary>
        /// Gets or sets the button descriptions.
        /// </summary>
        /// <value>The button descriptions.</value>
        public IBlogProviderButtonDescription[] ButtonDescriptions { get; set; }

        /// <summary>
        /// Gets the user option overrides.
        /// </summary>
        /// <value>The user option overrides.</value>
        public IDictionary<string, string> UserOptionOverrides { get; }

        /// <summary>
        /// Gets or sets the option overrides.
        /// </summary>
        /// <value>The option overrides.</value>
        public IDictionary<string, string> OptionOverrides { get; set; }

        /// <summary>
        /// Gets or sets the home page overrides.
        /// </summary>
        /// <value>The home page overrides.</value>
        public IDictionary<string, string> HomePageOverrides { get; set; }

        /// <summary>
        /// Gets the initial category scheme.
        /// </summary>
        /// <value>The initial category scheme.</value>
        public string InitialCategoryScheme =>
            this.OptionOverrides != null && this.OptionOverrides.Keys.Contains(BlogClientOptions.CATEGORY_SCHEME)
                ? this.OptionOverrides[BlogClientOptions.CATEGORY_SCHEME]
                : this.initialCategoryScheme;

        /// <summary>
        /// Gets the blog settings contents.
        /// </summary>
        /// <param name="blogSettings">The blog settings.</param>
        /// <returns>System.String.</returns>
        private string GetBlogSettingsContents(IBlogSettingsDetectionContext blogSettings)
        {
            // return normalized string of settings contents
            var settingsContents = new StringBuilder();
            this.AppendClientType(blogSettings.ClientType, settingsContents);
            ServiceUpdateSettingsDetectionContext.AppendImages(blogSettings.Image, blogSettings.WatermarkImage, settingsContents);
            this.AppendCategories(blogSettings.Categories, settingsContents);
            this.AppendButtons(blogSettings.ButtonDescriptions, settingsContents);
            ServiceUpdateSettingsDetectionContext.AppendOptionOverrides(blogSettings.OptionOverrides, settingsContents);
            ServiceUpdateSettingsDetectionContext.AppendOptionOverrides(blogSettings.HomePageOverrides, settingsContents);
            return settingsContents.ToString();
        }

        /// <summary>
        /// Gets the settings contents.
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetSettingsContents()
        {
            var settingsContents = new StringBuilder();
            this.AppendClientType(this.ClientType, settingsContents);
            ServiceUpdateSettingsDetectionContext.AppendImages(this.Image, this.WatermarkImage, settingsContents);
            this.AppendCategories(this.Categories, settingsContents);
            this.AppendButtons(this.ButtonDescriptions, settingsContents);
            ServiceUpdateSettingsDetectionContext.AppendOptionOverrides(this.OptionOverrides, settingsContents);
            ServiceUpdateSettingsDetectionContext.AppendOptionOverrides(this.HomePageOverrides, settingsContents);
            return settingsContents.ToString();
        }

        /// <summary>
        /// Appends the manifest download information.
        /// </summary>
        /// <param name="manifestDownloadInfo">The manifest download information.</param>
        /// <param name="settingsContents">The settings contents.</param>
        private void AppendManifestDownloadInfo(WriterEditingManifestDownloadInfo manifestDownloadInfo,
                                                StringBuilder settingsContents)
        {
            if (manifestDownloadInfo != null)
            {
                settingsContents.AppendFormat(CultureInfo.InvariantCulture,
                                              "ManifestUrl:{0}ManifestExpires:{1}ManifestLastModified:{2}ManifestEtag:{3}",
                                              manifestDownloadInfo.SourceUrl,
                                              manifestDownloadInfo.Expires,
                                              manifestDownloadInfo.LastModified,
                                              manifestDownloadInfo.ETag);
            }
        }

        /// <summary>
        /// Appends the type of the client.
        /// </summary>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="settingsContents">The settings contents.</param>
        private void AppendClientType(string clientType, StringBuilder settingsContents)
        {
            if (clientType != null)
            {
                settingsContents.AppendFormat("ClientType:{0}", clientType);
            }
        }

        /// <summary>
        /// Appends the images.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="watermarkImage">The watermark image.</param>
        /// <param name="settingsContents">The settings contents.</param>
        private static void AppendImages(IReadOnlyCollection<byte> image, IReadOnlyCollection<byte> watermarkImage, StringBuilder settingsContents)
        {
            if (image != null && image.Count > 0)
            {
                settingsContents.Append("Image:");
                foreach (var imageByte in image)
                {
                    settingsContents.Append(imageByte);
                }
            }

            if (watermarkImage == null || watermarkImage.Count <= 0)
            {
                return;
            }

            settingsContents.Append("WatemarkImage:");
            foreach (var watermarkImageByte in watermarkImage)
            {
                settingsContents.Append(watermarkImageByte);
            }
        }

        /// <summary>
        /// Appends the categories.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <param name="settingsContents">The settings contents.</param>
        private void AppendCategories(BlogPostCategory[] categories, StringBuilder settingsContents) =>
            settingsContents.Append(this.GetCategoriesContents(categories));

        /// <summary>
        /// Gets the categories contents.
        /// </summary>
        /// <param name="categories">The categories.</param>
        /// <returns>System.String.</returns>
        private string GetCategoriesContents(BlogPostCategory[] categories)
        {
            var categoriesBuilder = new StringBuilder();
            if (categories == null)
            {
                return categoriesBuilder.ToString();
            }

            Array.Sort(categories, new SortCategoriesComparer());
            foreach (var category in categories)
            {
                categoriesBuilder.AppendFormat("Category:{0}/{1}/{2}", category.Id, category.Name, category.Parent);
            }

            return categoriesBuilder.ToString();
        }

        /// <summary>
        /// Appends the buttons.
        /// </summary>
        /// <param name="buttons">The buttons.</param>
        /// <param name="settingsContents">The settings contents.</param>
        private void AppendButtons(IBlogProviderButtonDescription[] buttons, StringBuilder settingsContents)
        {
            if (buttons == null)
            {
                return;
            }

            Array.Sort(buttons, new SortButtonsComparer());
            foreach (var button in buttons)
            {
                settingsContents.AppendFormat(CultureInfo.InvariantCulture, "Button:{0}/{1}/{2}/{3}/{4}/{5}/{6}",
                                              button.Id, button.Description, button.ImageUrl, button.ClickUrl,
                                              button.ContentUrl, button.ContentDisplaySize, button.NotificationUrl);
            }
        }

        /// <summary>
        /// Appends the option overrides.
        /// </summary>
        /// <param name="optionOverrides">The option overrides.</param>
        /// <param name="settingsContents">The settings contents.</param>
        private static void AppendOptionOverrides(IDictionary<string, string> optionOverrides, StringBuilder settingsContents)
        {
            if (optionOverrides == null)
            {
                return;
            }

            var optionOverrideList = optionOverrides.ToList();

            optionOverrideList.Sort(new SortOptionOverridesComparer<string, string>());

            foreach (var optionOverride in optionOverrideList)
            {
                settingsContents.AppendFormat("OptionOverride:{0}/{1}", optionOverride.Key, optionOverride.Value);
            }
        }
    }
}
