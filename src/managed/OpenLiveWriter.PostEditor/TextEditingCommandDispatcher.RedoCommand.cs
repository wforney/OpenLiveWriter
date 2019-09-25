// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The RedoCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class RedoCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Redo;

            /// <inheritdoc />
            public override bool ManageAggressively => true;

            /// <inheritdoc />
            protected override void Execute() => this.ActiveSimpleTextEditor.Redo();

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.ActiveSimpleTextEditor.CanRedo;
        }
    }
}
