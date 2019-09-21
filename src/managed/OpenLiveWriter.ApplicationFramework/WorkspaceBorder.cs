// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    ///     Enum WorkspaceBorder
    /// </summary>
    [Flags]
    public enum WorkspaceBorder
    {
        /// <summary>
        ///     The none
        /// </summary>
        None = 0,

        /// <summary>
        ///     The left
        /// </summary>
        Left = 1,

        /// <summary>
        ///     The right
        /// </summary>
        Right = 2,

        /// <summary>
        ///     The top
        /// </summary>
        Top = 4,

        /// <summary>
        ///     The bottom
        /// </summary>
        Bottom = 8
    }
}
