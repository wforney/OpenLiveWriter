// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Provides a control which is useful for providing a border for, or replacing the border of,
    /// another control.
    /// </summary>
    public class BorderControl : UserControl
    {
        /// <summary>
        /// The border size.
        /// </summary>
        private const int BORDER_SIZE = 1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The theme border color color.
        /// </summary>
        private Color themeBorderColor;

        /// <summary>
        /// The FocusWatchingUserControl that contains the control.  This allows us to hide borders
        /// or add borders to any control.
        /// </summary>
        private FocusWatchingUserControl focusWatchingUserControl;

        /// <summary>
        /// The control that this BorderControl is providing a border for.
        /// </summary>
        private Control control;

        /// <summary>
        ///
        /// </summary>
        private bool themeBorder = false;

        /// <summary>
        /// The top inset.
        /// </summary>
        private int topInset;

        /// <summary>
        /// The left inset.
        /// </summary>
        private int leftInset;

        /// <summary>
        /// The bottom inset.
        /// </summary>
        private int bottomInset;

        /// <summary>
        /// True if bottom border should not be used.
        /// </summary>
        private bool suppressBottomBorder;

        /// <summary>
        /// The right inset.
        /// </summary>
        private int rightInset;

        /// <summary>
        /// Initializes a new instance of the BorderObscuringControl class.
        /// </summary>
        public BorderControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            //	Set the theme border color.
            this.themeBorderColor = SystemColors.ControlDark;
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
        private void InitializeComponent()
        {
            this.focusWatchingUserControl = new FocusWatchingUserControl();
            this.SuspendLayout();
            //
            // focusWatchingUserControl
            //
            this.focusWatchingUserControl.Anchor = AnchorStyles.None;
            this.focusWatchingUserControl.BackColor = SystemColors.Window;
            this.focusWatchingUserControl.Location = new Point(1, 1);
            this.focusWatchingUserControl.Name = "focusWatchingUserControl";
            this.focusWatchingUserControl.Size = new Size(148, 148);
            this.focusWatchingUserControl.TabStop = false;
            //this.focusWatchingUserControl.TabIndex = 0;
            //
            // BorderControl
            //
            this.Controls.Add(this.focusWatchingUserControl);
            this.Name = "BorderControl";
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Gets or sets
        /// </summary>
        public Control Control
        {
            get => this.control;
            set
            {
                if (this.control != value)
                {
                    if (this.control != null)
                    {
                        this.control.EnabledChanged -= new EventHandler(this.control_EnabledChanged);
                        this.control.Parent = null;
                    }

                    this.control = value;

                    if (this.control != null)
                    {
                        this.control.EnabledChanged += new EventHandler(this.control_EnabledChanged);
                        this.control.Parent = this.focusWatchingUserControl;
                        this.focusWatchingUserControl.BackColor = this.control.Enabled ? this.control.BackColor : SystemColors.Control;
                    }
                }
            }
        }

        /// <summary>Gets or sets a value indicating whether [theme border].</summary>
        /// <value>
        ///   <c>true</c> if [theme border]; otherwise, <c>false</c>.</value>
        public bool ThemeBorder
        {
            get => this.themeBorder;
            set
            {
                if (this.themeBorder != value)
                {
                    this.themeBorder = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the height of the BorderControl is
        /// automatically determined based on the height of the control.
        /// </summary>
        public bool AutoHeight { get; set; } = false;

        /// <summary>
        /// Gets or sets the top inset.
        /// </summary>
        public int TopInset
        {
            get => this.topInset;
            set
            {
                if (this.topInset != value)
                {
                    this.topInset = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the left inset.
        /// </summary>
        public int LeftInset
        {
            get => this.leftInset;
            set
            {
                if (this.leftInset != value)
                {
                    this.leftInset = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bottom inset.
        /// </summary>
        public int BottomInset
        {
            get => this.bottomInset;
            set
            {
                if (this.bottomInset != value)
                {
                    this.bottomInset = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [suppress bottom border].
        /// </summary>
        /// <value><c>true</c> if [suppress bottom border]; otherwise, <c>false</c>.</value>
        public bool SuppressBottomBorder
        {
            get => this.suppressBottomBorder;
            set
            {
                if (this.suppressBottomBorder != value)
                {
                    this.suppressBottomBorder = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the right inset.
        /// </summary>
        public int RightInset
        {
            get => this.rightInset;
            set
            {
                if (this.rightInset != value)
                {
                    this.rightInset = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the background color for the control.
        /// </summary>
        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                this.focusWatchingUserControl.BackColor = this.BackColor;
            }
        }

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new Left property value of the control.</param>
        /// <param name="y">The new Right property value of the control.</param>
        /// <param name="width">The new Width property value of the control.</param>
        /// <param name="height">The new Height property value of the control.</param>
        /// <param name="specified">A bitwise combination of the BoundsSpecified values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            //	If this is an auto-height BorderControl, and we have a control, constrain the height
            //	of the BorderControl based on the height of the control.
            if (this.AutoHeight && this.control != null)
            {
                height = this.control.Size.Height + this.topInset + this.bottomInset + (BORDER_SIZE * 2);
            }

            //	Call the base class's method.
            base.SetBoundsCore(x, y, width, height, specified);
        }

        /// <summary>
        /// Raises the SystemColorsChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnSystemColorsChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnSystemColorsChanged(e);

            //	Obtain the theme border color again.
            this.themeBorderColor = ColorHelper.GetThemeBorderColor(SystemColors.ControlDark);

            //	Invalidate.
            this.Invalidate();
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout the focusWatchingUserControl.
            this.focusWatchingUserControl.Bounds = this.themeBorder
                ? new Rectangle(1, 1, this.Width - 2, this.suppressBottomBorder ? this.Height - 1 : this.Height - 2)
                : new Rectangle(2, 2, this.Width - 4, this.suppressBottomBorder ? this.Height - 2 : this.Height - 4);

            //	Layout the control.
            if (this.control != null)
            {
                this.control.Bounds = new Rectangle(this.leftInset,
                                                this.topInset,
                                                this.focusWatchingUserControl.Width - this.rightInset,
                                                this.AutoHeight
                                                    ? this.control.Height
                                                    : this.focusWatchingUserControl.Height - (this.topInset + this.bottomInset));
            }

            //	Make sure the control gets repainted.
            this.Invalidate();
        }

        /// <summary>
        /// Raises the PaintBackground event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaintBackground(e);

            if (this.themeBorder)
            {
                //	Draw the border.
                using (var pen = new Pen(this.themeBorderColor))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
                }
            }
            else
            {
                ControlPaint.DrawBorder3D(e.Graphics, this.ClientRectangle, Border3DStyle.Sunken);
            }
        }

        /// <summary>
        /// control_EnabledChanged event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void control_EnabledChanged(object sender, EventArgs e)
        {
            this.focusWatchingUserControl.BackColor = this.control.Enabled ? this.control.BackColor : SystemColors.Control;
            this.focusWatchingUserControl.Invalidate();
        }
    }
}
