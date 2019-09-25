// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Drawing;

    using ApplicationFramework;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The StrikethroughCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        private class StrikethroughCommand : LatchedTextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Strikethrough;

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected override void Execute()
            {
                this.PostEditor.ApplyStrikethrough();
                base.Execute();
            }

            /// <inheritdoc />
            public override void Manage()
            {
                this.Latched = this.PostEditor.SelectionStrikethrough;
                this.Enabled = this.PostEditor.CanApplyFormatting(this.CommandId);
                this.PostEditor.CommandManager.Invalidate(this.CommandId);
            }

            /// <inheritdoc />
            public override Command CreateCommand() =>
                new LetterCommand(this.CommandId, Res.Get(StringId.ToolbarStrikethroughLetter)[0], FontStyle.Strikeout);
        }
    }
}
