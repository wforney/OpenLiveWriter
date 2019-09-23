// <copyright company=".NET Foundation" file="MenuDefinitionEntryCollectionEditor.cs">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel.Design;
    using System.Linq;

    /// <summary>
    ///     Provides a user interface for editing a MenuDefinitionEntryCollection.
    /// </summary>
    internal class MenuDefinitionEntryCollectionEditor : CollectionEditor
    {
        /// <inheritdoc />
        public MenuDefinitionEntryCollectionEditor()
            : base(typeof(MenuDefinitionEntryCollection))
        {
        }

        /// <inheritdoc />
        protected override Type[] CreateNewItemTypes() =>
            new[]
                {
                    typeof(MenuDefinitionEntryCommand),
                    typeof(MenuDefinitionEntryPlaceholder)
                };

        /// <inheritdoc />
        protected override object[] GetItems(object editValue)
        {
            var commandContextMenuDefinitionEntryCollection = (MenuDefinitionEntryCollection)editValue;
            var commandContextMenuDefinitionEntries =
                new MenuDefinitionEntry[commandContextMenuDefinitionEntryCollection.Count];
            if (commandContextMenuDefinitionEntryCollection.Count > 0)
            {
                commandContextMenuDefinitionEntryCollection.CopyTo(commandContextMenuDefinitionEntries, 0);
            }

            return commandContextMenuDefinitionEntries;
        }

        /// <inheritdoc />
        protected override object SetItems(object editValue, object[] value)
        {
            var commandContextMenuDefinitionEntryCollection = (MenuDefinitionEntryCollection)editValue;
            commandContextMenuDefinitionEntryCollection.Clear();
            commandContextMenuDefinitionEntryCollection.AddRange(value.Cast<MenuDefinitionEntry>());

            return commandContextMenuDefinitionEntryCollection;
        }
    }
}
