// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;

    /// <summary>Interface IMainMenuBackgroundPainter</summary>
    internal interface IMainMenuBackgroundPainter
    {
        /// <summary>Paints the background.</summary>
        /// <param name="g">The graphics.</param>
        /// <param name="menuItemBounds">The menu item bounds.</param>
        void PaintBackground(Graphics g, Rectangle menuItemBounds);
    }
}
