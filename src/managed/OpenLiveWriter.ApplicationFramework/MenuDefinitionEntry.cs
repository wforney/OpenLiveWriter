// <copyright company=".NET Foundation" file="MenuDefinitionEntry.cs">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>
    ///     Base class of MenuDefinition entries.
    /// </summary>
    [DesignTimeVisible(false)]
    [ToolboxItem(false)]
    public class MenuDefinitionEntry : Component
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private Container components;

        /// <summary>
        ///     Initializes a new instance of the MenuDefinitionEntry class.
        /// </summary>
        /// <param name="container">The IContainer that contains this component.</param>
        public MenuDefinitionEntry(IContainer container)
        {
            // Required for Windows.Forms Class Composition Designer support
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MenuDefinitionEntry" /> class.
        /// </summary>
        public MenuDefinitionEntry() => this.InitializeComponent();

        /// <summary>
        ///     Gets or sets a value indicating whether this
        ///     <see cref="T:OpenLiveWriter.ApplicationFramework.MenuDefinitionEntry" /> is on.
        /// </summary>
        /// <value>
        ///     <c>true</c> if on; otherwise, <c>false</c>.
        /// </value>
        public bool On { get; set; } = true;

        /// <summary>
        ///     Gets or sets a value indicating whether a separator will be placed before the entry.
        /// </summary>
        [Category("Menu")]
        [Localizable(false)]
        [Description("Indicates whether a separator will be placed after the entry.")]
        public bool SeparatorAfter { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether a separator will be placed before the entry.
        /// </summary>
        [Category("Menu")]
        [Localizable(false)]
        [Description("Indicates whether a separator will be placed before the entry.")]
        public bool SeparatorBefore { get; set; }

        /// <summary>
        ///     Gets the set of menu items for this MenuDefinitionEntry.
        /// </summary>
        /// <param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuType">The MenuType.</param>
        /// <returns>Array of menu items for this MenuDefinitionEntry.</returns>
        public MenuItem[] GetMenuItems(CommandManager commandManager, MenuType menuType)
        {
            if (!this.On)
            {
                return null;
            }

            // Get the MenuItem for this MenuDefinitionEntry.  If it's null, return null.
            var menuItem = this.GetMenuItem(commandManager, menuType);
            if (menuItem == null)
            {
                return null;
            }

            // Sort out whether we'll be adding separator menu items.
            var count = 1;
            if (this.SeparatorBefore)
            {
                count++;
            }

            if (this.SeparatorAfter)
            {
                count++;
            }

            // Return the array of menu items for this MenuDefinitionEntry.
            var index = 0;
            var menuItems = new MenuItem[count];
            if (this.SeparatorBefore)
            {
                menuItems[index++] = MenuDefinitionEntry.MakeSeparatorMenuItem(MenuType.Context);
            }

            menuItems[index++] = menuItem;
            if (this.SeparatorAfter)
            {
                menuItems[index] = MenuDefinitionEntry.MakeSeparatorMenuItem(MenuType.Context);
            }

            return menuItems;
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Gets the MenuItem for this MenuDefinitionEntry.
        /// </summary>
        /// <param name="commandManager">The CommandManager to use.</param>
        /// <param name="menuType">The MenuType.</param>
        /// <returns>The menu item for this MenuDefinitionEntry.</returns>
        protected virtual MenuItem GetMenuItem(CommandManager commandManager, MenuType menuType) => null;

        /// <summary>
        ///     Helper to make a separator menu item.
        /// </summary>
        /// <param name="menuType">The menu type.</param>
        /// <returns>A MenuItem that is a separator MenuItem.</returns>
        private static MenuItem MakeSeparatorMenuItem(MenuType menuType)
        {
            // Instantiate the separator menu item.
            MenuItem separatorMenuItem = new OwnerDrawMenuItem(menuType, "-");
            return separatorMenuItem;
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();
    }
}
