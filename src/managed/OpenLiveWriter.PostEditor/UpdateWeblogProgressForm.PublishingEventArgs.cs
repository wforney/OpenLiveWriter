// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;

    public partial class UpdateWeblogProgressForm
    {
        /// <summary>
        /// The PublishingEventArgs class.
        /// Implements the <see cref="System.EventArgs" />
        /// </summary>
        /// <seealso cref="System.EventArgs" />
        public class PublishingEventArgs : EventArgs
        {
            /// <summary>
            /// The publish
            /// </summary>
            public readonly bool Publish;

            /// <summary>
            /// The republish on success
            /// </summary>
            public bool RepublishOnSuccess = false;

            /// <summary>
            /// Initializes a new instance of the <see cref="PublishingEventArgs"/> class.
            /// </summary>
            /// <param name="publish">if set to <c>true</c> [publish].</param>
            public PublishingEventArgs(bool publish) => this.Publish = publish;
        }
    }
}
