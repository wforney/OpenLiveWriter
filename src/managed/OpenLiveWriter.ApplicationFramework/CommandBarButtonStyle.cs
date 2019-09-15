// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Specifies the style of command bar buttons.
    /// </summary>
    public enum CommandBarButtonStyle
    {
        /// <summary>
        /// Command bar button is drawn by the system.  System command bar buttons can have text.
        /// </summary>
        System,

        /// <summary>
        /// Command bar button is drawn from the command bar button bitmaps.  Bitmap command bar
        /// buttons do not have text and are sized to match the command bar buttons.
        /// </summary>
        Bitmap,

        /// <summary>
        /// Special style used for blog provider buttons
        /// </summary>
        Provider
    }
}
