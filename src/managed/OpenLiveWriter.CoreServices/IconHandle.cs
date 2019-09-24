// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Drawing;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Class that encapsulates a Win32 Icon Handle. The class can be implicitly
    /// converted to a .NET Icon. The class must be disposed when the caller
    /// is finished with using the Icon (this frees the HANDLE via DestroyIcon
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class IconHandle : IDisposable
    {
        /// <summary>
        /// .NET Icon for HICON
        /// </summary>
        private Icon icon;

        /// <summary>
        /// Initializes a new instance of the <see cref="IconHandle"/> class.
        /// </summary>
        /// <param name="hIcon">
        /// The h icon.
        /// </param>
        public IconHandle(IntPtr hIcon) => this.Handle = hIcon;

        /// <summary>
        /// Underlying HICON
        /// </summary>
        /// <value>The handle.</value>
        public IntPtr Handle { get; } = IntPtr.Zero;

        /// <summary>
        /// .NET Icon for HICON (tied to underlying HICON)
        /// </summary>
        /// <value>The icon.</value>
        public Icon Icon => this.icon ?? (this.icon = Icon.FromHandle(this.Handle));

        /// <summary>
        /// Dispose by destroying underlying HICON (makes all .NET icons returned
        /// from the Icon property invalid)
        /// </summary>
        public void Dispose() => User32.DestroyIcon(this.Handle);
    }
}
