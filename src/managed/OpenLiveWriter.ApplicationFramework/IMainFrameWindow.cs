// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;
    using OpenLiveWriter.Controls;

    /// <summary>
    /// Interface IMainFrameWindow
    /// Implements the <see cref="IWin32Window" />
    /// Implements the <see cref="ISynchronizeInvoke" />
    /// Implements the <see cref="IMiniFormOwner" />
    /// </summary>
    /// <seealso cref="IWin32Window" />
    /// <seealso cref="ISynchronizeInvoke" />
    /// <seealso cref="IMiniFormOwner" />
    public interface IMainFrameWindow : IWin32Window, ISynchronizeInvoke, IMiniFormOwner
    {
        /// <summary>
        /// Sets the caption.
        /// </summary>
        /// <value>The caption.</value>
        string Caption { set; }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        Point Location { get; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        Size Size { get; }

        /// <summary>
        /// Occurs when [location changed].
        /// </summary>
        event EventHandler LocationChanged;

        /// <summary>
        /// Occurs when [size changed].
        /// </summary>
        event EventHandler SizeChanged;

        /// <summary>
        /// Occurs when [deactivate].
        /// </summary>
        event EventHandler Deactivate;

        /// <summary>
        /// Occurs when [layout].
        /// </summary>
        event LayoutEventHandler Layout;

        /// <summary>
        /// Activates this instance.
        /// </summary>
        void Activate();

        /// <summary>
        /// Updates this instance.
        /// </summary>
        void Update();

        /// <summary>
        /// Performs the layout.
        /// </summary>
        void PerformLayout();

        /// <summary>
        /// Invalidates this instance.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        void Close();

        /// <summary>
        /// Called when [keyboard language changed].
        /// </summary>
        void OnKeyboardLanguageChanged();
    }
}
