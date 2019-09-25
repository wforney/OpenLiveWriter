// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using HtmlEditor;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The AlignJustifyCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        private class AlignJustifyCommand : AlignCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AlignJustifyCommand"/> class.
            /// </summary>
            /// <inheritdoc />
            public AlignJustifyCommand() : base(EditorTextAlignment.Justify)
            {
            }

            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Justify;

            /// <inheritdoc />
            public override string ContextMenuText => this.Command.MenuText;
        }
    }
}
