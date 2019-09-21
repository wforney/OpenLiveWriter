// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Localization;

    public delegate void ExecuteWithArgsDelegate(ExecuteEventHandlerArgs args);

    /// <summary>
    /// Class CommandManager.
    /// Implements the <see cref="System.ComponentModel.Component" />
    /// Implements the <see cref="OpenLiveWriter.Interop.Com.Ribbon.IUICommandHandler" />
    /// Implements the <see cref="OpenLiveWriter.Interop.Com.Ribbon.IUICommandHandlerOverride" />
    /// </summary>
    /// <seealso cref="System.ComponentModel.Component" />
    /// <seealso cref="OpenLiveWriter.Interop.Com.Ribbon.IUICommandHandler" />
    /// <seealso cref="OpenLiveWriter.Interop.Com.Ribbon.IUICommandHandlerOverride" />
    public partial class CommandManager : Component, IUICommandHandler, IUICommandHandlerOverride
    {
        /// <summary>
        /// The generic command handler
        /// </summary>
        private GenericCommandHandler genericCommandHandler;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The command Command table, keyed by command identifier.
        /// </summary>
        private Hashtable commandTable = new Hashtable();

        /// <summary>
        ///	The cross-referenced set of active Shortcuts.  Keyed by Command.Shortcut.  This table
        ///	is not constructed or maintained until it is first used.
        /// </summary>
        private Hashtable commandShortcutTable = null;

        /// <summary>
        /// If true, the commandShortcutTable needs to be repopulated before the next time it is
        /// consulted. The two things that affect the state here are 1) the set of commands that
        /// are loaded (whether enabled or disabled), and 2) the shortcuts/advanced shortcuts of
        /// those commands. If either changes then this flag needs to be set to true.
        ///
        /// The reason this is important is because otherwise the commandShortcutTable will be
        /// fully rebuilt on every keypress in the editor (actually it currently happens twice),
        /// which is enough to make typing feel sluggish.
        /// </summary>
        private bool commandShortcutTableIsStale = true;

        /// <summary>
        ///	The cross-referenced set of active AcceleratorMnemonic values. Keyed by Command.AcceleratorMnemonic.
        ///	This table is not constructed or maintained until it is first used.
        /// </summary>
        private Hashtable acceleratorMnemonicTable = null;

        /// <summary>
        ///	The cross-referenced set of active CommandBarButtonContextMenuAcceleratorMnemonic
        ///	values. Keyed by Command.CommandBarButtonContextMenuAcceleratorMnemonic.  This table is
        ///	not constructed or maintained until it is first used.
        /// </summary>
        private Hashtable commandBarButtonContextMenuAcceleratorMnemonicTable = null;

        /// <summary>
        /// A set of shortcuts that need to be ignored. Always check for null before accessing.
        /// </summary>
        private HashSet maskedShortcuts;

        /// <summary>
        /// Update count.
        /// </summary>
        private int updateCount = 0;

        /// <summary>
        /// Change count.
        /// </summary>
        private bool pendingChange = false;

        /// <summary>
        /// The changed event.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        public CommandManager(IContainer container)
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            this.CreateGenericCommandHandler();
            container.Add(this);
            this.InitializeComponent();
        }

        private void CreateGenericCommandHandler() => this.genericCommandHandler = new GenericCommandHandler(this);

        /// <summary>
        /// Initializes a new instance of the Command class.
        /// </summary>
        public CommandManager()
        {
            ///
            /// Required for Windows.Forms Class Composition Designer support
            ///
            this.CreateGenericCommandHandler();
            this.InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();
        #endregion

        /// <summary>
        /// Gets or sets a value which indicates whether we are suppressing events.
        /// </summary>
        public bool SuppressEvents { get; set; }

        /// <summary>
        /// Gets the count of commands in the command manager.
        /// </summary>
        public int Count => this.commandTable.Count;

        /// <summary>
        /// Begins update to CommandManager allowing multiple Change events to be batched.
        /// </summary>
        public void BeginUpdate() => this.updateCount++;

        /// <summary>
        /// Ends update to CommandManager allowing multiple Change events to be batched.
        /// </summary>
        public void EndUpdate() => this.EndUpdate(false);

        public void EndUpdate(bool forceUpdate)
        {
            Debug.Assert(this.updateCount > 0, "EndUpdate called incorrectly.");
            if (this.updateCount > 0)
            {
                if (--this.updateCount == 0)
                {
                    if (this.pendingChange || forceUpdate)
                    {
                        this.pendingChange = false;

                        foreach (var entry in this.batchedCommands)
                        {
                            CommandStateChanged?.Invoke(entry.Key, entry.Value);
                        }

                        this.batchedCommands.Clear();

                        this.OnChanged(EventArgs.Empty);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a set of Commands.
        /// </summary>
        /// <param name="commands">The Command(s) to add.</param>
        public void Add(params Command[] commands)
        {
            foreach (var command in commands)
            {
                this.AddCommand(command);
                command.StateChanged += this.OnCommandStateChanged;
                this.OnCommandStateChanged(command, EventArgs.Empty);
            }

            this.OnChanged(EventArgs.Empty);
        }

        private const int MAX_BATCHED_INVALIDATIONS = 90;
        private readonly Dictionary<object, EventArgs> batchedCommands = new Dictionary<object, EventArgs>(MAX_BATCHED_INVALIDATIONS);

        private void OnCommandStateChanged(object sender, EventArgs e)
        {
            // If we're in batch mode, then make a note of the sender and batch notifications
            if (this.updateCount > 0)
            {
                if (!this.batchedCommands.ContainsKey(sender))
                {
                    this.pendingChange = true;
                    this.batchedCommands.Add(sender, e);
                }

                Debug.Assert(
                    this.batchedCommands.Count <= MAX_BATCHED_INVALIDATIONS,
                    "Need to increase the size of MAX_BATCHED_INVALIDATIONS.");
            }
            else
            {
                // Send notification directly
                CommandStateChanged?.Invoke(sender, e);
            }
        }

        public Command Add(CommandId commandId, EventHandler handler)
        {
            var command = new Command(commandId);
            command.Execute += handler;
            this.Add(command);
            return command;
        }

        public Command Add(Command command, ExecuteEventHandler handler)
        {
            command.ExecuteWithArgs += handler;
            this.Add(command);
            return command;
        }

        public Command Add(CommandId commandId, EventHandler handler, bool enabled)
        {
            var command = new Command(commandId);
            command.Execute += handler;
            command.Enabled = enabled;
            this.Add(command);
            return command;
        }

        public event EventHandler CommandStateChanged;

        /// <summary>
        /// Adds a set of Commands.
        /// </summary>
        /// <param name="commandCollection">The Command(s) to add.</param>
        public void Add(CommandCollection commandCollection)
        {
            foreach (var command in commandCollection)
            {
                this.AddCommand(command);
            }

            this.OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes a set of Commands.
        /// </summary>
        /// <param name="commands">The Command(s) to remove.</param>
        public void Remove(params Command[] commands)
        {
            foreach (var command in commands)
            {
                this.RemoveCommand(command);
            }

            this.OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes a set of Commands.
        /// </summary>
        /// <param name="commandCollection">The Command(s) to remove.</param>
        public void Remove(CommandCollection commandCollection)
        {
            foreach (var command in commandCollection)
            {
                this.RemoveCommand(command);
            }

            this.OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Processes a CmdKey for Command.Shortcut matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyShortcut(Keys keyData)
        {
            Command command = null;

            if (keyData != Keys.None)
            {
                command = this.FindCommandWithShortcut(keyData);
            }

            if (command != null)
            {
                if (command.On && command.Enabled)
                {
                    //	Ensure that any command initialization has been performed by manually
                    //	firing the BeforeShowInMenu event on the command.  Very important.
                    command.InvokeBeforeShowInMenu(EventArgs.Empty);
                    if (!command.On || !command.Enabled)
                    {
                        return false;
                    }

                    //	Execute the command.
                    this.ExecuteCommandAndFireEvents(command);
                    return true;
                }
            }

            //	We did not process the CmdKey.
            return false;
        }

        /// <summary>
        /// Processes a CmdKey for Command.AcceleratorMnemonic matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyAcceleratorMnemonic(Keys keyData)
        {
            //	Attempt to process the Keys value as a Command.AcceleratorMnemonic.
            var acceleratorMnemonic = KeyboardHelper.MapToAcceleratorMnemonic(keyData);
            if (acceleratorMnemonic != AcceleratorMnemonic.None)
            {
                var command = this.FindCommandWithAcceleratorMnemonic(acceleratorMnemonic);
                if (command != null)
                {
                    if (command.On && command.Enabled)
                    {
                        //	Ensure that any command initialization has been performed by manually
                        //	firing the BeforeShowInMenu event on the command.  Very important.
                        command.InvokeBeforeShowInMenu(EventArgs.Empty);
                        if (!command.On || !command.Enabled)
                        {
                            return false;
                        }

                        //	Execute the command.
                        this.ExecuteCommandAndFireEvents(command);
                        return true;
                    }
                }
            }

            //	We did not process the CmdKey.
            return false;
        }

        /// <summary>
        /// Processes a CmdKey for Command.CommandBarButtonContextMenuAcceleratorMnemonic matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyCommandBarButtonContextMenuAcceleratorMnemonic(Keys keyData)
        {
            //	Attempt to process the Keys value as a Command.CommandBarButtonContextMenuAcceleratorMnemonic.
            var acceleratorMnemonic = KeyboardHelper.MapToAcceleratorMnemonic(keyData);
            if (acceleratorMnemonic != AcceleratorMnemonic.None)
            {
                var command = this.FindCommandWithCommandBarButtonContextMenuAcceleratorMnemonic(acceleratorMnemonic);
                if (command != null)
                {
                    if (command.On && command.Enabled)
                    {
                        command.PerformShowCommandBarButtonContextMenu();
                        return true;
                    }
                }
            }

            //	We did not process the accelerator.
            return false;
        }

        /// <summary>
        /// Processes a CmdKey for Command.Shortcut, Command.AcceleratorMnemonic and
        /// Command.CommandBarButtonContextMenuAcceleratorMnemonic matches.
        /// </summary>
        /// <param name="keyData">Specifies key codes and modifiers and process.</param>
        /// <returns>true if the Keys value was processed; otherwise, false.</returns>
        public bool ProcessCmdKeyAll(Keys keyData) => this.ProcessCmdKeyShortcut(keyData) || this.ProcessCmdKeyAcceleratorMnemonic(keyData)
                ? true
                : this.ProcessCmdKeyCommandBarButtonContextMenuAcceleratorMnemonic(keyData);

        /// <summary>
        /// Instructs the command manager to ignore the shortcut until
        /// UnignoreShortcut is called.
        ///
        /// LIMITATION: You cannot currently ignore an AdvancedShortcut
        /// (i.e. one based on Keys instead of Shortcut).
        /// </summary>
        public void IgnoreShortcut(Shortcut shortcut)
        {
            if (this.maskedShortcuts == null)
            {
                this.maskedShortcuts = new HashSet();
            }

            var isNewElement = this.maskedShortcuts.Add(shortcut);
            Debug.Assert(isNewElement, $"Shortcut {shortcut} was already masked");
        }

        /// <summary>
        /// Instructs the command manager to respond to the shortcut again.
        /// </summary>
        public void UnignoreShortcut(Shortcut shortcut)
        {
            Trace.Assert(this.maskedShortcuts != null, "UnignoreShortcut called before IgnoreShortcut");
            if (this.maskedShortcuts != null)
            {
                var wasPresent = this.maskedShortcuts.Remove(shortcut);
                Trace.Assert(wasPresent, $"Shortcut {shortcut} was not masked");
                if (this.maskedShortcuts.Count == 0)
                {
                    this.maskedShortcuts = null;
                }
            }
        }

        /// <summary>
        /// Shoulds the ignore.
        /// </summary>
        /// <param name="shortcut">The shortcut.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool ShouldIgnore(Shortcut shortcut) =>
            this.maskedShortcuts != null && this.maskedShortcuts.Contains(shortcut);

        /// <summary>
        /// Shoulds the ignore.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool ShouldIgnore(Keys keys) =>
            this.maskedShortcuts != null && this.maskedShortcuts.Contains(KeyboardHelper.MapToShortcut(keys));

        /// <summary>
        /// Gets the command with the specified command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier of the command to get.</param>
        /// <returns>The command, or null if a command with the specified command identifier cannot be found.</returns>
        public Command Get(string commandIdentifier)
        {
            var commandInstanceManager = (CommandInstanceManager)this.commandTable[commandIdentifier];
            var command = commandInstanceManager?.ActiveCommandInstance;
            return command;
        }

        private readonly Dictionary<CommandId, string> commandIdToString = new Dictionary<CommandId, string>();

        /// <summary>
        /// Gets the specified command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier.</param>
        /// <returns>Command.</returns>
        public Command Get(CommandId commandIdentifier)
        {
            if (!this.commandIdToString.TryGetValue(commandIdentifier, out var str))
            {
                str = commandIdentifier.ToString();
                this.commandIdToString.Add(commandIdentifier, str);
            }

            var commandInstanceManager = (CommandInstanceManager)this.commandTable[str];
            var command = commandInstanceManager?.ActiveCommandInstance;
            return command;
        }

        /// <summary>
        /// Gets the command with the specified Shortcut.
        /// </summary>
        /// <param name="commandShortcut">The shortcut of the command to get.</param>
        /// <returns>The command, or null if a command with the specified Shortcut cannot be found.</returns>
        public Command FindCommandWithShortcut(Keys commandShortcut)
        {
            this.RebuildCommandShortcutTable(true);

            // WinLive 293185: If the shortcut involves CTRL - (right) ALT key, ignore it.
            if (KeyboardHelper.IsCtrlRightAlt(commandShortcut))
            {
                return null;
            }

            //	Return the command with the matching shortcut.
            var command = (Command)this.commandShortcutTable[commandShortcut];
            if (command != null)
            {
                return command;
            }

            var shortcut = KeyboardHelper.MapToShortcut(commandShortcut);
            if (shortcut != Shortcut.None && !this.ShouldIgnore(shortcut))
            {
                return (Command)this.commandShortcutTable[shortcut];
            }

            return null;
        }

        /// <summary>
        /// Gets the command with the specified AcceleratorMnemonic.
        /// </summary>
        /// <param name="acceleratorMnemonic">The AcceleratorMnemonic of the command to get.</param>
        /// <returns>The command, or null if a command with the specified AcceleratorMnemonic cannot be found.</returns>
        public Command FindCommandWithAcceleratorMnemonic(AcceleratorMnemonic acceleratorMnemonic)
        {
            //	If the AcceleratorMnemonic table has not been built, build it.
            if (this.acceleratorMnemonicTable == null)
            {
                //	Instantiate the AcceleratorMnemonic table.
                this.acceleratorMnemonicTable = new Hashtable();

                //	Rebuild the AcceleratorMnemonic table.
                this.RebuildAcceleratorMnemonicTable();
            }

            //	Return the command with the specified AcceleratorMnemonic.
            return (Command)this.acceleratorMnemonicTable[acceleratorMnemonic];
        }

        /// <summary>
        /// Gets the command with the specified CommandBarButtonContextMenuAcceleratorMnemonic.
        /// </summary>
        /// <param name="acceleratorMnemonic">The CommandBarButtonContextMenuAcceleratorMnemonic of the command to get.</param>
        /// <returns>The command, or null if a command with the specified CommandBarButtonContextMenuAcceleratorMnemonic cannot be found.</returns>
        public Command FindCommandWithCommandBarButtonContextMenuAcceleratorMnemonic(AcceleratorMnemonic acceleratorMnemonic)
        {
            //	If the CommandBarButtonContextMenuAcceleratorMnemonic table has not been built, build it.
            if (this.commandBarButtonContextMenuAcceleratorMnemonicTable == null)
            {
                //	Instantiate the CommandBarButtonContextMenuAcceleratorMnemonic table.
                this.commandBarButtonContextMenuAcceleratorMnemonicTable = new Hashtable();

                //	Rebuild the CommandBarButtonContextMenuAcceleratorMnemonic table.
                this.RebuildCommandBarButtonContextMenuAcceleratorMnemonicTable();
            }

            //	Return the command with the specified CommandBarButtonContextMenuAcceleratorMnemonic.
            return (Command)this.commandBarButtonContextMenuAcceleratorMnemonicTable[acceleratorMnemonic];
        }

        /// <summary>
        /// Builds a menu of the specified MenuType from the commands in the command manager.
        /// </summary>
        /// <param name="menuType">Specifies the type of menu to build.</param>
        /// <returns>An array of MenuItem values for the menu.</returns>
        public MenuItem[] BuildMenu(MenuType menuType)
        {
            //	Instantiate a new CommandMenuBuilder so we can build the menu from the set of
            //	commands in this command manager.
            var commandMenuBuilder = new CommandMenuBuilder(menuType);

            //	Enumerate the commands and merge each one into the merge menu.
            foreach (CommandInstanceManager commandInstanceManager in this.commandTable.Values)
            {
                if (commandInstanceManager.ActiveCommandInstance != null && commandInstanceManager.ActiveCommandInstance.On)
                {
                    commandMenuBuilder.MergeCommand(commandInstanceManager.ActiveCommandInstance);
                }
            }

            //	Return the menu items.
            return commandMenuBuilder.CreateMenuItems();
        }

        /// <summary>
        /// Clears the command manager.
        /// </summary>
        public void Clear()
        {
            this.commandShortcutTableIsStale = true;
            this.commandTable.Clear();
            if (this.commandShortcutTable != null)
            {
                this.commandShortcutTable.Clear();
            }

            if (this.acceleratorMnemonicTable != null)
            {
                this.acceleratorMnemonicTable.Clear();
            }

            if (this.commandBarButtonContextMenuAcceleratorMnemonicTable != null)
            {
                this.commandBarButtonContextMenuAcceleratorMnemonicTable.Clear();
            }

            this.OnChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        public virtual void OnChanged(EventArgs e)
        {
            if (this.updateCount > 0)
            {
                this.pendingChange = true;
            }
            else
            {
                this.RebuildCommandShortcutTable(false);
                this.RebuildAcceleratorMnemonicTable();
                this.RebuildCommandBarButtonContextMenuAcceleratorMnemonicTable();
                if (!this.SuppressEvents && Changed != null)
                {
                    Changed(null, e);
                }
            }
        }

        /// <summary>
        /// Adds a command instance.
        /// </summary>
        /// <param name="command">The Command instance to add.</param>
        private void AddCommand(Command command)
        {
            this.commandShortcutTableIsStale = true;
            var commandInstanceManager = (CommandInstanceManager)this.commandTable[command.Identifier];
            if (commandInstanceManager == null)
            {
                this.commandTable[command.Identifier] = new CommandInstanceManager(command);
            }
            else
            {
                commandInstanceManager.Add(command);
            }
        }

        /// <summary>
        /// Removes a command instance.
        /// </summary>
        /// <param name="commandList">The Command instance to remove.</param>
        private void RemoveCommand(Command command)
        {
            this.commandShortcutTableIsStale = true;
            var commandInstanceManager = (CommandInstanceManager)this.commandTable[command.Identifier];
            if (commandInstanceManager != null)
            {
                commandInstanceManager.Remove(command);
                if (commandInstanceManager.IsEmpty)
                {
                    this.commandTable.Remove(command.Identifier);
                }
            }
        }

        /// <summary>
        /// Rebuilds the Command Shortcut table.
        /// </summary>
        private void RebuildCommandShortcutTable(bool createIfNecessary)
        {
            if (createIfNecessary && this.commandShortcutTable == null)
            {
                this.commandShortcutTable = new Hashtable();
            }

            if (this.commandShortcutTable != null)
            {
                if (!this.commandShortcutTableIsStale)
                {
                    return;
                }

                this.commandShortcutTable.Clear();
                foreach (CommandInstanceManager commandInstanceManager in this.commandTable.Values)
                {
                    var cmd = commandInstanceManager.ActiveCommandInstance;
                    if (cmd != null)
                    {
                        if (cmd.AdvancedShortcut != Keys.None)
                        {
                            Debug.Assert(!this.commandShortcutTable.ContainsKey(cmd.AdvancedShortcut), $"Shortcut {cmd.AdvancedShortcut} is already registered");
                            this.commandShortcutTable[cmd.AdvancedShortcut] = cmd;
                        }

                        if (cmd.Shortcut != Shortcut.None)
                        {
                            Debug.Assert(!this.commandShortcutTable.ContainsKey(cmd.Shortcut), $"Shortcut {cmd.Shortcut} is already registered");
                            this.commandShortcutTable[cmd.Shortcut] = cmd;
                        }
                    }
                }

                this.commandShortcutTableIsStale = false;
            }
        }

        /// <summary>
        /// Rebuilds the AcceleratorMnemonicTable table.
        /// </summary>
        private void RebuildAcceleratorMnemonicTable()
        {
            if (this.acceleratorMnemonicTable != null)
            {
                this.acceleratorMnemonicTable.Clear();
                foreach (CommandInstanceManager commandInstanceManager in this.commandTable.Values)
                {
                    if (commandInstanceManager.ActiveCommandInstance != null && commandInstanceManager.ActiveCommandInstance.AcceleratorMnemonic != AcceleratorMnemonic.None)
                    {
                        this.acceleratorMnemonicTable[commandInstanceManager.ActiveCommandInstance.AcceleratorMnemonic] = commandInstanceManager.ActiveCommandInstance;
                    }
                }
            }
        }

        /// <summary>
        /// Rebuilds the CommandBarButtonContextMenu AcceleratorMnemonicTable table.
        /// </summary>
        private void RebuildCommandBarButtonContextMenuAcceleratorMnemonicTable()
        {
            if (this.commandBarButtonContextMenuAcceleratorMnemonicTable != null)
            {
                this.commandBarButtonContextMenuAcceleratorMnemonicTable.Clear();
                foreach (CommandInstanceManager commandInstanceManager in this.commandTable.Values)
                {
                    if (commandInstanceManager.ActiveCommandInstance != null && commandInstanceManager.ActiveCommandInstance.CommandBarButtonContextMenuAcceleratorMnemonic != AcceleratorMnemonic.None)
                    {
                        this.commandBarButtonContextMenuAcceleratorMnemonicTable[commandInstanceManager.ActiveCommandInstance.CommandBarButtonContextMenuAcceleratorMnemonic] = commandInstanceManager.ActiveCommandInstance;
                    }
                }
            }
        }

        private static readonly PropertyKey[] ImageKeys = new[]
        {
            PropertyKeys.SmallImage,
            PropertyKeys.SmallHighContrastImage,
            PropertyKeys.LargeImage,
            PropertyKeys.LargeHighContrastImage
        };

        public void InvalidateAllImages()
        {
            foreach (CommandInstanceManager commandInstanceManager in this.commandTable.Values)
            {
                if (commandInstanceManager.ActiveCommandInstance != null)
                {
                    commandInstanceManager.ActiveCommandInstance.Invalidate(ImageKeys);
                }
            }
        }

        public void Invalidate(CommandId commandId)
        {
            var command = this.Get(commandId);
            if (command != null)
            {
                command.Invalidate();
            }
        }

        public bool IsEnabled(CommandId id)
        {
            var c = this.Get(id);
            return c == null ? false : c.Enabled;
        }

        public void SetEnabled(CommandId id, bool enabled)
        {
            var c = this.Get(id);
            if (c != null)
            {
                c.Enabled = enabled;
            }
        }

        public void Execute(CommandId commandId)
        {
            var command = this.Get(commandId);
            if (command != null)
            {
                this.ExecuteCommandAndFireEvents(command);
            }
        }

        private void ExecuteCommandAndFireEvents(Command command)
        {
            this.FireBeforeExecute(command.CommandId);
            try
            {
                command.PerformExecute();
            }
            finally
            {
                this.FireAfterExecute(command.CommandId);
            }
        }

        public int Execute(
            uint commandId,
            CommandExecutionVerb verb,
            PropertyKeyRef key,
            PropVariantRef currentValue,
            IUISimplePropertySet commandExecutionProperties)
        {
            try
            {
                var command = this.Get((CommandId)commandId);

                if (verb != CommandExecutionVerb.Execute)
                {
                    return HRESULT.S_OK;
                }

                this.FireBeforeExecute((CommandId)commandId);
                int result;
                try
                {
                    result = command.PerformExecute(verb, key, currentValue, commandExecutionProperties);
                }
                finally
                {
                    this.FireAfterExecute((CommandId)commandId);
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.Fail($"Exception thrown when executing {(CommandId)commandId}: {ex}");
            }

            return HRESULT.S_OK;
        }

        public event CommandManagerExecuteEventHandler BeforeExecute;
        protected void FireBeforeExecute(CommandId commandId) => BeforeExecute?.Invoke(this, new CommandManagerExecuteEventArgs(commandId));

        public event CommandManagerExecuteEventHandler AfterExecute;
        protected void FireAfterExecute(CommandId commandId) => AfterExecute?.Invoke(this, new CommandManagerExecuteEventArgs(commandId));

        public int UpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            try
            {
                var command = this.Get((CommandId)commandId);
                return command == null
                    ? this.genericCommandHandler.NullCommandUpdateProperty(commandId, ref key, currentValue, out newValue)
                    : command.UpdateProperty(ref key, currentValue, out newValue);
            }
            catch (Exception ex)
            {
                Debug.Fail($"Exception throw in CommandManager.UpdateProperty: {ex}\r\n\r\nCommand: {commandId} Key: {PropertyKeys.GetName(key)}");
                throw;
            }
        }

        public int OverrideProperty(uint commandId, ref PropertyKey key, PropVariantRef overrideValue)
        {
            try
            {
                return !(this.Get((CommandId)commandId) is IOverridableCommand overridableCommand)
                    ? HRESULT.E_INVALIDARG
                    : overridableCommand.OverrideProperty(ref key, overrideValue);
            }
            catch (Exception ex)
            {
                Debug.Fail($"Exception throw in CommandManager.OverrideProperty: {ex}\r\n\r\nCommand: {commandId} Key: {PropertyKeys.GetName(key)}");
                throw;
            }
        }

        public int CancelOverride(uint commandId, ref PropertyKey key)
        {
            try
            {
                return !(this.Get((CommandId)commandId) is IOverridableCommand overridableCommand)
                    ? HRESULT.E_INVALIDARG
                    : overridableCommand.CancelOverride(ref key);
            }
            catch (Exception ex)
            {
                Debug.Fail($"Exception throw in CommandManager.OverrideProperty: {ex}\r\n\r\nCommand: {commandId} Key: {PropertyKeys.GetName(key)}");
                throw;
            }
        }
    }

    public delegate void CommandManagerExecuteEventHandler(object sender, CommandManagerExecuteEventArgs eventArgs);
}
