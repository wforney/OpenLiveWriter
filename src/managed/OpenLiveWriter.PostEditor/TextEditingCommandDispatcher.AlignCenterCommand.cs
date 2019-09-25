// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using HtmlEditor;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The AlignCenterCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.AlignCommand" />
        private class AlignCenterCommand : AlignCommand
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AlignCenterCommand"/> class.
            /// </summary>
            /// <inheritdoc />
            public AlignCenterCommand() : base(EditorTextAlignment.Center)
            {
            }

            /// <inheritdoc />
            public override CommandId CommandId => CommandId.AlignCenter;

            /// <inheritdoc />
            public override string ContextMenuText => this.Command.MenuText;
        }
    }
}
