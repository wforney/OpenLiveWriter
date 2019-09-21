// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;

    /// <summary>
    /// Represents a collection of commands.
    /// Implements the <see cref="List{Command}" />
    /// </summary>
    /// <seealso cref="List{Command}" />
    [Editor(typeof(CommandCollectionEditor), typeof(UITypeEditor))]
    public class CommandCollection : List<Command>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        public CommandCollection() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public CommandCollection(int capacity) : base(capacity) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandCollection"/> class.
        /// </summary>
        /// <param name="commands">The commands.</param>
        public CommandCollection(IEnumerable<Command> commands) : base(commands) { }
    }
}
