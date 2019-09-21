// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using OpenLiveWriter.Interop.Com.Ribbon;

    /// <summary>
    /// Interface IColorPickerCommand
    /// </summary>
    public interface IColorPickerCommand
    {
        /// <summary>
        /// Gets the standard colors tooltips.
        /// </summary>
        /// <value>The standard colors tooltips.</value>
        string[] StandardColorsTooltips { get; }

        /// <summary>
        /// Gets the standard colors.
        /// </summary>
        /// <value>The standard colors.</value>
        uint[] StandardColors { get; }

        /// <summary>
        /// Gets the number standard colors rows.
        /// </summary>
        /// <value>The number standard colors rows.</value>
        uint NumStandardColorsRows { get; }

        /// <summary>
        /// Gets the number columns.
        /// </summary>
        /// <value>The number columns.</value>
        uint NumColumns { get; }

        /// <summary>
        /// Gets the color of the selected.
        /// </summary>
        /// <value>The color of the selected.</value>
        Color SelectedColor { get; }

        /// <summary>
        /// Gets the selected color as BGR.
        /// </summary>
        /// <value>The selected color as BGR.</value>
        int SelectedColorAsBGR { get; }

        /// <summary>
        /// Gets the type of the selected color.
        /// </summary>
        /// <value>The type of the selected color.</value>
        SwatchColorType SelectedColorType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IColorPickerCommand"/> is automatic.
        /// </summary>
        /// <value><c>true</c> if automatic; otherwise, <c>false</c>.</value>
        bool Automatic { get; set; }

        /// <summary>
        /// Sets the color of the selected.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="type">The type.</param>
        void SetSelectedColor(Color color, SwatchColorType type);
    }
}
