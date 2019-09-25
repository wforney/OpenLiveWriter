// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The ClearCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class ClearCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Clear;

            /// <inheritdoc />
            protected override void Execute() => this.ActiveSimpleTextEditor.Clear();

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.ActiveSimpleTextEditor.CanClear;
        }
    }
}
