// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The IndentCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class IndentCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Indent;

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.ApplyIndent();

            /// <inheritdoc />
            public override void Manage()
            {
                if (this.Command.On)
                {
                    this.Enabled = this.PostEditor.CanIndent;
                }
            }
        }
    }
}
