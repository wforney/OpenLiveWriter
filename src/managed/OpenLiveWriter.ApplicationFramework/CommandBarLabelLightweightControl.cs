// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// CommandBar label lightweight control.
    /// </summary>
    public class CommandBarLabelLightweightControl : LightweightControl
    {
        /// <summary>
        ///	The maximum text with of text on a label.
        /// </summary>
        private const int MAX_TEXT_WIDTH = 300;

        /// <summary>
        /// The top margin to leave around the command bar label bitmap.
        /// </summary>
        private const int TOP_MARGIN = 4;

        /// <summary>
        /// The left margin to leave around the command bar label image and text.
        /// </summary>
        private const int LEFT_MARGIN = 2;

        /// <summary>
        /// The bottom margin to leave around the command bar label image and text.
        /// </summary>
        private const int BOTTOM_MARGIN = 4;

        /// <summary>
        /// The right margin to leave around the command bar label image and text.
        /// </summary>
        private const int RIGHT_MARGIN = 2;

        /// <summary>
        /// The label text.
        /// </summary>
        private readonly string text;

        /// <summary>
        /// The string format used to format text.
        /// </summary>
        private readonly TextFormatFlags textFormatFlags =
            TextFormatFlags.HorizontalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.PreserveGraphicsTranslateTransform;

        /// <summary>
        /// The text layout rectangle.  This is the rectangle into which the text is measured and
        /// drawn.  It is not the actual text rectangle.
        /// </summary>
        private Rectangle textLayoutRectangle;

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarLabelLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelLightweightControl class.
        /// </summary>
        public CommandBarLabelLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
            this.SetAccesibleInfo();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelLightweightControl class.
        /// </summary>
        /// <param name="text">The text for the label.</param>
        public CommandBarLabelLightweightControl(string text)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

            //	Set the text.
            this.text = text;

            this.SetAccesibleInfo();
        }

        private void SetAccesibleInfo()
        {
            this.AccessibleName = ControlHelper.ToAccessibleName(this.text);
            this.AccessibleRole = AccessibleRole.StaticText;
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        public Container Components { get; private set; } = null;

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.Components = new Container();
        #endregion

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            if (this.Parent == null)
            {
                return;
            }

            //	If there is no text, we're done.
            if (this.text == null || this.text.Length == 0)
            {
                this.VirtualSize = Size.Empty;
                return;
            }

            //	Obtain the font.
            var font = ApplicationManager.ApplicationStyle.NormalApplicationFont;

            //	The label width and height.
            int labelWidth = LEFT_MARGIN, labelHeight = TOP_MARGIN + font.Height + BOTTOM_MARGIN;

            using (var graphics = this.Parent.CreateGraphics())
            {
                //	Initialize the text layout rectangle.
                this.textLayoutRectangle = new Rectangle(labelWidth, TOP_MARGIN, MAX_TEXT_WIDTH, font.Height);

                var textSize = TextRenderer.MeasureText(graphics, this.text, font, this.textLayoutRectangle.Size, this.textFormatFlags);
                this.textLayoutRectangle.Size = textSize;

                //	Increase the label width to account for the text, plus a bit of extra space.
                labelWidth += textSize.Width + FontHelper.WidthOfSpace(graphics, font);
            }

            //	Increase the label width for the right margin.
            labelWidth += RIGHT_MARGIN;

            //	Set the new virtual size.
            this.VirtualSize = new Size(labelWidth, labelHeight);
        }

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            if (!string.IsNullOrEmpty(this.text))
            {
                var bidiGraphics = new BidiGraphics(e.Graphics, this.VirtualSize);
                bidiGraphics.DrawText(
                    this.text,
                    ApplicationManager.ApplicationStyle.NormalApplicationFont,
                    this.textLayoutRectangle,
                    Color.Black,
                    this.textFormatFlags | TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.NoPadding);
            }
        }
    }
}
