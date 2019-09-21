// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using Controls;

    using CoreServices;

    using Skinning;

    /// <summary>
    ///     Provides a tab pane lightweight control similar to (but cooler than) TabControl.
    /// </summary>
    public class TabLightweightControl : LightweightControl
    {
        /// <summary>
        ///     The number of pixels to scroll for each auto scroll.
        /// </summary>
        private const int AUTO_SCROLL_DELTA = 3;

        /// <summary>
        ///     WHEEL_DELTA, as specified in WinUser.h.  Somehow Microsoft forgot this in the
        ///     SystemInformation class in .NET...
        /// </summary>
        private const int WHEEL_DELTA = 120;

        /// <summary>
        ///     The minimum tab width, below which the tab selector area will not be displayed.
        /// </summary>
        private const int MINIMUM_TAB_WIDTH = 12;

        /// <summary>
        ///     Pad space, in pixels.  This value is used to provide a bit of "air" around the visual
        ///     elements of the tab lightweight control.
        /// </summary>
        internal const int PAD = 2;

        /// <summary>
        ///     The tab inset, in pixels.
        /// </summary>
        private const int TAB_INSET = 4;

        /// <summary>
        ///     The drag-and-drop tab selection delay.
        /// </summary>
        private const int DRAG_DROP_SELECTION_DELAY = 500;

        /// <summary>
        ///     The tab entry list, sorted by tab number.
        /// </summary>
        private readonly SortedList tabEntryList = new SortedList();

        /// <summary>
        ///     A value indicating whether the tab selector area will allow tab text/bitmaps to be clipped.
        /// </summary>
        private bool allowTabClipping;

        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        ///     The DateTime of the last DragInside event.
        /// </summary>
        private DateTime dragInsideTime;

        /// <summary>
        ///     A value which indicates whether side and bottom tab page borders will be drawn.
        /// </summary>
        private bool drawSideAndBottomTabPageBorders = true;

        /// <summary>
        ///     A value indicating whether the tab selector area is scrollable.
        /// </summary>
        private bool scrollableTabSelectorArea;

        /// <summary>
        ///     The selected tab entry.
        /// </summary>
        private TabEntry selectedTabEntry;

        /// <summary>
        ///     A value indicating whether tabs should be small.
        /// </summary>
        private bool smallTabs;

        /// <summary>
        ///     The tab page container control.  This control is used to contain all the TabPageControls
        ///     that are added by calls to the SetTab method.  Note that it's important that each
        ///     TabPageControl is properly sized and contained in the TabPageContainerControl.  We use
        ///     Z order to display the right TabPageControl.
        /// </summary>
        private TabPageContainerControl tabPageContainerControl;

        /// <summary>
        ///     The left tab scroller button.
        /// </summary>
        private TabScrollerButtonLightweightControl tabScrollerButtonLightweightControlLeft;

        /// <summary>
        ///     The right tab scroller button.
        /// </summary>
        private TabScrollerButtonLightweightControl tabScrollerButtonLightweightControlRight;

        /// <summary>
        ///     The tab scroller position.
        /// </summary>
        private int tabScrollerPosition;

        /// <summary>
        ///     The tab selector area size.
        /// </summary>
        private Size tabSelectorAreaSize;

        /// <summary>
        ///     Initializes a new instance of the TabLightweightControl class.
        /// </summary>
        public TabLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the TabLightweightControl class.
        /// </summary>
        public TabLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
        }

        public bool ColorizeBorder { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether tabs should be small.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Specifies whether whether side and bottom tab page borders will be drawn.")]
        public bool SmallTabs
        {
            get => this.smallTabs;
            set
            {
                if (this.smallTabs != value)
                {
                    this.smallTabs = value;

                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets or sets a value which indicates whether side and bottom tab page borders will be drawn.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Specifies whether whether side and bottom tab page borders will be drawn.")]
        public bool DrawSideAndBottomTabPageBorders
        {
            get => this.drawSideAndBottomTabPageBorders;
            set
            {
                if (this.drawSideAndBottomTabPageBorders != value)
                {
                    this.drawSideAndBottomTabPageBorders = value;
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        public override Size MinimumVirtualSize =>
            new Size(0, this.tabPageContainerControl.Location.Y + this.SelectedTab.MinimumSize.Height);

        /// <summary>
        ///     Gets or sets a value indicating whether the tab selector area is scrollable.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Specifies whether whether whether the tab selecter area is scrollable.")]
        public bool ScrollableTabSelectorArea
        {
            get => this.scrollableTabSelectorArea;
            set
            {
                Debug.Assert(!value, "'Scrollable tab selectors may not work correctly with bidi'");

                if (this.scrollableTabSelectorArea != value)
                {
                    this.scrollableTabSelectorArea = value;
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets or sets whether the tab selector area will allow tab text/bitmaps to be clipped.
        ///     If false, text or bitmaps will be dropped to shrink the tab size.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Specifies whether whether whether the tab selecter area is scrollable.")]
        public bool AllowTabClipping
        {
            get => this.allowTabClipping;
            set
            {
                if (this.allowTabClipping != value)
                {
                    this.allowTabClipping = value;
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        ///     Gets the tab count.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int TabCount => this.tabEntryList.Count;

        /// <summary>
        ///     Gets or sets the index of the currently-selected tab page.
        /// </summary>
        public int SelectedTabNumber
        {
            get => this.tabEntryList.IndexOfValue(this.SelectedTabEntry);
            set
            {
                var tabEntry = (TabEntry) this.tabEntryList[value];
                if (tabEntry != null)
                {
                    this.SelectedTabEntry = tabEntry;
                }
            }
        }

        /// <summary>
        ///     Gets the selected TabPageControl that was used to create the tab.
        /// </summary>
        public TabPageControl SelectedTab => this.SelectedTabEntry.TabPageControl;

        /// <summary>
        ///     Gets the selected tab entry.
        /// </summary>
        internal TabEntry SelectedTabEntry
        {
            get
            {
                //	Select the first tab entry, if there isn't a selected tab entry.
                if (this.selectedTabEntry == null)
                {
                    this.SelectedTabEntry = this.FirstTabEntry;
                }

                return this.selectedTabEntry;
            }
            set
            {
                if (this.selectedTabEntry != value)
                {
                    this.selectedTabEntry?.Unselected();

                    //	Select the new tab entry.
                    this.selectedTabEntry = value;

                    this.selectedTabEntry?.Selected();

                    this.PerformLayout();
                    this.Invalidate();

                    //	Raise the SelectedTabNumberChanged event.
                    this.OnSelectedTabNumberChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///     Gets the first tab entry.
        /// </summary>
        internal TabEntry FirstTabEntry =>
            this.tabEntryList.Count == 0 ? null : (TabEntry) this.tabEntryList.GetByIndex(0);

        /// <summary>
        ///     Gets or sets the tab scroller position.
        /// </summary>
        private int TabScrollerPosition
        {
            get => this.tabScrollerPosition;
            set => this.tabScrollerPosition = MathHelper.Clip(value, 0, this.MaximumTabScrollerPosition);
        }

        /// <summary>
        ///     Gets the tab area rectangle.  This is the entire area available for tab and tab
        ///     scroller button layout.
        /// </summary>
        private Rectangle TabAreaRectangle =>
            new Rectangle(0,
                          0, this.VirtualWidth, this.tabSelectorAreaSize.Height);

        /// <summary>
        ///     Gets the tab page border rectangle.
        /// </summary>
        private Rectangle TabPageBorderRectangle => new Rectangle(0, this.tabSelectorAreaSize.Height - 1,
                                                                  this.VirtualWidth,
                                                                  this.VirtualHeight -
                                                                  (this.tabSelectorAreaSize.Height - 1));

        /// <summary>
        ///     Gets the tab page rectangle.
        /// </summary>
        private Rectangle TabPageRectangle
        {
            get
            {
                var border = this.drawSideAndBottomTabPageBorders ? 1 : 0;
                return new Rectangle(border, this.tabSelectorAreaSize.Height + border, this.VirtualWidth - border * 2,
                                     this.VirtualHeight - (this.tabSelectorAreaSize.Height + border * 2));
            }
        }

        /// <summary>
        ///     Gets the maximum tab scroller position.
        /// </summary>
        private int MaximumTabScrollerPosition =>
            Math.Max(0, this.tabSelectorAreaSize.Width - this.TabAreaRectangle.Width);

        /// <summary>
        ///     Gets the scroll delta, or how many pixels to scroll the tab area.
        /// </summary>
        private int ScrollDelta => this.TabAreaRectangle.Width / 8;

        /// <summary>
        ///     Occurs when the SelectedTabNumber property changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the SelectedTabNumber property changes.")]
        public event EventHandler SelectedTabNumberChanged;

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
            this.tabScrollerButtonLightweightControlLeft =
                new TabScrollerButtonLightweightControl(this.components);
            this.tabScrollerButtonLightweightControlRight =
                new TabScrollerButtonLightweightControl(this.components);
            this.tabPageContainerControl = new TabPageContainerControl();
            ((ISupportInitialize) this.tabScrollerButtonLightweightControlLeft).BeginInit();
            ((ISupportInitialize) this.tabScrollerButtonLightweightControlRight).BeginInit();
            ((ISupportInitialize) this).BeginInit();

            //
            // tabScrollerButtonLightweightControlLeft
            //
            this.tabScrollerButtonLightweightControlLeft.LightweightControlContainerControl = this;
            this.tabScrollerButtonLightweightControlLeft.Scroll +=
                this.tabScrollerButtonLightweightControlLeft_Scroll;
            this.tabScrollerButtonLightweightControlLeft.AutoScroll +=
                this.tabScrollerButtonLightweightControlLeft_AutoScroll;

            //
            // tabScrollerButtonLightweightControlRight
            //
            this.tabScrollerButtonLightweightControlRight.LightweightControlContainerControl = this;
            this.tabScrollerButtonLightweightControlRight.Scroll +=
                this.tabScrollerButtonLightweightControlRight_Scroll;
            this.tabScrollerButtonLightweightControlRight.AutoScroll +=
                this.tabScrollerButtonLightweightControlRight_AutoScroll;

            //
            // tabPageContainerControl
            //
            this.tabPageContainerControl.Location = new Point(524, 17);
            this.tabPageContainerControl.Name = "tabPageContainerControl";
            this.tabPageContainerControl.TabIndex = 0;

            //
            // TabLightweightControl
            //
            this.AllowMouseWheel = true;
            ((ISupportInitialize) this.tabScrollerButtonLightweightControlLeft).EndInit();
            ((ISupportInitialize) this.tabScrollerButtonLightweightControlRight).EndInit();
            ((ISupportInitialize) this).EndInit();
        }

        /// <summary>
        ///     Raises the SelectedTabNumberChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelectedTabNumberChanged(EventArgs e) => this.SelectedTabNumberChanged?.Invoke(this, e);

        protected override void AddAccessibleControlsToList(ArrayList list)
        {
            for (var i = 0; i < this.TabCount; i++)
            {
                var tabEntry = (TabEntry) this.tabEntryList[i];
                list.Add(tabEntry.TabSelectorLightweightControl);
            }

            base.AddAccessibleControlsToList(list);
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        public TabPageControl GetTab(int i) => ((TabEntry) this.tabEntryList[i]).TabPageControl;

        private void IncrementTab()
        {
            var currentIndex = this.SelectedTabNumber + 1;
            if (currentIndex >= this.tabEntryList.Count)
            {
                currentIndex = 0;
            }

            var tabEntry = (TabEntry) this.tabEntryList[currentIndex];
            if (tabEntry != null)
            {
                this.SelectedTabEntry = tabEntry;
            }
        }

        private void DecrementTab()
        {
            var currentIndex = this.SelectedTabNumber - 1;
            if (currentIndex < 0)
            {
                currentIndex = this.tabEntryList.Count - 1;
            }

            var tabEntry = (TabEntry) this.tabEntryList[currentIndex];
            if (tabEntry != null)
            {
                this.SelectedTabEntry = tabEntry;
            }
        }

        public bool CheckForTabSwitch(Keys keyData)
        {
            if ((Control.ModifierKeys & Keys.Control) != 0)
            {
                var keys1 = keyData & Keys.KeyCode;
                if (keys1 == Keys.Tab)
                {
                    if ((Control.ModifierKeys & Keys.Shift) != 0)
                    {
                        this.DecrementTab();
                    }
                    else
                    {
                        this.IncrementTab();
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Sets a tab control.
        /// </summary>
        /// <param name="tabNumber">The tab index to set.</param>
        /// <param name="tabPageControl">The tab page control to set.</param>
        public void SetTab(int tabNumber, TabPageControl tabPageControl)
        {
            //	If there already is a tab entry for the specified tab number, remove it.
            this.RemoveTabEntry(tabNumber);

            //	Instantiate the new tab entry.
            var tabEntry = new TabEntry(this, tabPageControl);
            this.tabEntryList.Add(tabNumber, tabEntry);
            this.tabPageContainerControl.Controls.Add(tabEntry.TabPageControl);
            this.LightweightControls.Add(tabEntry.TabSelectorLightweightControl);
            tabEntry.TabSelectorLightweightControl.DragInside += this.TabSelectorLightweightControl_DragInside;
            tabEntry.TabSelectorLightweightControl.DragOver += this.TabSelectorLightweightControl_DragOver;
            tabEntry.TabSelectorLightweightControl.Selected += this.TabSelectorLightweightControl_Selected;
            tabEntry.TabPageControl.VisibleChanged += this.TabPageControl_VisibleChanged;

            //	Layout and invalidate.
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        ///     Removes a tab control.
        /// </summary>
        /// <param name="tabNumber">The tab number to remove.</param>
        public void RemoveTab(int tabNumber)
        {
            //	Remove it.
            this.RemoveTabEntry(tabNumber);

            //	Layout and invalidate.
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        ///     Determines whether the specified tab number has been added.
        /// </summary>
        /// <param name="tabNumber">The tab number to look for.</param>
        /// <returns>True if a tab has been added with the specified tab number; false otherwise.</returns>
        public bool HasTab(int tabNumber) => this.tabEntryList.Contains(tabNumber);

        /// <summary>
        ///     Raises the Layout event.  The mother of all layout processing.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required if this control is not visible.
            if (this.Parent == null)
            {
                return;
            }

            //	If there are not tabs, layout processing is simple.
            if (this.tabEntryList.Count == 0)
            {
                this.HideTabScrollerInterface();
                this.tabSelectorAreaSize = new Size(0, 1);
                return;
            }

            //	Pre-process the layout of tab selectors.
            int width = TabLightweightControl.TAB_INSET * 2 + TabLightweightControl.PAD * 4 +
                        TabLightweightControl.PAD * (this.tabEntryList.Count - 1),
                height = 0;
            foreach (TabEntry tabEntry in this.tabEntryList.Values)
            {
                if (!tabEntry.Hidden)
                {
                    //	Reset the tab selector and have it perform its layout logic so that it is at
                    //	its natural size.
                    tabEntry.TabSelectorLightweightControl.SmallTabs = this.SmallTabs;
                    tabEntry.TabSelectorLightweightControl.ShowTabBitmap = true;
                    tabEntry.TabSelectorLightweightControl.ShowTabText = true;
                    tabEntry.TabSelectorLightweightControl.PerformLayout();

                    //	If this tab selector is the tallest one we've seen, it sets a new high-
                    //	water mark.
                    if (tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Height > height)
                    {
                        height = tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Height;
                    }

                    //	Adjust the width to account for this tab selector.
                    width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;

                    // Push the tabs closer together
                    width -= 6;
                }
            }

            width += 6;

            //	Pad height.
            height += TabLightweightControl.PAD;

            //	If the tab selection area is scrollable, enable/disable the scroller interface.
            if (this.scrollableTabSelectorArea)
            {
                //	Set the tab selector area size.
                this.tabSelectorAreaSize = new Size(width, height);

                //	If the entire tab selector area is visible, hide the tab scroller interface.
                if (width <= this.VirtualWidth)
                {
                    this.HideTabScrollerInterface();
                }
                else
                {
                    //	If the tab scroller position is zero, the left tab scroller button is not
                    //	shown.
                    if (this.tabScrollerPosition == 0)
                    {
                        this.tabScrollerButtonLightweightControlLeft.Visible = false;
                    }
                    else
                    {
                        //	Layout the left tab scroller button.
                        this.tabScrollerButtonLightweightControlLeft.VirtualBounds = new Rectangle(0,
                                                                                                   0,
                                                                                                   this
                                                                                                      .tabScrollerButtonLightweightControlLeft
                                                                                                      .DefaultVirtualSize
                                                                                                      .Width,
                                                                                                   this
                                                                                                      .tabSelectorAreaSize
                                                                                                      .Height);
                        this.tabScrollerButtonLightweightControlLeft.Visible = true;
                        this.tabScrollerButtonLightweightControlLeft.BringToFront();
                    }

                    //	Layout the right tab scroller button.
                    if (this.tabScrollerPosition == this.MaximumTabScrollerPosition)
                    {
                        this.tabScrollerButtonLightweightControlRight.Visible = false;
                    }
                    else
                    {
                        var rightScrollerButtonWidth =
                            this.tabScrollerButtonLightweightControlRight.DefaultVirtualSize.Width;
                        this.tabScrollerButtonLightweightControlRight.VirtualBounds = new Rectangle(
                            this.TabAreaRectangle.Right - rightScrollerButtonWidth,
                            0,
                            rightScrollerButtonWidth, this.tabSelectorAreaSize.Height);
                        this.tabScrollerButtonLightweightControlRight.Visible = true;
                        this.tabScrollerButtonLightweightControlRight.BringToFront();
                    }
                }
            }
            else
            {
                //	Hide the tab scroller interface.
                this.HideTabScrollerInterface();

                if (!this.allowTabClipping)
                {
                    //	If the entire tab selector area is not visible, switch into "no bitmap" mode.
                    if (width > this.VirtualWidth)
                    {
                        //	Reset the width.
                        width = TabLightweightControl.TAB_INSET * 2 + TabLightweightControl.PAD * 4 +
                                TabLightweightControl.PAD * (this.tabEntryList.Count - 1);

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in this.tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab bitmap.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = false;
                                tabEntry.TabSelectorLightweightControl.ShowTabText = true;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }
                    }

                    //	If the entire tab selector area is not visible, switch into "no text except selected" mode.
                    if (width > this.VirtualWidth)
                    {
                        //	Reset the width.
                        width = TabLightweightControl.TAB_INSET * 2 + TabLightweightControl.PAD * 4 +
                                TabLightweightControl.PAD * (this.tabEntryList.Count - 1);

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in this.tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab text.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = true;
                                tabEntry.TabSelectorLightweightControl.ShowTabText = tabEntry.IsSelected;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }
                    }

                    //	If the entire tab selector area is not visible, switch into "no text" mode.
                    if (width > this.VirtualWidth)
                    {
                        //	Reset the width.
                        width = TabLightweightControl.TAB_INSET * 2 + TabLightweightControl.PAD * 4 +
                                TabLightweightControl.PAD * (this.tabEntryList.Count - 1);

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in this.tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab text.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = true;
                                tabEntry.TabSelectorLightweightControl.ShowTabText = false;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }
                    }

                    //	If the entire tab selector area is not visible, hide it.
                    if (width > this.VirtualWidth)
                    {
                        //	Reset the width.
                        width = TabLightweightControl.TAB_INSET * 2 + TabLightweightControl.PAD * 4 +
                                TabLightweightControl.PAD * (this.tabEntryList.Count - 1);

                        //	Re-process the layout of tab selectors.
                        foreach (TabEntry tabEntry in this.tabEntryList.Values)
                        {
                            if (!tabEntry.Hidden)
                            {
                                //	Hide tab text.
                                tabEntry.TabSelectorLightweightControl.ShowTabBitmap = false;
                                tabEntry.TabSelectorLightweightControl.PerformLayout();

                                //	Adjust the width to account for this tab selector.
                                width += tabEntry.TabSelectorLightweightControl.UnselectedVirtualSize.Width;
                            }
                        }

                        if (width > this.VirtualWidth)
                        {
                            height = 1;
                        }
                    }
                }

                //	Set the tab selector layout size.
                this.tabSelectorAreaSize = new Size(width, height);
            }

            //	Finally, actually layout the tab entries.
            var x = TabLightweightControl.TAB_INSET + TabLightweightControl.PAD * 2;
            TabEntry previousTabEntry = null;
            foreach (TabEntry tabEntry in this.tabEntryList.Values)
            {
                if (!tabEntry.Hidden)
                {
                    //	Adjust the x offset for proper positioning of this tab and set the y offset,
                    //	too.  Now we know WHERE the tab will be laid out in the tab area.
                    if (previousTabEntry != null)
                    {
                        x += previousTabEntry.IsSelected ? -TabLightweightControl.PAD + 1 : -1;
                    }

                    var y = Math.Max(
                        0,
                        this.tabSelectorAreaSize.Height - tabEntry.TabSelectorLightweightControl.VirtualBounds.Height);

                    //	Layout the tab entry.
                    tabEntry.TabSelectorLightweightControl.VirtualLocation = new Point(x - this.tabScrollerPosition, y);

                    //	Adjust the x offset to account for the tab entry.
                    x += tabEntry.TabSelectorLightweightControl.VirtualBounds.Width;

                    //	Set the previous tab entry for the next loop iteration.
                    previousTabEntry = tabEntry;
                }
            }

            //	Set the bounds of the tab page control.
            var tabPageControlBounds = this.VirtualClientRectangleToParent(this.TabPageRectangle);
            if (this.tabPageContainerControl.Bounds != tabPageControlBounds)
            {
                this.tabPageContainerControl.Bounds = tabPageControlBounds;
            }

            //	Make sure the selected tab entry and its TabPageControl are visible and at the top
            //	of the Z order and that, if there is one, the previously selected tab entry's
            //	TabPageControl is not visible.
            if (this.SelectedTabEntry != null)
            {
                this.SelectedTabEntry.TabSelectorLightweightControl.BringToFront();
                this.SelectedTabEntry.TabPageControl.BringToFront();
            }

            this.RtlLayoutFixup(true);
        }

        /// <summary>
        ///     Raises the LightweightControlContainerControlChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLightweightControlContainerControlChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLightweightControlContainerControlChanged(e);

            //	Set the tab page control's parent.
            if (this.tabPageContainerControl.Parent != this.Parent)
            {
                this.tabPageContainerControl.Parent = this.Parent;
            }

            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        ///     Raises the LightweightControlContainerControlVirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLightweightControlContainerControlVirtualLocationChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLightweightControlContainerControlVirtualLocationChanged(e);

            //	Layout and invalidate.
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        ///     Raises the MouseWheel event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseWheel(e);

            //	Scroll the tab layout area.
            this.ScrollTabLayoutArea(-(e.Delta / TabLightweightControl.WHEEL_DELTA) * this.ScrollDelta);
        }

        /// <summary>
        ///     Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Draw the tab page border.
            this.DrawTabPageBorders(e.Graphics);

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }

        /// <summary>
        ///     Raises the VirtualLocationChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnVirtualLocationChanged(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnVirtualLocationChanged(e);

            //	Layout and invalidate.
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        ///     Helper to remove a tab entry.
        /// </summary>
        /// <param name="tabNumber">The tab number of the tab entry to remove.</param>
        private void RemoveTabEntry(int tabNumber)
        {
            //	Locate the tab entry for the specified tab number.  It it's found, remove it.
            var tabEntry = (TabEntry) this.tabEntryList[tabNumber];
            if (tabEntry != null)
            {
                if (this.SelectedTabEntry == tabEntry)
                {
                    this.SelectedTabEntry = this.FirstTabEntry;
                }

                this.tabPageContainerControl.Controls.Remove(tabEntry.TabPageControl);
                this.LightweightControls.Remove(tabEntry.TabSelectorLightweightControl);
                this.tabEntryList.Remove(tabNumber);
            }
        }

        /// <summary>
        ///     Hides the tab scroller interface.
        /// </summary>
        private void HideTabScrollerInterface()
        {
            this.tabScrollerPosition = 0;
            this.tabScrollerButtonLightweightControlLeft.Visible = false;
            this.tabScrollerButtonLightweightControlRight.Visible = false;
        }

        /// <summary>
        ///     Draws the tab page border.
        /// </summary>
        /// <param name="graphics">Graphics context where the tab page border is to be drawn.</param>
        private void DrawTabPageBorders(Graphics graphics)
        {
            var c = SystemColors.ControlDark;
            if (this.ColorizeBorder)
            {
                c = ColorizedResources.Instance.BorderLightColor;
            }

            //	Draw tab page borders.
            using (var borderBrush = new SolidBrush(c))
            {
                //	Obtain the tab page border rectangle.
                var tabPageBorderRectangle = this.TabPageBorderRectangle;

                //	Draw the top edge.
                graphics.FillRectangle(borderBrush,
                                       tabPageBorderRectangle.X,
                                       tabPageBorderRectangle.Y,
                                       tabPageBorderRectangle.Width,
                                       1);

                //	Draw the highlight under the top edge.
                using (var highlightBrush = new SolidBrush(Color.FromArgb(206, 219, 248)))
                {
                    graphics.FillRectangle(highlightBrush,
                                           tabPageBorderRectangle.X,
                                           tabPageBorderRectangle.Y + 1,
                                           tabPageBorderRectangle.Width,
                                           1);
                }

                //	Draw tab page borders, if we should.
                if (this.drawSideAndBottomTabPageBorders)
                {
                    //	Draw the left edge.
                    graphics.FillRectangle(borderBrush,
                                           tabPageBorderRectangle.X,
                                           tabPageBorderRectangle.Y + 1,
                                           1,
                                           tabPageBorderRectangle.Height - 1);

                    //	Draw the right edge.
                    graphics.FillRectangle(borderBrush,
                                           tabPageBorderRectangle.Right - 1,
                                           tabPageBorderRectangle.Y + 1,
                                           1,
                                           tabPageBorderRectangle.Height - 1);

                    //	Draw the bottom edge.
                    graphics.FillRectangle(borderBrush,
                                           tabPageBorderRectangle.X + 1,
                                           tabPageBorderRectangle.Bottom - 1,
                                           tabPageBorderRectangle.Width - 2,
                                           1);
                }
            }
        }

        /// <summary>
        ///     TabSelectorLightweightControl_DragInside event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TabSelectorLightweightControl_DragInside(object sender, DragEventArgs e) =>
            //	Note the DateTime of the last DragInside event.
            this.dragInsideTime = DateTime.Now;

        /// <summary>
        ///     TabSelectorLightweightControl_DragInside event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TabSelectorLightweightControl_DragOver(object sender, DragEventArgs e)
        {
            //	Wait an amount of time before selecting the tab page.
            if (DateTime.Now.Subtract(this.dragInsideTime).Milliseconds <
                TabLightweightControl.DRAG_DROP_SELECTION_DELAY)
            {
                return;
            }

            //	Ensure that the sender is who we think it is.
            Debug.Assert(sender is TabSelectorLightweightControl, "Doh!",
                         "Bad event wiring is the leading cause of code decay.");
            //	Set the selected tab entry, if we should.
            if (sender is TabSelectorLightweightControl tabSelectorLightweightControl)
            {
                if (tabSelectorLightweightControl.TabEntry.TabPageControl.DragDropSelectable &&
                    this.SelectedTabEntry != tabSelectorLightweightControl.TabEntry)
                {
                    this.SelectedTabEntry = tabSelectorLightweightControl.TabEntry;
                }
            }
        }

        /// <summary>
        ///     TabSelectorLightweightControl_Selected event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void TabSelectorLightweightControl_Selected(object sender, EventArgs e)
        {
            //	Ensure that the sender is who we think it is.
            Debug.Assert(sender is TabSelectorLightweightControl, "Doh!",
                         "Bad event wiring is the leading cause of code decay.");
            if (sender is TabSelectorLightweightControl)
            {
                //	Set the selected tab entry.
                var tabSelectorLightweightControl = (TabSelectorLightweightControl) sender;
                this.SelectedTabEntry = tabSelectorLightweightControl.TabEntry;
            }
        }

        private void TabPageControl_VisibleChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        ///     tabScrollerButtonLightweightControlLeft_Scroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlLeft_Scroll(object sender, EventArgs e) => this.ScrollTabLayoutArea(-this.ScrollDelta);

        /// <summary>
        ///     tabScrollerButtonLightweightControlRight_Scroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlRight_Scroll(object sender, EventArgs e) => this.ScrollTabLayoutArea(this.ScrollDelta);

        /// <summary>
        ///     tabScrollerButtonLightweightControlLeft_AutoScroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlLeft_AutoScroll(object sender, EventArgs e) => this.ScrollTabLayoutArea(-TabLightweightControl.AUTO_SCROLL_DELTA);

        /// <summary>
        ///     tabScrollerButtonLightweightControlRight_AutoScroll event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void tabScrollerButtonLightweightControlRight_AutoScroll(object sender, EventArgs e) => this.ScrollTabLayoutArea(TabLightweightControl.AUTO_SCROLL_DELTA);

        /// <summary>
        ///     Scrolls the tab layout area.
        /// </summary>
        /// <param name="delta">
        ///     Scroll delta.  This value does not have to take scroll limits into
        ///     account as the TabScrollerPosition property handles this.
        /// </param>
        private void ScrollTabLayoutArea(int delta)
        {
            //	If we have a scrollable tab layout area, scroll it.
            if (this.ScrollableTabSelectorArea)
            {
                //	Adjust the tab scroller position.
                this.TabScrollerPosition += delta;

                //	Get the screen up to date.
                this.PerformLayout();
                this.Invalidate();
                this.Update();
            }
        }
    }
}