// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// Provides a tab selector lightweight control (i.e. the tab itself).
    /// </summary>
    internal class TabSelectorLightweightControl : LightweightControl
    {
        /// <summary>
        /// Pad space, in pixels.  This value is used to provide a bit of "air" around the visual
        /// elements of the tab selected lightweight control.
        /// </summary>
        private const int PAD = TabLightweightControl.PAD;

        /// <summary>
        ///	The maximum tab text width.
        /// </summary>
        private const int MAXIMUM_TAB_TEXT_WIDTH = 200;

        /// <summary>
        ///	The minimum tab interior width.
        /// </summary>
        private const int MINIMUM_TAB_INTERIOR_WIDTH = 4;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly IContainer components;

        /// <summary>
        /// Gets the tab entry.
        /// </summary>
        public TabEntry TabEntry { get; }

        /// <summary>
        /// Gets or sets a value indicating whether tabs should be small.
        /// </summary>
        public bool SmallTabs { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the tab text is shown.
        /// </summary>
        public bool ShowTabText { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the tab bitmap is shown.
        /// </summary>
        public bool ShowTabBitmap { get; set; } = true;

        /// <summary>
        /// The unselected virtual size.
        /// </summary>
        private Size unselectedVirtualSize;

        /// <summary>
        /// Gets or sets the unselected virtual size.
        /// </summary>
        public Size UnselectedVirtualSize
        {
            get => this.unselectedVirtualSize;
            set => this.unselectedVirtualSize = value;
        }

        private TextFormatFlags textFormatFlags;

        /// <summary>
        /// The bitmap rectangle.
        /// </summary>
        private Rectangle bitmapRectangle;

        /// <summary>
        /// The text layout rectangle.  This is the rectangle into which the text is measured and
        /// drawn.  It is not the actual text rectangle.
        /// </summary>
        private Rectangle textLayoutRectangle;

        /// <summary>
        /// The selected event.
        /// </summary>
        public event EventHandler Selected;

        /// <summary>
        /// Initializes a new instance of the TabSelectorLightweightControl class.
        /// </summary>
        public TabSelectorLightweightControl(IContainer container)
        {
            //	Shut up!
            if (this.components == null)
            {
                this.components = null;
            }

            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();

            //	Initialize the object.
            this.InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the TabSelectorLightweightControl class.
        /// </summary>
        public TabSelectorLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

            //	Initialize the object.
            this.InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the TabSelectorLightweightControl class.
        /// </summary>
        public TabSelectorLightweightControl(TabEntry tabEntry)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

            //	Set the tab entry.
            this.TabEntry = tabEntry;

            //	Initialize the object.
            this.InitializeObject();
        }

        /// <summary>
        /// Object initialization.
        /// </summary>
        private void InitializeObject()
        {
            //	Initialize the string format.
            this.textFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsTranslateTransform;

            //set the custom accessibility values for this control
            this.AccessibleName = this.TabEntry.TabPageControl.TabText;
            this.AccessibleRole = AccessibleRole.PageTab;

            this.TabStop = true;
        }

        private LightweightControlAccessibleObject accessibleObject;
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if (this.accessibleObject == null)
            {
                this.accessibleObject = (LightweightControlAccessibleObject)base.CreateAccessibilityInstance();
                // tabs are selectable role
                this.accessibleObject.SetAccessibleStateOverride(this.TabEntry.IsSelected ? AccessibleStates.Selectable | AccessibleStates.Selected : AccessibleStates.Selectable);
            }

            return this.accessibleObject;
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((ISupportInitialize)(this)).BeginInit();
            //
            // TabSelectorLightweightControl
            //
            this.AllowDrop = true;
            ((ISupportInitialize)(this)).EndInit();
        }
        #endregion

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">An MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnMouseDown(e);

            //	Raise the Selected event.
            if (e.Button == MouseButtons.Left)
            {
                this.OnSelected(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required if this control is not visible.
            if (this.Parent == null)// || Parent.Parent == null)
            {
                return;
            }

            //	Obtain the font we'll use to draw the tab selector lightweight control.
            var font = this.TabEntry.TabPageControl.ApplicationStyle.NormalApplicationFont;

            //	Obtain the tab bitmap and tab text.
            var tabBitmap = this.TabEntry.TabPageControl.TabBitmap;
            var tabText = this.TabEntry.TabPageControl.TabText;

            //	The tab height.
            var tabHeight = 0;

            //	Adjust the tab height for the bitmap.
            if (tabBitmap != null && tabBitmap.Height > tabHeight)
            {
                tabHeight = tabBitmap.Height;
            }

            //	Adjust the tab height for the font.
            if (font.Height > tabHeight)
            {
                tabHeight = font.Height;
            }

            //	Pad the tab height.
            tabHeight += this.SmallTabs ? PAD * 2 : PAD * 4;

            //	Set the initial tab width (padded).
            var tabWidth = PAD * 2;

            //	Reset the bitmap and text layout rectangles for the layout code below.
            this.bitmapRectangle = this.textLayoutRectangle = Rectangle.Empty;

            //	Layout the tab bitmap.
            if (this.ShowTabBitmap && tabBitmap != null)
            {
                tabWidth += PAD;
                //	Set the bitmap rectangle.
                this.bitmapRectangle = new Rectangle(tabWidth,
                                                Utility.CenterMinZero(tabBitmap.Height, tabHeight),
                                                tabBitmap.Width,
                                                tabBitmap.Height);

                //	Update the tab width.
                tabWidth += this.bitmapRectangle.Width;
                tabWidth += PAD;
            }

            //	Layout the tab text.
            if (this.ShowTabText && tabText != null && tabText.Length != 0)
            {
                //	Initialize the text layout rectangle.
                this.textLayoutRectangle = new Rectangle(tabWidth,
                                                    Utility.CenterMinZero(font.Height, tabHeight + 2),
                                                    MAXIMUM_TAB_TEXT_WIDTH,
                                                    font.Height);

                var textSize = TextRenderer.MeasureText(
                    tabText,
                    font,
                    this.textLayoutRectangle.Size,
                    this.textFormatFlags);

                this.textLayoutRectangle.Size = textSize;

                //	Update the tab width.
                tabWidth = this.textLayoutRectangle.Right + PAD;
            }

            //	If the tab is black (neither the bitmap or name will fit, or neither is present),
            //	assure that it is laid out with a minimum interior width.
            if (this.bitmapRectangle.IsEmpty && this.textLayoutRectangle.IsEmpty)
            {
                tabWidth += MINIMUM_TAB_INTERIOR_WIDTH;
            }

            //	Pad the tab width.
            tabWidth += PAD * 2;

            //	Note the unselected virtual size (used by layout logic by TabLightweightControl).
            this.unselectedVirtualSize = new Size(tabWidth, tabHeight);

            //	If the tab entry is selected, make necessary adjustments to have it occlude
            //	surrounding tabs and miter itself into the tab page border.
            if (this.TabEntry.IsSelected)
            {
                tabHeight += this.SmallTabs ? 1 : PAD * 2;

                // With localized tab text, we can't afford to waste any space
                /*
                tabWidth += PAD*4;
                if (!bitmapRectangle.IsEmpty)
                    bitmapRectangle.X += PAD*2;
                if (!textLayoutRectangle.IsEmpty)
                    textLayoutRectangle.X += PAD*2;
                */
            }

            //	Set the virtual size.
            this.VirtualSize = new Size(tabWidth, tabHeight);
        }

        #region Protected Event Overrides

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Set the ToolTip.
            if (this.TabEntry.TabPageControl.TabToolTipText != null && this.TabEntry.TabPageControl.TabToolTipText.Length != 0)
            {
                this.LightweightControlContainerControl.SetToolTip(this.TabEntry.TabPageControl.TabToolTipText);
            }
            else if (!this.ShowTabText)
            {
                this.LightweightControlContainerControl.SetToolTip(this.TabEntry.TabPageControl.TabText);
            }
        }

        /// <summary>
        /// Raises the MouseLeave event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseLeave(e);

            //	Clear the ToolTip.
            if ((this.TabEntry.TabPageControl.TabToolTipText != null && this.TabEntry.TabPageControl.TabToolTipText.Length != 0)
                || !this.ShowTabText)
            {
                this.LightweightControlContainerControl.SetToolTip(null);
            }
        }

        #endregion

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs args)
        {
            if (!this.TabEntry.Hidden)
            {
                //	Call the base class's method so that registered delegates receive the event.
                base.OnPaint(args);

                var g = new BidiGraphics(args.Graphics, this.VirtualClientRectangle);

                //	Draw the tab.
                this.DrawTab(g);

                //	Draw the tab bitmap.
                if (!this.bitmapRectangle.IsEmpty)
                {
                    g.DrawImage(false, this.TabEntry.TabPageControl.TabBitmap, this.bitmapRectangle);
                }

                //	Draw tab text.
                if (!this.textLayoutRectangle.IsEmpty)
                {
                    //	Select the text color to use.
                    var textColor = this.TabEntry.IsSelected
                        ? this.TabEntry.TabPageControl.ApplicationStyle.ActiveTabTextColor
                        : this.TabEntry.TabPageControl.ApplicationStyle.InactiveTabTextColor;

                    var tempRect = this.textLayoutRectangle;
                    //	Draw the tab text.
                    g.DrawText(this.TabEntry.TabPageControl.TabText,
                        this.TabEntry.TabPageControl.ApplicationStyle.NormalApplicationFont,
                        tempRect,
                        textColor,
                        this.textFormatFlags);
                }
            }

            if (this.Focused)
            {
                ControlPaint.DrawFocusRectangle(args.Graphics, this.VirtualClientRectangle, this.Parent.ForeColor, this.Parent.BackColor);
            }
        }

        /// <summary>
        /// Raises the Selected event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelected(EventArgs e) => Selected?.Invoke(this, e);

        /// <summary>
        /// Draws the tab.
        /// </summary>
        /// <param name="graphics">Graphics context where the tab page border is to be drawn.</param>
        private void DrawTab(BidiGraphics graphics)
        {
            //	Obtain the rectangle.
            var virtualClientRectangle = this.VirtualClientRectangle;

            //	Compute the face rectangle.
            var faceRectangle = new Rectangle(virtualClientRectangle.X + 1,
                                                    virtualClientRectangle.Y + 1,
                                                    virtualClientRectangle.Width - 2,
                                                    virtualClientRectangle.Height - (this.TabEntry.IsSelected ? 1 : 2));

            //	Fill face of the tab.
            Color topColor, bottomColor;
            if (this.TabEntry.IsSelected)
            {
                topColor = this.TabEntry.TabPageControl.ApplicationStyle.ActiveTabTopColor;
                bottomColor = this.TabEntry.TabPageControl.ApplicationStyle.ActiveTabBottomColor;
            }
            else
            {
                topColor = this.TabEntry.TabPageControl.ApplicationStyle.InactiveTabTopColor;
                bottomColor = this.TabEntry.TabPageControl.ApplicationStyle.InactiveTabBottomColor;
            }

            if (topColor == bottomColor)
            {
                using (var solidBrush = new SolidBrush(topColor))
                {
                    graphics.FillRectangle(solidBrush, faceRectangle);
                }
            }
            else
            {
                using (var linearGradientBrush = new LinearGradientBrush(this.VirtualClientRectangle, topColor, bottomColor, LinearGradientMode.Vertical))
                {
                    graphics.FillRectangle(linearGradientBrush, faceRectangle);
                }
            }

#if THREEDEE
            //	Draw the highlight inside the tab selector.
            Color highlightColor;
            if (tabEntry.IsSelected)
                highlightColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabHighlightColor;
            else
                highlightColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabHighlightColor;
            using (SolidBrush solidBrush = new SolidBrush(highlightColor))
            {
                //	Draw the top edge.
                graphics.FillRectangle(	solidBrush,
                                        faceRectangle.X,
                                        faceRectangle.Y,
                                        faceRectangle.Width-1,
                                        1);

                //	Draw the left edge.
                graphics.FillRectangle(	solidBrush,
                                        faceRectangle.X,
                                        faceRectangle.Y+1,
                                        1,
                                        faceRectangle.Height-(tabEntry.IsSelected ? 2 : 1));
            }

            //	Draw the lowlight inside the tab selector.
            Color lowlightColor;
            if (tabEntry.IsSelected)
                lowlightColor = TabEntry.TabPageControl.ApplicationStyle.ActiveTabLowlightColor;
            else
                lowlightColor = TabEntry.TabPageControl.ApplicationStyle.InactiveTabLowlightColor;
            using (SolidBrush solidBrush = new SolidBrush(lowlightColor))
            {
                //	Draw the right edge.
                graphics.FillRectangle(	solidBrush,
                                        faceRectangle.Right-1,
                                        faceRectangle.Y+1,
                                        1,
                                        faceRectangle.Height-(tabEntry.IsSelected ? 2 : 1));
            }
#endif

            //	Draw the edges of the tab selector.
            using (var solidBrush = new SolidBrush(this.TabEntry.TabPageControl.ApplicationStyle.BorderColor))
            {
                //	Draw the top edge.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.X + 2,
                    virtualClientRectangle.Y,
                    virtualClientRectangle.Width - 4,
                    1);

                //	Draw the left edge.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.X,
                    virtualClientRectangle.Y + 2,
                    1,
                    virtualClientRectangle.Height - (this.TabEntry.IsSelected ? 1 : 2));

                //	Draw the right edge.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.Right - 1,
                    virtualClientRectangle.Y + 2,
                    1,
                    virtualClientRectangle.Height - (this.TabEntry.IsSelected ? 1 : 2));

                //  Draw the corners.
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.X + 1,
                    virtualClientRectangle.Y + 1,
                    1, 1);
                graphics.FillRectangle(solidBrush,
                    virtualClientRectangle.Right - 2,
                    virtualClientRectangle.Y + 1,
                    1, 1);
            }
        }

        #region Accessibility
        public override bool DoDefaultAction()
        {
            this.OnSelected(EventArgs.Empty);
            return true;
        }
        #endregion
    }
}
