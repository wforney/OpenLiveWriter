// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The PasteSpecialCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class PasteSpecialCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.PasteSpecial;

            /// <inheritdoc />
            public override string ContextMenuText => this.Command.MenuText;

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.PasteSpecial();

            /// <inheritdoc />
            public override void Manage() =>
                // For some reason the next line does
                // not cause the main menu to be rebuilt. This causes the
                // Paste Special command to not show up in the main menu.
                //
                // this.Command.On = PostEditor.AllowPasteSpecial ;

                this.Enabled = this.PostEditor.AllowPasteSpecial && this.PostEditor.CanPasteSpecial;
        }
    }
}
