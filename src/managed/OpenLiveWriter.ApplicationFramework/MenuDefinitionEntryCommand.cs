// <copyright company=".NET Foundation" file="MenuDefinitionEntryCommand.cs">
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
    public class MenuDefinitionEntryCommand : MenuDefinitionEntry
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private Container components;

        /// <summary>
        ///     Initializes a new instance of the MenuDefinitionEntryCommand class.
        /// </summary>
        /// <param name="container">The IContainer that contains this component.</param>
        public MenuDefinitionEntryCommand(IContainer container)
        {
            // Required for Windows.Forms Class Composition Designer support
            container.Add(this);
            this.InitializeComponent();
        }

        /// <inheritdoc />
        public MenuDefinitionEntryCommand() => this.InitializeComponent();

        /// <summary>
        /// Gets or sets the command identifier.
        /// </summary>
        /// <value>The command identifier.</value>
        public string CommandIdentifier { get; set; }

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
        protected override MenuItem GetMenuItem(CommandManager commandManager, MenuType menuType)
        {
            var command = commandManager.Get(this.CommandIdentifier);
            if (command == null || !command.On)
            {
                return null;
            }

            if ((menuType == MenuType.Context || menuType == MenuType.CommandBarContext)
             && !command.VisibleOnContextMenu)
            {
                return null;
            }

            // Instantiate and initialize the CommandOwnerDrawMenuItem.
            var commandOwnerDrawMenuItem = new CommandOwnerDrawMenuItem(menuType, command, command.MenuText);
            return commandOwnerDrawMenuItem;
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();
    }
}
