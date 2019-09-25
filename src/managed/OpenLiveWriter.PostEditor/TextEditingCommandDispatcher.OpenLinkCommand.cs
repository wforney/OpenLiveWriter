// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The OpenLinkCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class OpenLinkCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.OpenLink;

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.OpenLink();

            /// <inheritdoc />
            public override void Manage()
            {
            }
        }
    }
}
