// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Collections;
    using System.Windows.Forms;

    /// <summary>
    /// CommandMenuBuilderEntry is an internal class used to build Command-based menus.
    /// </summary>
    internal sealed partial class CommandMenuBuilderEntry
    {
        /// <summary>
        /// Used to indicate that the menu item should include a separator.  Example: File@1/-Save@20
        /// </summary>
        private static readonly char SEPARATOR_CHAR = '-';

        /// <summary>
        /// The CommandMenuBuilder for this CommandMenuBuilderEntry.
        /// </summary>
        private readonly CommandMenuBuilder commandMenuBuilder;

        /// <summary>
        /// Gets the merge menu entry level.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets the merge menu entry position.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets the merge menu entry text.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Gets the merge menu entry command.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// Child merge menu entries.
        /// </summary>
        private SortedList childMergeMenuEntries = new SortedList();

        /// <summary>
        /// Initializes a new instance of the CommandMenuBuilderEntry class.
        /// </summary>
        public CommandMenuBuilderEntry(CommandMenuBuilder commandMenuBuilder)
        {
            this.commandMenuBuilder = commandMenuBuilder;
            this.Level = -1;
        }

        /// <summary>
        /// Initializes a new instance of the CommandMenuBuilderEntry class.  This constructor is used for
        /// top level and "container" menu items.
        /// </summary>
        /// <param name="position">Merge menu entry position.</param>
        /// <param name="text">Merge menu entry text.</param>
        public CommandMenuBuilderEntry(CommandMenuBuilder commandMenuBuilder, int level, int position, string text)
        : this(commandMenuBuilder, level, position, text, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandMenuBuilderEntry class.  This constructor is used for
        /// the command menu entry.
        /// </summary>
        /// <param name="position">Merge menu entry position.</param>
        /// <param name="text">Merge menu entry text.</param>
        /// <param name="command">Merge menu entry command.</param>
        public CommandMenuBuilderEntry(CommandMenuBuilder commandMenuBuilder, int level, int position, string text, Command command)
        {
            this.commandMenuBuilder = commandMenuBuilder;
            this.Level = level;
            this.Position = position;
            this.Text = text;
            //			if (command != null)
            //				this.text = command.Identifier;
            this.Command = command;
        }

        /// <summary>
        /// Indexer for access child merge menu entries.
        /// </summary>
        public CommandMenuBuilderEntry this[int position, string text]
        {
            get => (CommandMenuBuilderEntry)this.childMergeMenuEntries[new Key(position, text)];
            set
            {
                this.childMergeMenuEntries[new Key(position, text)] = value;
                return;
            }
        }

        /// <summary>
        /// Creates and returns a set of menu items from the child merge menu entries in this merge
        /// menu entry.
        /// </summary>
        /// <param name="mainMenu">The level at which the MenuItems will appear.</param>
        /// <returns>Array of menu items.</returns>
        public MenuItem[] CreateMenuItems()
        {
            //	If this merge menu entry has no child merge menu entries, return null.
            if (this.childMergeMenuEntries.Count == 0)
            {
                return null;
            }

            //	Construct an array list to hold the menu items being created.
            var menuItemArrayList = new ArrayList();

            //	Enumerate the child merge menu entries of this merge menu entry.
            foreach (CommandMenuBuilderEntry mergeMenuEntry in this.childMergeMenuEntries.Values)
            {
                //	Get the text of the merge menu entry.
                var text = mergeMenuEntry.Text;

                //	Create the menu item for this child merge menu entry.
                MenuItem menuItem;
                bool separatorBefore, separatorAfter;
                if (this.commandMenuBuilder.MenuType == MenuType.Main && mergeMenuEntry.Level == 0)
                {
                    //	Level zero of a main menu.
                    menuItem = new OwnerDrawMenuItem(this.commandMenuBuilder.MenuType, text);
                    separatorBefore = separatorAfter = false;
                }
                else
                {
                    //	Determine whether a separator before and a separator after the menu item
                    //	should be inserted.
                    separatorBefore = text.StartsWith(SEPARATOR_CHAR.ToString());
                    separatorAfter = text.EndsWith(SEPARATOR_CHAR.ToString());
                    if (separatorBefore || separatorAfter)
                    {
                        text = text.Trim(SEPARATOR_CHAR);
                    }

                    //	Instantiate the menu item.
                    if (mergeMenuEntry.Command == null)
                    {
                        menuItem = new OwnerDrawMenuItem(this.commandMenuBuilder.MenuType, text);
                    }
                    else
                    {
                        menuItem = new CommandOwnerDrawMenuItem(this.commandMenuBuilder.MenuType, mergeMenuEntry.Command, text);
                    }
                }

                //	Set the menu item text.
                // menuItem.Text = text;

                //	If this child merge menu entry has any child merge menu entries, recursively
                //	create their menu items.
                var childMenuItems = mergeMenuEntry.CreateMenuItems();
                if (childMenuItems != null)
                {
                    menuItem.MenuItems.AddRange(childMenuItems);
                }

                //	Add the separator menu item, as needed.
                if (separatorBefore)
                {
                    menuItemArrayList.Add(MakeSeparatorMenuItem(this.commandMenuBuilder.MenuType));
                }

                //	Add the menu item to the array of menu items being returned.
                menuItemArrayList.Add(menuItem);

                //	Add the separator menu item, as needed.
                if (separatorAfter)
                {
                    menuItemArrayList.Add(MakeSeparatorMenuItem(this.commandMenuBuilder.MenuType));
                }
            }

            // remove leading, trailing, and adjacent separators
            for (int i = menuItemArrayList.Count - 1; i >= 0; i--)
            {
                if (((MenuItem)menuItemArrayList[i]).Text == "-")
                {
                    if (i == 0 ||  // leading
                        i == menuItemArrayList.Count - 1 ||  // trailing
                        ((MenuItem)menuItemArrayList[i - 1]).Text == "-")  // adjacent
                    {
                        menuItemArrayList.RemoveAt(i);
                    }
                }
            }

            //	Done.  Convert the array list into a MenuItem array and return it.
            return (MenuItem[])menuItemArrayList.ToArray(typeof(MenuItem));
        }

        /// <summary>
        /// Helper to make a separator menu item.
        /// </summary>
        /// <returns>A MenuItem that is a separator MenuItem.</returns>
        private static MenuItem MakeSeparatorMenuItem(MenuType menuType)
        {
            // Instantiate the separator menu item.
            MenuItem separatorMenuItem = new OwnerDrawMenuItem(menuType, SEPARATOR_CHAR.ToString());
            return separatorMenuItem;
        }
    }
}
