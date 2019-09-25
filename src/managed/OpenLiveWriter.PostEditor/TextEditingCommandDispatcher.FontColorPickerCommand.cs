// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;

    using ApplicationFramework;

    using Commands;

    using CoreServices;

    using Interop.Com;
    using Interop.Com.Ribbon;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// This command corresponds to a ColorPickerDropDown in ribbon markup.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.Commands.PreviewCommand" />
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IColorPickerCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.Commands.PreviewCommand" />
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.IColorPickerCommand" />
        private class FontColorPickerCommand : PreviewCommand, IColorPickerCommand
        {
            /// <summary>
            /// The number standard colors rows
            /// </summary>
            /// <remarks>
            /// Note: These numbers need to match the corresponding attributes in the ribbon markup.
            /// </remarks>
            private const uint NumberOfStandardColorsRows = 6;

            /// <summary>
            /// The number columns
            /// </summary>
            /// <remarks>
            /// Note: These numbers need to match the corresponding attributes in the ribbon markup.
            /// </remarks>
            private const uint NumberOfColumns = 5;

            /// <summary>
            /// The saved selected color
            /// </summary>
            private Color savedSelectedColor;

            /// <summary>
            /// The saved selected color type
            /// </summary>
            private SwatchColorType savedSelectedColorType;

            /// <summary>
            /// The standard colors
            /// </summary>
            private readonly ColorPickerColor[] standardColors =
            {
                new ColorPickerColor(Color.FromArgb(255, 255, 255), StringId.ColorWhite),
                new ColorPickerColor(Color.FromArgb(255, 0, 0), StringId.ColorVibrantRed),
                new ColorPickerColor(Color.FromArgb(192, 80, 77), StringId.ColorProfessionalRed),
                new ColorPickerColor(Color.FromArgb(209, 99, 73), StringId.ColorEarthyRed),
                new ColorPickerColor(Color.FromArgb(221, 132, 132), StringId.ColorPastelRed),

                new ColorPickerColor(Color.FromArgb(204, 204, 204), StringId.ColorLightGray),
                new ColorPickerColor(Color.FromArgb(255, 192, 0), StringId.ColorVibrantOrange),
                new ColorPickerColor(Color.FromArgb(247, 150, 70), StringId.ColorProfessionalOrange),
                new ColorPickerColor(Color.FromArgb(209, 144, 73), StringId.ColorEarthyOrange),
                new ColorPickerColor(Color.FromArgb(243, 164, 71), StringId.ColorPastelOrange),

                new ColorPickerColor(Color.FromArgb(165, 165, 165), StringId.ColorMediumGray),
                new ColorPickerColor(Color.FromArgb(255, 255, 0), StringId.ColorVibrantYellow),
                new ColorPickerColor(Color.FromArgb(155, 187, 89), StringId.ColorProfessionalGreen),
                new ColorPickerColor(Color.FromArgb(204, 180, 0), StringId.ColorEarthyYellow),
                new ColorPickerColor(Color.FromArgb(223, 206, 4), StringId.ColorPastelYellow),

                new ColorPickerColor(Color.FromArgb(102, 102, 102), StringId.ColorDarkGray),
                new ColorPickerColor(Color.FromArgb(0, 255, 0), StringId.ColorVibrantGreen),
                new ColorPickerColor(Color.FromArgb(75, 172, 198), StringId.ColorProfessionalAqua),
                new ColorPickerColor(Color.FromArgb(143, 176, 140), StringId.ColorEarthyGreen),
                new ColorPickerColor(Color.FromArgb(165, 181, 146), StringId.ColorPastelGreen),

                new ColorPickerColor(Color.FromArgb(51, 51, 51), StringId.ColorCharcoal),
                new ColorPickerColor(Color.FromArgb(0, 0, 255), StringId.ColorVibrantBlue),
                new ColorPickerColor(Color.FromArgb(79, 129, 189), StringId.ColorProfessionalBlue),
                new ColorPickerColor(Color.FromArgb(100, 107, 134), StringId.ColorEarthyBlue),
                new ColorPickerColor(Color.FromArgb(128, 158, 194), StringId.ColorPastelBlue),

                new ColorPickerColor(Color.FromArgb(0, 0, 0), StringId.ColorBlack),
                new ColorPickerColor(Color.FromArgb(155, 0, 211), StringId.ColorVibrantPurple),
                new ColorPickerColor(Color.FromArgb(128, 100, 162), StringId.ColorProfessionalPurple),
                new ColorPickerColor(Color.FromArgb(158, 124, 124), StringId.ColorEarthyBrown),
                new ColorPickerColor(Color.FromArgb(156, 133, 192), StringId.ColorPastelPurple)
            };

            /// <summary>
            /// Initializes a new instance of the <see cref="FontColorPickerCommand"/> class.
            /// </summary>
            /// <param name="commandId">The command identifier.</param>
            /// <param name="color">The color.</param>
            public FontColorPickerCommand(CommandId commandId, Color color)
                : base(commandId)
            {
                this.SelectedColorType = SwatchColorType.RGB;
                this.SelectedColor = color;

                this.OnStartPreview += this.FontColorPickerCommand_OnStartPreview;
                this.OnCancelPreview += this.FontColorPickerCommand_OnCancelPreview;

                this.UpdateInvalidationState(PropertyKeys.Color, InvalidationState.Pending);
                this.UpdateInvalidationState(PropertyKeys.ColorType, InvalidationState.Pending);
            }

            /// <summary>
            /// Handles the OnStartPreview event of the FontColorPickerCommand control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void FontColorPickerCommand_OnStartPreview(object sender, EventArgs e)
            {
                this.savedSelectedColorType = this.SelectedColorType;
                this.savedSelectedColor = this.SelectedColor;
            }

            /// <summary>
            /// Handles the OnCancelPreview event of the FontColorPickerCommand control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void FontColorPickerCommand_OnCancelPreview(object sender, EventArgs e) =>
                this.SetSelectedColor(this.savedSelectedColor, this.savedSelectedColorType);

            /// <inheritdoc />
            public override int PerformExecute(
                CommandExecutionVerb verb,
                PropertyKeyRef key,
                PropVariantRef currentValue,
                IUISimplePropertySet commandExecutionProperties)
            {
                var colorType =
                    (SwatchColorType) Convert.ToInt32(currentValue.PropVariant.Value, CultureInfo.InvariantCulture);
                var args = new ExecuteEventHandlerArgs();

                switch (colorType)
                {
                    case SwatchColorType.NoColor:
                        break;
                    case SwatchColorType.RGB:
                        PropVariant color;
                        commandExecutionProperties.GetValue(ref PropertyKeys.Color, out color);

                        args.Add("Automatic", false);
                        args.Add("SelectedColor",
                                 ColorHelper.BGRToColor(Convert.ToInt32(color.Value, CultureInfo.InvariantCulture)));
                        args.Add("SwatchColorType", (int) colorType);

                        break;
                    case SwatchColorType.Automatic:
                        Debug.Assert(false, "Automatic is not implemented.");
                        args.Add("Automatic", true);
                        break;
                }

                this.PerformExecuteWithArgs(verb, args);
                return HRESULT.S_OK;
            }

            #region Overrides of ColorPickerCommand

            /// <inheritdoc />
            public string[] StandardColorsTooltips
            {
                get
                {
                    var tooltips = new string[this.NumStandardColorsRows * this.NumColumns];
                    for (var row = 0; row < this.NumStandardColorsRows; row++)
                    {
                        for (var column = 0; column < this.NumColumns; column++)
                        {
                            var idx = Convert.ToInt32(row * this.NumColumns + column);
                            tooltips[idx] = Res.Get(this.standardColors[idx].StringId);
                        }
                    }

                    return tooltips;
                }
            }

            /// <inheritdoc />
            public uint[] StandardColors
            {
                get
                {
                    var colors = new uint[this.NumStandardColorsRows * this.NumColumns];
                    for (var row = 0; row < this.NumStandardColorsRows; row++)
                    {
                        for (var column = 0; column < this.NumColumns; column++)
                        {
                            var idx = Convert.ToInt32(row * this.NumColumns + column);
                            colors[idx] = Convert.ToUInt32(ColorHelper.ColorToBGR(this.standardColors[idx].Color));
                        }
                    }

                    return colors;
                }
            }

            /// <inheritdoc />
            public uint NumStandardColorsRows => FontColorPickerCommand.NumberOfStandardColorsRows;

            /// <inheritdoc />
            public uint NumColumns => FontColorPickerCommand.NumberOfColumns;

            /// <inheritdoc />
            public Color SelectedColor { get; private set; }

            /// <inheritdoc />
            public int SelectedColorAsBGR => ColorHelper.ColorToBGR(this.SelectedColor);

            /// <inheritdoc />
            public SwatchColorType SelectedColorType { get; private set; } = SwatchColorType.RGB;

            /// <inheritdoc />
            public bool Automatic { get; set; }

            /// <summary>
            /// Sets the color of the selected.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <param name="colorType">Type of the color.</param>
            public void SetSelectedColor(Color color, SwatchColorType colorType)
            {
                this.SelectedColor = color;
                this.SelectedColorType = colorType;

                this.UpdateInvalidationState(PropertyKeys.Color, InvalidationState.Pending);
                this.UpdateInvalidationState(PropertyKeys.ColorType, InvalidationState.Pending);

                this.Invalidate();
            }

            /// <inheritdoc />
            public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
            {
                if (key == PropertyKeys.AutomaticColorLabel ||
                    key == PropertyKeys.NoColorLabel ||
                    key == PropertyKeys.MoreColorsLabel)
                {
                    value.SetString((string) currentValue.PropVariant.Value);
                }
                else if (key == PropertyKeys.Color)
                {
                    value.SetUInt((uint) this.SelectedColorAsBGR);
                }
                else if (key == PropertyKeys.ColorType)
                {
                    value.SetUInt(Convert.ToUInt32(this.SelectedColorType, CultureInfo.InvariantCulture));
                }
                else if (key == PropertyKeys.StandardColors)
                {
                    value.SetUIntVector(this.StandardColors);
                }
                else if (key == PropertyKeys.StandardColorsTooltips)
                {
                    value.SetStringVector(this.StandardColorsTooltips);
                }
                else
                {
                    base.GetPropVariant(key, currentValue, ref value);
                }
            }

            #endregion
        }
    }
}
