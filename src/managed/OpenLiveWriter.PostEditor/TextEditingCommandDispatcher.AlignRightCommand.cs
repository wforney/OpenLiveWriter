// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using HtmlEditor;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The AlignRightCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        private class AlignRightCommand : AlignCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AlignRightCommand"/> class.
            /// </summary>
            /// <inheritdoc />
            public AlignRightCommand() : base(EditorTextAlignment.Right)
            {
            }

            /// <inheritdoc />
            public override CommandId CommandId => CommandId.AlignRight;

            /// <inheritdoc />
            public override string ContextMenuText => this.Command.MenuText;
        }
    }
}
