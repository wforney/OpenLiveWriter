// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The AddToGlossaryCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class AddToGlossaryCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.AddToGlossary;

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.AddToGlossary();

            /// <inheritdoc />
            public override void Manage() =>
                // this command only appears on the context-menu for links,
                // so by default it is always enabled (if we don't do this
                // then it gets tied up in context-menu command management
                // funkiness, where sometimes it is enabled and sometimes
                // it is not
                this.Enabled = true;
        }
    }
}
