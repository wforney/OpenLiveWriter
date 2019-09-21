// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Collections;
    using System.Diagnostics;

    /// <summary>
    /// Provides services for managing commands.
    /// </summary>
    public class CommandContextManager
    {
        /// <summary>
        ///	The CommandManager.
        /// </summary>
        private readonly CommandManager commandManager;

        /// <summary>
        /// The command context table.
        /// </summary>
        private readonly Hashtable commandContextTable = new Hashtable();

        /// <summary>
        /// Collection of commands that are added to the CommandManager at all times.
        /// </summary>
        private readonly CommandCollection normalCommandCollection = new CommandCollection();

        /// <summary>
        /// Collection of commands that are added to the CommandManager when the user of this
        /// CommandContextManager is "Activated".
        /// </summary>
        private readonly CommandCollection activatedCommandCollection = new CommandCollection();

        /// <summary>
        /// Collection of commands that are added to the CommandManager when the user of this
        /// CommandContextManager is "Entered".
        /// </summary>
        private readonly CommandCollection enteredCommandCollection = new CommandCollection();

        /// <summary>
        /// Initializes a new instance of the CommandContextManager class.
        /// </summary>
        public CommandContextManager(CommandManager commandManager) => this.commandManager = commandManager;

        /// <summary>
        /// Begins update to CommandContextManager allowing multiple change events to be batched.
        /// </summary>
        public void BeginUpdate() => this.commandManager.BeginUpdate();

        /// <summary>
        /// Ends update to CommandContextManager allowing multiple change events to be batched.
        /// </summary>
        public void EndUpdate() => this.commandManager.EndUpdate();

        /// <summary>
        /// Closes the CommandContextManager, ensuring that all commands have been removed from
        /// the CommandManager.
        /// </summary>
        public void Close()
        {
            //	Begin the batch update.
            this.BeginUpdate();

            //	If entered, leave.
            if (this.Entered)
            {
                this.Leave();
            }

            //	If activated, deactivate.
            if (this.Activated)
            {
                this.Deactivate();
            }

            //	Remove normal commands.
            this.commandManager.Remove(this.normalCommandCollection);

            //	End the batch update.
            this.EndUpdate();

            //	Clear our internal tables.
            this.normalCommandCollection.Clear();
            this.activatedCommandCollection.Clear();
            this.enteredCommandCollection.Clear();
            this.commandContextTable.Clear();
        }

        /// <summary>
        /// Adds a command to the CommandContextManager.
        /// </summary>
        /// <param name="command">The Command to add.</param>
        /// <param name="commandContext">The context in which the command is added to the CommandManager.</param>
        public void AddCommand(Command command, CommandContext commandContext)
        {
            //	Ensure that the command is not null.
            Debug.Assert(command != null, "Command cannot be null");
            if (command == null)
            {
                return;
            }

            //	Ensure the the command has not already been added.
            if (this.commandContextTable.Contains(command))
            {
                Debug.Fail($"Command {command.Identifier} was already added.");
                return;
            }

            //	Handle the command, adding it to the appropriate command collection.
            switch (commandContext)
            {
                //	Normal commands.
                case CommandContext.Normal:
                    this.normalCommandCollection.Add(command);
                    this.commandManager.Add(command);
                    break;

                //	Activated commands.
                case CommandContext.Activated:
                    this.activatedCommandCollection.Add(command);
                    if (this.Activated)
                    {
                        this.commandManager.Add(command);
                    }

                    break;

                //	Entered commands.
                case CommandContext.Entered:
                    this.enteredCommandCollection.Add(command);
                    if (this.Entered)
                    {
                        this.commandManager.Add(command);
                    }

                    break;

                //	Can't happen.
                default:
                    Debug.Fail("Unknown CommandContext");
                    return;
            }

            //	Add the command to the command context table.
            this.commandContextTable[command] = commandContext;
        }

        /// <summary>
        /// Removes a command from the CommandContextManager.
        /// </summary>
        /// <param name="command">The Command to remove.</param>
        /// <param name="commandContext">The context in which the command is added to the CommandManager.</param>
        public void RemoveCommand(Command command)
        {
            //	Ensure that the command is not null.
            Debug.Assert(command != null, "Command cannot be null");
            if (command == null)
            {
                return;
            }

            //	Ensure the the command has been added.
            if (!this.commandContextTable.Contains(command))
            {
                Debug.Fail($"Command {command.Identifier} was not added.");
                return;
            }

            //	Handle the command, removing it from the appropriate command collection.
            switch ((CommandContext)this.commandContextTable[command])
            {
                //	Normal commands.
                case CommandContext.Normal:
                    this.normalCommandCollection.Remove(command);
                    this.commandManager.Remove(command);
                    break;

                //	Activated commands.
                case CommandContext.Activated:
                    this.activatedCommandCollection.Remove(command);
                    if (this.Activated)
                    {
                        this.commandManager.Remove(command);
                    }

                    break;

                //	Entered commands.
                case CommandContext.Entered:
                    this.enteredCommandCollection.Remove(command);
                    if (this.Entered)
                    {
                        this.commandManager.Remove(command);
                    }

                    break;

                //	Can't happen.
                default:
                    Debug.Fail("Unknown CommandContext");
                    return;
            }

            //	Remove the command from the command context table.
            this.commandContextTable.Remove(command);
        }

        public bool Activated { get; private set; } = false;

        /// <summary>
        /// Set the "Activated" state of the CommandContextManager.
        /// </summary>
        public void Activate()
        {
            Debug.Assert(!this.Activated, "CommandContextManager already activated.");
            if (!this.Activated)
            {
                this.commandManager.Add(this.activatedCommandCollection);
                this.Activated = true;
            }
        }

        /// <summary>
        /// Clears the "Activated" state of the CommandContextManager.
        /// </summary>
        public void Deactivate()
        {
            Debug.Assert(this.Activated, "CommandContextManager not activated.");
            if (this.Activated)
            {
                this.commandManager.Remove(this.activatedCommandCollection);
                this.Activated = false;
            }
        }

        public bool Entered { get; private set; } = false;

        /// <summary>
        /// Set the "Entered" state of the CommandContextManager.
        /// </summary>
        public void Enter()
        {
            //Debug.Assert(!entered, "CommandContextManager already entered.");
            if (!this.Entered)
            {
                this.commandManager.Add(this.enteredCommandCollection);
                this.Entered = true;
            }
        }

        /// <summary>
        /// Clears the "Entered" state of the CommandContextManager.
        /// </summary>
        public void Leave()
        {
            //Debug.Assert(entered, "CommandContextManager not entered.");
            if (this.Entered)
            {
                this.commandManager.Remove(this.enteredCommandCollection);
                this.Entered = false;
            }
        }
    }
}
