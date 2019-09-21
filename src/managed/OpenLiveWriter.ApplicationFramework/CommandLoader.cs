// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections.Generic;

    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class CommandLoader.
    /// Implements the <see cref="IDisposable" />
    /// </summary>
    /// <seealso cref="IDisposable" />
    public class CommandLoader : IDisposable
    {
        /// <summary>
        /// The command manager
        /// </summary>
        private readonly CommandManager commandManager;

        /// <summary>
        /// The loaded commands
        /// </summary>
        private readonly List<Command> loadedCommands = new List<Command>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLoader"/> class.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        /// <param name="commandIds">The command ids.</param>
        /// <exception cref="ArgumentNullException">commandManager</exception>
        public CommandLoader(CommandManager commandManager, params CommandId[] commandIds)
        {
            this.commandManager = commandManager ?? throw new ArgumentNullException(nameof(commandManager));
            try
            {
                this.commandManager.BeginUpdate();
                foreach (var commandId in commandIds)
                {
                    var command = new Command(commandId);
                    commandManager.Add(command);
                    this.loadedCommands.Add(command);
                }
            }
            catch (Exception)
            {
                this.Dispose();
                throw;
            }
            finally
            {
                this.commandManager.EndUpdate();
            }
        }

        /// <summary>
        /// Gets the commands.
        /// </summary>
        /// <value>The commands.</value>
        public Command[] Commands => this.loadedCommands.ToArray();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                this.commandManager.BeginUpdate();
                foreach (var command in this.loadedCommands)
                {
                    this.commandManager.Remove(command);
                }

                this.loadedCommands.Clear();
            }
            finally
            {
                this.commandManager.EndUpdate();
            }
        }
    }
}
