// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
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
                return;
            Debug.Assert(text != null, "Text was null");

            //	Set the command.
            Command = command;

            //	Initialize the menu item.
            if (menuType == MenuType.Main)
            {
                ShowShortcut = command.ShowShortcut;
                Shortcut = command.Shortcut;
                Visible = command.VisibleOnMainMenu;
            }
            else if (menuType == MenuType.Context)
            {
                ShowShortcut = false;
                Shortcut = Shortcut.None;
                Visible = command.VisibleOnContextMenu;
            }
            else if (menuType == MenuType.CommandBarContext)
            {
                ShowShortcut = command.ShowShortcut;
                Shortcut = command.Shortcut;
                Visible = command.VisibleOnContextMenu;
            }
            else
            {
                Trace.Assert(false, "CommandOwnerDrawMenuItem - MenuType is not supported.");
                ShowShortcut = false;
                Shortcut = Shortcut.None;
            }

            Enabled = command.Enabled;

            //	Add event handlers for the command events.
            command.StateChanged += command_StateChanged;
            command.VisibleOnContextMenuChanged += command_VisibleOnContextMenuChanged;
            command.VisibleOnMainMenuChanged += command_VisibleOnMainMenuChanged;
        }

        /// <summary>
        /// Gets the MenuID of this MenuItem.
        /// </summary>
        /// <returns>The MenuID.</returns>
        public int GetMenuID()
        {
            return MenuID;
        }

        /// <summary>
        /// Dispose of the CommandOwnerDrawMenuItem.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            //	If we're disposing of managed resources, remove event handlers for the command
            //	events and release our reference to the command.
            if (disposing && Command != null)
            {
                Command.StateChanged -= command_StateChanged;
                Command.VisibleOnContextMenuChanged -= command_VisibleOnContextMenuChanged;
                Command.VisibleOnMainMenuChanged -= command_VisibleOnMainMenuChanged;
                Command = null;
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
            if (!Command.Latched && (drawItemState & DrawItemState.Disabled) != 0)
                return Command.MenuBitmapDisabled;
            else if ((drawItemState & DrawItemState.Selected) != 0)
                return Command.Latched ? Command.MenuBitmapLatchedSelected : Command.MenuBitmapSelected;
            else
                return Command.Latched ? Command.MenuBitmapLatchedEnabled : Command.MenuBitmapEnabled;
        }

        /// <summary>
        ///	Menu text method.  Returns the text to draw for the owner draw menu item.
        /// </summary>
        /// <returns>Text to draw.</returns>
        protected override string MenuText()
        {
            //	The the text is devoid of format arguments, just return it.
            if (text.IndexOf("{") == -1)
                return text;
            else
            {
                //	Set the format arguments.
                object[] formatArgs = Command.MenuFormatArgs;

                //	Format the text, using default arguments if necessary.
                if (formatArgs == null)
                    return String.Format(CultureInfo.CurrentCulture, text, new object[] { string.Empty, string.Empty, string.Empty });
                else
                    return String.Format(CultureInfo.CurrentCulture, text, formatArgs);
            }
        }

        /// <summary>
        /// Raises the BeforeShow event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnBeforeShow(EventArgs e)
        {
            //	Raise the BeforeShowInMenu event on the command.
            Command.InvokeBeforeShowInMenu(EventArgs.Empty);

            Checked = Command.Latched;
            Text = MenuText();

            //	Arrrgh!
            bool visibleState;
            switch (MenuType)
            {
                case MenuType.Context:
                case MenuType.CommandBarContext:
                    visibleState = Command.VisibleOnContextMenu;
                    break;

                case MenuType.Main:
                    visibleState = Command.VisibleOnMainMenu;
                    break;

                default:
                    Trace.Assert(false, "CommandOwnerDrawMenuItem - MenuType is not supported.");
                    return;
            }

            if (Visible != visibleState)
                Visible = visibleState;
        }

        /// <summary>
        /// Raises the Click event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            //	Assert that the command is enabled.
            Debug.Assert(Command.On && Command.Enabled, "Click event raised for a command that is not on and enabled.");

            //	Execute the command.
            if (Command.On && Command.Enabled)
                Command.PerformExecute();
        }

        /// <summary>
        /// command_StateChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_StateChanged(object sender, EventArgs e)
        {
            bool enabled = Command.On && Command.Enabled;
            if (Enabled != enabled)
                Enabled = enabled;
        }

        /// <summary>
        /// command_VisibleOnContextMenuChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_VisibleOnContextMenuChanged(object sender, EventArgs e)
        {
            if (MenuType == MenuType.Context || MenuType == MenuType.CommandBarContext)
                Visible = Command.VisibleOnContextMenu;
        }

        /// <summary>
        /// command_VisibleOnMainMenuChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_VisibleOnMainMenuChanged(object sender, EventArgs e)
        {
            if (MenuType == MenuType.Main)
                Visible = Command.VisibleOnMainMenu;
        }
    }
}
