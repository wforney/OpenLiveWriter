// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.ApplicationFramework.Skinning;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    ///     Class SectionHeaderControl.
    ///     Implements the <see cref="System.Windows.Forms.Control" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Control" />
    public partial class SectionHeaderControl : Control
    {
        /// <summary>
        ///     The font
        /// </summary>
        private readonly Font font;

        /// <summary>
        ///     The header text
        /// </summary>
        private string headerText;

        /// <summary>
        ///     The UI theme
        /// </summary>
        private UITheme uiTheme;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SectionHeaderControl" /> class.
        /// </summary>
        public SectionHeaderControl()
        {
            this.TabStop = false;
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.AccessibleRole = AccessibleRole.Grouping;

            this.uiTheme = new UITheme(this);
            this.font = Res.GetFont(FontSize.Large, FontStyle.Regular);
        }

        /// <summary>
        ///     Gets or sets the header text.
        /// </summary>
        /// <value>The header text.</value>
        public string HeaderText
        {
            get => this.headerText;
            set
            {
                this.headerText = value;
                this.AccessibleName = ControlHelper.ToAccessibleName(value);
                this.Invalidate();
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.Control.Layout" /> event.
        /// </summary>
        /// <param name="levent">A <see cref="T:System.Windows.Forms.LayoutEventArgs" /> that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            using (var g = this.CreateGraphics())
            {
                this.Height = Convert.ToInt32(g.MeasureString(this.HeaderText, this.font).Height);
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = new BidiGraphics(e.Graphics, this.ClientRectangle);

            var format = new StringFormat { LineAlignment = StringAlignment.Center };
            var rectangle = this.ClientRectangle;

            // draw text
            g.DrawText(
                this.HeaderText,
                this.font,
                rectangle,
                ColorizedResources.Instance.SidebarHeaderTextColor,
                TextFormatFlags.VerticalCenter);
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            this.Invalidate();
        }
    }
}
