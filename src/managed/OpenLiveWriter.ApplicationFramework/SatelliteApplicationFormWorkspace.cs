// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>
    ///     Class SatelliteApplicationFormWorkspace.
    ///     Implements the <see cref="System.Windows.Forms.UserControl" />
    ///     Implements the <see cref="OpenLiveWriter.ApplicationFramework.IWorkspaceBorderManager" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.IWorkspaceBorderManager" />
    [Obsolete]
    internal class SatelliteApplicationFormWorkspace : UserControl, IWorkspaceBorderManager
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        ///     The panel bottom
        /// </summary>
        private readonly Panel panelBottom;

        /// <summary>
        ///     The panel left
        /// </summary>
        private readonly Panel panelLeft;

        /// <summary>
        ///     The panel main
        /// </summary>
        private readonly Panel panelMain;

        /// <summary>
        ///     The panel right
        /// </summary>
        private readonly Panel panelRight;

        /// <summary>
        ///     The panel top
        /// </summary>
        private readonly Panel panelTop;

        /// <summary>
        ///     The workspace inset
        /// </summary>
        private readonly int workspaceInset;

        /// <summary>
        ///     The main control
        /// </summary>
        private Control mainControl;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SatelliteApplicationFormWorkspace" /> class.
        /// </summary>
        public SatelliteApplicationFormWorkspace()
            : this(4)
        {
            // for designer
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SatelliteApplicationFormWorkspace" /> class.
        /// </summary>
        /// <param name="workspaceInset">The workspace inset.</param>
        public SatelliteApplicationFormWorkspace(int workspaceInset)
        {
            // record workspace inset
            this.workspaceInset = workspaceInset;

            this.SuspendLayout();

            // create main panel
            this.panelMain = new Panel
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(this.panelMain);

            // create border panels
            this.panelLeft = new Panel
            {
                Width = this.workspaceInset,
                Dock = DockStyle.Left
            };
            this.Controls.Add(this.panelLeft);

            this.panelRight = new Panel
            {
                Width = this.workspaceInset,
                Dock = DockStyle.Right
            };
            this.Controls.Add(this.panelRight);

            this.panelTop = new Panel
            {
                Height = this.workspaceInset,
                Dock = DockStyle.Top
            };
            this.Controls.Add(this.panelTop);

            this.panelBottom = new Panel
            {
                Height = this.workspaceInset, // was -2
                Dock = DockStyle.Bottom
            };
            this.Controls.Add(this.panelBottom);

            this.ResumeLayout();
        }

        /// <summary>
        ///     Gets or sets the workspace borders.
        /// </summary>
        /// <value>The workspace borders.</value>
        public WorkspaceBorder WorkspaceBorders
        {
            get =>
                (this.panelLeft.Visible ? WorkspaceBorder.Left : WorkspaceBorder.None)
              | (this.panelRight.Visible ? WorkspaceBorder.Right : WorkspaceBorder.None)
              | (this.panelTop.Visible ? WorkspaceBorder.Top : WorkspaceBorder.None)
              | (this.panelBottom.Visible ? WorkspaceBorder.Bottom : WorkspaceBorder.None);

            set
            {
                this.panelLeft.Visible = (value & WorkspaceBorder.Left) > 0;
                this.panelRight.Visible = (value & WorkspaceBorder.Right) > 0;
                this.panelTop.Visible = (value & WorkspaceBorder.Top) > 0;
                this.panelBottom.Visible = (value & WorkspaceBorder.Bottom) > 0;
            }
        }

        /// <summary>
        ///     Sets the main control.
        /// </summary>
        /// <param name="newMainControl">The main control.</param>
        public void SetMainControl(Control newMainControl)
        {
            // add main control to main panel
            this.mainControl = newMainControl;
            newMainControl.Dock = DockStyle.Fill;
            this.panelMain.Controls.Add(this.mainControl);

            // sync to application style
            this.UpdateAppearance();
        }

        /// <summary>
        ///     Updates the appearance.
        /// </summary>
        public void UpdateAppearance()
        {
            var backgroundColor = ApplicationManager.ApplicationStyle.SecondaryWorkspaceBottomColor;

            // set back-color of panels to correct application style
            this.panelLeft.BackColor = this.panelTop.BackColor =
                                           this.panelRight.BackColor = this.panelBottom.BackColor = backgroundColor;

            // set back color of the main control to the correct application style
            this.mainControl.BackColor = backgroundColor;
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
