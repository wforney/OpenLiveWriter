// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    ///     Interface IWorkspaceBorderManager
    /// </summary>
    [Obsolete]
    public interface IWorkspaceBorderManager
    {
        /// <summary>
        ///     Gets or sets the workspace borders.
        /// </summary>
        /// <value>The workspace borders.</value>
        WorkspaceBorder WorkspaceBorders { get; set; }
    }
}
