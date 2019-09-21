// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    /// Class DynamicCommandMenuOptions.
    /// </summary>
    public class DynamicCommandMenuOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandMenuOptions"/> class.
        /// </summary>
        /// <param name="mainMenuBasePath">The main menu base path.</param>
        /// <param name="menuMergeOffset">The menu merge offset.</param>
        public DynamicCommandMenuOptions(string mainMenuBasePath, int menuMergeOffset)
            : this(mainMenuBasePath, menuMergeOffset, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandMenuOptions"/> class.
        /// </summary>
        /// <param name="mainMenuBasePath">The main menu base path.</param>
        /// <param name="menuMergeOffset">The menu merge offset.</param>
        /// <param name="moreCommandsMenuCaption">The more commands menu caption.</param>
        /// <param name="moreCommandsDialogTitle">The more commands dialog title.</param>
        public DynamicCommandMenuOptions(
            string mainMenuBasePath, int menuMergeOffset,
            string moreCommandsMenuCaption, string moreCommandsDialogTitle)
        {
            // copy passed in values
            this.MainMenuBasePath = mainMenuBasePath;
            this.MenuMergeOffset = menuMergeOffset;
            this.MoreCommandsMenuCaption = moreCommandsMenuCaption;
            this.MoreCommandsDialogTitle = moreCommandsDialogTitle;
            this.SeparatorBegin = true;

            // default other options
            this.MaxCommandsShownOnMenu = 9;
            this.UseNumericMnemonics = true;
            this.SeparatorBegin = false;
        }

        /// <summary>
        /// Base path for main menu
        /// </summary>
        public readonly string MainMenuBasePath;

        /// <summary>
        /// Offset for menu items to be merged into the main menu
        /// </summary>
        public readonly int MenuMergeOffset;

        /// <summary>
        /// Menu caption to be used if more commands are available than are displayable on the menu
        /// (you can display up to 9 on the menu). If null then no 'More' option is provided.
        /// </summary>
        public readonly string MoreCommandsMenuCaption;

        /// <summary>
        /// Dialog title to be used when showing the more dialog
        /// </summary>
        public readonly string MoreCommandsDialogTitle;

        /// <summary>
        /// Maximum number of commands to show on the menu (must be from 1 to 9)
        /// </summary>
        /// <value>The maximum commands shown on menu.</value>
        /// <exception cref="ArgumentException">Invalid value for MaxCommandsShownOnMenu</exception>
        public int MaxCommandsShownOnMenu
        {
            get => this.maxCommandsShownOnMenu;
            set
            {
                if (value < 1 || (this.UseNumericMnemonics && (value > 9)))
                {
                    throw new ArgumentException("Invalid value for MaxCommandsShownOnMenu");
                }
                else
                {
                    this.maxCommandsShownOnMenu = value;
                }
            }
        }

        /// <summary>
        /// The maximum commands shown on menu
        /// </summary>
        private int maxCommandsShownOnMenu;

        /// <summary>
        /// Should we add numeric Mnemonics to the commands
        /// </summary>
        public bool UseNumericMnemonics;

        /// <summary>
        /// Use a separator before the first command
        /// </summary>
        public bool SeparatorBegin;
    }
}

