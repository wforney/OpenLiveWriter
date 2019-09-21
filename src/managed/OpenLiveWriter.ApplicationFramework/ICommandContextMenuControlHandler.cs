// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Windows.Forms;

    /// <summary>
    /// Interface implemented to create and respond to user execute request
    /// for context menu controls
    /// </summary>
    public interface ICommandContextMenuControlHandler
    {
        /// <summary>
        /// Creates the control.
        /// </summary>
        /// <returns>Control.</returns>
        Control CreateControl();

        /// <summary>
        /// Gets the caption text.
        /// </summary>
        /// <value>The caption text.</value>
        string CaptionText { get; }

        /// <summary>
        /// Gets the button text.
        /// </summary>
        /// <value>The button text.</value>
        string ButtonText { get; }

        /// <summary>
        /// Gets the user input.
        /// </summary>
        /// <returns>System.Object.</returns>
        object GetUserInput();

        /// <summary>
        /// Executes the specified user input.
        /// </summary>
        /// <param name="userInput">The user input.</param>
        void Execute(object userInput);
    }
}
