// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;

    using OpenLiveWriter.Controls;

    /// <summary>
    /// Use this if you need a command bar as a heavyweight control.
    /// Implements the <see cref="OpenLiveWriter.Controls.LightweightControlContainerControl" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.LightweightControlContainerControl" />
    public class CommandBarControl : LightweightControlContainerControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The command bar
        /// </summary>
        protected CommandBarLightweightControl commandBar;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBarControl"/> class.
        /// </summary>
        public CommandBarControl() => this.InitializeComponent();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBarControl"/> class.
        /// </summary>
        /// <param name="commandBar">The command bar.</param>
        /// <param name="commandBarDefinition">The command bar definition.</param>
        public CommandBarControl(CommandBarLightweightControl commandBar, CommandBarDefinition commandBarDefinition)
        {
            // It's important that the commandBarDefinition not be set
            // on the command bar until after the command bar has a heavyweight parent.
            // Otherwise, command bar entries that have heavyweight controls will never
            // be parented properly and therefore never show up.

            Debug.Assert(commandBar.CommandBarDefinition == null,
                         "Don't set the command bar definition before creating CommandBarControl!");

            this.InitializeComponent();

            this.commandBar = commandBar;

            this.commandBar.LightweightControlContainerControl = this;
            this.commandBar.CommandBarDefinition = commandBarDefinition;

            this.commandBar.VirtualBounds = new Rectangle(new Point(0, 0), this.commandBar.DefaultVirtualSize);
            this.Height = this.commandBar.VirtualHeight;
            SizeChanged += new EventHandler(this.CommandBarControl_SizeChanged);

            this.AccessibleRole = System.Windows.Forms.AccessibleRole.ToolBar;
            this.AccessibleName = commandBar.AccessibleName;
            this.InitFocusAndAccessibility();
        }

        /// <summary>
        /// Initializes the focus and accessibility.
        /// </summary>
        private void InitFocusAndAccessibility()
        {
            this.InitFocusManager();
            this.AddFocusableControls(this.commandBar.GetAccessibleControls());
        }

        /// <summary>
        /// Gets the command bar lightweight control.
        /// </summary>
        /// <value>The command bar lightweight control.</value>
        public CommandBarLightweightControl CommandBarLightweightControl => this.commandBar;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
        }

        /// <summary>
        /// Handles the SizeChanged event of the CommandBarControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CommandBarControl_SizeChanged(object sender, EventArgs e)
        {
            this.commandBar.VirtualWidth = this.Width;
            this.commandBar.VirtualHeight = this.Height;
        }
    }
}
