// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The EditLinkCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class EditLinkCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.EditLink;

            // WinLive 276086: 'Edit hyperlink' context menu item shows up blank
            // A string was accidentally removed, it's too late now to add it back.
            // Fortunately, we have a duplicate string already in the resources.
            // Fix this in W5 MQ.
            /// <inheritdoc />
            public override string ContextMenuText => Res.Get(StringId.LinkEditHyperlink);

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.InsertLink();

            /// <inheritdoc />
            public override void Manage()
            {
            }
        }
    }
}
