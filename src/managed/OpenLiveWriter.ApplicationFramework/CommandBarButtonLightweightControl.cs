// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// CommandBar button lightweight control.
    /// Implements the <see cref="LightweightControl" />
    /// </summary>
    /// <seealso cref="LightweightControl" />
    public partial class CommandBarButtonLightweightControl : LightweightControl
    {
        /// <summary>
        /// The string format used to format text.
        /// </summary>
        private static readonly StringFormat stringFormat;

        /// <summary>
        /// The maximum text with of text on a button.
        /// </summary>
        private const int MAX_TEXT_WIDTH = 300;

        /// <summary>
        /// The top margin to leave around the command bar button bitmap.
        /// </summary>
        public const int TOP_MARGIN = 4;

        /// <summary>
        /// The bottom margin to leave around the command bar button image and text.
        /// </summary>
        public const int BOTTOM_MARGIN = 4;

        /// <summary>
        /// The drop down button width.
        /// </summary>
        private const int DROP_DOWN_BUTTON_WIDTH = 17;

        /// <summary>
        /// The context menu indicator width.
        /// </summary>
        private const int CONTEXT_MENU_INDICATOR_WIDTH = 12;

        /// <summary>
        /// The offset at which to paint the context menu arrow.
        /// </summary>
        private const int CONTEXT_MENU_ARROW_OFFSET = 3;

        /// <summary>
        /// Horizontal padding between provider buttons
        /// </summary>
        private const int PROVIDER_HORIZONTAL_PAD = 1;

        /// <summary>
        /// The context menu arrow bitmap.
        /// </summary>
        private readonly Bitmap contextMenuArrowBitmap;

        /// <summary>
        /// The disabled context menu arrow bitmap.
        /// </summary>
        private readonly Bitmap contextMenuArrowBitmapDisabled;

        /// <summary>
        /// The command bar lightweight control that this CommandBarButtonLightweightControl is
        /// associated with.
        /// </summary>
        private readonly CommandBarLightweightControl commandBarLightweightControl;

        /// <summary>
        /// The command identifier of the command that is associated with this
        /// CommandBarButtonLightweightControl.
        /// </summary>
        private readonly string commandIdentifier;

        /// <summary>
        /// The right aligned
        /// </summary>
        private readonly bool rightAligned;

        /// <summary>
        /// The command that is associated with this CommandBarButtonLightweightControl.
        /// </summary>
        private Command command;

        /// <summary>
        /// Used to avoid extra layouts and paints.
        /// </summary>
        private CommandState lastLayoutCommandState;

        /// <summary>
        /// A value indicating whether the mouse is inside the control.
        /// </summary>
        private bool mouseInside = false;

        /// <summary>
        /// The mouse inside context menu
        /// </summary>
        private bool mouseInsideContextMenu = false;

        /// <summary>
        /// A value indicating whether the button is pushed.  (Note that "pushed" applies to
        /// the button and not to the context menu.)
        /// </summary>
        private bool buttonPushed = false;

        /// <summary>
        /// A value indicating whether the context menu is showing.
        /// </summary>
        private bool contextMenuShowing;

        /// <summary>
        /// The r image
        /// </summary>
        private Rectangle rImage = Rectangle.Empty;

        /// <summary>
        /// The r text
        /// </summary>
        private Rectangle rText = Rectangle.Empty;

        /// <summary>
        /// The r arrow
        /// </summary>
        private Rectangle rArrow = Rectangle.Empty;

        /// <summary>
        /// Static initialization of the CommandBarButtonLightweightControl class.
        /// </summary>
        static CommandBarButtonLightweightControl() =>
            //	Initialize the string format.
            stringFormat = new StringFormat
            {
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBarButtonLightweightControl"/> class.
        /// </summary>
        /// <param name="commandBarLightweightControl">The command bar lightweight control.</param>
        /// <param name="commandIdentifier">The command identifier.</param>
        /// <param name="rightAligned">if set to <c>true</c> [right aligned].</param>
        public CommandBarButtonLightweightControl(CommandBarLightweightControl commandBarLightweightControl, string commandIdentifier, bool rightAligned)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
            this.InitializeObject();

            //	Set the command bar lightweight control.
            this.commandBarLightweightControl = commandBarLightweightControl;
            commandBarLightweightControl.CommandManagerChanged += new EventHandler(this.commandBarLightweightControl_CommandManagerChanged);

            //	Set the command identifier.
            this.commandIdentifier = commandIdentifier;

            this.rightAligned = rightAligned;

            this.contextMenuArrowBitmap = commandBarLightweightControl.ContextMenuArrowBitmap;
            this.contextMenuArrowBitmapDisabled = commandBarLightweightControl.ContextMenuArrowBitmapDisabled;

            //	Update the command.
            this.UpdateCommand();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Command = null;
                if (this.commandBarLightweightControl != null)
                {
                    this.commandBarLightweightControl.CommandManagerChanged -= new EventHandler(this.commandBarLightweightControl_CommandManagerChanged);
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
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            //
            // CommandBarButtonLightweightControl
            //
            this.Visible = false;
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject()
        {
            this.AccessibleRole = AccessibleRole.PushButton;
            this.AccessibleDefaultAction = "Press";
            this.TabStop = true;
        }

        /// <summary>
        /// Gets the margin left.
        /// </summary>
        /// <value>The margin left.</value>
        public int MarginLeft { get; private set; }

        /// <summary>
        /// Gets the margin right.
        /// </summary>
        /// <value>The margin right.</value>
        public int MarginRight { get; private set; }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required if there is no parent or no command.
            if (this.Parent == null || this.Command == null)
            {
                this.lastLayoutCommandState = null;

                this.VirtualSize = Size.Empty;
                return;
            }

            this.lastLayoutCommandState = new CommandState(this.Command, this.ImageSize);

            //	The button width and height.
            int buttonWidth = 0, buttonHeight = 0;
            this.MarginLeft = 0;
            this.MarginRight = 0;

            //	If the command bar button style is "System", then do the layout.
            switch (this.Command.CommandBarButtonStyle)
            {
                case CommandBarButtonStyle.System:
                    {
                        this.rImage = Rectangle.Empty;
                        this.rText = Rectangle.Empty;
                        this.rArrow = Rectangle.Empty;

                        buttonHeight = 24;

                        //	Obtain the font.
                        var font = ApplicationManager.ApplicationStyle.NormalApplicationFont;

                        var features = ButtonFeatures.None;

                        //	Adjust the button width and height for the command bar button bitmap, if there is one.
                        if (this.HasImage && !this.Command.SuppressCommandBarBitmap)
                        {
                            features |= ButtonFeatures.Image;
                            this.rImage.Size = this.ImageSize;
                        }

                        //	Adjust the button width and height for the command bar button text, if there is text.
                        if (!string.IsNullOrEmpty(this.Command.CommandBarButtonText))
                        {
                            features |= ButtonFeatures.Text;
                            using (var graphics = this.Parent.CreateGraphics())
                            {
                                this.rText.Size = TextRenderer.MeasureText(graphics, this.Command.CommandBarButtonText, font, Size.Empty, TextFormatFlags.NoPadding);
                            }
                        }

                        //	Enlarge the width for the drop-down button or context menu indicator, as needed.
                        if (this.DropDownContextMenuUserInterface)
                        {
                            features |= ButtonFeatures.SplitMenu;
                            this.rArrow.Size = this.contextMenuArrowBitmap.Size;
                        }
                        else if (this.ContextMenuUserInterface)
                        {
                            features |= ButtonFeatures.Menu;
                            this.rArrow.Size = this.contextMenuArrowBitmap.Size;
                        }

                        var margins = this.commandBarLightweightControl.GetButtonMargins(features, this.rightAligned);
                        if (margins == null)
                        {
                            Trace.Fail("Don't know how to layout these features: " + features + ", " + this.commandBarLightweightControl.GetType().Name);
                            this.VirtualSize = Size.Empty;
                            return;
                        }
                        var margin = margins.Value;
                        this.MarginRight = margin.RightMargin;

                        this.rImage.Y = Utility.CenterMinZero(this.rImage.Height, buttonHeight);
                        this.rText.Y = Utility.CenterMinZero(this.rText.Height, buttonHeight);
                        this.rArrow.Y = Utility.CenterMinZero(this.rArrow.Height, buttonHeight);

                        this.rImage.X = margin.LeftOfImage;
                        this.rText.X = this.rImage.Right + margin.LeftOfText;
                        this.rArrow.X = this.rText.Right + margin.LeftOfArrow;
                        buttonWidth = this.rArrow.Right + margin.RightPadding;
                        break;
                    }

                case CommandBarButtonStyle.Bitmap:
                    buttonWidth += this.HorizontalMargin + this.Command.CommandBarButtonBitmapEnabled.Width + this.HorizontalMargin;
                    buttonHeight += TOP_MARGIN + this.Command.CommandBarButtonBitmapEnabled.Height + BOTTOM_MARGIN;
                    break;
                case CommandBarButtonStyle.Provider:
                    if (this.HasImage)
                    {
                        buttonHeight = ProviderButtonFaceLeftEnabled.Height;
                        if (this.DropDownContextMenuUserInterface)
                        {
                            buttonWidth = ProviderButtonFaceLeftEnabled.Width + ProviderButtonFaceRightEnabled.Width;
                        }
                        else if (this.ContextMenuUserInterface)
                        {
                            buttonWidth = ProviderButtonFaceDropDownEnabled.Width;
                        }
                        else
                        {
                            buttonWidth = ProviderButtonFaceEnabled.Width;
                        }
                        buttonWidth += (2 * PROVIDER_HORIZONTAL_PAD);
                    }

                    break;
            }

            //	Set the new virtual size.
            this.VirtualSize = new Size(buttonWidth, buttonHeight + 2 * this.VerticalPadding);
        }

        /// <summary>
        /// Gets the horizontal margin.
        /// </summary>
        /// <value>The horizontal margin.</value>
        private int HorizontalMargin => this.IsLargeButton ? 6 : 4;

        /// <summary>
        /// Gets the vertical padding.
        /// </summary>
        /// <value>The vertical padding.</value>
        private int VerticalPadding => 0;

        /// <summary>
        /// Snaps the height of the button.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <returns>System.Int32.</returns>
        private int SnapButtonHeight(int height) =>
            // large buttons always occupy 42 pixels
            this.IsLargeButton ? SystemButtonHelper.LARGE_BUTTON_TOTAL_SIZE : TOP_MARGIN + height + BOTTOM_MARGIN;

        /// <summary>
        /// Gets a value indicating whether this instance is large button.
        /// </summary>
        /// <value><c>true</c> if this instance is large button; otherwise, <c>false</c>.</value>
        private bool IsLargeButton => this.ImageSize.Height > SystemButtonHelper.SMALL_BUTTON_IMAGE_SIZE;

        /// <summary>
        /// Gets a value indicating whether this instance has image.
        /// </summary>
        /// <value><c>true</c> if this instance has image; otherwise, <c>false</c>.</value>
        private bool HasImage
        {
            get
            {
                var size = this.ImageSize;
                return size.Width > 0 && size.Height > 0;
            }
        }

        /// <summary>
        /// Gets the size of the image.
        /// </summary>
        /// <value>The size of the image.</value>
        private Size ImageSize =>
            this.Command is ICustomButtonBitmapPaint
                ? new Size(((ICustomButtonBitmapPaint)this.Command).Width, ((ICustomButtonBitmapPaint)this.Command).Height)
                : this.Command.CommandBarButtonBitmapEnabled == null ? new Size(0, 0) : this.Command.CommandBarButtonBitmapEnabled.Size;

        /// <summary>
        /// Raises the MouseEnter event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseEnter(e);

            //	Ignore the event if input events are disabled.
            if (this.InputEventsDisabled)
            {
                return;
            }

            //	Set the MouseInside property.
            this.MouseInside = true;
        }

        /// <summary>
        /// Raises the MouseDown event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            //	Ignore the event if input events are disabled.
            if (this.InputEventsDisabled)
            {
                return;
            }

            //	Process left mouse button.
            if (e.Button == MouseButtons.Left)
            {
                //	If the drop down context menu user interface is being displayed, hit-test and
                //	do the dropdown if the mouse was pressed in the right area.  Otherwise, if the
                if (this.DropDownContextMenuUserInterface)
                {
                    if (this.InContextMenuArrow(e))
                    {
                        this.DoShowContextMenu();
                    }
                    else
                    {
                        this.ButtonPushed = true;
                    }
                }
                else if (this.ContextMenuUserInterface)
                {
                    this.DoShowContextMenu();
                }
                else
                {
                    this.ButtonPushed = true;
                }
            }

            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseDown(e);
        }

        /// <summary>
        /// Ins the context menu arrow.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool InContextMenuArrow(MouseEventArgs e)
        {
            if (!this.DropDownContextMenuUserInterface)
            {
                return false;
            }

            var x = e.X;

            if (BidiHelper.IsRightToLeft)
            {
                x = this.VirtualWidth - x;
            }

            switch (this.Command.CommandBarButtonStyle)
            {
                case CommandBarButtonStyle.System:
                    return x >= this.VirtualWidth - DROP_DOWN_BUTTON_WIDTH;
                case CommandBarButtonStyle.Provider:
                    return x >= this.VirtualWidth - ProviderButtonFaceRightEnabled.Width;
                default:
                    return false;
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

            //	Ignore the event if input events are disabled.
            if (this.InputEventsDisabled)
            {
                return;
            }

            //	Clear the ButtonPushed and MouseInside properties.
            this.ButtonPushed = this.MouseInside = this.MouseInsideContextMenu = false;
        }

        /// <summary>
        /// Raises the MouseMove event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseMove(e);

            //	Ignore the event if input events are disabled.
            if (this.InputEventsDisabled)
            {
                return;
            }

            this.MouseInsideContextMenu = this.InContextMenuArrow(e);

            //	Update the state of the MouseInside property if the LeftMouseButtonDown property
            //	is true.
            if (this.ButtonPushed)
            {
                this.MouseInside = this.VirtualClientRectangle.Contains(e.X, e.Y);
            }
        }

        /// <summary>
        /// Raises the MouseUp event.
        /// </summary>
        /// <param name="e">A MouseEventArgs that contains the event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnMouseUp(e);

            //	Ignore the event if input events are disabled.
            if (this.InputEventsDisabled)
            {
                return;
            }

            //	If the button is the left button, set the LeftMouseButtonDown property.
            if (e.Button == MouseButtons.Left && this.ButtonPushed)
            {
                //	Note that the button is not pushed.
                this.ButtonPushed = false;

                //	If the mouse was inside the lightweight control, execute the event.
                if (this.VirtualClientRectangle.Contains(e.X, e.Y))
                {
                    //	Execute the event.
                    if (this.Command != null && this.Command.On && this.Command.Enabled)
                    {
                        this.Command.PerformExecute();
                    }
                }
            }
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="args">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected override void OnPaint(PaintEventArgs args)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(args);

            // Fix bug 602576: The 'Remove Effects' button is truncated in right-to-left builds
            if (BidiHelper.IsRightToLeft)
            {
                args.Graphics.ResetClip();
            }

            // Take int account vertical padding
            var rect = new Rectangle(this.VirtualClientRectangle.X, this.VirtualClientRectangle.Y + this.VerticalPadding, this.VirtualClientRectangle.Width, this.VirtualClientRectangle.Height);

            var g = new BidiGraphics(args.Graphics, rect);

            //	No painting required if there is no parent or no command.
            if (this.Parent == null || this.Command == null)
            {
                return;
            }

            //	Determine the draw state of the button.
            DrawState drawState;
            if (this.Command.Enabled)
            {
                if (this.Command.Latched || (this.ButtonPushed && this.MouseInside) || (this.ContextMenuUserInterface && this.ContextMenuShowing))
                {
                    drawState = DrawState.Pushed;
                }
                else if (this.MouseInside || (this.DropDownContextMenuUserInterface && this.ContextMenuShowing))
                {
                    drawState = DrawState.Selected;
                }
                else
                {
                    drawState = DrawState.Enabled;
                }
            }
            else
            {
                drawState = DrawState.Disabled;
            }

            //	Draw the button.
            switch (this.Command.CommandBarButtonStyle)
            {
                case CommandBarButtonStyle.System:
                    {
                        var imageSize = this.ImageSize;

                        // is this a large button?
                        var isLargeButton = false;
                        if (imageSize.Height > SystemButtonHelper.SMALL_BUTTON_IMAGE_SIZE)
                        {
                            isLargeButton = true;
                        }

                        if (this.Command is ICustomButtonBitmapPaint)
                        {
                            switch (drawState)
                            {
                                case DrawState.Selected:
                                    SystemButtonHelper.DrawSystemButtonFace(
                                        g,
                                        this.DropDownContextMenuUserInterface,
                                        this.contextMenuShowing,
                                        this.VirtualClientRectangle,
                                        isLargeButton);
                                    break;
                                case DrawState.Pushed:
                                    SystemButtonHelper.DrawSystemButtonFacePushed(
                                        g,
                                        this.DropDownContextMenuUserInterface,
                                        this.VirtualClientRectangle,
                                        isLargeButton);
                                    break;
                            }

                            ((ICustomButtonBitmapPaint)this.Command).Paint(g, this.rImage, drawState);
                        }
                        else
                        {
                            Bitmap buttonBitmap = null;
                            switch (drawState)
                            {
                                case DrawState.Disabled:
                                    buttonBitmap = this.Command.CommandBarButtonBitmapDisabled;
                                    break;
                                case DrawState.Enabled:
                                    buttonBitmap = this.Command.CommandBarButtonBitmapEnabled;
                                    break;
                                case DrawState.Selected:
                                    SystemButtonHelper.DrawSystemButtonFace(
                                        g,
                                        this.DropDownContextMenuUserInterface,
                                        this.contextMenuShowing,
                                        this.VirtualClientRectangle,
                                        isLargeButton);
                                    buttonBitmap = this.Command.CommandBarButtonBitmapSelected;
                                    break;
                                case DrawState.Pushed:
                                    SystemButtonHelper.DrawSystemButtonFacePushed(
                                        g,
                                        this.DropDownContextMenuUserInterface,
                                        this.VirtualClientRectangle,
                                        isLargeButton);
                                    buttonBitmap = this.Command.CommandBarButtonBitmapSelected;
                                    break;
                            }

                            //	Draw the button bitmap.
                            if (buttonBitmap != null && !this.Command.SuppressCommandBarBitmap)
                            {
                                if (SystemInformation.HighContrast)
                                {
                                    //apply a high contrast image matrix
                                    var ia = new ImageAttributes();
                                    ia.SetColorMatrix(HighContrastColorMatrix);
                                    var cm = new ColorMap
                                    {
                                        OldColor = Color.White,
                                        NewColor = SystemColors.Control
                                    };
                                    ia.SetRemapTable(new ColorMap[] { cm });
                                    g.DrawImage(false, buttonBitmap, this.rImage, 0, 0, buttonBitmap.Width, buttonBitmap.Height, GraphicsUnit.Pixel, ia);
                                }
                                else
                                {
                                    g.DrawImage(false, buttonBitmap, this.rImage);
                                }
                            }
                        }

                        //	Draw the text.
                        if (this.Command.CommandBarButtonText != null && this.Command.CommandBarButtonText.Length != 0)
                        {
                            var textColor = drawState == DrawState.Disabled
                                ? this.commandBarLightweightControl.DisabledTextColor
                                : this.commandBarLightweightControl.TextColor;

                            g.DrawText(
                                this.Command.CommandBarButtonText,
                                ApplicationManager.ApplicationStyle.NormalApplicationFont,
                                this.rText,
                                textColor,
                                TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.NoPadding | TextFormatFlags.NoClipping);
                        }

                        //	Draw the context menu arrow, if needed.
                        if (this.DropDownContextMenuUserInterface || this.ContextMenuUserInterface)
                        {
                            var contextMenuArrowBitmapToDraw = drawState == DrawState.Disabled
                                ? this.contextMenuArrowBitmapDisabled
                                : this.contextMenuArrowBitmap;

                            _ = isLargeButton ? SystemButtonHelper.LARGE_BUTTON_TOTAL_SIZE : this.VirtualHeight;
                            g.DrawImage(false, contextMenuArrowBitmapToDraw, this.rArrow);
                        }

                        break;
                    }

                case CommandBarButtonStyle.Bitmap:
                    switch (drawState)
                    {
                        case DrawState.Disabled:
                            g.DrawImage(true, this.Command.CommandBarButtonBitmapDisabled, this.HorizontalMargin, TOP_MARGIN);
                            break;
                        case DrawState.Enabled:
                            g.DrawImage(true, this.Command.CommandBarButtonBitmapEnabled, this.HorizontalMargin, TOP_MARGIN);
                            break;
                        case DrawState.Selected:
                            g.DrawImage(true, this.Command.CommandBarButtonBitmapSelected, this.HorizontalMargin, TOP_MARGIN);
                            break;
                        case DrawState.Pushed:
                            g.DrawImage(true, this.Command.CommandBarButtonBitmapPushed, this.HorizontalMargin, TOP_MARGIN);
                            break;
                    }

                    break;
                case CommandBarButtonStyle.Provider:
                    this.DrawProviderButtonFace(g, drawState);
                    this.DrawProviderButton(g, drawState);
                    break;
            }

            if (this.Focused)
            {
                g.DrawFocusRectangle(new Rectangle(0, 0, this.VirtualWidth, this.VirtualHeight), this.Parent.ForeColor, this.Parent.BackColor);
            }
        }

        /// <summary>
        /// Draws the provider button face.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="drawState">State of the draw.</param>
        private void DrawProviderButtonFace(BidiGraphics g, DrawState drawState)
        {
            if (this.DropDownContextMenuUserInterface)
            {
                switch (drawState)
                {
                    case DrawState.Selected:
                        g.DrawImage(true, ProviderButtonFaceLeftEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                        if (this.ContextMenuShowing)
                        {
                            g.DrawImage(true, ProviderButtonFaceRightPressed, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                        }
                        else
                        {
                            g.DrawImage(true, ProviderButtonFaceRightEnabled, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                        }

                        break;
                    case DrawState.Pushed:
                        if (this.ContextMenuShowing)
                        {
                            g.DrawImage(true, ProviderButtonFaceLeftEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                            g.DrawImage(true, ProviderButtonFaceRightPressed, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                        }
                        else
                        {
                            g.DrawImage(true, ProviderButtonFaceLeftPressed, PROVIDER_HORIZONTAL_PAD, 0);
                            g.DrawImage(true, ProviderButtonFaceRightPressed, PROVIDER_HORIZONTAL_PAD + ProviderButtonFaceLeftEnabled.Width, 0);
                        }

                        break;
                }
            }
            else if (this.ContextMenuUserInterface)
            {
                switch (drawState)
                {
                    case DrawState.Selected:
                        g.DrawImage(true, ProviderButtonFaceDropDownEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                        break;
                    case DrawState.Pushed:
                        g.DrawImage(true, ProviderButtonFaceDropDownPressed, PROVIDER_HORIZONTAL_PAD, 0);
                        break;
                }
            }
            else
            {
                switch (drawState)
                {
                    case DrawState.Selected:
                        g.DrawImage(true, ProviderButtonFaceEnabled, PROVIDER_HORIZONTAL_PAD, 0);
                        break;
                    case DrawState.Pushed:
                        g.DrawImage(true, ProviderButtonFacePressed, PROVIDER_HORIZONTAL_PAD, 0);
                        break;
                }
            }
        }

        /// <summary>
        /// Draws the provider button.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="drawState">State of the draw.</param>
        private void DrawProviderButton(BidiGraphics g, DrawState drawState)
        {
            // determine the bitmap to draw
            Bitmap buttonBitmap;
            switch (drawState)
            {
                case DrawState.Disabled:
                    buttonBitmap = this.Command.CommandBarButtonBitmapDisabled;
                    break;
                case DrawState.Enabled:
                    buttonBitmap = this.Command.CommandBarButtonBitmapEnabled;
                    break;
                case DrawState.Pushed:
                    buttonBitmap = this.Command.CommandBarButtonBitmapPushed;
                    break;
                case DrawState.Selected:
                    buttonBitmap = this.Command.CommandBarButtonBitmapSelected;
                    break;
                default:
                    Debug.Fail("Unexpected DrawState!");
                    buttonBitmap = this.Command.CommandBarButtonBitmapEnabled;
                    break;
            }

            // draw the button
            Rectangle centerButtonInRect;
            if (this.DropDownContextMenuUserInterface)
            {
                centerButtonInRect = new Rectangle(PROVIDER_HORIZONTAL_PAD, 0, ProviderButtonFaceLeftEnabled.Width, ProviderButtonFaceRightEnabled.Height);
            }
            else if (this.ContextMenuUserInterface)
            {
                centerButtonInRect = new Rectangle(PROVIDER_HORIZONTAL_PAD, 0, ProviderButtonFaceEnabled.Width, ProviderButtonFaceEnabled.Height);
            }
            else
            {
                centerButtonInRect = new Rectangle(PROVIDER_HORIZONTAL_PAD, 0, ProviderButtonFaceEnabled.Width, ProviderButtonFaceEnabled.Height);
            }

            var left = centerButtonInRect.Left + (centerButtonInRect.Width / 2) - (buttonBitmap.Width / 2) + (drawState == DrawState.Pushed ? 1 : 0);
            var top = centerButtonInRect.Top + (centerButtonInRect.Height / 2) - (buttonBitmap.Height / 2) + (drawState == DrawState.Pushed ? 1 : 0);
            g.DrawImage(false, buttonBitmap, left, top);

            // draw the accompanying arrow if necessary
            var centerArrowInRect = Rectangle.Empty;
            if (this.DropDownContextMenuUserInterface)
            {
                centerArrowInRect = new Rectangle(centerButtonInRect.Width, 0, ProviderButtonFaceRightEnabled.Width, ProviderButtonFaceRightEnabled.Height);
            }
            else if (this.ContextMenuUserInterface)
            {
                centerArrowInRect = new Rectangle(centerButtonInRect.Width - 1, 0, ProviderButtonFaceDropDownEnabled.Width - ProviderButtonFaceEnabled.Width, ProviderButtonFaceDropDownEnabled.Height);
            }

            if (centerArrowInRect != Rectangle.Empty)
            {
                var arrowLeft = centerArrowInRect.Left + (centerArrowInRect.Width / 2) - (ProviderDownArrow.Width / 2) + (this.ContextMenuShowing ? 1 : 0);
                var arrowTop = centerArrowInRect.Top + (centerArrowInRect.Height / 2) - (ProviderDownArrow.Height / 2) + (this.ContextMenuShowing ? 1 : 0);
                g.DrawImage(true, ProviderDownArrow, arrowLeft, arrowTop);
            }
        }

        /// <summary>
        /// Gets or sets command that is associated with this CommandBarButtonLightweightControl.
        /// </summary>
        /// <value>The command.</value>
        private Command Command
        {
            get => this.command;
            set
            {
                //	If the command is changing, change it.
                if (this.command != value)
                {
                    //	Remove event handlers.
                    if (this.command != null)
                    {
                        this.command.ShowCommandBarButtonContextMenu -= new EventHandler(this.command_ShowCommandBarButtonContextMenu);
                        this.command.StateChanged -= new EventHandler(this.command_StateChanged);
                        this.command.CommandBarButtonTextChanged -= new EventHandler(this.command_StateChanged);
                        this.command.VisibleOnCommandBarChanged -= new EventHandler(this.command_StateChanged);
                        this.command.CommandBarButtonContextMenuDefinitionChanged -= new EventHandler(this.command_CommandBarButtonContextMenuDefinitionChanged);
                        this.Visible = false;
                    }

                    //	Set the new command.
                    this.command = value;

                    //	Add event handlers.
                    if (this.command != null)
                    {
                        this.command.ShowCommandBarButtonContextMenu += new EventHandler(this.command_ShowCommandBarButtonContextMenu);
                        this.command.StateChanged += new EventHandler(this.command_StateChanged);
                        this.command.CommandBarButtonTextChanged += new EventHandler(this.command_StateChanged);
                        this.command.VisibleOnCommandBarChanged += new EventHandler(this.command_StateChanged);
                        this.Visible = this.command.On && this.command.VisibleOnCommandBar;
                        this.command.CommandBarButtonContextMenuDefinitionChanged += new EventHandler(this.command_CommandBarButtonContextMenuDefinitionChanged);
                    }

                    this.SetAccessibleInfo();
                }
            }
        }

        /// <summary>
        /// Handles the CommandBarButtonContextMenuDefinitionChanged event of the command control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void command_CommandBarButtonContextMenuDefinitionChanged(object sender, EventArgs e) => this.SetAccessibleInfo();

        /// <summary>
        /// Sets the accessible information.
        /// </summary>
        public void SetAccessibleInfo()
        {
            this.AccessibleRole =
                this.ContextMenuUserInterface || this.DropDownContextMenuUserInterface
                    ? AccessibleRole.ButtonMenu
                    : AccessibleRole.PushButton;

            //update the accessibility name of this control
            this.AccessibleName = this.command?.Text;

            this.AccessibleKeyboardShortcut =
                this.command != null
                && this.command.Shortcut != Shortcut.None
                    ? KeyboardHelper.FormatShortcutString(this.command.Shortcut)
                    : null;
        }

        /// <summary>
        /// Returns a value indicating whether input events are disabled.
        /// </summary>
        /// <value><c>true</c> if [input events disabled]; otherwise, <c>false</c>.</value>
        private bool InputEventsDisabled =>
                //	Disable input events if there is no command, or if there is a command but it is
                //	not active and enabled.
                this.Command == null || !(this.Command.On && this.Command.Enabled);

        /// <summary>
        /// Gets the tool tip text.
        /// </summary>
        /// <value>The tool tip text.</value>
        private string ToolTipText => this.Command == null || !this.Command.On ? null : this.Command.Text;

        /// <summary>
        /// Gets or sets a value indicating whether the mouse is inside the control.
        /// </summary>
        /// <value><c>true</c> if [mouse inside]; otherwise, <c>false</c>.</value>
        private bool MouseInside
        {
            get => this.mouseInside;
            set
            {
                //	Ensure that the property is actually changing.
                if (this.mouseInside != value)
                {
                    //	Update the value.
                    this.mouseInside = value;

                    this.Invalidate();

                    //	Update the tool tip text, if the parent implements IToolTipDisplay.  Note
                    //	that we set the tool tip text when the parent implements IToolTipDisplay so
                    //	an older tool tip will be erased if it was being displayed.
                    if (this.Parent is IToolTipDisplay toolTipDisplay)
                    {
                        if (this.mouseInside && !this.ButtonPushed)
                        {
                            toolTipDisplay.SetToolTip(this.ToolTipText);
                        }
                        else
                        {
                            toolTipDisplay.SetToolTip(null);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [mouse inside context menu].
        /// </summary>
        /// <value><c>true</c> if [mouse inside context menu]; otherwise, <c>false</c>.</value>
        private bool MouseInsideContextMenu
        {
            get => this.mouseInsideContextMenu;
            set
            {
                if (this.mouseInsideContextMenu != value)
                {
                    this.mouseInsideContextMenu = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is pushed.
        /// </summary>
        /// <value><c>true</c> if [button pushed]; otherwise, <c>false</c>.</value>
        private bool ButtonPushed
        {
            get => this.buttonPushed;
            set
            {
                if (this.buttonPushed != value)
                {
                    this.buttonPushed = value;
                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the dropdown context menu user interface should be displayed.
        /// </summary>
        /// <value><c>true</c> if [drop down context menu user interface]; otherwise, <c>false</c>.</value>
        public bool DropDownContextMenuUserInterface => this.Command != null &&
                        (this.Command.CommandBarButtonContextMenu != null
                            || this.Command.CommandBarButtonContextMenuDefinition != null
                            || this.Command.CommandBarButtonContextMenuControlHandler != null
                            || this.Command.CommandBarButtonContextMenuHandler != null) &&
                        this.Command.CommandBarButtonContextMenuDropDown;

        /// <summary>
        /// Gets a value indicating whether the context menu user interface should be displayed.
        /// </summary>
        /// <value><c>true</c> if [context menu user interface]; otherwise, <c>false</c>.</value>
        private bool ContextMenuUserInterface => this.Command != null &&
                        (this.Command.CommandBarButtonContextMenu != null
                            || this.Command.CommandBarButtonContextMenuDefinition != null
                            || this.Command.CommandBarButtonContextMenuControlHandler != null
                            || this.Command.CommandBarButtonContextMenuHandler != null) &&
                        !this.Command.CommandBarButtonContextMenuDropDown;

        /// <summary>
        /// Gets or sets a value indicating whether the context menu is showing.
        /// </summary>
        /// <value><c>true</c> if [context menu showing]; otherwise, <c>false</c>.</value>
        private bool ContextMenuShowing
        {
            get => this.contextMenuShowing;
            set
            {
                if (this.contextMenuShowing != value)
                {
                    this.contextMenuShowing = value;
                    this.Invalidate();
                    this.Update();
                }
            }
        }

        /// <summary>
        /// Gets the provider button face enabled.
        /// </summary>
        /// <value>The provider button face enabled.</value>
        private static Bitmap ProviderButtonFaceEnabled => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceEnabled.png");

        /// <summary>
        /// Gets the provider button face pressed.
        /// </summary>
        /// <value>The provider button face pressed.</value>
        private static Bitmap ProviderButtonFacePressed => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFacePressed.png");

        /// <summary>
        /// Gets the provider button face drop down enabled.
        /// </summary>
        /// <value>The provider button face drop down enabled.</value>
        private static Bitmap ProviderButtonFaceDropDownEnabled => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceDropDownEnabled.png");

        /// <summary>
        /// Gets the provider button face drop down pressed.
        /// </summary>
        /// <value>The provider button face drop down pressed.</value>
        private static Bitmap ProviderButtonFaceDropDownPressed => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceDropDownPressed.png");

        /// <summary>
        /// Gets the provider button face left enabled.
        /// </summary>
        /// <value>The provider button face left enabled.</value>
        private static Bitmap ProviderButtonFaceLeftEnabled => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceLeftEnabled.png");

        /// <summary>
        /// Gets the provider button face left pressed.
        /// </summary>
        /// <value>The provider button face left pressed.</value>
        private static Bitmap ProviderButtonFaceLeftPressed => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceLeftPressed.png");

        /// <summary>
        /// Gets the provider button face right enabled.
        /// </summary>
        /// <value>The provider button face right enabled.</value>
        private static Bitmap ProviderButtonFaceRightEnabled => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceRightEnabled.png");

        /// <summary>
        /// Gets the provider button face right pressed.
        /// </summary>
        /// <value>The provider button face right pressed.</value>
        private static Bitmap ProviderButtonFaceRightPressed => ResourceHelper.LoadAssemblyResourceBitmap(
            "Images.CommandBar.ProviderButtonFaceRightPressed.png");

        /// <summary>
        /// Gets the provider down arrow.
        /// </summary>
        /// <value>The provider down arrow.</value>
        private static Bitmap ProviderDownArrow => ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.ProviderDownArrow.png");

        /// <summary>
        /// Updates the command.
        /// </summary>
        private void UpdateCommand()
        {
            //	Locate the command.
            var updateCommand = this.commandBarLightweightControl.CommandManager.Get(this.commandIdentifier);

            //	Set the command.
            this.Command = updateCommand;
        }

        /// <summary>
        /// Shows the context menu.  This is a modal operation -- it does not return until the user
        /// dismisses the context menu.
        /// </summary>
        private void DoShowContextMenu()
        {
            if (this.Command != null)
            {
                // calculate point to show context menu at as well as alternative point
                // in the case where the menu might go off the right edge of the screen
                var menuLocation = this.VirtualClientPointToScreen(new Point(0, this.VirtualHeight));
                var alternativeLocation = this.VirtualClientPointToScreen(new Point(this.VirtualWidth, this.VirtualHeight)).X;

                if (this.Command.CommandBarButtonContextMenuDefinition != null)
                {
                    //	Note that the context menu is showing.
                    this.StartShowContextMenu();

                    //	Show the context menu.
                    var command = CommandContextMenu.ShowModal(this.commandBarLightweightControl.CommandManager, this.Parent, menuLocation, alternativeLocation, this.Command.CommandBarButtonContextMenuDefinition);

                    // cleanup state/ui
                    this.EndShowContextMenu();

                    //	If a command was selected, execute it.
                    if (command != null)
                    {
                        command.PerformExecute();
                    }
                }
                else if (this.Command.CommandBarButtonContextMenuControlHandler != null)
                {
                    //	Note that the context menu is showing.
                    this.StartShowContextMenu();

                    // create the mini-form
                    var miniForm = new CommandContextMenuMiniForm(Win32WindowImpl.ForegroundWin32Window, this.Command);

                    miniForm.Location = PositionMenu(menuLocation, alternativeLocation, miniForm.Size);
                    miniForm.Closed += new EventHandler(this.miniForm_Closed);
                    miniForm.Show();
                }
                else if (this.Command.CommandBarButtonContextMenuHandler != null)
                {
                    this.StartShowContextMenu();
                    this.Command.CommandBarButtonContextMenuHandler(this.Parent, menuLocation, alternativeLocation, new EndShowContextMenuDisposable(this));
                }
            }

        }

        /// <summary>
        /// Positions the menu.
        /// </summary>
        /// <param name="menuLocation">The menu location.</param>
        /// <param name="alternativeLocation">The alternative location.</param>
        /// <param name="menuSize">Size of the menu.</param>
        /// <returns>Point.</returns>
        public static Point PositionMenu(Point menuLocation, int alternativeLocation, Size menuSize)
        {
            if (!BidiHelper.IsRightToLeft)
            {
                var top = menuLocation.Y;
                var left = menuLocation.X;
                if (!Screen.FromPoint(new Point(alternativeLocation, menuLocation.Y)).Bounds.Contains(new Point(left + menuSize.Width, top)))
                {
                    left = alternativeLocation - menuSize.Width;
                }

                return new Point(left, top);
            }
            else
            {
                var top = menuLocation.Y;
                var left = alternativeLocation - menuSize.Width;
                if (!Screen.FromPoint(new Point(alternativeLocation, menuLocation.Y)).Bounds.Contains(new Point(left, top)))
                {
                    left = menuLocation.X;
                }

                return new Point(left, top);
            }
        }

        // cleanup when the mini form closes
        /// <summary>
        /// Handles the Closed event of the miniForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void miniForm_Closed(object sender, EventArgs e)
        {
            // cleanup form
            var miniForm = sender as CommandContextMenuMiniForm;
            miniForm.Closed -= new EventHandler(this.miniForm_Closed);
            miniForm.Dispose();

            // cleanup ui
            this.EndShowContextMenu();
        }

        /// <summary>
        /// Initialize state/UI for context menu
        /// </summary>
        private void StartShowContextMenu() => this.ContextMenuShowing = true;

        /// <summary>
        /// Make sure the state and UI is cleaned up after showing the context menu
        /// </summary>
        private void EndShowContextMenu()
        {
            //	Note that the context menu is not showing and ensure that the mouse is not inside.
            this.ContextMenuShowing = false;
            this.MouseInside = false;

            // if requested, invalidate the parent so that the command bar repaints
            // correctly (this is necessary for the IE ToolBand which won't paint
            // its background correctly unless the entire control is invalidated)
            if (this.Command != null)
            {
                if (this.Command.CommandBarButtonContextMenuInvalidateParent)
                {
                    this.Parent.Invalidate(true);
                }
            }
        }

        /// <summary>
        /// Returns a color matrix that will draw buttons in a high-contrast-friendly mode.
        /// </summary>
        /// <value>The high contrast color matrix.</value>
        private static ColorMatrix HighContrastColorMatrix
        {
            get
            {
                if (highContrastColorMatrix == null)
                {
                    highContrastColorMatrix = ImageHelper.GetHighContrastImageMatrix();
                }

                return highContrastColorMatrix;
            }
        }
        /// <summary>
        /// The high contrast color matrix
        /// </summary>
        private static ColorMatrix highContrastColorMatrix;

        /// <summary>
        /// commandBarLightweightControl_CommandManagerChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void commandBarLightweightControl_CommandManagerChanged(object sender, EventArgs e) => this.UpdateCommand();

        /// <summary>
        /// command_CommandBarButtonTextChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_CommandBarButtonTextChanged(object sender, EventArgs e)
        {
            this.commandBarLightweightControl.PerformLayout();
            this.Parent.Invalidate();
        }

        /// <summary>
        /// command_ShowCommandBarButtonContextMenu event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_ShowCommandBarButtonContextMenu(object sender, EventArgs e) => this.DoShowContextMenu();

        /// <summary>
        /// command_StateChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void command_StateChanged(object sender, EventArgs e)
        {
            this.AccessibleName = this.command != null ? this.command.Text : null;
            this.Visible = this.command.On && this.command.VisibleOnCommandBar;

            if (this.lastLayoutCommandState == null || this.lastLayoutCommandState.NeedsLayout(this.Command, this.ImageSize))
            {
                this.commandBarLightweightControl.PerformLayout();
            }

            this.Parent.Invalidate();
        }

        /// <summary>
        /// Does the default action.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool DoDefaultAction()
        {
            if (this.ContextMenuUserInterface || this.DropDownContextMenuUserInterface)
            {
                this.DoShowContextMenu();
                return true;
            }

            //	Execute the event.
            if (this.Command != null && this.Command.On && this.Command.Enabled)
            {
                this.Command.PerformExecute();
            }

            return true;
        }

        /// <summary>
        /// Focuses this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Focus() => base.Focus();

        /// <summary>
        /// Unfocuses this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public override bool Unfocus() => base.Unfocus();
    }
}
