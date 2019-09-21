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
    /// WorkspaceColumnPaneLightweightControl.
    /// </summary>
    public class WorkspaceColumnPaneLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected Container components = null;

        /// <summary>
        /// The control.
        /// </summary>
        private Control control;

        /// <summary>
        /// The lightweight control.
        /// </summary>
        private LightweightControl lightweightControl;

        /// <summary>
        /// A value which indicates whether the pane should be layed out with a fixed height, when
        /// possible.
        /// </summary>
        private bool fixedHeightLayout;

        /// <summary>
        /// The fixed height to be used when the FixedHeightLayout property is true.
        /// </summary>
        private int fixedHeight;

        /// <summary>
        /// Initializes a new instance of the DummyPaneLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public WorkspaceColumnPaneLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the DummyPaneLightweightControl class.
        /// </summary>
        public WorkspaceColumnPaneLightweightControl() =>
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((ISupportInitialize)this).BeginInit();
            ((ISupportInitialize)this).EndInit();
        }

        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        public Control Control
        {
            get => this.control;
            set
            {
                //	If the control is changing, change it.
                if (this.control != value)
                {
                    //	Clear.
                    this.Clear();

                    //	Set the new control.
                    if ((this.control = value) != null)
                    {
                        this.control.Parent = this.Parent;
                        this.Visible = true;
                        this.PerformLayout();
                        this.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the control.
        /// </summary>
        public LightweightControl LightweightControl
        {
            get => this.lightweightControl;
            set
            {
                //	If the control is changing, change it.
                if (this.lightweightControl != value)
                {
                    //	Clear.
                    this.Clear();

                    //	Set the new control.
                    if ((this.lightweightControl = value) != null)
                    {
                        this.lightweightControl.LightweightControlContainerControl = this;
                        this.Visible = true;
                        this.PerformLayout();
                        this.Invalidate();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a border will be drawn.
        /// </summary>
        public bool Border { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the pane should be layed out with a fixed
        /// height, when possible.
        /// </summary>
        public bool FixedHeightLayout
        {
            get => this.fixedHeightLayout;
            set
            {
                if (this.fixedHeightLayout != value)
                {
                    this.fixedHeightLayout = value;
                    this.Parent.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fixed height to be used when the FixedHeightLayout property is true.
        /// </summary>
        public int FixedHeight
        {
            get => this.fixedHeight;
            set
            {
                if (this.fixedHeight != value)
                {
                    this.fixedHeight = value;
                    this.Parent.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            if (this.Parent == null)
            {
                return;
            }

            //	Layout the control.
            if (this.control != null)
            {
                var layoutRectangle = this.VirtualClientRectangle;
                if (this.Border)
                {
                    layoutRectangle.Inflate(-1, -1);
                }

                this.control.Bounds = this.VirtualClientRectangleToParent(layoutRectangle);
            }
            else if (this.lightweightControl != null)
            {
                var layoutRectangle = this.VirtualClientRectangle;
                if (this.Border)
                {
                    layoutRectangle.Inflate(-1, -1);
                }

                this.lightweightControl.VirtualBounds = layoutRectangle;
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            //	If we're drawing a border, draw it.
            if (this.Border)
            {
                using (var pen = new Pen(ApplicationManager.ApplicationStyle.BorderColor))
                {
                    e.Graphics.DrawRectangle(pen,
                                                this.VirtualClientRectangle.X,
                                                this.VirtualClientRectangle.Y,
                                                this.VirtualClientRectangle.Width - 1,
                                                this.VirtualClientRectangle.Height - 1);
                }
            }
        }

        /// <summary>
        /// Raises the VisibleChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnVisibleChanged(e);

            //	Ensure that the Control/LightweightControl Visible property is matched.
            if (this.control != null)
            {
                this.control.Visible = this.Visible;
            }
            else if (this.lightweightControl != null)
            {
                this.lightweightControl.Visible = this.Visible;
            }
        }

        /// <summary>
        /// Clears the workspace column pane.
        /// </summary>
        private void Clear()
        {
            //	If there's a control or a lightweight control, remove it.
            if (this.control != null)
            {
                this.control.Parent = null;
                this.control.Dispose();
                this.control = null;
            }
            else if (this.lightweightControl != null)
            {
                this.lightweightControl.LightweightControlContainerControl = null;
                this.lightweightControl.Dispose();
                this.lightweightControl = null;
            }

            //	Poof!  We're invisible.
            this.Visible = false;
        }
    }
}
