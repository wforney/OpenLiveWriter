// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface IDynamicCommandMenuContext
    /// </summary>
    public interface IDynamicCommandMenuContext
    {
        /// <summary>
        /// Get the options for the menu
        /// </summary>
        /// <value>The options.</value>
        DynamicCommandMenuOptions Options { get; }

        /// <summary>
        /// Command manager to add commands to
        /// </summary>
        /// <value>The command manager.</value>
        CommandManager CommandManager { get; }

        /// <summary>
        /// Command objects to show on the menu
        /// </summary>
        /// <returns>IMenuCommandObject[].</returns>
        IMenuCommandObject[] GetMenuCommandObjects();

        /// <summary>
        /// Notification that the user executed a command
        /// </summary>
        /// <param name="menuCommandObject">The menu command object.</param>
        void CommandExecuted(IMenuCommandObject menuCommandObject);
    }

}

