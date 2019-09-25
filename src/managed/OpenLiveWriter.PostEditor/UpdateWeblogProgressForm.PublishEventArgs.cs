// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;

    public partial class UpdateWeblogProgressForm
    {
        /// <summary>
        /// The PublishEventArgs class.
        /// Implements the <see cref="System.EventArgs" />
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        public class PublishEventArgs : EventArgs
        {
            /// <summary>
            /// The publish
            /// </summary>
            public readonly bool Publish;

            /// <summary>
            /// The cancel
            /// </summary>
            public bool Cancel = false;

            /// <summary>
            /// The cancel reason
            /// </summary>
            public string CancelReason = null;

            /// <summary>
            /// Initializes a new instance of the <see cref="PublishEventArgs"/> class.
            /// </summary>
            /// <param name="publish">if set to <c>true</c> [publish].</param>
            public PublishEventArgs(bool publish) => this.Publish = publish;
        }
    }
}
