// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Windows.Forms;

    /// <summary>
    /// Interface IFrameManager
    /// </summary>
    public interface IFrameManager
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IFrameManager"/> is frameless.
        /// </summary>
        /// <value><c>true</c> if frameless; otherwise, <c>false</c>.</value>
        bool Frameless { get; }

        /// <summary>
        /// Adds the owned form.
        /// </summary>
        /// <param name="f">The f.</param>
        void AddOwnedForm(Form f);

        /// <summary>
        /// Paints the background.
        /// </summary>
        /// <param name="pevent">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        void PaintBackground(PaintEventArgs pevent);

        /// <summary>
        /// Removes the owned form.
        /// </summary>
        /// <param name="f">The f.</param>
        void RemoveOwnedForm(Form f);

        /// <summary>
        /// The window process
        /// </summary>
        /// <param name="m">The message.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        bool WndProc(ref Message m);
    }
}
