// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;

    /// <summary>
    ///     Represents the method that will handle the SplitterMoved event.
    /// </summary>
    public delegate void LightweightSplitterEventHandler(object sender, LightweightSplitterEventArgs e);

    /// <summary>
    ///     Splitter lightweight control.  Provides a horizontal or vertical splitter for use in a
    ///     multipane window with resizable panes.
    /// </summary>
    public partial class SplitterLightweightControl : LightweightControl
    {
        /// <summary>
        ///     The attached control.
        /// </summary>
        private LightweightControl attachedControl;

        /// <summary>
        ///     The layout rectangle for the attached control.
        /// </summary>
        private Rectangle attachedControlRectangle;

        /// <summary>
        ///     A value indicating whether the left mouse button is down.
        /// </summary>
        private bool leftMouseButtonDown;

        /// <summary>
        ///     The starting position of a move.
        /// </summary>
        private int startingPosition;

        /// <summary>
        ///     Initializes a new instance of the SplitterLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public SplitterLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the SplitterLightweightControl class.
        /// </summary>
        public SplitterLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
        }

        /// <summary>
        ///     Occurs when the splitter control begins a move operation.
        /// </summary>
        public event EventHandler SplitterBeginMove;

        /// <summary>
        ///     Occurs when the splitter control ends a move operation.
        /// </summary>
        public event LightweightSplitterEventHandler SplitterEndMove;

        /// <summary>
        ///     Occurs when the splitter control is moving.
        /// </summary>
        public event LightweightSplitterEventHandler SplitterMoving;

        /// <summary>
        ///     Gets or sets the LightweightControl attached to the center of the splitter bar.
        /// </summary>
        public LightweightControl AttachedControl
        {
            get => this.attachedControl;
            set
            {
                if (this.attachedControl != value)
                {
                    if (this.attachedControl != null)
                    {
                        this.LightweightControls.Remove(this.attachedControl);
                        this.attachedControl.MouseDown -= this._attachedControl_MouseDown;
                        this.attachedControl.MouseEnter -= this._attachedControl_MouseEnter;
                        this.attachedControl.MouseLeave -= this._attachedControl_MouseLeave;
                        this.attachedControl.MouseMove -= this._attachedControl_MouseMove;
                        this.attachedControl.MouseUp -= this._attachedControl_MouseUp;
                    }

                    this.attachedControl = value;
                    if (this.attachedControl != null)
                    {
                        this.LightweightControls.Add(this.attachedControl);
                        this.attachedControl.MouseDown += this._attachedControl_MouseDown;
                        this.attachedControl.MouseEnter += this._attachedControl_MouseEnter;
                        this.attachedControl.MouseLeave += this._attachedControl_MouseLeave;
                        this.attachedControl.MouseMove += this._attachedControl_MouseMove;
                        this.attachedControl.MouseUp += this._attachedControl_MouseUp;
                    }

                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets or sets the splitter orientation.
        /// </summary>
        [Category("Behavior")]
        [Localizable(false)]
        [DefaultValue(true)]
        [Description("Specifies whether the splitter is initially enabled.")]
        public bool Enabled { get; set; } = true;

        /// <summary>
        ///     Gets or sets the splitter orientation.
        /// </summary>
        [Category("Design")]
        [Localizable(false)]
        [DefaultValue(SplitterOrientation.Horizontal)]
        [Description("Specifies the the splitter orientation.")]
        public SplitterOrientation Orientation { get; set; } = SplitterOrientation.Horizontal;

        /// <summary>Handles the layout event.</summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            base.OnLayout(e);

            // If a control is attached to the splitter, lay it out.
            if (this.AttachedControl == null)
            {
                return;
            }

            // No layout required if this control is not visible.
            if (this.Parent?.Parent == null)
            {
                return;
            }

            // Layout the expand control.
            this.attachedControlRectangle = new Rectangle(
                Utility.CenterMinZero(this.AttachedControl.DefaultVirtualSize.Width, this.VirtualWidth),
                Utility.CenterMinZero(this.AttachedControl.DefaultVirtualSize.Height, this.VirtualHeight),
                this.AttachedControl.DefaultVirtualSize.Width,
                this.VirtualHeight);

            this.AttachedControl.VirtualBounds = this.attachedControlRectangle;
            this.AttachedControl.PerformLayout();
        }

        /// <summary>
        ///     Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            // Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);

            // Ignore the event if the splitter is disabled.
            if (!this.Enabled)
            {
                return;
            }

            // If the mouse button is the left button, begin a splitter resize.
            if (e.Button == MouseButtons.Left)
            {
                // Note that the left mouse button is down.
                this.leftMouseButtonDown = true;

                // Note the starting position.
                this.startingPosition = this.Orientation == SplitterOrientation.Vertical ? e.X : e.Y;

                // Raise the SplitterBeginMove event.
                this.OnSplitterBeginMove(EventArgs.Empty);
            }
        }

        /// <summary>
        ///     Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            // Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            // Ignore the event if the splitter is disabled.
            if (!this.Enabled)
            {
                return;
            }

            // Ensure that the left mouse button isn't down.
            Debug.Assert(!this.leftMouseButtonDown, "What?", "How can the left mouse button be down on mouse enter?");

            // Turn on the splitter cursor.
            this.Parent.Cursor = this.Orientation == SplitterOrientation.Vertical ? Cursors.VSplit : Cursors.HSplit;
        }

        /// <summary>
        ///     Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            // Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            // Ignore the event if the splitter is disabled.
            if (!this.Enabled)
            {
                return;
            }

            // If the left mouse button was down, end the resize operation.
            if (this.leftMouseButtonDown)
            {
                // Raise the event.
                this.OnSplitterEndMove(new LightweightSplitterEventArgs(0));

                // The left mouse button is not down.
                this.leftMouseButtonDown = false;
            }

            // Turn off the splitter cursor.
            this.Parent.Cursor = Cursors.Default;
        }

        /// <summary>
        ///     Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            // Call the base class's method so that registered delegates receive the event.
            base.OnMouseMove(e);

            // Ignore the event if the splitter is disabled.
            if (!this.Enabled)
            {
                return;
            }

            // If the left mouse button is down, continue the resize operation.
            if (this.leftMouseButtonDown)
            {
                // If we have one or more registered delegates for the SplitterMoving event, raise
                // the event.
                if (this.SplitterMoving != null)
                {
                    // Calculate the new position.
                    var newPosition = (this.Orientation == SplitterOrientation.Vertical ? e.X : e.Y)
                                    - this.startingPosition;

                    // Raise the SplitterMoving event.
                    this.OnSplitterMoving(new LightweightSplitterEventArgs(newPosition));
                }
            }
        }

        /// <summary>
        ///     Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            // 	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            // 	Ignore the event if the splitter is disabled.
            if (!this.Enabled)
            {
                return;
            }

            // 	If the left mouse button is down, end the resize operation.
            if (e.Button == MouseButtons.Left)
            {
                // 	Ensure that the left mouse button is down.
                Debug.Assert(this.leftMouseButtonDown, "What?", "Got a MouseUp that was unexpected.");
                if (this.leftMouseButtonDown)
                {
                    // 	Obtain the new position.
                    var newPosition = (this.Orientation == SplitterOrientation.Vertical ? e.X : e.Y)
                                    - this.startingPosition;

                    // 	Raise the event.
                    this.OnSplitterEndMove(new LightweightSplitterEventArgs(newPosition));

                    // 	The left mouse button is not down.
                    this.leftMouseButtonDown = false;
                }
            }
        }

        /// <summary>
        ///     Raises the SplitterBeginMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSplitterBeginMove(EventArgs e) => this.SplitterBeginMove?.Invoke(this, e);

        /// <summary>
        ///     Raises the SplitterEndMove event.
        /// </summary>
        /// <param name="e">A LightweightSplitterEventArgs that contains the event data.</param>
        protected virtual void OnSplitterEndMove(LightweightSplitterEventArgs e) => this.SplitterEndMove?.Invoke(this, e);

        /// <summary>
        ///     Raises the SplitterMoving event.
        /// </summary>
        /// <param name="e">A LightweightSplitterEventArgs that contains the event data.</param>
        protected virtual void OnSplitterMoving(LightweightSplitterEventArgs e) => this.SplitterMoving?.Invoke(this, e);

        /// <summary>
        ///     Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseDown(object sender, MouseEventArgs e) => this.OnMouseDown(e);

        /// <summary>
        ///     Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseEnter(object sender, EventArgs e) => this.OnMouseEnter(e);

        /// <summary>
        ///     Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseLeave(object sender, EventArgs e) => this.OnMouseLeave(e);

        /// <summary>
        ///     Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseMove(object sender, MouseEventArgs e) => this.OnMouseMove(e);

        /// <summary>
        ///     Propagates the mouse event from the attached control.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _attachedControl_MouseUp(object sender, MouseEventArgs e) => this.OnMouseUp(e);

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }
    }
}