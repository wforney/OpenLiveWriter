// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Class DesignModeMainFrameWindow.
    /// Implements the <see cref="SameThreadSimpleInvokeTarget" />
    /// Implements the <see cref="IMainFrameWindow" />
    /// </summary>
    /// <seealso cref="SameThreadSimpleInvokeTarget" />
    /// <seealso cref="IMainFrameWindow" />
    public class DesignModeMainFrameWindow : SameThreadSimpleInvokeTarget, IMainFrameWindow
    {
        /// <summary>
        /// Gets or sets the caption.
        /// </summary>
        /// <value>The caption.</value>
        public string Caption
        {
            get => string.Empty;
            set
            {
            }
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        public Point Location => Point.Empty;

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        public Size Size => Size.Empty;

        /// <summary>
        /// Activates this instance.
        /// </summary>
        public void Activate()
        {
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public void Update()
        {
        }

        /// <summary>
        /// Adds the owned form.
        /// </summary>
        /// <param name="form">The form.</param>
        public void AddOwnedForm(Form form)
        {
        }

        /// <summary>
        /// Removes the owned form.
        /// </summary>
        /// <param name="form">The form.</param>
        public void RemoveOwnedForm(Form form)
        {
        }

        /// <summary>
        /// Sets the status bar message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void SetStatusBarMessage(StatusMessage message)
        {
        }

        /// <summary>
        /// Pushes the status bar message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void PushStatusBarMessage(StatusMessage message)
        {
        }

        /// <summary>
        /// Pops the status bar message.
        /// </summary>
        public void PopStatusBarMessage()
        {
        }

        /// <summary>
        /// Performs the layout.
        /// </summary>
        public void PerformLayout()
        {
        }

        /// <summary>
        /// Invalidates this instance.
        /// </summary>
        public void Invalidate()
        {
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        /// Gets the handle to the window represented by the implementer.
        /// </summary>
        /// <value>The handle.</value>
        public IntPtr Handle => User32.GetForegroundWindow();

        /// <summary>
        /// Occurs when [size changed].
        /// </summary>
        public event EventHandler SizeChanged;

        /// <summary>
        /// Called when [size changed].
        /// </summary>
        protected void OnSizeChanged() =>
            // prevent compiler warnings
            SizeChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Occurs when [location changed].
        /// </summary>
        public event EventHandler LocationChanged;

        /// <summary>
        /// Called when [location changed].
        /// </summary>
        protected void OnLocationChanged() =>
            // prevent compiler warnings
            LocationChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Occurs when [deactivate].
        /// </summary>
        public event EventHandler Deactivate;

        /// <summary>
        /// Called when [deactivate].
        /// </summary>
        protected void OnDeactivate() =>
            // prevent compiler warnings
            Deactivate?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Occurs when [layout].
        /// </summary>
        public event LayoutEventHandler Layout;

        /// <summary>
        /// Handles the <see cref="E:Layout" /> event.
        /// </summary>
        /// <param name="ea">The <see cref="LayoutEventArgs"/> instance containing the event data.</param>
        protected void OnLayout(LayoutEventArgs ea) => Layout?.Invoke(this, ea);

        /// <summary>
        /// Called when [keyboard language changed].
        /// </summary>
        public void OnKeyboardLanguageChanged()
        {
        }
    }
}
