// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove obsolete code.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Forms;
    using ApplicationFramework;
    using BlogClient;
    using Commands;
    using Configuration.Settings;
    using Localization;

    /// <summary>
    /// The WeblogCommandManager class.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IDynamicCommandMenuContext" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.IDynamicCommandMenuContext" />
    /// <seealso cref="System.IDisposable" />
    internal partial class WeblogCommandManager : IDynamicCommandMenuContext, IDisposable
    {
        /// <summary>
        /// The components
        /// </summary>
        private readonly IContainer components = new Container();

        /// <summary>
        /// The editing site
        /// </summary>
        private readonly IBlogPostEditingSite editingSite;

        /// <summary>
        /// The command add weblog
        /// </summary>
        private Command commandAddWeblog;

        /// <summary>
        /// The command configure weblog
        /// </summary>
        private Command commandConfigureWeblog;

        /// <summary>
        /// The command select blog
        /// </summary>
        private SelectBlogGalleryCommand commandSelectBlog;

        /// <summary>
        /// The command weblog picker
        /// </summary>
        private CommandWeblogPicker commandWeblogPicker;

        /// <summary>
        /// The editing manager
        /// </summary>
        private BlogPostEditingManager editingManager;

        /// <summary>
        /// The options
        /// </summary>
        private DynamicCommandMenuOptions options;

        /// <summary>
        /// The switch weblog command menu
        /// </summary>
        private DynamicCommandMenu switchWeblogCommandMenu;

        /// <summary>
        /// Initializes a new instance of the <see cref="WeblogCommandManager"/> class.
        /// </summary>
        /// <param name="editingManager">The editing manager.</param>
        /// <param name="editingSite">The editing site.</param>
        public WeblogCommandManager(BlogPostEditingManager editingManager, IBlogPostEditingSite editingSite)
        {
            // save reference to editing context and subscribe to blog-changed event
            this.editingManager = editingManager;
            this.editingManager.BlogChanged += this._editingManager_BlogChanged;
            this.editingManager.BlogSettingsChanged += this._editingManager_BlogSettingsChanged;

            this.editingSite = editingSite;

            BlogSettings.BlogSettingsDeleted += this.BlogSettings_BlogSettingsDeleted;

            // initialize commands
            this.InitializeCommands();

            // initialize UI
            this.InitializeUI();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.editingManager != null)
            {
                this.editingManager.BlogChanged -= this._editingManager_BlogChanged;
                this.editingManager = null;

                BlogSettings.BlogSettingsDeleted -= this.BlogSettings_BlogSettingsDeleted;
            }

            this.switchWeblogCommandMenu.Dispose();

            this.components?.Dispose();
        }

        /// <inheritdoc />
        public CommandManager CommandManager => this.editingSite.CommandManager;

        /// <inheritdoc />
        DynamicCommandMenuOptions IDynamicCommandMenuContext.Options =>
            this.options ?? (this.options = new DynamicCommandMenuOptions(
                                 new Command(CommandId.ViewWeblog).MainMenuPath.Split('/')[0],
                                 100,
                                 Res.Get(StringId.MoreWeblogs),
                                 Res.Get(StringId.SwitchWeblogs))
                             {
                                 UseNumericMnemonics = false,
                                 MaxCommandsShownOnMenu = 25,
                                 SeparatorBegin = true
                             });

        /// <inheritdoc />
        IMenuCommandObject[] IDynamicCommandMenuContext.GetMenuCommandObjects() =>

            // generate an array of command objects for the current list of weblogs
            BlogSettings.GetBlogs(true)
                        .Select(blog => new SwitchWeblogMenuCommand(blog.Id, blog.Id == this.editingManager.BlogId))
                        .Cast<IMenuCommandObject>()
                        .ToArray();

        /// <inheritdoc />
        void IDynamicCommandMenuContext.CommandExecuted(IMenuCommandObject menuCommandObject)
        {
            // get reference to underlying command object
            if (menuCommandObject is SwitchWeblogMenuCommand menuCommand)
            {
                // fire notification to listeners
                this.WeblogSelected?.Invoke(menuCommand.BlogId);
            }
        }

        /// <summary>
        /// Editings the site on weblog list changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="eventArgs">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void EditingSiteOnWeblogListChanged(object sender, EventArgs eventArgs) =>
            this.commandSelectBlog?.ReloadAndInvalidate();

        /// <summary>
        /// Blogs the settings blog settings deleted.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        private void BlogSettings_BlogSettingsDeleted(string blogId) => this.commandSelectBlog.ReloadAndInvalidate();

        /// <summary>
        /// Notification that the user has selected a weblog menu
        /// </summary>
        public event WeblogHandler WeblogSelected;

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.CommandManager.BeginUpdate();

            this.commandWeblogPicker = new CommandWeblogPicker();
            this.editingSite.CommandManager.Add(this.commandWeblogPicker);
            this.editingSite.WeblogListChanged -= this.EditingSiteOnWeblogListChanged;
            this.editingSite.WeblogListChanged += this.EditingSiteOnWeblogListChanged;

            this.commandAddWeblog = new Command(CommandId.AddWeblog);
            this.commandAddWeblog.Execute += this.commandAddWeblog_Execute;
            this.CommandManager.Add(this.commandAddWeblog);

            this.commandConfigureWeblog = new Command(CommandId.ConfigureWeblog);
            this.commandConfigureWeblog.Execute += this.commandConfigureWeblog_Execute;
            this.CommandManager.Add(this.commandConfigureWeblog);

            this.commandSelectBlog = new SelectBlogGalleryCommand(this.editingManager);
            this.commandSelectBlog.ExecuteWithArgs += this.commandSelectBlog_ExecuteWithArgs;
            this.commandSelectBlog.Invalidate();
            this.CommandManager.Add(this.commandSelectBlog);

            this.CommandManager.EndUpdate();

            // create the dynamic menu items that correspond to the available weblogs
            this.switchWeblogCommandMenu = new DynamicCommandMenu(this);
        }

        /// <summary>
        /// Commands the select blog execute with arguments.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        private void commandSelectBlog_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args) =>
            this.WeblogSelected?.Invoke(BlogSettings.GetBlogs(true)[this.commandSelectBlog.SelectedIndex].Id);

        /// <summary>
        /// Initializes the UI.
        /// </summary>
        private void InitializeUI() =>

            // hookup menu definition to command bar button
            this.commandWeblogPicker.CommandBarButtonContextMenuDefinition = this.GetWeblogContextMenuDefinition(false);

        /// <summary>
        /// Gets the weblog context menu definition.
        /// </summary>
        /// <param name="includeAllCommands">if set to <c>true</c> [include all commands].</param>
        /// <returns>CommandContextMenuDefinition.</returns>
        private CommandContextMenuDefinition GetWeblogContextMenuDefinition(bool includeAllCommands)
        {
            // initialize context-menu definition
            var weblogContextMenuDefinition = new CommandContextMenuDefinition(this.components) {CommandBar = true};

            if (includeAllCommands)
            {
                weblogContextMenuDefinition.Entries.Add(CommandId.ViewWeblog, false, false);
                weblogContextMenuDefinition.Entries.Add(CommandId.ViewWeblogAdmin, false, false);
                weblogContextMenuDefinition.Entries.Add(CommandId.ConfigureWeblog, true, true);
            }

            // weblog switching commands
            foreach (var commandIdentifier in this.switchWeblogCommandMenu.CommandIdentifiers)
            {
                weblogContextMenuDefinition.Entries.Add(commandIdentifier, false, false);
            }

            weblogContextMenuDefinition.Entries.Add(CommandId.AddWeblog, true, false);
            return weblogContextMenuDefinition;
        }

        /// <summary>
        /// Handles the BlogChanged event of the _editingManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _editingManager_BlogChanged(object sender, EventArgs e)
        {
            this.commandSelectBlog.Invalidate();
            this.UpdateWeblogPicker();
        }

        /// <summary>
        /// Editings the manager blog settings changed.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="templateChanged">if set to <c>true</c> [template changed].</param>
        private void _editingManager_BlogSettingsChanged(string blogId, bool templateChanged)
        {
            using (var blog = new Blog(blogId))
            {
                // Find the item that is changing.
                var blogItem =
                    this.commandSelectBlog.Items.Find(item => item.Cookie.Equals(blogId, StringComparison.Ordinal));

                if (blogItem == null || blogItem.Label.Equals(SelectBlogGalleryCommand.GetShortenedBlogName(blog.Name),
                                                              StringComparison.Ordinal))
                {
                    // WinLive 43696: The blog settings have changed, but the UI doesn't need to be refreshed. In
                    // order to avoid "Windows 8 Bugs" 43242, we don't want to do a full reload.
                    this.commandSelectBlog.Invalidate();
                }
                else
                {
                    // The blog name has changed so we need to do a full reload to refresh the UI.
                    this.commandSelectBlog.ReloadAndInvalidate();
                }
            }

            this.UpdateWeblogPicker();
        }

        /// <summary>
        /// Updates the weblog picker.
        /// </summary>
        private void UpdateWeblogPicker()
        {
            var c = (Control) this.editingSite;
            var wpbc = new CommandWeblogPicker.WeblogPicker(
                Res.DefaultFont, this.editingManager.BlogImage, this.editingManager.BlogIcon,
                this.editingManager.BlogServiceDisplayName, this.editingManager.BlogName);
            using (var g = c.CreateGraphics())
            {
                wpbc.Layout(g);
                this.commandWeblogPicker.WeblogPickerHelper = wpbc;
            }
        }

        /// <summary>
        /// Handles the Execute event of the commandConfigureWeblog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void commandConfigureWeblog_Execute(object sender, EventArgs e) =>
            this.editingSite.ConfigureWeblog(this.editingManager.BlogId, typeof(AccountPanel));

        /// <summary>
        /// Handles the Execute event of the commandAddWeblog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void commandAddWeblog_Execute(object sender, EventArgs e) => this.editingSite.AddWeblog();
    }
}
