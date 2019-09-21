// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Class MiniTabsControl.
    /// Implements the <see cref="OpenLiveWriter.Controls.LightweightControlContainerControl" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.LightweightControlContainerControl" />
    public class MiniTabsControl : LightweightControlContainerControl
    {
        /// <summary>
        /// Class SelectedTabChangedEventArgs.
        /// </summary>
        public class SelectedTabChangedEventArgs
        {
            /// <summary>
            /// The selected tab index
            /// </summary>
            public readonly int SelectedTabIndex;

            /// <summary>
            /// Initializes a new instance of the <see cref="SelectedTabChangedEventArgs"/> class.
            /// </summary>
            /// <param name="selectedTabIndex">Index of the selected tab.</param>
            public SelectedTabChangedEventArgs(int selectedTabIndex)
            {
                SelectedTabIndex = selectedTabIndex;
            }
        }

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
        /// The CTX
        /// </summary>
        private MiniTabContext ctx;
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
            TabStop = false;
            ctx = new MiniTabContext(this);

            InitFocusAndAccessibility();
        }

        /// <summary>
        /// Initializes the focus and accessibility.
        /// </summary>
        private void InitFocusAndAccessibility()
        {
            InitFocusManager();
        }

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
            get { return indent; }
            set
            {
                if (indent != value)
                {
                    indent = value;
                    PerformLayout();
                    Invalidate();
                }
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
                SuspendLayout();
                try
                {
                    foreach (MiniTab tab in tabs)
                    {
                        tab.SelectedChanged -= MiniTab_SelectedChanged;
                        tab.LightweightControlContainerControl = null;
                        tab.Dispose();
                    }

                    tabs = new MiniTab[value.Length];
                    for (int i = value.Length - 1; i >= 0; i--)
                    {
                        tabs[i] = new MiniTab(ctx);
                        if (i == 0)
                            tabs[i].Select();

                        tabs[i].AccessibleName = value[i];
                        tabs[i].LightweightControlContainerControl = this;
                        tabs[i].Text = value[i];
                        tabs[i].SelectedChanged += MiniTab_SelectedChanged;
                        tabs[i].MouseDown += MiniTabsControl_MouseDown;
                    }

                    ClearFocusableControls();
                    AddFocusableControls(tabs);
                }
                finally
                {
                    ResumeLayout(true);
                }
            }
        }

        /// <summary>
        /// Sets the tool tip.
        /// </summary>
        /// <param name="tabNum">The tab number.</param>
        /// <param name="toolTipText">The tool tip text.</param>
        public void SetToolTip(int tabNum, string toolTipText)
        {
            tabs[tabNum].ToolTip = toolTipText;
        }

        /// <summary>
        /// Gets or sets the color of the top border.
        /// </summary>
        /// <value>The color of the top border.</value>
        public Color TopBorderColor
        {
            get { return topBorderColor; }
            set
            {
                topBorderColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [draw shadow].
        /// </summary>
        /// <value><c>true</c> if [draw shadow]; otherwise, <c>false</c>.</value>
        public bool DrawShadow
        {
            get
            {
                return _drawShadow;
            }
            set
            {
                _drawShadow = value;
            }
        }

        /// <summary>
        /// The draw shadow
        /// </summary>
        private bool _drawShadow;

        /// <summary>
        /// Gets or sets the width of the shadow.
        /// </summary>
        /// <value>The width of the shadow.</value>
        public int ShadowWidth
        {
            get
            {
                if (_shadowWidth == -1)
                    _shadowWidth = Width;
                return _shadowWidth;
            }
            set
            {
                _shadowWidth = value;
                Update();
            }
        }

        /// <summary>
        /// The shadow width
        /// </summary>
        private int _shadowWidth = -1;

        /// <summary>
        /// Handles the MouseDown event of the MiniTabsControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void MiniTabsControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ((MiniTab)sender).Select();
        }

        /// <summary>
        /// Handles the SelectedChanged event of the MiniTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void MiniTab_SelectedChanged(object sender, EventArgs e)
        {
            if (!((MiniTab)sender).Selected)
                return;

            int selectedIndex = -1;
            for (int i = tabs.Length - 1; i >= 0; i--)
            {
                MiniTab tab = tabs[i];
                if (!ReferenceEquals(sender, tab))
                {
                    tab.Unselect();
                    tab.BringToFront();
                }
                else
                {
                    this.AccessibleName = tabs[i].AccessibleName;
                    selectedIndex = i;
                }
            }

            ((MiniTab)sender).BringToFront();

            PerformLayout();
            Invalidate();

            if (selectedIndex >= 0 && SelectedTabChanged != null)
                SelectedTabChanged(this, new SelectedTabChangedEventArgs(selectedIndex));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs" /> that contains the event data.</param>
        protected override void OnLayout(System.Windows.Forms.LayoutEventArgs e)
        {
            base.OnLayout(e);

            int height = 0;

            int x = indent;
            foreach (MiniTab tab in tabs)
            {
                tab.VirtualSize = tab.DefaultVirtualSize;
                tab.VirtualLocation = new Point(x, 0);
                x += tab.VirtualWidth - 1;
                height = Math.Max(height, tab.VirtualBounds.Bottom);
            }

            Height = height + PADDING;

            RtlLayoutFixupLightweightControls(true);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (topBorderColor != Color.Transparent)
            {
                using (Pen p = new Pen(topBorderColor))
                    e.Graphics.DrawLine(p, 0, 0, Width, 0);
            }

            if (DrawShadow)
            {
                BidiGraphics g = new BidiGraphics(e.Graphics, e.ClipRectangle);
                GraphicsHelper.TileFillScaledImageHorizontally(g, ColorizedResources.Instance.DropShadowBitmap, new Rectangle(0, 0, ShadowWidth, ColorizedResources.Instance.DropShadowBitmap.Height));
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// Selects the tab.
        /// </summary>
        /// <param name="i">The i.</param>
        public void SelectTab(int i)
        {
            tabs[i].Select();
        }
    }
}
