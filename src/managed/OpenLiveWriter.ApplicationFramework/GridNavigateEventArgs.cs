// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    /// Class GridNavigateEventArgs.
    /// Implements the <see cref="System.EventArgs" />
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public partial class GridNavigateEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GridNavigateEventArgs"/> class.
        /// </summary>
        /// <param name="dir">The dir.</param>
        public GridNavigateEventArgs(Direction dir) => this.Navigate = dir;

        /// <summary>
        /// Gets the navigate.
        /// </summary>
        /// <value>The navigate.</value>
        public Direction Navigate { get; }
    }
}
