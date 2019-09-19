// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Collections;
using System.Diagnostics;
using Microsoft.Win32;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Progress;
using OpenLiveWriter.BlogClient;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices.Threading;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Interop.Windows;
using OpenLiveWriter.PostEditor.Configuration;
using System.Security.AccessControl;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;

namespace OpenLiveWriter.PostEditor
{

    internal class ServiceUpdateChecker
    {
        public ServiceUpdateChecker(string blogId, WeblogSettingsChangedHandler settingsChangedHandler)
        {
            this._blogId = blogId;
            this._settingsChangedHandler = settingsChangedHandler;
        }

        public void Start()
        {
            var checkerThread = ThreadHelper.NewThread(new ThreadStart(this.Main), "ServiceUpdateChecker", true, true, true);
            checkerThread.Start();
        }

        private void Main()
        {
            try
            {
                // delay the check for updates
                Thread.Sleep(1000);

                // only run one service-update at a time process wide
                lock (_serviceUpdateLock)
                {
                    // establish settings detection context
                    var settingsDetectionContext = new ServiceUpdateSettingsDetectionContext(this._blogId);

                    // fire-up a blog settings detector to query for changes
                    var settingsDetector = new BlogSettingsDetector(settingsDetectionContext);
                    settingsDetector.SilentMode = true;
                    using (var key = Registry.CurrentUser.OpenSubKey(ApplicationEnvironment.SettingsRootKeyName + @"\Weblogs\" + this._blogId + @"\HomepageOptions"))
                    {
                        if (key != null)
                        {
                            settingsDetector.IncludeFavIcon = false;
                            settingsDetector.IncludeCategories = settingsDetectionContext.BlogSupportsCategories;
                            settingsDetector.UseManifestCache = true;
                            settingsDetector.IncludeHomePageSettings = false;
                            settingsDetector.IncludeCategoryScheme = false;
                            settingsDetector.IncludeInsecureOperations = false;
                        }
                    }
                    settingsDetector.IncludeImageEndpoints = false;
                    settingsDetector.DetectSettings(SilentProgressHost.Instance);

                    // write the settings
                    using (ProcessKeepalive.Open())
                    {
                        using (var settings = BlogSettings.ForBlogId(this._blogId))
                        {
                            settings.ApplyUpdates(settingsDetectionContext);
                        }
                    }

                    // if changes were made then fire an event to notify the UI
                    if (settingsDetectionContext.HasUpdates)
                    {
                        this._settingsChangedHandler(this._blogId, false);
                    }
                }

            }
            catch (ManualKeepaliveOperationException)
            {
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception during ServiceUpdateChecker.Main: " + ex.ToString());
            }
        }

        private string _blogId;
        private WeblogSettingsChangedHandler _settingsChangedHandler;
        private readonly static object _serviceUpdateLock = new object();
    }

    internal class ServiceUpdateSettingsDetectionContext : IBlogSettingsDetectionContextForCategorySchemeHack
    {
        public ServiceUpdateSettingsDetectionContext(string blogId)
        {
            using (var settings = BlogSettings.ForBlogId(blogId))
            {
                this._blogId = blogId;
                this.HomepageUrl = settings.HomepageUrl;
                this.ProviderId = settings.ProviderId;
                this.ManifestDownloadInfo = settings.ManifestDownloadInfo;
                this.HostBlogId = settings.HostBlogId;
                this.PostApiUrl = settings.PostApiUrl;
                this.ClientType = settings.ClientType;
                this.UserOptionOverrides = settings.UserOptionOverrides;
                this.HomePageOverrides = settings.HomePageOverrides;

                this._initialCategoryScheme = settings.OptionOverrides != null
                    ? settings.OptionOverrides[BlogClientOptions.CATEGORY_SCHEME] as string
                    : null;

                BlogCredentialsHelper.Copy(settings.Credentials, this._credentials);

                using (var blog = new Blog(settings))
                {
                    this.BlogSupportsCategories = blog.ClientOptions.SupportsCategories;
                }

                this._initialBlogSettingsContents = this.GetBlogSettingsContents(settings);
                this._initialCategoriesContents = this.GetCategoriesContents(settings.Categories);

            }
        }

        public bool HasUpdates
        {
            get
            {
                // If all of our manifest fields are null that means we either
                // don't support manifests or we are using a cached manifest.
                // In this case just compare categories.
                if (this.Image == null && this.WatermarkImage == null && this.ButtonDescriptions == null && this.OptionOverrides == null && this.HomePageOverrides == null)
                {
                    var updatedCategories = this.GetCategoriesContents(this.Categories);
                    return this._initialCategoriesContents != updatedCategories;
                }
                else
                {
                    var updatedSettingsContents = this.GetSettingsContents();
                    return this._initialBlogSettingsContents != updatedSettingsContents;
                }
            }
        }

        public bool BlogSupportsCategories { get; }

        public string PostApiUrl { get; }

        public IBlogCredentialsAccessor Credentials => new BlogCredentialsAccessor(this._blogId, this._credentials);
        private IBlogCredentials _credentials = new TemporaryBlogCredentials();

        public string HomepageUrl { get; }

        public string ProviderId { get; }

        public string HostBlogId { get; }

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo { get; set; }

        public string ClientType { get; set; }

        public byte[] FavIcon { get; set; }

        public byte[] Image { get; set; }

        public byte[] WatermarkImage { get; set; }

        public BlogPostCategory[] Categories { get; set; }

        public BlogPostKeyword[] Keywords { get; set; }

        public IBlogProviderButtonDescription[] ButtonDescriptions { get; set; }

        public IDictionary<string, string> UserOptionOverrides { get; }

        public IDictionary<string, string> OptionOverrides { get; set; }

        public IDictionary<string, string> HomePageOverrides { get; set; }

        public string InitialCategoryScheme =>
            this.OptionOverrides != null && this.OptionOverrides.Keys.Contains(BlogClientOptions.CATEGORY_SCHEME)
                ? this.OptionOverrides[BlogClientOptions.CATEGORY_SCHEME] as string
                : this._initialCategoryScheme;

        private readonly string _blogId;

        private readonly string _initialBlogSettingsContents;
        private readonly string _initialCategoriesContents;
        private readonly string _initialCategoryScheme;

        private string GetBlogSettingsContents(BlogSettings blogSettings)
        {
            // return normalized string of settings contents
            var settingsContents = new StringBuilder();
            this.AppendClientType(blogSettings.ClientType, settingsContents);
            this.AppendImages(blogSettings.Image, blogSettings.WatermarkImage, settingsContents);
            this.AppendCategories(blogSettings.Categories, settingsContents);
            this.AppendButtons(blogSettings.ButtonDescriptions, settingsContents);
            AppendOptionOverrides(blogSettings.OptionOverrides, settingsContents);
            AppendOptionOverrides(blogSettings.HomePageOverrides, settingsContents);
            return settingsContents.ToString();
        }

        private string GetSettingsContents()
        {
            var settingsContents = new StringBuilder();
            this.AppendClientType(this.ClientType, settingsContents);
            this.AppendImages(this.Image, this.WatermarkImage, settingsContents);
            this.AppendCategories(this.Categories, settingsContents);
            this.AppendButtons(this.ButtonDescriptions, settingsContents);
            AppendOptionOverrides(this.OptionOverrides, settingsContents);
            AppendOptionOverrides(this.HomePageOverrides, settingsContents);
            return settingsContents.ToString();
        }

        private void AppendManifestDownloadInfo(WriterEditingManifestDownloadInfo manifestDownloadInfo, StringBuilder settingsContents)
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

        private void AppendClientType(string clientType, StringBuilder settingsContents)
        {
            if (clientType != null)
            {
                settingsContents.AppendFormat("ClientType:{0}", clientType);
            }
        }

        private void AppendImages(byte[] image, byte[] watermarkImage, StringBuilder settingsContents)
        {
            if (image != null && image.Length > 0)
            {
                settingsContents.Append("Image:");
                foreach (var imageByte in image)
                {
                    settingsContents.Append(imageByte);
                }
            }

            if (watermarkImage != null && watermarkImage.Length > 0)
            {
                settingsContents.Append("WatemarkImage:");
                foreach (var watermarkImageByte in watermarkImage)
                {
                    settingsContents.Append(watermarkImageByte);
                }
            }
        }

        private void AppendCategories(BlogPostCategory[] categories, StringBuilder settingsContents) =>
            settingsContents.Append(this.GetCategoriesContents(categories));

        private string GetCategoriesContents(BlogPostCategory[] categories)
        {
            var categoriesBuilder = new StringBuilder();
            if (categories != null)
            {
                Array.Sort(categories, new SortCategoriesComparer());
                foreach (var category in categories)
                {
                    categoriesBuilder.AppendFormat("Category:{0}/{1}/{2}", category.Id, category.Name, category.Parent);
                }
            }
            return categoriesBuilder.ToString();
        }

        private class SortCategoriesComparer : IComparer
        {
            public int Compare(object x, object y) => (x as BlogPostCategory).Id.CompareTo((y as BlogPostCategory).Id);
        }

        private void AppendButtons(IBlogProviderButtonDescription[] buttons, StringBuilder settingsContents)
        {
            if (buttons != null)
            {
                Array.Sort(buttons, new SortButtonsComparer());
                foreach (var button in buttons)
                {
                    settingsContents.AppendFormat(CultureInfo.InvariantCulture, "Button:{0}/{1}/{2}/{3}/{4}/{5}/{6}", button.Id, button.Description, button.ImageUrl, button.ClickUrl, button.ContentUrl, button.ContentDisplaySize, button.NotificationUrl);
                }
            }
        }

        private class SortButtonsComparer : IComparer
        {
            public int Compare(object x, object y) => (x as IBlogProviderButtonDescription).Id.CompareTo((y as IBlogProviderButtonDescription).Id);
        }

        private void AppendOptionOverrides(IDictionary<string, string> optionOverrides, StringBuilder settingsContents)
        {
            if (optionOverrides != null)
            {
                var optionOverrideList = new List<KeyValuePair<string, string>>();
                foreach (var optionOverride in optionOverrides)
                {
                    optionOverrideList.Add(optionOverride);
                }

                optionOverrideList.Sort(new SortOptionOverridesComparer<string, string>());

                foreach (var optionOverride in optionOverrideList)
                {
                    settingsContents.AppendFormat("OptionOverride:{0}/{1}", optionOverride.Key, optionOverride.Value);
                }
            }
        }

        private class SortOptionOverridesComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                var xKey = ((DictionaryEntry)x).Key.ToString();
                var yKey = ((DictionaryEntry)y).Key.ToString();
                return xKey.CompareTo(yKey);
            }
        }

        private class SortOptionOverridesComparer<TKey, TValue> : IComparer<KeyValuePair<TKey, TValue>>
        {
            public int Compare(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => x.Key.ToString().CompareTo(y.Key.ToString());
        }
    }
}
