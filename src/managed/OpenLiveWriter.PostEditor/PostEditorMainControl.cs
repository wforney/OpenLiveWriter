// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove obsolete code

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;
    using System.Windows.Forms;

    using OpenLiveWriter.ApplicationFramework;
    using OpenLiveWriter.ApplicationFramework.Skinning;
    using OpenLiveWriter.BlogClient;
    using OpenLiveWriter.BlogClient.Clients;
    using OpenLiveWriter.BlogClient.Detection;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.HtmlEditor;
    using OpenLiveWriter.HtmlEditor.Controls;
    using OpenLiveWriter.HtmlParser.Parser;
    using OpenLiveWriter.HtmlParser.Parser.FormAgent;
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Interop.Com.StructuredStorage;
    using OpenLiveWriter.Interop.Windows;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;
    using OpenLiveWriter.PostEditor.Commands;
    using OpenLiveWriter.PostEditor.Configuration.Settings;
    using OpenLiveWriter.PostEditor.Configuration.Wizard;
    using OpenLiveWriter.PostEditor.JumpList;
    using OpenLiveWriter.PostEditor.OpenPost;
    using OpenLiveWriter.PostEditor.PostHtmlEditing;
    using OpenLiveWriter.PostEditor.SupportingFiles;
    using OpenLiveWriter.SpellChecker;

    using Timer = System.Windows.Forms.Timer;

    internal enum ApplicationMode
    {
        Normal = 0,
        Preview = 1,
        LTR = 2,
        RTL = 3,
        NoPlugins = 4,
        HasPlugins = 5,
        Test = 31
    }

    internal class PostEditorMainControl : UserControl, IFormClosingHandler, IBlogPostEditor, IBlogPostEditingSite, IUIApplication, ISessionHandler
    {
        #region Private Data Declarations

        private IMainFrameWindow _mainFrameWindow;
        private BlogPostEditingManager _editingManager;

        private Command commandNewPost;
        private Command commandNewPage;
        private Command commandSavePost;
        private Command commandDeleteDraft;
        private Command commandPostAsDraft;
        private Command commandPostAsDraftAndEditOnline;

        private Command commandColorize;
        private CommandContextMenuDefinition _newPostContextMenuDefinition;
        private CommandContextMenuDefinition _savePostContextMenuDefinition;

        private Panel _mainEditorPanel;
        private HtmlStylePicker _styleComboControl;
        private BlogPostHtmlEditor _htmlEditor;

        private PostEditorPreferencesEditor _optionsEditor;
        private WeblogCommandManager _weblogCommandManager;

        //        private Bitmap statusNewPostBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusComposingPost.png");
        //        private Bitmap statusDraftPostBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusPosted.png");
        //        private Bitmap statusPublishedPostBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.StatusPublished.png");

        private System.Windows.Forms.Timer _autoSaveTimer;
        private System.Windows.Forms.Timer _autoSaveMessageDismissTimer;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = new Container();
        #endregion

        #region Initialization/Disposal

        public PostEditorMainControl(IMainFrameWindow mainFrameWindow, IBlogPostEditingContext editingContext)
        {
            this.Init(mainFrameWindow, editingContext);
        }

        private void Init(IMainFrameWindow mainFrameWindow, IBlogPostEditingContext editingContext)
        {
            // save reference to the frame window and workspace border manager
            this._mainFrameWindow = mainFrameWindow;

            // This call is required by the Windows.Forms Form Designer.
            this.Font = Res.DefaultFont;
            this.InitializeComponent();

            // initialize UI
            this.InitializeUI();

            // Initialize the editing manager
            this.InitializeEditingManager();

            // initialize our commands
            this.InitializeCommands();

            //subscribe to global events
            BlogSettings.BlogSettingsDeleted += new BlogSettings.BlogSettingsListener(this.HandleBlogDeleted);

            // edit the post
            this._editingManager.EditPost(editingContext, false);

            this.InitializeRibbon();
        }

        private void InitializeUI()
        {
            ColorizedResources.Instance.RegisterControlForBackColorUpdates(this);

            // initialize workspace
            this.InitializeWorkspace();

            // initialize the post property editors
            this.InitializePostPropertyEditors();

            // initialize the core editor
            this.InitializeHtmlEditor();

            // initialize options editor
            this.InitializeOptionsEditor();
        }

        private GalleryCommand<string> commandPluginsGallery = null;
        private void InitializeEditingManager()
        {
            this._editingManager = new BlogPostEditingManager(
                this,
                new IBlogPostEditor[] { this._htmlEditor, this },
                this._htmlEditor
                );

            this.commandPluginsGallery = (GalleryCommand<string>)this.CommandManager.Get(CommandId.PluginsGallery);
            this.commandPluginsGallery.StateChanged += new EventHandler(this.commandPluginsGallery_StateChanged);
            this._editingManager.BlogChanged += new EventHandler(this._editingManager_BlogChanged);
            this._editingManager.BlogSettingsChanged += new WeblogSettingsChangedHandler(this._editingManager_BlogSettingsChanged);
            this._editingManager.EditingStatusChanged += new EventHandler(this._editingManager_EditingStatusChanged);
            this._editingManager.UserSavedPost += new EventHandler(this._editingManager_UserSavedPost);
            this._editingManager.UserPublishedPost += new EventHandler(this._editingManager_UserPublishedPost);
            this._editingManager.UserDeletedPost += new EventHandler(this._editingManager_UserDeletedPost);

            // initialize auto-save timer
            this._autoSaveTimer = new System.Windows.Forms.Timer(this.components);
            this._autoSaveTimer.Interval = 5000;
            this._autoSaveTimer.Tick += new EventHandler(this._autoSaveTimer_Tick);

            this._autoSaveMessageDismissTimer = new Timer(this.components);
            this._autoSaveMessageDismissTimer.Interval = 450;
            this._autoSaveMessageDismissTimer.Tick += this._autoSaveMessageDismissTimer_Tick;
        }

        void commandPluginsGallery_StateChanged(object sender, EventArgs e) => this.UpdateRibbonMode();

        private void InitializeCommands()
        {
            this._htmlEditor.CommandManager.BeginUpdate();

            this.commandNewPost = this._htmlEditor.CommandManager.Add(CommandId.NewPost, this.commandNewPost_Execute);
            this.commandNewPage = this._htmlEditor.CommandManager.Add(CommandId.NewPage, this.commandNewPage_Execute);

            // new context menu definition
            this._newPostContextMenuDefinition = new CommandContextMenuDefinition(this.components);
            this._newPostContextMenuDefinition.Entries.Add(CommandId.NewPost, false, false);
            this._newPostContextMenuDefinition.Entries.Add(CommandId.NewPage, false, false);
            this.commandNewPost.CommandBarButtonContextMenuDropDown = true;

            this._htmlEditor.CommandManager.Add(CommandId.OpenDrafts, this.commandOpenDrafts_Execute);
            this._htmlEditor.CommandManager.Add(CommandId.OpenRecentPosts, this.commandOpenRecentPosts_Execute);
            this._htmlEditor.CommandManager.Add(CommandId.OpenPost, this.commandOpenPost_Execute);
            this.commandSavePost = this._htmlEditor.CommandManager.Add(CommandId.SavePost, this.commandSavePost_Execute);
            this.commandDeleteDraft = this._htmlEditor.CommandManager.Add(CommandId.DeleteDraft, this.commandDeleteDraft_Execute);
            this._htmlEditor.CommandManager.Add(CommandId.PostAndPublish, this.commandPostAndPublish_Execute);
            this.commandPostAsDraft = this._htmlEditor.CommandManager.Add(CommandId.PostAsDraft, this.commandPostAsDraft_Execute);
            this.commandPostAsDraftAndEditOnline = this._htmlEditor.CommandManager.Add(CommandId.PostAsDraftAndEditOnline, this.commandPostAsDraftAndEditOnline_Execute);

            var draftGallery = new DraftPostItemsGalleryCommand(this as IBlogPostEditingSite,
                                                                                         this.CommandManager, false);
            draftGallery.Execute += this.commandOpenDrafts_Execute;
            var postGallery = new DraftPostItemsGalleryCommand(this as IBlogPostEditingSite,
                                                                                         this.CommandManager, true);
            postGallery.Execute += this.commandOpenRecentPosts_Execute;

            // publish command bar context menu
            this._savePostContextMenuDefinition = new CommandContextMenuDefinition(this.components);
            this._savePostContextMenuDefinition.CommandBar = true;
            this._savePostContextMenuDefinition.Entries.Add(CommandId.SavePost, false, true);
            this._savePostContextMenuDefinition.Entries.Add(CommandId.PostAsDraft, false, false);
            this._savePostContextMenuDefinition.Entries.Add(CommandId.PostAsDraftAndEditOnline, false, false);
            this.commandSavePost.CommandBarButtonContextMenuDropDown = true;

            if (ApplicationDiagnostics.TestMode)
            {
                this._htmlEditor.CommandManager.Add(CommandId.DiagnosticsConsole, new EventHandler(this.commandDiagnosticsConsole_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ShowBetaExpiredDialogs, new EventHandler(this.commandShowBetaExpiredDialogs_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ShowWebLayoutWarning, delegate
                { new WebLayoutViewWarningForm().ShowDialog(this); });
                this._htmlEditor.CommandManager.Add(CommandId.ShowErrorDialog, new EventHandler(this.commandErrorDialog_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.BlogClientOptions, new EventHandler(this.commandBlogClientOptions_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ShowDisplayMessageTestForm, new EventHandler(this.commandShowDisplayMessageTestForm_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ShowSupportingFilesForm, new EventHandler(this.commandShowSupportingFilesForm_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.InsertLoremIpsum, new EventHandler(this.commandInsertLoremIpsum_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ValidateHtml, new EventHandler(this.commandValidateHtml_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ValidateXhtml, new EventHandler(this.commandValidateXhtml_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ValidateLocalizedResources, new EventHandler(this.commandValidateLocalizedResources_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.ShowAtomImageEndpointSelector, new EventHandler(this.commandShowAtomImageEndpointSelector_Execute));
                this._htmlEditor.CommandManager.Add(CommandId.RaiseAssertion, delegate
                { Trace.Fail("You asked for it"); });
                this._htmlEditor.CommandManager.Add(CommandId.ShowGoogleCaptcha, delegate
                { new GDataCaptchaForm().ShowDialog(this); });
                this._htmlEditor.CommandManager.Add(CommandId.TerminateProcess, delegate
                { Process.GetCurrentProcess().Kill(); });
            }

            this.commandColorize = new Command(CommandId.Colorize);
            this.commandColorize.CommandBarButtonContextMenuHandler = new CommandBarButtonContextMenuHandler(new ColorizationContextHelper().Handler);
            this._htmlEditor.CommandManager.Add(this.commandColorize);

            this._htmlEditor.CommandManager.EndUpdate();

            // initialize the weblog menu commands
            this._weblogCommandManager = new WeblogCommandManager(this._editingManager, this);
            this._weblogCommandManager.WeblogSelected += new WeblogHandler(this._weblogMenuManager_WeblogSelected);
        }

        private void commandShowAtomImageEndpointSelector_Execute(object sender, EventArgs e)
        {
            var controller = new OpenLiveWriter.Controls.Wizard.WizardController();
            var selectBlogControl = new WeblogConfigurationWizardPanelSelectBlog();
            selectBlogControl.HeaderText = Res.Get(StringId.ConfigWizardSelectImageEndpoint);
            selectBlogControl.LabelText = Res.Get(StringId.CWSelectImageEndpointText);
            selectBlogControl.PrepareForAdd();
            controller.addWizardStep(new OpenLiveWriter.Controls.Wizard.WizardStep(selectBlogControl, StringId.ConfigWizardSelectImageEndpoint, null, null, null, null, null));

            using (var form = new OpenLiveWriter.Controls.Wizard.WizardForm(controller))
            {
                form.Size = new Size((int)Math.Ceiling(DisplayHelper.ScaleX(460)), (int)Math.Ceiling(DisplayHelper.ScaleY(400)));
                form.Text = Res.Get(StringId.CWTitle);
                form.ShowDialog(this);
            }
        }

        private void MenuContextHandler(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone)
        {
            Command cmd;
            using (disposeWhenDone)
            {
                var mainMenuItems = new ArrayList(this._htmlEditor.CommandManager.BuildMenu(MenuType.Main));
                mainMenuItems.Add(new OwnerDrawMenuItem(MenuType.Context, "-"));
                var commandShowMenu = this._htmlEditor.CommandManager.Get(CommandId.ShowMenu);
                mainMenuItems.Add(new CommandOwnerDrawMenuItem(
                    MenuType.Context,
                    commandShowMenu,
                    commandShowMenu.Text));

                cmd = CommandContextMenu.ShowModal(this, menuLocation, alternativeLocation, (MenuItem[])mainMenuItems.ToArray(typeof(MenuItem)));
            }
            if (cmd != null)
            {
                cmd.PerformExecute();
            }
        }

        private class ColorizationContextHelper
        {
            private IDisposable _disposeWhenDone;

            public void Handler(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone)
            {
                try
                {
                    var form = new ColorPickerForm();
                    form.Color = ColorizedResources.AppColor;
                    form.ColorSelected += new ColorSelectedEventHandler(this.form_ColorSelected);
                    form.Closed += new EventHandler(this.form_Closed);

                    form.StartPosition = FormStartPosition.Manual;
                    var startLocation = CommandBarButtonLightweightControl.PositionMenu(menuLocation, alternativeLocation, form.Size);
                    form.Location = startLocation;
                    this._disposeWhenDone = disposeWhenDone;
                    var miniFormOwner = parent.FindForm() as IMiniFormOwner;
                    if (miniFormOwner != null)
                    {
                        form.FloatAboveOwner(miniFormOwner);
                    }

                    form.Show();
                }
                catch
                {
                    disposeWhenDone.Dispose();
                    throw;
                }
            }

            private void form_Closed(object sender, EventArgs e) => this._disposeWhenDone.Dispose();

            private void form_ColorSelected(object sender, ColorSelectedEventArgs args) => ColorizedResources.AppColor = args.SelectedColor;
        }

        private void InitializeWorkspace()
        {
            if (!BidiHelper.IsRightToLeft)
            {
                this.DockPadding.Left = 0;
            }
            else
            {
                this.DockPadding.Right = 0;
            }

            this._mainEditorPanel = new Panel();
            this._mainEditorPanel.Dock = DockStyle.Fill;

            //Controls.Add(_publishBar);
            this.Controls.Add(this._mainEditorPanel);
        }

        private static int ToAppMode(ApplicationMode m) => Convert.ToInt32(1 << Convert.ToInt32(m, CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

        private ApplicationMode TextDirection
        {
            get
            {
                return this._htmlEditor.IsRTLTemplate || BidiHelper.IsRightToLeft ? ApplicationMode.RTL : ApplicationMode.LTR;
            }
        }

        private int mode = ToAppMode(ApplicationMode.Normal);
        public bool TestMode
        {
            get
            {
                return Convert.ToBoolean(this.mode & ToAppMode(ApplicationMode.Test));
            }

            set
            {
                if (this.TestMode != value)
                {
                    this.mode ^= ToAppMode(ApplicationMode.Test);
                    this.UpdateRibbonMode();
                }
            }
        }

        public bool PreviewMode
        {
            get
            {
                return Convert.ToBoolean(this.mode & ToAppMode(ApplicationMode.Preview));
            }

            set
            {
                if (this.PreviewMode != value)
                {
                    this.mode ^= ToAppMode(ApplicationMode.Preview);
                    this.mode ^= ToAppMode(ApplicationMode.Normal);
                    Debug.Assert(!(this.PreviewMode && Convert.ToBoolean(this.mode & ToAppMode(ApplicationMode.Normal))));
                    this.UpdateRibbonMode();
                }
            }
        }

        private void UpdateRibbonMode()
        {
            if (this.TextDirection == ApplicationMode.RTL)
            {
                this.mode |= ToAppMode(ApplicationMode.RTL);
                this.mode &= ~ToAppMode(ApplicationMode.LTR);
            }
            else
            {
                this.mode |= ToAppMode(ApplicationMode.LTR);
                this.mode &= ~ToAppMode(ApplicationMode.RTL);
            }

            if (this.commandPluginsGallery.Items.Count > 0)
            {
                this.mode |= ToAppMode(ApplicationMode.HasPlugins);
                this.mode &= ~ToAppMode(ApplicationMode.NoPlugins);
            }
            else
            {
                this.mode |= ToAppMode(ApplicationMode.NoPlugins);
                this.mode &= ~ToAppMode(ApplicationMode.HasPlugins);
            }

            if (this._framework != null)
            {
                this._framework.SetModes(this.mode);
            }
        }

        private void InvalidateCommand(CommandId commandId)
        {
            var command = this.CommandManager.Get(commandId);
            if (command != null)
            {
                command.Invalidate();
            }
        }

        public void OnTestModeChanged(object sender, EventArgs e) => this.TestMode = ApplicationDiagnostics.TestMode;

        [ComImport]
        [Guid("926749fa-2615-4987-8845-c33e65f2b957")]
        public class Framework
        {
        }

        public IUIFramework RibbonFramework
        {
            get
            {
                return this._framework;
            }
        }

        private RibbonControl ribbonControl;
        private IUIFramework _framework;
        private IUIRibbon ribbon;
        private void InitializeRibbon()
        {

            var framework = (IUIFramework)Activator.CreateInstance<Framework>();

            Trace.Assert(framework != null, "Failed to create IUIFramework.");

            this.ribbonControl = new RibbonControl(this._htmlEditor.IHtmlEditorComponentContext, this._htmlEditor);

            var initializeResult = framework.Initialize(this._mainFrameWindow.Handle, this);
            Trace.Assert(initializeResult == HRESULT.S_OK, "Ribbon framework failed to initialize: " + initializeResult);

            this._framework = framework;

            var nativeResourceDLL = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\OpenLiveWriter.Ribbon.dll";
            var hMod = Kernel32.LoadLibrary(nativeResourceDLL);

            using (new QuickTimer("IUIRibbonFramework::LoadUI"))
            {
                var loadResult = this._framework.LoadUI(hMod, "RIBBON_RIBBON");
                Trace.Assert(loadResult == HRESULT.S_OK, "Ribbon failed to load: " + loadResult);
            }

            this._framework.SetModes(this.mode);

            this.CommandManager.Invalidate(CommandId.MRUList);
            this.CommandManager.Invalidate(CommandId.OpenDraftSplit);
            this.CommandManager.Invalidate(CommandId.OpenPostSplit);

            ApplicationDiagnostics.TestModeChanged += this.OnTestModeChanged;
            this.TestMode = ApplicationDiagnostics.TestMode;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if (this._mainEditorPanel != null && this.ribbon != null)
            {
                uint ribbonHeight = 0;
                if (this.ribbon != null)
                {
                    this.ribbon.GetHeight(out ribbonHeight);
                }
                this._mainEditorPanel.DockPadding.Top = (int)ribbonHeight;
            }

            this.Invalidate(false);
            this.Update();
        }

        private void InitializeHtmlEditor()
        {
            // create the editor
            this._htmlEditor = BlogPostHtmlEditor.Create(this._mainFrameWindow, this._mainEditorPanel, this as IBlogPostEditingSite);
            this._htmlEditor.TitleFocusChanged += new EventHandler(this.htmlEditor_TitleFocusChanged);
            this._htmlEditor.Dirty += new EventHandler(this.htmlEditor_Dirty);
            this._htmlEditor.EditorLoaded += new EventHandler(this._htmlEditor_EditingModeChanged);
            this._htmlEditor.DocumentComplete += new EventHandler(this._htmlEditor_DocumentComplete);
        }

        void _htmlEditor_DocumentComplete(object sender, EventArgs e) => this._htmlEditor.FocusBody();

        public void OnKeyboardLanguageChanged()
        {
            //// Sync dictionary language with keyboard language (if enabled)
            //ushort currentLangId = (ushort) (User32.GetKeyboardLayout(0) & 0xFFFF);
            //SpellingLanguageEntry[] langs = SpellingSettings.GetInstalledLanguages();
            //foreach (var v in langs)
            //{
            //    if (v.LCID == currentLangId)
            //    {
            //        SpellingSettings.Language = v.Language;
            //        SpellingSettings.FireChangedEvent();
            //        break;
            //    }
            //}
        }

        void _htmlEditor_EditingModeChanged(object sender, EventArgs e) => this.PreviewMode = this._htmlEditor.CurrentEditingMode == EditingMode.Preview;

        private void htmlEditor_Dirty(object sender, EventArgs e)
        {
            this._autoSaveTimer.Stop();
            if (PostEditorSettings.AutoSaveDrafts)
            {
                this._autoSaveTimer.Start();
            }
        }

        private void InitializeOptionsEditor() => this._optionsEditor = new PostEditorPreferencesEditor(this._mainFrameWindow, this);

        public CommandManager CommandManager
        {
            get
            {
                return this._htmlEditor.CommandManager;
            }
        }

        private class PostContentEditor : IBlogPostContentEditor
        {
            public PostContentEditor(PostEditorMainControl parent) { this._parent = parent; }
            public bool FullyEditableRegionActive { get { return this._parent._htmlEditor.FullyEditableRegionActive; } }
            public string SelectedText { get { return this._parent._htmlEditor.SelectedText; } }
            public string SelectedHtml { get { return this._parent._htmlEditor.SelectedHtml; } }
            public void InsertHtml(string content, bool moveSelectionRight) => this._parent._htmlEditor.InsertHtml(content, moveSelectionRight);
            public void InsertLink(string url, string linkText, string linkTitle, string rel, bool newWindow) => this._parent._htmlEditor.InsertLink(url, linkText, linkTitle, rel, newWindow);
            private PostEditorMainControl _parent;
        }

        private void InitializePostPropertyEditors()
        {
            this._styleComboControl = new HtmlStylePicker(this._htmlEditor);
            this._styleComboControl.Enabled = false;
        }

        internal BlogPostEditingManager BlogPostEditingManager
        {
            get { return this._editingManager; }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this._htmlEditor != null)
                {
                    this._htmlEditor.Dispose();
                }

                if (this._editingManager != null)
                {
                    this._editingManager.Dispose();
                }

                if (this._weblogCommandManager != null)
                {
                    this._weblogCommandManager.Dispose();
                }

                if (this._optionsEditor != null)
                {
                    this._optionsEditor.Dispose();
                }

                if (this.components != null)
                {
                    this.components.Dispose();
                }

                this._framework.Destroy();

                BlogSettings.BlogSettingsDeleted -= new BlogSettings.BlogSettingsListener(this.HandleBlogDeleted);
                this.commandPluginsGallery.StateChanged -= new EventHandler(this.commandPluginsGallery_StateChanged);
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Command Handlers

        private void commandNewPost_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            this._editingManager.NewPost();
        }

        private void commandNewPage_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            this._editingManager.NewPage();
        }

        private void commandOpenDrafts_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            this._editingManager.OpenPost(OpenPostForm.OpenMode.Drafts);
        }

        private void commandOpenRecentPosts_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            this._editingManager.OpenPost(OpenPostForm.OpenMode.RecentPosts);
        }

        private void commandOpenPost_Execute(object sender, EventArgs e)
        {
            WindowCascadeHelper.SetNextOpenedLocation(this._mainFrameWindow.Location);
            this._editingManager.OpenPost(OpenPostForm.OpenMode.Auto);
        }

        private void commandSavePost_Execute(object sender, EventArgs e)
        {
            // save the draft
            using (new PaddedWaitCursor(250))
            {
                this._editingManager.SaveDraft();
            }
        }

        private void commandDeleteDraft_Execute(object sender, EventArgs e) => this._editingManager.DeleteCurrentDraft();

        private void commandPostAsDraft_Execute(object sender, EventArgs e)
        {
            if (this._editingManager.PublishAsDraft())
            {
                // respect close settings
                if (this.CloseWindowOnPublish)
                {
                    this.CloseMainFrameWindow();
                }
            }
        }

        private void commandPostAsDraftAndEditOnline_Execute(object sender, EventArgs e)
        {
            if (this._editingManager.PublishAsDraft())
            {
                // edit post online
                this._editingManager.EditPostOnline(true);

                // respect close settings
                if (this.CloseWindowOnPublish)
                {
                    this.CloseMainFrameWindow();
                }
            }
        }

        private void commandPostAndPublish_Execute(object sender, EventArgs e)
        {
            if (this._editingManager.Publish())
            {
                // respect post after publish
                if (PostEditorSettings.ViewPostAfterPublish)
                {
                    this._editingManager.ViewPost();
                }

                // respect close settings
                if (this.CloseWindowOnPublish)
                {
                    this.CloseMainFrameWindow();
                }
            }
        }

        private void CloseMainFrameWindow() =>
            // WinLive 164570: This function is called from inside a Ribbon execute handler, so we don't want to close
            // the current window (and thereby destroy the Ribbon) while we're still inside a call from the Ribbon.
            // Using BeginInvoke is basically just a wrapper around using User32.PostMessage, which puts the WM.CLOSE
            // message at the end of the message queue.
            this.BeginInvoke(new InvokeInUIThreadDelegate(() => this._mainFrameWindow.Close()), null);

        private void commandDiagnosticsConsole_Execute(object sender, EventArgs e) => ApplicationEnvironment.ApplicationDiagnostics.ShowDiagnosticsConsole("test");

        private void commandShowBetaExpiredDialogs_Execute(object sender, EventArgs e)
        {
            ExpirationForm.ShowExpiredDialog(20);
            ExpirationForm.ShowExpiredDialog(1);
            ExpirationForm.ShowExpiredDialog(-1);
        }

        private void commandErrorDialog_Execute(object sender, EventArgs e) => UnexpectedErrorMessage.Show(new ApplicationException("Force Error Dialog"));

        private void commandBlogClientOptions_Execute(object sender, EventArgs e) => this._editingManager.DisplayBlogClientOptions();

        private void commandShowDisplayMessageTestForm_Execute(object sender, EventArgs e)
        {
            using (var form = new DisplayMessageTestForm())
            {
                form.ShowDialog(this.FindForm());
            }
        }

        private void commandShowSupportingFilesForm_Execute(object sender, EventArgs e) => SupportingFilesForm.ShowForm(this._mainFrameWindow as Form, this.BlogPostEditingManager);

        private void commandValidateLocalizedResources_Execute(object sender, EventArgs e)
        {
            var errors = Res.Validate();
            if (errors.Length > 0)
            {
                Trace.WriteLine("Localized resource validation errors:\r\n" + StringHelper.Join(errors, "\r\n"));
                MessageBox.Show(StringHelper.Join(errors, "\r\n\r\n"), ApplicationEnvironment.ProductNameQualified, MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, (BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0));
            }
            else
            {
                MessageBox.Show("No problems detected.", ApplicationEnvironment.ProductNameQualified, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, (BidiHelper.IsRightToLeft ? (MessageBoxOptions.RightAlign | MessageBoxOptions.RtlReading) : 0));
            }
        }

        private void commandInsertLoremIpsum_Execute(object sender, EventArgs e)
        {
            var loremIpsum = "<p>Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.</p>";
            this._htmlEditor.InsertHtml(loremIpsum, HtmlInsertionOptions.SuppressSpellCheck | HtmlInsertionOptions.MoveCursorAfter);
        }

        private void commandValidateXhtml_Execute(object sender, EventArgs e) => this.ValidateHtml(true);
        private void commandValidateHtml_Execute(object sender, EventArgs e) => this.ValidateHtml(false);

        private void ValidateHtml(bool xhtml)
        {
            const string XHTML_DOCTYPE =
                @"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">";
            const string HTML_DOCTYPE =
                @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"">";
            const string HTML_TEMPLATE = "<html><head><title>Untitled</title></head><body>{0}</body></html>";
            const string XHTML_TEMPLATE = "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\" lang=\"en\"><head><title>Untitled</title></head><body>{0}</body></html>";
            const string VALIDATOR_URL = "http://validator.w3.org/";

            HtmlForm form;
            FormData data;

            var html = string.Format(CultureInfo.InvariantCulture, xhtml ? XHTML_TEMPLATE : HTML_TEMPLATE, this._htmlEditor.Body);
            if (xhtml)
            {
                html = XHTML_DOCTYPE + html;
            }
            else
            {
                html = HTML_DOCTYPE + html;
            }

            var response = HttpRequestHelper.SendRequest(VALIDATOR_URL);
            try
            {
                using (var s = response.GetResponseStream())
                {
                    var formFactory = new FormFactory(s);
                    formFactory.NextForm();
                    formFactory.NextForm();
                    form = formFactory.NextForm();
                    var textarea = form.GetElementByIndex(0) as Textarea;
                    if (textarea == null)
                    {
                        throw new ArgumentException("Unexpected HTML: textarea element not found");
                    }

                    textarea.Value = html;
                    data = form.Submit(null);
                }
            }
            finally
            {
                response.Close();
            }

            using (var formData = data.ToStream())
            {
                var request = HttpRequestHelper.CreateHttpWebRequest(UrlHelper.EscapeRelativeURL(UrlHelper.SafeToAbsoluteUri(response.ResponseUri), form.Action), false);
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = formData.Length;
                request.Method = form.Method.ToUpper(CultureInfo.InvariantCulture);
                using (var requestStream = request.GetRequestStream())
                {
                    StreamHelper.Transfer(formData, requestStream);
                }

                var response2 = (HttpWebResponse)request.GetResponse();
                try
                {
                    using (var s2 = response2.GetResponseStream())
                    {
                        var resultsHtml = StreamHelper.AsString(s2, Encoding.UTF8);
                        resultsHtml = resultsHtml.Replace("<head>",
                                                          string.Format(CultureInfo.InvariantCulture, "<head><base href=\"{0}\"/>",
                                                                        HtmlUtils.EscapeEntities(UrlHelper.SafeToAbsoluteUri(response2.ResponseUri))));

                        var tempFile = TempFileManager.Instance.CreateTempFile("results.htm");
                        using (Stream fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            var bytes = Encoding.UTF8.GetBytes(resultsHtml);
                            fileStream.Write(bytes, 0, bytes.Length);
                        }
                        ShellHelper.LaunchUrl(tempFile);
                    }
                }
                finally
                {
                    response2.Close();
                }
            }
        }

        private bool CloseWindowOnPublish
        {
            get
            {
                return PostEditorSettings.CloseWindowOnPublish;
            }
        }

        private bool ValidateTitleSpecified()
        {

            if (this._htmlEditor.Title == string.Empty)
            {
                if (this._editingManager.BlogRequiresTitles)
                {
                    // show error
                    DisplayMessage.Show(MessageId.NoTitleSpecified, this.FindForm());

                    // focus the title and return false
                    this._htmlEditor.FocusTitle();
                    return false;
                }
                else if (PostEditorSettings.TitleReminder)
                {
                    using (var titleReminderForm = new TitleReminderForm(this._editingManager.EditingPage))
                    {
                        if (titleReminderForm.ShowDialog(this.FindForm()) != DialogResult.Yes)
                        {
                            this._htmlEditor.FocusTitle();
                            return false;
                        }
                    }
                }
            }

            // got this far so the title must be valid
            return true;
        }

        private bool CheckSpelling()
        {
            // do auto spell check
            if (SpellingSettings.CheckSpellingBeforePublish && this._htmlEditor.CanSpellCheck)
            {
                if (!this._htmlEditor.CheckSpelling())
                {
                    return (DialogResult.Yes == DisplayMessage.Show(MessageId.SpellCheckCancelledStillPost, this._mainFrameWindow));
                }
            }
            return true;
        }

        #endregion

        #region Event handlers for UI state management

        /// <summary>
        /// Calling with null sender will cause a forced auto save if the post is dirty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _autoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (!this._htmlEditor.SuspendAutoSave)
            {
                this._autoSaveTimer.Stop();
                var forceSave = sender == null && this._editingManager.PostIsDirty;
                if (forceSave || this._editingManager.ShouldAutoSave)
                {
                    this._htmlEditor.StatusBar.PushStatusMessage(Res.Get(StringId.StatusAutoSaving));
                    try
                    {
                        this._editingManager.AutoSaveIfRequired(forceSave);
                    }
                    catch
                    {
                        this._htmlEditor.StatusBar.PopStatusMessage();
                        throw;
                    }
                    this._autoSaveMessageDismissTimer.Start();
                }
            }
        }

        private void _autoSaveMessageDismissTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                this._autoSaveMessageDismissTimer.Stop();
                this._htmlEditor.StatusBar.PopStatusMessage();
            }
            catch (Exception ex)
            {
                Trace.Fail(ex.ToString());
            }
        }

        private void _weblogMenuManager_WeblogSelected(string blogId)
        {
            var currentBlogId = this.CurrentBlogId;

            this._editingManager.SwitchBlog(blogId);

            // Only set the editor dirty if we actually switched blogs.
            if (blogId != currentBlogId)
            {
                this._htmlEditor.SetCurrentEditorDirty();
            }
        }

        private void _editingManager_EditingStatusChanged(object sender, EventArgs e)
        {
            this.ManageCommands();
            this.UpdateFrameUI();
        }

        public void NotifyWeblogStylePreviewChanged() => this._htmlEditor.CommandManager.Invalidate(CommandId.SemanticHtmlGallery);

        private void _editingManager_BlogChanged(object sender, EventArgs e)
        {
            if (WeblogChanged != null)
            {
                WeblogChanged(this._editingManager.BlogId);
            }

            this.UpdateRibbonMode();

            this.UpdateFrameUI();
        }

        private void _editingManager_BlogSettingsChanged(string blogId, bool templateChanged)
        {
            if (WeblogSettingsChanged != null)
            {
                WeblogSettingsChanged(blogId, templateChanged);
            }

            this.UpdateRibbonMode();

            this.UpdateFrameUI();
        }

        private void htmlEditor_TitleFocusChanged(object sender, EventArgs e)
        {
            this.UpdateFrameUI();
            this.ribbonControl.ManageCommands();
        }

        void IFormClosingHandler.OnClosing(CancelEventArgs e)
        {
            this._editingManager.Closing(e);

            // if the control IsDirty then see if the user wants to publish their edits
            if (!e.Cancel && this._editingManager.PostIsDirty)
            {
                this._mainFrameWindow.Activate();
                var result = DisplayMessage.Show(MessageId.QueryForUnsavedChanges, this._mainFrameWindow);
                if (result == DialogResult.Yes)
                {
                    using (new WaitCursor())
                    {
                        this._editingManager.SaveDraft();
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        void IFormClosingHandler.OnClosed() => this._editingManager.OnClosed();

        // implement IBlogPostEditor so we can participating in showing/hiding
        // the property editor control
        void IBlogPostEditor.Initialize(IBlogPostEditingContext editingContext, IBlogClientOptions clientOptions) { }
        void IBlogPostEditor.SaveChanges(BlogPost post, BlogPostSaveOptions options) { }
        bool IBlogPostEditor.ValidatePublish()
        {
            if (!this.ValidateTitleSpecified())
            {
                return false;
            }

            if (!this.CheckSpelling())
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        void IBlogPostEditor.OnPublishSucceeded(BlogPost blogPost, PostResult postResult) { }
        bool IBlogPostEditor.IsDirty { get { return false; } }
        void IBlogPostEditor.OnBlogChanged(Blog newBlog)
        {
            this.AdaptToBlog(newBlog);
            this.CheckForServiceUpdates(newBlog);
        }

        void IBlogPostEditor.OnBlogSettingsChanged(bool templateChanged)
        {
            using (var blog = new Blog(this._editingManager.BlogId))
            {
                this.AdaptToBlog(blog);
            }
        }

        private void AdaptToBlog(Blog newBlog)
        {
            // if the blog supports posting to draft or not
            this.commandPostAsDraft.Enabled = newBlog.ClientOptions.SupportsPostAsDraft;
            if (newBlog.ClientOptions.SupportsPostAsDraft)
            {
                this.commandSavePost.CommandBarButtonContextMenuDefinition = this._savePostContextMenuDefinition;
            }
            else
            {
                this.commandSavePost.CommandBarButtonContextMenuDefinition = null;
            }

            // if the blog supports post draft and edit online
            this.commandPostAsDraftAndEditOnline.Enabled = newBlog.ClientOptions.SupportsPostAsDraft && (newBlog.ClientOptions.PostEditingUrl != string.Empty);

            // if the blog supports pages or not
            var enablePages = newBlog.ClientOptions.SupportsPages;
            this.commandNewPage.Enabled = enablePages;
            if (enablePages)
            {
                this.commandNewPost.CommandBarButtonContextMenuDefinition = this._newPostContextMenuDefinition;
            }
            else
            {
                this.commandNewPost.CommandBarButtonContextMenuDefinition = null;
            }
        }

        private void CheckForServiceUpdates(Blog blog)
        {
            if (this._editingManager.BlogIsAutoUpdatable && !ApplicationDiagnostics.SuppressBackgroundRequests)
            {
                var checker = new ServiceUpdateChecker(blog.Id, new WeblogSettingsChangedHandler(FireWeblogSettingsChangedEvent));
                checker.Start();
            }
        }

        /// <summary>
        /// Responds to global blog deletion events.
        /// </summary>
        /// <param name="blogId"></param>
        private void HandleBlogDeleted(string blogId)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new InvokeInUIThreadDelegate(this.HandleMaybeBlogDeleted));
            }
            else
            {
                this.HandleMaybeBlogDeleted();
            }
        }

        /// <summary>
        /// Updates the editor to use a new blog if the current blog has been deleted.
        /// </summary>
        private void HandleMaybeBlogDeleted()
        {
            Debug.Assert(!this.InvokeRequired, "This method must be invoked on the UI thread!");

            // if the current weblog got deleted as part of this operation then reselect the
            // new default weblog
            if (!BlogSettings.BlogIdIsValid(this._editingManager.BlogId))
            {
                this._editingManager.SwitchBlog(BlogSettings.DefaultBlogId);
            }
        }

        #endregion

        #region Private Helper Methods

        private void ManageCommands()
        {
            // Temporary work around for WinLive 51425.
            if (ApplicationDiagnostics.AutomationMode)
            {
                this.commandDeleteDraft.Enabled = true;
            }
            else
            {
                this.commandDeleteDraft.Enabled = this._editingManager.PostIsDraft && this._editingManager.PostIsSaved;
            }
        }

        /// <summary>
        /// Update the title and status bars as appropriate
        /// </summary>
        private void UpdateFrameUI()
        {
            // calculate the text that describes the post
            var title = this._htmlEditor.Title;
            var postDescription = (title != string.Empty) ? title : Res.Get(StringId.Untitled);

            // update frame window
            this._mainFrameWindow.Caption = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.WindowTitleFormat), postDescription, ApplicationEnvironment.ProductNameQualified);

            this.UpdatePostStatusUI();
        }

        private void UpdatePostStatusUI()
        {
            string statusText;
            if (this._editingManager.PostIsDraft || string.IsNullOrEmpty(this._editingManager.BlogPostId))
            {
                var dateSaved = this._editingManager.PostDateSaved ?? DateTime.MinValue;
                statusText = dateSaved != DateTime.MinValue
                                 ? string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.StatusDraftSaved),
                                                   this.FormatUtcDate(dateSaved))
                                 : Res.Get(StringId.StatusDraftUnsaved);
            }
            else
            {
                statusText = string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.StatusPublished), this.FormatUtcDate(this._editingManager.PostDatePublished));
            }

            this._htmlEditor.StatusBar.SetStatusMessage(statusText);
        }

        private string FormatUtcDate(DateTime dateTime)
        {
            var localDateTime = DateTimeHelper.UtcToLocal(dateTime);
            return CultureHelper.GetDateTimeCombinedPattern(localDateTime.ToShortDateString(), localDateTime.ToShortTimeString());
        }

        #endregion

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // PostEditorMainControl
            //
            this.Name = "PostEditorMainControl";
            this.Size = new System.Drawing.Size(499, 276);

        }
        #endregion

        #region IBlogPostEditingSite Members

        IMainFrameWindow IBlogPostEditingSite.FrameWindow
        {
            get
            {
                return this._mainFrameWindow;
            }
        }

        void IBlogPostEditingSite.ConfigureWeblog(string blogId, Type selectedPanel)
        {
            using (new WaitCursor())
            {
                // edit settings
                if (WeblogSettingsManager.EditSettings(this.FindForm(), blogId, selectedPanel))
                {
                    // broadcast event
                    FireWeblogSettingsChangedEvent(blogId, true);
                }
            }
        }

        void IBlogPostEditingSite.ConfigureWeblogFtpUpload(string blogId)
        {
            if (WeblogSettingsManager.EditFtpImageUpload(this.FindForm(), blogId))
            {
                // broadcast event
                FireWeblogSettingsChangedEvent(blogId, false);
            }
        }

        bool IBlogPostEditingSite.UpdateWeblogTemplate(string blogID)
        {
            if (this._editingManager.VerifyBlogCredentials())
            {
                using (var editSettings = new PostHtmlEditingSettings(blogID))
                {
                    using (var settings = BlogSettings.ForBlogId(blogID))
                    {
                        Color? backgroundColor;
                        var templates = BlogEditingTemplateDetector.DetectTemplate(
                            new BlogClientUIContextImpl(this._mainFrameWindow, this._mainFrameWindow),
                            this,
                            settings,
                            !this._editingManager.BlogIsAutoUpdatable,
                            out backgroundColor); // only probe if we do not support auto-update

                        if (templates.Length != 0)
                        {
                            editSettings.EditorTemplateHtmlFiles = templates;
                            if (backgroundColor != null)
                            {
                                var hpo = settings.HomePageOverrides ?? new Dictionary<string, string>();
                                hpo[BlogClientOptions.POST_BODY_BACKGROUND_COLOR] =
                                    backgroundColor.Value.ToArgb().ToString(CultureInfo.InvariantCulture);

                                settings.HomePageOverrides = hpo;
                            }
                            FireWeblogSettingsChangedEvent(blogID, true);
                            return true;
                        }

                    }
                }
            }

            return false;
        }

        void IBlogPostEditingSite.AddWeblog()
        {
            using (new WaitCursor())
            {
                bool switchToWeblog;
                var newBlogId = WeblogConfigurationWizardController.Add(this, true, out switchToWeblog);
                if (newBlogId != null)
                {
                    (this as IBlogPostEditingSite).NotifyWeblogAccountListEdited();
                    if (switchToWeblog)
                    {
                        this._editingManager.SwitchBlog(newBlogId);
                    }
                }
            }
        }

        void IBlogPostEditingSite.NotifyWeblogSettingsChanged(bool templateChanged) => (this as IBlogPostEditingSite).NotifyWeblogSettingsChanged(this._editingManager.BlogId, templateChanged);

        void IBlogPostEditingSite.NotifyWeblogSettingsChanged(string blogId, bool templateChanged) => FireWeblogSettingsChangedEvent(blogId, templateChanged);

        void IBlogPostEditingSite.NotifyWeblogAccountListEdited() => FireWeblogListChangedEvent();

        private void weblogAccountManagementForm_WeblogSettingsEdited(string blogId, bool templateChanged) => FireWeblogSettingsChangedEvent(blogId, templateChanged);

        void IBlogPostEditingSite.OpenLocalPost(PostInfo postInfo) => this._editingManager.OpenLocalPost(postInfo);

        void IBlogPostEditingSite.DeleteLocalPost(PostInfo postInfo)
        {
            try
            {
                this._editingManager.DeleteLocalPost(postInfo);
            }
            catch (Exception ex)
            {
                DisplayableExceptionDisplayForm.Show(this._mainFrameWindow, ex);
            }
        }

        string IBlogPostEditingSite.CurrentAccountId
        {
            get
            {
                if (this._editingManager != null)
                {
                    return this._editingManager.BlogId;
                }
                else
                {
                    return null;
                }
            }
        }

        public event WeblogHandler WeblogChanged;

        public event WeblogSettingsChangedHandler WeblogSettingsChanged;

        public event WeblogSettingsChangedHandler GlobalWeblogSettingsChanged
        {
            add
            {
                RegisterWeblogSettingsChangedListener(this, value);
            }
            remove
            {
                UnregisterWeblogSettingsChangedListener(this, value);
            }
        }

        public event EventHandler WeblogListChanged
        {
            add
            {
                RegisterWeblogListChangedListener(this, value);
            }
            remove
            {
                UnregisterWeblogListChangedListener(this, value);
            }
        }

        public event EventHandler PostListChanged
        {
            add
            {
                RegisterPostListChangedListener(this, value);
            }
            remove
            {
                UnregisterPostListChangedListener(this, value);
            }
        }

        #endregion

        #region Implementation of post list changed event

        private void _editingManager_UserSavedPost(object sender, EventArgs e) => this.FirePostListChangedEvent();

        private void _editingManager_UserPublishedPost(object sender, EventArgs e) => this.FirePostListChangedEvent();

        private void _editingManager_UserDeletedPost(object sender, EventArgs e) => this.FirePostListChangedEvent();

        private static void RegisterPostListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_postListChangedListeners)
            {
                _postListChangedListeners[listener] = controlContext;
            }
        }
        private static void UnregisterPostListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_postListChangedListeners)
            {
                _postListChangedListeners.Remove(listener);
            }
        }

        public void FirePostListChangedEvent()
        {
            try
            {
                // notify listeners of post-list changed
                lock (_postListChangedListeners)
                {
                    // first refresh the post-list cache for high-performance refresh
                    PostListCache.Update();
                    WriterJumpList.Invalidate(this.Handle);

                    this.CommandManager.Invalidate(CommandId.MRUList);
                    this.CommandManager.Invalidate(CommandId.OpenDraftSplit);
                    this.CommandManager.Invalidate(CommandId.OpenPostSplit);

                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _postListChangedListeners)
                    {
                        var control = listener.Value as Control;
                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            control.BeginInvoke(listener.Key as EventHandler, new object[] { control, EventArgs.Empty });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception firing post list changed event: " + ex.ToString());
            }
        }

        private static Hashtable _postListChangedListeners = new Hashtable();

        #endregion

        #region Implementation of weblog settings changed event

        private static void RegisterWeblogSettingsChangedListener(Control controlContext, WeblogSettingsChangedHandler listener)
        {
            lock (_weblogSettingsChangedListeners)
            {
                _weblogSettingsChangedListeners[listener] = controlContext;
            }
        }
        private static void UnregisterWeblogSettingsChangedListener(Control controlContext, WeblogSettingsChangedHandler listener)
        {
            lock (_weblogSettingsChangedListeners)
            {
                _weblogSettingsChangedListeners.Remove(listener);
            }
        }

        private static void RegisterWeblogListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_weblogListChangedListeners)
            {
                _weblogListChangedListeners[listener] = controlContext;
            }
        }
        private static void UnregisterWeblogListChangedListener(Control controlContext, EventHandler listener)
        {
            lock (_weblogListChangedListeners)
            {
                _weblogListChangedListeners.Remove(listener);
            }
        }

        private static void FireWeblogSettingsChangedEvent(string blogId, bool templateChanged)
        {
            try
            {
                // notify listeners of post-list changed
                lock (_weblogSettingsChangedListeners)
                {
                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _weblogSettingsChangedListeners)
                    {
                        var control = listener.Value as Control;

                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            try
                            {
                                control.BeginInvoke(listener.Key as WeblogSettingsChangedHandler, new object[] { blogId, templateChanged });
                            }
                            catch (Exception ex)
                            {
                                Trace.Fail("Unexpected error calling BeginInvoke: " + ex.ToString());
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception firing weblog settings changed event: " + ex.ToString());
            }
        }

        private static Hashtable _weblogSettingsChangedListeners = new Hashtable();

        private static Hashtable _weblogListChangedListeners = new Hashtable();
        private static void FireWeblogListChangedEvent()
        {
            try
            {
                // notify listeners of post-list changed
                lock (_weblogListChangedListeners)
                {
                    // now notify all of the listeners asynchronously
                    foreach (DictionaryEntry listener in _weblogListChangedListeners)
                    {
                        var control = listener.Value as Control;

                        if (ControlHelper.ControlCanHandleInvoke(control))
                        {
                            try
                            {
                                control.BeginInvoke(listener.Key as EventHandler, EventArgs.Empty);
                            }
                            catch (Exception ex)
                            {
                                Trace.Fail("Unexpected error calling BeginInvoke: " + ex.ToString());
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception firing weblog settings changed event: " + ex.ToString());
            }
        }

        #endregion

        #region internal properties

        internal string CurrentBlogId
        {
            get
            {
                return this._editingManager.BlogId;
            }
        }

        #endregion

        #region Focus Methods
        internal IFocusableControl[] GetFocusPanes() => this._htmlEditor.GetFocusablePanes();

        #endregion

        public void OnClosing(CancelEventArgs e)
        {
        }

        public void OnPostClosing(CancelEventArgs e)
        {

        }

        public void OnClosed() { }

        public void OnPostClosed() { }

        #region IBlogPostEditingSite Members

        public IHtmlStylePicker StyleControl
        {
            get { return this._styleComboControl; }
        }

        #endregion

        public int OnViewChanged(uint viewId, CommandTypeID typeID, object view, ViewVerb verb, int uReasonCode)
        {
            if (this.ribbon == null)
            {
                this.ribbon = view as IUIRibbon;
            }

            if (this.ribbon != null)
            {
                switch (verb)
                {
                    case ViewVerb.Create:
                        this.LoadRibbonSettings();
                        break;
                    case ViewVerb.Destroy:
                        break;
                    case ViewVerb.Error:
                        Trace.Fail("Ribbon error: " + uReasonCode);
                        break;
                    case ViewVerb.Size:
                        uint ribbonHeight;
                        if (ComHelper.SUCCEEDED(this.ribbon.GetHeight(out ribbonHeight)))
                        {
                            Debug.Assert(ribbonHeight >= 0);
                            this.OnSizeChanged(EventArgs.Empty);
                        }
                        break;
                    default:
                        Debug.Assert(false, "Unexpected ViewVerb!");
                        break;
                }
            }
            return HRESULT.S_OK;
        }

        /// <summary>
        /// All instances of Writer use the same Ribbon.dat file, so we need to be careful of race conditions.
        /// </summary>
        private static object _ribbonSettingsLock = new object();
        private static bool _ribbonSettingsLoadSaveActive = false;

        public void LoadRibbonSettings()
        {
            lock (_ribbonSettingsLock)
            {
                try
                {
                    this.WithRibbonSettingsIStream(false, false, true, this.ribbon.LoadSettingsFromStream);
                }
                catch (Exception e)
                {
                    Trace.Fail("LoadRibbonSettings failed: " + e);
                }
            }
        }

        public void SaveRibbonSettings()
        {
            lock (_ribbonSettingsLock)
            {
                try
                {
                    this.WithRibbonSettingsIStream(true, true, false, this.ribbon.SaveSettingsToStream);
                }
                catch (Exception e)
                {
                    Trace.Fail("SaveRibbonSettings failed: " + e);
                }
            }
        }

        private DateTime? ribbonSettingsTimestamp;

        private delegate int WithIStreamAction(IStream stream);
        /// <summary>
        ///
        /// </summary>
        /// <param name="create">Create the stream if it doesn't exist, and make it writable.</param>
        /// <param name="onlyIfChanged"></param>
        /// <param name="action"></param>
        private void WithRibbonSettingsIStream(bool create, bool writable, bool onlyIfChanged, WithIStreamAction action)
        {
            // Reentrancy check
            // We could have reentrancy while loading/saving the settings because of CLR/COM pumping messages
            // while pinvoking and marshalling interface pointers.
            // If we are already in the process of saving/loading, then we can't do this action.
            // There is no user bad of skipping this in case of reentrancy for the following reasons:
            //  - If this is a Load and the one before is a Save, then we can skip this since we haven't finished saving to load anything.
            //  - And vice-versa.
            if (_ribbonSettingsLoadSaveActive)
            {
                return;
            }

            try
            {
                // Flag this to prevent us from shooting ourselves on the foot in case of reentrancy
                _ribbonSettingsLoadSaveActive = true;

                var ribbonFile = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "Ribbon.dat");
                var fileInfo = new FileInfo(ribbonFile);
                if (onlyIfChanged && this.ribbonSettingsTimestamp != null && fileInfo.LastWriteTimeUtc == this.ribbonSettingsTimestamp)
                {
                    // We're up-to-date, skip the action
                    return;
                }

                var stream = CreateRibbonIStream(ribbonFile, writable, create);
                if (stream == null)
                {
                    return;
                }

                try
                {
                    var hr = action(stream);
                    if (hr != HRESULT.S_OK)
                    {
                        Trace.Fail("Ribbon state load/save operation failed: 0x" + hr.ToString("X8", CultureInfo.InvariantCulture));
                    }
                }
                catch (Exception)
                {
                    Trace.Fail("WithIStreamAction failed.");
                    throw;
                }
                finally
                {
                    Marshal.ReleaseComObject(stream);
                }

                // on successful completion, save the time
                fileInfo.Refresh();
                this.ribbonSettingsTimestamp = fileInfo.LastWriteTimeUtc;
            }
            catch (Exception)
            {
                Trace.Fail("Ribbon settings failure.  Check the ribbon.dat file.");
                throw;
            }
            finally
            {
                _ribbonSettingsLoadSaveActive = false;
            }
        }

        private static IStream CreateRibbonIStream(string filename, bool writable, bool create)
        {
            try
            {
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, "Creating a {0} ribbon istream for {1}", writable ? "writable" : "readable", filename));
                var mode = writable ? STGM.WRITE : STGM.READ;
                if (create)
                {
                    mode |= STGM.CREATE;
                }

                const int FILE_ATTRIBUTE_NORMAL = 0x00000080;
                var hr = Shlwapi.SHCreateStreamOnFileEx(filename, (int)mode, FILE_ATTRIBUTE_NORMAL, create, IntPtr.Zero, out var stream);
                if (hr != HRESULT.S_OK)
                {
                    Trace.WriteLine("Failed to create ribbon stream for " + filename + ": hr = " + hr.ToString("X8", CultureInfo.InvariantCulture));
                    return null;
                }

                return stream;
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
                return null;
            }
        }

        public int OnCreateUICommand(uint commandId, CommandTypeID typeID, out IUICommandHandler commandHandler)
        {
            commandHandler = this._htmlEditor.CommandManager;
            return HRESULT.S_OK;
        }

        public int OnDestroyUICommand(uint commandId, CommandTypeID typeID, IUICommandHandler commandHandler) => HRESULT.E_NOTIMPL;

        #region Implementation of ISessionHandler

        public void OnEndSession() => this._autoSaveTimer_Tick(null, EventArgs.Empty);

        #endregion
    }
}
