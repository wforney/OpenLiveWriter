// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class CommandBarDefinition.
    /// Implements the <see cref="Component" />
    /// </summary>
    /// <seealso cref="Component" />
    public class CommandBarDefinition : Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Gets or sets the command bar identifier.
        /// </summary>
        [
            Category("Design"),
                Localizable(false),
                Description("The identifier of the command bar definition.")
        ]
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the collection of command bar entries that define the command bar.
        /// </summary>
        [
            Localizable(true),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public CommandBarEntryCollection LeftCommandBarEntries { get; set; } = new CommandBarEntryCollection();

        /// <summary>
        /// Gets or sets the collection of command bar entries that define the command bar.
        /// </summary>
        [
            Localizable(true),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Content)
        ]
        public CommandBarEntryCollection RightCommandBarEntries { get; set; } = new CommandBarEntryCollection();

        /// <summary>
        /// Initializes a new instance of the CommandBarDefinition class.
        /// </summary>
        /// <param name="container">The component container to add this component to.</param>
        public CommandBarDefinition(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarDefinition class.
        /// </summary>
        public CommandBarDefinition() =>
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();

        public static CommandBarDefinition Create(params object[] items)
        {
            var cbd = new CommandBarDefinition();
            AddItems(cbd.LeftCommandBarEntries, items);
            return cbd;
        }

        public static CommandBarDefinition Create(object[] leftItems, object[] rightItems)
        {
            var cbd = new CommandBarDefinition();
            AddItems(cbd.LeftCommandBarEntries, leftItems);
            AddItems(cbd.RightCommandBarEntries, rightItems);
            return cbd;
        }

        private static void AddItems(CommandBarEntryCollection entries, object[] items)
        {
            foreach (var item in items)
            {
                if (item is string || item is CommandId)
                {
                    var commandIdentifier = item.ToString();
                    if (commandIdentifier == "-")
                    {
                        entries.Add(new CommandBarSeparatorEntry());
                    }
                    else if (commandIdentifier == " ")
                    {
                        entries.Add(new CommandBarSpacerEntry());
                    }
                    else
                    {
                        var cbbe = new CommandBarButtonEntry
                        {
                            CommandIdentifier = commandIdentifier
                        };
                        entries.Add(cbbe);
                    }
                }
                else if (item is CommandBarEntry)
                {
                    entries.Add((CommandBarEntry)item);
                }
                else
                {
                    Trace.Fail("Unexpected command bar definition item");
                    throw new ArgumentException("Unexpected command bar definition item");
                }
            }
        }
    }
}
