// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Text;
    using System.Windows.Forms;

    using ApplicationFramework;

    using Localization;
    using Localization.Bidi;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The LetterCommand class.
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.OverridableCommand" />
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.CommandBarButtonLightweightControl.ICustomButtonBitmapPaint" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.OverridableCommand" />
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.CommandBarButtonLightweightControl.ICustomButtonBitmapPaint" />
        internal class LetterCommand : OverridableCommand, CommandBarButtonLightweightControl.ICustomButtonBitmapPaint
        {
            /// <summary>
            /// The assert on font family failure
            /// </summary>
            private static bool assertOnFontFamilyFailure = true;

            /// <summary>
            /// The font style
            /// </summary>
            private readonly FontStyle fontStyle;

            /// <summary>
            /// The letter
            /// </summary>
            private readonly char letter;

            /// <summary>
            /// Initializes a new instance of the <see cref="LetterCommand"/> class.
            /// </summary>
            /// <param name="commandId">The command identifier.</param>
            /// <param name="letter">The letter.</param>
            /// <param name="fontStyle">The font style.</param>
            public LetterCommand(CommandId commandId, char letter, FontStyle fontStyle)
                : base(commandId)
            {
                this.letter = letter;
                this.fontStyle = fontStyle;
            }

            /// <inheritdoc />
            public int Width => 16;

            /// <inheritdoc />
            public int Height => 16;

            /// <inheritdoc />
            public void Paint(BidiGraphics g, Rectangle bounds, CommandBarButtonLightweightControl.DrawState drawState)
            {
                var fontFamily = new FontFamily(GenericFontFamilies.Serif);
                try
                {
                    var fontFamilyStr = Res.Get(StringId.ToolbarFontStyleFontFamily);
                    if (!string.IsNullOrEmpty(fontFamilyStr))
                    {
                        fontFamily = new FontFamily(fontFamilyStr);
                    }
                }
                catch (Exception e)
                {
                    if (LetterCommand.assertOnFontFamilyFailure)
                    {
                        LetterCommand.assertOnFontFamilyFailure = false;
                        Trace.WriteLine("Failed to load font family: " + e);
                    }
                }

                using (var f = new Font(fontFamily, Res.ToolbarFormatButtonFontSize, FontStyle.Bold | this.fontStyle,
                                        GraphicsUnit.Pixel, 0))
                {
                    // Note: no high contrast mode support here
                    Color color;
                    if (!SystemInformation.HighContrast)
                    {
                        color = Color.FromArgb(54, 73, 98);
                        if (drawState == CommandBarButtonLightweightControl.DrawState.Disabled)
                        {
                            color = Color.FromArgb(202, 202, 202);
                        }
                    }
                    else
                    {
                        color = drawState == CommandBarButtonLightweightControl.DrawState.Disabled
                                    ? SystemColors.GrayText
                                    : SystemColors.WindowText;
                    }

                    bounds.Y -= 1;
                    g.DrawText($"{this.letter}", f, bounds, color,
                               TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                               TextFormatFlags.PreserveGraphicsTranslateTransform | TextFormatFlags.NoPadding |
                               TextFormatFlags.NoClipping);
                }
            }
        }
    }
}
