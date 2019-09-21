// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;

    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class DispatchingCommand.
    /// Implements the <see cref="Command" />
    /// </summary>
    /// <seealso cref="Command" />
    public class DispatchingCommand : Command
    {
        /// <summary>
        /// The command manager
        /// </summary>
        protected readonly CommandManager CommandManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatchingCommand"/> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="commandManager">The command manager.</param>
        public DispatchingCommand(CommandId commandId, CommandManager commandManager)
            : base(commandId) => this.CommandManager = commandManager;

        /// <summary>
        /// The commands
        /// </summary>
        protected Hashtable commands = new Hashtable();

        /// <summary>
        /// Handles the StateChanged event of the command control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void command_StateChanged(object sender, EventArgs e) => this.OnStateChanged(e);

        /// <summary>
        /// Associates a commandid with the property key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="executeWithArgsDelegate">The execute with arguments delegate.</param>
        public void AddCommand(PropertyKey key, ExecuteWithArgsDelegate executeWithArgsDelegate) => this.commands.Add(key, executeWithArgsDelegate);

        /// <summary>
        /// Dispatches the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="args">The arguments.</param>
        public void Dispatch(PropertyKey key, ExecuteEventHandlerArgs args)
        {
            if (this.commands[key] is ExecuteWithArgsDelegate executeWithArgsDelegate)
            {
                executeWithArgsDelegate(args);
                return;
            }

            var commandId = (CommandId)this.commands[key];
            var command = this.CommandManager.Get(commandId);
            command.PerformExecuteWithArgs(args);
        }
    }
}
