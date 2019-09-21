// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;

    using OpenLiveWriter.Controls;

    /// <summary>
    /// Provides the workspace column control.
    /// </summary>
    public class WorkspaceColumnLightweightControl : LightweightControl
    {
        /// <summary>
        /// The minimum horizontal splitter position.
        /// </summary>
        private static readonly double MINIMUM_HORIZONTAL_SPLITTER_POSITION = 0.20;

        /// <summary>
        /// The default maximum horizontal splitter position.
        /// </summary>
        private static readonly double MAXIMUM_HORIZONTAL_SPLITTER_POSITION = 0.80;

        /// <summary>
        /// Required designer cruft.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// The vertical splitter lightweight control.
        /// </summary>
        private SplitterLightweightControl splitterLightweightControlVertical;

        /// <summary>
        /// The horizontal splitter lightweight control.
        /// </summary>
        private SplitterLightweightControl splitterLightweightControlHorizontal;

        /// <summary>
        /// The vertical splitter style.
        /// </summary>
        private VerticalSplitterStyle verticalSplitterStyle = VerticalSplitterStyle.None;

        /// <summary>
        /// The vertical splitter width.
        /// </summary>
        private int verticalSplitterWidth = 5;

        /// <summary>
        /// The horizontal splitter height.
        /// </summary>
        private int horizontalSplitterHeight = 5;

        /// <summary>
        /// The maximum horizontal splitter position.
        /// </summary>
        private double maximumHorizontalSplitterPosition = MAXIMUM_HORIZONTAL_SPLITTER_POSITION;

        /// <summary>
        /// The starting preferred column width.
        /// </summary>
        private double startingPreferredColumnWidth;

        /// <summary>
        /// The preferred column width.
        /// </summary>
        private int preferredColumnWidth = 0;

        /// <summary>
        /// The minimum column width.
        /// </summary>
        private int minimumColumnWidth = 0;

        /// <summary>
        /// The maximum column width.
        /// </summary>
        private int maximumColumnWidth = 0;

        /// <summary>
        /// The starting horizontal splitter position.
        /// </summary>
        private double startingHorizontalSplitterPosition;

        /// <summary>
        /// The horizontal splitter position.
        /// </summary>
        private double horizontalSplitterPosition = 0.50;

        /// <summary>
        /// Occurs when the PreferredColumnWidth changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the PreferredColumnWidth property is changed.")
        ]
        public event EventHandler PreferredColumnWidthChanged;

        /// <summary>
        /// Occurs when the MinimumColumnWidth changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the MinimumColumnWidth property is changed.")
        ]
        public event EventHandler MinimumColumnWidthChanged;

        /// <summary>
        /// Occurs when the MaximumColumnWidth changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the MaximumColumnWidth property is changed.")
        ]
        public event EventHandler MaximumColumnWidthChanged;

        /// <summary>
        /// Occurs when the HorizontalSplitterPosition changes.
        /// </summary>
        [
            Category("Property Changed"),
                Description("Occurs when the HorizontalSplitterPosition property is changed.")
        ]
        public event EventHandler HorizontalSplitterPositionChanged;

        /// <summary>
        /// Occurs when the horizontal splitter begins moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the horizontal splitter begins moving.")
        ]
        public event EventHandler HorizontalSplitterBeginMove;

        /// <summary>
        /// Occurs when the horizontal splitter is ends moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the horizontal splitter ends moving.")
        ]
        public event EventHandler HorizontalSplitterEndMove;

        /// <summary>
        /// Occurs when the horizontal splitter is moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the horizontal splitter is moving.")
        ]
        public event EventHandler HorizontalSplitterMoving;

        /// <summary>
        /// Occurs when the vertical splitter begins moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the vertical splitter begins moving.")
        ]
        public event EventHandler VerticalSplitterBeginMove;

        /// <summary>
        /// Occurs when the vertical splitter is ends moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the vertical splitter ends moving.")
        ]
        public event EventHandler VerticalSplitterEndMove;

        /// <summary>
        /// Occurs when the vertical splitter is moving.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the vertical splitter is moving.")
        ]
        public event EventHandler VerticalSplitterMoving;

        /// <summary>
        /// Initializes a new instance of the WorkspaceColumnLightweightControl class.
        /// </summary>
        public WorkspaceColumnLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the WorkspaceColumnLightweightControl class.
        /// </summary>
        public WorkspaceColumnLightweightControl() =>
            // This call is required by the Windows Form Designer.
            this.InitializeComponent();

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
            this.components = new Container();
            this.splitterLightweightControlVertical = new SplitterLightweightControl(this.components);
            this.splitterLightweightControlHorizontal = new SplitterLightweightControl(this.components);
            this.UpperPane = new WorkspaceColumnPaneLightweightControl(this.components);
            this.LowerPane = new WorkspaceColumnPaneLightweightControl(this.components);
            ((ISupportInitialize)this.splitterLightweightControlVertical).BeginInit();
            ((ISupportInitialize)this.splitterLightweightControlHorizontal).BeginInit();
            ((ISupportInitialize)this.UpperPane).BeginInit();
            ((ISupportInitialize)this.LowerPane).BeginInit();
            ((ISupportInitialize)this).BeginInit();
            //
            // splitterLightweightControlVertical
            //
            this.splitterLightweightControlVertical.LightweightControlContainerControl = this;
            this.splitterLightweightControlVertical.Orientation = SplitterLightweightControl.SplitterOrientation.Vertical;
            this.splitterLightweightControlVertical.SplitterEndMove += new LightweightSplitterEventHandler(this.splitterLightweightControlVertical_SplitterEndMove);
            this.splitterLightweightControlVertical.SplitterBeginMove += new EventHandler(this.splitterLightweightControlVertical_SplitterBeginMove);
            this.splitterLightweightControlVertical.SplitterMoving += new LightweightSplitterEventHandler(this.splitterLightweightControlVertical_SplitterMoving);
            //
            // splitterLightweightControlHorizontal
            //
            this.splitterLightweightControlHorizontal.LightweightControlContainerControl = this;
            this.splitterLightweightControlHorizontal.SplitterEndMove += new LightweightSplitterEventHandler(this.splitterLightweightControlHorizontal_SplitterEndMove);
            this.splitterLightweightControlHorizontal.SplitterBeginMove += new EventHandler(this.splitterLightweightControlHorizontal_SplitterBeginMove);
            this.splitterLightweightControlHorizontal.SplitterMoving += new LightweightSplitterEventHandler(this.splitterLightweightControlHorizontal_SplitterMoving);
            //
            // workspaceColumnPaneUpper
            //
            this.UpperPane.Border = false;
            this.UpperPane.Control = null;
            this.UpperPane.FixedHeight = 0;
            this.UpperPane.FixedHeightLayout = false;
            this.UpperPane.LightweightControl = null;
            this.UpperPane.LightweightControlContainerControl = this;
            this.UpperPane.Visible = false;
            this.UpperPane.VisibleChanged += new EventHandler(this.workspaceColumnPaneUpper_VisibleChanged);
            //
            // workspaceColumnPaneLower
            //
            this.LowerPane.Border = false;
            this.LowerPane.Control = null;
            this.LowerPane.FixedHeight = 0;
            this.LowerPane.FixedHeightLayout = false;
            this.LowerPane.LightweightControl = null;
            this.LowerPane.LightweightControlContainerControl = this;
            this.LowerPane.Visible = false;
            this.LowerPane.VisibleChanged += new EventHandler(this.workspaceColumnPaneLower_VisibleChanged);
            //
            // WorkspaceColumnLightweightControl
            //
            this.AllowDrop = true;
            ((ISupportInitialize)this.splitterLightweightControlVertical).EndInit();
            ((ISupportInitialize)this.splitterLightweightControlHorizontal).EndInit();
            ((ISupportInitialize)this.UpperPane).EndInit();
            ((ISupportInitialize)this.LowerPane).EndInit();
            ((ISupportInitialize)this).EndInit();

        }
        #endregion

        /// <summary>
        /// Gets or sets the upper pane WorkspaceColumnPaneLightweightControl.
        /// </summary>
        public WorkspaceColumnPaneLightweightControl UpperPane { get; private set; }

        /// <summary>
        /// Gets or sets the lower pane WorkspaceColumnPaneLightweightControl.
        /// </summary>
        public WorkspaceColumnPaneLightweightControl LowerPane { get; private set; }

        /// <summary>
        /// Gets or sets the control that is attached to the splitter control.
        /// </summary>
        public LightweightControl HorizontalSplitterAttachedControl
        {
            get => this.splitterLightweightControlHorizontal.AttachedControl;

            set => this.splitterLightweightControlHorizontal.AttachedControl = value;
        }

        /// <summary>
        /// Gets or sets the vertical splitter style.
        /// </summary>
        [
            Browsable(false)
        ]
        internal VerticalSplitterStyle VerticalSplitterStyle
        {
            get => this.verticalSplitterStyle;
            set
            {
                if (this.verticalSplitterStyle != value)
                {
                    this.verticalSplitterStyle = value;
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the vertical splitter width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(5),
                Description("Specifies the initial vertical splitter width.")
        ]
        public int VerticalSplitterWidth
        {
            get => this.verticalSplitterWidth;
            set
            {
                if (this.verticalSplitterWidth != value)
                {
                    this.verticalSplitterWidth = value;
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the horizontal splitter height.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(5),
                Description("Specifies the initial horizontal splitter height.")
        ]
        public int HorizontalSplitterHeight
        {
            get => this.horizontalSplitterHeight;
            set
            {
                if (this.horizontalSplitterHeight != value)
                {
                    this.horizontalSplitterHeight = value;
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred column width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0),
                Description("Specifies the preferred column width.")
        ]
        public int PreferredColumnWidth
        {
            get => this.preferredColumnWidth;
            set
            {
                if (this.preferredColumnWidth != value)
                {
                    this.preferredColumnWidth = value;
                    this.OnPreferredColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred column width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0),
                Description("Specifies the minimum column width.")
        ]
        public int MinimumColumnWidth
        {
            get => this.minimumColumnWidth;
            set
            {
                if (this.minimumColumnWidth != value)
                {
                    this.minimumColumnWidth = value;
                    this.OnMinimumColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum column width.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0),
                Description("Specifies the maximum column width.")
        ]
        public int MaximumColumnWidth
        {
            get => this.maximumColumnWidth;
            set
            {
                if (this.maximumColumnWidth != value)
                {
                    this.maximumColumnWidth = value;
                    this.OnMaximumColumnWidthChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preferred horizontal splitter position.
        /// </summary>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(0.50),
                Description("Specifies the horizontal splitter position.")
        ]
        public double HorizontalSplitterPosition
        {
            get => this.horizontalSplitterPosition;
            set
            {
                //	Check the new horizontal splitter position.
                if (value < MINIMUM_HORIZONTAL_SPLITTER_POSITION)
                {
                    value = MINIMUM_HORIZONTAL_SPLITTER_POSITION;
                }
                else if (value > this.MaximumHorizontalSplitterPosition)
                {
                    value = this.MaximumHorizontalSplitterPosition;
                }

                //	If the horizontal splitter position is changing, change it.
                if (this.horizontalSplitterPosition != value)
                {
                    //	Change it.
                    this.horizontalSplitterPosition = value;

                    //	Raise the HorizontalSplitterPositionChanged event.
                    this.OnHorizontalSplitterPositionChanged(EventArgs.Empty);

                    //	Layout and invalidate.
                    this.PerformLayout();
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Specifies the maximum horizontal splitter position (as a percentage of the overall column size).
        /// </summary>
        [
            Category("Behavior"),
                DefaultValue(0.80),
                Description("Specifies the maximum horizontal splitter position (as a percentage of the overall column size).")
        ]
        public double MaximumHorizontalSplitterPosition
        {
            get => this.maximumHorizontalSplitterPosition;
            set
            {
                this.maximumHorizontalSplitterPosition = value;

                //update the current horizontal  position in case it exceeds the new limit.
                this.HorizontalSplitterPosition = this.HorizontalSplitterPosition;
            }
        }

        /// <summary>
        /// Gets or Sets the layout position of the horizontal splitter (in Y pixels).
        /// </summary>
        public int HorizontalSplitterLayoutPosition
        {
            set => this.HorizontalSplitterPosition = value == 0 || this.PaneLayoutHeight == 0
                    ? this.MinimumHorizontalSplitterLayoutPosition
                    : ((double)value) / this.PaneLayoutHeight;
            get => (int)(this.PaneLayoutHeight * this.HorizontalSplitterPosition);
        }

        /// <summary>
        /// Raises the MaximumColumnWidthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMaximumColumnWidthChanged(EventArgs e) => MaximumColumnWidthChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the MinimumColumnWidthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnMinimumColumnWidthChanged(EventArgs e) => MinimumColumnWidthChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the PreferredColumnWidthChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnPreferredColumnWidthChanged(EventArgs e) => PreferredColumnWidthChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the HorizontalSplitterPositionChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterPositionChanged(EventArgs e) => HorizontalSplitterPositionChanged?.Invoke(this, e);

        /// <summary>
        /// Raises the HorizontalSplitterBeginMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterBeginMove(EventArgs e) => HorizontalSplitterBeginMove?.Invoke(this, e);

        /// <summary>
        /// Raises the HorizontalSplitterEndMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterEndMove(EventArgs e) => HorizontalSplitterEndMove?.Invoke(this, e);

        /// <summary>
        /// Raises the HorizontalSplitterMoving event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnHorizontalSplitterMoving(EventArgs e) => HorizontalSplitterMoving?.Invoke(this, e);

        /// <summary>
        /// Raises the VerticalSplitterBeginMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVerticalSplitterBeginMove(EventArgs e) => VerticalSplitterBeginMove?.Invoke(this, e);

        /// <summary>
        /// Raises the VerticalSplitterEndMove event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVerticalSplitterEndMove(EventArgs e) => VerticalSplitterEndMove?.Invoke(this, e);

        /// <summary>
        /// Raises the VerticalSplitterMoving event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnVerticalSplitterMoving(EventArgs e) => VerticalSplitterMoving?.Invoke(this, e);

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Layout the vertical splitter.
            var layoutRectangle = this.LayoutVerticalSplitter();

            //	Layout the upper and lower panes if they are both visible.
            if (this.UpperPane.Visible && this.LowerPane.Visible)
            {
                //	Calculate the pane layout height (the area available for layout of the upper
                //	and lower panes).
                var paneLayoutHeight = layoutRectangle.Height - this.horizontalSplitterHeight;

                //	If the upper pane is fixed height, layout the column this way.
                if (this.UpperPane.FixedHeightLayout)
                {
                    //	If the upper pane's fixed height is larger than the layout rectangle height,
                    //	layout just the upper pane.
                    if (this.UpperPane.FixedHeight > layoutRectangle.Height)
                    {
                        this.UpperPane.VirtualBounds = layoutRectangle;
                        this.splitterLightweightControlHorizontal.Visible = false;
                        this.LowerPane.VirtualBounds = Rectangle.Empty;
                        this.UpperPane.PerformLayout();
                    }
                    //	Layout both the upper and lower panes.
                    else
                    {
                        //	Layout the upper pane lightweight control.
                        this.UpperPane.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                layoutRectangle.Y,
                                                                                layoutRectangle.Width,
                                                                                this.UpperPane.FixedHeight);
                        this.UpperPane.PerformLayout();

                        //	Layout the horizontal splitter lightweight control and disable it.
                        this.splitterLightweightControlHorizontal.Visible = true;
                        this.splitterLightweightControlHorizontal.Enabled = false;
                        this.splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                            this.UpperPane.VirtualBounds.Bottom,
                                                                                            layoutRectangle.Width,
                                                                                            this.horizontalSplitterHeight);

                        //	Layout the lower pane lightweight control.
                        this.LowerPane.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                this.splitterLightweightControlHorizontal.VirtualBounds.Bottom,
                                                                                layoutRectangle.Width,
                                                                                layoutRectangle.Height - (this.UpperPane.VirtualHeight + this.horizontalSplitterHeight));
                        this.LowerPane.PerformLayout();
                    }
                }
                //	If the lower pane is fixed height, layout the column this way.
                else if (this.LowerPane.FixedHeightLayout)
                {
                    //	If the upper pane's fixed height is larger than the layout rectangle height,
                    //	layout just the upper pane.
                    if (this.LowerPane.FixedHeight > layoutRectangle.Height)
                    {
                        this.LowerPane.VirtualBounds = layoutRectangle;
                        this.splitterLightweightControlHorizontal.Visible = false;
                        this.LowerPane.VirtualBounds = Rectangle.Empty;
                        this.LowerPane.PerformLayout();
                    }
                    //	Layout both the upper and lower panes.
                    else
                    {
                        //	Layout the lower pane lightweight control.
                        this.LowerPane.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                layoutRectangle.Bottom - this.LowerPane.FixedHeight,
                                                                                layoutRectangle.Width,
                                                                                this.LowerPane.FixedHeight);
                        this.LowerPane.PerformLayout();

                        //	Layout the horizontal splitter lightweight control and disable it.
                        this.splitterLightweightControlHorizontal.Visible = true;
                        this.splitterLightweightControlHorizontal.Enabled = false;
                        this.splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                            this.LowerPane.VirtualBounds.Top - this.horizontalSplitterHeight,
                                                                                            layoutRectangle.Width,
                                                                                            this.horizontalSplitterHeight);

                        //	Layout the upper pane lightweight control.
                        this.UpperPane.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                layoutRectangle.Y,
                                                                                layoutRectangle.Width,
                                                                                this.splitterLightweightControlHorizontal.VirtualBounds.Top);
                        this.UpperPane.PerformLayout();
                    }
                }
                //	If the pane layout height is too small (i.e. there isn't enough room to show
                //	both panes), err on the side of showing just the top pane.  This is an extreme
                //	edge condition.
                else if (paneLayoutHeight < 10 * this.horizontalSplitterHeight)
                {
                    //	Only the upper pane lightweight control is visible.
                    this.UpperPane.VirtualBounds = layoutRectangle;
                    this.LowerPane.VirtualBounds = Rectangle.Empty;
                    this.splitterLightweightControlHorizontal.Visible = false;
                    this.splitterLightweightControlHorizontal.VirtualBounds = Rectangle.Empty;
                }
                else
                {
                    //	Get the horizontal splitter layout position.
                    var horizontalSplitterLayoutPosition = this.HorizontalSplitterLayoutPosition;

                    //	Layout the upper pane lightweight control.
                    this.UpperPane.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                            layoutRectangle.Y,
                                                                            layoutRectangle.Width,
                                                                            horizontalSplitterLayoutPosition);
                    this.UpperPane.PerformLayout();

                    //	Layout the horizontal splitter lightweight control and enable it.
                    this.splitterLightweightControlHorizontal.Visible = true;
                    this.splitterLightweightControlHorizontal.Enabled = true;
                    this.splitterLightweightControlHorizontal.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                        this.UpperPane.VirtualBounds.Bottom,
                                                                                        layoutRectangle.Width,
                                                                                        this.horizontalSplitterHeight);

                    //	Layout the lower pane lightweight control.
                    this.LowerPane.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                                this.splitterLightweightControlHorizontal.VirtualBounds.Bottom,
                                                                                                layoutRectangle.Width,
                                                                                                layoutRectangle.Height - (this.UpperPane.VirtualHeight + this.horizontalSplitterHeight));
                    this.LowerPane.PerformLayout();
                }
            }
            //	Layout the upper pane, if it's visible.  Note that we ignore the FixedHeight
            //	property of the pane in this case because it doesn't make sense to layout a
            //	pane that doesn't fill the entire height of the column.
            else if (this.UpperPane.Visible)
            {
                //	Only the upper pane lightweight control is visible.
                this.UpperPane.VirtualBounds = layoutRectangle;
                this.splitterLightweightControlHorizontal.Visible = false;
                this.UpperPane.PerformLayout();
            }
            //	Layout the lower pane, if it's visible.  Note that we ignore the FixedHeight
            //	property of the pane in this case because it doesn't make sense to layout a
            //	pane that doesn't fill the entire height of the column.
            else if (this.LowerPane.Visible)
            {
                //	Only the lower pane lightweight control is visible.
                this.LowerPane.VirtualBounds = layoutRectangle;
                this.splitterLightweightControlHorizontal.Visible = false;
                this.LowerPane.PerformLayout();
            }
        }

        /// <summary>
        /// Gets the maximum width increase.
        /// </summary>
        private int MaximumWidthIncrease
        {
            get
            {
                Debug.Assert(this.VirtualWidth <= this.MaximumColumnWidth, "The column is wider than it's maximum width.");
                return Math.Max(0, this.MaximumColumnWidth - this.VirtualWidth);
            }
        }

        /// <summary>
        /// Gets the maximum width decrease.
        /// </summary>
        private int MaximumWidthDecrease
        {
            get
            {
                Debug.Assert(this.VirtualWidth >= this.MinimumColumnWidth, "The column is narrower than it's minimum width.");
                return Math.Max(0, this.VirtualWidth - this.MinimumColumnWidth);
            }
        }

        /// <summary>
        /// Gets the pane layout height.
        /// </summary>
        private int PaneLayoutHeight => Math.Max(0, this.VirtualClientRectangle.Height - this.horizontalSplitterHeight);

        /// <summary>
        /// Gets the minimum layout position of the horizontal splitter.
        /// </summary>
        private int MinimumHorizontalSplitterLayoutPosition => (int)(this.PaneLayoutHeight * MINIMUM_HORIZONTAL_SPLITTER_POSITION);

        /// <summary>
        /// Gets the maximum layout position of the horizontal splitter.
        /// </summary>
        private int MaximumHorizontalSplitterLayoutPosition => (int)(this.PaneLayoutHeight * this.MaximumHorizontalSplitterPosition);

        /// <summary>
        /// Helper that performs layout logic for the vertical splitter.
        /// </summary>
        /// <returns>The layout rectangle for the next phases of layout.</returns>
        private Rectangle LayoutVerticalSplitter()
        {
            //	Obtain the layout rectangle.
            var layoutRectangle = this.VirtualClientRectangle;

            //	Layout the vertical splitter lightweight control and adjust the layout rectangle
            //	as needed.
            if (this.verticalSplitterStyle == VerticalSplitterStyle.None)
            {
                //	No vertical splitter lightweight control.
                this.splitterLightweightControlVertical.Visible = false;
                this.splitterLightweightControlVertical.VirtualBounds = Rectangle.Empty;
            }
            else if (this.verticalSplitterStyle == VerticalSplitterStyle.Left)
            {
                //	Left vertical splitter lightweight control.
                this.splitterLightweightControlVertical.Visible = true;
                this.splitterLightweightControlVertical.VirtualBounds = new Rectangle(layoutRectangle.X,
                                                                                    layoutRectangle.Y,
                                                                                    this.verticalSplitterWidth,
                                                                                    layoutRectangle.Height);

                //	Adjust the layout rectangle.
                layoutRectangle.X += this.verticalSplitterWidth;
                layoutRectangle.Width -= this.verticalSplitterWidth;
            }
            else if (this.verticalSplitterStyle == VerticalSplitterStyle.Right)
            {
                //	Right vertical splitter lightweight control.
                this.splitterLightweightControlVertical.Visible = true;
                this.splitterLightweightControlVertical.VirtualBounds = new Rectangle(layoutRectangle.Right - this.verticalSplitterWidth,
                                                                                    layoutRectangle.Top,
                                                                                    this.verticalSplitterWidth,
                                                                                    layoutRectangle.Height);

                //	Adjust the layout rectangle.
                layoutRectangle.Width -= this.verticalSplitterWidth;
            }

            //	Done!  Return the layout rectangle.
            return layoutRectangle;
        }

        private void LayoutWithFixedHeightUpperPane(Rectangle layoutRectangle)
        {
        }

        private void LayoutWithFixedHeightLowerPane(Rectangle layoutRectangle)
        {
        }

        /// <summary>
        /// Helper to adjust the Position in the LightweightSplitterEventArgs for the vertical splitter.
        /// </summary>
        /// <param name="e">LightweightSplitterEventArgs to adjust.</param>
        private void AdjustVerticalLightweightSplitterEventArgsPosition(ref LightweightSplitterEventArgs e)
        {
            //	If the vertical splitter style is non, we shouldn't receive this event.
            Debug.Assert(this.verticalSplitterStyle != VerticalSplitterStyle.None);
            if (this.verticalSplitterStyle == VerticalSplitterStyle.None)
            {
                return;
            }

            //	Left or right splitter style.
            if (this.verticalSplitterStyle == VerticalSplitterStyle.Left)
            {
                if (e.Position < 0)
                {
                    if (Math.Abs(e.Position) > this.MaximumWidthIncrease)
                    {
                        e.Position = this.MaximumWidthIncrease * -1;
                    }
                }
                else
                {
                    if (e.Position > this.MaximumWidthDecrease)
                    {
                        e.Position = this.MaximumWidthDecrease;
                    }
                }
            }
            else if (this.verticalSplitterStyle == VerticalSplitterStyle.Right)
            {
                if (e.Position > 0)
                {
                    if (e.Position > this.MaximumWidthIncrease)
                    {
                        e.Position = this.MaximumWidthIncrease;
                    }
                }
                else
                {
                    if (Math.Abs(e.Position) > this.MaximumWidthDecrease)
                    {
                        e.Position = this.MaximumWidthDecrease * -1;
                    }
                }
            }
        }

        /// <summary>
        /// Helper to adjust the Position in the LightweightSplitterEventArgs for the horizontal splitter.
        /// </summary>
        /// <param name="e">LightweightSplitterEventArgs to adjust.</param>
        private void AdjustHorizontalLightweightSplitterEventArgsPosition(ref LightweightSplitterEventArgs e)
        {
            var horizontalSplitterLayoutPosition = this.HorizontalSplitterLayoutPosition;
            if (e.Position < 0)
            {
                if (this.HorizontalSplitterLayoutPosition + e.Position < this.MinimumHorizontalSplitterLayoutPosition)
                {
                    e.Position = this.MinimumHorizontalSplitterLayoutPosition - horizontalSplitterLayoutPosition;
                }
            }
            else
            {
                if (this.HorizontalSplitterLayoutPosition + e.Position > this.MaximumHorizontalSplitterLayoutPosition)
                {
                    e.Position = this.MaximumHorizontalSplitterLayoutPosition - horizontalSplitterLayoutPosition;
                }
            }
        }

        /// <summary>
        /// splitterLightweightControlVertical_SplitterBeginMove event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterLightweightControlVertical_SplitterBeginMove(object sender, EventArgs e)
        {
            this.startingPreferredColumnWidth = this.PreferredColumnWidth;

            //	Raise the VerticalSplitterBeginMove event.
            this.OnVerticalSplitterBeginMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlVertical_SplitterEndMove event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterLightweightControlVertical_SplitterEndMove(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                //	Adjust the vertical splitter position.
                this.AdjustVerticalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the preferred column width.
                if (this.verticalSplitterStyle == VerticalSplitterStyle.Left)
                {
                    this.PreferredColumnWidth -= e.Position;
                }
                else if (this.verticalSplitterStyle == VerticalSplitterStyle.Right)
                {
                    this.PreferredColumnWidth += e.Position;
                }
            }

            //	Raise the VerticalSplitterEndMove event.
            this.OnVerticalSplitterEndMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlVertical_SplitterMoving event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void splitterLightweightControlVertical_SplitterMoving(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                //	Adjust the splitter position.
                this.AdjustVerticalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the preferred column width - in real time.
                if (this.verticalSplitterStyle == VerticalSplitterStyle.Left)
                {
                    this.PreferredColumnWidth -= e.Position;
                }
                else if (this.verticalSplitterStyle == VerticalSplitterStyle.Right)
                {
                    this.PreferredColumnWidth += e.Position;
                }

                //	Update manually to keep the screen as up to date as possible.
                this.Update();
            }

            //	Raise the VerticalSplitterMoving event.
            this.OnVerticalSplitterMoving(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlHorizontal_SplitterBeginMove event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void splitterLightweightControlHorizontal_SplitterBeginMove(object sender, EventArgs e)
        {
            this.startingHorizontalSplitterPosition = this.HorizontalSplitterPosition;

            //	Raise the HorizontalSplitterBeginMove event.
            this.OnHorizontalSplitterBeginMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlHorizontal_SplitterEndMove event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void splitterLightweightControlHorizontal_SplitterEndMove(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                //	Adjust the horizontal splitter position.
                this.AdjustHorizontalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the horizontal splitter position.
                this.HorizontalSplitterPosition += (double)e.Position / this.PaneLayoutHeight;
            }

            //	Raise the HorizontalSplitterEndMove event.
            this.OnHorizontalSplitterEndMove(EventArgs.Empty);
        }

        /// <summary>
        /// splitterLightweightControlHorizontal_SplitterMoving event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void splitterLightweightControlHorizontal_SplitterMoving(object sender, LightweightSplitterEventArgs e)
        {
            //	If the splitter has moved.
            if (e.Position != 0)
            {
                this.AdjustHorizontalLightweightSplitterEventArgsPosition(ref e);

                //	Adjust the horizontal splitter position.
                this.HorizontalSplitterPosition += (double)e.Position / this.PaneLayoutHeight;

                //	Update manually to keep the screen as up to date as possible.
                this.Update();
            }

            //	Raise the HorizontalSplitterMoving event.
            this.OnHorizontalSplitterMoving(EventArgs.Empty);
        }

        /// <summary>
        /// workspaceColumnPaneUpper_VisibleChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void workspaceColumnPaneUpper_VisibleChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        /// workspaceColumnPaneLower_VisibleChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void workspaceColumnPaneLower_VisibleChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }
    }
}
