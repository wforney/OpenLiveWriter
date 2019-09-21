// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Class DynamicCommandMenu.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class DynamicCommandMenu : IDisposable
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandMenu"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        public DynamicCommandMenu(IDynamicCommandMenuContext context)
        {
            // save reference to command list context
            this.Context = context;

            // initialize commands
            this.InitializeCommands();
        }

        /// <summary>
        /// Get the underlying command identifiers managed by this dynamic command menu
        /// (this would allow us to embed these commands within the scope of a context-menu definition)
        /// </summary>
        /// <value>The command identifiers.</value>
        public string[] CommandIdentifiers
        {
            get
            {
                var commands = new ArrayList(this.commands.Count + 1);
                foreach (Command command in this.commands)
                {
                    commands.Add(command.Identifier);
                }

                if (this.commandMore != null)
                {
                    commands.Add(this.commandMore.Identifier);
                }

                return (string[])commands.ToArray(typeof(string));
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (Command command in this.commands)
            {
                this.Context.CommandManager.Remove(command);
            }

            if (this.components != null)
            {
                this.components.Dispose();
            }
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.Context.CommandManager.BeginUpdate();

            // add commands
            for (var i = 0; i < this.Context.Options.MaxCommandsShownOnMenu; i++)
            {
                // create the command
                var command = new Command(this.components);

                // First command added should have a BeforeShowInMenu so that we can
                // dynamically update the contents of the menu
                if (i == 0)
                {
                    command.BeforeShowInMenu += new EventHandler(this.commands_BeforeShowInMenu);
                }

                // provide the command with a unique identifier
                command.Identifier = Guid.NewGuid().ToString();

                var separator = i == 0 && this.Context.Options.SeparatorBegin ? "-" : string.Empty;

                // define menu paths
                var menuMergeText = (this.Context.Options.MenuMergeOffset + i).ToString(CultureInfo.InvariantCulture);
                command.MenuText = (this.Context.Options.UseNumericMnemonics ? $"&{(i + 1).ToString(CultureInfo.InvariantCulture)} {{0}}" : "{0}");
                command.MainMenuPath = $"{this.Context.Options.MainMenuBasePath}/{separator}{command.MenuText}@{menuMergeText}";

                // generic execute handler for all window menu commands
                command.Execute += new EventHandler(this.command_Execute);

                // add the command to our internal list
                this.commands.Add(command);

                // add the command to the system command manager
                this.Context.CommandManager.Add(command);
            }

            // add 'more' command if appropriate
            if (this.Context.Options.MoreCommandsMenuCaption != null)
            {
                this.commandMore = new Command(this.components);
                this.commandMore.Identifier = Guid.NewGuid().ToString();
                this.commandMore.VisibleOnContextMenu = true;
                this.commandMore.VisibleOnMainMenu = true;
                this.commandMore.MenuText = this.Context.Options.MoreCommandsMenuCaption;
                this.commandMore.MainMenuPath = $"{this.Context.Options.MainMenuBasePath}/{this.Context.Options.MoreCommandsMenuCaption}@{(this.Context.Options.MenuMergeOffset + this.Context.Options.MaxCommandsShownOnMenu).ToString(CultureInfo.InvariantCulture)}";
                this.commandMore.Execute += new EventHandler(this.commandMore_Execute);
                this.Context.CommandManager.Add(this.commandMore);
            }

            this.Context.CommandManager.EndUpdate();
        }

        /// <summary>
        /// BeforeShowInMenu to dynamically update the contents of the window menu
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="ea">event args</param>
        private void commands_BeforeShowInMenu(object sender, EventArgs ea)
        {
            // obtain the list of command objects
            var menuCommandObjects = this.Context.GetMenuCommandObjects();

            //	Adjust the commands
            for (var i = 0; i < this.commands.Count; i++)
            {
                //	Get the command.
                var command = this.commands[i] as Command;

                //	If the command is beyond the set of command objects files turn it off.  Otherwise,
                //	turn it on and update it.
                if (i >= menuCommandObjects.Length)
                {
                    //  Turn the command off
                    command.VisibleOnContextMenu = false;
                    command.VisibleOnMainMenu = false;
                    command.Enabled = false;

                    //	Update the command.
                    command.Tag = string.Empty;
                    command.MenuFormatArgs = new object[] { string.Empty };
                }
                else
                {
                    //	Turn the command on
                    command.VisibleOnContextMenu = true;
                    command.VisibleOnMainMenu = true;
                    command.Enabled = menuCommandObjects[i].Enabled;
                    command.Latched = menuCommandObjects[i].Latched;

                    //	Update the command.
                    command.CommandBarButtonBitmapEnabled = menuCommandObjects[i].Image;
                    command.MenuFormatArgs = new object[] { menuCommandObjects[i].Caption };
                    command.Tag = menuCommandObjects[i];
                }
            }

            // show or hide the 'more' command as necessary
            if (this.commandMore != null)
            {
                var showCommandMore = this.Context.Options.MoreCommandsMenuCaption != null && (menuCommandObjects.Length > this.commands.Count);
                this.commandMore.VisibleOnContextMenu = showCommandMore;
                this.commandMore.VisibleOnMainMenu = showCommandMore;
                this.commandMore.Enabled = showCommandMore;
            }
        }

        /// <summary>
        /// Handles the Execute event of the command control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ea">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void command_Execute(object sender, EventArgs ea) =>
            // notify context
            this.Context.CommandExecuted((sender as Command).Tag as IMenuCommandObject);

        /// <summary>
        /// Handles the Execute event of the commandMore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ea">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void commandMore_Execute(object sender, EventArgs ea)
        {
            using (var form = new DynamicCommandMenuOverflowForm(this.Context.GetMenuCommandObjects()))
            {
                // configure the title bar
                form.Text = this.Context.Options.MoreCommandsDialogTitle;

                // show the form
                using (new WaitCursor())
                {
                    var result = form.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        this.Context.CommandExecuted(form.SelectedObject);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <value>The context.</value>
        private IDynamicCommandMenuContext Context { get; }

        /// <summary>
        /// The commands
        /// </summary>
        private readonly ArrayList commands = new ArrayList();

        /// <summary>
        /// The command more
        /// </summary>
        private Command commandMore;

        /// <summary>
        /// The components
        /// </summary>
        private readonly Container components = new Container();
    }
}
