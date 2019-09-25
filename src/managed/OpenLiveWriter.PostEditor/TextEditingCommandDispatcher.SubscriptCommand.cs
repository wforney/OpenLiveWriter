// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using ApplicationFramework;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The SubscriptCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        private class SubscriptCommand : LatchedTextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Subscript;

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected override void Execute()
            {
                this.PostEditor.ApplySubscript();
                base.Execute();
            }

            /// <inheritdoc />
            public override void Manage()
            {
                this.Latched = this.PostEditor.SelectionSubscript;
                this.Enabled = this.PostEditor.CanApplyFormatting(this.CommandId);
                this.PostEditor.CommandManager.Invalidate(this.CommandId);
            }

            /// <inheritdoc />
            public override Command CreateCommand() => new OverridableCommand(this.CommandId);
        }
    }
}
