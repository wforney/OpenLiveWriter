// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    /// Lightweight splitter event arguments.
    /// </summary>
    public class LightweightSplitterEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the position of the splitter.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Initializes a new instance of the LightweightSplitterEventArgs class.
        /// </summary>
        /// <param name="position">The position of the splitter.</param>
        public LightweightSplitterEventArgs(int position) => this.Position = position;
    }
}
