// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The RemoveLinkAndClearFormattingCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class RemoveLinkAndClearFormattingCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.RemoveLinkAndClearFormatting;

            /// <inheritdoc />
            protected override void Execute()
            {
                if (this.PostEditor.CanRemoveLink)
                {
                    this.PostEditor.RemoveLink();
                }

                if (this.PostEditor.CanApplyFormatting(CommandId.ClearFormatting))
                {
                    this.PostEditor.ClearFormatting();
                }
            }

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.PostEditor.CanRemoveLink ||
                               this.PostEditor.CanApplyFormatting(CommandId.ClearFormatting);
        }
    }
}
