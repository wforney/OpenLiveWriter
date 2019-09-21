// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using CoreServices;
    using Localization.Bidi;

    public partial class SystemButtonHelper
    {
        public const int SMALL_BUTTON_IMAGE_SIZE = 16;
        public const int LARGE_BUTTON_IMAGE_SIZE = 38;
        public const int LARGE_BUTTON_TOTAL_SIZE = 42;
        private static readonly ButtonSettings DefaultButtonSettings = new ButtonSettings();

        /// <summary>
        ///     Left button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceLeftRectangle = new Rectangle(0, 0, 3, 24);

        /// <summary>
        ///     Center button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceCenterRectangle = new Rectangle(3, 0, 1, 24);

        /// <summary>
        ///     Right button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceRightRectangle = new Rectangle(4, 0, 3, 24);

        /// <summary>
        ///     Left button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceLeftRectangleLarge = new Rectangle(0, 0, 4, 42);

        /// <summary>
        ///     Center button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceCenterRectangleLarge = new Rectangle(4, 0, 1, 42);

        /// <summary>
        ///     Right button face image rectangle.
        /// </summary>
        private static readonly Rectangle buttonFaceRightRectangleLarge = new Rectangle(5, 0, 4, 42);

        /// <summary>
        ///     Button face.
        /// </summary>
        private static Bitmap buttonFaceBitmap =>
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonHover.png");

        /// <summary>
        ///     Pushed button face.
        /// </summary>
        private static Bitmap buttonFacePushedBitmap =>
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonPressed.png");

        /// <summary>
        ///     Button face.
        /// </summary>
        private static Bitmap buttonFaceBitmapLarge =>
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonHoverLarge.png");

        /// <summary>
        ///     Pushed button face.
        /// </summary>
        private static Bitmap buttonFacePushedBitmapLarge =>
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ToolbarButtonPressedLarge.png");

        /// <summary>
        ///     Draw the system button face.
        /// </summary>
        /// <param name="graphics">Graphics context in which the system button face is to be drawn.</param>
        public static void DrawSystemButtonFace(BidiGraphics graphics, bool DropDownContextMenuUserInterface,
                                                bool contextMenuShowing, Rectangle clientRectangle, bool isLargeButton)
        {
            SystemButtonHelper.DrawSystemButtonFace(graphics, DropDownContextMenuUserInterface, contextMenuShowing,
                                                    clientRectangle, isLargeButton,
                                                    SystemButtonHelper.DefaultButtonSettings);
        }

        private static void DrawSystemButtonFace(BidiGraphics graphics, bool DropDownContextMenuUserInterface,
                                                 bool contextMenuShowing, Rectangle VirtualClientRectangle,
                                                 bool isLargeButton, ButtonSettings settings)
        {
            // calculate bitmaps
            var hoverButtonFace = isLargeButton
                                      ? SystemButtonHelper.buttonFaceBitmapLarge
                                      : SystemButtonHelper.buttonFaceBitmap;
            var pressedButtonFace = isLargeButton
                                        ? SystemButtonHelper.buttonFacePushedBitmapLarge
                                        : SystemButtonHelper.buttonFacePushedBitmap;

            // draw button face
            SystemButtonHelper.DrawSystemButtonFace(graphics,
                                                    DropDownContextMenuUserInterface,
                                                    VirtualClientRectangle,
                                                    hoverButtonFace,
                                                    contextMenuShowing ? pressedButtonFace : hoverButtonFace,
                                                    isLargeButton,
                                                    settings);
        }

        /// <summary>
        ///     Draw system button face in the pushed state.
        /// </summary>
        /// <param name="graphics">Graphics context in which the system button face is to be drawn in the pushed state.</param>
        public static void DrawSystemButtonFacePushed(BidiGraphics graphics, bool DropDownContextMenuUserInterface,
                                                      Rectangle clientRectangle, bool isLargeButton)
        {
            SystemButtonHelper.DrawSystemButtonFacePushed(graphics, DropDownContextMenuUserInterface, clientRectangle,
                                                          isLargeButton, SystemButtonHelper.DefaultButtonSettings);
        }

        private static void DrawSystemButtonFacePushed(BidiGraphics graphics, bool DropDownContextMenuUserInterface,
                                                       Rectangle clientRectangle, bool isLargeButton,
                                                       ButtonSettings settings)
        {
            var pressedButtonFace = isLargeButton
                                        ? SystemButtonHelper.buttonFacePushedBitmapLarge
                                        : SystemButtonHelper.buttonFacePushedBitmap;
            SystemButtonHelper.DrawSystemButtonFace(graphics, DropDownContextMenuUserInterface, clientRectangle,
                                                    pressedButtonFace, pressedButtonFace, isLargeButton, settings);
        }

        private static void DrawSystemButtonFace(BidiGraphics graphics, bool DropDownContextMenuUserInterface,
                                                 Rectangle clientRectangle, Image buttonBitmap,
                                                 Image contextMenuButtonBitmap, bool isLargeButton,
                                                 ButtonSettings settings)
        {
            // get rectangles
            var leftRectangle = isLargeButton
                                    ? SystemButtonHelper.buttonFaceLeftRectangleLarge
                                    : SystemButtonHelper.buttonFaceLeftRectangle;
            var centerRectangle = isLargeButton
                                      ? SystemButtonHelper.buttonFaceCenterRectangleLarge
                                      : SystemButtonHelper.buttonFaceCenterRectangle;
            var rightRectangle = isLargeButton
                                     ? SystemButtonHelper.buttonFaceRightRectangleLarge
                                     : SystemButtonHelper.buttonFaceRightRectangle;

            // determine height
            var height = isLargeButton ? SystemButtonHelper.LARGE_BUTTON_TOTAL_SIZE : clientRectangle.Height;

            //	Compute the button face rectangle.
            var buttonRectangle = new Rectangle(clientRectangle.X,
                                                clientRectangle.Y,
                                                clientRectangle.Width,
                                                height);

            if (DropDownContextMenuUserInterface)
            {
                //	Compute the drop-down rectangle.
                var dropDownRectangle = new Rectangle(clientRectangle.Right - settings.DROP_DOWN_BUTTON_WIDTH,
                                                      clientRectangle.Y,
                                                      settings.DROP_DOWN_BUTTON_WIDTH,
                                                      height);
                GraphicsHelper.DrawLeftCenterRightImageBorder(graphics,
                                                              dropDownRectangle,
                                                              contextMenuButtonBitmap,
                                                              leftRectangle,
                                                              centerRectangle,
                                                              rightRectangle);

                buttonRectangle.Width -= dropDownRectangle.Width - 1;
            }

            //	Draw the border.
            GraphicsHelper.DrawLeftCenterRightImageBorder(graphics,
                                                          buttonRectangle,
                                                          buttonBitmap,
                                                          leftRectangle,
                                                          centerRectangle,
                                                          rightRectangle);
        }
    }
}