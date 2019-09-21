// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Skinning
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.UI;

    /// <summary>
    /// Summary description for ColorizedResources.
    /// </summary>
    public class ColorizedResources
    {
        [ThreadStatic]
        private static ColorizedResources threadInstance;
        public static ColorizedResources Instance
        {
            get
            {
                if (threadInstance == null)
                {
                    threadInstance = new ColorizedResources();
                }

                return threadInstance;
            }
        }

        internal static event EventHandler GlobalColorizationChanged;

        private static void OnChange() => GlobalColorizationChanged?.Invoke(null, EventArgs.Empty);

        public static void FireColorChanged() => OnChange();

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
                    {
                        return Color.Empty;
                    }

                    var strColor = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("Appearance").GetString("AppColor", null);
                    return strColor == null || strColor == string.Empty ? Color.Empty : ColorHelper.StringToColor(strColor);
                }
            }
            set
            {
                var strColor = value == Color.Empty ? null : ColorHelper.ColorToString(value);

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

        private Color colorizeColor;
        private int colorizeScale;
        private BorderPaint borderAppFrameOutline;
        private BorderPaint borderFooterBackground;
        private BorderPaint borderToolbar;
        private BorderPaint viewSwitchingTabSelected;
        private BorderPaint viewSwitchingTabUnselected;

        private ColorizedResources()
        {
            Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(this.SystemEvents_UserPreferenceChanged);
            this.Refresh();
        }

        public event EventHandler ColorizationChanged;

        internal void FireColorizationChanged() => ColorizationChanged?.Invoke(this, EventArgs.Empty);

        public static bool UseSystemColors => SystemInformation.HighContrast;

        private void ReplaceWithEmptyBitmap(Bitmap b, Color c)
        {
            using (var g = Graphics.FromImage(b))
            {
                g.FillRectangle(new SolidBrush(c), 0, 0, b.Width, b.Height);
            }
        }

        private Bitmap AdjustImageForSystemDPI(Bitmap b)
        {
            // see if we need to adjust our width for non-standard DPI (large fonts)
            const double DESIGNTIME_DPI = 96;
            var dpiX = Convert.ToDouble(DisplayHelper.PixelsPerLogicalInchX);
            if (dpiX > DESIGNTIME_DPI)
            {
                // adjust scale ration for percentage of toolbar containing text
                var scaleRatio = dpiX / DESIGNTIME_DPI;

                // change width as appropriate
                var adjustedBitmap = new Bitmap(b, Convert.ToInt32(Convert.ToDouble(b.Width) * scaleRatio), Convert.ToInt32(Convert.ToDouble(b.Height) * scaleRatio));
                b.Dispose();
                return adjustedBitmap;
            }

            return b;
        }

        private void RefreshImages()
        {
            this.DropShadowBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.DropShadow.png");
            this.FooterBackground = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.FooterBackgroundBW.png");

            var imgBorderAppFrameOutline = this.ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.AppFrameOutline.png"));
            var imgBorderToolbar =
                DisplayHelper.IsCompositionEnabled(false)
                    ? this.AdjustImageForSystemDPI(this.ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.Toolbar.png")))
                    : this.AdjustImageForSystemDPI(this.ColorizeBitmap(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.XPActionBarBottom.png")));
            var imgSelectedTab = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.TabSelected.png");
            imgSelectedTab =
                this.ColorizeBitmap(imgSelectedTab, new Rectangle(1, 0, imgSelectedTab.Width - 2, imgSelectedTab.Height - 1));
            imgSelectedTab = this.AdjustImageForSystemDPI(imgSelectedTab);
            var imgUnselectedTab = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.TabUnselected.png");
            imgUnselectedTab =
                this.ColorizeBitmap(imgUnselectedTab, new Rectangle(1, 1, imgUnselectedTab.Width - 2, imgUnselectedTab.Height - 2));
            imgUnselectedTab = this.AdjustImageForSystemDPI(imgUnselectedTab);

            if (UseSystemColors)
            {
                this.ReplaceWithEmptyBitmap(this.FooterBackground, SystemColors.Control);

                this.ReplaceWithEmptyBitmap(imgBorderAppFrameOutline, SystemColors.Control);
                this.ReplaceWithEmptyBitmap(imgBorderToolbar, SystemColors.Control);
                this.ReplaceWithEmptyBitmap(imgUnselectedTab, SystemColors.Control);
                this.ReplaceWithEmptyBitmap(imgSelectedTab, SystemColors.Control);
            }

            using (new QuickTimer("Colorize bitmaps"))
            {
                this.SwapAndDispose(ref this.borderAppFrameOutline, new BorderPaint(
                    imgBorderAppFrameOutline,
                    true,
                    BorderPaintMode.GDI, 6, 9, 221, 222));

                this.SwapAndDispose(ref this.borderFooterBackground,
                               new BorderPaint(this.FooterBackground, false, BorderPaintMode.GDI,
                                               0, this.FooterBackground.Width, this.FooterBackground.Height, this.FooterBackground.Height));

                if (DisplayHelper.IsCompositionEnabled(false))
                {
                    this.SwapAndDispose(ref this.borderToolbar, new BorderPaint(
                        imgBorderToolbar,
                        true,
                        BorderPaintMode.StretchToFill | BorderPaintMode.Cached, 2, imgBorderToolbar.Width - 2, 0, 0));
                }
                else
                {
                    this.SwapAndDispose(ref this.borderToolbar, new BorderPaint(
                        imgBorderToolbar,
                        true,
                        BorderPaintMode.GDI, 0, imgBorderToolbar.Width - 1, 0, 0));
                }

                this.SwapAndDispose(ref this.viewSwitchingTabSelected, new BorderPaint(imgSelectedTab, true, BorderPaintMode.StretchToFill | BorderPaintMode.Cached | BorderPaintMode.PaintMiddleCenter, 1, 8, 2, 8));
                this.SwapAndDispose(ref this.viewSwitchingTabUnselected, new BorderPaint(imgUnselectedTab, true, BorderPaintMode.StretchToFill | BorderPaintMode.Cached | BorderPaintMode.PaintMiddleCenter, 1, 9, 1, 9));

                this.SidebarLinkColor = SystemInformation.HighContrast ? SystemColors.HotTrack : Color.FromArgb(0, 134, 198);

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
            var coolGray = Color.FromArgb(243, 243, 247);

            var useThemeColors = !UseSystemColors;
            this.colorizeColor = AppColor;
            this.colorizeScale = AppColorScale;

            this.RefreshImages();

            this.BorderDarkColor = useThemeColors ? Color.FromArgb(188, 188, 188) : SystemColors.ControlDark;
            this.BorderLightColor = useThemeColors ? Color.FromArgb(218, 229, 242) : SystemColors.ControlLight;
            //_borderLight = Colorize(Color.FromArgb(62, 152, 180));
            //_sidebarGradientTopColor = useThemeColors ? Colorize(Color.FromArgb(134, 209, 240)) : SystemColors.Control;
            this.SidebarGradientTopColor = useThemeColors ? this.Colorize(Color.FromArgb(255, 255, 255)) : SystemColors.Control;
            this.SidebarGradientBottomColor = useThemeColors ? this.Colorize(Color.FromArgb(255, 255, 255)) : SystemColors.Control;
#pragma warning disable CS0618 // Type or member is obsolete
            this.SidebarTextColor = SystemColors.HotTrack;
#pragma warning restore CS0618 // Type or member is obsolete
            this.SidebarHeaderBackgroundColor = !useThemeColors ? SystemColors.Control : this.Colorize(Color.FromArgb(255, 255, 255));
            this.SidebarHeaderTextColor = Color.FromArgb(53, 90, 136);
            this.FrameGradientLight = useThemeColors ? this.Colorize(Color.FromArgb(255, 255, 255)) : SystemColors.Control;
            this.WorkspaceBackgroundColor = useThemeColors ? this.Colorize(coolGray) : SystemColors.Control;
            this.MainMenuGradientTopColor = !useThemeColors ? SystemColors.Control :
                DisplayHelper.IsCompositionEnabled(false) ? this.Colorize(Color.FromArgb(229, 238, 248)) :
                this.Colorize(Color.FromArgb(229, 238, 248));
            this.MainMenuGradientBottomColor = !useThemeColors ? SystemColors.Control :
                DisplayHelper.IsCompositionEnabled(false) ? this.Colorize(Color.FromArgb(229, 238, 248)) :
                this.MainMenuGradientTopColor;
            this.SecondaryToolbarColor = useThemeColors ? this.Colorize(coolGray) : SystemColors.Control;

        }

        private void SwapAndDispose(ref Bitmap image, Bitmap newImage)
        {
            var oldImage = image;
            image = newImage;
            if (oldImage != null)
            {
                oldImage.Dispose();
            }
        }

        private void SwapAndDispose(ref BorderPaint bd, BorderPaint newBd)
        {
            var oldBd = bd;
            bd = newBd;
            if (oldBd != null)
            {
                oldBd.Dispose();
            }
        }

        private Bitmap ColorizeBitmap(Bitmap bitmap) => Colorizer.ColorizeBitmap(bitmap, this.colorizeColor, this.colorizeScale);

        private Bitmap ColorizeBitmap(Bitmap bmp, Rectangle colorizeRectangle) =>
            Colorizer.ColorizeBitmap(bmp, this.colorizeColor, this.colorizeScale, colorizeRectangle);

        public Bitmap FooterBackground { get; private set; }

        public Bitmap DropShadowBitmap { get; private set; }

        public BorderPaint AppOutlineBorder => this.borderAppFrameOutline;

        public BorderPaint AppFooterBackground => this.borderFooterBackground;

        public BorderPaint ToolbarBorder => this.borderToolbar;

        public BorderPaint ViewSwitchingTabSelected => this.viewSwitchingTabSelected;

        public BorderPaint ViewSwitchingTabUnselected => this.viewSwitchingTabUnselected;

        public Color SidebarLinkColor { get; private set; }

        public Color FrameGradientLight { get; private set; }

        public Color SidebarGradientTopColor { get; private set; }

        public Color SidebarGradientBottomColor { get; private set; }

        [Obsolete("This looks funny when juxtapositioned with GDI-drawn LinkLabel controls, which appear a little darker than they should.")]
        public Color SidebarTextColor { get; private set; }

        public Color SidebarDisabledTextColor => SystemColors.GrayText;

        public Color BorderDarkColor { get; private set; }

        public Color SecondaryToolbarColor { get; private set; }

        public Color BorderLightColor { get; private set; }

        public Color SidebarHeaderBackgroundColor { get; private set; }

        public Color SidebarHeaderTextColor { get; private set; }

        public Color WorkspaceBackgroundColor { get; private set; }

        public bool CustomMainMenuPainting => !UseSystemColors;

        public Color MainMenuGradientTopColor { get; private set; }

        public Color MainMenuGradientBottomColor { get; private set; }

        public Color MainMenuHighlightColor =>
            this.CustomMainMenuPainting ? Color.FromArgb(160, 160, 160) : SystemColors.Highlight;

        public Color MainMenuTextColor =>
            this.CustomMainMenuPainting
                ? DisplayHelper.IsCompositionEnabled(false) && ColorHelper.GetLuminosity(this.colorizeColor) > 128
                    ? Color.FromArgb(99, 101, 99)
                    : Color.FromArgb(99, 101, 99)
                : SystemColors.MenuText;

        public Color Colorize(Color color) => Colorizer.ColorizeARGB(color, this.colorizeColor, this.colorizeScale);

        public Bitmap Colorize(Bitmap bmp) => Colorizer.ColorizeBitmap(bmp, this.colorizeColor, this.colorizeScale);

        public delegate void ControlUpdater<TControl>(ColorizedResources cr, TControl c) where TControl : Control;
        public void RegisterControlForUpdates<TControl>(TControl control, ControlUpdater<TControl> updater) where TControl : Control
        {
            updater(Instance, control);
            new ColorizationUpdateRegistrationHandle<TControl>(control, updater);
        }

        public void RegisterControlForBackColorUpdates(Control control) =>
            this.RegisterControlForUpdates(control, delegate (ColorizedResources cr, Control c)
            {
                c.BackColor = cr.WorkspaceBackgroundColor;
            });

        private class ColorizationUpdateRegistrationHandle<TControl> where TControl : Control
        {
            private readonly TControl control;
            private readonly ControlUpdater<TControl> updater;

            public ColorizationUpdateRegistrationHandle(TControl control, ControlUpdater<TControl> updater)
            {
                this.control = control;
                this.updater = updater;
                control.Disposed += new EventHandler(this.ControlDisposedHandler);
                GlobalColorizationChanged += new EventHandler(this.Handler);
            }

            private void Handler(object sender, EventArgs e)
            {
                if (ControlHelper.ControlCanHandleInvoke(this.control))
                {
                    this.control.BeginInvoke(new EventHandler(this.Handler2), new object[] { sender, e });
                }
            }

            private void Handler2(object sender, EventArgs e) => this.updater(ColorizedResources.Instance, this.control);

            private void ControlDisposedHandler(object sender, EventArgs e) => GlobalColorizationChanged -= new EventHandler(this.Handler);
        }

        private void SystemEvents_UserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e) => OnChange();
    }
}
