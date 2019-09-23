// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.ApplicationFramework.Skinning;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    ///     Class MiniTab.
    ///     Implements the <see cref="OpenLiveWriter.Controls.LightweightControl" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.LightweightControl" />
    public class MiniTab : LightweightControl
    {
        /// <summary>
        ///     The CTX
        /// </summary>
        private readonly MiniTabContext ctx;

        /// <summary>
        ///     The border color
        /// </summary>
        private Color? borderColor;

        /// <summary>
        ///     The text
        /// </summary>
        private string text;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MiniTab" /> class.
        /// </summary>
        /// <param name="ctx">The CTX.</param>
        public MiniTab(MiniTabContext ctx)
        {
            this.ctx = ctx;
            this.TabStop = true;
        }

        /// <summary>
        ///     Occurs when [selected changed].
        /// </summary>
        public event EventHandler SelectedChanged;

        /// <summary>
        ///     Gets the default virtual size of the lightweight control.
        /// </summary>
        /// <value>The default size of the virtual.</value>
        public override Size DefaultVirtualSize
        {
            get
            {
                if (this.Parent == null)
                {
                    return Size.Empty;
                }

                var size = TextRenderer.MeasureText(this.text, this.Selected ? this.ctx.FontSelected : this.ctx.Font);
                size.Height += (int)(this.Selected ? DisplayHelper.ScaleX(7) : DisplayHelper.ScaleY(5));
                size.Width += (int)DisplayHelper.ScaleX(5) + (size.Height / 2);
                return size;
            }
        }

        /// <summary>
        ///     Gets a value indicating whether this <see cref="MiniTab" /> is selected.
        /// </summary>
        /// <value><c>true</c> if selected; otherwise, <c>false</c>.</value>
        public bool Selected { get; private set; }

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get => this.text;
            set
            {
                this.text = value;
                this.PerformLayout();
                this.Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the tool tip.
        /// </summary>
        /// <value>The tool tip.</value>
        public string ToolTip { get; set; }

        /// <summary>
        ///     Gets the color of the border.
        /// </summary>
        /// <value>The color of the border.</value>
        private Color? BorderColor
        {
            get
            {
                if (this.borderColor == null && this.LightweightControlContainerControl is MiniTabsControl)
                {
                    this.borderColor = ((MiniTabsControl)this.LightweightControlContainerControl).TopBorderColor;
                }

                return this.borderColor;
            }
        }

        /// <summary>
        ///     Selects this instance.
        /// </summary>
        public void Select()
        {
            this.Selected = true;
            this.OnSelectedChanged();
            this.Invalidate();
        }

        /// <summary>
        ///     Unselects this instance.
        /// </summary>
        internal void Unselect()
        {
            this.Selected = false;
            this.Invalidate();
        }

        /// <summary>
        ///     Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.SetToolTip(this.ToolTip);
        }

        /// <summary>
        ///     Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.SetToolTip(null);
        }

        /// <summary>
        ///     Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = new BidiGraphics(e.Graphics, this.VirtualClientRectangle);

            var tabRectangle = this.VirtualClientRectangle;

            if (this.Selected)
            {
                ColorizedResources.Instance.ViewSwitchingTabSelected.DrawBorder(e.Graphics, tabRectangle);
            }
            else
            {
                ColorizedResources.Instance.ViewSwitchingTabUnselected.DrawBorder(e.Graphics, tabRectangle);
            }

            if (ColorizedResources.UseSystemColors)
            {
                if (this.BorderColor.HasValue)
                {
                    using (var pen = new Pen(this.BorderColor.Value))
                    {
                        if (!this.Selected)
                        {
                            g.DrawLine(pen, tabRectangle.Left, tabRectangle.Top, tabRectangle.Right, tabRectangle.Top);
                        }

                        g.DrawLine(pen, tabRectangle.Left, tabRectangle.Top, tabRectangle.Left, tabRectangle.Bottom);
                        g.DrawLine(
                            pen,
                            tabRectangle.Right - 1,
                            tabRectangle.Top,
                            tabRectangle.Right - 1,
                            tabRectangle.Bottom);
                        g.DrawLine(
                            pen,
                            tabRectangle.Left,
                            tabRectangle.Bottom - 1,
                            tabRectangle.Right,
                            tabRectangle.Bottom - 1);
                    }
                }
            }

            /*
            if (!selected && !SystemInformation.HighContrast)
            {

                using (Pen p = new Pen(borderColor, 1.0f))
                    g.DrawLine(p, 0, 0, VirtualWidth, 0);
                using (Pen p = new Pen(Color.FromArgb(192, borderColor), 1.0f))
                    g.DrawLine(p, 0, 1, VirtualWidth - 1, 1);
                using (Pen p = new Pen(Color.FromArgb(128, borderColor), 1.0f))
                    g.DrawLine(p, 0, 2, VirtualWidth - 2, 2);
                using (Pen p = new Pen(Color.FromArgb(64, borderColor), 1.0f))
                    g.DrawLine(p, 0, 3, VirtualWidth - 2, 3);
            }
             * */
            var textBounds = tabRectangle;
            if (!this.Selected)
            {
                textBounds.Y += (int)DisplayHelper.ScaleX(3);
            }
            else
            {
                textBounds.Y += (int)DisplayHelper.ScaleX(3);
            }

            var textColor = ColorizedResources.Instance.MainMenuTextColor;
            if (this.Selected)
            {
                textColor = this.Parent.ForeColor;
            }

            g.DrawText(
                this.Text,
                this.ctx.Font,
                textBounds,
                SystemInformation.HighContrast ? SystemColors.ControlText : textColor,
                TextFormatFlags.Top | TextFormatFlags.HorizontalCenter
                                    | TextFormatFlags.PreserveGraphicsTranslateTransform
                                    | TextFormatFlags.PreserveGraphicsClipping);
        }

        /// <summary>
        ///     Called when [selected changed].
        /// </summary>
        protected virtual void OnSelectedChanged() => this.SelectedChanged?.Invoke(this, EventArgs.Empty);
    }
}
