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
    /// Class MiniTabsControl.
    /// Implements the <see cref="OpenLiveWriter.Controls.LightweightControlContainerControl" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.LightweightControlContainerControl" />
    public partial class MiniTabsControl : LightweightControlContainerControl
    {
        /// <summary>
        /// Delegate SelectedTabChangedEventHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="SelectedTabChangedEventArgs"/> instance containing the event data.</param>
        public delegate void SelectedTabChangedEventHandler(object sender, SelectedTabChangedEventArgs args);

        /// <summary>
        /// The tabs
        /// </summary>
        private MiniTab[] tabs = new MiniTab[0];

        /// <summary>
        /// The mini tab context
        /// </summary>
        private readonly MiniTabContext ctx;

        /// <summary>
        /// The indent
        /// </summary>
        private int indent = 6;

        /// <summary>
        /// The padding
        /// </summary>
        private const int PADDING = 5;

        /// <summary>
        /// The top border color
        /// </summary>
        private Color topBorderColor = Color.Transparent;

        /// <summary>
        /// Initializes a new instance of the <see cref="MiniTabsControl"/> class.
        /// </summary>
        public MiniTabsControl()
        {
            this.TabStop = false;
            this.ctx = new MiniTabContext(this);

            this.InitFocusAndAccessibility();
        }

        /// <summary>
        /// Initializes the focus and accessibility.
        /// </summary>
        private void InitFocusAndAccessibility() => this.InitFocusManager();

        /// <summary>
        /// Occurs when [selected tab changed].
        /// </summary>
        public event SelectedTabChangedEventHandler SelectedTabChanged;

        /// <summary>
        /// Gets or sets the indent.
        /// </summary>
        /// <value>The indent.</value>
        public int Indent
        {
            get => this.indent;
            set
            {
                if (this.indent == value)
                {
                    return;
                }

                this.indent = value;
                this.PerformLayout();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Sets the tab names.
        /// </summary>
        /// <value>The tab names.</value>
        public string[] TabNames
        {
            set
            {
                this.SuspendLayout();
                try
                {
                    foreach (var tab in this.tabs)
                    {
                        tab.SelectedChanged -= this.MiniTab_SelectedChanged;
                        tab.LightweightControlContainerControl = null;
                        tab.Dispose();
                    }

                    this.tabs = new MiniTab[value.Length];
                    for (var i = value.Length - 1; i >= 0; i--)
                    {
                        this.tabs[i] = new MiniTab(this.ctx);
                        if (i == 0)
                        {
                            this.tabs[i].Select();
                        }

                        this.tabs[i].AccessibleName = value[i];
                        this.tabs[i].LightweightControlContainerControl = this;
                        this.tabs[i].Text = value[i];
                        this.tabs[i].SelectedChanged += this.MiniTab_SelectedChanged;
                        this.tabs[i].MouseDown += this.MiniTabsControl_MouseDown;
                    }

                    this.ClearFocusableControls();
                    this.AddFocusableControls(this.tabs);
                }
                finally
                {
                    this.ResumeLayout(true);
                }
            }
        }

        /// <summary>
        /// Sets the tool tip.
        /// </summary>
        /// <param name="tabNum">The tab number.</param>
        /// <param name="toolTipText">The tool tip text.</param>
        public void SetToolTip(int tabNum, string toolTipText) => this.tabs[tabNum].ToolTip = toolTipText;

        /// <summary>
        /// Gets or sets the color of the top border.
        /// </summary>
        /// <value>The color of the top border.</value>
        public Color TopBorderColor
        {
            get => this.topBorderColor;
            set
            {
                this.topBorderColor = value;
                this.Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [draw shadow].
        /// </summary>
        /// <value><c>true</c> if [draw shadow]; otherwise, <c>false</c>.</value>
        public bool DrawShadow { get; set; }

        /// <summary>
        /// Gets or sets the width of the shadow.
        /// </summary>
        /// <value>The width of the shadow.</value>
        public int ShadowWidth
        {
            get
            {
                if (this.shadowWidth == -1)
                {
                    this.shadowWidth = this.Width;
                }

                return this.shadowWidth;
            }

            set
            {
                this.shadowWidth = value;
                this.Update();
            }
        }

        /// <summary>
        /// The shadow width
        /// </summary>
        private int shadowWidth = -1;

        /// <summary>
        /// Handles the MouseDown event of the MiniTabsControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void MiniTabsControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ((MiniTab)sender).Select();
            }
        }

        /// <summary>
        /// Handles the SelectedChanged event of the MiniTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void MiniTab_SelectedChanged(object sender, EventArgs e)
        {
            if (!((MiniTab)sender).Selected)
            {
                return;
            }

            var selectedIndex = -1;
            for (var i = this.tabs.Length - 1; i >= 0; i--)
            {
                var tab = this.tabs[i];
                if (!object.ReferenceEquals(sender, tab))
                {
                    tab?.Unselect();
                    tab?.BringToFront();
                }
                else
                {
                    this.AccessibleName = this.tabs[i].AccessibleName;
                    selectedIndex = i;
                }
            }

            ((MiniTab)sender).BringToFront();

            this.PerformLayout();
            this.Invalidate();

            if (selectedIndex >= 0)
            {
                this.SelectedTabChanged?.Invoke(this, new SelectedTabChangedEventArgs(selectedIndex));
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs" /> that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            var height = 0;

            var x = this.indent;
            foreach (var tab in this.tabs)
            {
                tab.VirtualSize = tab.DefaultVirtualSize;
                tab.VirtualLocation = new Point(x, 0);
                x += tab.VirtualWidth - 1;
                height = Math.Max(height, tab.VirtualBounds.Bottom);
            }

            this.Height = height + MiniTabsControl.PADDING;

            this.RtlLayoutFixupLightweightControls(true);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (this.topBorderColor != Color.Transparent)
            {
                using (var p = new Pen(this.topBorderColor))
                {
                    e.Graphics.DrawLine(p, 0, 0, this.Width, 0);
                }
            }

            if (this.DrawShadow)
            {
                var g = new BidiGraphics(e.Graphics, e.ClipRectangle);
                GraphicsHelper.TileFillScaledImageHorizontally(g, ColorizedResources.Instance.DropShadowBitmap, new Rectangle(0, 0, this.ShadowWidth, ColorizedResources.Instance.DropShadowBitmap.Height));
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// Selects the tab.
        /// </summary>
        /// <param name="i">The i.</param>
        public void SelectTab(int i) => this.tabs[i].Select();
    }
}
