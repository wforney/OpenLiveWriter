// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Class GridNavigateEventArgs.
    /// Implements the <see cref="System.EventArgs" />
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public partial class GridNavigateEventArgs
    {
        /// <summary>
        /// Enum Direction
        /// </summary>
        public enum Direction
        {
            /// <summary>
            /// Up
            /// </summary>
            Up,
            /// <summary>
            /// Down
            /// </summary>
            Down,
            /// <summary>
            /// The left
            /// </summary>
            Left,
            /// <summary>
            /// The right
            /// </summary>
            Right
        }
    }
}
