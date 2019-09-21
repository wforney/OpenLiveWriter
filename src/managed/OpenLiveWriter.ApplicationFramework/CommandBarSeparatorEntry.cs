// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using OpenLiveWriter.Controls;

    /// <summary>
    /// Separator command bar entry.
    /// </summary>
    [
        DesignTimeVisible(false),
            ToolboxItem(false)
    ]
    public class CommandBarSeparatorEntry : CommandBarEntry
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorEntry class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarSeparatorEntry(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorEntry class.
        /// </summary>
        public CommandBarSeparatorEntry() =>
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();

        /// <summary>
        /// Gets the lightweight control for this entry.
        /// </summary>
        /// <returns>Lightweight control.</returns>
        public override LightweightControl GetLightweightControl(
            CommandBarLightweightControl commandBarLightweightControl,
            bool rightAligned) => new CommandBarSeparatorLightweightControl();
    }
}
