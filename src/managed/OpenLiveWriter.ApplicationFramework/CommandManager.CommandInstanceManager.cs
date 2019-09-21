// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Diagnostics;
    using System.Globalization;

    public partial class CommandManager
    {
        /// <summary>
        ///	Manages command instances.
        /// </summary>
        private class CommandInstanceManager
        {
            /// <summary>
            ///	The command instance collection.
            /// </summary>
            private readonly CommandCollection commandInstanceCollection = new CommandCollection();

            /// <summary>
            /// Initializes a new instance of the CommandInstanceManager class.
            /// </summary>
            /// <param name="command">The initial command instance to add.</param>
            public CommandInstanceManager(Command command) => this.Add(command);

            /// <summary>
            /// Gets a value indicating whether the CommandInstanceManager is empty.
            /// </summary>
            public bool IsEmpty => this.commandInstanceCollection.Count == 0;

            /// <summary>
            /// Gets the active command instance.
            /// </summary>
            public Command ActiveCommandInstance =>
                this.IsEmpty ? null : this.commandInstanceCollection[this.commandInstanceCollection.Count - 1];

            /// <summary>
            /// Adds a command instance.
            /// </summary>
            /// <param name="command">The command instance to add.</param>
            public void Add(Command command)
            {
                //	Ensure that the command instance has not already been added.
                Debug.Assert(
                    !this.commandInstanceCollection.Contains(command),
                    string.Format(CultureInfo.InvariantCulture, "Command instance {0} already added.", command.Identifier));

                //	Add the command instance.
                if (!this.commandInstanceCollection.Contains(command))
                {
                    this.commandInstanceCollection.Add(command);
                }
            }

            /// <summary>
            /// Removes a command instance.
            /// </summary>
            /// <param name="command">The command to remove.</param>
            public void Remove(Command command)
            {
                //	Ensure that the command instance has been added and is not active.
                Debug.Assert(
                    this.commandInstanceCollection.Contains(command),
                    string.Format(CultureInfo.InvariantCulture, "Command instance {0} not found.", command.Identifier));

                //	Remove the command instance.
                if (this.commandInstanceCollection.Contains(command))
                {
                    this.commandInstanceCollection.Remove(command);
                }
            }
        }
    }
}
