// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using OpenLiveWriter.Interop.Com.Ribbon;

    /// <summary>
    /// Interface IFontControlCommand
    /// </summary>
    public interface IFontControlCommand
    {
        /// <summary>
        /// Gets the font family.
        /// </summary>
        /// <value>The font family.</value>
        string FontFamily { get; }

        /// <summary>
        /// Gets the size of the font.
        /// </summary>
        /// <value>The size of the font.</value>
        int FontSize { get; }

        /// <summary>
        /// Gets the bold.
        /// </summary>
        /// <value>The bold.</value>
        FontProperties Bold { get; }

        /// <summary>
        /// Gets the italic.
        /// </summary>
        /// <value>The italic.</value>
        FontProperties Italic { get; }

        /// <summary>
        /// Gets the underline.
        /// </summary>
        /// <value>The underline.</value>
        FontProperties Underline { get; }

        /// <summary>
        /// Gets the strikethrough.
        /// </summary>
        /// <value>The strikethrough.</value>
        FontProperties Strikethrough { get; }

        // @RIBBON TODO: Implement vertical positioning
        //FontPropertiesVerticalPositioning

        /// <summary>
        /// Gets the color of the foreground.
        /// </summary>
        /// <value>The color of the foreground.</value>
        int ForegroundColor { get; }

        /// <summary>
        /// Gets the color of the background.
        /// </summary>
        /// <value>The color of the background.</value>
        int BackgroundColor { get; }
    }
}
