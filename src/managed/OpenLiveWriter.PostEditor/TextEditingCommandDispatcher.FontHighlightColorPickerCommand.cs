// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Drawing;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The FontHighlightColorPickerCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.FontColorPickerCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.FontColorPickerCommand" />
        private class FontHighlightColorPickerCommand : FontColorPickerCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FontHighlightColorPickerCommand"/> class.
            /// </summary>
            /// <param name="color">The color.</param>
            internal FontHighlightColorPickerCommand(Color color)
                : base(CommandId.FontBackgroundColor, color)
            {
            }

            /// <inheritdoc />
            public override Bitmap SmallImage
            {
                get
                {
                    var bitmap = base.SmallImage;
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        using (var brush = new SolidBrush(this.SelectedColor))
                        {
                            g.FillRectangle(brush, 0, 12, 16, 4);
                        }
                    }

                    return bitmap;
                }
                set => base.SmallImage = value;
            }
        }
    }
}
