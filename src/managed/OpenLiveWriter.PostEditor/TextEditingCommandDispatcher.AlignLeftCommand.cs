// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using HtmlEditor;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The AlignLeftCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        private class AlignLeftCommand : AlignCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AlignLeftCommand"/> class.
            /// </summary>
            /// <inheritdoc />
            public AlignLeftCommand() : base(EditorTextAlignment.Left)
            {
            }

            /// <inheritdoc />
            public override CommandId CommandId => CommandId.AlignLeft;

            /// <inheritdoc />
            public override string ContextMenuText => this.Command.MenuText;
        }
    }
}
