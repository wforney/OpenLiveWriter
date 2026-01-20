// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.UI;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.ApplicationFramework.Skinning
{
    /// <summary>
    /// Summary description for ColorizedResources.
    /// </summary>
    public class ColorizedResources
    {
        [ThreadStatic]
        private static ColorizedResources s_threadInstance;
        public static ColorizedResources Instance
        {
            get
            {
                if (s_threadInstance == null)
                    s_threadInstance = new ColorizedResources();

                return s_threadInstance;
            }
        }

        internal static event EventHandler GlobalColorizationChanged;

        private static void OnChange()
        {
            GlobalColorizationChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void FireColorChanged()
        {
            OnChange();
        }

        public static Color AppColor
        {
            get
            {
                if (UseSystemColors)
                {
                    return SystemColors.Control;
                }
                else
                {
                    if (ApplicationEnvironment.PreferencesSettingsRoot == null)
                        return Color.Empty;

                    string strColor = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetString("AppColor", null);
                    return strColor == null || strColor == string.Empty ? Color.Empty : ColorHelper.StringToColor(strColor);
                }
            }
            set
            {
                string strColor = value == Color.Empty ? null : ColorHelper.ColorToString(value);
                ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").SetString("AppColor", strColor);
                OnChange();
            }
        }

        public static int AppColorScale
        {
            get
            {
                return ApplicationEnvironment.PreferencesSettingsRoot == null
                    ? 128
                    : ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetInt32("AppColorScale", 128);
            }
            set
            {
                ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").SetInt32("AppColorScale", value);
                OnChange();
            }
        }

        private Color _colorizeColor;
        private int _colorizeScale;
        private BorderPaint _borderAppFrameOutline;
        private BorderPaint _borderFooterBackground;
        private BorderPaint _borderToolbar;
        private BorderPaint _viewSwitchingTabSelected;
        private BorderPaint _viewSwitchingTabUnselected;

        private ColorizedResources()
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(SystemEvents_UserPreferenceChanged);
            Refresh();
        }

        public event EventHandler ColorizationChanged;

        internal void FireColorizationChanged()
        {
            ColorizationChanged?.Invoke(this, EventArgs.Empty);
        }

        public static bool UseSystemColors
        {
            get
            {
                return SystemInformation.HighContrast;
            }
        }

        private void ReplaceWithEmptyBitmap(Bitmap b, Color c)
        {
            using (Graphics g = Graphics.FromImage(b))
            {
                g.FillRectangle(new SolidBrush(c), 0, 0, b.Width, b.Height);
            }
        }

        private Bitmap AdjustImageForSystemDPI(Bitmap b)
        {
            // see if we need to adjust our width for non-standard DPI (large fonts)
            const double DESIGNTIME_DPI = 96;
            double dpiX = Convert.ToDouble(DisplayHelper.PixelsPerLogicalInchX);
            if (dpiX > DESIGNTIME_DPI)
            {
                // adjust scale ration for percentage of toolbar containing text
                double scaleRatio = dpiX / DESIGNTIME_DPI;

                // change width as appropriate
                Bitmap adjustedBitmap = new Bitmap(b, Convert.ToInt32(Convert.ToDouble(b.Width) * scaleRatio), Convert.ToInt32(Convert.ToDouble(b.Height) * scaleRatio));
                b.Dispose();
                return adjustedBitmap;
            }

            return b;
        }

        private void RefreshImages()
        {
            DropShadowBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.DropShadow.png");
            FooterBackground = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.FooterBackgroundBW.png");

            Bitmap imgBorderAppFrameOutline = ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.AppFrameOutline.png"));
            Bitmap imgBorderToolbar =
                DisplayHelper.IsCompositionEnabled(false)
                    ? AdjustImageForSystemDPI(ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.Toolbar.png")))
                    : AdjustImageForSystemDPI(ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.XPActionBarBottom.png")));
            Bitmap imgSelectedTab = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.TabSelected.png");
            imgSelectedTab =
                ColorizeBitmap(imgSelectedTab, new Rectangle(1, 0, imgSelectedTab.Width - 2, imgSelectedTab.Height - 1));
            imgSelectedTab = AdjustImageForSystemDPI(imgSelectedTab);
            Bitmap imgUnselectedTab = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.TabUnselected.png");
            imgUnselectedTab =
                ColorizeBitmap(imgUnselectedTab, new Rectangle(1, 1, imgUnselectedTab.Width - 2, imgUnselectedTab.Height - 2));
            imgUnselectedTab = AdjustImageForSystemDPI(imgUnselectedTab);

            if (UseSystemColors)
            {
                ReplaceWithEmptyBitmap(FooterBackground, SystemColors.Control);

                ReplaceWithEmptyBitmap(imgBorderAppFrameOutline, SystemColors.Control);
                ReplaceWithEmptyBitmap(imgBorderToolbar, SystemColors.Control);
                ReplaceWithEmptyBitmap(imgUnselectedTab, SystemColors.Control);
                ReplaceWithEmptyBitmap(imgSelectedTab, SystemColors.Control);
            }

            using (new QuickTimer("Colorize bitmaps"))
            {
                SwapAndDispose(ref _borderAppFrameOutline, new BorderPaint(
                    imgBorderAppFrameOutline,
                    true,
                    BorderPaintMode.GDI, 6, 9, 221, 222));

                SwapAndDispose(ref _borderFooterBackground,
                               new BorderPaint(FooterBackground, false, BorderPaintMode.GDI,
                                               0, FooterBackground.Width, FooterBackground.Height, FooterBackground.Height));

                if (DisplayHelper.IsCompositionEnabled(false))
                {
                    SwapAndDispose(ref _borderToolbar, new BorderPaint(
                        imgBorderToolbar,
                        true,
                        BorderPaintMode.StretchToFill | BorderPaintMode.Cached, 2, imgBorderToolbar.Width - 2, 0, 0));
                }
                else
                {
                    SwapAndDispose(ref _borderToolbar, new BorderPaint(
                        imgBorderToolbar,
                        true,
                        BorderPaintMode.GDI, 0, imgBorderToolbar.Width - 1, 0, 0));
                }

                SwapAndDispose(ref _viewSwitchingTabSelected, new BorderPaint(imgSelectedTab, true, BorderPaintMode.StretchToFill | BorderPaintMode.Cached | BorderPaintMode.PaintMiddleCenter, 1, 8, 2, 8));
                SwapAndDispose(ref _viewSwitchingTabUnselected, new BorderPaint(imgUnselectedTab, true, BorderPaintMode.StretchToFill | BorderPaintMode.Cached | BorderPaintMode.PaintMiddleCenter, 1, 9, 1, 9));

                SidebarLinkColor = SystemInformation.HighContrast ? SystemColors.HotTrack : Color.FromArgb(0, 134, 198);

                /*
                SwapAndDispose(ref _imgAppVapor,
                    ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.AppVapor.png")));

                if (_hbAppVapor != IntPtr.Zero)
                    Gdi32.DeleteObject(_hbAppVapor);
                _hbAppVapor = _imgAppVapor.GetHbitmap();

                SwapAndDispose(ref _imgAppVaporFaded,
                    ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.AppVaporFaded.png")));

                if (_hbAppVaporFaded != IntPtr.Zero)
                    Gdi32.DeleteObject(_hbAppVaporFaded);
                _hbAppVaporFaded = _imgAppVaporFaded.GetHbitmap();
                */
            }
        }

        internal void Refresh()
        {
            Color coolGray = Color.FromArgb(243, 243, 247);

            bool useThemeColors = !UseSystemColors;
            _colorizeColor = AppColor;
            _colorizeScale = AppColorScale;

            RefreshImages();

            BorderDarkColor = useThemeColors ? Color.FromArgb(188, 188, 188) : SystemColors.ControlDark;
            BorderLightColor = useThemeColors ? Color.FromArgb(218, 229, 242) : SystemColors.ControlLight;
            //_borderLight = Colorize(Color.FromArgb(62, 152, 180));
            //_sidebarGradientTopColor = useThemeColors ? Colorize(Color.FromArgb(134, 209, 240)) : SystemColors.Control;
            SidebarGradientTopColor = useThemeColors ? Colorize(Color.FromArgb(255, 255, 255)) : SystemColors.Control;
            SidebarGradientBottomColor = useThemeColors ? Colorize(Color.FromArgb(255, 255, 255)) : SystemColors.Control;
            // TODO: Fix the colors.
            //SidebarTextColor = SystemColors.HotTrack;
            SidebarHeaderBackgroundColor = !useThemeColors ? SystemColors.Control : Colorize(Color.FromArgb(255, 255, 255));
            SidebarHeaderTextColor = Color.FromArgb(53, 90, 136);
            FrameGradientLight = useThemeColors ? Colorize(Color.FromArgb(255, 255, 255)) : SystemColors.Control;
            WorkspaceBackgroundColor = useThemeColors ? Colorize(coolGray) : SystemColors.Control;
            MainMenuGradientTopColor = !useThemeColors ? SystemColors.Control :
                DisplayHelper.IsCompositionEnabled(false) ? Colorize(Color.FromArgb(229, 238, 248)) :
                Colorize(Color.FromArgb(229, 238, 248));
            MainMenuGradientBottomColor = !useThemeColors ? SystemColors.Control :
                DisplayHelper.IsCompositionEnabled(false) ? Colorize(Color.FromArgb(229, 238, 248)) :
                MainMenuGradientTopColor;
            SecondaryToolbarColor = useThemeColors ? Colorize(coolGray) : SystemColors.Control;

        }

        private void SwapAndDispose(ref Bitmap image, Bitmap newImage)
        {
            Bitmap oldImage = image;
            image = newImage;
            oldImage?.Dispose();
        }

        private void SwapAndDispose(ref BorderPaint bd, BorderPaint newBd)
        {
            BorderPaint oldBd = bd;
            bd = newBd;
            oldBd?.Dispose();
        }

        private Bitmap ColorizeBitmap(Bitmap bitmap)
        {
            return Colorizer.ColorizeBitmap(bitmap, _colorizeColor, _colorizeScale);
        }

        private Bitmap ColorizeBitmap(Bitmap bmp, Rectangle colorizeRectangle)
        {
            return Colorizer.ColorizeBitmap(bmp, _colorizeColor, _colorizeScale, colorizeRectangle);
        }

        public Bitmap FooterBackground { get; private set; }

        public Bitmap DropShadowBitmap { get; private set; }

        public BorderPaint AppOutlineBorder
        {
            get { return _borderAppFrameOutline; }
        }

        public BorderPaint AppFooterBackground
        {
            get { return _borderFooterBackground; }
        }

        public BorderPaint ToolbarBorder
        {
            get { return _borderToolbar; }
        }

        public BorderPaint ViewSwitchingTabSelected
        {
            get
            {
                return _viewSwitchingTabSelected;
            }
        }

        public BorderPaint ViewSwitchingTabUnselected
        {
            get
            {
                return _viewSwitchingTabUnselected;
            }
        }

        public Color SidebarLinkColor { get; private set; }

        public Color FrameGradientLight { get; private set; }

        public Color SidebarGradientTopColor { get; private set; }

        public Color SidebarGradientBottomColor { get; private set; }

        [Obsolete("This looks funny when juxtapositioned with GDI-drawn LinkLabel controls, which appear a little darker than they should.")]
        public Color SidebarTextColor { get; private set; }

        public Color SidebarDisabledTextColor
        {
            get { return SystemColors.GrayText; }
        }

        public Color BorderDarkColor { get; private set; }

        public Color SecondaryToolbarColor { get; private set; }

        public Color BorderLightColor { get; private set; }

        public Color SidebarHeaderBackgroundColor { get; private set; }

        public Color SidebarHeaderTextColor { get; private set; }

        public Color WorkspaceBackgroundColor { get; private set; }

        public bool CustomMainMenuPainting
        {
            get
            {
                return !UseSystemColors;
            }
        }

        public Color MainMenuGradientTopColor { get; private set; }

        public Color MainMenuGradientBottomColor { get; private set; }

        public Color MainMenuHighlightColor
        {
            get
            {
                return CustomMainMenuPainting ? Color.FromArgb(160, 160, 160) : SystemColors.Highlight;
            }
        }

        public Color MainMenuTextColor
        {
            get
            {
                return CustomMainMenuPainting
                    ? DisplayHelper.IsCompositionEnabled(false) && ColorHelper.GetLuminosity(_colorizeColor) > 128
                        ? Color.FromArgb(99, 101, 99)
                        : Color.FromArgb(99, 101, 99)
                    : SystemColors.MenuText;
            }
        }

        public Color Colorize(Color color)
        {
            return Colorizer.ColorizeARGB(color, _colorizeColor, _colorizeScale);
        }

        public Bitmap Colorize(Bitmap bmp)
        {
            return Colorizer.ColorizeBitmap(bmp, _colorizeColor, _colorizeScale);
        }

        public delegate void ControlUpdater<TControl>(ColorizedResources cr, TControl c) where TControl : Control;
        public void RegisterControlForUpdates<TControl>(TControl control, ControlUpdater<TControl> updater) where TControl : Control
        {
            updater(Instance, control);
            new ColorizationUpdateRegistrationHandle<TControl>(control, updater);
        }

        public void RegisterControlForBackColorUpdates(Control control)
        {
            RegisterControlForUpdates(control, delegate (ColorizedResources cr, Control c) { c.BackColor = cr.WorkspaceBackgroundColor; });
        }

        private class ColorizationUpdateRegistrationHandle<TControl> where TControl : Control
        {
            private readonly TControl control;
            private readonly ControlUpdater<TControl> updater;

            public ColorizationUpdateRegistrationHandle(TControl control, ControlUpdater<TControl> updater)
            {
                this.control = control;
                this.updater = updater;
                control.Disposed += new EventHandler(ControlDisposedHandler);
                GlobalColorizationChanged += new EventHandler(Handler);
            }

            private void Handler(object sender, EventArgs e)
            {
                if (ControlHelper.ControlCanHandleInvoke(control))
                {
                    control.BeginInvoke(new EventHandler(Handler2), new object[] { sender, e });
                }
            }

            private void Handler2(object sender, EventArgs e)
            {
                updater(ColorizedResources.Instance, control);
            }

            private void ControlDisposedHandler(object sender, EventArgs e)
            {
                GlobalColorizationChanged -= new EventHandler(Handler);
            }
        }

        private void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
        {
            OnChange();
        }
    }

    class Colorizer
    {
        public static Color ColorizeRGB(Color crOld, Color crColorize, int wScale /*= 220*/)
        {
            // Convert special COL_GrayText value to the system's graytext color.
            /*
            if (crColorize == COL_GrayText)
            {
                crColorize = ::GetSysColor(COLOR_GRAYTEXT);
            }
            */

            // Is the colorize value a flag?
            if (Color.Empty == crColorize)
            {
                // Yes!  Then don't colorize.  Return old value unchanged.
                return crOld;
            }

            int wLumOld = (Math.Max(crOld.R, Math.Max(crOld.G, crOld.B)) +
                Math.Min(crOld.R, Math.Min(crOld.G, crOld.B))) / 2;

            if (wScale < wLumOld)
            {
                byte rColorizeHi = (byte)(255 - crColorize.R);
                byte gColorizeHi = (byte)(255 - crColorize.G);
                byte bColorizeHi = (byte)(255 - crColorize.B);
                int wScaleHi = 255 - wScale;

                wLumOld -= wScale;
                byte rNew = (byte)(crColorize.R + (byte)(rColorizeHi * wLumOld / wScaleHi));
                byte gNew = (byte)(crColorize.G + (byte)(gColorizeHi * wLumOld / wScaleHi));
                byte bNew = (byte)(crColorize.B + (byte)(bColorizeHi * wLumOld / wScaleHi));

                return Color.FromArgb(rNew, gNew, bNew);
            }
            else
            {
                byte rNew = (byte)(crColorize.R * wLumOld / wScale);
                byte gNew = (byte)(crColorize.G * wLumOld / wScale);
                byte bNew = (byte)(crColorize.B * wLumOld / wScale);

                return Color.FromArgb(rNew, gNew, bNew);
            }
        }

        public static Color ColorizeARGB(Color crOld, Color crColorize, int wScale)
        {
            if (Color.Empty == crColorize)
                return crOld;

            int rNew = crColorize.R;
            int gNew = crColorize.G;
            int bNew = crColorize.B;
            int rNewHi = 255 - rNew;
            int gNewHi = 255 - gNew;
            int bNewHi = 255 - bNew;
            int wScaleHi = 255 - wScale;

            // the pixel RGB has been prescaled with alpha
            // we need to multiply the scale point and the new color by alpha as well
            int wLumAlpha = (Math.Max(crOld.R, Math.Max(crOld.G, crOld.B)) +
                Math.Min(crOld.R, Math.Min(crOld.G, crOld.B))) / 2;

            int wScaleAlpha = wScale * crOld.A / 255;
            if (wScaleAlpha < wLumAlpha)
            {
                wLumAlpha -= wScaleAlpha;

                // New needs to be scaled by alpha.
                // NewHi does not as it is multiplied by the LumAlpha which includes the alpha scaling
                return Color.FromArgb(
                    crOld.A,
                    (byte)((rNew * crOld.A / 255) + (rNewHi * wLumAlpha / wScaleHi)),
                    (byte)((gNew * crOld.A / 255) + (gNewHi * wLumAlpha / wScaleHi)),
                    (byte)((bNew * crOld.A / 255) + (bNewHi * wLumAlpha / wScaleHi)));
            }
            else
            {
                // New is multiplied by the LumAlpha which includes the alpha scaling
                return Color.FromArgb(
                    (byte)(rNew * wLumAlpha / wScale),
                    (byte)(gNew * wLumAlpha / wScale),
                    (byte)(bNew * wLumAlpha / wScale));
            }
        }

        public static Bitmap ColorizeBitmap(Bitmap bitmap, Color crColorize, int wScale)
        {
            return ColorizeBitmap(bitmap, crColorize, wScale, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
        }

        public static Bitmap ColorizeBitmap(Bitmap bitmap, Color crColorize, int wScale, Rectangle colorizeRect)
        {
            if (crColorize == Color.Empty)
                return new Bitmap(bitmap);

            Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            for (int x = 0; x < bitmap.Width; x++)
            {
                for (int y = 0; y < bitmap.Height; y++)
                {
                    if (colorizeRect.Contains(x, y))
                        newBitmap.SetPixel(x, y, ColorizeARGB(bitmap.GetPixel(x, y), crColorize, wScale));
                    else
                        newBitmap.SetPixel(x, y, bitmap.GetPixel(x, y));
                }
            }

            return newBitmap;
        }
    }
}
