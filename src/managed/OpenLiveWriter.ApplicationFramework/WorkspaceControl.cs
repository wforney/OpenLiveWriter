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

    /// <summary>
    /// The WorkspaceControl control provides a multi-pane workspace, with an optional
    /// command bar.
    /// </summary>
    public class WorkspaceControl : LightweightControlContainerControl
    {
        /// <summary>
        /// The default left column preferred width.
        /// </summary>
        private const int LEFT_COLUMN_DEFAULT_PREFERRED_WIDTH = 150;

        /// <summary>
        /// The default center column preferred width.
        /// </summary>
        private const int CENTER_COLUMN_DEFAULT_PREFERRED_WIDTH = 20;

        /// <summary>
        /// The default right column preferred width.
        /// </summary>
        private const int RIGHT_COLUMN_DEFAULT_PREFERRED_WIDTH = 150;

        /// <summary>
        /// The default left column minimum width.
        /// </summary>
        private const int LEFT_COLUMN_DEFAULT_MINIMUM_WIDTH = 10;

        /// <summary>
        /// The default center column minimum width.
        /// </summary>
        private const int CENTER_COLUMN_DEFAULT_MINIMUM_WIDTH = 10;

        /// <summary>
        /// The default right column minimum width.
        /// </summary>
        private const int RIGHT_COLUMN_DEFAULT_MINIMUM_WIDTH = 10;

        /// <summary>
        /// Required designer cruft.
        /// </summary>
        private IContainer components = null;

        /// <summary>
        /// Gets the CommandManager for this WorkspaceControl.
        /// </summary>
        public CommandManager CommandManager { get; }

        /// <summary>
        /// Gets or sets the top layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the top layout margin.")
        ]
        public int TopLayoutMargin { get; set; }

        /// <summary>
        /// Gets or sets the left layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the left layout margin.")
        ]
        public int LeftLayoutMargin { get; set; }

        /// <summary>
        /// Gets or sets the bottom layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the bottom layout margin.")
        ]
        public int BottomLayoutMargin { get; set; }

        /// <summary>
        /// Gets or sets the right layout margin.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(0),
                Description("Specifies the right layout margin.")
        ]
        public int RightLayoutMargin { get; set; }

        /// <summary>
        ///	Gets the top color.
        /// </summary>
        public virtual Color TopColor => SystemColors.Control;

        /// <summary>
        ///	Gets the bottom color.
        /// </summary>
        public virtual Color BottomColor => SystemColors.Control;

        /// <summary>
        /// Gets the first command bar lightweight control.
        /// </summary>
        public virtual CommandBarLightweightControl FirstCommandBarLightweightControl => null;

        /// <summary>
        /// Gets the second command bar lightweight control.
        /// </summary>
        public virtual CommandBarLightweightControl SecondCommandBarLightweightControl => null;

        /// <summary>
        /// Gets the left WorkspaceColumnLightweightControl.
        /// </summary>
        public WorkspaceColumnLightweightControl LeftColumn { get; private set; }

        /// <summary>
        /// Gets the center WorkspaceColumnLightweightControl.
        /// </summary>
        public WorkspaceColumnLightweightControl CenterColumn { get; private set; }

        /// <summary>
        /// Gets the right WorkspaceColumnLightweightControl.
        /// </summary>
        public WorkspaceColumnLightweightControl RightColumn { get; private set; }

        /// <summary>
        /// Initializes a new instance of the WorkspaceControl class.
        /// </summary>
        public WorkspaceControl(CommandManager commandManager)
        {
            // This call is required by the Windows Form Designer.
            this.InitializeComponent();

            //	Set the CommandManager.
            this.CommandManager = commandManager;

            //	Turn on double buffered painting.
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
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
            this.components = new Container();
            this.LeftColumn = new WorkspaceColumnLightweightControl(this.components);
            this.CenterColumn = new WorkspaceColumnLightweightControl(this.components);
            this.RightColumn = new WorkspaceColumnLightweightControl(this.components);
            ((ISupportInitialize)this.LeftColumn).BeginInit();
            ((ISupportInitialize)this.CenterColumn).BeginInit();
            ((ISupportInitialize)this.RightColumn).BeginInit();
            ((ISupportInitialize)this).BeginInit();
            //
            // leftColumn
            //
            this.LeftColumn.HorizontalSplitterHeight = 4;
            this.LeftColumn.LightweightControlContainerControl = this;
            this.LeftColumn.MinimumColumnWidth = 30;
            this.LeftColumn.PreferredColumnWidth = 190;
            this.LeftColumn.VerticalSplitterWidth = 4;
            this.LeftColumn.MaximumColumnWidthChanged += new EventHandler(this.leftColumn_MaximumColumnWidthChanged);
            this.LeftColumn.PreferredColumnWidthChanged += new EventHandler(this.leftColumn_PreferredColumnWidthChanged);
            this.LeftColumn.MinimumColumnWidthChanged += new EventHandler(this.leftColumn_MinimumColumnWidthChanged);
            //
            // centerColumn
            //
            this.CenterColumn.HorizontalSplitterHeight = 4;
            this.CenterColumn.LightweightControlContainerControl = this;
            this.CenterColumn.MinimumColumnWidth = 30;
            this.CenterColumn.PreferredColumnWidth = 30;
            this.CenterColumn.VerticalSplitterWidth = 4;
            //
            // rightColumn
            //
            this.RightColumn.HorizontalSplitterHeight = 4;
            this.RightColumn.LightweightControlContainerControl = this;
            this.RightColumn.MinimumColumnWidth = 30;
            this.RightColumn.PreferredColumnWidth = 150;
            this.RightColumn.VerticalSplitterWidth = 4;
            this.RightColumn.MaximumColumnWidthChanged += new EventHandler(this.rightColumn_MaximumColumnWidthChanged);
            this.RightColumn.PreferredColumnWidthChanged += new EventHandler(this.rightColumn_PreferredColumnWidthChanged);
            this.RightColumn.MinimumColumnWidthChanged += new EventHandler(this.rightColumn_MinimumColumnWidthChanged);
            //
            // WorkspaceControl
            //
            this.BackColor = SystemColors.Control;
            this.Name = "WorkspaceControl";
            this.Size = new Size(294, 286);
            ((ISupportInitialize)this.LeftColumn).EndInit();
            ((ISupportInitialize)this.CenterColumn).EndInit();
            ((ISupportInitialize)this.RightColumn).EndInit();
            ((ISupportInitialize)this).EndInit();

        }
        #endregion

        /// <summary>
        /// Raises the DragEnter event.
        /// </summary>
        /// <param name="e">A DragEventArgs that contains the event data.</param>
        protected override void OnDragEnter(DragEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnDragEnter(e);

            //	None.
            e.Effect = DragDropEffects.None;
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	If we have no parent, no layout is required.
            if (this.Parent == null)
            {
                return;
            }

            //	Layout the command bar lightweight controls.  The return is the column layout rectangle.
            var columnLayoutRectangle = this.LayoutCommandBars();

            //	Set the initial (to be adjusted below) column widths.
            var leftColumnWidth = this.LeftColumnPreferredWidth;
            var centerColumnWidth = this.CenterColumnMinimumWidth;
            var rightColumnWidth = this.RightColumnPreferredWidth;

            //	Adjust the column widths as needed for the layout width.
            if (leftColumnWidth + centerColumnWidth + rightColumnWidth > columnLayoutRectangle.Width)
            {
                //	Calculate the width that is available to the left and right columns.
                var availableWidth = columnLayoutRectangle.Width - centerColumnWidth;

                //	Adjust the left and right column widths.
                if (this.LeftColumnVisible && this.RightColumnVisible)
                {
                    //	Calculate the relative width of the left column.
                    var leftColumnRelativeWidth = ((double)leftColumnWidth) / (leftColumnWidth + rightColumnWidth);

                    //	Adjust the left and right column widths.
                    leftColumnWidth = Math.Max((int)(leftColumnRelativeWidth * availableWidth), this.LeftColumnMinimumWidth);
                    rightColumnWidth = Math.Max(availableWidth - leftColumnWidth, this.RightColumnMinimumWidth);
                }
                else if (this.LeftColumnVisible)
                {
                    //	Only the left column is visible, so it gets all the available width.
                    leftColumnWidth = Math.Max(availableWidth, this.LeftColumnMinimumWidth);
                }
                else if (this.RightColumnVisible)
                {
                    //	Only the right column is visible, so it gets all the available width.
                    rightColumnWidth = Math.Max(availableWidth, this.RightColumnMinimumWidth);
                }
            }
            else
            {
                //	We have a surplus of room.  Allocate additional space to the center column, if
                //	if is visible, or the left column if it is not.
                if (this.CenterColumnVisible)
                {
                    centerColumnWidth = columnLayoutRectangle.Width - (leftColumnWidth + rightColumnWidth);
                }
                else
                {
                    leftColumnWidth = columnLayoutRectangle.Width - rightColumnWidth;
                }
            }

            //	Set the layout X offset.
            var layoutX = columnLayoutRectangle.X;

            //	Layout the left column, if it is visible.
            if (this.LeftColumnVisible)
            {
                //	Set the virtual bounds of the left column.
                this.LeftColumn.VirtualBounds = new Rectangle(layoutX,
                                                            columnLayoutRectangle.Y,
                                                            leftColumnWidth,
                                                            columnLayoutRectangle.Height);
                this.LeftColumn.PerformLayout();

                //	Adjust the layout X to account for the left column.
                layoutX += leftColumnWidth;

                //	Update the left column vertical splitter and maximum column width.
                if (this.CenterColumnVisible)
                {
                    //	Turn on the left column vertical splitter on the right side.
                    this.LeftColumn.VerticalSplitterStyle = VerticalSplitterStyle.Right;

                    //	Set the left column's maximum width.
                    this.LeftColumn.MaximumColumnWidth = columnLayoutRectangle.Width -
                                                    (this.CenterColumnMinimumWidth + this.RightColumnPreferredWidth);
                }
                else
                {
                    this.LeftColumn.VerticalSplitterStyle = VerticalSplitterStyle.None;
                    this.LeftColumn.MaximumColumnWidth = 0;
                }
            }

            //	Layout the center column.
            if (this.CenterColumnVisible)
            {
                //	Set the virtual bounds of the center column.
                this.CenterColumn.VirtualBounds = new Rectangle(layoutX,
                                                            columnLayoutRectangle.Y,
                                                            centerColumnWidth,
                                                            columnLayoutRectangle.Height);
                this.CenterColumn.PerformLayout();

                //	Adjust the layout X to account for the center column.
                layoutX += centerColumnWidth;

                //	The center column never has a vertical splitter or a maximum column width.
                this.CenterColumn.VerticalSplitterStyle = VerticalSplitterStyle.None;
                this.CenterColumn.MaximumColumnWidth = 0;
            }

            //	Layout the right column.
            if (this.RightColumnVisible)
            {
                //	Set the virtual bounds of the right column.
                this.RightColumn.VirtualBounds = new Rectangle(layoutX,
                                                            columnLayoutRectangle.Y,
                                                            rightColumnWidth,
                                                            columnLayoutRectangle.Height);
                this.RightColumn.PerformLayout();

                //	Update the right column vertical splitter and maximum column width.
                if (this.CenterColumnVisible || this.LeftColumnVisible)
                {
                    //	Turn on the right column's vertical splitter on the left side.
                    this.RightColumn.VerticalSplitterStyle = VerticalSplitterStyle.Left;

                    //	Set the right column's maximum width.
                    this.RightColumn.MaximumColumnWidth = columnLayoutRectangle.Width -
                                                        (this.LeftColumnPreferredWidth + this.CenterColumnMinimumWidth);
                }
                else
                {
                    this.RightColumn.VerticalSplitterStyle = VerticalSplitterStyle.None;
                    this.RightColumn.MaximumColumnWidth = 0;
                }
            }
        }

        /// <summary>
        /// Override background painting.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //	Fill the background.
            if (this.TopColor == this.BottomColor)
            {
                using (var solidBrush = new SolidBrush(this.TopColor))
                {
                    e.Graphics.FillRectangle(solidBrush, this.ClientRectangle);
                }
            }
            else
            {
                using (var linearGradientBrush = new LinearGradientBrush(this.ClientRectangle, this.TopColor, this.BottomColor, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(linearGradientBrush, this.ClientRectangle);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the left column is visible.
        /// </summary>
        private bool LeftColumnVisible => this.LeftColumn != null && this.LeftColumn.Visible;

        /// <summary>
        /// Gets a value indicating whether the center column is visible.
        /// </summary>
        private bool CenterColumnVisible => this.CenterColumn != null && this.CenterColumn.Visible;

        /// <summary>
        /// Gets a value indicating whether the right column is visible.
        /// </summary>
        private bool RightColumnVisible => this.RightColumn != null && this.RightColumn.Visible;

        /// <summary>
        /// Gets the preferred width of the left column.
        /// </summary>
        private int LeftColumnPreferredWidth
        {
            get
            {
                if (!this.LeftColumnVisible)
                {
                    return 0;
                }
                else
                {
                    if (this.LeftColumn.PreferredColumnWidth == 0)
                    {
                        this.LeftColumn.PreferredColumnWidth = LEFT_COLUMN_DEFAULT_PREFERRED_WIDTH;
                    }

                    return this.LeftColumn.PreferredColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the preferred width of the center column.
        /// </summary>
        private int CenterColumnPreferredWidth
        {
            get
            {
                if (!this.CenterColumnVisible)
                {
                    return 0;
                }
                else
                {
                    if (this.CenterColumn.PreferredColumnWidth == 0)
                    {
                        this.CenterColumn.PreferredColumnWidth = CENTER_COLUMN_DEFAULT_PREFERRED_WIDTH;
                    }

                    return this.CenterColumn.PreferredColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the preferred width of the right column.
        /// </summary>
        private int RightColumnPreferredWidth
        {
            get
            {
                if (!this.RightColumnVisible)
                {
                    return 0;
                }
                else
                {
                    if (this.RightColumn.PreferredColumnWidth == 0)
                    {
                        this.RightColumn.PreferredColumnWidth = RIGHT_COLUMN_DEFAULT_PREFERRED_WIDTH;
                    }

                    return this.RightColumn.PreferredColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the minimum width of the left column.
        /// </summary>
        private int LeftColumnMinimumWidth
        {
            get
            {
                if (!this.LeftColumnVisible)
                {
                    return 0;
                }
                else
                {
                    if (this.LeftColumn.MinimumColumnWidth == 0)
                    {
                        this.LeftColumn.MinimumColumnWidth = LEFT_COLUMN_DEFAULT_MINIMUM_WIDTH;
                    }

                    return this.LeftColumn.MinimumColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the minimum width of the center column.
        /// </summary>
        private int CenterColumnMinimumWidth
        {
            get
            {
                if (!this.CenterColumnVisible)
                {
                    return 0;
                }
                else
                {
                    if (this.CenterColumn.MinimumColumnWidth == 0)
                    {
                        this.CenterColumn.MinimumColumnWidth = CENTER_COLUMN_DEFAULT_MINIMUM_WIDTH;
                    }

                    return this.CenterColumn.MinimumColumnWidth;
                }
            }
        }

        /// <summary>
        /// Gets the minimum width of the right column.
        /// </summary>
        private int RightColumnMinimumWidth
        {
            get
            {
                if (!this.RightColumnVisible)
                {
                    return 0;
                }
                else
                {
                    if (this.RightColumn.MinimumColumnWidth == 0)
                    {
                        this.RightColumn.MinimumColumnWidth = RIGHT_COLUMN_DEFAULT_MINIMUM_WIDTH;
                    }

                    return this.RightColumn.MinimumColumnWidth;
                }
            }
        }

        /// <summary>
        /// Layout the command bars.
        /// </summary>
        /// <returns>Column layout rectangle.</returns>
        private Rectangle LayoutCommandBars()
        {
            //	The command bar area height (set below).
            var commandBarAreaHeight = 0;

            //	If we have a "first" command bar lightweight control, lay it out.
            if (this.FirstCommandBarLightweightControl != null)
            {
                //	If a command bar definition has been supplied, layout the "first" command bar
                //	lightweight control. Otherwise, hide it.
                if (this.FirstCommandBarLightweightControl.CommandBarDefinition != null)
                {
                    //	Make the command bar visible before getting height.
                    this.FirstCommandBarLightweightControl.Visible = true;

                    //	Obtain the height and lay it out.
                    var firstCommandBarHeight = this.FirstCommandBarLightweightControl.DefaultVirtualSize.Height;
                    this.FirstCommandBarLightweightControl.VirtualBounds = new Rectangle(0, commandBarAreaHeight, this.Width, firstCommandBarHeight);

                    //	Increment the command bar area height.
                    commandBarAreaHeight += firstCommandBarHeight;
                }
                else
                {
                    //	Hide the command bar lightweight control.
                    this.FirstCommandBarLightweightControl.Visible = false;
                    this.FirstCommandBarLightweightControl.VirtualBounds = Rectangle.Empty;
                }
            }

            //	If we have a "second" command bar lightweight control, lay it out.
            if (this.SecondCommandBarLightweightControl != null)
            {
                //	If a command bar definition has been supplied, layout the "second" command bar
                //	lightweight control. Otherwise, hide it.
                if (this.SecondCommandBarLightweightControl.CommandBarDefinition != null)
                {
                    //	Make the command bar visible before getting height.
                    this.SecondCommandBarLightweightControl.Visible = true;

                    //	Obtain the height and lay it out.
                    var secondCommandBarHeight = this.SecondCommandBarLightweightControl.DefaultVirtualSize.Height;
                    this.SecondCommandBarLightweightControl.VirtualBounds = new Rectangle(0, commandBarAreaHeight, this.Width, secondCommandBarHeight);

                    //	Increment the command bar area height.
                    commandBarAreaHeight += secondCommandBarHeight;
                }
                else
                {
                    //	Hide the command bar lightweight control.
                    this.SecondCommandBarLightweightControl.Visible = false;
                    this.SecondCommandBarLightweightControl.VirtualBounds = Rectangle.Empty;
                }
            }

            //	Return the column layout rectangle.
            return new Rectangle(this.LeftLayoutMargin,
                                    commandBarAreaHeight + this.TopLayoutMargin,
                                    this.Width - (this.LeftLayoutMargin + this.RightLayoutMargin),
                                    this.Height - commandBarAreaHeight - (this.TopLayoutMargin + this.BottomLayoutMargin));
        }

        /// <summary>
        /// leftColumn_MaximumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_MaximumColumnWidthChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        /// leftColumn_MinimumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_MinimumColumnWidthChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        /// leftColumn_PreferredColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void leftColumn_PreferredColumnWidthChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        /// rightColumn_MaximumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_MaximumColumnWidthChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        /// rightColumn_MinimumColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_MinimumColumnWidthChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }

        /// <summary>
        /// rightColumn_PreferredColumnWidthChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void rightColumn_PreferredColumnWidthChanged(object sender, EventArgs e)
        {
            this.PerformLayout();
            this.Invalidate();
        }
    }
}
