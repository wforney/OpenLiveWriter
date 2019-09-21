// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;

    public partial class CommandBarButtonLightweightControl
    {
        /// <summary>
        /// Encapsulates the relevant state from a Command that determines
        /// whether a re-layout is needed.
        /// </summary>
        private class CommandState
        {
            /// <summary>
            /// The style
            /// </summary>
            private readonly CommandBarButtonStyle style;

            /// <summary>
            /// The text
            /// </summary>
            private readonly string text;

            /// <summary>
            /// The on
            /// </summary>
            private readonly bool on;

            /// <summary>
            /// The visible on command bar
            /// </summary>
            private readonly bool visibleOnCommandBar;

            /// <summary>
            /// The image size
            /// </summary>
            private readonly Size imageSize;

            /// <summary>
            /// Initializes a new instance of the <see cref="CommandState"/> class.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="imageSize">Size of the image.</param>
            public CommandState(Command command, Size imageSize)
            {
                this.style = command.CommandBarButtonStyle;
                this.text = command.Text;
                this.on = command.On;
                this.visibleOnCommandBar = command.VisibleOnCommandBar;
                this.imageSize = imageSize;
            }

            /// <summary>
            /// Needses the layout.
            /// </summary>
            /// <param name="command">The command.</param>
            /// <param name="imageSize">Size of the image.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool NeedsLayout(Command command, Size imageSize) => this.style != command.CommandBarButtonStyle
                       || this.on != command.On
                       || this.visibleOnCommandBar != command.VisibleOnCommandBar
                       || command.Text != this.text
                       || !this.imageSize.Equals(imageSize);
        }
    }
}
