// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using OpenLiveWriter.Controls;

    /// <summary>
    /// Command bar label entry.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class CommandBarLabelEntry : CommandBarEntry
    {
        /// <summary>
        ///	Gets or sets the text for this CommandBarLabelEntry.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The CommandBarLabelLightweightControl for this entry.
        /// </summary>
        private CommandBarLabelLightweightControl commandBarLabelLightweightControl;

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelEntry class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarLabelEntry(IContainer container)
        {
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarLabelEntry class.
        /// </summary>
        public CommandBarLabelEntry()
        {
        }

        /// <summary>
        /// Gets the lightweight control for this entry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public override LightweightControl GetLightweightControl(CommandBarLightweightControl commandBarLightweightControl, bool rightAligned)
        {
            if (this.commandBarLabelLightweightControl == null)
            {
                this.commandBarLabelLightweightControl = new CommandBarLabelLightweightControl(this.Text);
            }

            return this.commandBarLabelLightweightControl;
        }
    }
}
