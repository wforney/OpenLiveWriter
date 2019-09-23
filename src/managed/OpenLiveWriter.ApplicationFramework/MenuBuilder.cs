// <copyright file="MenuBuilder.cs" company=".NET Foundation">
// Copyright © .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Windows.Forms;

    /// <summary>
    /// Command-based menu builder.
    /// </summary>
    public static class MenuBuilder
    {
        /// <summary>
        /// Creates the menu items.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        /// <param name="commandContextMenuDefinition">The command context menu definition.</param>
        /// <returns>A <see cref="Array"/>.</returns>
        public static MenuItem[] CreateMenuItems(
            CommandManager commandManager,
            CommandContextMenuDefinition commandContextMenuDefinition) =>
            MenuBuilder.CreateMenuItems(
                commandManager,
                commandContextMenuDefinition.CommandBar ? MenuType.CommandBarContext : MenuType.Context,
                commandContextMenuDefinition.Entries);

        /// <summary>
        /// Creates menu items for the specified MenuDefinitionEntryCollection.
        /// </summary>
        /// <param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuType">Type of the menu.</param>
        /// <param name="menuDefinitionEntryCollection">The MenuDefinitionEntryCollection to create menu items for.</param>
        /// <returns>The menu items.</returns>
        public static MenuItem[] CreateMenuItems(
            CommandManager commandManager,
            MenuType menuType,
            MenuDefinitionEntryCollection menuDefinitionEntryCollection)
        {
            var menuItemArrayList = new ArrayList();
            foreach (var menuItems in menuDefinitionEntryCollection
                .Select(menuDefinitionEntry => menuDefinitionEntry.GetMenuItems(commandManager, menuType))
                .Where(menuItems => menuItems != null))
            {
                menuItemArrayList.AddRange(menuItems);
            }

            // remove leading, trailing, and adjacent separators
            for (var i = menuItemArrayList.Count - 1; i >= 0; i--)
            {
                const string Dash = "-";
                if (((MenuItem)menuItemArrayList[i]).Text == Dash)
                {
                    if (i == 0 || // leading
                        i == menuItemArrayList.Count - 1 || // trailing
                        ((MenuItem)menuItemArrayList[i - 1]).Text == Dash)
                    {
                        // adjacent
                        menuItemArrayList.RemoveAt(i);
                    }
                }
            }

            return menuItemArrayList.Count == 0 ? null : (MenuItem[])menuItemArrayList.ToArray(typeof(MenuItem));
        }
    }
}
