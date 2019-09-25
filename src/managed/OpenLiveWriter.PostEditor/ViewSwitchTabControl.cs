// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using CoreServices;

    using Localization.Bidi;

    /// <summary>
    /// The ViewSwitchTabControl class.
    /// Implements the <see cref="System.Windows.Forms.Control" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Control" />
    public partial class ViewSwitchTabControl : Control
    {
        /// <summary>
        /// The clip left border
        /// </summary>
        private bool clipLeftBorder;

        /// <summary>
        /// The clip right border
        /// </summary>
        private bool clipRightBorder;

        /// <summary>
        /// The selected
        /// </summary>
        private bool selected;

        /// <inheritdoc />
        public ViewSwitchTabControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint
                        | ControlStyles.DoubleBuffer
                        | ControlStyles.OptimizedDoubleBuffer
                        | ControlStyles.SupportsTransparentBackColor,
                          true);

            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ViewSwitchTabControl"/> is selected.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        public bool Selected
        {
            get => this.selected;
            set
            {
                if (value == this.selected)
                {
                    return;
                }

                this.selected = value;
                var accessibilityObject = this.AccessibilityObject;
                if (accessibilityObject != null)
                {
                    accessibilityObject.Name = this.Text + (value ? "*" : "");
                }

                if (this.selected)
                {
                    this.Select();
                }

                this.PerformLayout();
                this.Invalidate();

                this.SelectedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [clip left border].
        /// </summary>
        /// <value><c>true</c> if [clip left border]; otherwise, <c>false</c>.</value>
        public bool ClipLeftBorder
        {
            get => this.clipLeftBorder;
            set
            {
                if (value == this.clipLeftBorder)
                {
                    return;
                }

                this.clipLeftBorder = value;
                this.PerformLayout();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [clip right border].
        /// </summary>
        /// <value><c>true</c> if [clip right border]; otherwise, <c>false</c>.</value>
        public bool ClipRightBorder
        {
            get => this.clipRightBorder;
            set
            {
                if (value == this.clipRightBorder)
                {
                    return;
                }

                this.clipRightBorder = value;
                this.PerformLayout();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the shortcut.
        /// </summary>
        /// <value>The shortcut.</value>
        public string Shortcut { internal get; set; }

        /// <summary>
        /// Occurs when [selected changed].
        /// </summary>
        public event EventHandler SelectedChanged;

        /// <inheritdoc />
        protected override void OnTextChanged(EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
            base.OnTextChanged(e);
        }

        /// <inheritdoc />
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Invalidate();
        }

        /// <inheritdoc />
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Invalidate();
        }

        /// <inheritdoc />
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyData == Keys.Space || e.KeyData == Keys.Enter)
            {
                this.Selected = true;
            }
        }

        /// <inheritdoc />
        protected override void OnLayout(LayoutEventArgs layoutEventArgs)
        {
            var size = TextRenderer.MeasureText(this.Text, this.Font, Size.Empty, TextFormatFlags.NoPrefix);
            if (size.Height == 0 || size.Width == 0)
            {
                // We're in some weird state, do nothing
                return;
            }

            size.Width += (int)Math.Ceiling(DisplayHelper.ScaleX(20)); // Scale tab width for DPI, however height doesn't look quite right
            size.Height += 8;

            // Extra height for Selected state, which we won't use if we aren't selected
            size.Height += 2;

            if (this.clipLeftBorder)
            {
                size.Width -= 1;
            }

            if (this.clipRightBorder)
            {
                size.Width -= 1;
            }

            this.Size = size;
        }

        /// <summary>
        /// Handles the <see cref="E:Paint" /> event.
        /// </summary>
        /// <param name="pe">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected override void OnPaint(PaintEventArgs pe)
        {
            Color bgColor, borderColor, textColor;
            Color lineColor = new HCColor(0xA5A5A5, SystemColors.ControlDarkDark);

            if (this.Selected)
            {
                bgColor = new HCColor(Color.White, SystemColors.Control);
                textColor = new HCColor(0x333333, SystemColors.ControlText);
                borderColor = new HCColor(0xA5A5A5, SystemColors.ControlDarkDark);
            }
            else
            {
                bgColor = new HCColor(0xF7F7F7, SystemColors.Control);
                textColor = new HCColor(0x6E6E6E, SystemColors.ControlText);
                borderColor = new HCColor(0xC9C9C9, SystemColors.ControlDark);
            }

            var g = new BidiGraphics(pe.Graphics, this.ClientRectangle);

            this.DrawTabFace(g, bgColor, borderColor, lineColor);
            this.DrawTabContents(g, textColor);
        }

        /// <summary>
        /// Draws the tab face.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="bgColor">Color of the bg.</param>
        /// <param name="borderColor">Color of the border.</param>
        /// <param name="lineColor">Color of the line.</param>
        private void DrawTabFace(BidiGraphics g, Color bgColor, Color borderColor, Color lineColor)
        {
            var borderRect = this.ClientRectangle;

            if (!this.Selected)
            {
                borderRect.Height -= 2;
            }

            // Remove one pixel for the bottom edge of the tab.
            // We don't want it filled in by the face color as
            // that would cause the corner pixels of the tab
            // to be filled in.
            borderRect.Height -= 1;

            using (Brush b = new SolidBrush(bgColor))
            {
                g.Graphics.FillRectangle(b, borderRect);
            }

            borderRect.Width -= 1;

            if (this.Selected)
            {
                borderRect.Y -= 1;
            }
            else
            {
                borderRect.Height -= 1;
            }

            if (this.clipLeftBorder)
            {
                borderRect.X -= 1;
                borderRect.Width += 1;
            }

            if (this.clipRightBorder)
            {
                borderRect.Width += 1;
            }

            var clip = g.Graphics.Clip;
            clip.Exclude(g.TranslateRectangle(new Rectangle(borderRect.X, borderRect.Bottom, 1, 1)));
            clip.Exclude(g.TranslateRectangle(new Rectangle(borderRect.Right, borderRect.Bottom, 1, 1)));
            g.Graphics.Clip = clip;

            using (var p = new Pen(borderColor))
            {
                g.DrawRectangle(p, borderRect);
            }

            if (this.Selected)
            {
                return;
            }

            using (var p = new Pen(lineColor))
            {
                g.DrawLine(p, 0, 0, this.ClientSize.Width, 0);
            }
        }

        /// <summary>
        /// Draws the tab contents.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="textColor">Color of the text.</param>
        private void DrawTabContents(BidiGraphics g, Color textColor)
        {
            var logicalClientRect = this.ClientRectangle;

            if (!this.Selected)
            {
                logicalClientRect.Height -= 2;
            }

            // Make up for the top "border" being just off the top of the control
            logicalClientRect.Y -= 1;
            logicalClientRect.Height += 1;

            if (this.clipLeftBorder)
            {
                logicalClientRect.X -= 1;
                logicalClientRect.Width += 1;
            }

            if (this.clipRightBorder)
            {
                logicalClientRect.Width += 1;
            }

            g.DrawText(this.Text, this.Font, logicalClientRect, textColor, Color.Transparent,
                       TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            //            if (Focused)
            //            {
            //                Rectangle rect = logicalClientRect;
            //                rect.Inflate(-2, -2);
            //                g.DrawFocusRectangle(rect);
            //            }
        }
    }
}
