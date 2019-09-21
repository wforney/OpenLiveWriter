// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Windows;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// Summary description for ColorPopup.
    /// </summary>
    public class ColorPopup : UserControl
    {
        private Color m_color = Color.Empty;
        private readonly Bitmap m_dropDownArrow = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.BlackDropArrow.png");
        private readonly Bitmap m_buttonOutlineHover = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonHover.png");
        private readonly Bitmap m_buttonOutlinePressed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonPressed.png");
        private bool m_hover = false;
        private bool m_pressed = false;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly System.ComponentModel.Container components = null;

        public ColorPopup()
        {
            // enable double buffered painting.
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
        }

        public Color Color
        {
            get => this.m_color;
            set
            {
                this.m_color = value;
                ColorSelected?.Invoke(this, new ColorSelectedEventArgs(value));
                this.Invalidate();
            }
        }

        public Color EffectiveColor
        {
            get
            {
                return this.m_color == Color.Empty ? Color.FromArgb(86, 150, 172) : this.m_color;
            }
        }

        public event ColorSelectedEventHandler ColorSelected;

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
            //
            // ColorPopup
            //
            this.Name = "ColorPopup";
            this.Size = new Size(150, 40);
            this.Text = "&Color Scheme";
        }
        #endregion

        public void AutoSizeForm()
        {
            using (var g = Graphics.FromHwnd(User32.GetDesktopWindow()))
            {
                var sf = new StringFormat(StringFormat.GenericDefault);
                sf.HotkeyPrefix = this.ShowKeyboardCues ? HotkeyPrefix.Show : HotkeyPrefix.Hide;
                var size = g.MeasureString(this.Text, this.Font, new PointF(0, 0), sf);
                this.Width = PADDING * 2 + GUTTER_SIZE * 2 + COLOR_SIZE + (int)Math.Ceiling(size.Width) + this.m_dropDownArrow.Width;
                this.Height = PADDING * 2 + COLOR_SIZE;
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.m_hover = true;
            this.Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.m_hover = false;
            this.Invalidate();
        }

        const int PADDING = 6;
        const int GUTTER_SIZE = 3;
        const int COLOR_SIZE = 14;

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = new BidiGraphics(e.Graphics, this.ClientRectangle);
            if (this.m_pressed)
            {
                SystemButtonHelper.DrawSystemButtonFacePushed(g, false, this.ClientRectangle, false);
            }
            else if (this.m_hover)
            {
                SystemButtonHelper.DrawSystemButtonFace(g, false, false, this.ClientRectangle, false);
            }

            var colorRect = new Rectangle(PADDING, PADDING, COLOR_SIZE, COLOR_SIZE);
            var dropDownArrowRect = new Rectangle(this.Width - PADDING - this.m_dropDownArrow.Width,
                                                        PADDING,
                                                        this.m_dropDownArrow.Width,
                                                        colorRect.Height);
            var textRect = new Rectangle(PADDING + GUTTER_SIZE + colorRect.Width,
                PADDING,
                this.Width - (PADDING + GUTTER_SIZE + colorRect.Width) - (PADDING + GUTTER_SIZE + dropDownArrowRect.Width),
                colorRect.Height);

            using (Brush b = new SolidBrush(this.EffectiveColor))
            {
                g.FillRectangle(b, colorRect);
            }

            using (var p = new Pen(SystemColors.Highlight, 1))
            {
                g.DrawRectangle(p, colorRect);
            }

            g.DrawText(this.Text, this.Font, textRect, SystemColors.ControlText, this.ShowKeyboardCues ? TextFormatFlags.Default : TextFormatFlags.NoPrefix);

            g.DrawImage(false,
                        this.m_dropDownArrow,
                        RectangleHelper.Center(this.m_dropDownArrow.Size, dropDownArrowRect, false),
                        0,
                        0,
                        this.m_dropDownArrow.Width,
                        this.m_dropDownArrow.Height,
                        GraphicsUnit.Pixel);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var form = new ColorPickerForm();
            form.Color = this.Color;
            form.ColorSelected += new ColorSelectedEventHandler(this.form_ColorSelected);
            form.Closed += new EventHandler(this.form_Closed);
            form.TopMost = true;

            form.StartPosition = FormStartPosition.Manual;
            var p = this.PointToScreen(new Point(0, this.Height));
            form.Location = p;
            form.Show();
            this.m_pressed = true;
            this.Invalidate();
        }

        private void form_ColorSelected(object sender, ColorSelectedEventArgs args) => this.Color = args.SelectedColor;

        private void form_Closed(object sender, EventArgs e)
        {
            this.m_pressed = false;
            this.Invalidate();
        }
    }
}
