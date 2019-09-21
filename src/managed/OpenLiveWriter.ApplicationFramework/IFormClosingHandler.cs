// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;

    /// <summary>
    /// Interface for optionally intercepting the Form.Closing event
    /// </summary>
    public interface IFormClosingHandler
    {
        /// <summary>
        /// Called when [closed].
        /// </summary>
        void OnClosed();

        /// <summary>
        /// Handles the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        void OnClosing(CancelEventArgs e);
    }
}
