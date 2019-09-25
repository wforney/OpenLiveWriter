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
        /// The ItalicCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        private class ItalicCommand : LatchedTextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Italic;

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected override void Execute()
            {
                this.PostEditor.ApplyItalic();
                base.Execute();
            }

            /// <inheritdoc />
            public override void Manage()
            {
                this.Latched = this.PostEditor.SelectionItalic;
                this.Enabled = this.PostEditor.CanApplyFormatting(this.CommandId);
                this.PostEditor.CommandManager.Invalidate(this.CommandId);
            }

            /// <inheritdoc />
            public override Command CreateCommand() =>
                new LetterCommand(this.CommandId, Res.Get(StringId.ToolbarItalicLetter)[0], FontStyle.Italic);
        }
    }
}
