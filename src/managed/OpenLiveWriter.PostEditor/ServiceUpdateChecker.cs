// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using BlogClient;
    using BlogClient.Detection;
    using CoreServices;
    using CoreServices.Progress;
    using CoreServices.Threading;
    using Microsoft.Win32;

    /// <summary>
    /// The ServiceUpdateChecker class.
    /// </summary>
    internal class ServiceUpdateChecker
    {
        /// <summary>
        /// The service update lock
        /// </summary>
        private static readonly object ServiceUpdateLock = new object();

        /// <summary>
        /// The blog identifier
        /// </summary>
        private readonly string blogId;

        /// <summary>
        /// The settings changed handler
        /// </summary>
        private readonly WeblogSettingsChangedHandler settingsChangedHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceUpdateChecker"/> class.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="settingsChangedHandler">The settings changed handler.</param>
        public ServiceUpdateChecker(string blogId, WeblogSettingsChangedHandler settingsChangedHandler)
        {
            this.blogId = blogId;
            this.settingsChangedHandler = settingsChangedHandler;
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            var checkerThread = ThreadHelper.NewThread(this.Main, "ServiceUpdateChecker", true, true, true);
            checkerThread.Start();
        }

        /// <summary>
        /// Mains this instance.
        /// </summary>
        private void Main()
        {
            try
            {
                // delay the check for updates
                Thread.Sleep(1000);

                // only run one service-update at a time process wide
                lock (ServiceUpdateChecker.ServiceUpdateLock)
                {
                    // establish settings detection context
                    var settingsDetectionContext = new ServiceUpdateSettingsDetectionContext(this.blogId);

                    // fire-up a blog settings detector to query for changes
                    var settingsDetector = new BlogSettingsDetector(settingsDetectionContext)
                    {
                        SilentMode = true
                    };
                    using (var key = Registry.CurrentUser.OpenSubKey(
                        $@"{ApplicationEnvironment.SettingsRootKeyName}\Weblogs\{this.blogId}\HomepageOptions"))
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
                    using (var settings = BlogSettings.ForBlogId(this.blogId))
                    {
                        settings.ApplyUpdates(settingsDetectionContext);
                    }

                    // if changes were made then fire an event to notify the UI
                    if (settingsDetectionContext.HasUpdates)
                    {
                        this.settingsChangedHandler(this.blogId, false);
                    }
                }
            }
            catch (ManualKeepaliveOperationException)
            {
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception during ServiceUpdateChecker.Main: " + ex);
            }
        }
    }
}
