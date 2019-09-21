// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;

    /// <summary>
    /// CommandBar control lightweight control.  Allows any control to be placed on a CommandBar.
    /// </summary>
    internal class CommandBarControlLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Gets or sets the control for the command bar control lightweight control.
        /// </summary>
        public Control Control { get; set; }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarControlLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlLightweightControl class.
        /// </summary>
        public CommandBarControlLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
            this.InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarControlLightweightControl class.
        /// </summary>
        public CommandBarControlLightweightControl(Control control)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
            this.InitializeObject();
            this.Control = control;
            control.TabStop = false;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();
        #endregion

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject() => this.TabStop = true;

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);
            this.VirtualSize = new Size(this.Control.Width + 4, this.Control.Height);
        }

        private void SyncControlLocation()
        {
            var bounds = new Rectangle(
                this.VirtualClientPointToParent(new Point(2, 0)),
                this.Control.Size);
            this.Control.Location = bounds.Location;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Fix bug 722229: paragraph button doesn't show in the right place for RTL builds
            // We used to do SyncControlLocation() in OnLayout. The problem is that OnLayout
            // only occurs when this lightweight control is moved relative to its immediate
            // parent--it doesn't get called when its parent is moved relative to the grandparent.
            // Since layout happens from the bottom of the hierarchy up, and the heavyweight
            // control's coordinate system is relative to the parent heavyweight control, the
            // two coordinate systems would get out of sync.
            //
            // By moving the call to OnPaint we can be confident that the heavyweight control
            // will be moved after all layout has completed.
            this.SyncControlLocation();

            base.OnPaint(e);
        }

        /// <summary>
        /// Raises the LightweightControlContainerControlChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLightweightControlContainerControlChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLightweightControlContainerControlChanged(e);

            if (this.Control.Parent != this.Parent)
            {
                this.Control.Parent = this.Parent;
            }

            if (this.Parent != null)
            {
                this.PerformLayout();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Focuses this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Focus()
        {
            this.Control.Focus();
            return base.Focus();
        }

        /// <summary>
        /// Creates the accessibility instance.
        /// </summary>
        /// <returns>AccessibleObject.</returns>
        protected override AccessibleObject CreateAccessibilityInstance() => this.Control.AccessibilityObject;
    }
}
