// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Class ColorButton.
    /// Implements the <see cref="Control" />
    /// </summary>
    /// <seealso cref="Control" />
    public class ColorButton : Control
    {
        /// <summary>
        /// Occurs when [color selected].
        /// </summary>
        public event ColorSelectedEventHandler ColorSelected;

        /// <summary>
        /// Occurs when [navigate].
        /// </summary>
        public event GridNavigateEventHandler Navigate;

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ColorButton"/> is selected.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        public bool Selected { get; set; }

        /// <summary>
        /// The square size
        /// </summary>
        private const int SQUARE_SIZE = 24;

        /// <summary>
        /// The border size
        /// </summary>
        private const int BORDER_SIZE = 20;

        /// <summary>
        /// The color well size
        /// </summary>
        private const int COLOR_WELL_SIZE = 18;

        /// <summary>
        /// The highlighted
        /// </summary>
        private bool _highlighted;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ColorButton"/> is highlighted.
        /// </summary>
        /// <value><c>true</c> if highlighted; otherwise, <c>false</c>.</value>
        private bool highlighted
        {
            get => this._highlighted;
            set
            {
                if (this._highlighted != value)
                {
                    this._highlighted = value;
                    this.Invalidate();
                }
            }

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorButton"/> class.
        /// </summary>
        /// <param name="color">The color.</param>
        public ColorButton(Color color) : base()
        {
            this.Width = SQUARE_SIZE;
            this.Height = SQUARE_SIZE;
            this.SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);
            this._highlighted = false;
            this.Color = color;
            this.KeyPress += new KeyPressEventHandler(this.ColorButton_KeyPress);
            this.Visible = true;
            this.TabStop = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.highlighted = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.highlighted = false;
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the window message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns><see langword="true" /> if the character was processed by the control; otherwise, <see langword="false" />.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Up:
                    this.highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Up));
                    break;
                case Keys.Down:
                    this.highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Down));
                    break;
                case Keys.Left:
                    this.highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Left));
                    break;
                case Keys.Right:
                    this.highlighted = false;
                    this.Navigate(this, new GridNavigateEventArgs(GridNavigateEventArgs.Direction.Right));
                    break;
                default:
                    return base.ProcessCmdKey(ref msg, keyData);
            }

            return true;
        }

        /// <summary>
        /// Handles the KeyPress event of the ColorButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyPressEventArgs"/> instance containing the event data.</param>
        private void ColorButton_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' || e.KeyChar == ' ')
            {
                ColorSelected(this, new ColorSelectedEventArgs(this.Color));
                return;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.highlighted = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.highlighted = false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Click" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.ColorSelected(this, new ColorSelectedEventArgs(this.Color));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var outerRect = new Rectangle(0, 0, this.Width, this.Height);
            var innerRect = RectangleHelper.Center(new Size(COLOR_WELL_SIZE, COLOR_WELL_SIZE), outerRect, false);

            if (this.Selected)
            {
                outerRect.Inflate(1, 1);
            }

            if (!this.highlighted)
            {
                outerRect.Inflate(COLOR_WELL_SIZE - BORDER_SIZE, COLOR_WELL_SIZE - BORDER_SIZE);
            }

            using (Brush b = new SolidBrush(SystemColors.Highlight))
            {
                e.Graphics.FillRectangle(b, outerRect);
            }

            using (Brush b = new SolidBrush(this.Color))
            {
                e.Graphics.FillRectangle(b, innerRect);
            }
        }
    }
}
