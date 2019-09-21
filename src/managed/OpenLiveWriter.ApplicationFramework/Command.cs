// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Delegate CommandBarButtonContextMenuHandler
    /// </summary>
    /// <param name="parent">The parent.</param>
    /// <param name="menuLocation">The menu location.</param>
    /// <param name="alternativeLocation">The alternative location.</param>
    /// <param name="disposeWhenDone">The dispose when done.</param>
    public delegate void CommandBarButtonContextMenuHandler(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone);

    /// <summary>
    /// Class Command.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.ICommandTextDisplayProperties" />
    /// Implements the <see cref="System.IComparable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.ICommandTextDisplayProperties" />
    /// <seealso cref="System.IComparable" />
    public partial class Command : ICommandTextDisplayProperties, IComparable
    {
        /// <summary>
        /// The command identifier.  Each command must have a unique command identifier.
        /// </summary>
        private string identifier;

        /// <summary>
        /// The name of the command used by accessibility client applications
        /// </summary>
        private string accessibleName;

        /// <summary>
        /// The Shortcut of the command.
        /// </summary>
        private Shortcut shortcut = Shortcut.None;

        /// <summary>
        /// The advanced shortcut
        /// </summary>
        private Keys advancedShortcut = Keys.None;

        /// <summary>
        /// A value indicating whether the command should be visible on a context menu.
        /// </summary>
        private bool visibleOnContextMenu = true;

        /// <summary>
        /// A value indicating whether the command should be visible on a main menu.
        /// </summary>
        private bool visibleOnMainMenu = true;

        /// <summary>
        /// A value indicating whether the command should be visible on a command bar
        /// </summary>
        private bool visibleOnCommandBar = true;

        /// <summary>
        /// Menu bitmap for the latched enabled state.
        /// </summary>
        private Bitmap menuBitmapLatchedEnabled;

        /// <summary>
        /// Command bar button text.
        /// </summary>
        private string commandBarButtonText = string.Empty;

        /// <summary>
        /// Command bar button bitmap for the disabled state.
        /// </summary>
        private Bitmap commandBarButtonBitmapDisabled;

        /// <summary>
        /// Command bar button bitmap for the enabled state.
        /// </summary>
        private Bitmap commandBarButtonBitmapEnabled;

        /// <summary>
        /// Command bar button bitmap for the selected state.
        /// </summary>
        private Bitmap commandBarButtonBitmapSelected;

        /// <summary>
        /// Command bar button bitmap for the pushed state.
        /// </summary>
        private Bitmap commandBarButtonBitmapPushed;

        /// <summary>
        /// The command bar button command context menu definition.
        /// </summary>
        private CommandContextMenuDefinition commandBarButtonContextMenuDefinition;

        /// <summary>
        /// A value indicating whether the Command is on or not.
        /// </summary>
        private bool on = true;

        /// <summary>
        /// A value indicating whether the Command is enabled or not.
        /// </summary>
        protected bool enabled = true;

        /// <summary>
        /// A value indicating whether the Command is latched or not.
        /// </summary>
        private bool latched;

        /// <summary>
        /// The events
        /// </summary>
        private EventHandlerList events;

        /// <summary>
        /// Gets the events.
        /// </summary>
        /// <value>The events.</value>
        private EventHandlerList Events
        {
            get
            {
                if (this.events == null)
                {
                    this.events = new EventHandlerList();
                }

                return this.events;
            }
        }

        /// <summary>
        /// The Execute event key.
        /// </summary>
        private static readonly object ExecuteEventKey = new object();

        /// <summary>
        /// Occurs when the command is executed.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the command is executed.")
        ]
        public event EventHandler Execute
        {
            add
            {
                this.Events.AddHandler(ExecuteEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(ExecuteEventKey, value);
            }
        }

        /// <summary>
        /// The ExecuteWithArgs event key.
        /// </summary>
        private static readonly object ExecuteWithArgsEventKey = new object();

        /// <summary>
        /// Occurs when the command is previewed.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the command is executed with args.")
        ]
        public event ExecuteEventHandler ExecuteWithArgs
        {
            add
            {
                this.Events.AddHandler(ExecuteWithArgsEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(ExecuteWithArgsEventKey, value);
            }
        }

        /// <summary>
        /// The ShowCommandBarButtonContextMenu event key.
        /// </summary>
        private static readonly object ShowCommandBarButtonContextMenuEventKey = new object();

        /// <summary>
        /// Occurs when the CommandBarButtonContextMenu should be shown.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the CommandBarButtonContextMenu should be shown.")
        ]
        public event EventHandler ShowCommandBarButtonContextMenu
        {
            add
            {
                this.Events.AddHandler(ShowCommandBarButtonContextMenuEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(ShowCommandBarButtonContextMenuEventKey, value);
            }
        }

        /// <summary>
        /// The command bar button context menu definition key
        /// </summary>
        private static readonly object CommandBarButtonContextMenuDefinitionKey = new object();
        /// <summary>
        /// Occurs when the CommandBarButtonContextMenuDefinition should be shown.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the CommandBarButtonContextMenuDefinition is set.")
        ]
        public event EventHandler CommandBarButtonContextMenuDefinitionChanged
        {
            add
            {
                this.Events.AddHandler(CommandBarButtonContextMenuDefinitionKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(CommandBarButtonContextMenuDefinitionKey, value);
            }
        }

        /// <summary>
        /// The StateChanged event key.
        /// </summary>
        private static readonly object StateChangedEventKey = new object();

        /// <summary>
        /// Occurs when the command's enabled state changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the command's On, Enabled or Latched properties change.")
        ]
        public event EventHandler StateChanged
        {
            add
            {
                this.Events.AddHandler(StateChangedEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(StateChangedEventKey, value);
            }
        }

        /// <summary>
        /// The VisibleOnMainMenuChanged event key.
        /// </summary>
        private static readonly object VisibleOnMainMenuChangedEventKey = new object();

        /// <summary>
        /// Occurs when the command's VisibleOnMainMenu property changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the command's VisibleOnMainMenu property changes.")
        ]
        public event EventHandler VisibleOnMainMenuChanged
        {
            add
            {
                this.Events.AddHandler(VisibleOnMainMenuChangedEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(VisibleOnMainMenuChangedEventKey, value);
            }
        }

        /// <summary>
        /// The VisibleOnCommandBarChanged event key.
        /// </summary>
        private static readonly object VisibleOnCommandBarChangedEventKey = new object();

        /// <summary>
        /// Occurs when the command's VisibleOnMainMenu property changes.
        /// </summary>
        [
        Category("Property Changed"),
        Description("Occurs when the command's VisibleOnCommandBar property changes.")
        ]
        public event EventHandler VisibleOnCommandBarChanged
        {
            add
            {
                this.Events.AddHandler(VisibleOnCommandBarChangedEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(VisibleOnCommandBarChangedEventKey, value);
            }
        }

        /// <summary>
        /// The VisibleOnContextMenuChanged event key.
        /// </summary>
        private static readonly object VisibleOnContextMenuChangedEventKey = new object();

        /// <summary>
        /// Occurs when the command's VisibleOnContextMenu property changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the command's VisibleOnContextMenu property changes.")
        ]
        public event EventHandler VisibleOnContextMenuChanged
        {
            add
            {
                this.Events.AddHandler(VisibleOnContextMenuChangedEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(VisibleOnContextMenuChangedEventKey, value);
            }
        }

        /// <summary>
        /// The CommandBarButtonTextChanged event key.
        /// </summary>
        private static readonly object CommandBarButtonTextChangedEventKey = new object();

        /// <summary>
        /// Occurs when the CommandBarButtonText property has changed.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the CommandBarButtonText property has changed.")
        ]
        public event EventHandler CommandBarButtonTextChanged
        {
            add
            {
                this.Events.AddHandler(CommandBarButtonTextChangedEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(CommandBarButtonTextChangedEventKey, value);
            }
        }

        /// <summary>
        /// The BeforeShowInMenu event key.
        /// </summary>
        private static readonly object BeforeShowInMenuEventKey = new object();

        /// <summary>
        /// Occurs before the command is shown on a menu.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs before the command is shown in a menu.")
        ]
        public event EventHandler BeforeShowInMenu
        {
            add
            {
                this.Events.AddHandler(BeforeShowInMenuEventKey, value);
            }
            remove
            {
                this.Events.RemoveHandler(BeforeShowInMenuEventKey, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="container">The container.</param>
        public Command(IContainer container) => this.InitializeImageLoaders();

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        public Command()
        {
            this.CommandId = CommandId.None;
            this.InitializeImageLoaders();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="tag">The tag.</param>
        public Command(CommandId commandId, object tag) : this(commandId) => this.Tag = tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        public Command(CommandId commandId)
        {
            this.UpdateInvalidationState(PropertyKeys.Enabled, InvalidationState.Pending);
            this.Identifier = commandId.ToString();
            this.CommandId = commandId;
            this.InitializeImageLoaders();
            this.LoadResources();
        }

        /// <summary>
        /// Gets the property variant.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="Exception">Failed to get PropVariant for " + key</exception>
        public virtual void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.Enabled)
            {
                value.SetBool(this.Enabled);
            }
            else if (key == PropertyKeys.SmallImage)
            {
                RibbonHelper.CreateImagePropVariant(this.SmallImage, out value);
            }
            else if (key == PropertyKeys.SmallHighContrastImage)
            {
                // If in High Contrast White mode or a disabled icon in High Contrast Black, use regular image
                RibbonHelper.CreateImagePropVariant(ApplicationEnvironment.IsHighContrastWhite || (ApplicationEnvironment.IsHighContrastBlack && !this.Enabled) ? this.SmallImage : this.SmallHighContrastImage, out value);
            }
            else if (key == PropertyKeys.LargeImage)
            {
                RibbonHelper.CreateImagePropVariant(this.LargeImage, out value);
            }
            else if (key == PropertyKeys.LargeHighContrastImage)
            {
                // If in High Contrast White mode or a disabled icon in High Contrast Black, use regular image
                RibbonHelper.CreateImagePropVariant(ApplicationEnvironment.IsHighContrastWhite || (ApplicationEnvironment.IsHighContrastBlack && !this.Enabled) ? this.LargeImage : this.LargeHighContrastImage, out value);
            }
            else if (key == PropertyKeys.Label)
            {
                value.SetString(this.LabelTitle ?? String.Empty);
            }
            else if (key == PropertyKeys.LabelDescription)
            {
                value.SetString(this.LabelDescription ?? String.Empty);
            }
            else if (key == PropertyKeys.TooltipTitle)
            {
                // In order to eliminate some duplicate strings in our localization we will
                // return the LabelTitle for the TooltipTitle if the TooltipTitle is missing in the markup.
                value.SetString(this.TooltipTitle ?? (this.LabelTitle ?? String.Empty));
            }
            else if (key == PropertyKeys.TooltipDescription)
            {
                value.SetString(this.TooltipDescription ?? String.Empty);
            }
            else if (key == PropertyKeys.Keytip)
            {
                value.SetString(this.Keytip ?? String.Empty);
            }
            else if (key == PropertyKeys.ContextAvailable)
            {
                value.SetUInt(this.Enabled ? (uint)ContextAvailability.Active : (uint)ContextAvailability.NotAvailable);
            }
            else if (key == PropertyKeys.RepresentativeString)
            {
                var rs = (IRepresentativeString)this;
                value.SetString(rs.RepresentativeString);
            }
            else if (key == PropertyKeys.BooleanValue)
            {
                value.SetBool(this.Latched);
            }
            else
            {
                Trace.Fail("Didn't properly update property for " + key + " on command " + this.CommandId);
                throw new Exception("Failed to get PropVariant for " + key);
            }
        }

        /// <summary>
        /// The command identifier.  Each command must have a unique command identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [
            Category("Design"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the identifier for the command.  Each command must have a unique command identifier.")
        ]
        public string Identifier
        {
            get
            {
                if (this.identifier == null)
                {
                    this.identifier = Guid.NewGuid().ToString();
                }

                return this.identifier;
            }
            set => this.identifier = value;
        }

        /// <summary>
        /// Gets or sets the description of the command used by accessibility client applications.
        /// </summary>
        /// <value>The accessible description.</value>
        [
            Category("Accessibility"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the description that will be reported to accessibility clients.")
        ]
        public string AccessibleDescription { get; set; }

        /// <summary>
        /// Gets or sets the name of the command used by accessibility client applications.
        /// </summary>
        /// <value>The name of the accessible.</value>
        [
            Category("Accessibility"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the name that will be reported to accessibility clients.")
        ]
        public string AccessibleName
        {
            get => this.accessibleName;
            set
            {
                this.accessibleName = value;
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the Shortcut of the command.
        /// </summary>
        /// <value>The shortcut.</value>
        [
            Category("Behavior"),
                Localizable(true),
                DefaultValue(Shortcut.None),
                Description("Specifies the Shortcut of the command.")
        ]
        public Shortcut Shortcut
        {
            get => this.shortcut;
            set
            {
                Debug.Assert(this.shortcut == Shortcut.None, "Changing shortcuts is a bad idea. CommandManager.RebuildCommandShortcutTable() will get stale.");
                this.shortcut = value;
            }
        }

        /// <summary>
        /// For shortcuts that can't be expressed by the Shortcut enum/property.
        /// </summary>
        /// <value>The advanced shortcut.</value>
        [Localizable(false)]
        // There's no reason why these couldn't be localized.  Consider enabling this in the future.  See Commands.xsl.
        //[
        //    Category("Behavior"),
        //        Localizable(true),
        //        DefaultValue(Keys.None),
        //        Description("Specifies the AdvancedShortcut of the command. For shortcuts that can't be expressed by the Shortcut enum.")
        //]
        public Keys AdvancedShortcut
        {
            get => this.advancedShortcut;
            set
            {
                Debug.Assert(this.advancedShortcut == Keys.None,
                             "Changing shortcuts is a bad idea. CommandManager.RebuildCommandShortcutTable() will get stale.");
                this.advancedShortcut = value;
            }
        }

        /// <summary>
        /// Gets or sets the Shortcut of the command.
        /// </summary>
        /// <value>The accelerator mnemonic.</value>
        [
            Category("Behavior"),
                Localizable(true),
                Description("Specifies the AcceleratorMnemonic of the command.")
        ]
        public AcceleratorMnemonic AcceleratorMnemonic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command should be visible on a context menu.
        /// </summary>
        /// <value><c>true</c> if [visible on context menu]; otherwise, <c>false</c>.</value>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the command should be visible on a context menu.")
        ]
        public bool VisibleOnContextMenu
        {
            get => this.visibleOnContextMenu;
            set
            {
                if (this.visibleOnContextMenu != value)
                {
                    this.visibleOnContextMenu = value;
                    this.OnVisibleOnContextMenuChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command should be visible on a main menu.
        /// </summary>
        /// <value><c>true</c> if [visible on main menu]; otherwise, <c>false</c>.</value>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the command should be visible on a main menu.")
        ]
        public bool VisibleOnMainMenu
        {
            get => this.visibleOnMainMenu;
            set
            {
                if (this.visibleOnMainMenu != value)
                {
                    this.visibleOnMainMenu = value;
                    this.OnVisibleOnMainMenuChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command should be visible on a command bar
        /// </summary>
        /// <value><c>true</c> if [visible on command bar]; otherwise, <c>false</c>.</value>
        [
        Category("Appearance.Menu"),
        Localizable(false),
        DefaultValue(true),
        Description("Specifies whether the command should be visible on a command bar.")
        ]
        public bool VisibleOnCommandBar
        {
            get => this.visibleOnCommandBar;
            set
            {
                if (this.visibleOnCommandBar != value)
                {
                    this.visibleOnCommandBar = value;
                    this.OnVisibleOnCommandBarChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets A value indicating whether the Shortcut of the command should be shown when the
        /// command appears on a menu.
        /// </summary>
        /// <value><c>true</c> if [show shortcut]; otherwise, <c>false</c>.</value>
        [
            Category("Appearance.MainMenu"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the Shortcut of the command should be shown when the command appears on a main menu.")
        ]
        public bool ShowShortcut { get; set; } = true;

        /// <summary>
        /// Gets or sets the main menu path of the command.
        /// </summary>
        /// <value>The menu text.</value>
        [
            Category("Appearance.Menu"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the menu text of the command when it appears on a menu.")
        ]
        public string MenuText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the menu format arguments.
        /// </summary>
        /// <value>The menu format arguments.</value>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public object[] MenuFormatArgs { get; set; }

        /// <summary>
        /// Gets or sets the main menu path of the command.
        /// </summary>
        /// <value>The main menu path.</value>
        [
            Category("Appearance.MainMenu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu path of the command when it appears on a main menu.  Include the menu merge order after each path entry.  Example: 'File@0/[-]New@1'.")
        ]
        public string MainMenuPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the menu bitmap for the disabled state.
        /// </summary>
        /// <value>The menu bitmap disabled.</value>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the disabled state.")
        ]
        public Bitmap MenuBitmapDisabled => this.SuppressMenuBitmap ? null : this.CommandBarButtonBitmapDisabled;

        /// <summary>
        /// Gets or sets the menu bitmap for the enabled state.
        /// </summary>
        /// <value>The menu bitmap enabled.</value>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the enabled state.")
        ]
        public Bitmap MenuBitmapEnabled => this.SuppressMenuBitmap ? null : this.CommandBarButtonBitmapEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether [suppress menu bitmap].
        /// </summary>
        /// <value><c>true</c> if [suppress menu bitmap]; otherwise, <c>false</c>.</value>
        [Localizable(false)]
        public bool SuppressMenuBitmap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [suppress command bar bitmap].
        /// </summary>
        /// <value><c>true</c> if [suppress command bar bitmap]; otherwise, <c>false</c>.</value>
        [Localizable(false)]
        public bool SuppressCommandBarBitmap { get; set; } = false;

        /// <summary>
        /// Gets or sets the menu bitmap for the latched enabled state.
        /// </summary>
        /// <value>The menu bitmap latched enabled.</value>
        [
            Category("Appearance.Latched"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the latched enabled state.")
        ]
        public Bitmap MenuBitmapLatchedEnabled
        {
            get
            {
                //	Attempt to load a custom bitmap for the command.
                this.BitmapProperty("MenuBitmapLatchedEnabled", ref this.menuBitmapLatchedEnabled);

                //	If there was no custom bitmap for the command, load the stock one.
                if (this.menuBitmapLatchedEnabled == null)
                {
                    this.menuBitmapLatchedEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.CommandMenuBitmapLatchedEnabled.png");
                }

                //	Done.
                return this.menuBitmapLatchedEnabled;
            }
            set => this.menuBitmapLatchedEnabled = value;
        }

        /// <summary>
        /// Gets or sets the menu bitmap for the latched selected state.
        /// </summary>
        /// <value>The menu bitmap latched selected.</value>
        [
            Category("Appearance.Latched"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the latched selected state.")
        ]
        public Bitmap MenuBitmapLatchedSelected => this.MenuBitmapLatchedEnabled;

        /// <summary>
        /// Gets or sets the menu selected bitmap.
        /// </summary>
        /// <value>The menu bitmap selected.</value>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the selected state.")
        ]
        public Bitmap MenuBitmapSelected => this.SuppressMenuBitmap ? null : this.CommandBarButtonBitmapSelected;

        /// <summary>
        /// Gets or sets the command bar button style.
        /// </summary>
        /// <value>The command bar button style.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(CommandBarButtonStyle.System),
                Description("Specifies the command bar button style.")
        ]
        public CommandBarButtonStyle CommandBarButtonStyle { get; set; } = CommandBarButtonStyle.System;

        /// <summary>
        /// Gets or sets the command bar button bar button text.
        /// </summary>
        /// <value>The command bar button text.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button text."),
        ]
        public string CommandBarButtonText
        {
            get => this.commandBarButtonText;
            set
            {
                if (this.commandBarButtonText != value)
                {
                    //	Set the text.
                    this.commandBarButtonText = value;

                    //	Fire the CommandBarButtonTextChanged event.
                    this.OnCommandBarButtonTextChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the command bar button bitmap for the disabled state.
        /// </summary>
        /// <value>The command bar button bitmap disabled.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button bitmap for the disabled state.")
        ]
        public Bitmap CommandBarButtonBitmapDisabled
        {
            get
            {
                this.BitmapProperty("CommandBarButtonBitmapDisabled", ref this.commandBarButtonBitmapDisabled);
                if (this.commandBarButtonBitmapDisabled == null && this.CommandBarButtonBitmapEnabled != null)
                {
                    this.commandBarButtonBitmapDisabled = ImageHelper.MakeDisabled(this.commandBarButtonBitmapEnabled);
                }
                return this.commandBarButtonBitmapDisabled;
            }
            set => this.commandBarButtonBitmapDisabled = value;
        }

        /// <summary>
        /// Gets or sets the command bar button bitmap for the enabled state.
        /// </summary>
        /// <value>The command bar button bitmap enabled.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button bitmap for the enabled state.")
        ]
        public Bitmap CommandBarButtonBitmapEnabled
        {
            get => this.BitmapProperty("CommandBarButtonBitmapEnabled", ref this.commandBarButtonBitmapEnabled);
            set
            {
                // set the value
                this.commandBarButtonBitmapEnabled = value;

                // since other command bar states can be auto-derived from enabled we
                // need to null them out so they can be updated
                this.commandBarButtonBitmapSelected = null;
                this.commandBarButtonBitmapPushed = null;
                this.commandBarButtonBitmapDisabled = null;

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the command bar button bitmap for the selected state.
        /// </summary>
        /// <value>The command bar button bitmap selected.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button bitmap for the selected state.")
        ]
        public Bitmap CommandBarButtonBitmapSelected
        {
            get
            {
                this.BitmapProperty("CommandBarButtonBitmapSelected", ref this.commandBarButtonBitmapSelected);
                if (this.commandBarButtonBitmapSelected == null && this.CommandBarButtonBitmapEnabled != null)
                {
                    this.commandBarButtonBitmapSelected = this.commandBarButtonBitmapEnabled;
                }
                return this.commandBarButtonBitmapSelected;
            }
            set => this.commandBarButtonBitmapSelected = value;
        }

        /// <summary>
        /// Gets or sets the command bar button bitmap for the selected state.
        /// </summary>
        /// <value>The command bar button bitmap pushed.</value>
        [
            Category("Appearance.CommandBar.System"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button bitmap for the pushed state.")
        ]
        public Bitmap CommandBarButtonBitmapPushed
        {
            get
            {
                this.BitmapProperty("CommandBarButtonBitmapPushed", ref this.commandBarButtonBitmapPushed);
                if (this.commandBarButtonBitmapPushed == null && this.CommandBarButtonBitmapEnabled != null)
                {
                    this.commandBarButtonBitmapPushed = this.commandBarButtonBitmapEnabled;
                }
                return this.commandBarButtonBitmapPushed;
            }
            set => this.commandBarButtonBitmapPushed = value;
        }

        /// <summary>
        /// Gets or sets the AcceleratorMnemonic that will show the CommandBarContextMenu.
        /// </summary>
        /// <value>The command bar button context menu accelerator mnemonic.</value>
        [
            Category("Behavior"),
            Description("Not normally used!  Specifies the the AcceleratorMnemonic that will show the CommandBarButtonContextMenu of the Command.  This value is only useful under certain conditions.")
        ]
        public AcceleratorMnemonic CommandBarButtonContextMenuAcceleratorMnemonic { get; set; }

        /// <summary>
        /// Gets or sets the command bar button context menu.
        /// </summary>
        /// <value>The command bar button context menu.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the command bar button context menu.")
        ]
        public CommandContextMenu CommandBarButtonContextMenu { get; set; }

        /// <summary>
        /// Gets or sets the command bar button context menu.
        /// </summary>
        /// <value>The command bar button context menu definition.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the command bar button context menu definition.")
        ]
        public CommandContextMenuDefinition CommandBarButtonContextMenuDefinition
        {
            get => this.commandBarButtonContextMenuDefinition;
            set
            {
                this.commandBarButtonContextMenuDefinition = value;
                this.RaiseEvent(CommandBarButtonContextMenuDefinitionKey, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the command bar button mini form factory
        /// </summary>
        /// <value>The command bar button context menu control handler.</value>
        public ICommandContextMenuControlHandler CommandBarButtonContextMenuControlHandler { get; set; } = null;

        /// <summary>
        /// Gets or sets the command bar button context menu handler.
        /// </summary>
        /// <value>The command bar button context menu handler.</value>
        public CommandBarButtonContextMenuHandler CommandBarButtonContextMenuHandler { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command bar context menu is accessed
        /// through a separate dropdown control.
        /// </summary>
        /// <value><c>true</c> if [command bar button context menu drop down]; otherwise, <c>false</c>.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies whether the the command bar button context menu is accessed through a separate dropdown control.")
        ]
        public bool CommandBarButtonContextMenuDropDown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the command bar context menu is accessed
        /// through a separate dropdown control.
        /// </summary>
        /// <value><c>true</c> if [command bar button context menu invalidate parent]; otherwise, <c>false</c>.</value>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(false),
                Description("Specifies whether the parent should be invalidated after showing the context menu (used for IE ToolBand).")
        ]
        public bool CommandBarButtonContextMenuInvalidateParent { get; set; } = false;

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        /// <value>The text.</value>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the text that is associated with the command.")
        ]
        public string Text { get; set; }

        /// <summary>
        /// The large image
        /// </summary>
        private LazyLoader<Bitmap> largeImage;
        /// <summary>
        /// Gets or sets the command LargeImage.
        /// </summary>
        /// <value>The large image.</value>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the large image that is associated with the command.")
        ]
        public virtual Bitmap LargeImage
        {
            get => this.largeImage;
            set
            {
                // set the value
                this.largeImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.LargeImage, InvalidationState.Pending);
                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The large high contrast image
        /// </summary>
        private LazyLoader<Bitmap> largeHighContrastImage;
        /// <summary>
        /// Gets or sets the command LargeImage.
        /// </summary>
        /// <value>The large high contrast image.</value>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the large image that is associated with the command.")
        ]
        public virtual Bitmap LargeHighContrastImage
        {
            get => this.largeHighContrastImage;
            set
            {
                // set the value
                this.largeHighContrastImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.LargeHighContrastImage, InvalidationState.Pending);

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The small image
        /// </summary>
        private LazyLoader<Bitmap> smallImage;
        /// <summary>
        /// Gets or sets the command SmallImage.
        /// </summary>
        /// <value>The small image.</value>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the small image that is associated with the command.")
        ]
        public virtual Bitmap SmallImage
        {
            get => this.smallImage;
            set
            {
                // set the value
                this.smallImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.SmallImage, InvalidationState.Pending);

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The small high contrast image
        /// </summary>
        private LazyLoader<Bitmap> smallHighContrastImage;

        /// <summary>
        /// Gets or sets the invalidation count.
        /// </summary>
        /// <value>The invalidation count.</value>
        public int InvalidationCount { get; set; }

        /// <summary>
        /// Gets or sets the command SmallHighContrastImage.
        /// </summary>
        /// <value>The small high contrast image.</value>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the small image that is associated with the command.")
        ]
        public virtual Bitmap SmallHighContrastImage
        {
            get =>
                // You'll need to take dpi into consideration as well.
                this.smallHighContrastImage;
            set
            {
                // set the value
                this.smallHighContrastImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.SmallHighContrastImage, InvalidationState.Pending);

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// The label title
        /// </summary>
        private string labelTitle;
        /// <summary>
        /// Gets or sets the command description.
        /// </summary>
        /// <value>The label title.</value>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the label title for the command.")
        ]
        public virtual string LabelTitle
        {
            get => this.labelTitle;

            set
            {
                if (this.labelTitle != value)
                {
                    this.labelTitle = TextHelper.UnescapeNewlines(value);

                    this.UpdateInvalidationState(PropertyKeys.Label, InvalidationState.Pending);

                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The label description
        /// </summary>
        private string labelDescription;
        /// <summary>
        /// Gets or sets the command label title.
        /// </summary>
        /// <value>The label description.</value>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the label description for the command.")
        ]
        public string LabelDescription
        {
            get => this.labelDescription;

            set
            {
                if (this.labelDescription != value)
                {
                    this.labelDescription = value;
                    this.UpdateInvalidationState(PropertyKeys.LabelDescription, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tooltip description
        /// </summary>
        private string tooltipDescription;
        /// <summary>
        /// Gets or sets the tooltip description.
        /// </summary>
        /// <value>The tooltip description.</value>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the tooltip description for the command.")
        ]
        public virtual string TooltipDescription
        {
            get => this.tooltipDescription;
            set
            {
                if (this.tooltipDescription != value)
                {
                    this.tooltipDescription = value;
                    this.UpdateInvalidationState(PropertyKeys.TooltipDescription, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the keytip.
        /// </summary>
        /// <value>The keytip.</value>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the keytip for the command.")
        ]
        public string Keytip { get; set; }

        /// <summary>
        /// The tooltip title
        /// </summary>
        private string tooltipTitle;
        /// <summary>
        /// Gets or sets the tooltip title.
        /// </summary>
        /// <value>The tooltip title.</value>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the description for the command.")
        ]
        public string TooltipTitle
        {
            get => this.tooltipTitle;
            set
            {
                if (this.tooltipTitle != value)
                {
                    this.tooltipTitle = value;
                    this.UpdateInvalidationState(PropertyKeys.TooltipTitle, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the command tag.
        /// </summary>
        /// <value>The tag.</value>
        [
            Category("Behavior"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the tag that is associated with the command.")
        ]
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Command is on or not.
        /// </summary>
        /// <value><c>true</c> if on; otherwise, <c>false</c>.</value>
        [
            Category("Behavior"),
                DefaultValue(true),
                Description("Specifies the initial value of the On property.")
        ]
        public bool On
        {
            get => this.on;
            set
            {
                if (this.on != value)
                {
                    //	Set the value.
                    this.on = value;

                    //	Fire the StateChanged event.
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Command is enabled or not.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [
            Category("Behavior"),
                DefaultValue(true),
                Description("Specifies the initial value of the Enabled property.")
        ]
        public virtual bool Enabled
        {
            get => this.enabled;
            set
            {
                if (this.enabled != value)
                {
                    //	Set the value.
                    this.enabled = value;

                    this.UpdateInvalidationState(PropertyKeys.Enabled, InvalidationState.Pending);

                    //	Fire the state changed event.
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets A value indicating whether the Command is latched or not.
        /// </summary>
        /// <value><c>true</c> if latched; otherwise, <c>false</c>.</value>
        [
            Category("Behavior"),
                DefaultValue(false),
                Description("Specifies the initial value of the Latched property.")
        ]
        public bool Latched
        {
            get => this.latched;
            set
            {
                if (this.latched != value)
                {
                    //	Set the value.
                    this.latched = value;
                    this.UpdateInvalidationState(PropertyKeys.BooleanValue, InvalidationState.Pending);
                    this.OnStateChanged(null);
                }
            }
        }

        /// <summary>
        /// Loads the resources.
        /// </summary>
        public void LoadResources() => CommandResourceLoader.ApplyResources(this);

        /// <summary>
        /// Initializes the image loaders.
        /// </summary>
        private void InitializeImageLoaders()
        {
            this.largeImage = new LazyLoader<Bitmap>(() => CommandResourceLoader.LoadCommandBitmap(this.Identifier, "LargeImage") ?? CommandResourceLoader.MissingLarge);
            this.smallImage = new LazyLoader<Bitmap>(() => CommandResourceLoader.LoadCommandBitmap(this.Identifier, "SmallImage") ?? CommandResourceLoader.MissingSmall);

            this.largeHighContrastImage = new LazyLoader<Bitmap>(() => CommandResourceLoader.LoadCommandBitmap(this.Identifier, "LargeHighContrastImage") ?? this.largeImage);
            this.smallHighContrastImage = new LazyLoader<Bitmap>(() => CommandResourceLoader.LoadCommandBitmap(this.Identifier, "SmallHighContrastImage") ?? this.smallImage);
        }

        /// <summary>
        /// This method can be called to raise the Execute event.
        /// </summary>
        public void PerformExecute()
        {
            if (this.On && this.Enabled)
            {
                this.OnExecute(EventArgs.Empty);
            }
            else
            {
                Debug.Fail("Command state error.", "It is illogical to execute a command that is not on and enabled.");
            }
        }

        /// <summary>
        /// This method can be called to raise the Execute event.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public void PerformExecuteWithArgs(ExecuteEventHandlerArgs args)
        {
            if (this.On && this.Enabled)
            {
                this.OnExecute(args);
            }
            else
            {
                Debug.Fail("Command state error.", "It is illogical to execute a command that is not on and enabled.");
            }
        }

        /// <summary>
        /// Performs the execute with arguments.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <param name="args">The arguments.</param>
        protected virtual void PerformExecuteWithArgs(CommandExecutionVerb verb, ExecuteEventHandlerArgs args)
        {
            if (verb == CommandExecutionVerb.Execute)
            {
                if (this.On && this.Enabled)
                {
                    this.OnExecute(args);
                }
                else
                {
                    Debug.Fail("Command state error.", "It is illogical to execute a command that is not on and enabled.");
                }
            }
            else
            {
                Debug.Fail("Expected execute verb on " + this.CommandId);
            }
        }

        /// <summary>
        /// Performs the execute.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="commandExecutionProperties">The command execution properties.</param>
        /// <returns>System.Int32.</returns>
        public virtual int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key, PropVariantRef currentValue, IUISimplePropertySet commandExecutionProperties)
        {
            switch (verb)
            {
                case CommandExecutionVerb.Execute:
                    this.PerformExecute();
                    break;
                case CommandExecutionVerb.Preview:
                    Debug.Fail("Preview is not implemented for " + this.CommandId);
                    break;
                case CommandExecutionVerb.CancelPreview:
                    Debug.Fail("CancelPreview is not implemented for " + this.CommandId);
                    break;
                default:
                    Debug.Fail("Unexpected CommandExecutionVerb.");
                    break;
            }

            return HRESULT.S_OK;
        }

        /// <summary>
        /// Updates the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>System.Int32.</returns>
        public virtual int UpdateProperty(ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            Debug.Assert(!this.flushing, "UpdateProperty called while flushing pending invalidations!");
            try
            {
                newValue = new PropVariant();
                this.GetPropVariant(key, currentValue, ref newValue);

                // Remove key from dictionary since we have updated it.
                this.pendingInvalidations.Remove(key);

                if (newValue.IsNull())
                {
                    Trace.Fail("Didn't properly update property for " + PropertyKeys.GetName(key) + " on command " + this.CommandId);
                    newValue = new PropVariant();
                }
            }
            catch (Exception ex)
            {
                this.UpdateInvalidationState(key, InvalidationState.Error);
                Trace.Fail("Exception in UpdateProperty for " + PropertyKeys.GetName(key) + " on command " + this.CommandId + ": " + ex);
                newValue = new PropVariant();
            }

            if (newValue.VarType == VarEnum.VT_ERROR)
            {
                // Returning an error tells the Ribbon to ignore this property.
                return HRESULT.E_NOTIMPL;
            }

            return HRESULT.S_OK;
        }

        /// <summary>
        /// This method can be called to raise the ShowCommandBarButtonContextMenu event.
        /// </summary>
        public void PerformShowCommandBarButtonContextMenu() => this.OnShowCommandBarButtonContextMenu(EventArgs.Empty);

        /// <summary>
        /// Raises the BeforeShowInMenu event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        internal void InvokeBeforeShowInMenu(EventArgs e) => this.OnBeforeShowInMenu(e);

        /// <summary>
        /// Raises the CommandBarButtonTextChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnCommandBarButtonTextChanged(EventArgs e) => this.RaiseEvent(CommandBarButtonTextChangedEventKey, e);

        /// <summary>
        /// Raises the Execute event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnExecute(EventArgs e)
        {
            try
            {
                var executed = this.RaiseEvent(ExecuteEventKey, e);
                Debug.Assert(executed || this.Events[ExecuteWithArgsEventKey] == null, "Command " + this.CommandId + " was executed without args, but only arg handlers were registered");
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        /// <summary>
        /// Raises the Execute event.
        /// </summary>
        /// <param name="args">The arguments.</param>
        protected virtual void OnExecute(ExecuteEventHandlerArgs args)
        {
            try
            {
                var executed = this.RaiseEvent(ExecuteWithArgsEventKey, args);
                Debug.Assert(executed || this.Events[ExecuteEventKey] == null, "Command " + this.CommandId + " was executed with args, but only non-arg handlers were registered");
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(ex);
            }
        }

        /// <summary>
        /// Raises the ShowCommandBarButtonContextMenu event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnShowCommandBarButtonContextMenu(EventArgs e) => this.RaiseEvent(ShowCommandBarButtonContextMenuEventKey, e);

        /// <summary>
        /// Invalidates this instance.
        /// </summary>
        public virtual void Invalidate()
        {
            this.UpdateInvalidationState(PropertyKeys.Enabled, InvalidationState.Pending);
            this.OnStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Invalidates the specified keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        public void Invalidate(PropertyKey[] keys)
        {
            foreach (var key in keys)
            {
                this.UpdateInvalidationState(key, InvalidationState.Pending);
            }

            this.OnStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnStateChanged(EventArgs e) => this.RaiseEvent(StateChangedEventKey, e);

        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        /// <value>The command identifier.</value>
        public CommandId CommandId { get; set; }

        /// <summary>
        /// Raises the VisibleOnContextMenuChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVisibleOnContextMenuChanged(EventArgs e) => this.RaiseEvent(VisibleOnContextMenuChangedEventKey, e);

        /// <summary>
        /// Raises the VisibleOnMainMenuChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVisibleOnMainMenuChanged(EventArgs e) => this.RaiseEvent(VisibleOnMainMenuChangedEventKey, e);

        /// <summary>
        /// Raises the VisibleOnCommandBarChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVisibleOnCommandBarChanged(EventArgs e) => this.RaiseEvent(VisibleOnCommandBarChangedEventKey, e);

        /// <summary>
        /// Raises the BeforeShowInMenu event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnBeforeShowInMenu(EventArgs e) => this.RaiseEvent(BeforeShowInMenuEventKey, e);

        /// <summary>
        /// Common initialization.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="bitmap">The bitmap.</param>
        /// <returns>Bitmap.</returns>
        private Bitmap BitmapProperty(string propertyName, ref Bitmap bitmap)
        {
            if (bitmap != null)
            {
                return bitmap;
            }

            //	Obtain the type.
            var type = this.GetType();

            //	Attempt to load the bitmap from an Assembly resource stream.
            bitmap = ResourceHelper.LoadAssemblyResourceBitmap(type.Assembly, type.Namespace + ".Images", type.Name + propertyName + ".png", false);
            //Debug.Assert(bitmap == null || (bitmap.Width == 16 && bitmap.Height == 16), "Bitmap " + type.Assembly.FullName + type.Namespace + ".Images." + type.Name + propertyName + ".png" + " is not 16x16");
            return bitmap;
        }

        /// <summary>
        /// Private helper to raise an EventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool RaiseEvent(object eventKey, EventArgs e)
        {
            var eventHandler = (EventHandler)this.Events[eventKey];
            if (eventHandler != null)
            {
                eventHandler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Private helper to raise an EventHandler event.
        /// </summary>
        /// <param name="eventKey">The event key of the event to raise.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool RaiseEvent(object eventKey, ExecuteEventHandlerArgs e)
        {
            if (this.Events[eventKey] is ExecuteEventHandler eventHandler)
            {
                eventHandler(this, e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.</returns>
        /// <exception cref="ArgumentException">object is not a Command</exception>
        public int CompareTo(object obj)
        {
            if (obj is Command otherCommand)
            {
                return this.CommandId.CompareTo(otherCommand.CommandId);
            }
            else if (obj == null)
            {
                // By definition, any object compares greater than a null reference.
                return 1;
            }

            throw new ArgumentException("object is not a Command");
        }

        /// <summary>
        /// The flushing
        /// </summary>
        bool flushing = false;

        /// <summary>
        /// The keys
        /// </summary>
        private PropertyKey[] _keys = new PropertyKey[MAX_PENDING_INVALIDATIONS];
        /// <summary>
        /// Flushes the pending invalidations.
        /// </summary>
        /// <param name="framework">The framework.</param>
        public void FlushPendingInvalidations(IUIFramework framework)
        {
            Debug.Assert(!this.flushing, "Flushing while already flushing!?!");
            this.flushing = true;

            try
            {
                Debug.Assert(this.pendingInvalidations.Count < this._keys.Length, "Need to increase the size of MAX_PENDING_INVALIDATIONS.");
                this.pendingInvalidations.Keys.CopyTo(this._keys, 0);

                for (var i = 0; i < this.pendingInvalidations.Count; i++)
                {
                    var key = this._keys[i];
                    if (this.pendingInvalidations[key] == InvalidationState.Pending)
                    {
                        var result = framework.InvalidateUICommand((uint)this.CommandId,
                                                      PropertyKeyExtensions.GetCommandInvalidationFlags(key),
                                                      key.ToPointer());
                        this.pendingInvalidations[key] = result == 0 ? InvalidationState.WaitingForUpdateProperty : InvalidationState.Error;
                    }
                }
            }
            catch (Exception)
            {
                this.pendingInvalidations.Clear();
                throw;
            }
            finally
            {
                this.flushing = false;
            }
        }

        /// <summary>
        /// The maximum pending invalidations
        /// </summary>
        private const int MAX_PENDING_INVALIDATIONS = 15;
        /// <summary>
        /// The pending invalidations
        /// </summary>
        private Dictionary<PropertyKey, InvalidationState> pendingInvalidations = new Dictionary<PropertyKey, InvalidationState>(MAX_PENDING_INVALIDATIONS);

        /// <summary>
        /// Updates the state of the invalidation.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="invalidationState">State of the invalidation.</param>
        protected internal void UpdateInvalidationState(PropertyKey key, InvalidationState invalidationState)
        {
            if (this.pendingInvalidations.ContainsKey(key))
            {
                if (invalidationState == InvalidationState.Pending &&
                    this.pendingInvalidations[key] == InvalidationState.WaitingForUpdateProperty)
                {
                    // Nothing to do.  We're already waiting for an UpdateProperty callback.
                    return;
                }

                this.pendingInvalidations[key] = invalidationState;
            }
            else
            {
                Debug.Assert(!this.flushing);
                this.pendingInvalidations.Add(key, invalidationState);
                Debug.Assert(this.pendingInvalidations.Count <= MAX_PENDING_INVALIDATIONS, "Need to increase MAX_PENDING_INVALIDATIONS?");
            }
        }
    }

    /// <summary>
    /// Delegate ExecuteEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The arguments.</param>
    public delegate void ExecuteEventHandler(object sender, ExecuteEventHandlerArgs args);
}
