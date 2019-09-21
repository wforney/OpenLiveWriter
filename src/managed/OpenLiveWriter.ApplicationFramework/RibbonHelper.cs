// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Diagnostics;
    using System.Drawing;

    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Localization;

    /// <summary>
    ///     Class RibbonHelper.
    /// </summary>
    public class RibbonHelper
    {
        /// <summary>
        ///     The gallery item text maximum chars
        /// </summary>
        public const int GalleryItemTextMaxChars = 20;

        /// <summary>
        ///     The gallery item text maximum width in pixels
        /// </summary>
        public const int GalleryItemTextMaxWidthInPixels = 120;

        /// <summary>
        ///     The in gallery image height
        /// </summary>
        public const int InGalleryImageHeight = 48;

        /// <summary>
        ///     The in gallery image height without label
        /// </summary>
        public const int InGalleryImageHeightWithoutLabel = 36;

        /// <summary>
        ///     The in gallery image width
        /// </summary>
        public const int InGalleryImageWidth = 64;

        /// <summary>
        ///     The image from bitmap
        /// </summary>
        [ThreadStatic]
        private static IUIImageFromBitmap imageFromBitmap;

        /// <summary>
        ///     Creates the image.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="options">The options.</param>
        /// <returns>An <see cref="IUIImage"/>.</returns>
        public static IUIImage CreateImage(IntPtr bitmap, ImageCreationOptions options)
        {
            if (RibbonHelper.imageFromBitmap == null)
            {
                RibbonHelper.imageFromBitmap = (IUIImageFromBitmap)new UIRibbonImageFromBitmapFactory();
            }

            return RibbonHelper.imageFromBitmap.CreateImage(bitmap, options);
        }

        /// <summary>
        ///     Creates the image property variant.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="pv">The property variant.</param>
        public static void CreateImagePropVariant(Image image, out PropVariant pv)
        {
            try
            {
                pv = new PropVariant();

                // Note that the missing image is a 32x32 png.
                if (image == null)
                {
                    image = Images.Missing_LargeImage;
                }

                pv.SetIUnknown(RibbonHelper.CreateImage(((Bitmap)image).GetHbitmap(), ImageCreationOptions.Transfer));
            }
            catch (Exception ex)
            {
                Trace.Fail("Failed to create image PropVariant: " + ex);
                pv = new PropVariant();
            }
        }

        /// <summary>
        ///     Gets the gallery item image from command.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        /// <param name="commandId">The command identifier.</param>
        /// <returns>A <see cref="Bitmap"/>.</returns>
        public static Bitmap GetGalleryItemImageFromCommand(CommandManager commandManager, CommandId commandId)
        {
            // @RIBBON TODO: Deal with high contrast appropriately
            var command = commandManager.Get(commandId);
            return command == null ? Images.Missing_LargeImage : command.LargeImage;
        }

        /// <summary>
        ///     Determines whether [is string property key] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if [is string property key] [the specified key]; otherwise, <c>false</c>.</returns>
        public static bool IsStringPropertyKey(PropertyKey key) =>
            key == PropertyKeys.Label || key == PropertyKeys.LabelDescription || key == PropertyKeys.Keytip
         || key == PropertyKeys.TooltipTitle || key == PropertyKeys.TooltipDescription;
    }
}
