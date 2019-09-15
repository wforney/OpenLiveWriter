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

    public delegate void CommandBarButtonContextMenuHandler(Control parent, Point menuLocation, int alternativeLocation, IDisposable disposeWhenDone);

    public class Command : ICommandTextDisplayProperties, IComparable
    {
        #region Private Member Variables

        /// <summary>
        /// The command identifier.  Each command must have a unique command identifier.
        /// </summary>
        private string identifier;

        /// <summary>
        /// The description of the command used by accessibility client applications.
        /// </summary>
        private string accessibleDescription;

        /// <summary>
        /// The name of the command used by accessibility client applications
        /// </summary>
        private string accessibleName;

        /// <summary>
        /// The Shortcut of the command.
        /// </summary>
        private Shortcut shortcut = Shortcut.None;

        private Keys advancedShortcut = Keys.None;

        /// <summary>
        /// The AcceleratorMnemonic of the command.
        /// </summary>
        private AcceleratorMnemonic acceleratorMnemonic;

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
        /// A value indicating whether the Shortcut of the command should be shown when the command
        /// appears on a menu.
        /// </summary>
        private bool showShortcut = true;

        /// <summary>
        /// The menu text of the command.
        /// </summary>
        private string menuText = string.Empty;

        /// <summary>
        /// The menu format arguments.
        /// </summary>
        private object[] menuFormatArgs;

        /// <summary>
        /// The menu path of the command when it appears on a main menu.  The menu path has the form Name@n/Name@n where 'n'
        /// is the merge level.
        /// </summary>
        private string mainMenuPath = string.Empty;

        private bool suppressMenuBitmap;

        /// <summary>
        /// Menu bitmap for the latched enabled state.
        /// </summary>
        private Bitmap menuBitmapLatchedEnabled;

        /// <summary>
        /// Command bar button style.
        /// </summary>
        private CommandBarButtonStyle commandBarButtonStyle = CommandBarButtonStyle.System;

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
        /// The AcceleratorMnemonic that will show the CommandBarButtonContextMenu.
        /// </summary>
        private AcceleratorMnemonic commandBarButtonContextMenuAcceleratorMnemonic;

        /// <summary>
        /// The command bar button context menu.
        /// </summary>
        private CommandContextMenu commandBarButtonContextMenu;

        /// <summary>
        /// The command bar button command context menu definition.
        /// </summary>
        private CommandContextMenuDefinition commandBarButtonContextMenuDefinition;

        private CommandBarButtonContextMenuHandler commandBarButtonContextMenuHandler;

        /// <summary>
        /// A value indicating whether the command bar button context menu is accessed through a
        /// separate dropdown control.
        /// </summary>
        private bool commandBarButtonContextMenuDropDown;

        /// <summary>
        /// The command text.  This is the "user visible text" that is associate with the command
        /// (such as "Save All").  It appears whenever the user can see text for the command.
        /// </summary>
        private string text;

        /// <summary>
        /// The command tag.
        /// </summary>
        private object tag;

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

        #endregion Private Member Variables

        #region Public Events

        private EventHandlerList events;
        private EventHandlerList Events
        {
            get
            {
                if (this.events == null)
                    this.events = new EventHandlerList();
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

        #endregion Public Events

        #region Class Initialization & Termination

        public Command(IContainer container)
        {
            this.InitializeImageLoaders();
        }

        public Command()
        {
            this.CommandId = CommandId.None;
            this.InitializeImageLoaders();
        }

        public Command(CommandId commandId, object tag) : this(commandId)
        {
            this.Tag = tag;
        }

        public Command(CommandId commandId)
        {
            this.UpdateInvalidationState(PropertyKeys.Enabled, InvalidationState.Pending);
            this.Identifier = commandId.ToString();
            this.CommandId = commandId;
            this.InitializeImageLoaders();
            this.LoadResources();
        }

        #endregion Class Initialization & Termination

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
                IRepresentativeString rs = (IRepresentativeString)this;
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

        #region Public Properties

        /// <summary>
        /// The command identifier.  Each command must have a unique command identifier.
        /// </summary>
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
                    this.identifier = Guid.NewGuid().ToString();
                return this.identifier;
            }
            set
            {
                this.identifier = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the command used by accessibility client applications.
        /// </summary>
        [
            Category("Accessibility"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the description that will be reported to accessibility clients.")
        ]
        public string AccessibleDescription
        {
            get
            {
                return this.accessibleDescription;
            }
            set
            {
                this.accessibleDescription = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the command used by accessibility client applications.
        /// </summary>
        [
            Category("Accessibility"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the name that will be reported to accessibility clients.")
        ]
        public string AccessibleName
        {
            get
            {
                return this.accessibleName;
            }
            set
            {
                this.accessibleName = value;
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the Shortcut of the command.
        /// </summary>
        [
            Category("Behavior"),
                Localizable(true),
                DefaultValue(Shortcut.None),
                Description("Specifies the Shortcut of the command.")
        ]
        public Shortcut Shortcut
        {
            get
            {
                return this.shortcut;
            }
            set
            {
                Debug.Assert(this.shortcut == Shortcut.None, "Changing shortcuts is a bad idea. CommandManager.RebuildCommandShortcutTable() will get stale.");
                this.shortcut = value;
            }
        }

        /// <summary>
        /// For shortcuts that can't be expressed by the Shortcut enum/property.
        /// </summary>
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
            get { return this.advancedShortcut; }
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
        [
            Category("Behavior"),
                Localizable(true),
                Description("Specifies the AcceleratorMnemonic of the command.")
        ]
        public AcceleratorMnemonic AcceleratorMnemonic
        {
            get
            {
                return this.acceleratorMnemonic;
            }
            set
            {
                this.acceleratorMnemonic = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command should be visible on a context menu.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the command should be visible on a context menu.")
        ]
        public bool VisibleOnContextMenu
        {
            get
            {
                return this.visibleOnContextMenu;
            }
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
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the command should be visible on a main menu.")
        ]
        public bool VisibleOnMainMenu
        {
            get
            {
                return this.visibleOnMainMenu;
            }
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
        [
        Category("Appearance.Menu"),
        Localizable(false),
        DefaultValue(true),
        Description("Specifies whether the command should be visible on a command bar.")
        ]
        public bool VisibleOnCommandBar
        {
            get
            {
                return this.visibleOnCommandBar;
            }
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
        [
            Category("Appearance.MainMenu"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the Shortcut of the command should be shown when the command appears on a main menu.")
        ]
        public bool ShowShortcut
        {
            get
            {
                return this.showShortcut;
            }
            set
            {
                this.showShortcut = value;
            }
        }

        /// <summary>
        /// Gets or sets the main menu path of the command.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the menu text of the command when it appears on a menu.")
        ]
        public string MenuText
        {
            get
            {
                return this.menuText;
            }
            set
            {
                this.menuText = value;
            }
        }

        /// <summary>
        /// Gets or sets the menu format arguments.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public object[] MenuFormatArgs
        {
            get
            {
                return this.menuFormatArgs;
            }
            set
            {
                this.menuFormatArgs = value;
            }
        }

        /// <summary>
        /// Gets or sets the main menu path of the command.
        /// </summary>
        [
            Category("Appearance.MainMenu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu path of the command when it appears on a main menu.  Include the menu merge order after each path entry.  Example: 'File@0/[-]New@1'.")
        ]
        public string MainMenuPath
        {
            get
            {
                return this.mainMenuPath;
            }
            set
            {
                this.mainMenuPath = value;
            }
        }

        /// <summary>
        /// Gets or sets the menu bitmap for the disabled state.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the disabled state.")
        ]
        public Bitmap MenuBitmapDisabled
        {
            get
            {
                return this.suppressMenuBitmap ? null : this.CommandBarButtonBitmapDisabled;
            }
        }

        /// <summary>
        /// Gets or sets the menu bitmap for the enabled state.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the enabled state.")
        ]
        public Bitmap MenuBitmapEnabled
        {
            get
            {
                return this.suppressMenuBitmap ? null : this.CommandBarButtonBitmapEnabled;
            }
        }

        [Localizable(false)]
        public bool SuppressMenuBitmap
        {
            get { return this.suppressMenuBitmap; }
            set { this.suppressMenuBitmap = value; }
        }

        [Localizable(false)]
        public bool SuppressCommandBarBitmap
        {
            get { return this._suppressCommandBarBitmap; }
            set { this._suppressCommandBarBitmap = value; }
        }

        private bool _suppressCommandBarBitmap = false;

        /// <summary>
        /// Gets or sets the menu bitmap for the latched enabled state.
        /// </summary>
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
                    this.menuBitmapLatchedEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.Application.CommandMenuBitmapLatchedEnabled.png");

                //	Done.
                return this.menuBitmapLatchedEnabled;
            }
            set
            {
                this.menuBitmapLatchedEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the menu bitmap for the latched selected state.
        /// </summary>
        [
            Category("Appearance.Latched"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the latched selected state.")
        ]
        public Bitmap MenuBitmapLatchedSelected
        {
            get
            {
                return this.MenuBitmapLatchedEnabled;
            }
        }

        /// <summary>
        /// Gets or sets the menu selected bitmap.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the menu bitmap for the selected state.")
        ]
        public Bitmap MenuBitmapSelected
        {
            get
            {
                return this.suppressMenuBitmap ? null : this.CommandBarButtonBitmapSelected;
            }
        }

        /// <summary>
        /// Gets or sets the command bar button style.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(CommandBarButtonStyle.System),
                Description("Specifies the command bar button style.")
        ]
        public CommandBarButtonStyle CommandBarButtonStyle
        {
            get
            {
                return this.commandBarButtonStyle;
            }
            set
            {
                this.commandBarButtonStyle = value;
            }
        }

        /// <summary>
        /// Gets or sets the command bar button bar button text.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button text."),
        ]
        public string CommandBarButtonText
        {
            get
            {
                return this.commandBarButtonText;
            }
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
            set
            {
                this.commandBarButtonBitmapDisabled = value;
            }
        }

        /// <summary>
        /// Gets or sets the command bar button bitmap for the enabled state.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the command bar button bitmap for the enabled state.")
        ]
        public Bitmap CommandBarButtonBitmapEnabled
        {
            get
            {
                return this.BitmapProperty("CommandBarButtonBitmapEnabled", ref this.commandBarButtonBitmapEnabled);
            }
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
            set
            {
                this.commandBarButtonBitmapSelected = value;
            }
        }

        /// <summary>
        /// Gets or sets the command bar button bitmap for the selected state.
        /// </summary>
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
            set
            {
                this.commandBarButtonBitmapPushed = value;
            }
        }

        /// <summary>
        /// Gets or sets the AcceleratorMnemonic that will show the CommandBarContextMenu.
        /// </summary>
        [
            Category("Behavior"),
            Description("Not normally used!  Specifies the the AcceleratorMnemonic that will show the CommandBarButtonContextMenu of the Command.  This value is only useful under certain conditions.")
        ]
        public AcceleratorMnemonic CommandBarButtonContextMenuAcceleratorMnemonic
        {
            get
            {
                return this.commandBarButtonContextMenuAcceleratorMnemonic;
            }
            set
            {
                this.commandBarButtonContextMenuAcceleratorMnemonic = value;
            }
        }

        /// <summary>
        /// Gets or sets the command bar button context menu.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the command bar button context menu.")
        ]
        public CommandContextMenu CommandBarButtonContextMenu
        {
            get
            {
                return this.commandBarButtonContextMenu;
            }
            set
            {
                this.commandBarButtonContextMenu = value;
            }
        }

        /// <summary>
        /// Gets or sets the command bar button context menu.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the command bar button context menu definition.")
        ]
        public CommandContextMenuDefinition CommandBarButtonContextMenuDefinition
        {
            get
            {
                return this.commandBarButtonContextMenuDefinition;
            }
            set
            {
                this.commandBarButtonContextMenuDefinition = value;
                this.RaiseEvent(CommandBarButtonContextMenuDefinitionKey, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the command bar button mini form factory
        /// </summary>
        public ICommandContextMenuControlHandler CommandBarButtonContextMenuControlHandler
        {
            get
            {
                return this.commandBarButtonContextMenuControlHandler;
            }
            set
            {
                this.commandBarButtonContextMenuControlHandler = value;
            }
        }
        private ICommandContextMenuControlHandler commandBarButtonContextMenuControlHandler = null;

        public CommandBarButtonContextMenuHandler CommandBarButtonContextMenuHandler
        {
            get { return this.commandBarButtonContextMenuHandler; }
            set { this.commandBarButtonContextMenuHandler = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command bar context menu is accessed
        /// through a separate dropdown control.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies whether the the command bar button context menu is accessed through a separate dropdown control.")
        ]
        public bool CommandBarButtonContextMenuDropDown
        {
            get
            {
                return this.commandBarButtonContextMenuDropDown;
            }
            set
            {
                this.commandBarButtonContextMenuDropDown = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the command bar context menu is accessed
        /// through a separate dropdown control.
        /// </summary>
        [
            Category("Appearance.CommandBar"),
                Localizable(false),
                DefaultValue(false),
                Description("Specifies whether the parent should be invalidated after showing the context menu (used for IE ToolBand).")
        ]
        public bool CommandBarButtonContextMenuInvalidateParent
        {
            get
            {
                return this.commandBarButtonContextMenuInvalidateParent;
            }
            set
            {
                this.commandBarButtonContextMenuInvalidateParent = value;
            }
        }
        private bool commandBarButtonContextMenuInvalidateParent = false;

        /// <summary>
        /// Gets or sets the command text.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the text that is associated with the command.")
        ]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
            }
        }

        private LazyLoader<Bitmap> largeImage;
        /// <summary>
        /// Gets or sets the command LargeImage.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the large image that is associated with the command.")
        ]
        public virtual Bitmap LargeImage
        {
            get
            {
                return this.largeImage;
            }
            set
            {
                // set the value
                this.largeImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.LargeImage, InvalidationState.Pending);
                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        private LazyLoader<Bitmap> largeHighContrastImage;
        /// <summary>
        /// Gets or sets the command LargeImage.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the large image that is associated with the command.")
        ]
        public virtual Bitmap LargeHighContrastImage
        {
            get
            {
                return this.largeHighContrastImage;
            }
            set
            {
                // set the value
                this.largeHighContrastImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.LargeHighContrastImage, InvalidationState.Pending);

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        private LazyLoader<Bitmap> smallImage;
        /// <summary>
        /// Gets or sets the command SmallImage.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the small image that is associated with the command.")
        ]
        public virtual Bitmap SmallImage
        {
            get
            {
                return this.smallImage;
            }
            set
            {
                // set the value
                this.smallImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.SmallImage, InvalidationState.Pending);

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        private LazyLoader<Bitmap> smallHighContrastImage;

        public int InvalidationCount { get; set; }

        /// <summary>
        /// Gets or sets the command SmallHighContrastImage.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the small image that is associated with the command.")
        ]
        public virtual Bitmap SmallHighContrastImage
        {
            get
            {
                // You'll need to take dpi into consideration as well.
                return this.smallHighContrastImage;
            }
            set
            {
                // set the value
                this.smallHighContrastImage.Value = value;

                this.UpdateInvalidationState(PropertyKeys.SmallHighContrastImage, InvalidationState.Pending);

                // force state changed
                this.OnStateChanged(EventArgs.Empty);
            }
        }

        private string _labelTitle;
        /// <summary>
        /// Gets or sets the command description.
        /// </summary>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the label title for the command.")
        ]
        public virtual string LabelTitle
        {
            get { return this._labelTitle; }

            set
            {
                if (this._labelTitle != value)
                {
                    this._labelTitle = TextHelper.UnescapeNewlines(value);

                    this.UpdateInvalidationState(PropertyKeys.Label, InvalidationState.Pending);

                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        private string _labelDescription;
        /// <summary>
        /// Gets or sets the command label title.
        /// </summary>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the label description for the command.")
        ]
        public string LabelDescription
        {
            get { return this._labelDescription; }

            set
            {
                if (this._labelDescription != value)
                {
                    this._labelDescription = value;
                    this.UpdateInvalidationState(PropertyKeys.LabelDescription, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        private string _tooltipDescription;
        /// <summary>
        /// Gets or sets the tooltip description.
        /// </summary>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the tooltip description for the command.")
        ]
        public virtual string TooltipDescription
        {
            get { return this._tooltipDescription; }
            set
            {
                if (this._tooltipDescription != value)
                {
                    this._tooltipDescription = value;
                    this.UpdateInvalidationState(PropertyKeys.TooltipDescription, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the keytip.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the keytip for the command.")
        ]
        public string Keytip { get; set; }

        private string _tooltipTitle;
        /// <summary>
        /// Gets or sets the tooltip title.
        /// </summary>
        [
            Category("Appearance"),
            Localizable(true),
            DefaultValue(null),
            Description("Specifies the description for the command.")
        ]
        public string TooltipTitle
        {
            get { return this._tooltipTitle; }
            set
            {
                if (this._tooltipTitle != value)
                {
                    this._tooltipTitle = value;
                    this.UpdateInvalidationState(PropertyKeys.TooltipTitle, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the command tag.
        /// </summary>
        [
            Category("Behavior"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the tag that is associated with the command.")
        ]
        public object Tag
        {
            get
            {
                return this.tag;
            }
            set
            {
                this.tag = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Command is on or not.
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(true),
                Description("Specifies the initial value of the On property.")
        ]
        public bool On
        {
            get
            {
                return this.on;
            }
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
        [
            Category("Behavior"),
                DefaultValue(true),
                Description("Specifies the initial value of the Enabled property.")
        ]
        public virtual bool Enabled
        {
            get
            {
                return this.enabled;
            }
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
        [
            Category("Behavior"),
                DefaultValue(false),
                Description("Specifies the initial value of the Latched property.")
        ]
        public bool Latched
        {
            get
            {
                return this.latched;
            }
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
        #endregion

        #region Public Methods

        public void LoadResources() => CommandResourceLoader.ApplyResources(this);

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

        public virtual int UpdateProperty(ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            Debug.Assert(!this._flushing, "UpdateProperty called while flushing pending invalidations!");
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

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Raises the BeforeShowInMenu event.
        /// </summary>
        internal void InvokeBeforeShowInMenu(EventArgs e) => this.OnBeforeShowInMenu(e);

        #endregion Internal Methods

        #region Protected Events

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
                bool executed = this.RaiseEvent(ExecuteEventKey, e);
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
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnExecute(ExecuteEventHandlerArgs args)
        {
            try
            {
                bool executed = this.RaiseEvent(ExecuteWithArgsEventKey, args);
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

        public virtual void Invalidate()
        {
            this.UpdateInvalidationState(PropertyKeys.Enabled, InvalidationState.Pending);
            this.OnStateChanged(EventArgs.Empty);
        }

        public void Invalidate(PropertyKey[] keys)
        {
            foreach (PropertyKey key in keys)
                this.UpdateInvalidationState(key, InvalidationState.Pending);

            this.OnStateChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the StateChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnStateChanged(EventArgs e) => this.RaiseEvent(StateChangedEventKey, e);

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

        #endregion Protected Events

        #region Private Methods

        /// <summary>
        /// Common initialization.
        /// </summary>
        private Bitmap BitmapProperty(string propertyName, ref Bitmap bitmap)
        {
            if (bitmap != null)
                return bitmap;

            //	Obtain the type.
            Type type = this.GetType();

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
        private bool RaiseEvent(object eventKey, EventArgs e)
        {
            EventHandler eventHandler = (EventHandler)this.Events[eventKey];
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
        private bool RaiseEvent(object eventKey, ExecuteEventHandlerArgs e)
        {
            ExecuteEventHandler eventHandler = this.Events[eventKey] as ExecuteEventHandler;
            if (eventHandler != null)
            {
                eventHandler(this, e);
                return true;
            }
            return false;
        }

        #endregion Private Methods

        #region IComparable Members

        public int CompareTo(object obj)
        {
            if (obj is Command)
            {
                Command otherCommand = (Command)obj;
                return this.CommandId.CompareTo(otherCommand.CommandId);
            }
            else if (obj == null)
            {
                // By definition, any object compares greater than a null reference.
                return 1;
            }

            throw new ArgumentException("object is not a Command");
        }

        #endregion

        public enum InvalidationState
        {
            Pending, // We have not yet set or invalidated the ribbon
            WaitingForUpdateProperty, // We have called InvalidateUICommand and are waiting for an UpdateProperty callback.
            Error // The ribbon APIs to set and invalidate this command have return an failing error code.
        }

        bool _flushing = false;

        private PropertyKey[] _keys = new PropertyKey[MAX_PENDING_INVALIDATIONS];
        public void FlushPendingInvalidations(IUIFramework framework)
        {
            Debug.Assert(!this._flushing, "Flushing while already flushing!?!");
            this._flushing = true;

            try
            {
                Debug.Assert(this.pendingInvalidations.Count < this._keys.Length, "Need to increase the size of MAX_PENDING_INVALIDATIONS.");
                this.pendingInvalidations.Keys.CopyTo(this._keys, 0);

                for (int i = 0; i < this.pendingInvalidations.Count; i++)
                {
                    PropertyKey key = this._keys[i];
                    if (this.pendingInvalidations[key] == InvalidationState.Pending)
                    {
                        int result = framework.InvalidateUICommand((uint)this.CommandId,
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
                this._flushing = false;
            }
        }

        private const int MAX_PENDING_INVALIDATIONS = 15;
        private Dictionary<PropertyKey, InvalidationState> pendingInvalidations = new Dictionary<PropertyKey, InvalidationState>(MAX_PENDING_INVALIDATIONS);

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
                Debug.Assert(!this._flushing);
                this.pendingInvalidations.Add(key, invalidationState);
                Debug.Assert(this.pendingInvalidations.Count <= MAX_PENDING_INVALIDATIONS, "Need to increase MAX_PENDING_INVALIDATIONS?");
            }
        }
    }

    public delegate void ExecuteEventHandler(object sender, ExecuteEventHandlerArgs args);
}
