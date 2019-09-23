// <copyright company=".NET Foundation" file="MenuDefinitionEntryPlaceholder.cs">
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
    ///     A menu item that has a submenu. The text is set by the MenuText property while the
    ///     child entries are set by Entries property.
    /// </summary>
    [DesignTimeVisible(false)]
    [ToolboxItem(false)]
    public class MenuDefinitionEntryPlaceholder : MenuDefinitionEntry
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private Container components;

        /// <summary>
        ///     Initializes a new instance of the MenuDefinitionEntryPlaceholder class.
        /// </summary>
        /// <param name="container">The IContainer that contains this component.</param>
        public MenuDefinitionEntryPlaceholder(IContainer container)
        {
            // Required for Windows.Forms Class Composition Designer support
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuDefinitionEntryPlaceholder"/> class.
        /// </summary>
        public MenuDefinitionEntryPlaceholder() => this.InitializeComponent();

        /// <summary>
        /// Gets or sets the collection of command bar entries that define the command bar.
        /// </summary>
        [Localizable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public MenuDefinitionEntryCollection Entries { get; set; } = new MenuDefinitionEntryCollection();

        /// <summary>
        /// Gets or sets the menu path.
        /// </summary>
        /// <value>The menu path.</value>
        public string MenuPath { get; set; }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
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
        protected override MenuItem GetMenuItem(CommandManager commandManager, MenuType menuType)
        {
            // Instantiate the placeholder OwnerDrawMenuItem.
            var ownerDrawMenuItem = new OwnerDrawMenuItem(menuType, this.MenuPath);

            // Build child MenuItems.
            var menuItems = MenuBuilder.CreateMenuItems(commandManager, menuType, this.Entries);
            if (menuItems != null)
            {
                ownerDrawMenuItem.MenuItems.AddRange(menuItems);
            }

            // Done.
            return ownerDrawMenuItem;
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();
    }
}
