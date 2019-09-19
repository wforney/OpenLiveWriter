// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Collections;
    using System.Diagnostics;
    using OpenLiveWriter.BlogClient.Clients;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Settings;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.BlogClient.Detection;
    using System.Linq;

    /// <summary>
    /// Class BlogSettings.
    /// Implements the <see cref="OpenLiveWriter.BlogClient.IBlogSettingsAccessor" />
    /// Implements the <see cref="OpenLiveWriter.BlogClient.Detection.IBlogSettingsDetectionContext" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.BlogClient.IBlogSettingsAccessor" />
    /// <seealso cref="OpenLiveWriter.BlogClient.Detection.IBlogSettingsDetectionContext" />
    /// <seealso cref="System.IDisposable" />
    public class BlogSettings : IBlogSettingsAccessor, IBlogSettingsDetectionContext, IDisposable
    {
        /// <summary>
        /// Gets the blog ids.
        /// </summary>
        /// <returns>System.String[].</returns>
        public static string[] GetBlogIds()
        {
            var blogIds = SettingsKey.GetSubSettingNames();

            for (var i = 0; i < blogIds.Length; i++)
            {
                if (!BlogIdIsValid(blogIds[i]))
                {
                    blogIds[i] = null;
                }
            }

            return ArrayHelper.Compact(blogIds);
        }

        /// <summary>
        /// Gets the blogs.
        /// </summary>
        /// <param name="sortByName">if set to <c>true</c> [sort by name].</param>
        /// <returns>BlogDescriptor[].</returns>
        public static BlogDescriptor[] GetBlogs(bool sortByName)
        {
            var ids = GetBlogIds();
            var blogs = new BlogDescriptor[ids.Length];
            for (var i = 0; i < ids.Length; i++)
            {
                using (var settings = ForBlogId(ids[i]))
                {
                    blogs[i] = new BlogDescriptor(ids[i], settings.BlogName, settings.HomepageUrl);
                }
            }

            if (sortByName)
            {
                Array.Sort(blogs, new BlogDescriptor.Comparer());
            }

            return blogs;
        }

        /// <summary>
        /// Blogs the identifier is valid.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool BlogIdIsValid(string id)
        {
            BlogSettings blogSettings = null;
            try
            {
                blogSettings = BlogSettings.ForBlogId(id);
                return blogSettings.IsValid ? BlogClientManager.IsValidClientType(blogSettings.ClientType) : false;

            }
            catch (ArgumentException)
            {
                Trace.WriteLine("Default blog has invalid client type, ignoring blog.");
                return false;
            }
            finally
            {
                if (blogSettings != null)
                {
                    blogSettings.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets or sets the default blog identifier.
        /// </summary>
        /// <value>The default blog identifier.</value>
        public static string DefaultBlogId
        {
            get
            {
                // try to get an explicitly set default profile id
                var defaultKey = SettingsKey.GetString(DEFAULT_WEBLOG, string.Empty);

                // if a default is specified and the key exists
                if (BlogIdIsValid(defaultKey))
                {
                    return defaultKey;
                }

                // if one is not specified then get the first one stored (if any)
                // (update the value while doing this so we don't have to repeat
                // this calculation)
                var blogIds = GetBlogIds();
                if (blogIds != null && blogIds.Length > 0)
                {
                    DefaultBlogId = blogIds[0];
                    return blogIds[0];
                }
                else
                {
                    return string.Empty;
                }
            }
            set => SettingsKey.SetString(DEFAULT_WEBLOG, value ?? string.Empty);
        }

        /// <summary>
        /// The default weblog
        /// </summary>
        public const string DEFAULT_WEBLOG = "DefaultWeblog";

        /// <summary>
        /// Delegate BlogSettingsListener
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        public delegate void BlogSettingsListener(string blogId);

        /// <summary>
        /// Occurs when [blog settings deleted].
        /// </summary>
        public static event BlogSettingsListener BlogSettingsDeleted;

        /// <summary>
        /// Called when [blog settings deleted].
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        private static void OnBlogSettingsDeleted(string blogId) => BlogSettingsDeleted?.Invoke(blogId);

        /// <summary>
        /// Fors the blog identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>BlogSettings.</returns>
        public static BlogSettings ForBlogId(string id) => new BlogSettings(id);

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogSettings" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <exception cref="ArgumentException">Invalid Blog Id {id} - id</exception>
        private BlogSettings(string id)
        {
            if (Guid.TryParse(id, out var guid))
            {
                this.Id = guid.ToString();
            }
            else
            {
                GC.SuppressFinalize(this);
                Trace.WriteLine($"Failed to load blog settings for: {id}");
                throw new ArgumentException($"Invalid Blog Id {id}", nameof(id));
            }
        }

        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        /// <remarks>used as a key into settings storage</remarks>
        public string Id { get; }

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        public bool IsValid => SettingsKey.HasSubSettings(this.Id);

        /// <summary>
        /// Gets or sets a value indicating whether this instance is spaces blog.
        /// </summary>
        /// <value><c>true</c> if this instance is spaces blog; otherwise, <c>false</c>.</value>
        public bool IsSpacesBlog
        {
            get => this.Settings.GetBoolean(IS_SPACES_BLOG, false);
            set => this.Settings.SetBoolean(IS_SPACES_BLOG, value);
        }

        /// <summary>
        /// The is spaces blog
        /// </summary>
        private const string IS_SPACES_BLOG = "IsSpacesBlog";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is share point blog.
        /// </summary>
        /// <value><c>true</c> if this instance is share point blog; otherwise, <c>false</c>.</value>
        public bool IsSharePointBlog
        {
            get => this.Settings.GetBoolean(IS_SHAREPOINT_BLOG, false);
            set => this.Settings.SetBoolean(IS_SHAREPOINT_BLOG, value);
        }

        /// <summary>
        /// The is sharepoint blog
        /// </summary>
        private const string IS_SHAREPOINT_BLOG = "IsSharePointBlog";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is google blogger blog.
        /// </summary>
        /// <value><c>true</c> if this instance is google blogger blog; otherwise, <c>false</c>.</value>
        public bool IsGoogleBloggerBlog
        {
            get => this.Settings.GetBoolean(IS_GOOGLE_BLOGGER_BLOG, false);
            set => this.Settings.SetBoolean(IS_GOOGLE_BLOGGER_BLOG, value);
        }

        /// <summary>
        /// The is google blogger blog
        /// </summary>
        private const string IS_GOOGLE_BLOGGER_BLOG = "IsGoogleBloggerBlog";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is static site blog.
        /// </summary>
        /// <value><c>true</c> if this instance is static site blog; otherwise, <c>false</c>.</value>
        public bool IsStaticSiteBlog
        {
            get => this.Settings.GetBoolean(IS_STATIC_SITE_BLOG, false);
            set => this.Settings.SetBoolean(IS_STATIC_SITE_BLOG, value);
        }

        /// <summary>
        /// The is static site blog
        /// </summary>
        private const string IS_STATIC_SITE_BLOG = "IsStaticSiteBlog";

        /// <summary>
        /// Id of the weblog on the host service
        /// </summary>
        /// <value>The host blog identifier.</value>
        public string HostBlogId
        {
            get => this.Settings.GetString(BLOG_ID, string.Empty);
            set => this.Settings.SetString(BLOG_ID, value);
        }

        /// <summary>
        /// The blog identifier
        /// </summary>
        private const string BLOG_ID = "BlogId";

        /// <summary>
        /// Gets or sets the name of the blog.
        /// </summary>
        /// <value>The name of the blog.</value>
        public string BlogName
        {
            get => this.Settings.GetString(BLOG_NAME, string.Empty);
            set => this.Settings.SetString(BLOG_NAME, value);
        }
        /// <summary>
        /// The blog name
        /// </summary>
        private const string BLOG_NAME = "BlogName";

        /// <summary>
        /// Gets or sets the homepage URL.
        /// </summary>
        /// <value>The homepage URL.</value>
        public string HomepageUrl
        {
            get => this.Settings.GetString(HOMEPAGE_URL, string.Empty);
            set => this.Settings.SetString(HOMEPAGE_URL, value);
        }

        /// <summary>
        /// The homepage URL
        /// </summary>
        private const string HOMEPAGE_URL = "HomepageUrl";

        /// <summary>
        /// Gets or sets a value indicating whether [force manual configuration].
        /// </summary>
        /// <value><c>true</c> if [force manual configuration]; otherwise, <c>false</c>.</value>
        public bool ForceManualConfig
        {
            get => this.Settings.GetBoolean(FORCE_MANUAL_CONFIG, false);
            set => this.Settings.SetBoolean(FORCE_MANUAL_CONFIG, value);
        }

        /// <summary>
        /// The force manual configuration
        /// </summary>
        private const string FORCE_MANUAL_CONFIG = "ForceManualConfig";

        /// <summary>
        /// Gets or sets the manifest download information.
        /// </summary>
        /// <value>The manifest download information.</value>
        public WriterEditingManifestDownloadInfo ManifestDownloadInfo
        {
            get
            {
                lock (manifestDownloadInfoLock)
                {
                    using (var manifestKey = this.Settings.GetSubSettings(WRITER_MANIFEST))
                    {
                        // at a minimum must have a source-url
                        var sourceUrl = manifestKey.GetString(MANIFEST_SOURCE_URL, string.Empty);
                        if (sourceUrl != string.Empty)
                        {
                            return new WriterEditingManifestDownloadInfo(
                                sourceUrl,
                                manifestKey.GetDateTime(MANIFEST_EXPIRES, DateTime.MinValue),
                                manifestKey.GetDateTime(MANIFEST_LAST_MODIFIED, DateTime.MinValue),
                                manifestKey.GetString(MANIFEST_ETAG, string.Empty));
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            set
            {
                lock (manifestDownloadInfoLock)
                {
                    if (value != null)
                    {
                        using (var manifestKey = this.Settings.GetSubSettings(WRITER_MANIFEST))
                        {
                            manifestKey.SetString(MANIFEST_SOURCE_URL, value.SourceUrl);
                            manifestKey.SetDateTime(MANIFEST_EXPIRES, value.Expires);
                            manifestKey.SetDateTime(MANIFEST_LAST_MODIFIED, value.LastModified);
                            manifestKey.SetString(MANIFEST_ETAG, value.ETag);
                        }
                    }
                    else
                    {
                        if (this.Settings.HasSubSettings(WRITER_MANIFEST))
                        {
                            this.Settings.UnsetSubsettingTree(WRITER_MANIFEST);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The writer manifest
        /// </summary>
        private const string WRITER_MANIFEST = "Manifest";

        /// <summary>
        /// The manifest source URL
        /// </summary>
        private const string MANIFEST_SOURCE_URL = "SourceUrl";

        /// <summary>
        /// The manifest expires
        /// </summary>
        private const string MANIFEST_EXPIRES = "Expires";

        /// <summary>
        /// The manifest last modified
        /// </summary>
        private const string MANIFEST_LAST_MODIFIED = "LastModified";

        /// <summary>
        /// The manifest etag
        /// </summary>
        private const string MANIFEST_ETAG = "ETag";

        /// <summary>
        /// The manifest download information lock
        /// </summary>
        private readonly static object manifestDownloadInfoLock = new object();

        /// <summary>
        /// Gets or sets the writer manifest URL.
        /// </summary>
        /// <value>The writer manifest URL.</value>
        public string WriterManifestUrl
        {
            get => this.Settings.GetString(WRITER_MANIFEST_URL, string.Empty);
            set => this.Settings.SetString(WRITER_MANIFEST_URL, value);
        }

        /// <summary>
        /// The writer manifest URL
        /// </summary>
        private const string WRITER_MANIFEST_URL = "ManifestUrl";

        /// <summary>
        /// Sets the provider.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <param name="serviceName">Name of the service.</param>
        public void SetProvider(string providerId, string serviceName)
        {
            this.Settings.SetString(PROVIDER_ID, providerId);
            this.Settings.SetString(SERVICE_NAME, serviceName);
        }

        /// <summary>
        /// Gets the provider identifier.
        /// </summary>
        /// <value>The provider identifier.</value>
        public string ProviderId
        {
            get
            {
                var providerId = this.Settings.GetString(PROVIDER_ID, string.Empty);
                if (providerId == "16B3FA3F-DAD7-4c93-A407-81CAE076883E")
                {
                    return "5FD58F3F-A36E-4aaf-8ABE-764248961FA0";
                }
                else
                {
                    return providerId;
                }
            }
        }
        /// <summary>
        /// The provider identifier
        /// </summary>
        private const string PROVIDER_ID = "ProviderId";

        /// <summary>
        /// Gets the name of the service.
        /// </summary>
        /// <value>The name of the service.</value>
        public string ServiceName => this.Settings.GetString(SERVICE_NAME, string.Empty);

        /// <summary>
        /// The service name
        /// </summary>
        private const string SERVICE_NAME = "ServiceName";

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>The type of the client.</value>
        public string ClientType
        {
            get
            {
                var clientType = this.Settings.GetString(CLIENT_TYPE, string.Empty);

                // temporary hack for migration of MovableType blogs
                // TODO: is there a cleaner place to do this?
                if (clientType == "MoveableType")
                {
                    return "MovableType";
                }

                return clientType;
            }
            set
            {
                // TODO:OLW
                // Hack to stop old Spaces configs to be violently/accidentally
                // upgrading to Atom. At time of this writing, this condition gets
                // hit by ServiceUpdateChecker running. This prevents the client
                // type from being changed in the registry from WindowsLiveSpaces
                // to WindowsLiveSpacesAtom; the only practical effect of letting
                // the write go to disk would be that you can't go back to an
                // older build of Writer. We don't have perfect forward compatibility
                // anyway--going through the config wizard with a Spaces blog will
                // also break older builds. But it seems like it's going too far
                // that just starting Writer will make that change.
                // We can take this out, if desired, anytime after Wave 3 goes final.
                if (value == "WindowsLiveSpacesAtom" && this.Settings.GetString(CLIENT_TYPE, string.Empty) == "WindowsLiveSpaces")
                {
                    return;
                }

                this.Settings.SetString(CLIENT_TYPE, value);
            }
        }

        /// <summary>
        /// The client type
        /// </summary>
        private const string CLIENT_TYPE = "ClientType";

        /// <summary>
        /// Gets or sets the post API URL.
        /// </summary>
        /// <value>The post API URL.</value>
        public string PostApiUrl
        {
            get => this.Settings.GetString(POST_API_URL, string.Empty);
            set => this.Settings.SetString(POST_API_URL, value);
        }

        /// <summary>
        /// The post API URL
        /// </summary>
        private const string POST_API_URL = "PostApiUrl";

        /// <summary>
        /// Gets or sets the home page overrides.
        /// </summary>
        /// <value>The home page overrides.</value>
        public IDictionary<string, string> HomePageOverrides
        {
            get
            {
                lock (homepageOptionOverridesLock)
                {
                    // Trying to avoid the creation of this key, so we will know when the service update runs whether we need to build
                    // these settings for the first time.
                    if (this.Settings.HasSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                    {
                        using (var homepageOptionOverridesKey = this.Settings.GetSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                        {
                            return homepageOptionOverridesKey.GetNames()
                                .ToDictionary(
                                    optionName => optionName,
                                    optionName => homepageOptionOverridesKey.GetString(optionName, string.Empty));
                        }
                    }

                    return new Dictionary<string, string>();
                }
            }

            set
            {
                lock (homepageOptionOverridesLock)
                {
                    // delete existing overrides
                    this.Settings.UnsetSubsettingTree(HOMEPAGE_OPTION_OVERRIDES);

                    // re-write overrides
                    using (var homepageOptionOverridesKey = this.Settings.GetSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                    {
                        foreach (var entry in value)
                        {
                            homepageOptionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The homepage option overrides
        /// </summary>
        private const string HOMEPAGE_OPTION_OVERRIDES = "HomepageOptions";

        /// <summary>
        /// The homepage option overrides lock
        /// </summary>
        private readonly static object homepageOptionOverridesLock = new object();

        /// <summary>
        /// Gets or sets the option overrides.
        /// </summary>
        /// <value>The option overrides.</value>
        public IDictionary<string, string> OptionOverrides
        {
            get
            {
                lock (optionOverridesLock)
                {
                    using (var optionOverridesKey = this.Settings.GetSubSettings(OPTION_OVERRIDES))
                    {
                        return optionOverridesKey.GetNames()
                            .ToDictionary(
                                optionName => optionName,
                                optionName => optionOverridesKey.GetString(optionName, string.Empty));
                    }
                }
            }
            set
            {
                lock (optionOverridesLock)
                {
                    // safely delete existing overrides
                    this.Settings.UnsetSubsettingTree(OPTION_OVERRIDES);

                    // re-write overrides
                    using (var optionOverridesKey = this.Settings.GetSubSettings(OPTION_OVERRIDES))
                    {
                        foreach (var entry in value)
                        {
                            optionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The option overrides
        /// </summary>
        private const string OPTION_OVERRIDES = "ManifestOptions";

        /// <summary>
        /// The option overrides lock
        /// </summary>
        private readonly static object optionOverridesLock = new object();

        /// <summary>
        /// Gets or sets the user option overrides.
        /// </summary>
        /// <value>The user option overrides.</value>
        public IDictionary<string, string> UserOptionOverrides
        {
            get
            {
                lock (userOptionOverridesLock)
                {
                    using (var userOptionOverridesKey = this.Settings.GetSubSettings(USER_OPTION_OVERRIDES))
                    {
                        return userOptionOverridesKey.GetNames()
                            .ToDictionary(
                                optionName => optionName,
                                optionName => userOptionOverridesKey.GetString(optionName, string.Empty));
                    }
                }
            }

            set
            {
                lock (userOptionOverridesLock)
                {
                    // delete existing overrides
                    this.Settings.UnsetSubsettingTree(USER_OPTION_OVERRIDES);

                    // re-write overrides
                    using (var userOptionOverridesKey = this.Settings.GetSubSettings(USER_OPTION_OVERRIDES))
                    {
                        foreach (var entry in value)
                        {
                            userOptionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The user option overrides
        /// </summary>
        private const string USER_OPTION_OVERRIDES = "UserOptionOverrides";

        /// <summary>
        /// The user option overrides lock
        /// </summary>
        private readonly static object userOptionOverridesLock = new object();

        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        IBlogCredentialsAccessor IBlogSettingsAccessor.Credentials => new BlogCredentialsAccessor(this.Id, this.Credentials);

        /// <summary>
        /// Gets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        IBlogCredentialsAccessor IBlogSettingsDetectionContext.Credentials => (this as IBlogSettingsAccessor).Credentials;

        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        public IBlogCredentials Credentials
        {
            get
            {
                if (this.blogCredentials == null)
                {
                    var credentialsDomain = new CredentialsDomain(this.ServiceName, this.BlogName, this.FavIcon, this.Image);
                    this.blogCredentials = new BlogCredentials(this.Settings, credentialsDomain);
                }

                return this.blogCredentials;
            }

            set => BlogCredentialsHelper.Copy(value, this.Credentials);
        }

        /// <summary>
        /// The blog credentials
        /// </summary>
        private BlogCredentials blogCredentials;

        /// <summary>
        /// Gets or sets the button descriptions.
        /// </summary>
        /// <value>The button descriptions.</value>
        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get
            {
                lock (buttonsLock)
                {
                    using (var providerButtons = this.Settings.GetSubSettings(BUTTONS_KEY))
                    {
                        return providerButtons.GetSubSettingNames()
                            .Select(
                                buttonId =>
                                {
                                    using (var buttonKey = providerButtons.GetSubSettings(buttonId))
                                    {
                                        return new BlogProviderButtonDescriptionFromSettings(buttonKey);
                                    }
                                })
                            .ToArray();
                    }
                }
            }
            set
            {
                lock (buttonsLock)
                {
                    // write button descriptions
                    using (var providerButtons = this.Settings.GetSubSettings(BUTTONS_KEY))
                    {
                        // track buttons that have been deleted (assume all have been deleted and then
                        // remove deleted buttons from the list as they are referenced)
                        var deletedButtons = new List<string>(providerButtons.GetSubSettingNames());

                        // write the descriptions
                        foreach (var buttonDescription in value)
                        {
                            // write
                            using (var buttonKey = providerButtons.GetSubSettings(buttonDescription.Id))
                            {
                                BlogProviderButtonDescriptionFromSettings.SaveFrameButtonDescriptionToSettings(buttonDescription, buttonKey);
                            }

                            // note that this button should not be deleted
                            deletedButtons.Remove(buttonDescription.Id);
                        }

                        // execute deletes
                        foreach (var buttonId in deletedButtons)
                        {
                            providerButtons.UnsetSubsettingTree(buttonId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The buttons key
        /// </summary>
        private const string BUTTONS_KEY = "CustomButtons";

        /// <summary>
        /// The buttons lock
        /// </summary>
        private readonly static object buttonsLock = new object();

        /// <summary>
        /// Gets or sets a value indicating whether [last publish failed].
        /// </summary>
        /// <value><c>true</c> if [last publish failed]; otherwise, <c>false</c>.</value>
        public bool LastPublishFailed
        {
            get => this.Settings.GetBoolean(LAST_PUBLISH_FAILED, false);
            set => this.Settings.SetBoolean(LAST_PUBLISH_FAILED, value);
        }

        /// <summary>
        /// The last publish failed
        /// </summary>
        private const string LAST_PUBLISH_FAILED = "LastPublishFailed";

        /// <summary>
        /// Gets or sets the fav icon.
        /// </summary>
        /// <value>The fav icon.</value>
        public byte[] FavIcon
        {
            get => this.Settings.GetByteArray(FAV_ICON, null);
            set => this.Settings.SetByteArray(FAV_ICON, value);
        }

        /// <summary>
        /// The fav icon
        /// </summary>
        private const string FAV_ICON = "FavIcon";

        /// <summary>
        /// Gets or sets the image.
        /// </summary>
        /// <value>The image.</value>
        public byte[] Image
        {
            get => this.Settings.GetByteArray(IMAGE, null);
            set
            {
                var imageBytes = value;
                if (imageBytes != null && imageBytes.Length == 0)
                {
                    imageBytes = null;
                }

                this.Settings.SetByteArray(IMAGE, imageBytes);
            }
        }

        /// <summary>
        /// The image
        /// </summary>
        private const string IMAGE = "ImageBytes";

        /// <summary>
        /// Gets or sets the watermark image.
        /// </summary>
        /// <value>The watermark image.</value>
        public byte[] WatermarkImage
        {
            get => this.Settings.GetByteArray(WATERMARK_IMAGE, null);
            set
            {
                var watermarkBytes = value;
                if (watermarkBytes != null && watermarkBytes.Length == 0)
                {
                    watermarkBytes = null;
                }

                this.Settings.SetByteArray(WATERMARK_IMAGE, watermarkBytes);
            }
        }
        /// <summary>
        /// The watermark image
        /// </summary>
        private const string WATERMARK_IMAGE = "WatermarkImageBytes";

        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        /// <value>The categories.</value>
        public BlogPostCategory[] Categories
        {
            get
            {
                lock (categoriesLock)
                {
                    // get the categories
                    BlogPostCategory[] categories;
                    using (var categoriesKey = this.Settings.GetSubSettings(CATEGORIES))
                    {
                        categories = categoriesKey.GetSubSettingNames()
                            .Select(
                                id =>
                                {
                                    using (var categoryKey = categoriesKey.GetSubSettings(id))
                                    {
                                        var name = categoryKey.GetString(CATEGORY_NAME, id);
                                        var parent = categoryKey.GetString(CATEGORY_PARENT, string.Empty);
                                        return new BlogPostCategory(id, name, parent);
                                    }
                                })
                            .ToArray();
                    }

                    if (categories.Length > 0)
                    {
                        return categories;
                    }
                    else // if we got no categories using the new format, try the old format
                    {
                        return this.LegacyCategories;
                    }
                }
            }
            set
            {
                lock (categoriesLock)
                {
                    // delete existing categories
                    var settings = this.Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(CATEGORIES);

                        // re-write categories
                        using (var categoriesKey = settings.GetSubSettings(CATEGORIES))
                        {
                            foreach (var category in value)
                            {
                                using (var categoryKey = categoriesKey.GetSubSettings(category.Id))
                                {
                                    categoryKey.SetString(CATEGORY_NAME, category.Name);
                                    categoryKey.SetString(CATEGORY_PARENT, category.Parent);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The categories
        /// </summary>
        private const string CATEGORIES = "Categories";

        /// <summary>
        /// The category name
        /// </summary>
        private const string CATEGORY_NAME = "Name";

        /// <summary>
        /// The category parent
        /// </summary>
        private const string CATEGORY_PARENT = "Parent";

        /// <summary>
        /// The categories lock
        /// </summary>
        private readonly static object categoriesLock = new object();

        /// <summary>
        /// The keyword persister
        /// </summary>
        private static readonly Dictionary<string, XmlSettingsPersister> keywordPersister = new Dictionary<string, XmlSettingsPersister>();

        /// <summary>
        /// Make sure to own _keywordsLock before calling this property
        /// </summary>
        /// <value>The keyword persister.</value>
        private XmlSettingsPersister KeywordPersister
        {
            get
            {
                if (!keywordPersister.ContainsKey(this.KeywordPath))
                {
                    keywordPersister.Add(this.KeywordPath, XmlFileSettingsPersister.Open(this.KeywordPath));
                }
                return keywordPersister[this.KeywordPath];
            }
        }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>The keywords.</value>
        public BlogPostKeyword[] Keywords
        {
            get
            {
                lock (keywordsLock)
                {
                    // Get all of the keyword subkeys
                    BlogPostKeyword[] keywords;
                    using (var keywordsKey = (XmlSettingsPersister)this.KeywordPersister.GetSubSettings(KEYWORDS))
                    {
                        // Read the name out of the subkey
                        keywords = keywordsKey.GetSubSettings()
                            .Select(id =>
                            {
                                using (var categoryKey = keywordsKey.GetSubSettings(id))
                                {
                                    var name = (string)categoryKey.Get(KEYWORD_NAME, typeof(string), id);
                                    return new BlogPostKeyword(name);
                                }
                            })
                            .ToArray();
                    }

                    if (keywords.Length > 0)
                    {
                        return keywords;
                    }
                    else
                    {
                        return new BlogPostKeyword[0];
                    }
                }

            }
            set
            {
                lock (keywordsLock)
                {
                    // safely delete existing categories
                    var keywordPersister = this.KeywordPersister;
                    using (keywordPersister.BatchUpdate())
                    {
                        keywordPersister.UnsetSubSettingsTree(KEYWORDS);

                        // re-write keywords
                        using (var keywordsKey = keywordPersister.GetSubSettings(KEYWORDS))
                        {
                            foreach (var keyword in value)
                            {
                                using (var keywordKey = keywordsKey.GetSubSettings(keyword.Name))
                                {
                                    keywordKey.Set(KEYWORD_NAME, keyword.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The keywords
        /// </summary>
        private const string KEYWORDS = "Keywords";

        /// <summary>
        /// The keyword name
        /// </summary>
        private const string KEYWORD_NAME = "Name";

        /// <summary>
        /// The keywords lock
        /// </summary>
        private readonly static object keywordsLock = new object();

        /// <summary>
        /// The keyword path
        /// </summary>
        private string keywordPath;

        /// <summary>
        /// The path to an xml file in the %APPDATA% folder that contains keywords for the current blog
        /// </summary>
        /// <value>The keyword path.</value>
        private string KeywordPath
        {
            get
            {
                if (string.IsNullOrEmpty(this.keywordPath))
                {
                    var folderPath = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "Keywords");
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }

                    this.keywordPath = Path.Combine(folderPath, string.Format(CultureInfo.InvariantCulture, "keywords_{0}.xml", this.Id));
                }

                return this.keywordPath;
            }
        }

        /// <summary>
        /// Gets the legacy categories.
        /// </summary>
        /// <value>The legacy categories.</value>
        private BlogPostCategory[] LegacyCategories
        {
            get
            {
                using (var categoriesKey = this.Settings.GetSubSettings(CATEGORIES))
                {
                    return categoriesKey.GetNames()
                        .Select(id => new BlogPostCategory(id, categoriesKey.GetString(id, id)))
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// Gets or sets the authors.
        /// </summary>
        /// <value>The authors.</value>
        public AuthorInfo[] Authors
        {
            get
            {
                lock (authorsLock)
                {
                    // get the authors
                    using (var authorsKey = this.Settings.GetSubSettings(AUTHORS))
                    {
                        return authorsKey.GetSubSettingNames()
                            .Select(
                                id =>
                                {
                                    using (var authorKey = authorsKey.GetSubSettings(id))
                                    {
                                        var name = authorKey.GetString(AUTHOR_NAME, string.Empty);
                                        if (name == string.Empty)
                                        {
                                            Trace.Fail($"Unexpected empty author name for id {id}");
                                            return null;
                                        }
                                        else
                                        {
                                            return new AuthorInfo(id, name);
                                        }
                                    }
                                })
                            .Where(a => a != null)
                            .ToArray();
                    }
                }
            }
            set
            {
                lock (authorsLock)
                {
                    // safely delete existing
                    var settings = this.Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(AUTHORS);

                        // re-write
                        using (var authorsKey = settings.GetSubSettings(AUTHORS))
                        {
                            foreach (var author in value)
                            {
                                using (var authorKey = authorsKey.GetSubSettings(author.Id))
                                {
                                    authorKey.SetString(AUTHOR_NAME, author.Name);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// The authors
        /// </summary>
        private const string AUTHORS = "Authors";

        /// <summary>
        /// The author name
        /// </summary>
        private const string AUTHOR_NAME = "Name";

        /// <summary>
        /// The authors lock
        /// </summary>
        private readonly static object authorsLock = new object();

        /// <summary>
        /// Gets or sets the pages.
        /// </summary>
        /// <value>The pages.</value>
        public PageInfo[] Pages
        {
            get
            {
                lock (pagesLock)
                {
                    // get the authors
                    using (var pagesKey = this.Settings.GetSubSettings(PAGES))
                    {
                        return pagesKey.GetSubSettingNames()
                            .Select(
                                id =>
                                {
                                    using (var pageKey = pagesKey.GetSubSettings(id))
                                    {
                                        var title = pageKey.GetString(PAGE_TITLE, string.Empty);
                                        var datePublished = pageKey.GetDateTime(PAGE_DATE_PUBLISHED, DateTime.MinValue);
                                        var parentId = pageKey.GetString(PAGE_PARENT_ID, string.Empty);

                                        return new PageInfo(id, title, datePublished, parentId);
                                    }
                                })
                            .ToArray();
                    }
                }
            }
            set
            {
                lock (pagesLock)
                {
                    // safely delete existing
                    var settings = this.Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(PAGES);

                        // re-write
                        using (var pagesKey = settings.GetSubSettings(PAGES))
                        {
                            foreach (var page in value)
                            {
                                using (var pageKey = pagesKey.GetSubSettings(page.Id))
                                {
                                    pageKey.SetString(PAGE_TITLE, page.Title);
                                    pageKey.SetDateTime(PAGE_DATE_PUBLISHED, page.DatePublished);
                                    pageKey.SetString(PAGE_PARENT_ID, page.ParentId);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The pages
        /// </summary>
        private const string PAGES = "Pages";

        /// <summary>
        /// The page title
        /// </summary>
        private const string PAGE_TITLE = "Name";

        /// <summary>
        /// The page date published
        /// </summary>
        private const string PAGE_DATE_PUBLISHED = "DatePublished";

        /// <summary>
        /// The page parent identifier
        /// </summary>
        private const string PAGE_PARENT_ID = "ParentId";

        /// <summary>
        /// The pages lock
        /// </summary>
        private readonly static object pagesLock = new object();

        /// <summary>
        /// Gets or sets the file upload support.
        /// </summary>
        /// <value>The file upload support.</value>
        public FileUploadSupport FileUploadSupport
        {
            get
            {
                var intVal = this.Settings.GetInt32(FILE_UPLOAD_SUPPORT, (int)FileUploadSupport.Weblog);
                switch (intVal)
                {
                    case (int)FileUploadSupport.FTP:
                        return FileUploadSupport.FTP;
                    case (int)FileUploadSupport.Weblog:
                    default:
                        return FileUploadSupport.Weblog;
                }
            }
            set => this.Settings.SetInt32(FILE_UPLOAD_SUPPORT, (int)value);
        }

        /// <summary>
        /// The file upload support
        /// </summary>
        private const string FILE_UPLOAD_SUPPORT = "FileUploadSupport";

        /// <summary>
        /// Gets the file upload settings.
        /// </summary>
        /// <value>The file upload settings.</value>
        public IBlogFileUploadSettings FileUploadSettings
        {
            get
            {
                if (this.fileUploadSettings == null)
                {
                    this.fileUploadSettings = new BlogFileUploadSettings(this.Settings.GetSubSettings("FileUploadSettings"));
                }

                return this.fileUploadSettings;
            }
        }

        /// <summary>
        /// The file upload settings
        /// </summary>
        private BlogFileUploadSettings fileUploadSettings;

        /// <summary>
        /// Gets the atom publishing protocol settings.
        /// </summary>
        /// <value>The atom publishing protocol settings.</value>
        public IBlogFileUploadSettings AtomPublishingProtocolSettings
        {
            get
            {
                if (this.atomPublishingProtocolSettings == null)
                {
                    this.atomPublishingProtocolSettings = new BlogFileUploadSettings(this.Settings.GetSubSettings("AtomSettings"));
                }

                return this.atomPublishingProtocolSettings;
            }
        }

        /// <summary>
        /// The atom publishing protocol settings
        /// </summary>
        private BlogFileUploadSettings atomPublishingProtocolSettings;

        /// <summary>
        /// Gets the publishing plugin settings.
        /// </summary>
        /// <value>The publishing plugin settings.</value>
        public BlogPublishingPluginSettings PublishingPluginSettings => new BlogPublishingPluginSettings(this.Settings.GetSubSettings("PublishingPlugins"));

        /// <summary>
        /// Delete this profile
        /// </summary>
        public void Delete()
        {
            // dispose the profile
            this.Dispose();

            using (this.MetaLock(APPLY_UPDATES_LOCK))
            {
                // delete the underlying settings tree
                SettingsKey.UnsetSubsettingTree(this.Id);
            }

            // if we are the default profile then set the default to null
            if (this.Id == DefaultBlogId)
            {
                DefaultBlogId = string.Empty;
            }

            OnBlogSettingsDeleted(this.Id);
        }

        /// <summary>
        /// Applies the updates.
        /// </summary>
        /// <param name="settingsContext">The settings context.</param>
        /// <exception cref="InvalidOperationException">Attempted to apply updates to invalid blog-id</exception>
        public void ApplyUpdates(IBlogSettingsDetectionContext settingsContext)
        {
            using (this.MetaLock(APPLY_UPDATES_LOCK))
            {
                if (BlogIdIsValid(this.Id))
                {
                    if (settingsContext.ManifestDownloadInfo != null)
                    {
                        this.ManifestDownloadInfo = settingsContext.ManifestDownloadInfo;
                    }

                    if (settingsContext.ClientType != null)
                    {
                        this.ClientType = settingsContext.ClientType;
                    }

                    if (settingsContext.FavIcon != null)
                    {
                        this.FavIcon = settingsContext.FavIcon;
                    }

                    if (settingsContext.Image != null)
                    {
                        this.Image = settingsContext.Image;
                    }

                    if (settingsContext.WatermarkImage != null)
                    {
                        this.WatermarkImage = settingsContext.WatermarkImage;
                    }

                    if (settingsContext.Categories != null)
                    {
                        this.Categories = settingsContext.Categories;
                    }

                    if (settingsContext.Keywords != null)
                    {
                        this.Keywords = settingsContext.Keywords;
                    }

                    if (settingsContext.ButtonDescriptions != null)
                    {
                        this.ButtonDescriptions = settingsContext.ButtonDescriptions;
                    }

                    if (settingsContext.OptionOverrides != null)
                    {
                        this.OptionOverrides = settingsContext.OptionOverrides;
                    }

                    if (settingsContext.HomePageOverrides != null)
                    {
                        this.HomePageOverrides = settingsContext.HomePageOverrides;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Attempted to apply updates to invalid blog-id");
                }
            }
        }

        /// <summary>
        /// Applies the updates lock.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>IDisposable.</returns>
        public static IDisposable ApplyUpdatesLock(string id) => metaLock.Lock(APPLY_UPDATES_LOCK + id);

        /// <summary>
        /// The meta lock
        /// </summary>
        private static readonly MetaLock metaLock = new MetaLock();

        /// <summary>
        /// Metas the lock.
        /// </summary>
        /// <param name="contextName">Name of the context.</param>
        /// <returns>IDisposable.</returns>
        private IDisposable MetaLock(string contextName) => metaLock.Lock(contextName + this.Id);

        /// <summary>
        /// The apply updates lock
        /// </summary>
        private const string APPLY_UPDATES_LOCK = "ApplyUpdates";

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.blogCredentials != null)
            {
                this.blogCredentials.Dispose();
                this.blogCredentials = null;
            }

            if (this.fileUploadSettings != null)
            {
                this.fileUploadSettings.Dispose();
                this.fileUploadSettings = null;
            }

            if (this.atomPublishingProtocolSettings != null)
            {
                this.atomPublishingProtocolSettings.Dispose();
                this.atomPublishingProtocolSettings = null;
            }

            if (this.settings != null)
            {
                this.settings.Dispose();
                this.settings = null;
            }

            // This block is unsafe because it's easy for a persister
            // to be disposed while it's still being used on another
            // thread.

            // if (_keywordPersister.ContainsKey(KeywordPath))
            // {
            //    _keywordPersister[KeywordPath].Dispose();
            //    _keywordPersister.Remove(KeywordPath);
            // }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="BlogSettings"/> class.
        /// </summary>
        ~BlogSettings()
        {
            Trace.Fail(string.Format(CultureInfo.InvariantCulture, "Failed to dispose BlogSettings!!! BlogId: {0} // BlogName: {1}", this.Id, this.BlogName));
        }

        /// <summary>
        /// Gets the file upload.
        /// </summary>
        /// <value>The file upload.</value>
        public IBlogFileUploadSettings FileUpload => this.FileUploadSettings;

        /// <summary>
        /// Key for this weblog
        /// </summary>
        /// <value>The settings.</value>
        private SettingsPersisterHelper Settings
        {
            get
            {
                if (this.settings == null)
                {
                    this.settings = GetWeblogSettingsKey(this.Id);
                }

                return this.settings;
            }
        }

        /// <summary>
        /// The settings
        /// </summary>
        private SettingsPersisterHelper settings;

        #region Class Configuration (location of settings, etc)

        /// <summary>
        /// Gets the provider buttons settings key.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>SettingsPersisterHelper.</returns>
        public static SettingsPersisterHelper GetProviderButtonsSettingsKey(string blogId) => GetWeblogSettingsKey(blogId).GetSubSettings(BUTTONS_KEY);

        /// <summary>
        /// Gets the weblog settings key.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>SettingsPersisterHelper.</returns>
        public static SettingsPersisterHelper GetWeblogSettingsKey(string blogId) => SettingsKey.GetSubSettings(blogId);

        /// <summary>
        /// Gets the settings key.
        /// </summary>
        /// <value>The settings key.</value>
        public static SettingsPersisterHelper SettingsKey { get; } = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Weblogs");

        #endregion
    }
}
