// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    public partial class CommandBarButtonLightweightControl
    {
        /// <summary>
        /// Class EndShowContextMenuDisposable.
        /// Implements the <see cref="System.IDisposable" />
        /// </summary>
        /// <seealso cref="System.IDisposable" />
        private class EndShowContextMenuDisposable : IDisposable
        {
            /// <summary>
            /// The control
            /// </summary>
            private readonly CommandBarButtonLightweightControl control;

            /// <summary>
            /// The disposed
            /// </summary>
            private bool disposed = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="EndShowContextMenuDisposable"/> class.
            /// </summary>
            /// <param name="control">The control.</param>
            public EndShowContextMenuDisposable(CommandBarButtonLightweightControl control) => this.control = control;

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                if (!this.disposed)
                {
                    this.disposed = true;
                    this.control.EndShowContextMenu();
                }
            }
        }
    }
}
