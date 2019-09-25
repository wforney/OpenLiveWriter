// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The RemoveLinkCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class RemoveLinkCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.RemoveLink;

            /// <inheritdoc />
            protected override void Execute()
            {
                if (this.PostEditor.CanRemoveLink)
                {
                    this.PostEditor.RemoveLink();
                }
            }

            /// <inheritdoc />
            public override void Manage() =>
                // tie enabled state to Insert Link -- it looks odd to have
                // Remove Link disabled on the command bar right next to
                // Insert Link -- the command no-ops in the case where it
                // is invalid for the current context (see Execute above)
                this.Enabled = this.PostEditor.CanInsertLink;
        }
    }
}
