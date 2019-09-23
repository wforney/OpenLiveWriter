// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Configuration.Wizard
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Forms;

    using OpenLiveWriter.BlogClient;
    using OpenLiveWriter.BlogClient.Clients.StaticSite;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.Controls.Wizard;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class WeblogConfigurationWizardController.
    /// Implements the <see cref="OpenLiveWriter.Controls.Wizard.WizardController" />
    /// Implements the <see cref="OpenLiveWriter.BlogClient.IBlogClientUIContext" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.Wizard.WizardController" />
    /// <seealso cref="OpenLiveWriter.BlogClient.IBlogClientUIContext" />
    /// <seealso cref="System.IDisposable" />
    public class WeblogConfigurationWizardController : WizardController, IBlogClientUIContext, IDisposable
    {
        #region Creation and Initialization and Disposal

        /// <summary>
        /// Welcomes the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>System.String.</returns>
        public static string Welcome(IWin32Window owner)
        {
            var temporarySettings = TemporaryBlogSettings.CreateNew();
            using (var controller = new WeblogConfigurationWizardController(temporarySettings))
            {
                return controller.WelcomeWeblog(owner);
            }
        }

        /// <summary>
        /// Adds the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="permitSwitchingWeblogs">if set to <c>true</c> [permit switching weblogs].</param>
        /// <returns>System.String.</returns>
        public static string Add(IWin32Window owner, bool permitSwitchingWeblogs) => Add(owner, permitSwitchingWeblogs, out var switchToWeblog);

        /// <summary>
        /// Adds the specified owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="permitSwitchingWeblogs">if set to <c>true</c> [permit switching weblogs].</param>
        /// <param name="switchToWeblog">if set to <c>true</c> [switch to weblog].</param>
        /// <returns>System.String.</returns>
        public static string Add(IWin32Window owner, bool permitSwitchingWeblogs, out bool switchToWeblog)
        {
            var temporarySettings = TemporaryBlogSettings.CreateNew();

            temporarySettings.IsNewWeblog = true;
            temporarySettings.SwitchToWeblog = true;

            using (var controller = new WeblogConfigurationWizardController(temporarySettings))
            {
                return controller.AddWeblog(owner, ApplicationEnvironment.ProductNameQualified, permitSwitchingWeblogs, out switchToWeblog);
            }
        }

        /// <summary>
        /// Adds the blog.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="blogToAdd">The blog to add.</param>
        /// <returns>System.String.</returns>
        public static string AddBlog(IWin32Window owner, Uri blogToAdd)
        {
            var temporarySettings = TemporaryBlogSettings.CreateNew();

            temporarySettings.IsNewWeblog = true;
            temporarySettings.SwitchToWeblog = true;

            ParseAddBlogUri(blogToAdd, out var username, out var password, out var homepageUrl);
            temporarySettings.HomepageUrl = homepageUrl.ToString();
            temporarySettings.Credentials.Username = username;
            temporarySettings.Credentials.Password = password;
            temporarySettings.SavePassword = false;

            using (var controller = new WeblogConfigurationWizardController(temporarySettings))
            {
                return controller.AddWeblogSkipType(owner, ApplicationEnvironment.ProductNameQualified, false, out var dummy);
            }
        }

        /// <summary>
        /// Edits the temporary settings.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="settings">The settings.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool EditTemporarySettings(IWin32Window owner, TemporaryBlogSettings settings)
        {
            using (var controller = new WeblogConfigurationWizardController(settings))
            {
                return controller.EditWeblogTemporarySettings(owner);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WeblogConfigurationWizardController"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        private WeblogConfigurationWizardController(TemporaryBlogSettings settings)
            : base() => this._temporarySettings = settings;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // clear any cached credential information that may have been set by the wizard
            this.ClearTransientCredentials();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="WeblogConfigurationWizardController"/> class.
        /// </summary>
        ~WeblogConfigurationWizardController()
        {
            Debug.Fail("Wizard controller was not disposed");
        }

        /// <summary>
        /// Welcomes the weblog.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>System.String.</returns>
        private string WelcomeWeblog(IWin32Window owner)
        {
            this._preventSwitchingToWeblog = true;

            // welcome is the same as add with one additional step on the front end
            WizardStep wizardStep;
            this.addWizardStep(
                wizardStep = new WizardStep(
                    new WeblogConfigurationWizardPanelWelcome(),
                    StringId.ConfigWizardWelcome,
                    null,
                    null,
                    new NextCallback(this.OnWelcomeCompleted),
                    null,
                    null));
            wizardStep.WantsFocus = false;

            this.addWizardStep(
                new WizardStep(
                    new WeblogConfigurationWizardPanelConfirmation(),
                    StringId.ConfigWizardComplete,
                    new DisplayCallback(this.OnConfirmationDisplayed),
                    new VerifyStepCallback(this.OnValidatePanel),
                    new NextCallback(this.OnConfirmationCompleted),
                    null,
                    null));

            return this.ShowBlogWizard(ApplicationEnvironment.ProductNameQualified, owner, out var switchToWeblog);
        }

        private string AddWeblogSkipType(IWin32Window owner, string caption, bool permitSwitchingWeblogs, out bool switchToWeblog)
        {
            this._preventSwitchingToWeblog = !permitSwitchingWeblogs;

            this._temporarySettings.IsSpacesBlog = false;
            this._temporarySettings.IsSharePointBlog = false;

            this.AddBasicInfoSubStep();

            this.AddConfirmationStep();

            return this.ShowBlogWizard(caption, owner, out switchToWeblog);
        }

        private string AddWeblog(IWin32Window owner, string caption, bool permitSwitchingWeblogs, out bool switchToWeblog)
        {
            this._preventSwitchingToWeblog = !permitSwitchingWeblogs;

            this.AddChooseBlogTypeStep();
            this.AddConfirmationStep();

            return this.ShowBlogWizard(caption, owner, out switchToWeblog);
        }

        private string ShowBlogWizard(string caption, IWin32Window owner, out bool switchToWeblog)
        {
            // blog id to return
            string blogId = null;
            if (this.ShowDialog(owner, caption) == DialogResult.OK)
            {
                // save the blog settings
                using (var blogSettings = BlogSettings.ForBlogId(this._temporarySettings.Id))
                {
                    this._temporarySettings.Save(blogSettings);
                    blogId = blogSettings.Id;
                }

                // note the last added weblog (for re-selection in subsequent invocations of the dialog)
                WeblogConfigurationWizardSettings.LastServiceName = this._temporarySettings.ServiceName;
            }

            switchToWeblog = this._temporarySettings.SwitchToWeblog;

            return blogId;
        }

        private bool EditWeblogTemporarySettings(IWin32Window owner)
        {
            // first step conditional on blog type
            if (this._temporarySettings.IsSharePointBlog)
            {
                this.AddSharePointBasicInfoSubStep(true);
            }
            else if (this._temporarySettings.IsStaticSiteBlog)
            {
                this.AddStaticSiteInitialSubStep();
            }
            else
            {
                this.AddBasicInfoSubStep();
            }

            this.AddConfirmationStep();

            if (this.ShowDialog(owner, Res.Get(StringId.UpdateAccountConfigurationTitle)) == DialogResult.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddChooseBlogTypeStep() => this.addWizardStep(
                new WizardStep(
                    new WeblogConfigurationWizardPanelBlogType(),
                    StringId.ConfigWizardChooseWeblogType,
                    new DisplayCallback(this.OnChooseBlogTypeDisplayed),
                    null,
                    new NextCallback(this.OnChooseBlogTypeCompleted),
                    null,
                    null));

        private void AddBasicInfoSubStep() => this.addWizardSubStep(
                new WizardSubStep(
                    new WeblogConfigurationWizardPanelBasicInfo(),
                    StringId.ConfigWizardBasicInfo,
                    new DisplayCallback(this.OnBasicInfoDisplayed),
                    new VerifyStepCallback(this.OnValidatePanel),
                    new NextCallback(this.OnBasicInfoCompleted),
                    null,
                    null));

        private void AddSharePointBasicInfoSubStep(bool showAuthenticationStep)
        {
            this.addWizardSubStep(
                new WizardSubStep(
                    new WeblogConfigurationWizardPanelSharePointBasicInfo(),
                    StringId.ConfigWizardSharePointHomepage,
                    new DisplayCallback(this.OnBasicInfoDisplayed),
                    new VerifyStepCallback(this.OnValidatePanel),
                    new NextCallback(this.OnSharePointBasicInfoCompleted),
                    new NextCallback(this.OnSharePointBasicInfoUndone),
                    null));

            this._authenticationRequired = showAuthenticationStep;
        }

        private void AddGoogleBloggerOAuthSubStep() => this.addWizardSubStep(
                new WizardSubStep(
                    new WeblogConfigurationWizardPanelGoogleBloggerAuthentication(this._temporarySettings.Id, this),
                    null,
                    new DisplayCallback(this.OnBasicInfoDisplayed),
                    new VerifyStepCallback(this.OnValidatePanel),
                    new NextCallback(this.OnGoogleBloggerOAuthCompleted),
                    null,
                    new BackCallback(this.OnGoogleBloggerOAuthBack)));

        private void AddConfirmationStep() => this.addWizardStep(
                new WizardStep(
                    new WeblogConfigurationWizardPanelConfirmation(),
                    StringId.ConfigWizardComplete,
                    new DisplayCallback(this.OnConfirmationDisplayed),
                    new VerifyStepCallback(this.OnValidatePanel),
                    new NextCallback(this.OnConfirmationCompleted),
                    null,
                    null));

        private DialogResult ShowDialog(IWin32Window owner, string title)
        {
            using (new WaitCursor())
            {
                DialogResult result;

                using (this._wizardForm = new WeblogConfigurationWizard(this))
                using (new BlogClientUIContextScope(this._wizardForm))
                {
                    this._owner = this._wizardForm;

                    this._wizardForm.Text = title;

                    // Show in taskbar if it's a top-level window.  This is true during welcome
                    if (owner == null)
                    {
                        this._wizardForm.ShowInTaskbar = true;
                        this._wizardForm.StartPosition = FormStartPosition.CenterScreen;
                    }

                    result = this._wizardForm.ShowDialog(owner);

                    this._owner = null;
                }

                this._wizardForm = null;
                if (this._detectionOperation != null && !this._detectionOperation.IsDone)
                {
                    this._detectionOperation.Cancel();
                }

                return result;
            }
        }

        #endregion

        #region Welcome Panel
        private void OnWelcomeCompleted(Object stepControl) =>
            //setup the next steps based on which choice the user selected.
            this.addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelBlogType(),
                StringId.ConfigWizardChooseWeblogType,
                new DisplayCallback(this.OnChooseBlogTypeDisplayed),
                null,
                new NextCallback(this.OnChooseBlogTypeCompleted),
                null,
                null));
        #endregion

        #region Choose Blog Type Panel

        private void OnChooseBlogTypeDisplayed(Object stepControl)
        {
            // Fixes for 483356: In account configuration wizard, hitting back in select provider or success screens causes anomalous behavior
            // Need to clear cached credentials and cached blogname otherwise they'll be used downstream in the wizard...
            this.ClearTransientCredentials();
            this._temporarySettings.BlogName = string.Empty;

            // Bug 681904: Insure that the next and cancel are always available when this panel is displayed.
            this.NextEnabled = true;
            this.CancelEnabled = true;

            // get reference to panel
            var panelBlogType = stepControl as WeblogConfigurationWizardPanelBlogType;

            // notify it that it is being displayed (reset dirty state)
            panelBlogType.OnDisplayPanel();
        }

        private void OnChooseBlogTypeCompleted(Object stepControl)
        {
            // get reference to panel
            var panelBlogType = stepControl as WeblogConfigurationWizardPanelBlogType;

            // if the user is changing types then blank out the blog info
            if (panelBlogType.UserChangedSelection)
            {
                this._temporarySettings.HomepageUrl = String.Empty;
                this._temporarySettings.Credentials.Clear();
            }

            // set the user's choice
            this._temporarySettings.IsSharePointBlog = panelBlogType.IsSharePointBlog;
            this._temporarySettings.IsGoogleBloggerBlog = panelBlogType.IsGoogleBloggerBlog;
            this._temporarySettings.IsStaticSiteBlog = panelBlogType.IsStaticSiteBlog;

            // did this bootstrap a custom account wizard?
            this._providerAccountWizard = panelBlogType.ProviderAccountWizard;

            // add the next wizard sub step as appropriate
            if (this._temporarySettings.IsSharePointBlog)
            {
                this.AddSharePointBasicInfoSubStep(false);
            }
            else if (this._temporarySettings.IsGoogleBloggerBlog)
            {
                this.AddGoogleBloggerOAuthSubStep();
            }
            else if (this._temporarySettings.IsStaticSiteBlog)
            {
                this.AddStaticSiteInitialSubStep();
            }
            else
            {
                this.AddBasicInfoSubStep();
            }
        }

        #endregion

        private static void ParseAddBlogUri(Uri blogToAdd, out string username, out string password, out Uri homepageUrl)
        {
            // The URL is in the format http://username:password@blogUrl/;
            // We use the Uri class to extract the username:password (comes as a single string) and then parse it.
            // We strip the username:password from the remaining url and return it.

            username = null;
            password = null;

            var userInfoSplit = System.Web.HttpUtility.UrlDecode(blogToAdd.UserInfo).Split(':');
            if (userInfoSplit.Length > 0)
            {
                username = userInfoSplit[0];
                if (userInfoSplit.Length > 1)
                {
                    password = userInfoSplit[1];
                }
            }

            homepageUrl = new Uri(blogToAdd.GetComponents(UriComponents.HttpRequestUrl, UriFormat.UriEscaped));
        }

        #region Basic Info Panel

        private void OnBasicInfoDisplayed(Object stepControl)
        {
            // Fixes for 483356: In account configuration wizard, hitting back in select provider or success screens causes anomalous behavior
            // Need to clear cached credentials and cached blogname otherwise they'll be used downstream in the wizard...
            this._temporarySettings.BlogName = string.Empty;

            // get reference to data interface for panel
            var basicInfo = stepControl as IAccountBasicInfoProvider;

            // populate basic data
            basicInfo.ProviderAccountWizard = this._providerAccountWizard;
            basicInfo.AccountId = this._temporarySettings.Id;
            basicInfo.HomepageUrl = this._temporarySettings.HomepageUrl;
            basicInfo.ForceManualConfiguration = this._temporarySettings.ForceManualConfig;
            basicInfo.Credentials = this._temporarySettings.Credentials;
            basicInfo.SavePassword = basicInfo.Credentials.Password != String.Empty && (this._temporarySettings.SavePassword ?? true);
        }

        private delegate void PerformBlogAutoDetection();
        private void OnBasicInfoCompleted(Object stepControl) => this.OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(this.PerformWeblogAndSettingsAutoDetectionSubStep));

        private void OnBasicInfoAndAuthenticationCompleted(IAccountBasicInfoProvider basicInfo, PerformBlogAutoDetection performBlogAutoDetection)
        {
            // copy the settings
            this._temporarySettings.HomepageUrl = basicInfo.HomepageUrl;
            this._temporarySettings.ForceManualConfig = basicInfo.ForceManualConfiguration;
            this._temporarySettings.Credentials = basicInfo.Credentials;
            this._temporarySettings.SavePassword = basicInfo.SavePassword;

            // clear the transient credentials so we don't accidentally use cached credentials
            this.ClearTransientCredentials();

            if (!this._temporarySettings.ForceManualConfig)
            {
                // perform auto-detection
                performBlogAutoDetection();
            }
            else
            {
                this.PerformSelectProviderSubStep();
            }
        }

        private void OnSharePointBasicInfoCompleted(Object stepControl)
        {
            if (this._authenticationRequired)
                this.AddSharePointAuthenticationStep((IAccountBasicInfoProvider)stepControl);
            else
                this.OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(this.PerformSharePointAutoDetectionSubStep));
        }

        private void OnSharePointBasicInfoUndone(Object stepControl)
        {
            if (this._authenticationRequired && !this._authenticationStepAdded)
            {
                this.AddSharePointAuthenticationStep((IAccountBasicInfoProvider)stepControl);
                this.next();
            }
        }

        private void AddSharePointAuthenticationStep(IAccountBasicInfoProvider basicInfoProvider)
        {
            if (!this._authenticationStepAdded)
            {
                this.addWizardSubStep(new WizardSubStep(new WeblogConfigurationWizardPanelSharePointAuthentication(basicInfoProvider),
                                                   StringId.ConfigWizardSharePointLogin,
                                                   new WizardController.DisplayCallback(this.OnSharePointAuthenticationDisplayed),
                                                   new VerifyStepCallback(this.OnValidatePanel),
                                                   new WizardController.NextCallback(this.OnSharePointAuthenticationComplete),
                                                   null,
                                                   new WizardController.BackCallback(this.OnSharePointAuthenticationBack)));
                this._authenticationStepAdded = true;
            }
        }

        #endregion

        #region Static Site Generator support
        private StaticSiteConfig staticSiteConfig;

        private void AddStaticSiteInitialSubStep() => this.addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelStaticSiteInitial(),
                null,
                new DisplayCallback(this.OnStaticSiteInitialDisplayed),
                new VerifyStepCallback(this.OnStaticSiteValidatePanel),
                new NextCallback(this.OnStaticSiteInitialCompleted),
                null,
                new BackCallback(this.OnStaticSiteBack)));

        private void OnStaticSiteInitialDisplayed(Object stepControl)
        {
            // Populate data
            var panel = (stepControl as WeblogConfigurationWizardPanelStaticSiteInitial);
            // Load static config from credentials provided
            this.staticSiteConfig = StaticSiteConfig.LoadConfigFromBlogSettings(this._temporarySettings);
            panel.LoadFromConfig(this.staticSiteConfig);
        }

        private void OnStaticSiteInitialCompleted(Object stepControl)
        {
            var panel = (stepControl as WeblogConfigurationWizardPanelStaticSiteInitial);

            // Fill blog settings
            this._temporarySettings.SetProvider(
                StaticSiteClient.ProviderId,
                StaticSiteClient.ServiceName,
                StaticSiteClient.PostApiUrl,
                StaticSiteClient.ClientType
                );

            // Save config
            panel.SaveToConfig(this.staticSiteConfig);

            if (!this.staticSiteConfig.Initialized)
            {
                // Set initialised flag so detection isn't undertaken again
                this.staticSiteConfig.Initialized = true;
                // Attempt parameter detection
                var detectionResult = StaticSiteConfigDetector.AttemptAutoDetect(this.staticSiteConfig);
                if (detectionResult)
                {
                    // Successful detection of parameters
                    MessageBox.Show(
                        string.Format(Res.Get(StringId.CWStaticSiteConfigDetection), Res.Get(StringId.ProductNameVersioned)),
                        Res.Get(StringId.ProductNameVersioned),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }

            // Go to next step
            this.AddStaticSiteFeaturesSubStep();
        }

        private void AddStaticSitePaths1SubStep() => this.addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelStaticSitePaths1(),
                null,
                new DisplayCallback(this.OnStaticSiteConfigProviderDisplayed),
                new VerifyStepCallback(this.OnStaticSiteValidatePanel),
                new NextCallback(this.OnStaticSitePaths1Completed),
                null,
                new BackCallback(this.OnStaticSiteBack)));

        private void OnStaticSitePaths1Completed(Object stepControl)
        {
            var panel = (stepControl as WeblogConfigurationWizardPanelStaticSitePaths1);

            // Save panel values into config
            panel.SaveToConfig(this.staticSiteConfig);

            // Go to next step
            this.AddStaticSitePaths2SubStep();
        }

        private void AddStaticSitePaths2SubStep() => this.addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelStaticSitePaths2(),
                null,
                new DisplayCallback(this.OnStaticSiteConfigProviderDisplayed),
                new VerifyStepCallback(this.OnStaticSiteValidatePanel),
                new NextCallback(this.OnStaticSitePaths2Completed),
                null,
                new BackCallback(this.OnStaticSiteBack)));

        private void OnStaticSitePaths2Completed(Object stepControl)
        {
            var panel = (stepControl as WeblogConfigurationWizardPanelStaticSitePaths2);

            // Save panel values into config
            panel.SaveToConfig(this.staticSiteConfig);

            // Go to next step
            this.AddStaticSiteCommandsSubStep();
        }

        private void AddStaticSiteFeaturesSubStep() => this.addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelStaticSiteFeatures(),
                null,
                new DisplayCallback(this.OnStaticSiteConfigProviderDisplayed),
                new VerifyStepCallback(this.OnStaticSiteValidatePanel),
                new NextCallback(this.OnStaticSiteFeaturesCompleted),
                null,
                new BackCallback(this.OnStaticSiteBack)));

        private void OnStaticSiteFeaturesCompleted(Object stepControl)
        {
            var panel = (stepControl as WeblogConfigurationWizardPanelStaticSiteFeatures);

            // Save panel values into config
            panel.SaveToConfig(this.staticSiteConfig);

            // Go to next step
            this.AddStaticSitePaths1SubStep();
        }

        private void AddStaticSiteCommandsSubStep() => this.addWizardSubStep(
                new WizardSubStep(new WeblogConfigurationWizardPanelStaticSiteCommands(),
                null,
                new DisplayCallback(this.OnStaticSiteConfigProviderDisplayed),
                new VerifyStepCallback(this.OnStaticSiteValidatePanel),
                new NextCallback(this.OnStaticSiteCommandsCompleted),
                null,
                new BackCallback(this.OnStaticSiteBack)));

        private void OnStaticSiteCommandsCompleted(Object stepControl)
        {
            var panel = (stepControl as WeblogConfigurationWizardPanelStaticSiteCommands);

            // Save panel values into config
            panel.SaveToConfig(this.staticSiteConfig);

            // Go to next step
            this.PerformStaticSiteWizardCompletion();
        }

        private void OnStaticSiteBack(object step) =>
            // Save panel values before going back
            (step as IWizardPanelStaticSite).SaveToConfig(this.staticSiteConfig);

        private bool OnStaticSiteValidatePanel(object step)
        {
            var newConfig = this.staticSiteConfig.Clone();
            var panel = step as IWizardPanelStaticSite;
            panel.SaveToConfig(newConfig);

            try
            {
                panel.ValidateWithConfig(newConfig);
            }
            catch (StaticSiteConfigValidationException ex)
            {
                MessageBox.Show(ex.Text, ex.Title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void PerformStaticSiteWizardCompletion()
        {
            // Fill blog settings
            this._temporarySettings.SetProvider(
                StaticSiteClient.ProviderId,
                StaticSiteClient.ServiceName,
                StaticSiteClient.PostApiUrl,
                StaticSiteClient.ClientType
                );

            this._temporarySettings.HomepageUrl = this.staticSiteConfig.SiteUrl;
            this._temporarySettings.BlogName = this.staticSiteConfig.SiteTitle;

            // Fill config into credentials
            this.staticSiteConfig.SaveToCredentials(this._temporarySettings.Credentials);

            // Perform auto-detection
            this.addWizardSubStep(new WizardAutoDetectionStep(
                (IBlogClientUIContext)this,
                this._temporarySettings,
                null,
                new WizardSettingsAutoDetectionOperation(this._editWithStyleStep)));
        }

        private void OnStaticSiteConfigProviderDisplayed(Object stepControl)
        {
            // Populate data
            var panel = (stepControl as IWizardPanelStaticSite);

            // Load panel values from config
            panel.LoadFromConfig(this.staticSiteConfig);
        }

        #endregion

        #region Weblog and Settings Auto Detection

        private void PerformWeblogAndSettingsAutoDetectionSubStep()
        {
            // Clear the provider so the user will be forced to do autodetection
            // until we have successfully configured a publishing interface
            this._temporarySettings.ClearProvider();
            this._detectionOperation = new WizardWeblogAndSettingsAutoDetectionOperation(this._editWithStyleStep);

            // perform the step
            this.addWizardSubStep(new WizardAutoDetectionStep(
                (IBlogClientUIContext)this,
                this._temporarySettings,
                new NextCallback(this.OnWeblogAndSettingsAutoDetectionCompleted),
                this._detectionOperation));
        }

        private WizardWeblogAndSettingsAutoDetectionOperation _detectionOperation;

        private void PerformSharePointAutoDetectionSubStep()
        {
            // Clear the provider so the user will be forced to do autodetection
            // until we have successfully configured a publishing interface
            this._temporarySettings.ClearProvider();

            this.AddAutoDetectionStep();
        }

        private void AddAutoDetectionStep()
        {
            this._detectionOperation = new WizardSharePointAutoDetectionOperation(this._editWithStyleStep);

            var sharePointDetectionStep =
                new WizardSharePointAutoDetectionStep(
                (IBlogClientUIContext)this,
                this._temporarySettings,
                new NextCallback(this.OnWeblogAndSettingsAutoDetectionCompleted),
                this._detectionOperation);

            if (!this._authenticationStepAdded)
                sharePointDetectionStep.AuthenticationErrorOccurred += new EventHandler(this.sharePointDetectionStep_AuthenticationErrorOccurred);

            this.addWizardSubStep(sharePointDetectionStep);
        }
        private void sharePointDetectionStep_AuthenticationErrorOccurred(object sender, EventArgs e) => this._authenticationRequired = true;

        private void OnSharePointAuthenticationDisplayed(Object stepControl)
        {
            // get reference to panel
            var panelBlogType = stepControl as WeblogConfigurationWizardPanelSharePointAuthentication;

            // set value
            panelBlogType.Credentials = this._temporarySettings.Credentials;
            panelBlogType.SavePassword = this._temporarySettings.Credentials.Password != String.Empty;
        }

        private void OnSharePointAuthenticationComplete(Object stepControl) => this.OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(this.PerformSharePointAutoDetectionSubStep));

        private void OnSharePointAuthenticationBack(Object stepControl) => this._authenticationStepAdded = false;

        private void OnGoogleBloggerOAuthCompleted(Object stepControl) => this.OnBasicInfoAndAuthenticationCompleted((IAccountBasicInfoProvider)stepControl, new PerformBlogAutoDetection(this.PerformWeblogAndSettingsAutoDetectionSubStep));

        private void OnGoogleBloggerOAuthBack(Object stepControl)
        {
            var panel = (WeblogConfigurationWizardPanelGoogleBloggerAuthentication)stepControl;
            panel.CancelAuthorization();
        }

        private void OnWeblogAndSettingsAutoDetectionCompleted(Object stepControl)
        {
            // if we weren't able to identify a specific weblog
            if (this._temporarySettings.HostBlogId == String.Empty)
            {
                // if we have a list of weblogs then show the blog list
                if (this._temporarySettings.HostBlogs.Length > 0)
                {
                    this.PerformSelectBlogSubStep();
                }
                else // kick down to select a provider
                {
                    this.PerformSelectProviderSubStep();
                }
            }
            else
            {
                this.PerformSelectImageEndpointIfNecessary();
            }
        }

        private void PerformSelectImageEndpointIfNecessary()
        {
            if (this._temporarySettings.HostBlogId != string.Empty
                && this._temporarySettings.AvailableImageEndpoints != null
                && this._temporarySettings.AvailableImageEndpoints.Length > 0)
            {
                /*
                if (_temporarySettings.AvailableImageEndpoints.Length == 1)
                {
                    IDictionary optionOverrides = _temporarySettings.OptionOverrides;
                    optionOverrides[BlogClientOptions.IMAGE_ENDPOINT] = _temporarySettings.AvailableImageEndpoints[0].Id;
                    _temporarySettings.OptionOverrides = optionOverrides;
                }
                else
                    PerformSelectImageEndpointSubStep();
                */

                // currently we always show the image endpoint selection UI if we find at least one.
                this.PerformSelectImageEndpointSubStep();
            }
        }

        #endregion

        #region Select Provider Panel

        void PerformSelectProviderSubStep() => this.addWizardSubStep(new WizardSubStep(
                new WeblogConfigurationWizardPanelSelectProvider(),
                StringId.ConfigWizardSelectProvider,
                new DisplayCallback(this.OnSelectProviderDisplayed),
                new VerifyStepCallback(this.OnValidatePanel),
                new NextCallback(this.OnSelectProviderCompleted),
                null,
                null));

        void OnSelectProviderDisplayed(Object stepControl)
        {
            // get reference to panel
            var panelSelectProvider = stepControl as WeblogConfigurationWizardPanelSelectProvider;

            // show the panel
            panelSelectProvider.ShowPanel(
                this._temporarySettings.ServiceName,
                this._temporarySettings.HomepageUrl,
                this._temporarySettings.Id,
                this._temporarySettings.Credentials);
        }

        void OnSelectProviderCompleted(Object stepControl)
        {
            // get reference to panel
            var panelSelectProvider = stepControl as WeblogConfigurationWizardPanelSelectProvider;

            // record the provider and blog info
            var provider = panelSelectProvider.SelectedBlogProvider;
            this._temporarySettings.SetProvider(provider.Id, provider.Name, provider.PostApiUrl, provider.ClientType);
            this._temporarySettings.HostBlogId = String.Empty;
            if (panelSelectProvider.TargetBlog != null)
                this._temporarySettings.SetBlogInfo(panelSelectProvider.TargetBlog);
            this._temporarySettings.HostBlogs = panelSelectProvider.UsersBlogs;

            // If we don't yet have a HostBlogId then the user needs to choose from
            // among available weblogs
            if (this._temporarySettings.HostBlogId == String.Empty)
            {
                this.PerformSelectBlogSubStep();
            }
            else
            {
                // if we have not downloaded an editing template yet for this
                // weblog then execute this now
                this.PerformSettingsAutoDetectionSubStepIfNecessary();
            }
        }

        #endregion

        #region Select Blog Panel

        void PerformSelectBlogSubStep() => this.addWizardSubStep(new WizardSubStep(
                new WeblogConfigurationWizardPanelSelectBlog(),
                StringId.ConfigWizardSelectWeblog,
                new DisplayCallback(this.OnSelectBlogDisplayed),
                new VerifyStepCallback(this.OnValidatePanel),
                new NextCallback(this.OnSelectBlogCompleted),
                null,
                null));

        void OnSelectBlogDisplayed(Object stepControl)
        {
            // get reference to panel
            var panelSelectBlog = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // show the panel
            panelSelectBlog.ShowPanel(this._temporarySettings.HostBlogs, this._temporarySettings.HostBlogId);
        }

        private void OnSelectBlogCompleted(Object stepControl)
        {
            // get reference to panel
            var panelSelectBlog = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // get the selected blog
            this._temporarySettings.SetBlogInfo(panelSelectBlog.SelectedBlog);

            // if we have not downloaded an editing template yet for this
            // weblog then execute this now
            this.PerformSettingsAutoDetectionSubStepIfNecessary();
        }

        #endregion

        #region Select Image Endpoint Panel

        void PerformSelectImageEndpointSubStep()
        {
            var panel = new WeblogConfigurationWizardPanelSelectBlog();
            panel.LabelText = Res.Get(StringId.CWSelectImageEndpointText);
            this.addWizardSubStep(new WizardSubStep(
                                panel,
                                StringId.ConfigWizardSelectImageEndpoint,
                                new DisplayCallback(this.OnSelectImageEndpointDisplayed),
                                new VerifyStepCallback(this.OnValidatePanel),
                                new NextCallback(this.OnSelectImageEndpointCompleted),
                                null,
                                null));
        }

        void OnSelectImageEndpointDisplayed(Object stepControl)
        {
            // get reference to panel
            var panelSelectImageEndpoint = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // show the panel
            panelSelectImageEndpoint.ShowPanel(this._temporarySettings.AvailableImageEndpoints, this._temporarySettings.OptionOverrides[BlogClientOptions.IMAGE_ENDPOINT] as string);
        }

        private void OnSelectImageEndpointCompleted(Object stepControl)
        {
            // get reference to panel
            var panelSelectBlog = stepControl as WeblogConfigurationWizardPanelSelectBlog;

            // get the selected blog
            var optionOverrides = this._temporarySettings.HomePageOverrides;
            optionOverrides[BlogClientOptions.IMAGE_ENDPOINT] = panelSelectBlog.SelectedBlog.Id;
            this._temporarySettings.HomePageOverrides = optionOverrides;
        }

        #endregion

        #region Weblog Settings Auto Detection

        private void PerformSettingsAutoDetectionSubStepIfNecessary()
        {
            if (this._temporarySettings.TemplateFiles.Length == 0)
            {
                this.PerformSettingsAutoDetectionSubStep();
            }
        }

        private void PerformSettingsAutoDetectionSubStep() =>
            // perform the step
            this.addWizardSubStep(new WizardAutoDetectionStep(
                (IBlogClientUIContext)this,
                this._temporarySettings, new NextCallback(this.OnPerformSettingsAutoDetectionCompleted),
                new WizardSettingsAutoDetectionOperation(this._editWithStyleStep)));

        private void OnPerformSettingsAutoDetectionCompleted(object stepControl) => this.PerformSelectImageEndpointIfNecessary();

        #endregion

        #region Confirmation Panel

        void OnConfirmationDisplayed(Object stepControl)
        {
            // get reference to panel
            var panelConfirmation = stepControl as WeblogConfigurationWizardPanelConfirmation;

            // show the panel
            panelConfirmation.ShowPanel(this._temporarySettings, this._preventSwitchingToWeblog);
        }

        void OnConfirmationCompleted(Object stepControl)
        {
            // get reference to panel
            var panelConfirmation = stepControl as WeblogConfigurationWizardPanelConfirmation;

            // save settings
            this._temporarySettings.BlogName = panelConfirmation.WeblogName;
            this._temporarySettings.SwitchToWeblog = panelConfirmation.SwitchToWeblog;
        }

        #endregion

        #region Generic Helpers

        private bool OnValidatePanel(Object panelControl)
        {
            var wizardPanel = panelControl as WeblogConfigurationWizardPanel;
            return wizardPanel.ValidatePanel();
        }

        /// <summary>
        /// Clear any cached credential information for the blog
        /// </summary>
        private void ClearTransientCredentials() =>
            //clear any cached credential information associated with this blog (fixes bug 373063)
            new BlogCredentialsAccessor(this._temporarySettings.Id, this._temporarySettings.Credentials).TransientCredentials = null;

        #endregion

        #region Private Members

        private IWin32Window _owner = null;
        private WeblogConfigurationWizard _wizardForm;
        private TemporaryBlogSettings _temporarySettings;
        private bool _preventSwitchingToWeblog = false;
        private WizardStep _editWithStyleStep = null;
        private IBlogProviderAccountWizardDescription _providerAccountWizard;
        private bool _authenticationRequired = false;
        private bool _authenticationStepAdded;

        #endregion

        #region IBlogClientUIContext Members

        IntPtr IWin32Window.Handle { get { return this._wizardForm.Handle; } }
        bool ISynchronizeInvoke.InvokeRequired { get { return this._wizardForm.InvokeRequired; } }
        IAsyncResult ISynchronizeInvoke.BeginInvoke(Delegate method, object[] args) => this._wizardForm.BeginInvoke(method, args);
        object ISynchronizeInvoke.EndInvoke(IAsyncResult result) => this._wizardForm.EndInvoke(result);
        object ISynchronizeInvoke.Invoke(Delegate method, object[] args) => this._wizardForm.Invoke(method, args);

        #endregion

    }

    internal interface IWizardPanelStaticSite
    {
        /// <summary>
        /// Validate the relevant parts of the Static Site Config, raising an exception if the configuration is invalid.
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        void ValidateWithConfig(StaticSiteConfig config);

        /// <summary>
        /// Saves panel form fields into a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        void SaveToConfig(StaticSiteConfig config);

        /// <summary>
        /// Loads panel form fields from a StaticSiteConfig
        /// </summary>
        /// <param name="config">a StaticSiteConfig instance</param>
        void LoadFromConfig(StaticSiteConfig config);
    }

    internal interface IAccountBasicInfoProvider
    {
        IBlogProviderAccountWizardDescription ProviderAccountWizard { set; }
        string AccountId { set; }
        string HomepageUrl { get; set; }
        bool SavePassword { get; set; }
        IBlogCredentials Credentials { get; set; }
        bool ForceManualConfiguration { get; set; }
        bool IsDirty(TemporaryBlogSettings settings);
        BlogInfo BlogAccount { get; }
    }
}
