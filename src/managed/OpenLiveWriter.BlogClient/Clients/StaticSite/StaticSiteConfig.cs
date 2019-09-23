// <copyright file="StaticSiteConfig.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    /// <summary>
    /// The StaticSiteConfig class.
    /// </summary>
    public class StaticSiteConfig
    {
        /// <summary>
        /// The configuration build command
        /// </summary>
        private const string ConfigBuildCommand = "BuildCommand";

        /// <summary>
        /// The configuration building enabled
        /// </summary>
        private const string ConfigBuildingEnabled = "BuildingEnabled";

        /// <summary>
        /// The configuration command timeout milliseconds
        /// </summary>
        private const string ConfigCmdTimeoutMs = "CmdTimeoutMs";

        /// <summary>
        /// The configuration drafts enabled
        /// </summary>
        private const string ConfigDraftsEnabled = "DraftsEnabled";

        /// <summary>
        /// The configuration drafts path
        /// </summary>
        private const string ConfigDraftsPath = "DraftsPath";

        /// <summary>
        /// The configuration images enabled
        /// </summary>
        private const string ConfigImagesEnabled = "ImagesEnabled";

        /// <summary>
        /// The configuration images path
        /// </summary>
        private const string ConfigImagesPath = "ImagesPath";

        /// <summary>
        /// The configuration initialized
        /// </summary>
        private const string ConfigInitialized = "Initialized";

        /// <summary>
        /// The configuration output path
        /// </summary>
        private const string ConfigOutputPath = "OutputPath";

        /// <summary>
        /// The configuration pages enabled
        /// </summary>
        private const string ConfigPagesEnabled = "PagesEnabled";

        /// <summary>
        /// The configuration pages path
        /// </summary>
        private const string ConfigPagesPath = "PagesPath";

        /// <summary>
        /// The configuration posts path
        /// </summary>
        private const string ConfigPostsPath = "PostsPath";

        /// <summary>
        /// The configuration publish command
        /// </summary>
        private const string ConfigPublishCommand = "PublishCommand";

        /// <summary>
        /// The configuration show command windows
        /// </summary>
        private const string ConfigShowCmdWindows = "ShowCmdWindows";

        /// <summary>
        /// The configuration site URL
        /// </summary>
        private const string
            ConfigSiteUrl = "SiteUrl"; // Store Site Url in credentials as well, for access by StaticSiteClient

        /// <summary>
        /// Gets the default command timeout
        /// </summary>
        public static int DefaultCmdTimeout { get; } = 60000;

        /// <summary>
        /// Gets or sets the build command, executed by system command interpreter with LocalSitePath working directory
        /// </summary>
        /// <value>The build command.</value>
        public string BuildCommand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the site is locally built.
        /// </summary>
        /// <value><c>true</c> if [building enabled]; otherwise, <c>false</c>.</value>
        public bool BuildingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the timeout for commands. Default is 60k MS (60 seconds).
        /// </summary>
        /// <value>The command timeout milliseconds.</value>
        public int CmdTimeoutMs { get; set; } = StaticSiteConfig.DefaultCmdTimeout;

        /// <summary>
        /// Gets or sets a value indicating whether drafts can be saved to this blog.
        /// </summary>
        /// <value><c>true</c> if [drafts enabled]; otherwise, <c>false</c>.</value>
        public bool DraftsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the path to Drafts directory, relative to LocalSitePath.
        /// </summary>
        /// <value>The drafts path.</value>
        public string DraftsPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the front matter keys.
        /// </summary>
        /// <value>The front matter keys.</value>
        public StaticSiteConfigFrontMatterKeys FrontMatterKeys { get; set; } = new StaticSiteConfigFrontMatterKeys();

        /// <summary>
        /// Gets or sets a value indicating whether images can be uploaded to this blog.
        /// </summary>
        /// <value><c>true</c> if [images enabled]; otherwise, <c>false</c>.</value>
        public bool ImagesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the path to Images directory, relative to LocalSitePath.
        /// </summary>
        /// <value>The images path.</value>
        public string ImagesPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the parameter detection has occurred, default false.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        public bool Initialized { get; set; }

        // Public Site Url is stored in the blog's BlogConfig. Loading is handled in this class, but saving is handled from the WizardController.
        // This is done to avoid referencing PostEditor from this project.

        // NOTE: When setting default config values below, also make sure to alter LoadFromCredentials to not overwrite defaults if a key was not found.

        /// <summary>
        /// Gets or sets the full path to the local static site 'project' directory
        /// </summary>
        /// <value>The local site path.</value>
        public string LocalSitePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to Output directory, relative to LocalSitePath. Can be possibly used in future for preset publishing routines.
        /// </summary>
        /// <value>The output path.</value>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether pages can be posted to this blog.
        /// </summary>
        /// <value><c>true</c> if [pages enabled]; otherwise, <c>false</c>.</value>
        public bool PagesEnabled { get; set; }

        /// <summary>
        /// Gets or sets the path to Pages directory, relative to LocalSitePath.
        /// </summary>
        /// <value>The pages path.</value>
        public string PagesPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the path to Posts directory, relative to LocalSitePath
        /// </summary>
        /// <value>The posts path.</value>
        public string PostsPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the publish command, executed by system command interpreter with LocalSitePath working directory
        /// </summary>
        /// <value>The publish command.</value>
        public string PublishCommand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether to show CMD windows. Useful for debugging. Default is false.
        /// </summary>
        /// <value><c>true</c> if [show command windows]; otherwise, <c>false</c>.</value>
        public bool ShowCmdWindows { get; set; }

        /// <summary>
        /// Gets or sets the site title
        /// </summary>
        /// <value>The site title.</value>
        public string SiteTitle { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the public site URL
        /// </summary>
        /// <value>The site URL.</value>
        public string SiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets the validator.
        /// </summary>
        /// <value>The validator.</value>
        public StaticSiteConfigValidator Validator => new StaticSiteConfigValidator(this);

        /// <summary>
        /// Create a new <see cref="StaticSiteConfig"/> instance and loads site configuration from blog settings
        /// </summary>
        /// <param name="blogSettings">The blog settings.</param>
        /// <returns>A <see cref="StaticSiteConfig"/>.</returns>
        public static StaticSiteConfig LoadConfigFromBlogSettings(IBlogSettingsAccessor blogSettings)
        {
            var config = new StaticSiteConfig();
            config.LoadFromBlogSettings(blogSettings);
            return config;
        }

        /// <summary>
        /// Create a new <see cref="StaticSiteConfig"/> instance and load site configuration from blog credentials
        /// </summary>
        /// <param name="blogCredentials">An IBlogCredentialsAccessor</param>
        /// <returns>A <see cref="StaticSiteConfig"/>.</returns>
        public static StaticSiteConfig LoadConfigFromCredentials(IBlogCredentialsAccessor blogCredentials)
        {
            var config = new StaticSiteConfig();
            config.LoadFromCredentials(blogCredentials);
            return config;
        }

        /// <summary>
        /// Loads the configuration from credentials.
        /// </summary>
        /// <param name="blogCredentials">The blog credentials.</param>
        /// <returns>A <see cref="StaticSiteConfig"/>.</returns>
        public static StaticSiteConfig LoadConfigFromCredentials(IBlogCredentials blogCredentials) =>
            StaticSiteConfig.LoadConfigFromCredentials(new BlogCredentialsAccessor(string.Empty, blogCredentials));

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfig"/>.</returns>
        public StaticSiteConfig Clone() =>
            new StaticSiteConfig
                {
                    LocalSitePath = this.LocalSitePath,
                    PostsPath = this.PostsPath,
                    PagesEnabled = this.PagesEnabled,
                    PagesPath = this.PagesPath,
                    DraftsEnabled = this.DraftsEnabled,
                    DraftsPath = this.DraftsPath,
                    ImagesEnabled = this.ImagesEnabled,
                    ImagesPath = this.ImagesPath,
                    BuildingEnabled = this.BuildingEnabled,
                    OutputPath = this.OutputPath,
                    BuildCommand = this.BuildCommand,
                    PublishCommand = this.PublishCommand,
                    SiteUrl = this.SiteUrl,
                    SiteTitle = this.SiteTitle,
                    ShowCmdWindows = this.ShowCmdWindows,
                    CmdTimeoutMs = this.CmdTimeoutMs,
                    Initialized = this.Initialized,
                    FrontMatterKeys = this.FrontMatterKeys.Clone()
                };

        /// <summary>
        /// Loads site configuration from blog settings
        /// </summary>
        /// <param name="blogSettings">The blog settings.</param>
        public void LoadFromBlogSettings(IBlogSettingsAccessor blogSettings)
        {
            this.LoadFromCredentials(blogSettings.Credentials);

            this.SiteUrl = blogSettings.HomepageUrl;
            this.SiteTitle = blogSettings.BlogName;
        }

        /// <summary>
        /// Load site configuration from blog credentials
        /// </summary>
        /// <param name="credentials">An <see cref="IBlogCredentialsAccessor"/>.</param>
        public void LoadFromCredentials(IBlogCredentialsAccessor credentials)
        {
            this.LocalSitePath = credentials.Username;
            this.PostsPath = credentials.GetCustomValue(StaticSiteConfig.ConfigPostsPath);

            this.PagesEnabled = credentials.GetCustomValue(StaticSiteConfig.ConfigPagesEnabled) == "1";
            this.PagesPath = credentials.GetCustomValue(StaticSiteConfig.ConfigPagesPath);

            this.DraftsEnabled = credentials.GetCustomValue(StaticSiteConfig.ConfigDraftsEnabled) == "1";
            this.DraftsPath = credentials.GetCustomValue(StaticSiteConfig.ConfigDraftsPath);

            this.ImagesEnabled = credentials.GetCustomValue(StaticSiteConfig.ConfigImagesEnabled) == "1";
            this.ImagesPath = credentials.GetCustomValue(StaticSiteConfig.ConfigImagesPath);

            this.BuildingEnabled = credentials.GetCustomValue(StaticSiteConfig.ConfigBuildingEnabled) == "1";
            this.OutputPath = credentials.GetCustomValue(StaticSiteConfig.ConfigOutputPath);
            this.BuildCommand = credentials.GetCustomValue(StaticSiteConfig.ConfigBuildCommand);

            this.PublishCommand = credentials.GetCustomValue(StaticSiteConfig.ConfigPublishCommand);

            this.SiteUrl =
                credentials.GetCustomValue(
                    StaticSiteConfig
                       .ConfigSiteUrl); // This will be overridden in LoadFromBlogSettings, HomepageUrl is considered a more accurate source of truth

            this.ShowCmdWindows = credentials.GetCustomValue(StaticSiteConfig.ConfigShowCmdWindows) == "1";
            if (credentials.GetCustomValue(StaticSiteConfig.ConfigCmdTimeoutMs) != string.Empty)
            {
                this.CmdTimeoutMs = int.Parse(credentials.GetCustomValue(StaticSiteConfig.ConfigCmdTimeoutMs));
            }

            this.Initialized = credentials.GetCustomValue(StaticSiteConfig.ConfigInitialized) == "1";

            // Load FrontMatterKeys
            this.FrontMatterKeys = StaticSiteConfigFrontMatterKeys.LoadKeysFromCredentials(credentials);
        }

        /// <summary>
        /// Saves site configuration to blog credentials
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        public void SaveToCredentials(IBlogCredentialsAccessor credentials)
        {
            // Set username to Local Site Path
            credentials.Username = this.LocalSitePath;
            credentials.SetCustomValue(StaticSiteConfig.ConfigPostsPath, this.PostsPath);

            credentials.SetCustomValue(StaticSiteConfig.ConfigPagesEnabled, this.PagesEnabled ? "1" : "0");
            credentials.SetCustomValue(StaticSiteConfig.ConfigPagesPath, this.PagesPath);

            credentials.SetCustomValue(StaticSiteConfig.ConfigDraftsEnabled, this.DraftsEnabled ? "1" : "0");
            credentials.SetCustomValue(StaticSiteConfig.ConfigDraftsPath, this.DraftsPath);

            credentials.SetCustomValue(StaticSiteConfig.ConfigImagesEnabled, this.ImagesEnabled ? "1" : "0");
            credentials.SetCustomValue(StaticSiteConfig.ConfigImagesPath, this.ImagesPath);

            credentials.SetCustomValue(StaticSiteConfig.ConfigBuildingEnabled, this.BuildingEnabled ? "1" : "0");
            credentials.SetCustomValue(StaticSiteConfig.ConfigOutputPath, this.OutputPath);
            credentials.SetCustomValue(StaticSiteConfig.ConfigBuildCommand, this.BuildCommand);

            credentials.SetCustomValue(StaticSiteConfig.ConfigPublishCommand, this.PublishCommand);
            credentials.SetCustomValue(StaticSiteConfig.ConfigSiteUrl, this.SiteUrl);

            credentials.SetCustomValue(StaticSiteConfig.ConfigShowCmdWindows, this.ShowCmdWindows ? "1" : "0");
            credentials.SetCustomValue(StaticSiteConfig.ConfigCmdTimeoutMs, this.CmdTimeoutMs.ToString());
            credentials.SetCustomValue(StaticSiteConfig.ConfigInitialized, this.Initialized ? "1" : "0");

            // Save FrontMatterKeys
            this.FrontMatterKeys.SaveToCredentials(credentials);
        }

        /// <summary>
        /// Saves to credentials.
        /// </summary>
        /// <param name="blogCredentials">The blog credentials.</param>
        public void SaveToCredentials(IBlogCredentials blogCredentials) =>
            this.SaveToCredentials(new BlogCredentialsAccessor(string.Empty, blogCredentials));
    }
}
