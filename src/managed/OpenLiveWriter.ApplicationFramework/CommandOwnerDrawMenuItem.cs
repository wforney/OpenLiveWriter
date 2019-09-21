// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    /// <summary>
    /// Command-based owner draw menu item.
    /// </summary>
    public class CommandOwnerDrawMenuItem : OwnerDrawMenuItem
    {
        /// <summary>
        /// Gets the command for this command owner draw menu item.
        /// </summary>
        public Command Command { get; private set; }

        /// <summary>
        /// Initializes a new instance of the CommandOwnerDrawMenuItem class.
        /// </summary>
        public CommandOwnerDrawMenuItem(MenuType menuType, Command command, string text) : base(menuType, text)
        {
            //	Make sure the command is non-null.
            Debug.Assert(command != null, "CommandOwnerDrawMenuItem - Command was null.");
            if (command == null)
            {
                return;
            }

            Debug.Assert(text != null, "Text was null");

            //	Set the command.
            this.Command = command;

            //	Initialize the menu item.
            if (menuType == MenuType.Main)
            {
                this.ShowShortcut = command.ShowShortcut;
                this.Shortcut = command.Shortcut;
                this.Visible = command.VisibleOnMainMenu;
            }
            else if (menuType == MenuType.Context)
            {
                this.ShowShortcut = false;
                this.Shortcut = Shortcut.None;
                this.Visible = command.VisibleOnContextMenu;
            }
            else if (menuType == MenuType.CommandBarContext)
            {
                this.ShowShortcut = command.ShowShortcut;
                this.Shortcut = command.Shortcut;
                this.Visible = command.VisibleOnContextMenu;
            }
            else
            {
                Trace.Assert(false, "CommandOwnerDrawMenuItem - MenuType is not supported.");
                this.ShowShortcut = false;
                this.Shortcut = Shortcut.None;
            }
            this.Enabled = command.Enabled;

            //	Add event handlers for the command events.
            command.StateChanged += this.command_StateChanged;
            command.VisibleOnContextMenuChanged += this.command_VisibleOnContextMenuChanged;
            command.VisibleOnMainMenuChanged += this.command_VisibleOnMainMenuChanged;
        }

        /// <summary>
        /// Gets the MenuID of this MenuItem.
        /// </summary>
        /// <returns>The MenuID.</returns>
        public int GetMenuID() => this.MenuID;

        /// <summary>
        /// Dispose of the CommandOwnerDrawMenuItem.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            //	If we're disposing of managed resources, remove event handlers for the command
            //	events and release our reference to the command.
            if (disposing && this.Command != null)
            {
                this.Command.StateChanged -= this.command_StateChanged;
                this.Command.VisibleOnContextMenuChanged -= this.command_VisibleOnContextMenuChanged;
                this.Command.VisibleOnMainMenuChanged -= this.command_VisibleOnMainMenuChanged;
                this.Command = null;
            }

            //	Call the base class's Dispose method.
            base.Dispose(disposing);
        }

        /// <summary>
        /// Returns the bitmap to draw for the owner draw menu item.
        /// </summary>
        /// <param name="drawItemState">The draw item state.</param>
        /// <returns>The bitmap to draw.</returns>
        protected override Bitmap MenuBitmap(DrawItemState drawItemState)
        {
            if (!this.Command.Latched && (drawItemState & DrawItemState.Disabled) != 0)
            {
                return this.Command.MenuBitmapDisabled;
            }
            else if ((drawItemState & DrawItemState.Selected) != 0)
            {
                return this.Command.Latched ? this.Command.MenuBitmapLatchedSelected : this.Command.MenuBitmapSelected;
            }
            else
            {
                return this.Command.Latched ? this.Command.MenuBitmapLatchedEnabled : this.Command.MenuBitmapEnabled;
            }
        }

        /// <summary>
        ///	Menu text method.  Returns the text to draw for the owner draw menu item.
        /// </summary>
        /// <returns>Text to draw.</returns>
        protected override string MenuText()
        {
            //	The the text is devoid of format arguments, just return it.
            if (this.text.IndexOf("{") == -1)
            {
                return this.text;
            }
            else
            {
                //	Set the format arguments.
                var formatArgs = this.Command.MenuFormatArgs;

                //	Format the text, using default arguments if necessary.
                if (formatArgs == null)
                {
                    return string.Format(CultureInfo.CurrentCulture, this.text, new object[] { string.Empty, string.Empty, string.Empty });
                }
                else
                {
                    return string.Format(CultureInfo.CurrentCulture, this.text, formatArgs);
                }
            }
        }

        /// <summary>
        /// Raises the BeforeShow event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnBeforeShow(EventArgs e)
        {
            //	Raise the BeforeShowInMenu event on the command.
            this.Command.InvokeBeforeShowInMenu(EventArgs.Empty);

            this.Checked = this.Command.Latched;
            this.Text = this.MenuText();

            //	Arrrgh!
            bool visibleState;
            switch (this.MenuType)
            {
                case MenuType.Context:
                case MenuType.CommandBarContext:
                    visibleState = this.Command.VisibleOnContextMenu;
                    break;

                case MenuType.Main:
                    visibleState = this.Command.VisibleOnMainMenu;
                    break;

                default:
                    Trace.Assert(false, "CommandOwnerDrawMenuItem - MenuType is not supported.");
                    return;
            }

            if (this.Visible != visibleState)
            {
                this.Visible = visibleState;
            }
        }

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            //	Assert that the command is enabled.
            Debug.Assert(this.Command.On && this.Command.Enabled, "Click event raised for a command that is not on and enabled.");

            //	Execute the command.
            if (this.Command.On && this.Command.Enabled)
            {
                this.Command.PerformExecute();
            }
        }

        /// <summary>
        /// command_StateChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_StateChanged(object sender, EventArgs e)
        {
            var enabled = this.Command.On && this.Command.Enabled;
            if (this.Enabled != enabled)
            {
                this.Enabled = enabled;
            }
        }

        /// <summary>
        /// command_VisibleOnContextMenuChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_VisibleOnContextMenuChanged(object sender, EventArgs e)
        {
            if (this.MenuType == MenuType.Context || this.MenuType == MenuType.CommandBarContext)
            {
                this.Visible = this.Command.VisibleOnContextMenu;
            }
        }

        /// <summary>
        /// command_VisibleOnMainMenuChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_VisibleOnMainMenuChanged(object sender, EventArgs e)
        {
            if (this.MenuType == MenuType.Main)
            {
                this.Visible = this.Command.VisibleOnMainMenu;
            }
        }
    }
}
