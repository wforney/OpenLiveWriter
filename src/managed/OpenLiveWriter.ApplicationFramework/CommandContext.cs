// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Enumeration of the allowable command context values.
    /// </summary>
    public enum CommandContext
    {
        /// <summary>
        /// A command that is added to the CommandManager at all times.
        /// </summary>
        Normal,

        /// <summary>
        /// A command that is added to the CommandManager when active.
        /// </summary>
        Activated,

        /// <summary>
        /// A command that is added to the CommandManager when entered.
        /// </summary>
        Entered
    }
}
