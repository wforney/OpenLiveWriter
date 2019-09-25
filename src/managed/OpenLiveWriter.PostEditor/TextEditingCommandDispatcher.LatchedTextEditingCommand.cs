// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using Interop.Com.Ribbon;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The LatchedTextEditingCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private abstract class LatchedTextEditingCommand : TextEditingCommand
        {
            // The ribbon maintains internal state about a command's latched value.
            // Normally, the toggle behavior (i.e. clicking on an unlatched command causes it to become latched) is what we want.
            // However, there are some commands that should not behave like toggle buttons in source mode.
            // We need to invalidate the latched property (BooleanValue) when we execute for these commands.
            /// <inheritdoc />
            protected override void Execute() => this.Command.Invalidate(new[] { PropertyKeys.BooleanValue });
        }
    }
}
