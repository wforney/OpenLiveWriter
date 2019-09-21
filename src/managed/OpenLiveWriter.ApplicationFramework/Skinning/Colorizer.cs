// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Skinning
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Class Colorizer.
    /// </summary>
    public class Colorizer
    {
        /// <summary>
        /// Colorizes the RGB.
        /// </summary>
        /// <param name="crOld">The old color.</param>
        /// <param name="crColorize">The colorize color.</param>
        /// <param name="wScale">The width scale.</param>
        /// <returns>The Color.</returns>
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

            var wLumOld = (Math.Max(crOld.R, Math.Max(crOld.G, crOld.B)) +
                Math.Min(crOld.R, Math.Min(crOld.G, crOld.B))) / 2;

            if (wScale < wLumOld)
            {
                var rColorizeHi = (byte)(255 - crColorize.R);
                var gColorizeHi = (byte)(255 - crColorize.G);
                var bColorizeHi = (byte)(255 - crColorize.B);
                var wScaleHi = 255 - wScale;

                wLumOld = wLumOld - wScale;
                var rNew = (byte)(crColorize.R + (byte)(rColorizeHi * wLumOld / wScaleHi));
                var gNew = (byte)(crColorize.G + (byte)(gColorizeHi * wLumOld / wScaleHi));
                var bNew = (byte)(crColorize.B + (byte)(bColorizeHi * wLumOld / wScaleHi));

                return Color.FromArgb(rNew, gNew, bNew);
            }
            else
            {
                var rNew = (byte)(crColorize.R * wLumOld / wScale);
                var gNew = (byte)(crColorize.G * wLumOld / wScale);
                var bNew = (byte)(crColorize.B * wLumOld / wScale);

                return Color.FromArgb(rNew, gNew, bNew);
            }
        }

        /// <summary>
        /// Colorizes the ARGB.
        /// </summary>
        /// <param name="crOld">The cr old.</param>
        /// <param name="crColorize">The cr colorize.</param>
        /// <param name="wScale">The w scale.</param>
        /// <returns>Color.</returns>
        public static Color ColorizeARGB(Color crOld, Color crColorize, int wScale)
        {
            if (Color.Empty == crColorize)
            {
                return crOld;
            }

            int rNew = crColorize.R;
            int gNew = crColorize.G;
            int bNew = crColorize.B;
            var rNewHi = 255 - rNew;
            var gNewHi = 255 - gNew;
            var bNewHi = 255 - bNew;
            var wScaleHi = 255 - wScale;

            // the pixel RGB has been prescaled with alpha
            // we need to multiply the scale point and the new color by alpha as well
            var wLumAlpha = (Math.Max(crOld.R, Math.Max(crOld.G, crOld.B)) +
                Math.Min(crOld.R, Math.Min(crOld.G, crOld.B))) / 2;

            var wScaleAlpha = wScale * crOld.A / 255;
            if (wScaleAlpha < wLumAlpha)
            {
                wLumAlpha = wLumAlpha - wScaleAlpha;

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

        /// <summary>
        /// Colorizes the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="crColorize">The cr colorize.</param>
        /// <param name="wScale">The w scale.</param>
        /// <returns>Bitmap.</returns>
        public static Bitmap ColorizeBitmap(Bitmap bitmap, Color crColorize, int wScale) => ColorizeBitmap(bitmap, crColorize, wScale, new Rectangle(0, 0, bitmap.Width, bitmap.Height));

        /// <summary>
        /// Colorizes the bitmap.
        /// </summary>
        /// <param name="bitmap">The bitmap.</param>
        /// <param name="crColorize">The cr colorize.</param>
        /// <param name="wScale">The w scale.</param>
        /// <param name="colorizeRect">The colorize rect.</param>
        /// <returns>Bitmap.</returns>
        public static Bitmap ColorizeBitmap(Bitmap bitmap, Color crColorize, int wScale, Rectangle colorizeRect)
        {
            if (crColorize == Color.Empty)
            {
                return new Bitmap(bitmap);
            }

            var newBitmap = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    if (colorizeRect.Contains(x, y))
                    {
                        newBitmap.SetPixel(x, y, ColorizeARGB(bitmap.GetPixel(x, y), crColorize, wScale));
                    }
                    else
                    {
                        newBitmap.SetPixel(x, y, bitmap.GetPixel(x, y));
                    }
                }
            }

            return newBitmap;
        }
    }
}
