// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// CommandBar lightweight control.
    /// </summary>
    public class CommandBarLightweightControl : LightweightControl
    {
        /// <summary>
        /// The CommandManager for the CommandBarLightweightControl.
        /// </summary>
        private CommandManager commandManager;

        /// <summary>
        /// The left command bar container lightweight control.
        /// </summary>
        private CommandBarContainerLightweightControl leftContainer;

        /// <summary>
        /// The right command bar container lightweight control.
        /// </summary>
        private CommandBarContainerLightweightControl rightContainer;

        /// <summary>
        /// The command bar definition for this command bar lightweight control.
        /// </summary>
        private CommandBarDefinition commandBarDefinition;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The top bevel style.
        /// </summary>
        private BevelStyle topBevelStyle = BevelStyle.None;

        /// <summary>
        /// The bottom bevel style.
        /// </summary>
        private BevelStyle bottomBevelStyle = BevelStyle.None;

        /// <summary>
        /// The CommandManagerChanged event.
        /// </summary>
        public event EventHandler CommandManagerChanged;

        /// <summary>
        /// Initializes a new instance of the CommandBarLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();

            //	Common object initialization.
            this.InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLightweightControl class.
        /// </summary>
        public CommandBarLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

            //	Common object initialization.
            this.InitializeObject();
        }

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject() => this.AccessibleRole = AccessibleRole.ToolBar;

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

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
            this.leftContainer = new CommandBarContainerLightweightControl(this.components);
            this.rightContainer = new CommandBarContainerLightweightControl(this.components);
            ((ISupportInitialize)(this.leftContainer)).BeginInit();
            ((ISupportInitialize)(this.rightContainer)).BeginInit();
            ((ISupportInitialize)(this)).BeginInit();
            //
            // leftContainer
            //
            this.leftContainer.LightweightControlContainerControl = this;
            //
            // rightContainer
            //
            this.rightContainer.LightweightControlContainerControl = this;
            ((ISupportInitialize)(this.leftContainer)).EndInit();
            ((ISupportInitialize)(this.rightContainer)).EndInit();
            ((ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Gets or sets the CommandManager for the CommandBarLightweightControl.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public CommandManager CommandManager
        {
            get => this.commandManager;
            set
            {
                if (this.commandManager != null)
                {
                    this.commandManager.Changed -= new EventHandler(this.commandManager_Changed);
                }

                this.commandManager = value;

                if (this.commandManager != null)
                {
                    this.commandManager.Changed += new EventHandler(this.commandManager_Changed);
                }
            }
        }

        /// <summary>
        /// Gets or sets the top bevel style.
        /// </summary>
        [
            Category("Appearance"),
                DefaultValue(BevelStyle.None),
                Description("Specifies the top bevel style.")
        ]
        public BevelStyle TopBevelStyle
        {
            get => this.topBevelStyle;
            set
            {
                if (this.topBevelStyle != value)
                {
                    this.topBevelStyle = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets or sets the bottom bevel style.
        /// </summary>
        ///
        [
            Category("Appearance"),
                DefaultValue(BevelStyle.DoubleLine),
                Description("Specifies the bottom bevel style.")
        ]
        public BevelStyle BottomBevelStyle
        {
            get => this.bottomBevelStyle;
            set
            {
                if (this.bottomBevelStyle != value)
                {
                    this.bottomBevelStyle = value;
                    this.PerformLayout();
                }
            }
        }

        /// <summary>
        /// Gets the top layout margin.
        /// </summary>
        public virtual int TopLayoutMargin => -1;

        /// <summary>
        /// Gets the left layout margin.
        /// </summary>
        public virtual int LeftLayoutMargin => 2;

        /// <summary>
        /// Gets the bottom layout margin.
        /// </summary>
        public virtual int BottomLayoutMargin => -2;

        /// <summary>
        /// Gets the right layout margin.
        /// </summary>
        public virtual int RightLayoutMargin => 2;

        /// <summary>
        /// Gets the separator layout margin.
        /// </summary>
        public virtual int SeparatorLayoutMargin => 2;

        /// <summary>
        ///	Gets the top command bar color.
        /// </summary>
        public virtual Color TopColor => SystemColors.Control;

        /// <summary>
        ///	Gets the bottom command bar color.
        /// </summary>
        public virtual Color BottomColor => SystemColors.Control;

        /// <summary>
        ///	Gets the top bevel first line color.
        /// </summary>
        public virtual Color TopBevelFirstLineColor => !this.UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;

        /// <summary>
        ///	Gets the top bevel second line color.
        /// </summary>
        public virtual Color TopBevelSecondLineColor => !this.UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;

        /// <summary>
        ///	Gets the bottom bevel first line color.
        /// </summary>
        public virtual Color BottomBevelFirstLineColor => !this.UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;

        /// <summary>
        ///	Gets the bottom bevel second line color.
        /// </summary>
        public virtual Color BottomBevelSecondLineColor => !this.UseHighContrastMode ? SystemColors.Control : SystemColors.ControlLight;

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public virtual Color TextColor => !this.UseHighContrastMode ? SystemColors.WindowText : SystemColors.ControlText;

        /// <summary>
        ///	Gets the text color.
        /// </summary>
        public virtual Color DisabledTextColor => SystemColors.GrayText;

        /// <summary>
        /// Gets or sets the left container off set spacing.
        /// </summary>
        /// <value>The left container off set spacing.</value>
        public int LeftContainerOffSetSpacing
        {
            get => this.leftContainer.OffsetSpacing;
            set => this.leftContainer.OffsetSpacing = value;
        }

        /// <summary>
        /// Gets or sets the right container off set spacing.
        /// </summary>
        /// <value>The right container off set spacing.</value>
        public int RightContainerOffSetSpacing
        {
            get => this.rightContainer.OffsetSpacing;
            set => this.rightContainer.OffsetSpacing = value;
        }

        /// <summary>
        /// Gets a value indicating whether [use high contrast mode].
        /// </summary>
        /// <value><c>true</c> if [use high contrast mode]; otherwise, <c>false</c>.</value>
        protected bool UseHighContrastMode => SystemInformation.HighContrast;

        /// <summary>
        /// Gets or sets the command bar definition for this command bar lightweight control.
        /// </summary>
        [
            Category("Design"),
                DefaultValue(null),
                Description("Specifies the command bar definition for this command bar lightweight control.")
        ]
        public CommandBarDefinition CommandBarDefinition
        {
            get => this.commandBarDefinition;
            set
            {
                //	If we have a current command bar definition, clear all the current command bar
                //	lightweight controls.
                if (this.commandBarDefinition != null)
                {
                    this.leftContainer.LightweightControls.Clear();
                    this.rightContainer.LightweightControls.Clear();
                }

                //	Se the new command bar definition.
                this.commandBarDefinition = value;

                //	If we have a new command bar definition, add the new command bar lightweight
                //	controls.
                if (this.commandBarDefinition != null)
                {
                    //	Enumerate the left command bar entries.
                    foreach (var commandBarEntry in this.commandBarDefinition.LeftCommandBarEntries)
                    {
                        //	Get the lightweight control for this command bar entry.
                        var lightweightControl = commandBarEntry.GetLightweightControl(this, false);

                        //	Add the lightweight control to the left command bar container lightweight control.
                        if (lightweightControl != null)
                        {
                            this.leftContainer.LightweightControls.Add(lightweightControl);
                        }
                    }

                    //	Enumerate the right command bar entries.
                    foreach (var commandBarEntry in this.commandBarDefinition.RightCommandBarEntries)
                    {
                        //	Get the lightweight control for this command bar entry.
                        var lightweightControl = commandBarEntry.GetLightweightControl(this, true);

                        //	Add the lightweight control to the left command bar container lightweight control.
                        if (lightweightControl != null)
                        {
                            this.rightContainer.LightweightControls.Add(lightweightControl);
                        }
                    }
                }

                //	Layout the command bar lightweight controls.
                this.PerformLayout();
            }
        }

        /// <summary>
        /// Gets the minimum virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size MinimumVirtualSize =>
                //	The minimum virtual size (adjust this for expansion joint).
                new Size(0, this.DefaultVirtualSize.Height);

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size DefaultVirtualSize
        {
            get
            {
                //	If we are not initialized, just return.
                if (this.leftContainer == null && this.rightContainer == null)
                {
                    return Size.Empty;
                }

                //	Obtain the left container size and the right container size.
                var leftContainerSize = this.leftContainer.DefaultVirtualSize;
                var rightContainerSize = this.rightContainer.DefaultVirtualSize;

                //	If left and and right containers are empty, the command bar is empty, so it has no size.
                if (leftContainerSize == Size.Empty && rightContainerSize == Size.Empty)
                {
                    return Size.Empty;
                }

                //	If right container is empty, the command bar is sized for the left container.
                if (rightContainerSize == Size.Empty)
                {
                    return new Size(this.LeftLayoutMargin + leftContainerSize.Width + this.RightLayoutMargin,
                        this.TopLayoutMargin + leftContainerSize.Height + this.BottomLayoutMargin);
                }

                //	If left container is empty, the command bar is sized for the right container.
                if (leftContainerSize == Size.Empty)
                {
                    return new Size(this.LeftLayoutMargin + rightContainerSize.Width + this.RightLayoutMargin,
                        this.TopLayoutMargin + rightContainerSize.Height + this.BottomLayoutMargin);
                }

                //	Size the command bar for both left and right containers.
                return new Size(this.LeftLayoutMargin + leftContainerSize.Width + this.LeftLayoutMargin + rightContainerSize.Width + this.RightLayoutMargin,
                    this.TopLayoutMargin + Math.Max(leftContainerSize.Height, rightContainerSize.Height) + this.BottomLayoutMargin);
            }
        }

        public virtual Bitmap ContextMenuArrowBitmap => ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ContextMenuArrow.png");

        public virtual Bitmap ContextMenuArrowBitmapDisabled => ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ContextMenuArrowDisabled.png");

        public virtual ButtonMargins? GetButtonMargins(ButtonFeatures features, bool rightAligned)
        {
            switch (features)
            {
                case ButtonFeatures.Image:
                    return new ButtonMargins(5, 0, 0, 5, 0);
                case ButtonFeatures.Image | ButtonFeatures.Menu:
                    return new ButtonMargins(5, 0, 3, 5, 0);
                case ButtonFeatures.Text:
                    return new ButtonMargins(0, 8, 0, 8, 0);
                case ButtonFeatures.Text | ButtonFeatures.Menu:
                    return new ButtonMargins(0, 8, 5, 8, 0);
                case ButtonFeatures.Image | ButtonFeatures.Text:
                    return new ButtonMargins(8, 4, 0, 8, 0);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Raises the CommandManagerChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnCommandManagerChanged(EventArgs e) => CommandManagerChanged?.Invoke(null, e);

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Have the containers perform their layout.
            this.leftContainer.PerformLayout();
            this.rightContainer.PerformLayout();

            //	Calculate the layout height.
            var layoutHeight = this.VirtualHeight - (this.TopLayoutMargin + this.BottomLayoutMargin);

            //	Set the left container height to the layout hight, and have it layout.
            this.leftContainer.VirtualHeight = layoutHeight;

            //	Set the right container height to the layout hight, and have it layout.
            this.rightContainer.VirtualHeight = layoutHeight;

            //	Place the left container control (this is easy).
            this.leftContainer.VirtualLocation = new Point(this.LeftLayoutMargin, this.TopLayoutMargin);
            this.leftContainer.PerformLayout();

            //	Place the right container (either immediately following the left container, or
            //	right aligned, if possible).
            if (this.leftContainer.VirtualBounds.Right + this.LeftLayoutMargin + this.rightContainer.VirtualWidth + this.RightLayoutMargin > this.VirtualWidth)
            {
                this.rightContainer.VirtualLocation = new Point(this.leftContainer.VirtualBounds.Right + this.LeftLayoutMargin, this.TopLayoutMargin);
            }
            else
            {
                this.rightContainer.VirtualLocation = new Point(this.VirtualClientRectangle.Right - (this.rightContainer.VirtualWidth + this.RightLayoutMargin), this.TopLayoutMargin);
            }

            this.rightContainer.PerformLayout();

            this.RtlLayoutFixup(false);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = new BidiGraphics(e.Graphics, this.VirtualClientRectangle);
            //	Fill the background.
            if (this.TopColor == this.BottomColor)
            {
                using (var solidBrush = new SolidBrush(this.TopColor))
                {
                    g.FillRectangle(solidBrush, this.VirtualClientRectangle);
                }
            }
            else
            {
                using (var linearGradientBrush = new LinearGradientBrush(this.VirtualClientRectangle, this.TopColor, this.BottomColor, LinearGradientMode.Vertical))
                {
                    g.FillRectangle(linearGradientBrush, this.VirtualClientRectangle);
                }
            }

            //	Draw the first line of the top bevel, if we should.
            if (this.TopBevelStyle != BevelStyle.None)
            {
                using (var solidBrush = new SolidBrush(this.TopBevelFirstLineColor))
                {
                    g.FillRectangle(solidBrush, 0, 0, this.VirtualWidth, 1);
                }
            }

            if (this.TopBevelStyle == BevelStyle.DoubleLine)
            {
                using (var solidBrush = new SolidBrush(this.TopBevelSecondLineColor))
                {
                    g.FillRectangle(solidBrush, 0, 1, this.VirtualWidth, 1);
                }
            }

            //	Draw the first line of the bottom bevel.
            if (this.BottomBevelStyle == BevelStyle.DoubleLine)
            {
                using (var solidBrush = new SolidBrush(this.BottomBevelFirstLineColor))
                {
                    g.FillRectangle(solidBrush, 0, this.VirtualHeight - 2, this.VirtualWidth, 1);
                }

                using (var solidBrush = new SolidBrush(this.BottomBevelSecondLineColor))
                {
                    g.FillRectangle(solidBrush, 0, this.VirtualHeight - 1, this.VirtualWidth, 1);
                }
            }
            else if (this.BottomBevelStyle == BevelStyle.SingleLine)
            {
                using (var solidBrush = new SolidBrush(this.BottomBevelFirstLineColor))
                {
                    g.FillRectangle(solidBrush, 0, this.VirtualHeight - 1, this.VirtualWidth, 1);
                }
            }

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }

        /// <summary>
        /// commandManager_Changed event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void commandManager_Changed(object sender, EventArgs e)
        {
            //	Raise the CommandManagerChanged event.
            this.OnCommandManagerChanged(EventArgs.Empty);

            //	Layout.
            this.LightweightControlContainerControl.PerformLayout();
            this.Invalidate();
        }

        protected override void AddAccessibleControlsToList(ArrayList list)
        {
            this.AddCommandBarEntries(this.commandBarDefinition.LeftCommandBarEntries, list, false);
            this.AddCommandBarEntries(this.commandBarDefinition.RightCommandBarEntries, list, true);
        }
        private void AddCommandBarEntries(CommandBarEntryCollection entries, ArrayList list, bool rightAligned)
        {
            foreach (var entry in entries)
            {
                var control = entry.GetLightweightControl(this, rightAligned);
                list.Add(control);
            }
        }
    }
}
