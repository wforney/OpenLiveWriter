// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;

    using Localization;
    using Localization.Bidi;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The RTLTextBlockCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.LatchedTextEditingCommand" />
        private class RTLTextBlockCommand : LatchedTextEditingCommand
        {
            /// <inheritdoc />
            public override CommandId CommandId => CommandId.RTLTextBlock;

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected override void Execute()
            {
                this.PostEditor.InsertRTLTextBlock();
                base.Execute();
            }

            /// <inheritdoc />
            public override void Manage()
            {
                var rtlState = ((ContentEditor) this.PostEditor).HasRTLFeatures || BidiHelper.IsRightToLeft;

                //Command.VisibleOnCommandBar = rtlState;
                if (this.Command.On != rtlState)
                {
                    this.Command.On = rtlState;
                    this.PostEditor.CommandManager.OnChanged(EventArgs.Empty);
                }

                //latched is a semi-intensive check, so only do it if command is on/visible!
                if (this.Command.On)
                {
                    this.Latched = this.PostEditor.SelectionIsRTL;
                }

                this.Enabled = rtlState && this.PostEditor.CanApplyFormatting(this.CommandId);
            }
        }
    }
}
