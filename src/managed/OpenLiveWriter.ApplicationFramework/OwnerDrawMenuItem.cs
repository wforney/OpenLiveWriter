// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    using OpenLiveWriter.ApplicationFramework.Skinning;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    ///     Base class for owner-draw menu items.
    /// </summary>
    public class OwnerDrawMenuItem : MenuItem
    {
        /// <summary>
        ///     Standard menu item bitmap area width.
        /// </summary>
        private const int STANDARD_BITMAP_AREA_WIDTH = 20;

        /// <summary>
        ///     Standard menu item maximum text width.
        /// </summary>
        private const int STANDARD_MAXIMUM_TEXT_WIDTH = 400;

        /// <summary>
        ///     Standard menu item minimum width.
        /// </summary>
        private const int STANDARD_MINIMUM_WIDTH = 150;

        /// <summary>
        ///     Standard menu item right edge padding.
        /// </summary>
        private const int STANDARD_RIGHT_EDGE_PAD = 12;

        /// <summary>
        ///     Standard menu item separator padding.
        /// </summary>
        private const int STANDARD_SEPARATOR_PADDING = 0;

        /// <summary>
        ///     Standard menu item text padding.
        /// </summary>
        private const int STANDARD_TEXT_PADDING = 4;

        /// <summary>
        ///     Top-level menu item text padding.
        /// </summary>
        private const int TOP_LEVEL_TEXT_PADDING = 2;

        /// <summary>
        ///     Main menu item text string format when hot keys are displayed.
        /// </summary>
        private static readonly TextFormatFlags mainMenuItemTextHotKeyStringFormat;

        /// <summary>
        ///     Main menu item text string format when hot keys are not displayed.
        /// </summary>
        private static readonly TextFormatFlags mainMenuItemTextNoHotKeyStringFormat;

        /// <summary>
        ///     Menu item text string format when hot keys are displayed.
        /// </summary>
        private static readonly TextFormatFlags menuItemTextHotKeyStringFormat;

        /// <summary>
        ///     Menu item text string format when hot keys are not displayed.
        /// </summary>
        private static readonly TextFormatFlags menuItemTextNoHotKeyStringFormat;

        /// <summary>
        ///     Shortcut text string format.
        /// </summary>
        private static readonly TextFormatFlags shortcutStringFormat;

        /// <summary>
        ///     The menu text.
        /// </summary>
        protected readonly string text;

        /// <summary>The menu font</summary>
        private readonly Font menuFont;

        /// <summary>
        ///     Offscreen bitmap.  Cached between calls to improve efficiency.
        /// </summary>
        private Bitmap offscreenBitmap;

        /// <summary>
        /// Initializes static members of the <see cref="OwnerDrawMenuItem"/> class.
        /// </summary>
        static OwnerDrawMenuItem()
        {
            // Initialize the main menu item hot key text string format.
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat = new TextFormatFlags();
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.HorizontalCenter;
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.WordBreak;
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.EndEllipsis;
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat |= TextFormatFlags.TextBoxControl;

            // Initialize the main menu item no hot key text string format.
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat = new TextFormatFlags();
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.HorizontalCenter;
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.WordBreak;
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.HidePrefix;
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.TextBoxControl;
            OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat |= TextFormatFlags.EndEllipsis;

            // Initialize the menu item hot key text string format.
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat = new TextFormatFlags();
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat |= TextFormatFlags.HidePrefix;
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat |= TextFormatFlags.WordBreak;
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat |= TextFormatFlags.TextBoxControl;
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat |= TextFormatFlags.EndEllipsis;

            // Initialize the menu item no hot key text string format.
            OwnerDrawMenuItem.menuItemTextHotKeyStringFormat = new TextFormatFlags();
            OwnerDrawMenuItem.menuItemTextHotKeyStringFormat |= TextFormatFlags.VerticalCenter;
            OwnerDrawMenuItem.menuItemTextHotKeyStringFormat |= TextFormatFlags.WordBreak;
            OwnerDrawMenuItem.menuItemTextHotKeyStringFormat |= TextFormatFlags.TextBoxControl;
            OwnerDrawMenuItem.menuItemTextHotKeyStringFormat |= TextFormatFlags.ExpandTabs;
            OwnerDrawMenuItem.menuItemTextHotKeyStringFormat |= TextFormatFlags.EndEllipsis;

            // Initialize the shortcut string format.
            OwnerDrawMenuItem.shortcutStringFormat = new TextFormatFlags();
            OwnerDrawMenuItem.shortcutStringFormat |= TextFormatFlags.VerticalCenter;
            OwnerDrawMenuItem.shortcutStringFormat |= TextFormatFlags.Right;
            OwnerDrawMenuItem.shortcutStringFormat |= TextFormatFlags.WordBreak;
            OwnerDrawMenuItem.shortcutStringFormat |= TextFormatFlags.TextBoxControl;
            OwnerDrawMenuItem.shortcutStringFormat |= TextFormatFlags.ExpandTabs;
            OwnerDrawMenuItem.shortcutStringFormat |= TextFormatFlags.EndEllipsis;
        }

        /// <summary>
        ///     Initializes a new instance of the OwnerDrawMenuItem class.
        /// </summary>
        public OwnerDrawMenuItem(MenuType menuType, string text)
        {
            // Set the menu type value.
            this.MenuType = menuType;
            this.text = text;
            this.Text = text;

            this.menuFont = Res.DefaultFont;

            // We don't owner draw menu items when in high contrast because it disabled some accessibility features
            // if(!SystemInformation.HighContrast && !BidiHelper.IsRightToLeft)
            // OwnerDraw = true;
        }

        /// <summary>
        ///     Occurs before the menu item is shown.
        /// </summary>
        [Category("Action")]
        [Description("Occurs before the menu item is shown.")]
        public event EventHandler BeforeShow;

        /// <summary>
        ///     Gets the menu type.
        /// </summary>
        public MenuType MenuType { get; }

        /// <summary>
        ///     Raises the BeforeShow event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        internal virtual void InvokeBeforeShow(EventArgs e)
        {
            this.OnBeforeShow(e);
        }

        /// <summary>
        ///     Menu text method.  Returns the text to draw for the owner draw menu item.  Override
        ///     this method in a derived class to return the appropriate text to draw based on the
        ///     draw item state of the MenuItem.
        /// </summary>
        /// <returns>Text to draw.</returns>
        protected virtual bool IncludeShortcutArea() => false;

        /// <summary>
        ///     Menu bitmap method.  Returns the bitmap to draw for the owner draw menu item.  Override
        ///     this method in a derived class to return the appropriate bitmap to draw based on the
        ///     draw item state of the MenuItem.
        /// </summary>
        /// <param name="drawItemState">The draw item state.</param>
        /// <returns>Bitmap to draw.</returns>
        protected virtual Bitmap MenuBitmap(DrawItemState drawItemState) => null;

        /// <summary>
        ///     Menu text method.  Returns the text to draw for the owner draw menu item.  Override
        ///     this method in a derived class to return the appropriate text to draw based on the
        ///     draw item state of the MenuItem.
        /// </summary>
        /// <returns>Text to draw.</returns>
        protected virtual string MenuText() => this.text;

        /// <summary>
        ///     Raises the BeforeShow event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnBeforeShow(EventArgs e) => this.BeforeShow?.Invoke(this, e);

        /// <summary>
        ///     Raises the DrawItem event.
        /// </summary>
        /// <param name="e">A DrawItemEventArgs that contains the event data.</param>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            // Call the base class's method.
            base.OnDrawItem(e);

            // Do the menu hack.
            this.MenuHack(e);

            // Draw the menu item.
            if (this.Parent is MainMenu)
            {
                this.DrawMainMenuItem(e);

                // Draw the offscreen bitmap.
                // BidiGraphics g = new BidiGraphics(e.Graphics, new Size(GetMainMenu().GetForm().Width, e.Bounds.Height));
                // g.DrawImage(false, offscreenBitmap, e.Bounds.Location);
            }
            else
            {
                // If we have an offscreen bitmap, and it's the wrong size, dispose of it.
                // Add try/catch due to lots of Watson-reported crashes here
                // on offscreenBitmap.Size ("object is in use elsewhere" yadda yadda)
                try
                {
                    if (this.offscreenBitmap != null && this.offscreenBitmap.Size != e.Bounds.Size)
                    {
                        this.offscreenBitmap.Dispose();
                        this.offscreenBitmap = null;
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail(ex.ToString());
                    this.offscreenBitmap = null;
                }

                // Allocate the offscreen bitmap, if we don't have one cached.
                if (this.offscreenBitmap == null)
                {
                    this.offscreenBitmap = new Bitmap(e.Bounds.Width, e.Bounds.Height);
                }

                this.DrawMenuItem(e.State, e.Bounds);

                // Draw the offscreen bitmap.
                var g = new BidiGraphics(e.Graphics, e.Bounds);

                // in the main menu drop downs, the highlight rectangle is off by one
                var location = e.Bounds.Location;
                if (Math.Abs(e.Graphics.VisibleClipBounds.X - (-1.0)) < float.Epsilon)
                {
                    location.X += 1;
                }

                g.DrawImage(false, this.offscreenBitmap, location);
            }
        }

        /// <summary>
        ///     Raises the DrawItem event.
        /// </summary>
        /// <param name="e">A MeasureItemEventArgs that contains the event data.</param>
        protected override void OnMeasureItem(MeasureItemEventArgs e)
        {
            // Call the base class's method.
            base.OnMeasureItem(e);

            // Measure the menu item.
            if (this.Parent is MainMenu)
            {
                // Measure main menu item.
                var textSize = this.MeasureMainMenuItemText(e.Graphics, this.MenuText());

                // Set the width and height.
                e.ItemWidth = textSize.Width + (OwnerDrawMenuItem.TOP_LEVEL_TEXT_PADDING * 2);
                e.ItemHeight = SystemInformation.MenuHeight;
            }
            else if (this.MenuType == MenuType.Context)
            {
                // Measure context menu item.
                var textSize = this.MeasureMenuItemText(e.Graphics, this.MenuText());

                // Set the width.
                e.ItemWidth = OwnerDrawMenuItem.STANDARD_BITMAP_AREA_WIDTH + OwnerDrawMenuItem.STANDARD_TEXT_PADDING
                                                                           + textSize.Width
                                                                           + OwnerDrawMenuItem.STANDARD_RIGHT_EDGE_PAD;

                // Set the height.
                e.ItemHeight = this.MenuText() == "-" ? 5 : SystemInformation.MenuHeight;
            }
            else
            {
                // Standard menu item.
                // Measure the menu text.
                var textSize = this.MeasureMenuItemText(e.Graphics, this.MenuText());

                // Determine the size of the shortcut.  If this item does not show a shortcut,
                // measure a default shortcut so it aligns with other menu entries.
                var shortcutSize = this.ShowShortcut && this.Shortcut != Shortcut.None
                    ? this.MeasureShortcutMenuItemText(
                        e.Graphics,
                        this.FormatShortcutString(this.Shortcut))
                    : this.MeasureShortcutMenuItemText(
                        e.Graphics,
                        this.FormatShortcutString(Shortcut.CtrlIns));

                // Set the width.
                e.ItemWidth = OwnerDrawMenuItem.STANDARD_BITMAP_AREA_WIDTH + OwnerDrawMenuItem.STANDARD_TEXT_PADDING
                                                                           + textSize.Width
                                                                           + OwnerDrawMenuItem.STANDARD_TEXT_PADDING
                                                                           + shortcutSize.Width
                                                                           + OwnerDrawMenuItem.STANDARD_RIGHT_EDGE_PAD;
                if (e.ItemWidth < OwnerDrawMenuItem.STANDARD_MINIMUM_WIDTH)
                {
                    e.ItemWidth = OwnerDrawMenuItem.STANDARD_MINIMUM_WIDTH;
                }

                // Set the height.
                e.ItemHeight = this.MenuText() == "-" ? 5 : SystemInformation.MenuHeight;
            }
        }

        /// <summary>
        ///     Raises the OnPopup event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnPopup(EventArgs e)
        {
            // Enumerate the menu items and raise the BeforeShow event on each.
            foreach (MenuItem menuItem in this.MenuItems)
            {
                if (menuItem is OwnerDrawMenuItem ownerDrawMenuItem)
                {
                    ownerDrawMenuItem.OnBeforeShow(EventArgs.Empty);
                }
            }

            // Call the base class's method so that registered delegates receive the event.
            base.OnPopup(e);
        }

        /// <summary>
        /// Draws the hotlight.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="highlightColor">Color of the highlight.</param>
        /// <param name="hotlightRectangle">The hotlight rectangle.</param>
        /// <param name="drawBackground">if set to <c>true</c> [draw background].</param>
        private static void DrawHotlight(
            BidiGraphics graphics,
            Color highlightColor,
            Rectangle hotlightRectangle,
            bool drawBackground)
        {
            // Fill the menu item with the system-defined menu color so the highlight color
            // looks right.
            if (drawBackground)
            {
                graphics.FillRectangle(SystemBrushes.Menu, hotlightRectangle);
            }

            // Draw the selection indicator using a 25% opaque version of the highlight color.
            using (var solidBrush = new SolidBrush(Color.FromArgb(64, highlightColor)))
            {
                graphics.FillRectangle(solidBrush, hotlightRectangle);
            }

            // Draw a rectangle around the selection indicator to frame it in better using a
            // 50% opaque version of the highlight color (this combines with the selection
            // indicator to be 75% opaque).
            using (var pen = new Pen(Color.FromArgb(128, highlightColor)))
            {
                graphics.DrawRectangle(
                    pen,
                    new Rectangle(
                        hotlightRectangle.X,
                        hotlightRectangle.Y,
                        hotlightRectangle.Width - 1,
                        hotlightRectangle.Height - 1));
            }
        }

        /// <summary>
        ///     Draws a top-level menu item.
        /// </summary>
        /// <param name="ea">A DrawItemEventArgs that contains the event data.</param>
        private void DrawMainMenuItem(DrawItemEventArgs ea)
        {
            // record state
            var drawItemState = ea.State;

            // Create graphics context on the offscreen bitmap and set the bounds for painting.
            // Graphics graphics = Graphics.FromImage(offscreenBitmap);
            var graphics = ea.Graphics;

            var g = new BidiGraphics(graphics, new Size(this.GetMainMenu().GetForm().Width, 0));

            // Rectangle bounds = new Rectangle(0, 0, offscreenBitmap.Width, offscreenBitmap.Height);
            var bounds = ea.Bounds;

            // get reference to colorized resources
            var cres = ColorizedResources.Instance;

            // Fill the menu item with the correct color
            var containingForm = this.GetMainMenu().GetForm();
            if (containingForm is IMainMenuBackgroundPainter)
            {
                (containingForm as IMainMenuBackgroundPainter).PaintBackground(
                    g.Graphics,
                    g.TranslateRectangle(ea.Bounds));
            }
            else
            {
                g.FillRectangle(SystemBrushes.Control, bounds);
            }

            // Draw the hotlight or selection rectangle.
            if ((drawItemState & DrawItemState.HotLight) != 0 || (drawItemState & DrawItemState.Selected) != 0)
            {
                // Cheat some for the first top-level menu item.  Provide a bit of "air" at the
                // left of the "HotLight" rectangle so it doesn't run into the frame.
                var xOffset = this.Index == 0 ? 1 : 0;

                // Calculate the hotlight rectangle.
                var hotlightRectangle = new Rectangle(
                    bounds.X + xOffset,
                    bounds.Y + 1,
                    bounds.Width - xOffset - 1,
                    bounds.Height - 1);
                OwnerDrawMenuItem.DrawHotlight(
                    g,
                    cres.MainMenuHighlightColor,
                    hotlightRectangle,
                    !cres.CustomMainMenuPainting);
            }

            // Calculate the text area rectangle.  This area excludes an area at the right
            // edge of the menu item where the system draws the cascade indicator.  It would
            // have been better if MenuItem let us draw the indicator (we did say "OwnerDraw"
            // after all), but this is just how it works.
            var textAreaRectangle = new Rectangle(bounds.X, bounds.Y + 1, bounds.Width, bounds.Height);

            // Determine which StringFormat to use when drawing the menu item text.
            var textFormat = (drawItemState & DrawItemState.NoAccelerator) != 0
                                 ? OwnerDrawMenuItem.mainMenuItemTextNoHotKeyStringFormat
                                 : OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat;

            // DisplayHelper.FixupGdiPlusLineCentering(graphics, menuFont, MenuText(), ref stringFormat, ref textAreaRectangle);

            // Draw the shortcut and the menu text.
            TextRenderer.DrawText(
                g.Graphics,
                this.MenuText(),
                this.menuFont,
                textAreaRectangle,
                cres.MainMenuTextColor,
                textFormat);

            // We're finished with the double buffered painting.  Dispose of the graphics context
            // and draw the offscreen image.  Cache the offscreen bitmap, though.
            graphics.Dispose();
        }

        /// <summary>
        ///     Draws a menu item.
        /// </summary>
        /// <param name="drawItemState">A DrawItemEventArgs that contains the event data.</param>
        /// <param name="rect">A DrawItemEventArgs that contains the client rectangle.</param>
        private void DrawMenuItem(DrawItemState drawItemState, Rectangle rect)
        {
            // Create graphics context on the offscreen bitmap and set the bounds for painting.
            var graphics = Graphics.FromImage(this.offscreenBitmap);
            graphics.CompositingMode = CompositingMode.SourceOver;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            var g = new BidiGraphics(graphics, new Rectangle(Point.Empty, this.offscreenBitmap.Size));
            try
            {
                var bounds = new Rectangle(0, 0, this.offscreenBitmap.Width, this.offscreenBitmap.Height);

                // Fill the menu item with the system-defined menu color.
                g.FillRectangle(SystemBrushes.Menu, bounds);

                // Fill the bitmap area with the system-defined control color.
                var bitmapAreaRectangle = new Rectangle(
                    bounds.X,
                    bounds.Y,
                    OwnerDrawMenuItem.STANDARD_BITMAP_AREA_WIDTH,
                    bounds.Height);

                // Fill the background.
                /*
                using (SolidBrush solidBrush = new SolidBrush(ApplicationManager.ApplicationStyle.MenuBitmapAreaColor))
                    graphics.FillRectangle(solidBrush, bitmapAreaRectangle);
                */
                var backgroundColor = SystemColors.Menu;

                // If the item is selected, draw the selection rectangle.
                if ((drawItemState & DrawItemState.Selected) != 0)
                {
                    OwnerDrawMenuItem.DrawHotlight(g, SystemColors.Highlight, bounds, true);
                    backgroundColor = this.offscreenBitmap.GetPixel(2, 2);
                }

                // Obtain the bitmap to draw.  If there is one, draw it centered in the bitmap area.
                var bitmap = this.MenuBitmap(drawItemState);
                if (bitmap != null)
                {
                    g.DrawImage(
                        false,
                        bitmap,
                        new Point(
                            bounds.X + Utility.CenterMinZero(bitmap.Width, bitmapAreaRectangle.Width),
                            bounds.Y + Utility.CenterMinZero(bitmap.Height, bitmapAreaRectangle.Height)));
                }

                // Obtain the menu text.  If it's not "-", then this is a menu item.  Otherwise, it's
                // a separator menu item.
                if (this.MenuText() != "-")
                {
                    // Calculate the text area rectangle.  This area excludes an area at the right
                    // edge of the menu item where the system draws the cascade indicator.  It would
                    // have been better if MenuItem let us draw the indicator (we did say "OwnerDraw"
                    // after all), but this is just how it works.
                    var textAreaRectangle = new Rectangle(
                        bounds.X + OwnerDrawMenuItem.STANDARD_BITMAP_AREA_WIDTH
                                 + OwnerDrawMenuItem.STANDARD_TEXT_PADDING,
                        bounds.Y,
                        bounds.Width - (OwnerDrawMenuItem.STANDARD_BITMAP_AREA_WIDTH
                                      + OwnerDrawMenuItem.STANDARD_TEXT_PADDING
                                      + OwnerDrawMenuItem.STANDARD_RIGHT_EDGE_PAD),
                        bounds.Height);

                    // Select the brush to draw the menu text with.
                    var color = (drawItemState & DrawItemState.Disabled) == 0 ? SystemColors.MenuText : SystemColors.GrayText;

                    // Determine the size of the shortcut, if it is being shown.
                    if (this.MenuType != MenuType.Context)
                    {
                        string shortcut;
                        Size shortcutSize;
                        if (this.ShowShortcut && this.Shortcut != Shortcut.None)
                        {
                            shortcut = this.FormatShortcutString(this.Shortcut);
                            shortcutSize = this.MeasureShortcutMenuItemText(graphics, shortcut);
                        }
                        else
                        {
                            shortcut = null;
                            shortcutSize = this.MeasureShortcutMenuItemText(
                                graphics,
                                this.FormatShortcutString(Shortcut.CtrlIns));
                        }

                        // Draw the shortcut.
                        if (shortcut != null)
                        {
                            var shortcutTextRect = textAreaRectangle;
                            var textFormatTemp = OwnerDrawMenuItem.shortcutStringFormat;

                            // DisplayHelper.FixupGdiPlusLineCentering(graphics, menuFont, shortcut, ref stringFormatTemp, ref shortcutTextRect);
                            // Draw the shortcut text.
                            g.DrawText(
                                shortcut,
                                this.menuFont,
                                shortcutTextRect,
                                color,
                                backgroundColor,
                                textFormatTemp);
                        }

                        // Reduce the width of the text area rectangle to account for the shortcut and
                        // the padding before it.
                        textAreaRectangle.Width -= shortcutSize.Width + OwnerDrawMenuItem.STANDARD_TEXT_PADDING;
                    }

                    // Determine which StringFormat to use when drawing the menu item text.
                    var textFormat = (drawItemState & DrawItemState.NoAccelerator) != 0
                                         ? OwnerDrawMenuItem.menuItemTextNoHotKeyStringFormat
                                         : OwnerDrawMenuItem.menuItemTextHotKeyStringFormat;

                    // DisplayHelper.FixupGdiPlusLineCentering(graphics, menuFont, MenuText(), ref stringFormat, ref textAreaRectangle);
                    // Draw the text.
                    g.DrawText(this.MenuText(), this.menuFont, textAreaRectangle, color, backgroundColor, textFormat);
                }
                else
                {
                    // Calculate the separator line rectangle.  This area excludes an area at the right
                    // edge of the menu item where the system draws the cascade indicator.  It would
                    // have been better if MenuItem let us draw the indicator (we did say "OwnerDraw"
                    // after all), but this is just how it works.
                    var separatorLineRectangle = new Rectangle(
                        bounds.X + OwnerDrawMenuItem.STANDARD_SEPARATOR_PADDING,
                        bounds.Y + Utility.CenterMinZero(1, bounds.Height),
                        bounds.Width - OwnerDrawMenuItem.STANDARD_SEPARATOR_PADDING,
                        1);

                    // Fill the separator line rectangle.
                    g.FillRectangle(SystemBrushes.ControlDark, separatorLineRectangle);
                }
            }
            finally
            {
                // We're finished with the double buffered painting.  Dispose of the graphics context
                // and draw the offscreen image.  Cache the offscreen bitmap, though.
                graphics.Dispose();
            }
        }

        /// <summary>
        ///     Format a shortcut as a string.
        /// </summary>
        /// <param name="shortcut">Shortcut to format.</param>
        /// <returns>String format of shortcut.</returns>
        private string FormatShortcutString(Shortcut shortcut) => KeyboardHelper.FormatShortcutString(shortcut);

        /// <summary>
        ///     Measures the size of main menu item text.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="theText">Text to measure.</param>
        /// <returns>Size of the text in pixels.</returns>
        private Size MeasureMainMenuItemText(IDeviceContext graphics, string theText)
        {
            // Measure the text.
            var size = Size.Empty;
            if (!string.IsNullOrEmpty(theText))
            {
                size = TextRenderer.MeasureText(
                    graphics,
                    theText,
                    this.menuFont,
                    new Size(OwnerDrawMenuItem.STANDARD_MAXIMUM_TEXT_WIDTH, this.menuFont.Height),
                    OwnerDrawMenuItem.mainMenuItemTextHotKeyStringFormat);
            }

            return size;
        }

        /// <summary>
        ///     Measures the size of menu item text.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="theText">Text to measure.</param>
        /// <returns>Size of the text in pixels.</returns>
        private Size MeasureMenuItemText(Graphics graphics, string theText)
        {
            // Measure the text.
            var size = Size.Empty;
            if (!string.IsNullOrEmpty(theText))
            {
                size = TextRenderer.MeasureText(
                    graphics,
                    theText,
                    this.menuFont,
                    new Size(OwnerDrawMenuItem.STANDARD_MAXIMUM_TEXT_WIDTH, this.menuFont.Height),
                    OwnerDrawMenuItem.menuItemTextHotKeyStringFormat);
            }

            return size;
        }

        /// <summary>
        ///     Measures the shortcut size of shortcut text.
        /// </summary>
        /// <param name="graphics">Graphics context in which to perform the measurement.</param>
        /// <param name="shortcut">Text to measure.</param>
        /// <returns>Size of the shortcut text in pixels.</returns>
        private Size MeasureShortcutMenuItemText(Graphics graphics, string shortcut)
        {
            // Measure the shortcut.
            var size = Size.Empty;
            if (!string.IsNullOrEmpty(shortcut))
            {
                size = TextRenderer.MeasureText(
                    graphics,
                    shortcut,
                    this.menuFont,
                    new Size(1000, this.menuFont.Height),
                    OwnerDrawMenuItem.shortcutStringFormat);
            }

            return size;
        }

        /// <summary>
        ///     Menu hack.
        /// </summary>
        /// <param name="e">A DrawItemEventArgs that contains the event data.</param>
        private void MenuHack(DrawItemEventArgs e)
        {
            // We start off with a bit of a hack.  The explanation is long.  There is a paint bug
            // in the .NET MenuItem that manifests itself when:
            // 1) The OwnerDraw property is true.
            // 2) The OnDrawImage draws an image over the entire Bounds of the menu item as
            //    specified in DrawItemEventArgs (in our case, this is true because we're
            //    painting using double buffering, but it would be true for any other sort
            //    of image).
            // 3) The OnDrawItem is being called to redraw a menu item as a result of a
            //    cascading menu being removed.
            // When these conditions are true, .NET inexplicably leaves a turd on the screen.
            // It's really amazing.  Anyway, the ONLY fix is to clean up the turd or not draw an
            // image.  We needed double buffer
            if (this.Parent is MainMenu)
            {
                return;
            }

            using (var solidBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control)))
            {
                // The turd rectangle.  Roughly 10 pixels at the right edge of the bounds of the
                // menu item.
                var turdRectangle = new Rectangle(
                    e.Bounds.X + (e.Bounds.Width - 10),
                    e.Bounds.Y,
                    10,
                    e.Bounds.Height);

                // Fill in the turd rectangle with the System.Drawing.KnownColor.Menu color, and
                // this masks the bug.
                e.Graphics.FillRectangle(SystemBrushes.Menu, turdRectangle);
            }
        }
    }
}
