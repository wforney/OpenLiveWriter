// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Windows.Forms;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.Localization;

    public partial class GDataCredentials
    {
        /// <summary>
        /// The ShowErrorHelper class.
        /// </summary>
        private class ShowErrorHelper
        {
            /// <summary>
            /// The arguments
            /// </summary>
            private readonly object[] args;

            /// <summary>
            /// The message identifier
            /// </summary>
            private readonly MessageId messageId;

            /// <summary>
            /// The owner
            /// </summary>
            private readonly IWin32Window owner;

            /// <summary>
            /// Initializes a new instance of the <see cref="ShowErrorHelper"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            /// <param name="messageId">The message identifier.</param>
            /// <param name="args">The arguments.</param>
            public ShowErrorHelper(IWin32Window owner, MessageId messageId, object[] args)
            {
                this.owner = owner;
                this.messageId = messageId;
                this.args = args;
            }

            /// <summary>
            /// Shows this instance.
            /// </summary>
            public void Show() => DisplayMessage.Show(this.messageId, this.owner, this.args);
        }
    }
}
