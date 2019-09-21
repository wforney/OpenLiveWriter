// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface ICommandManagerHost
    /// </summary>
    public interface ICommandManagerHost
    {
        /// <summary>
        /// Gets the command manager.
        /// </summary>
        /// <value>The command manager.</value>
        CommandManager CommandManager { get; }
    }
}
