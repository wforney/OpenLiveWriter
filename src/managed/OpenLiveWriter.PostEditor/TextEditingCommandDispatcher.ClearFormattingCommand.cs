// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using ApplicationFramework;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The ClearFormattingCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class ClearFormattingCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.ClearFormatting;

            /// <inheritdoc />
            protected override void Execute()
            {
                this.PostEditor.ClearFormatting();
                this.ManageAll();
            }

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.PostEditor.CanApplyFormatting(this.CommandId);

            /// <inheritdoc />
            public override Command CreateCommand() => new OverridableCommand(this.CommandId);
        }
    }
}
