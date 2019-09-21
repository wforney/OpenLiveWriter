// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;

    /// <summary>
    /// Interface IMenuCommandObject
    /// </summary>
    public interface IMenuCommandObject
    {
        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>The image.</value>
        Bitmap Image { get; }

        /// <summary>
        /// Gets the caption.
        /// </summary>
        /// <value>The caption.</value>
        string Caption { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMenuCommandObject"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IMenuCommandObject"/> is latched.
        /// </summary>
        /// <value><c>true</c> if latched; otherwise, <c>false</c>.</value>
        bool Latched { get; }

        /// <summary>
        /// Gets the caption no mnemonic.
        /// </summary>
        /// <value>The caption no mnemonic.</value>
        string CaptionNoMnemonic { get; }
    }

}

