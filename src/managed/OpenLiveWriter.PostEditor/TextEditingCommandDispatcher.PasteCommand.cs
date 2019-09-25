// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using CoreServices.Diagnostics;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The PasteCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class PasteCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Paste;

            /// <inheritdoc />
            protected override void Execute()
            {
                using (ApplicationPerformance.LogEvent("Paste"))
                {
                    this.ActiveSimpleTextEditor.Paste();
                }
            }

            /// <inheritdoc />
            public override void Manage() => this.Enabled = this.ActiveSimpleTextEditor.CanPaste;
        }
    }
}
