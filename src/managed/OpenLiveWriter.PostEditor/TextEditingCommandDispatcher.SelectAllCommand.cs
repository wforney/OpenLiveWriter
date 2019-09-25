// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The SelectAllCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class SelectAllCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.SelectAll;

            /// <inheritdoc />
            protected override void Execute() => this.ActiveSimpleTextEditor.SelectAll();

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.PostEditor.FullyEditableRegionActive;
        }
    }
}
