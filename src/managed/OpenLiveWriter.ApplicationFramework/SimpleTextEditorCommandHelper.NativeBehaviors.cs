// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    ///     Class SimpleTextEditorCommandHelper.
    /// </summary>
    public partial class SimpleTextEditorCommandHelper
    {
        /// <summary>
        ///     Class NativeBehaviors.
        ///     Implements the <see cref="System.IDisposable" />
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        public class NativeBehaviors : IDisposable
        {
            /// <summary>
            ///     The command manager
            /// </summary>
            private readonly CommandManager CommandManager;

            /// <summary>
            ///     The controls
            /// </summary>
            private readonly Control[] Controls;

            /// <summary>
            ///     Initializes a new instance of the <see cref="NativeBehaviors" /> class.
            /// </summary>
            /// <param name="commandManager">The command manager.</param>
            /// <param name="controls">The controls.</param>
            public NativeBehaviors(CommandManager commandManager, params Control[] controls)
            {
                this.Controls = controls;
                this.CommandManager = commandManager;
                foreach (var c in this.Controls)
                {
                    c.GotFocus += this.c_GotFocus;
                    c.LostFocus += this.c_LostFocus;
                }
            }

            /// <summary>
            ///     Call this method to ensure that the passed control
            ///     does NOT get to handle cut, copy, paste, undo, redo,
            ///     and del natively, but gets passed through the
            ///     SimpleTextEditorCommand system instead.
            /// </summary>
            public void Dispose()
            {
                foreach (var c in this.Controls)
                {
                    c.GotFocus -= this.c_GotFocus;
                    c.LostFocus -= this.c_LostFocus;
                }
            }

            /// <summary>
            ///     Handles the GotFocus event of the c control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
            private void c_GotFocus(object sender, EventArgs e)
            {
                this.CommandManager.IgnoreShortcut(Shortcut.CtrlZ);
                this.CommandManager.IgnoreShortcut(Shortcut.CtrlY);
                this.CommandManager.IgnoreShortcut(Shortcut.CtrlX);
                this.CommandManager.IgnoreShortcut(Shortcut.CtrlC);
                this.CommandManager.IgnoreShortcut(Shortcut.CtrlV);
                this.CommandManager.IgnoreShortcut(Shortcut.Del);
            }

            /// <summary>
            ///     Handles the LostFocus event of the c control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
            private void c_LostFocus(object sender, EventArgs e)
            {
                this.CommandManager.UnignoreShortcut(Shortcut.CtrlZ);
                this.CommandManager.UnignoreShortcut(Shortcut.CtrlY);
                this.CommandManager.UnignoreShortcut(Shortcut.CtrlX);
                this.CommandManager.UnignoreShortcut(Shortcut.CtrlC);
                this.CommandManager.UnignoreShortcut(Shortcut.CtrlV);
                this.CommandManager.UnignoreShortcut(Shortcut.Del);
            }
        }
    }
}