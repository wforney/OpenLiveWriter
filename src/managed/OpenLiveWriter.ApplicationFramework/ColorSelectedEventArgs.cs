// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Class ColorSelectedEventArgs.
    /// Implements the <see cref="System.EventArgs" />
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ColorSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorSelectedEventArgs"/> class.
        /// </summary>
        /// <param name="selectedColor">Color of the selected.</param>
        public ColorSelectedEventArgs(Color selectedColor) => this.SelectedColor = selectedColor;

        /// <summary>
        /// Gets the color of the selected.
        /// </summary>
        /// <value>The color of the selected.</value>
        public Color SelectedColor { get; }
    }
}
