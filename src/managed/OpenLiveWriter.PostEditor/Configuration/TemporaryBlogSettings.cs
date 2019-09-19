// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Configuration
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;

    using OpenLiveWriter.BlogClient;
    using OpenLiveWriter.BlogClient.Detection;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Settings;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.PostEditor.PostHtmlEditing;

    public class TemporaryBlogSettings
        : IBlogSettingsAccessor, IBlogSettingsDetectionContext, ITemporaryBlogSettingsDetectionContext, ICloneable
    {
        public static TemporaryBlogSettings CreateNew() => new TemporaryBlogSettings();

        public static TemporaryBlogSettings ForBlogId(string blogId)
        {
            using (var blogSettings = BlogSettings.ForBlogId(blogId))
            {
                var tempSettings = new TemporaryBlogSettings(blogId)
                {
                    IsNewWeblog = false,
                    IsSpacesBlog = blogSettings.IsSpacesBlog,
                    IsSharePointBlog = blogSettings.IsSharePointBlog,
                    IsGoogleBloggerBlog = blogSettings.IsGoogleBloggerBlog,
                    IsStaticSiteBlog = blogSettings.IsStaticSiteBlog,
                    HostBlogId = blogSettings.HostBlogId,
                    BlogName = blogSettings.BlogName,
                    HomepageUrl = blogSettings.HomepageUrl,
                    ForceManualConfig = blogSettings.ForceManualConfig,
                    ManifestDownloadInfo = blogSettings.ManifestDownloadInfo
                };
                tempSettings.SetProvider(blogSettings.ProviderId, blogSettings.ServiceName, blogSettings.PostApiUrl, blogSettings.ClientType);
                tempSettings.Credentials = blogSettings.Credentials;
                tempSettings.LastPublishFailed = blogSettings.LastPublishFailed;
                tempSettings.Categories = blogSettings.Categories;
                tempSettings.Keywords = blogSettings.Keywords;
                tempSettings.Authors = blogSettings.Authors;
                tempSettings.Pages = blogSettings.Pages;
                tempSettings.FavIcon = blogSettings.FavIcon;
                tempSettings.Image = blogSettings.Image;
                tempSettings.WatermarkImage = blogSettings.WatermarkImage;
                tempSettings.OptionOverrides = blogSettings.OptionOverrides;
                tempSettings.UserOptionOverrides = blogSettings.UserOptionOverrides;
                tempSettings.ButtonDescriptions = blogSettings.ButtonDescriptions;
                tempSettings.HomePageOverrides = blogSettings.HomePageOverrides;

                //set the save password flag
                tempSettings.SavePassword = blogSettings.Credentials.Password != null &&
                    blogSettings.Credentials.Password != string.Empty;

                // file upload support
                tempSettings.FileUploadSupport = blogSettings.FileUploadSupport;

                // get ftp settings if necessary
                if (blogSettings.FileUploadSupport == FileUploadSupport.FTP)
                {
                    FtpUploaderSettings.Copy(blogSettings.FileUploadSettings, tempSettings.FileUploadSettings);
                }

                blogSettings.PublishingPluginSettings.CopyTo(tempSettings.PublishingPluginSettings);

                using (var editSettings = new PostHtmlEditingSettings(blogId))
                {
                    tempSettings.TemplateFiles = editSettings.EditorTemplateHtmlFiles;
                }
                return tempSettings;
            }
        }

        public void Save(BlogSettings settings)
        {
            settings.HostBlogId = this.HostBlogId;
            settings.IsSpacesBlog = this.IsSpacesBlog;
            settings.IsSharePointBlog = this.IsSharePointBlog;
            settings.IsGoogleBloggerBlog = this.IsGoogleBloggerBlog;
            settings.IsStaticSiteBlog = this.IsStaticSiteBlog;
            settings.BlogName = this.BlogName;
            settings.HomepageUrl = this.HomepageUrl;
            settings.ForceManualConfig = this.ForceManualConfig;
            settings.ManifestDownloadInfo = this.ManifestDownloadInfo;
            settings.SetProvider(this.ProviderId, this.ServiceName);
            settings.ClientType = this.ClientType;
            settings.PostApiUrl = this.PostApiUrl;
            if (this.IsSpacesBlog || !(this.SavePassword ?? false)) // clear out password so we don't save it
            {
                this.Credentials.Password = "";
            }

            settings.Credentials = this.Credentials;

            if (this.Categories != null)
            {
                settings.Categories = this.Categories;
            }

            if (this.Keywords != null)
            {
                settings.Keywords = this.Keywords;
            }

            settings.Authors = this.Authors;
            settings.Pages = this.Pages;

            settings.FavIcon = this.FavIcon;
            settings.Image = this.Image;
            settings.WatermarkImage = this.WatermarkImage;

            if (this.OptionOverrides != null)
            {
                settings.OptionOverrides = this.OptionOverrides;
            }

            if (this.UserOptionOverrides != null)
            {
                settings.UserOptionOverrides = this.UserOptionOverrides;
            }

            if (this.HomePageOverrides != null)
            {
                settings.HomePageOverrides = this.HomePageOverrides;
            }

            settings.ButtonDescriptions = this.ButtonDescriptions;

            // file upload support
            settings.FileUploadSupport = this.FileUploadSupport;

            // save ftp settings if necessary
            if (this.FileUploadSupport == FileUploadSupport.FTP)
            {
                FtpUploaderSettings.Copy(this.FileUploadSettings, settings.FileUploadSettings);
            }

            this.PublishingPluginSettings.CopyTo(settings.PublishingPluginSettings);

            using (var editSettings = new PostHtmlEditingSettings(settings.Id))
            {
                editSettings.EditorTemplateHtmlFiles = this.TemplateFiles;
            }
        }

        public void SetBlogInfo(BlogInfo blogInfo)
        {
            if (blogInfo.Id != this.HostBlogId)
            {
                this.BlogName = blogInfo.Name;
                this.HostBlogId = blogInfo.Id;
                this.HomepageUrl = blogInfo.HomepageUrl;
                if (!UrlHelper.IsUrl(this.HomepageUrl))
                {
                    Trace.Assert(!string.IsNullOrEmpty(this.HomepageUrl), "Homepage URL was null or empty");
                    var baseUrl = UrlHelper.GetBaseUrl(this.PostApiUrl);
                    this.HomepageUrl = UrlHelper.UrlCombineIfRelative(baseUrl, this.HomepageUrl);
                }

                // reset categories, authors, and pages
                this.Categories = new BlogPostCategory[] { };
                this.Keywords = new BlogPostKeyword[] { };
                this.Authors = new AuthorInfo[] { };
                this.Pages = new PageInfo[] { };

                // reset option overrides
                if (this.OptionOverrides != null)
                {
                    this.OptionOverrides.Clear();
                }

                if (this.UserOptionOverrides != null)
                {
                    this.UserOptionOverrides.Clear();
                }

                if (this.HomePageOverrides != null)
                {
                    this.HomePageOverrides.Clear();
                }

                // reset provider buttons
                if (this.ButtonDescriptions != null)
                {
                    this.ButtonDescriptions = new IBlogProviderButtonDescription[0];
                }

                // reset template
                this.TemplateFiles = new BlogEditingTemplateFile[0];
            }
        }

        public void SetProvider(string providerId, string serviceName, string postApiUrl, string clientType)
        {
            // for dirty states only
            if (this.ProviderId != providerId ||
                    this.ServiceName != serviceName ||
                    this.PostApiUrl != postApiUrl ||
                    this.ClientType != clientType)
            {
                // reset the provider info
                this.ProviderId = providerId;
                this.ServiceName = serviceName;
                this.PostApiUrl = postApiUrl;
                this.ClientType = clientType;
            }
        }

        public void ClearProvider()
        {
            this.ProviderId = string.Empty;
            this.ServiceName = string.Empty;
            this.PostApiUrl = string.Empty;
            this.ClientType = string.Empty;
            this.HostBlogId = string.Empty;
            this.ManifestDownloadInfo = null;
            this.OptionOverrides.Clear();
            this.TemplateFiles = new BlogEditingTemplateFile[0];
            this.HomePageOverrides.Clear();
            this.buttonDescriptions = new BlogProviderButtonDescription[0];
            this.Categories = new BlogPostCategory[0];
        }

        public string Id { get; set; } = string.Empty;

        public bool IsSpacesBlog { get; set; } = false;

        public bool? SavePassword { get; set; }

        public bool IsSharePointBlog { get; set; } = false;

        public bool IsGoogleBloggerBlog { get; set; } = false;

        public bool IsStaticSiteBlog { get; set; } = false;

        public string HostBlogId { get; set; } = string.Empty;

        public string BlogName { get; set; } = string.Empty;

        public string HomepageUrl { get; set; } = string.Empty;

        public bool ForceManualConfig { get; set; } = false;

        public WriterEditingManifestDownloadInfo ManifestDownloadInfo { get; set; } = null;

        public IDictionary<string, string> OptionOverrides { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> UserOptionOverrides { get; set; } = new Dictionary<string, string>();

        public IDictionary<string, string> HomePageOverrides { get; set; } = new Dictionary<string, string>();

        public void UpdatePostBodyBackgroundColor(Color color)
        {
            var dictionary = this.HomePageOverrides ?? new Dictionary<string, string>();
            dictionary[BlogClientOptions.POST_BODY_BACKGROUND_COLOR] = color.ToArgb().ToString(CultureInfo.InvariantCulture);
            this.HomePageOverrides = dictionary;
        }

        public IBlogProviderButtonDescription[] ButtonDescriptions
        {
            get => this.buttonDescriptions;
            set
            {
                this.buttonDescriptions = new BlogProviderButtonDescription[value.Length];
                for (var i = 0; i < value.Length; i++)
                {
                    this.buttonDescriptions[i] = new BlogProviderButtonDescription(value[i]);
                }
            }
        }

        public string ProviderId { get; private set; } = string.Empty;

        public string ServiceName { get; private set; } = string.Empty;

        public string ClientType { get; set; } = string.Empty;

        public string PostApiUrl { get; private set; } = string.Empty;

        IBlogCredentialsAccessor IBlogSettingsAccessor.Credentials => new BlogCredentialsAccessor(this.Id, this.Credentials);

        IBlogCredentialsAccessor IBlogSettingsDetectionContext.Credentials => new BlogCredentialsAccessor(this.Id, this.Credentials);

        public IBlogCredentials Credentials
        {
            get => this.credentials;
            set => BlogCredentialsHelper.Copy(value, this.credentials);
        }

        public bool LastPublishFailed { get; set; } = false;

        public BlogPostCategory[] Categories { get; set; } = null;

        public BlogPostKeyword[] Keywords { get; set; } = null;

        public AuthorInfo[] Authors { get; set; } = new AuthorInfo[0];

        public PageInfo[] Pages { get; set; } = new PageInfo[0];

        public byte[] FavIcon { get; set; } = null;

        public byte[] Image { get; set; } = null;

        public byte[] WatermarkImage { get; set; } = null;

        public FileUploadSupport FileUploadSupport { get; set; } = FileUploadSupport.Weblog;

        public IBlogFileUploadSettings FileUploadSettings => this.fileUploadSettings;

        public IBlogFileUploadSettings AtomPublishingProtocolSettings => this.atomPublishingProtocolSettings;

        public BlogPublishingPluginSettings PublishingPluginSettings => new BlogPublishingPluginSettings(this.pluginSettings);

        public BlogEditingTemplateFile[] TemplateFiles { get; set; } = new BlogEditingTemplateFile[0];

        public bool IsNewWeblog { get; set; } = true;

        public bool SwitchToWeblog { get; set; } = false;

        public BlogInfo[] HostBlogs { get; set; } = new BlogInfo[] { };

        public bool InstrumentationOptIn { get; set; } = false;

        public BlogInfo[] AvailableImageEndpoints { get; set; }

        private readonly TemporaryBlogCredentials credentials = new TemporaryBlogCredentials();
        private BlogProviderButtonDescription[] buttonDescriptions = new BlogProviderButtonDescription[0];
        private TemporaryFileUploadSettings fileUploadSettings = new TemporaryFileUploadSettings();
        private readonly TemporaryFileUploadSettings atomPublishingProtocolSettings = new TemporaryFileUploadSettings();
        private SettingsPersisterHelper pluginSettings = new SettingsPersisterHelper(new MemorySettingsPersister());

        //
        // IMPORTANT NOTE: When adding member variables you MUST update the CopyFrom() implementation below!!!!
        //
        private TemporaryBlogSettings() => this.Id = Guid.NewGuid().ToString();

        private TemporaryBlogSettings(string id) => this.Id = id;

        public void Dispose()
        {
        }

        public void CopyFrom(TemporaryBlogSettings sourceSettings)
        {
            // simple members
            this.Id = sourceSettings.Id;
            this.SwitchToWeblog = sourceSettings.SwitchToWeblog;
            this.IsNewWeblog = sourceSettings.IsNewWeblog;
            this.SavePassword = sourceSettings.SavePassword;
            this.IsSpacesBlog = sourceSettings.IsSpacesBlog;
            this.IsSharePointBlog = sourceSettings.IsSharePointBlog;
            this.IsGoogleBloggerBlog = sourceSettings.IsGoogleBloggerBlog;
            this.IsStaticSiteBlog = sourceSettings.IsStaticSiteBlog;
            this.HostBlogId = sourceSettings.HostBlogId;
            this.BlogName = sourceSettings.BlogName;
            this.HomepageUrl = sourceSettings.HomepageUrl;
            this.ManifestDownloadInfo = sourceSettings.ManifestDownloadInfo;
            this.ProviderId = sourceSettings.ProviderId;
            this.ServiceName = sourceSettings.ServiceName;
            this.ClientType = sourceSettings.ClientType;
            this.PostApiUrl = sourceSettings.PostApiUrl;
            this.LastPublishFailed = sourceSettings.LastPublishFailed;
            this.FileUploadSupport = sourceSettings.FileUploadSupport;
            this.InstrumentationOptIn = sourceSettings.InstrumentationOptIn;

            if (sourceSettings.AvailableImageEndpoints == null)
            {
                this.AvailableImageEndpoints = null;
            }
            else
            {
                // Good thing BlogInfo is immutable!
                this.AvailableImageEndpoints = (BlogInfo[])sourceSettings.AvailableImageEndpoints.Clone();
            }

            // credentials
            BlogCredentialsHelper.Copy(sourceSettings.credentials, this.credentials);

            // template files
            this.TemplateFiles = new BlogEditingTemplateFile[sourceSettings.TemplateFiles.Length];
            for (var i = 0; i < sourceSettings.TemplateFiles.Length; i++)
            {
                var sourceFile = sourceSettings.TemplateFiles[i];
                this.TemplateFiles[i] = new BlogEditingTemplateFile(sourceFile.TemplateType, sourceFile.TemplateFile);
            }

            // option overrides
            if (sourceSettings.OptionOverrides != null)
            {
                this.OptionOverrides.Clear();
                foreach (var entry in sourceSettings.OptionOverrides)
                {
                    this.OptionOverrides.Add(entry.Key, entry.Value);
                }
            }

            // user option overrides
            if (sourceSettings.UserOptionOverrides != null)
            {
                this.UserOptionOverrides.Clear();
                foreach (var entry in sourceSettings.UserOptionOverrides)
                {
                    this.UserOptionOverrides.Add(entry.Key, entry.Value);
                }
            }

            // homepage overrides
            if (sourceSettings.HomePageOverrides != null)
            {
                this.HomePageOverrides.Clear();
                foreach (var entry in sourceSettings.HomePageOverrides)
                {
                    this.HomePageOverrides.Add(entry.Key, entry.Value);
                }
            }

            // categories
            if (sourceSettings.Categories != null)
            {
                this.Categories = new BlogPostCategory[sourceSettings.Categories.Length];
                for (var i = 0; i < sourceSettings.Categories.Length; i++)
                {
                    var sourceCategory = sourceSettings.Categories[i];
                    this.Categories[i] = sourceCategory.Clone() as BlogPostCategory;
                }
            }
            else
            {
                this.Categories = null;
            }

            if (sourceSettings.Keywords != null)
            {
                this.Keywords = new BlogPostKeyword[sourceSettings.Keywords.Length];
                for (var i = 0; i < sourceSettings.Keywords.Length; i++)
                {
                    var sourceKeyword = sourceSettings.Keywords[i];
                    this.Keywords[i] = sourceKeyword.Clone() as BlogPostKeyword;
                }
            }
            else
            {
                this.Keywords = null;
            }

            // authors and pages
            this.Authors = sourceSettings.Authors.Clone() as AuthorInfo[];
            this.Pages = sourceSettings.Pages.Clone() as PageInfo[];

            // buttons
            if (sourceSettings.buttonDescriptions != null)
            {
                this.buttonDescriptions = new BlogProviderButtonDescription[sourceSettings.buttonDescriptions.Length];
                for (var i = 0; i < sourceSettings.buttonDescriptions.Length; i++)
                {
                    this.buttonDescriptions[i] = sourceSettings.buttonDescriptions[i].Clone() as BlogProviderButtonDescription;
                }
            }
            else
            {
                this.buttonDescriptions = null;
            }

            // favicon
            this.FavIcon = sourceSettings.FavIcon;

            // images
            this.Image = sourceSettings.Image;
            this.WatermarkImage = sourceSettings.WatermarkImage;

            // host blogs
            this.HostBlogs = new BlogInfo[sourceSettings.HostBlogs.Length];
            for (var i = 0; i < sourceSettings.HostBlogs.Length; i++)
            {
                var sourceBlog = sourceSettings.HostBlogs[i];
                this.HostBlogs[i] = new BlogInfo(sourceBlog.Id, sourceBlog.Name, sourceBlog.HomepageUrl);
            }

            // file upload settings
            this.fileUploadSettings = sourceSettings.fileUploadSettings.Clone() as TemporaryFileUploadSettings;

            this.pluginSettings = new SettingsPersisterHelper(new MemorySettingsPersister());
            this.pluginSettings.CopyFrom(sourceSettings.pluginSettings, true, true);
        }

        public object Clone()
        {
            var newSettings = new TemporaryBlogSettings();
            newSettings.CopyFrom(this);
            return newSettings;
        }

    }

    public class TemporaryBlogCredentials : IBlogCredentials
    {
        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string[] CustomValues
        {
            get
            {
                var customValues = new string[this.values.Count];
                if (this.values.Count > 0)
                {
                    this.values.Keys.CopyTo(customValues, 0);
                }

                return customValues;
            }
        }

        public string GetCustomValue(string name)
        {
            if (this.values.Contains(name))
            {
                return this.values[name] as string;
            }
            else
            {
                return string.Empty;
            }
        }

        public void SetCustomValue(string name, string value) => this.values[name] = value;

        public ICredentialsDomain Domain { get; set; }

        public void Clear()
        {
            this.Username = string.Empty;
            this.Password = string.Empty;
            this.values.Clear();
        }

        private readonly Hashtable values = new Hashtable();

    }

    public class TemporaryFileUploadSettings : IBlogFileUploadSettings, ICloneable
    {
        public TemporaryFileUploadSettings()
        {
        }

        public string GetValue(string name)
        {
            if (this.values.Contains(name))
            {
                return this.values[name] as string;
            }
            else
            {
                return string.Empty;
            }
        }

        public void SetValue(string name, string value) => this.values[name] = value;

        public string[] Names => (string[])new ArrayList(this.values.Keys).ToArray(typeof(string));

        public void Clear() => this.values.Clear();

        private readonly Hashtable values = new Hashtable();

        public object Clone()
        {
            var newSettings = new TemporaryFileUploadSettings();

            foreach (DictionaryEntry entry in this.values)
            {
                newSettings.SetValue(entry.Key as string, entry.Value as string);
            }

            return newSettings;
        }
    }
}

