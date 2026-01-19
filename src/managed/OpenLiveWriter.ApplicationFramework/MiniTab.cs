// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.ApplicationFramework.Skinning;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Localization.Bidi;

namespace OpenLiveWriter.ApplicationFramework
{
    class MiniTab : LightweightControl
    {
        private string text;
        private readonly MiniTabContext ctx;
        private Color? borderColor = null;

        public MiniTab(MiniTabContext ctx)
        {
            this.ctx = ctx;
            TabStop = true;
        }

        private Color? BorderColor
        {
            get
            {
                if (borderColor == null)
                {
                    if (LightweightControlContainerControl is MiniTabsControl control)
                    {
                        borderColor = control.TopBorderColor;
                    }
                }

                return borderColor;
            }
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                PerformLayout();
                Invalidate();
            }
        }

        public string ToolTip { get; set; }

        public event EventHandler SelectedChanged;

        public bool Selected { get; private set; }

        public void Select()
        {
            Selected = true;
            OnSelectedChanged();
            Invalidate();
        }

        internal void Unselect()
        {
            Selected = false;
            Invalidate();
        }

        protected virtual void OnSelectedChanged()
        {
            SelectedChanged?.Invoke(this, EventArgs.Empty);
        }

        public override Size DefaultVirtualSize
        {
            get
            {
                if (Parent == null)
                    return Size.Empty;
                Size size = TextRenderer.MeasureText(text, Selected ? ctx.FontSelected : ctx.Font);
                size.Height += (int)(Selected ? DisplayHelper.ScaleX(7) : DisplayHelper.ScaleY(5));
                size.Width += (int)DisplayHelper.ScaleX(5) + (size.Height / 2);
                return size;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            base.OnPaint(e);
            BidiGraphics g = new BidiGraphics(e.Graphics, VirtualClientRectangle);

            Rectangle tabRectangle = VirtualClientRectangle;

            if (Selected)
                ColorizedResources.Instance.ViewSwitchingTabSelected.DrawBorder(e.Graphics, tabRectangle);
            else
                ColorizedResources.Instance.ViewSwitchingTabUnselected.DrawBorder(e.Graphics, tabRectangle);

            if (ColorizedResources.UseSystemColors)
            {
                if (BorderColor.HasValue)
                {
                    using (Pen pen = new Pen(BorderColor.Value))
                    {
                        if (!Selected)
                            g.DrawLine(pen, tabRectangle.Left, tabRectangle.Top, tabRectangle.Right,
                                       tabRectangle.Top);
                        g.DrawLine(pen, tabRectangle.Left, tabRectangle.Top, tabRectangle.Left,
                                   tabRectangle.Bottom);
                        g.DrawLine(pen, tabRectangle.Right - 1, tabRectangle.Top, tabRectangle.Right - 1,
                                   tabRectangle.Bottom);
                        g.DrawLine(pen, tabRectangle.Left, tabRectangle.Bottom - 1,
                                   tabRectangle.Right, tabRectangle.Bottom - 1);
                    }
                }
            }

            /*
            if (!selected && !SystemInformation.HighContrast)
            {

                using (Pen p = new Pen(borderColor, 1.0f))
                    g.DrawLine(p, 0, 0, VirtualWidth, 0);
                using (Pen p = new Pen(Color.FromArgb(192, borderColor), 1.0f))
                    g.DrawLine(p, 0, 1, VirtualWidth - 1, 1);
                using (Pen p = new Pen(Color.FromArgb(128, borderColor), 1.0f))
                    g.DrawLine(p, 0, 2, VirtualWidth - 2, 2);
                using (Pen p = new Pen(Color.FromArgb(64, borderColor), 1.0f))
                    g.DrawLine(p, 0, 3, VirtualWidth - 2, 3);
            }
             * */

            Rectangle textBounds = tabRectangle;
            if (!Selected)
                textBounds.Y += (int)DisplayHelper.ScaleX(3);
            else
                textBounds.Y += (int)DisplayHelper.ScaleX(3);

            Color textColor = ColorizedResources.Instance.MainMenuTextColor;
            if (Selected)
                textColor = Parent.ForeColor;

            g.DrawText(Text, Selected ? ctx.Font : ctx.Font, textBounds, SystemInformation.HighContrast ? SystemColors.ControlText : textColor,
                       TextFormatFlags.Top | TextFormatFlags.HorizontalCenter | TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.PreserveGraphicsClipping);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            SetToolTip(ToolTip);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            SetToolTip(null);
        }
    }

    class MiniTabContext
    {
        private readonly Control parent;

        public MiniTabContext(Control parent)
        {
            this.parent = parent;
            string suffix = SystemInformation.HighContrast ? "-hi.png" : ".png";
            LeftOn = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabLeftSelected" + suffix);
            CenterOn = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabCenterSelected" + suffix);
            RightOn = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabRightSelected" + suffix);
            LeftOff = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabLeft" + suffix);
            CenterOff = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabCenter" + suffix);
            RightOff = ResourceHelper.LoadAssemblyResourceBitmap("Images.TabRight" + suffix);

            parent.FontChanged += parent_FontChanged;
            RefreshFonts();
        }

        void parent_FontChanged(object sender, EventArgs e)
        {
            RefreshFonts();
        }

        private void RefreshFonts()
        {
            Font = parent.Font;
            FontSelected = new Font(Font, FontStyle.Bold);
        }

        public Font Font { get; private set; }

        public Font FontSelected { get; private set; }

        public Bitmap LeftOn { get; }

        public Bitmap CenterOn { get; }

        public Bitmap RightOn { get; }

        public Bitmap LeftOff { get; }

        public Bitmap CenterOff { get; }

        public Bitmap RightOff { get; }
    }
}
