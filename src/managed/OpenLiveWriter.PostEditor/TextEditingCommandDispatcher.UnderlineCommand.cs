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
        /// The UnderlineCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        private class UnderlineCommand : LatchedTextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Underline;

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected override void Execute()
            {
                this.PostEditor.ApplyUnderline();
                base.Execute();
            }

            /// <inheritdoc />
            public override void Manage()
            {
                this.Latched = this.PostEditor.SelectionUnderlined;
                this.Enabled = this.PostEditor.CanApplyFormatting(this.CommandId);
                this.PostEditor.CommandManager.Invalidate(this.CommandId);
            }

            /// <inheritdoc />
            public override Command CreateCommand() =>
                new LetterCommand(this.CommandId, Res.Get(StringId.ToolbarUnderlineLetter)[0], FontStyle.Underline);
        }
    }
}
