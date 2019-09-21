// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using OpenLiveWriter.Localization.Bidi;

    public partial class CommandBarButtonLightweightControl
    {
        /// <summary>
        /// Commands can implement this interface to provide
        /// dynamic command bar button images.
        /// </summary>
        public interface ICustomButtonBitmapPaint
        {
            /// <summary>
            /// Gets the width.
            /// </summary>
            /// <value>The width.</value>
            int Width { get; }

            /// <summary>
            /// Gets the height.
            /// </summary>
            /// <value>The height.</value>
            int Height { get; }

            /// <summary>
            /// Paints the specified g.
            /// </summary>
            /// <param name="g">The g.</param>
            /// <param name="bounds">The bounds.</param>
            /// <param name="drawState">State of the draw.</param>
            void Paint(BidiGraphics g, Rectangle bounds, DrawState drawState);
        }
    }
}
