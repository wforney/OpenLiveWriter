// <copyright company=".NET Foundation" file="MenuDefinitionEntryCollection.cs">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;

    using OpenLiveWriter.Localization;

    /// <summary>
    ///     Represents a collection of MenuDefinitionEntry objects.
    ///     Implements the
    ///     <see cref="List{MenuDefinitionEntry}" />
    /// </summary>
    /// <seealso cref="List{MenuDefinitionEntry}" />
    [Editor(typeof(MenuDefinitionEntryCollectionEditor), typeof(UITypeEditor))]
    public class MenuDefinitionEntryCollection : List<MenuDefinitionEntry>
    {
        /// <summary>
        ///     Use strongly typed overload instead of this if possible!!
        /// </summary>
        /// <param name="commandIdentifier">The command identifier.</param>
        /// <param name="separatorBefore">if set to <c>true</c> [separator before].</param>
        /// <param name="separatorAfter">if set to <c>true</c> [separator after].</param>
        public void Add(string commandIdentifier, bool separatorBefore, bool separatorAfter)
        {
            var mde = new MenuDefinitionEntryCommand
            {
                CommandIdentifier = commandIdentifier,
                SeparatorBefore = separatorBefore,
                SeparatorAfter = separatorAfter
            };
            this.Add(mde);
        }

        /// <summary>
        ///     Adds the specified command identifier.
        /// </summary>
        /// <param name="commandIdentifier">The command identifier.</param>
        /// <param name="separatorBefore">if set to <c>true</c> [separator before].</param>
        /// <param name="separatorAfter">if set to <c>true</c> [separator after].</param>
        public void Add(CommandId commandIdentifier, bool separatorBefore, bool separatorAfter)
        {
            var mde = new MenuDefinitionEntryCommand
            {
                CommandIdentifier = commandIdentifier.ToString(),
                SeparatorBefore = separatorBefore,
                SeparatorAfter = separatorAfter
            };
            this.Add(mde);
        }
    }
}
