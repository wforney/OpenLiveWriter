// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define PORTABLE

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Microsoft.Win32;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.Settings;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class ApplicationEnvironment.
    /// </summary>
    public class ApplicationEnvironment
    {
        /// <summary>
        /// The taskbar application identifier.
        /// </summary>
        /// <remarks>
        /// Changing the taskbar application id in upgrade scenarios can break the jumplist
        /// in the sense that it will be empty (no drafts/posts) until the post list cache is
        /// refreshed, which happens on initial configuration, post-publishing, and draft-saving.
        /// We use a unique, culture-invariant, hard-coded string here to avoid any inadvertent breaking changes.
        /// </remarks>
        public static string TaskbarApplicationId = "Open Live Writer - {3DDDAFC5-5C01-4BCF-B81A-A4976A0999E9}";

        private const string DefaultProductName = "Open Live Writer";
        private const string AppDataFolderName = "OpenLiveWriter";              // Squirrel installs the app to the folder that matches nuspec's ID.
        private const string DefaultSettingsRootKeyName = @"Software\\OpenLiveWriter";

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        public static void Initialize() =>
            Initialize(Assembly.GetCallingAssembly());

        /// <summary>
        /// Initializes the specified root assembly.
        /// </summary>
        /// <param name="rootAssembly">The root assembly.</param>
        public static void Initialize(Assembly rootAssembly) =>
            Initialize(rootAssembly, Path.GetDirectoryName(rootAssembly.Location));

        /// <summary>
        /// Initializes the specified root assembly.
        /// </summary>
        /// <param name="rootAssembly">The root assembly.</param>
        /// <param name="installationDirectory">The installation directory.</param>
        public static void Initialize(Assembly rootAssembly, string installationDirectory) =>
            Initialize(rootAssembly, installationDirectory, DefaultSettingsRootKeyName, DefaultProductName);

        /// <summary>
        /// Initializes the specified root assembly.
        /// </summary>
        /// <param name="rootAssembly">The root assembly.</param>
        /// <param name="installationDirectory">The installation directory.</param>
        /// <param name="settingsRootKeyName">Name of the settings root key.</param>
        /// <param name="productName">Name of the product.</param>
        public static void Initialize(Assembly rootAssembly, string installationDirectory, string settingsRootKeyName, string productName)
        {
            // initialize name and version based on assembly metadata
            var rootAssemblyPath = rootAssembly.Location;
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(rootAssemblyPath);
            CompanyName = fileVersionInfo.CompanyName;
            ProductName = productName;
            ProductVersion = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", fileVersionInfo.ProductMajorPart, fileVersionInfo.ProductMinorPart, fileVersionInfo.ProductBuildPart, fileVersionInfo.ProductPrivatePart);
            _appVersion = new Version(ProductVersion);

            Debug.Assert(
                _appVersion.Build < ushort.MaxValue &&
                _appVersion.Revision < ushort.MaxValue &&
                _appVersion.Major < ushort.MaxValue &&
                _appVersion.Minor < ushort.MaxValue,
                $"Invalid ApplicationVersion: {_appVersion}");

            // set installation directory and executable name
            InstallationDirectory = installationDirectory;
            MainExecutableName = Path.GetFileName(rootAssemblyPath);

            // initialize icon/user-agent, etc.
            UserAgent = FormatUserAgentString(ProductName, true);
            ProductIcon = ResourceHelper.LoadAssemblyResourceIcon("Images.ApplicationIcon.ico");
            ProductIconSmall = ResourceHelper.LoadAssemblyResourceIcon("Images.ApplicationIcon.ico", 16, 16);

            // initialize IsHighContrastWhite and IsHighContrastBlack
            InitializeIsHighContrastBlackWhite();

            SettingsRootKeyName = settingsRootKeyName;
            string dataPath;

            // see if we're running in portable mode
#if PORTABLE
            dataPath = Path.Combine(InstallationDirectory, "UserData");
            if (Directory.Exists(dataPath))
            {
                portable = true;

                // initialize application data directories
                ApplicationDataDirectory = Path.Combine(dataPath, "AppData\\Roaming");
                LocalApplicationDataDirectory = Path.Combine(dataPath, "AppData\\Local");

                // initialize settings
                UserSettingsRoot = new SettingsPersisterHelper(XmlFileSettingsPersister.Open(Path.Combine(dataPath, "UserSettings.xml")));
                MachineSettingsRoot = new SettingsPersisterHelper(XmlFileSettingsPersister.Open(Path.Combine(dataPath, "MachineSettings.xml")));
                PreferencesSettingsRoot = UserSettingsRoot.GetSubSettings(ApplicationConstants.PREFERENCES_SUB_KEY);
            }
            else
#endif
            {
                portable = false;

                // initialize application data directories.
                ApplicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolderName);
                LocalApplicationDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolderName);

                // initialize settings
                UserSettingsRoot = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.CurrentUser, SettingsRootKeyName));
                MachineSettingsRoot = new SettingsPersisterHelper(new RegistrySettingsPersister(Registry.LocalMachine, SettingsRootKeyName));
                PreferencesSettingsRoot = UserSettingsRoot.GetSubSettings(ApplicationConstants.PREFERENCES_SUB_KEY);

                dataPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }

            var postsDirectoryPostEditor = PreferencesSettingsRoot.GetSubSettings("PostEditor").GetString("PostsDirectory", null);

            if (string.IsNullOrEmpty(postsDirectoryPostEditor))
            {
                myWeblogPostsFolder = UserSettingsRoot.GetString("PostsDirectory", null);
                if (string.IsNullOrEmpty(myWeblogPostsFolder))
                {
                    if ((ProductName == DefaultProductName) && string.IsNullOrEmpty(dataPath))
                    {
                        throw new DirectoryException(MessageId.PersonalDirectoryFail);
                    }
                    else
                    {
                        myWeblogPostsFolder = Path.Combine(dataPath, "My Weblog Posts");
                    }
                }

                PreferencesSettingsRoot.GetSubSettings("PostEditor").SetString("PostsDirectory", myWeblogPostsFolder);
            }
            else
            {
                myWeblogPostsFolder = postsDirectoryPostEditor;
            }

            // initialize diagnostics
            InitializeLogFilePath();
            applicationDiagnostics = new ApplicationDiagnostics(LogFilePath, rootAssembly.GetName().Name);

            if (!Directory.Exists(ApplicationDataDirectory))
            {
                Directory.CreateDirectory(ApplicationDataDirectory);
            }

            if (!Directory.Exists(LocalApplicationDataDirectory))
            {
                Directory.CreateDirectory(LocalApplicationDataDirectory);
            }
        }

        /// <summary>
        /// Overrides the user agent.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <param name="browserBased">if set to <c>true</c> [browser based].</param>
        /// <remarks>
        /// allow override of product-name for user-agent (useful to cloak product's real identify during private beta testing).
        /// </remarks>
        public static void OverrideUserAgent(string productName, bool browserBased) => UserAgent = FormatUserAgentString(productName, browserBased);

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        /// <value>The name of the company.</value>
        public static string CompanyName { get; private set; } = string.Empty;

        /// <summary>
        /// Gets or sets the product name short.
        /// </summary>
        /// <value>The product name short.</value>
        public static string ProductName_Short { get; set; } = string.Empty;

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        /// <value>The name of the product.</value>
        public static string ProductName { get; private set; }

        /// <summary>
        /// Gets the product name qualified.
        /// </summary>
        /// <value>The product name qualified.</value>
        public static string ProductNameQualified
        {
            get
            {
#if BETA_BUILD
                return ProductName + " " + Res.Get(StringId.Beta);
#else
                return ProductName;
#endif
            }
        }

        /// <summary>
        /// Gets the product name versioned.
        /// </summary>
        /// <value>The product name versioned.</value>
        public static string ProductNameVersioned => Res.Get(StringId.ProductNameVersioned);

        /// <summary>
        /// Gets the product version.
        /// </summary>
        /// <value>The product version.</value>
        public static string ProductVersion { get; private set; }

        /// <summary>
        /// Gets the product version major.
        /// </summary>
        /// <value>The product version major.</value>
        public static string ProductVersionMajor => string.Format(CultureInfo.InvariantCulture, "{0}.{1}", _appVersion.Major, _appVersion.Build);

        /// <summary>
        /// Gets the product version minor.
        /// </summary>
        /// <value>The product version minor.</value>
        public static string ProductVersionMinor => string.Format(CultureInfo.InvariantCulture, "{0}.{1}", _appVersion.Minor, _appVersion.Revision);

        private static Version _appVersion;

        /// <summary>
        /// Gets or sets the product display version.
        /// </summary>
        /// <value>The product display version.</value>
        public static string ProductDisplayVersion { get; set; }

        /// <summary>
        /// Gets the installation directory.
        /// </summary>
        /// <value>The installation directory.</value>
        public static string InstallationDirectory { get; private set; }

        /// <summary>
        /// Gets the name of the main executable.
        /// </summary>
        /// <value>The name of the main executable.</value>
        public static string MainExecutableName { get; private set; }

        /// <summary>
        /// Gets the application data directory.
        /// </summary>
        /// <value>The application data directory.</value>
        public static string ApplicationDataDirectory { get; private set; }

        /// <summary>
        /// Gets the local application data directory.
        /// </summary>
        /// <value>The local application data directory.</value>
        public static string LocalApplicationDataDirectory { get; private set; }

        /// <summary>
        /// Gets the user agent.
        /// </summary>
        /// <value>The user agent.</value>
        public static string UserAgent { get; private set; }

        /// <summary>
        /// Gets the product icon.
        /// </summary>
        /// <value>The product icon.</value>
        public static Icon ProductIcon { get; private set; }

        /// <summary>
        /// Gets the product icon small.
        /// </summary>
        /// <value>The product icon small.</value>
        public static Icon ProductIconSmall { get; private set; }

        /// <summary>
        /// Initializes the is high contrast black white.
        /// </summary>
        private static void InitializeIsHighContrastBlackWhite()
        {
            if (System.Windows.Forms.SystemInformation.HighContrast)
            {
                if (SystemColors.Window.R.Equals(255) &&
                    SystemColors.Window.G.Equals(255) &&
                    SystemColors.Window.B.Equals(255))
                {
                    IsHighContrastWhite = true;
                }
                else
                {
                    IsHighContrastBlack = true;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is high contrast white.
        /// </summary>
        /// <value><c>true</c> if this instance is high contrast white; otherwise, <c>false</c>.</value>
        public static bool IsHighContrastWhite { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is high contrast black.
        /// </summary>
        /// <value><c>true</c> if this instance is high contrast black; otherwise, <c>false</c>.</value>
        public static bool IsHighContrastBlack { get; private set; }

        /// <summary>
        /// Gets the name of the settings root key.
        /// </summary>
        /// <value>The name of the settings root key.</value>
        public static string SettingsRootKeyName { get; private set; }

        /// <summary>
        /// Gets the user settings root.
        /// </summary>
        /// <value>The user settings root.</value>
        public static SettingsPersisterHelper UserSettingsRoot { get; private set; }

        /// <summary>
        /// Gets the machine settings root.
        /// </summary>
        /// <value>The machine settings root.</value>
        public static SettingsPersisterHelper MachineSettingsRoot { get; private set; }

        /// <summary>
        /// Gets the preferences settings root.
        /// </summary>
        /// <value>The preferences settings root.</value>
        public static SettingsPersisterHelper PreferencesSettingsRoot { get; private set; }

        private const string CustomColorsName = "CustomColors";

        /// <summary>
        /// Gets or sets the custom colors.
        /// </summary>
        /// <value>The custom colors.</value>
        public static int[] CustomColors
        {
            get
            {
                try
                {
                    var strVal = PreferencesSettingsRoot.GetString(CustomColorsName, null);
                    if (strVal != null)
                    {
                        var parts = StringHelper.Split(strVal, ",");
                        var retVal = new int[parts.Length];
                        for (var i = 0; i < retVal.Length; i++)
                        {
                            retVal[i] = int.Parse(parts[i], CultureInfo.InvariantCulture);
                        }

                        return retVal;
                    }
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                }

                return new int[]
                        {
                            (0 | 0 << 8 | 0 << 16),
                            (64 | 64 << 8 | 64 << 16),
                            (128 | 128 << 8 | 128 << 16),
                            (255 | 255 << 8 | 255 << 16),
                            (0 | 0 << 8 | 128 << 16),
                            (0 | 128 << 8 | 0 << 16),
                            (0 | 128 << 8 | 128 << 16),
                            (128 | 0 << 8 | 0 << 16),
                            (128 | 0 << 8 | 128 << 16),
                            (128 | 128 << 8 | 0 << 16),
                            (0 | 0 << 8 | 255 << 16),
                            (0 | 255 << 8 | 0 << 16),
                            (0 | 255 << 8 | 255 << 16),
                            (255 | 0 << 8 | 0 << 16),
                            (255 | 0 << 8 | 255 << 16),
                            (255 | 255 << 8 | 0 << 16),
                        };
            }

            set
            {
                if (value == null)
                {
                    PreferencesSettingsRoot.Unset(CustomColorsName);
                }
                else
                {
                    var sb = new StringBuilder();
                    var delim = string.Empty;
                    foreach (var i in value)
                    {
                        sb.Append(delim);
                        sb.Append(i.ToString(CultureInfo.InvariantCulture));
                        delim = ",";
                    }

                    PreferencesSettingsRoot.SetString(CustomColorsName, sb.ToString());
                }
            }
        }

        /// <summary>
        /// Gets the log file path.
        /// </summary>
        /// <value>The log file path.</value>
        public static string LogFilePath { get; private set; }

        /// <summary>
        /// Initializes the log file path.
        /// </summary>
        private static void InitializeLogFilePath()
        {
#if DEBUG
            LogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
#else
            _logFilePath = LocalApplicationDataDirectory ;
#endif
            LogFilePath = Path.Combine(LogFilePath, string.Format(CultureInfo.InvariantCulture, "{0}.log", ProductName));
        }

        /// <summary>
        /// Gets the application diagnostics.
        /// </summary>
        /// <value>The application diagnostics.</value>
        public static ApplicationDiagnostics ApplicationDiagnostics
        {
            get
            {
                // WinLive 218929 : If we are null, most likely something went wrong before we are fully
                // initialized and we are trying to watson. Just create a new instance here
                // using temp paths.
                if (applicationDiagnostics == null)
                {
                    var templogPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        string.Format(CultureInfo.InvariantCulture, "{0}.log", DefaultProductName));
                    applicationDiagnostics = new ApplicationDiagnostics(templogPath, Assembly.GetCallingAssembly().GetName().Name);
                }

                return applicationDiagnostics;
            }
        }

        private static ApplicationDiagnostics applicationDiagnostics;

        /// <summary>
        /// Gets a value indicating whether this instance is portable mode.
        /// </summary>
        /// <value><c>true</c> if this instance is portable mode; otherwise, <c>false</c>.</value>
        /// <exception cref="System.InvalidOperationException">ApplicationEnvironment has not been initialized</exception>
        public static bool IsPortableMode
        {
            get
            {
                if (portable == null)
                {
                    throw new InvalidOperationException("ApplicationEnvironment has not been initialized");
                }

                return portable.Value;
            }
        }

        private static bool? portable;

        /// <summary>
        /// Formats the user agent string.
        /// </summary>
        /// <param name="productName">Name of the product.</param>
        /// <param name="browserBased">if set to <c>true</c> [browser based].</param>
        /// <returns>System.String.</returns>
        public static string FormatUserAgentString(string productName, bool browserBased)
        {
            // get browser version
            SafeGetBrowserVersion(out var majorBrowserVersion, out var minorBrowserVersion);

            // get os version
            var osVersion = Environment.OSVersion.Version;

            // format user-agent string
            string userAgent;
            if (browserBased)
            {
                // e.g. "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 7.0; Open Live Writer 1.0)"
                userAgent = string.Format(
                    CultureInfo.InvariantCulture,
                    "Mozilla/4.0 (compatible; MSIE {0}.{1}; Windows NT {2}.{3}; {4} 1.0)",
                    majorBrowserVersion,
                    minorBrowserVersion,
                    osVersion.Major,
                    osVersion.Minor,
                    productName);
            }
            else
            {
                // e.g. "Open Live Writer 1.0 (Windows NT 7.0)"
                userAgent = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} 1.0 (Windows NT {1}.{2})",
                    productName,
                    osVersion.Major,
                    osVersion.Minor);
            }

            return userAgent;
        }

        /// <summary>
        /// Gets the browser version.
        /// </summary>
        /// <value>The browser version.</value>
        public static Version BrowserVersion
        {
            get
            {
                SafeGetBrowserVersion(out var majorBrowserVersion, out var minorBrowserVersion);
                return new Version(majorBrowserVersion, minorBrowserVersion);
            }
        }

        private static void SafeGetBrowserVersion(out int majorBrowserVersion, out int minorBrowserVersion)
        {
            try
            {
                BrowserHelper.GetInstalledVersion(out majorBrowserVersion, out minorBrowserVersion);
            }
            catch (Exception ex)
            {
                Debug.Fail("Unexpected exception getting browser version: " + ex.ToString());
                majorBrowserVersion = 6;
                minorBrowserVersion = 0;
            }
        }

        // default initialization for designer dependencies (only do this
        // when running in the IDE)
#if DEBUG
        static ApplicationEnvironment()
        {
            if (ProcessHelper.GetCurrentProcessName() == "devenv.exe")
            {
                Initialize(Assembly.GetExecutingAssembly());
            }
        }
#endif

        private static string myWeblogPostsFolder;

        /// <summary>
        /// Gets my weblog posts folder.
        /// </summary>
        /// <value>My weblog posts folder.</value>
        public static string MyWeblogPostsFolder => PreferencesSettingsRoot.GetSubSettings("PostEditor").GetString("PostsDirectory", null);

        /// <summary>
        /// Gets or sets the insert image directory.
        /// </summary>
        /// <value>The insert image directory.</value>
        public static string InsertImageDirectory
        {
            get
            {
                using (var settings = UserSettingsRoot.GetSubSettings("Preferences\\PostEditor"))
                {
                    var insertImageDirectory = settings.GetString("ImageInsertDir", null);
                    if (string.IsNullOrEmpty(insertImageDirectory) || !Directory.Exists(insertImageDirectory))
                    {
                        insertImageDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    }

                    return insertImageDirectory;
                }
            }

            set
            {
                using (var settings = UserSettingsRoot.GetSubSettings("Preferences\\PostEditor"))
                {
                    settings.SetString("ImageInsertDir", value);
                }
            }
        }
    }
}
