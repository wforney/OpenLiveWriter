// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The InsertLinkCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class InsertLinkCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.InsertLink;

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.InsertLink();

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.PostEditor.CanInsertLink;
        }
    }
}
