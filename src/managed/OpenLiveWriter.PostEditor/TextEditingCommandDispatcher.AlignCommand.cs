// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using HtmlEditor;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The AlignCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        private abstract class AlignCommand : LatchedTextEditingCommand
        {
            /// <summary>
            /// The alignment
            /// </summary>
            private readonly EditorTextAlignment alignment;

            /// <summary>
            /// Initializes a new instance of the <see cref="AlignCommand"/> class.
            /// </summary>
            /// <param name="alignment">The alignment.</param>
            protected AlignCommand(EditorTextAlignment alignment) => this.alignment = alignment;

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected override void Execute()
            {
                // if we are already latched then this means remove formatting
                this.PostEditor.ApplyAlignment(this.Command.Latched ? EditorTextAlignment.None : this.alignment);

                base.Execute();
            }

            /// <inheritdoc />
            public override void Manage()
            {
                // Do nothing, we will explicitly manage them all together in ManageCommands
            }
        }
    }
}
