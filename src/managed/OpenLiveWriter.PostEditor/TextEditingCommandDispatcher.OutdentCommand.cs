// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The OutdentCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class OutdentCommand : TextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Outdent;

            /// <inheritdoc />
            protected override void Execute() => this.PostEditor.ApplyOutdent();

            /// <inheritdoc />
            public override void Manage()
            {
                if (this.Command.On)
                {
                    this.Enabled = this.PostEditor.CanOutdent;
                }
            }
        }
    }
}
