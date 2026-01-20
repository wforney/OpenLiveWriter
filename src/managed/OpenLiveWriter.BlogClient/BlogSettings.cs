// Copyright (c) .NET Foundation. All rights reserved. Licensed under the MIT license. See LICENSE
// file in the project root for details.

using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.BlogClient.Detection;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace OpenLiveWriter.BlogClient
{
    public class BlogCredentials : IBlogCredentials, IDisposable
    {
        private const string PASSWORD = "Password";

        private const string USERNAME = "Username";

        private readonly SettingsPersisterHelper _settingsRoot;

        private SettingsPersisterHelper _credentialsSettingsRoot;

        public BlogCredentials(SettingsPersisterHelper settingsRoot, ICredentialsDomain domain)
        {
            this._settingsRoot = settingsRoot;
            Domain = domain;
        }

        public string[] CustomValues
        {
            get
            {
                ArrayList customValues = new ArrayList();
                string[] names = CredentialsSettings.GetNames();
                foreach (string name in names)
                {
                    if (name != USERNAME && name != PASSWORD)
                    {
                        _ = customValues.Add(name);
                    }
                }

                return customValues.ToArray(typeof(string)) as string[];
            }
        }

        public ICredentialsDomain Domain { get; set; }

        public string Password
        {
            get => GetPassword() ?? string.Empty;
            set
            {
                // save an encrypted password
                try
                {
                    CredentialsSettings.SetEncryptedString(PASSWORD, value);
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to encrypt weblog password: " + e.Message, e.StackTrace);
                }
            }
        }

        public string Username
        {
            get => GetUsername(); set => CredentialsSettings.SetString(USERNAME, value);
        }

        private SettingsPersisterHelper CredentialsSettings
        {
            get
            {
                if (this._credentialsSettingsRoot == null)
                {
                    this._credentialsSettingsRoot = this._settingsRoot.GetSubSettings("Credentials");
                }

                return this._credentialsSettingsRoot;
            }
        }

        public void Clear()
        {
            Username = string.Empty;
            Password = string.Empty;
            foreach (string name in CredentialsSettings.GetNames())
            {
                CredentialsSettings.SetString(name, null);
            }
        }

        public void Dispose() => this._credentialsSettingsRoot?.Dispose();

        public string GetCustomValue(string name) => CredentialsSettings.GetString(name, string.Empty);

        public void SetCustomValue(string name, string value) => CredentialsSettings.SetString(name, value);

        /// <summary>
        /// Get Password from either the credentials key or the root key (seamless migration of accounts that existed
        /// prior to us moving the credentials into their own subkey)
        /// </summary>
        /// <returns></returns>
        private string GetPassword()
        {
            string password = CredentialsSettings.GetEncryptedString(PASSWORD);
            return password ?? this._settingsRoot.GetEncryptedString(PASSWORD);
        }

        /// <summary>
        /// Get Username from either the credentials key or the root key (seamless migration of accounts that existed
        /// prior to us moving the credentials into their own subkey)
        /// </summary>
        /// <returns></returns>
        private string GetUsername()
        {
            string username = CredentialsSettings.GetString(USERNAME, null);
            return username ?? this._settingsRoot.GetString(USERNAME, string.Empty);
        }
    }

    public class BlogFileUploadSettings : IBlogFileUploadSettings, IDisposable
    {
        private SettingsPersisterHelper _settings;

        public BlogFileUploadSettings(SettingsPersisterHelper settings) => this._settings = settings;

        public string[] Names => this._settings.GetNames();

        public void Dispose()
        {
            this._settings?.Dispose();
            this._settings = null;
        }

        public string GetValue(string name) => this._settings.GetString(name, string.Empty);

        public void SetValue(string name, string value) => this._settings.SetString(name, value);
    }

    public class BlogSettings : IBlogSettingsAccessor, IBlogSettingsDetectionContext, IDisposable
    {
        public const string DEFAULT_WEBLOG = "DefaultWeblog";

        private const string APPLY_UPDATES_LOCK = "ApplyUpdates";

        private const string AUTHOR_NAME = "Name";

        private const string AUTHORS = "Authors";

        private const string BLOG_ID = "BlogId";

        private const string BLOG_NAME = "BlogName";

        private const string BUTTONS_KEY = "CustomButtons";

        private const string CATEGORIES = "Categories";

        private const string CATEGORY_NAME = "Name";

        private const string CATEGORY_PARENT = "Parent";

        private const string CLIENT_TYPE = "ClientType";

        private const string FAV_ICON = "FavIcon";

        private const string FILE_UPLOAD_SUPPORT = "FileUploadSupport";

        private const string FORCE_MANUAL_CONFIG = "ForceManualConfig";

        private const string HOMEPAGE_OPTION_OVERRIDES = "HomepageOptions";

        private const string HOMEPAGE_URL = "HomepageUrl";

        private const string IMAGE = "ImageBytes";

        private const string IS_GOOGLE_BLOGGER_BLOG = "IsGoogleBloggerBlog";

        private const string IS_SHAREPOINT_BLOG = "IsSharePointBlog";

        private const string IS_SPACES_BLOG = "IsSpacesBlog";

        private const string IS_STATIC_SITE_BLOG = "IsStaticSiteBlog";

        private const string KEYWORD_NAME = "Name";

        private const string KEYWORDS = "Keywords";

        private const string LAST_PUBLISH_FAILED = "LastPublishFailed";

        private const string MANIFEST_ETAG = "ETag";

        private const string MANIFEST_EXPIRES = "Expires";

        private const string MANIFEST_LAST_MODIFIED = "LastModified";

        private const string MANIFEST_SOURCE_URL = "SourceUrl";

        private const string OPTION_OVERRIDES = "ManifestOptions";

        private const string PAGE_DATE_PUBLISHED = "DatePublished";

        private const string PAGE_PARENT_ID = "ParentId";

        private const string PAGE_TITLE = "Name";

        private const string PAGES = "Pages";

        private const string POST_API_URL = "PostApiUrl";

        private const string PROVIDER_ID = "ProviderId";

        private const string SERVICE_NAME = "ServiceName";

        private const string USER_OPTION_OVERRIDES = "UserOptionOverrides";

        private const string WATERMARK_IMAGE = "WatermarkImageBytes";

        private const string WRITER_MANIFEST = "Manifest";

        private const string WRITER_MANIFEST_URL = "ManifestUrl";

        private static readonly object _authorsLock = new object();

        private static readonly object _buttonsLock = new object();

        private static readonly object _categoriesLock = new object();

        private static readonly object _homepageOptionOverridesLock = new object();

        private static readonly Dictionary<string, XmlSettingsPersister> _keywordPersister = new Dictionary<string, XmlSettingsPersister>();

        private static readonly object _keywordsLock = new object();

        private static readonly object _manifestDownloadInfoLock = new object();

        private static readonly MetaLock _metaLock = new MetaLock();

        private static readonly object _optionOverridesLock = new object();

        private static readonly object _pagesLock = new object();

        private static readonly object _userOptionOverridesLock = new object();
        private BlogFileUploadSettings _atomPublishingProtocolSettings;

        private BlogCredentials _blogCredentials;

        private BlogFileUploadSettings _fileUploadSettings;

        private string _keywordPath;

        private SettingsPersisterHelper _settings;

        private BlogSettings(string id)
        {
            try
            {
                Guid guid = new Guid(id);
                Id = guid.ToString();
            }
            catch (FormatException ex)
            {
                GC.SuppressFinalize(this);
                Trace.WriteLine("Failed to load blog settings for: " + id);
                throw new ArgumentException("Invalid Blog Id " + id, ex);
            }
        }

        ~BlogSettings()
        {
            Trace.Fail(string.Format(CultureInfo.InvariantCulture, "Failed to dispose BlogSettings!!! BlogId: {0} // BlogName: {1}", Id, BlogName));
        }

        public delegate void BlogSettingsListener(string blogId);

        public static event BlogSettingsListener BlogSettingsDeleted;

        public static string DefaultBlogId
        {
            get
            {
                // try to get an explicitly set default profile id
                string defaultKey = SettingsKey.GetString(DEFAULT_WEBLOG, string.Empty);

                // if a default is specified and the key exists
                if (BlogIdIsValid(defaultKey))
                {
                    return defaultKey;
                }

                // if one is not specified then get the first one stored (if any) (update the value
                // while doing this so we don't have to repeat this calculation)
                string[] blogIds = GetBlogIds();
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

        public IBlogFileUploadSettings AtomPublishingProtocolSettings
        {
            get
            {
                if (this._atomPublishingProtocolSettings == null)
                {
                    this._atomPublishingProtocolSettings = new BlogFileUploadSettings(Settings.GetSubSettings("AtomSettings"));
                }

                return this._atomPublishingProtocolSettings;
            }
        }

        public AuthorInfo[] Authors
        {
            get
            {
                lock (_authorsLock)
                {
                    // get the authors
                    ArrayList authors = new ArrayList();
                    using (SettingsPersisterHelper authorsKey = Settings.GetSubSettings(AUTHORS))
                    {
                        foreach (string id in authorsKey.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper authorKey = authorsKey.GetSubSettings(id))
                            {
                                string name = authorKey.GetString(AUTHOR_NAME, string.Empty);
                                if (name != string.Empty)
                                {
                                    _ = authors.Add(new AuthorInfo(id, name));
                                }
                                else
                                {
                                    Trace.Fail("Unexpected empty author name for id " + id);
                                }
                            }
                        }
                    }

                    return (AuthorInfo[])authors.ToArray(typeof(AuthorInfo));
                }
            }
            set
            {
                lock (_authorsLock)
                {
                    // safely delete existing
                    SettingsPersisterHelper settings = Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(AUTHORS);

                        // re-write
                        using (SettingsPersisterHelper authorsKey = settings.GetSubSettings(AUTHORS))
                        {
                            foreach (AuthorInfo author in value)
                            {
                                using (SettingsPersisterHelper authorKey = authorsKey.GetSubSettings(author.Id))
                                {
                                    authorKey.SetString(AUTHOR_NAME, author.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public string BlogName
        {
            get => Settings.GetString(BLOG_NAME, string.Empty); set => Settings.SetString(BLOG_NAME, value);
        }

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get
            {
                lock (_buttonsLock)
                {
                    ArrayList buttonDescriptions = new ArrayList();
                    using (SettingsPersisterHelper providerButtons = Settings.GetSubSettings(BUTTONS_KEY))
                    {
                        foreach (string buttonId in providerButtons.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper buttonKey = providerButtons.GetSubSettings(buttonId))
                            {
                                _ = buttonDescriptions.Add(new BlogProviderButtonDescriptionFromSettings(buttonKey));
                            }
                        }
                    }

                    return buttonDescriptions.ToArray(typeof(IBlogProviderButtonDescription)) as IBlogProviderButtonDescription[];
                }
            }
            set
            {
                lock (_buttonsLock)
                {
                    // write button descriptions
                    using (SettingsPersisterHelper providerButtons = Settings.GetSubSettings(BUTTONS_KEY))
                    {
                        // track buttons that have been deleted (assume all have been deleted and
                        // then remove deleted buttons from the list as they are referenced)
                        ArrayList deletedButtons = new ArrayList(providerButtons.GetSubSettingNames());

                        // write the descriptions
                        foreach (IBlogProviderButtonDescription buttonDescription in value)
                        {
                            // write
                            using (SettingsPersisterHelper buttonKey = providerButtons.GetSubSettings(buttonDescription.Id))
                            {
                                BlogProviderButtonDescriptionFromSettings.SaveFrameButtonDescriptionToSettings(buttonDescription, buttonKey);
                            }

                            // note that this button should not be deleted
                            deletedButtons.Remove(buttonDescription.Id);
                        }

                        // execute deletes
                        foreach (string buttonId in deletedButtons)
                        {
                            providerButtons.UnsetSubsettingTree(buttonId);
                        }
                    }
                }
            }
        }

        public BlogPostCategory[] Categories
        {
            get
            {
                lock (_categoriesLock)
                {
                    // get the categories
                    ArrayList categories = new ArrayList();
                    using (SettingsPersisterHelper categoriesKey = Settings.GetSubSettings(CATEGORIES))
                    {
                        foreach (string id in categoriesKey.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper categoryKey = categoriesKey.GetSubSettings(id))
                            {
                                string name = categoryKey.GetString(CATEGORY_NAME, id);
                                string parent = categoryKey.GetString(CATEGORY_PARENT, string.Empty);
                                _ = categories.Add(new BlogPostCategory(id, name, parent));
                            }
                        }
                    }

                    return categories.Count > 0 ? (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory)) : LegacyCategories;
                }
            }
            set
            {
                lock (_categoriesLock)
                {
                    // delete existing categories
                    SettingsPersisterHelper settings = Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(CATEGORIES);

                        // re-write categories
                        using (SettingsPersisterHelper categoriesKey = settings.GetSubSettings(CATEGORIES))
                        {
                            foreach (BlogPostCategory category in value)
                            {
                                using (SettingsPersisterHelper categoryKey = categoriesKey.GetSubSettings(category.Id))
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

        public string ClientType
        {
            get
            {
                string clientType = Settings.GetString(CLIENT_TYPE, string.Empty);

                // temporary hack for migration of MovableType blogs
                // TODO: is there a cleaner place to do this?
                return clientType == "MoveableType" ? "MovableType" : clientType;
            }
            set
            {
                // TODO:OLW Hack to stop old Spaces configs to be violently/accidentally upgrading
                // to Atom. At time of this writing, this condition gets hit by ServiceUpdateChecker
                // running. This prevents the client type from being changed in the registry from
                // WindowsLiveSpaces to WindowsLiveSpacesAtom; the only practical effect of letting
                // the write go to disk would be that you can't go back to an older build of Writer.
                // We don't have perfect forward compatibility anyway--going through the config
                // wizard with a Spaces blog will also break older builds. But it seems like it's
                // going too far that just starting Writer will make that change. We can take this
                // out, if desired, anytime after Wave 3 goes final.
                if (value == "WindowsLiveSpacesAtom" && Settings.GetString(CLIENT_TYPE, string.Empty) == "WindowsLiveSpaces")
                {
                    return;
                }

                Settings.SetString(CLIENT_TYPE, value);
            }
        }

        IBlogCredentialsAccessor IBlogSettingsAccessor.Credentials => new BlogCredentialsAccessor(Id, Credentials);

        IBlogCredentialsAccessor IBlogSettingsDetectionContext.Credentials => (this as IBlogSettingsAccessor).Credentials;

        public IBlogCredentials Credentials
        {
            get
            {
                if (this._blogCredentials == null)
                {
                    CredentialsDomain credentialsDomain = new CredentialsDomain(ServiceName, BlogName, FavIcon, Image);
                    this._blogCredentials = new BlogCredentials(Settings, credentialsDomain);
                }

                return this._blogCredentials;
            }

            set => BlogCredentialsHelper.Copy(value, Credentials);
        }

        public byte[] FavIcon
        {
            get => Settings.GetByteArray(FAV_ICON, null); set => Settings.SetByteArray(FAV_ICON, value);
        }

        public IBlogFileUploadSettings FileUpload => FileUploadSettings;

        public IBlogFileUploadSettings FileUploadSettings
        {
            get
            {
                if (this._fileUploadSettings == null)
                {
                    this._fileUploadSettings = new BlogFileUploadSettings(Settings.GetSubSettings("FileUploadSettings"));
                }

                return this._fileUploadSettings;
            }
        }

        public FileUploadSupport FileUploadSupport
        {
            get
            {
                int intVal = Settings.GetInt32(FILE_UPLOAD_SUPPORT, (int)FileUploadSupport.Weblog);
                switch (intVal)
                {
                    case (int)FileUploadSupport.FTP:
                        return FileUploadSupport.FTP;

                    case (int)FileUploadSupport.Weblog:
                    default:
                        return FileUploadSupport.Weblog;
                }
            }

            set => Settings.SetInt32(FILE_UPLOAD_SUPPORT, (int)value);
        }

        public bool ForceManualConfig
        {
            get => Settings.GetBoolean(FORCE_MANUAL_CONFIG, false); set => Settings.SetBoolean(FORCE_MANUAL_CONFIG, value);
        }

        public IDictionary HomePageOverrides
        {
            get
            {
                lock (_homepageOptionOverridesLock)
                {
                    IDictionary homepageOptionOverrides = new Hashtable();
                    // Trying to avoid the creation of this key, so we will know when the service
                    // update runs whether we need to build these settings for the first time.
                    if (Settings.HasSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                    {
                        using (SettingsPersisterHelper homepageOptionOverridesKey = Settings.GetSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                        {
                            foreach (string optionName in homepageOptionOverridesKey.GetNames())
                            {
                                homepageOptionOverrides.Add(optionName, homepageOptionOverridesKey.GetString(optionName, string.Empty));
                            }
                        }
                    }

                    return homepageOptionOverrides;
                }
            }
            set
            {
                lock (_homepageOptionOverridesLock)
                {
                    // delete existing overrides
                    Settings.UnsetSubsettingTree(HOMEPAGE_OPTION_OVERRIDES);

                    // re-write overrides
                    using (SettingsPersisterHelper homepageOptionOverridesKey = Settings.GetSubSettings(HOMEPAGE_OPTION_OVERRIDES))
                    {
                        foreach (DictionaryEntry entry in value)
                        {
                            homepageOptionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
        }

        public string HomepageUrl
        {
            get => Settings.GetString(HOMEPAGE_URL, string.Empty); set => Settings.SetString(HOMEPAGE_URL, value);
        }

        /// <summary>
        /// Id of the weblog on the host service
        /// </summary>
        public string HostBlogId
        {
            get => Settings.GetString(BLOG_ID, string.Empty); set => Settings.SetString(BLOG_ID, value);
        }

        /// <summary>
        /// used as a key into settings storage
        /// </summary>
        public string Id { get; }

        public byte[] Image
        {
            get => Settings.GetByteArray(IMAGE, null);
            set
            {
                byte[] imageBytes = value;
                if (imageBytes != null && imageBytes.Length == 0)
                {
                    imageBytes = null;
                }

                Settings.SetByteArray(IMAGE, imageBytes);
            }
        }

        public bool IsGoogleBloggerBlog
        {
            get => Settings.GetBoolean(IS_GOOGLE_BLOGGER_BLOG, false); set => Settings.SetBoolean(IS_GOOGLE_BLOGGER_BLOG, value);
        }

        public bool IsSharePointBlog
        {
            get => Settings.GetBoolean(IS_SHAREPOINT_BLOG, false); set => Settings.SetBoolean(IS_SHAREPOINT_BLOG, value);
        }

        public bool IsSpacesBlog
        {
            get => Settings.GetBoolean(IS_SPACES_BLOG, false); set => Settings.SetBoolean(IS_SPACES_BLOG, value);
        }

        public bool IsStaticSiteBlog
        {
            get => Settings.GetBoolean(IS_STATIC_SITE_BLOG, false); set => Settings.SetBoolean(IS_STATIC_SITE_BLOG, value);
        }

        public bool IsValid => SettingsKey.HasSubSettings(Id);

        public BlogPostKeyword[] Keywords
        {
            get
            {
                lock (_keywordsLock)
                {
                    ArrayList keywords = new ArrayList();
                    // Get all of the keyword subkeys
                    using (XmlSettingsPersister keywordsKey = (XmlSettingsPersister)KeywordPersister.GetSubSettings(KEYWORDS))
                    {
                        // Read the name out of the subkey
                        foreach (string id in keywordsKey.GetSubSettings())
                        {
                            using (ISettingsPersister categoryKey = keywordsKey.GetSubSettings(id))
                            {
                                string name = (string)categoryKey.Get(KEYWORD_NAME, typeof(string), id);
                                _ = keywords.Add(new BlogPostKeyword(name));
                            }
                        }
                    }

                    return keywords.Count > 0 ? (BlogPostKeyword[])keywords.ToArray(typeof(BlogPostKeyword)) : (new BlogPostKeyword[0]);
                }
            }
            set
            {
                lock (_keywordsLock)
                {
                    // safely delete existing categories
                    XmlSettingsPersister keywordPersister = KeywordPersister;
                    using (keywordPersister.BatchUpdate())
                    {
                        keywordPersister.UnsetSubSettingsTree(KEYWORDS);

                        // re-write keywords
                        using (ISettingsPersister keywordsKey = keywordPersister.GetSubSettings(KEYWORDS))
                        {
                            foreach (BlogPostKeyword keyword in value)
                            {
                                using (ISettingsPersister keywordKey = keywordsKey.GetSubSettings(keyword.Name))
                                {
                                    keywordKey.Set(KEYWORD_NAME, keyword.Name);
                                }
                            }
                        }
                    }
                }
            }
        }

        public bool LastPublishFailed
        {
            get => Settings.GetBoolean(LAST_PUBLISH_FAILED, false); set => Settings.SetBoolean(LAST_PUBLISH_FAILED, value);
        }

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo
        {
            get
            {
                lock (_manifestDownloadInfoLock)
                {
                    using (SettingsPersisterHelper manifestKey = Settings.GetSubSettings(WRITER_MANIFEST))
                    {
                        // at a minimum must have a source-url
                        string sourceUrl = manifestKey.GetString(MANIFEST_SOURCE_URL, string.Empty);
                        return sourceUrl != string.Empty
                            ? new WriterEditingManifestDownloadInfo(
                                sourceUrl,
                                manifestKey.GetDateTime(MANIFEST_EXPIRES, DateTime.MinValue),
                                manifestKey.GetDateTime(MANIFEST_LAST_MODIFIED, DateTime.MinValue),
                                manifestKey.GetString(MANIFEST_ETAG, string.Empty))
                            : null;
                    }
                }
            }
            set
            {
                lock (_manifestDownloadInfoLock)
                {
                    if (value != null)
                    {
                        using (SettingsPersisterHelper manifestKey = Settings.GetSubSettings(WRITER_MANIFEST))
                        {
                            manifestKey.SetString(MANIFEST_SOURCE_URL, value.SourceUrl);
                            manifestKey.SetDateTime(MANIFEST_EXPIRES, value.Expires);
                            manifestKey.SetDateTime(MANIFEST_LAST_MODIFIED, value.LastModified);
                            manifestKey.SetString(MANIFEST_ETAG, value.ETag);
                        }
                    }
                    else
                    {
                        if (Settings.HasSubSettings(WRITER_MANIFEST))
                        {
                            Settings.UnsetSubsettingTree(WRITER_MANIFEST);
                        }
                    }
                }
            }
        }

        public IDictionary OptionOverrides
        {
            get
            {
                lock (_optionOverridesLock)
                {
                    IDictionary optionOverrides = new Hashtable();
                    using (SettingsPersisterHelper optionOverridesKey = Settings.GetSubSettings(OPTION_OVERRIDES))
                    {
                        foreach (string optionName in optionOverridesKey.GetNames())
                        {
                            optionOverrides.Add(optionName, optionOverridesKey.GetString(optionName, string.Empty));
                        }
                    }

                    return optionOverrides;
                }
            }
            set
            {
                lock (_optionOverridesLock)
                {
                    // safely delete existing overrides
                    Settings.UnsetSubsettingTree(OPTION_OVERRIDES);

                    // re-write overrides
                    using (SettingsPersisterHelper optionOverridesKey = Settings.GetSubSettings(OPTION_OVERRIDES))
                    {
                        foreach (DictionaryEntry entry in value)
                        {
                            optionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
        }

        public PageInfo[] Pages
        {
            get
            {
                lock (_pagesLock)
                {
                    // get the authors
                    ArrayList pages = new ArrayList();
                    using (SettingsPersisterHelper pagesKey = Settings.GetSubSettings(PAGES))
                    {
                        foreach (string id in pagesKey.GetSubSettingNames())
                        {
                            using (SettingsPersisterHelper pageKey = pagesKey.GetSubSettings(id))
                            {
                                string title = pageKey.GetString(PAGE_TITLE, string.Empty);
                                DateTime datePublished = pageKey.GetDateTime(PAGE_DATE_PUBLISHED, DateTime.MinValue);
                                string parentId = pageKey.GetString(PAGE_PARENT_ID, string.Empty);
                                _ = pages.Add(new PageInfo(id, title, datePublished, parentId));
                            }
                        }
                    }

                    return (PageInfo[])pages.ToArray(typeof(PageInfo));
                }
            }
            set
            {
                lock (_pagesLock)
                {
                    // safely delete existing
                    SettingsPersisterHelper settings = Settings;
                    using (settings.BatchUpdate())
                    {
                        settings.UnsetSubsettingTree(PAGES);

                        // re-write
                        using (SettingsPersisterHelper pagesKey = settings.GetSubSettings(PAGES))
                        {
                            foreach (PageInfo page in value)
                            {
                                using (SettingsPersisterHelper pageKey = pagesKey.GetSubSettings(page.Id))
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

        public string PostApiUrl
        {
            get => Settings.GetString(POST_API_URL, string.Empty); set => Settings.SetString(POST_API_URL, value);
        }

        public string ProviderId
        {
            get
            {
                string providerId = Settings.GetString(PROVIDER_ID, string.Empty);
                return providerId == "16B3FA3F-DAD7-4c93-A407-81CAE076883E" ? "5FD58F3F-A36E-4aaf-8ABE-764248961FA0" : providerId;
            }
        }

        public BlogPublishingPluginSettings PublishingPluginSettings => new BlogPublishingPluginSettings(Settings.GetSubSettings("PublishingPlugins"));

        public string ServiceName => Settings.GetString(SERVICE_NAME, string.Empty);

        public IDictionary UserOptionOverrides
        {
            get
            {
                lock (_userOptionOverridesLock)
                {
                    IDictionary userOptionOverrides = new Hashtable();
                    using (SettingsPersisterHelper userOptionOverridesKey = Settings.GetSubSettings(USER_OPTION_OVERRIDES))
                    {
                        foreach (string optionName in userOptionOverridesKey.GetNames())
                        {
                            userOptionOverrides.Add(optionName, userOptionOverridesKey.GetString(optionName, string.Empty));
                        }
                    }

                    return userOptionOverrides;
                }
            }
            set
            {
                lock (_userOptionOverridesLock)
                {
                    // delete existing overrides
                    Settings.UnsetSubsettingTree(USER_OPTION_OVERRIDES);

                    // re-write overrides
                    using (SettingsPersisterHelper userOptionOverridesKey = Settings.GetSubSettings(USER_OPTION_OVERRIDES))
                    {
                        foreach (DictionaryEntry entry in value)
                        {
                            userOptionOverridesKey.SetString(entry.Key.ToString(), entry.Value.ToString());
                        }
                    }
                }
            }
        }

        public byte[] WatermarkImage
        {
            get => Settings.GetByteArray(WATERMARK_IMAGE, null);
            set
            {
                byte[] watermarkBytes = value;
                if (watermarkBytes != null && watermarkBytes.Length == 0)
                {
                    watermarkBytes = null;
                }

                Settings.SetByteArray(WATERMARK_IMAGE, watermarkBytes);
            }
        }

        public string WriterManifestUrl
        {
            get => Settings.GetString(WRITER_MANIFEST_URL, string.Empty); set => Settings.SetString(WRITER_MANIFEST_URL, value);
        }

        /// <summary>
        /// The path to an xml file in the %APPDATA% folder that contains keywords for the current blog
        /// </summary>
        private string KeywordPath
        {
            get
            {
                if (string.IsNullOrEmpty(this._keywordPath))
                {
                    string folderPath = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "Keywords");
                    if (!Directory.Exists(folderPath))
                    {
                        _ = Directory.CreateDirectory(folderPath);
                    }

                    this._keywordPath = Path.Combine(folderPath, string.Format(CultureInfo.InvariantCulture, "keywords_{0}.xml", Id));
                }

                return this._keywordPath;
            }
        }

        /// <summary>
        /// Make sure to own _keywordsLock before calling this property
        /// </summary>
        private XmlSettingsPersister KeywordPersister
        {
            get
            {
                if (!_keywordPersister.ContainsKey(KeywordPath))
                {
                    _keywordPersister.Add(KeywordPath, XmlFileSettingsPersister.Open(KeywordPath));
                }

                return _keywordPersister[KeywordPath];
            }
        }

        private BlogPostCategory[] LegacyCategories
        {
            get
            {
                ArrayList categories = new ArrayList();
                using (SettingsPersisterHelper categoriesKey = Settings.GetSubSettings(CATEGORIES))
                {
                    foreach (string id in categoriesKey.GetNames())
                    {
                        _ = categories.Add(new BlogPostCategory(id, categoriesKey.GetString(id, id)));
                    }
                }

                return (BlogPostCategory[])categories.ToArray(typeof(BlogPostCategory));
            }
        }

        /// <summary>
        /// Key for this weblog
        /// </summary>
        private SettingsPersisterHelper Settings
        {
            get
            {
                if (this._settings == null)
                {
                    this._settings = GetWeblogSettingsKey(Id);
                }

                return this._settings;
            }
        }

        public static IDisposable ApplyUpdatesLock(string id) => _metaLock.Lock(APPLY_UPDATES_LOCK + id);

        public static bool BlogIdIsValid(string id)
        {
            BlogSettings blogSettings = null;
            try
            {
                blogSettings = BlogSettings.ForBlogId(id);
                return blogSettings.IsValid && BlogClientManager.IsValidClientType(blogSettings.ClientType);
            }
            catch (ArgumentException)
            {
                Trace.WriteLine("Default blog has invalid client type, ignoring blog.");
                return false;
            }
            finally
            {
                blogSettings?.Dispose();
            }
        }

        public static BlogSettings ForBlogId(string id) => new BlogSettings(id);

        public static string[] GetBlogIds()
        {
            string[] blogIds = SettingsKey.GetSubSettingNames();

            for (int i = 0; i < blogIds.Length; i++)
            {
                if (!BlogIdIsValid(blogIds[i]))
                {
                    blogIds[i] = null;
                }
            }

            return (string[])ArrayHelper.Compact(blogIds);
        }

        public static BlogDescriptor[] GetBlogs(bool sortByName)
        {
            string[] ids = GetBlogIds();
            BlogDescriptor[] blogs = new BlogDescriptor[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                using (BlogSettings settings = BlogSettings.ForBlogId(ids[i]))
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

        public void ApplyUpdates(IBlogSettingsDetectionContext settingsContext)
        {
            using (MetaLock(APPLY_UPDATES_LOCK))
            {
                if (BlogSettings.BlogIdIsValid(Id))
                {
                    if (settingsContext.ManifestDownloadInfo != null)
                    {
                        ManifestDownloadInfo = settingsContext.ManifestDownloadInfo;
                    }

                    if (settingsContext.ClientType != null)
                    {
                        ClientType = settingsContext.ClientType;
                    }

                    if (settingsContext.FavIcon != null)
                    {
                        FavIcon = settingsContext.FavIcon;
                    }

                    if (settingsContext.Image != null)
                    {
                        Image = settingsContext.Image;
                    }

                    if (settingsContext.WatermarkImage != null)
                    {
                        WatermarkImage = settingsContext.WatermarkImage;
                    }

                    if (settingsContext.Categories != null)
                    {
                        Categories = settingsContext.Categories;
                    }

                    if (settingsContext.Keywords != null)
                    {
                        Keywords = settingsContext.Keywords;
                    }

                    if (settingsContext.ButtonDescriptions != null)
                    {
                        ButtonDescriptions = settingsContext.ButtonDescriptions;
                    }

                    if (settingsContext.OptionOverrides != null)
                    {
                        OptionOverrides = settingsContext.OptionOverrides;
                    }

                    if (settingsContext.HomePageOverrides != null)
                    {
                        HomePageOverrides = settingsContext.HomePageOverrides;
                    }
                }
                else
                {
                    throw new InvalidOperationException("Attempted to apply updates to invalid blog-id");
                }
            }
        }

        /// <summary>
        /// Delete this profile
        /// </summary>
        public void Delete()
        {
            // dispose the profile
            Dispose();

            using (MetaLock(APPLY_UPDATES_LOCK))
            {
                // delete the underlying settings tree
                SettingsKey.UnsetSubsettingTree(Id);
            }

            // if we are the default profile then set the default to null
            if (Id == DefaultBlogId)
            {
                DefaultBlogId = string.Empty;
            }

            OnBlogSettingsDeleted(Id);
        }

        public void Dispose()
        {
            this._blogCredentials?.Dispose();
            this._blogCredentials = null;
            this._fileUploadSettings?.Dispose();
            this._fileUploadSettings = null;
            this._atomPublishingProtocolSettings?.Dispose();
            this._atomPublishingProtocolSettings = null;
            this._settings?.Dispose();
            this._settings = null;

            // This block is unsafe because it's easy for a persister to be disposed while it's
            // still being used on another thread.

            // if (_keywordPersister.ContainsKey(KeywordPath)) {
            // _keywordPersister[KeywordPath].Dispose(); _keywordPersister.Remove(KeywordPath); }

            GC.SuppressFinalize(this);
        }

        public void SetProvider(string providerId, string serviceName)
        {
            Settings.SetString(PROVIDER_ID, providerId);
            Settings.SetString(SERVICE_NAME, serviceName);
        }

        private static void OnBlogSettingsDeleted(string blogId) => BlogSettingsDeleted?.Invoke(blogId);

        private IDisposable MetaLock(string contextName) => _metaLock.Lock(contextName + Id);

        #region Class Configuration (location of settings, etc)

        public static SettingsPersisterHelper SettingsKey { get; } = ApplicationEnvironment.UserSettingsRoot.GetSubSettings("Weblogs");

        public static SettingsPersisterHelper GetProviderButtonsSettingsKey(string blogId) => GetWeblogSettingsKey(blogId).GetSubSettings(BUTTONS_KEY);

        public static SettingsPersisterHelper GetWeblogSettingsKey(string blogId) => SettingsKey.GetSubSettings(blogId);

        #endregion Class Configuration (location of settings, etc)
    }
}