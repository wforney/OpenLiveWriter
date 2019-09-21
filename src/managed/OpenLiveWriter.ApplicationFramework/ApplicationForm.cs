// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Need to cleanly remove the UI code is made obsolete by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using OpenLiveWriter.Localization.Bidi;
    using OpenLiveWriter.Interop.Windows;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.ApplicationFramework.Skinning;

    /// <summary>
    /// Application form.
    /// </summary>
    public class ApplicationForm : BaseForm, IMainMenuBackgroundPainter
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly IContainer components;

        /// <summary>
        /// Initializes a new instance of the ApplicationForm class.
        /// </summary>
        public ApplicationForm()
        {
            //	Shut up!
            if (this.components == null)
            {
                this.components = null;
            }

            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.Text = ApplicationEnvironment.ProductNameQualified;

            //	Turn on double buffered painting.
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            if (!BidiHelper.IsRightToLeft)
            {
                this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            }

            //	Attach our command manager event handler.
            ColorizedResources.Instance.ColorizationChanged += new EventHandler(this.Instance_ColorizationChanged);
            SizeChanged += new EventHandler(this.ApplicationForm_SizeChanged);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                /* Explicit removal of the menu causes weird close-time jerkiness on Vista. There's a finalizer on the menu, it will get disposed eventually.
                if (Menu != null)
                {
                    MainMenu oldMainMenu = Menu;
                    Menu = null;
                    oldMainMenu.Dispose();
                }
                */
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            //
            // ApplicationForm
            //
            this.AutoScaleBaseSize = new Size(5, 14);
            this.ClientSize = new Size(442, 422);
            this.Name = "ApplicationForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Raises the Closed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnClosed(e);

            //	Detach our command management event handler.
            ColorizedResources.Instance.ColorizationChanged -= new EventHandler(this.Instance_ColorizationChanged);
            SizeChanged -= new EventHandler(this.ApplicationForm_SizeChanged);
        }

        /// <summary>
        /// Disposed a menu and all of its submenu items.
        /// </summary>
        /// <param name="menu"></param>
        private void DisposeMenu(Menu menu)
        {
            if (menu != null)
            {
                foreach (MenuItem subMenu in menu.MenuItems)
                {
                    this.DisposeMenu(subMenu);
                }

                menu.Dispose();
            }
        }

        #region Custom Main Menu Background Painting

        protected virtual bool MainMenuVisible => false;

        private void Instance_ColorizationChanged(object sender, EventArgs e) => this.UpdateMainMenuBrush();

        private void ApplicationForm_SizeChanged(object sender, EventArgs e)
        {
            //UpdateMainMenuBrush() ;
        }

        private void UpdateMainMenuBrush()
        {
            // screen states where there is no menu yet
            if (this.Menu == null)
            {
                return;
            }

            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }

            // alias colorized resources
            var cres = ColorizedResources.Instance;

            // dispose any existing brush and/or bitmaps
            if (this.hMainMenuBrush != IntPtr.Zero)
            {
                this.DisposeGDIObject(ref this.hMainMenuBrush);
            }

            if (this.hMainMenuBitmap != IntPtr.Zero)
            {
                this.DisposeGDIObject(ref this.hMainMenuBitmap);
            }

            if (this.hMainMenuBrushBitmap != IntPtr.Zero)
            {
                this.DisposeGDIObject(ref this.hMainMenuBrushBitmap);
            }

            if (this.mainMenuBitmap != null)
            {
                var tmp = this.mainMenuBitmap;
                this.mainMenuBitmap = null;
                tmp.Dispose();
            }

            // create a brush which contains the menu background
            this.mainMenuBitmap = new Bitmap(this.Width, -this.RelativeWindowBounds.Y);
            this.hMainMenuBitmap = this.mainMenuBitmap.GetHbitmap();
            using (var g = Graphics.FromImage(this.mainMenuBitmap))
            {
                var bounds = this.MenuBounds;
                Debug.WriteLine($"MenuBounds: {bounds}");
                if (cres.CustomMainMenuPainting)
                {
                    // paint custom menu background
                    this.CustomPaintMenuBackground(g, bounds);
                }
                else
                {
                    using (Brush brush = new SolidBrush(this.SystemMainMenuColor))
                    {
                        g.FillRectangle(brush, bounds);
                    }
                }
            }

            this.hMainMenuBrushBitmap = this.mainMenuBitmap.GetHbitmap();
            this.hMainMenuBrush = Gdi32.CreatePatternBrush(this.hMainMenuBrushBitmap);

            // set the brush
            var mi = new MENUINFO
            {
                cbSize = Marshal.SizeOf(typeof(MENUINFO)),
                fMask = MIM.BACKGROUND,
                hbrBack = this.hMainMenuBrush
            };
            User32.SetMenuInfo(this.Menu.Handle, ref mi);
            User32.DrawMenuBar(this.Handle);
        }

        private void DisposeGDIObject(ref IntPtr hObject)
        {
            var tmp = hObject;
            hObject = IntPtr.Zero;
            Gdi32.DeleteObject(tmp);
        }

        private void CustomPaintMenuBackground(Graphics g, Rectangle menuBounds)
        {
            var cres = ColorizedResources.Instance;

            // Fill in the background--important for narrow window sizes when the main menu items are stacked
            using (Brush brush = new SolidBrush(cres.MainMenuGradientTopColor))
            {
                g.FillRectangle(brush, new Rectangle(0, 0, this.Width, -this.RelativeWindowBounds.Y));
            }

            // Fill in the gradient
            using (Brush brush = new LinearGradientBrush(menuBounds, cres.MainMenuGradientTopColor, cres.MainMenuGradientBottomColor, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, menuBounds);
            }
        }

        /// <summary>
        /// The bounds of the menu, relative to the outer bounds of the window.
        /// </summary>
        private Rectangle MenuBounds =>
            new Rectangle(
                new Point(
                    -this.RelativeWindowBounds.X,
                    -this.RelativeWindowBounds.Y - SystemInformation.MenuHeight),
                new Size(
                    this.ClientSize.Width,
                    SystemInformation.MenuHeight));

        /// <summary>
        /// The outer bounds of the window, expressed in client coordinates.
        /// </summary>
        private Rectangle RelativeWindowBounds
        {
            get
            {
                var rect = new RECT();
                User32.GetWindowRect(this.Handle, ref rect);
                return this.RectangleToClient(RectangleHelper.Convert(rect));
            }
        }

        private IntPtr hMainMenuBrush = IntPtr.Zero;
        private Bitmap mainMenuBitmap = null;
        private IntPtr hMainMenuBitmap = IntPtr.Zero;
        private IntPtr hMainMenuBrushBitmap = IntPtr.Zero;

        void IMainMenuBackgroundPainter.PaintBackground(Graphics g, Rectangle menuItemBounds)
        {
            if (ColorizedResources.Instance.CustomMainMenuPainting)
            {
                if (this.mainMenuBitmap != null)
                {
                    try
                    {
                        // This has to be GDI+, not GDI, in order for the menu text
                        // anti-aliasing to look good
                        g.DrawImage(
                            this.mainMenuBitmap,
                            menuItemBounds,
                            menuItemBounds,
                            GraphicsUnit.Pixel);
                        return;
                    }
                    catch
                    {
                        Debug.Fail("Buffered paint of menu background failed");
                    }
                }

                try
                {
                    var graphicsState = g.Save();
                    try
                    {
                        g.SetClip(menuItemBounds);
                        this.CustomPaintMenuBackground(g, menuItemBounds);
                    }
                    finally
                    {
                        g.Restore(graphicsState);
                    }
                }
                catch
                {
                    Debug.Fail("Unbuffered paint of menu background also failed");
                }
            }
            else
            {
                using (var brush = new SolidBrush(this.SystemMainMenuColor))
                {
                    g.FillRectangle(brush, 0, 0, menuItemBounds.Width, menuItemBounds.Height);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            switch ((uint)m.Msg)
            {
                case WM.NCPAINT:
                case WM.NCACTIVATE:
                case WM.ERASEBKGND:
                    {
                        if (this.MainMenuVisible)
                        {
                            this.NonClientPaint(ref m);
                        }

                        break;
                    }
            }
        }

        private void NonClientPaint(ref Message m)
        {
            try
            {
                // screen states where we can't do the nc paint
                if (!this.IsHandleCreated)
                {
                    return;
                }

                if (this.mainMenuBitmap == null)
                {
                    return;
                }

                var hDC = User32.GetWindowDC(this.Handle);
                try
                {
                    using (var g = Graphics.FromHdc(hDC))
                    {
                        // destination rect
                        var frameWidth = User32.GetSystemMetrics(SM.CXSIZEFRAME);
                        Debug.Assert(frameWidth != 0);
                        var frameHeight = User32.GetSystemMetrics(SM.CYSIZEFRAME);
                        Debug.Assert(frameHeight != 0);

                        /*
                        Rectangle destinationRect = new Rectangle(
                            frameWidth,
                            frameHeight + SystemInformation.CaptionHeight + SystemInformation.MenuHeight - 1,
                            ClientSize.Width, 1) ;
                        */
                        // takes into account narrow window sizes where the main menu items start stacking vertically
                        var destinationRect = new Rectangle(-this.RelativeWindowBounds.X, -this.RelativeWindowBounds.Y - 1, this.ClientSize.Width, 1);

                        if (ColorizedResources.Instance.CustomMainMenuPainting)
                        {
                            // source rect (from menu bitmap)
                            var sourceRect = new Rectangle(
                                destinationRect.X,
                                this.mainMenuBitmap.Height - 1,
                                this.ClientSize.Width,
                                1);

                            //GdiPaint.BitBlt(g, _hMainMenuBitmap, sourceRect, destinationRect.Location);
                            g.DrawImage(this.mainMenuBitmap, destinationRect, sourceRect, GraphicsUnit.Pixel);
                        }
                        else
                        {
                            using (Brush b = new SolidBrush(this.SystemMainMenuColor))
                            {
                                g.FillRectangle(b, destinationRect);
                            }
                        }
                    }
                }
                finally
                {
                    User32.ReleaseDC(this.Handle, hDC);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail($"Unexpected exception during non-client painting: {ex.ToString()}");
            }
        }

        private Color SystemMainMenuColor => ColorizedResources.Instance.WorkspaceBackgroundColor;

        #endregion
    }
}
