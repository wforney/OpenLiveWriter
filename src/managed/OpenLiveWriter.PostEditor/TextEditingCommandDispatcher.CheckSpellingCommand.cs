// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{

    using ApplicationFramework;

    using Controls;

    using CoreServices;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The CheckSpellingCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class CheckSpellingCommand : TextEditingCommand
        {
            /// <summary>
            /// The is executing
            /// </summary>
            private bool isExecuting;

            /// <inheritdoc />
            public override CommandId CommandId => CommandId.CheckSpelling;

            /// <inheritdoc />
            protected override void Execute() => this.ExecuteWithArgs(new ExecuteEventHandlerArgs("OLECMDEXECOPT_DONTPROMPTUSER", "false"));

            /// <inheritdoc />
            protected override void ExecuteWithArgs(ExecuteEventHandlerArgs args)
            {
                try
                {
                    if (!this.isExecuting)
                    {
                        this.isExecuting = true;

                        var doNotShow = args.GetString("OLECMDEXECOPT_DONTPROMPTUSER");
                        var showFinishedUI = !StringHelper.ToBool(doNotShow, false);

                        if (!this.PostEditor.CheckSpelling())
                        {
                            args.Cancelled = true;
                            return;
                        }

                        if (showFinishedUI)
                        {
                            DisplayMessage.Show(MessageId.SpellCheckComplete, this.Owner);
                        }
                    }
                }
                finally
                {
                    this.isExecuting = false;
                }
            }

            /// <inheritdoc />
            public override void Manage() => this.Enabled = !this.PostEditor.ReadOnly && this.Command.On;

            /// <inheritdoc />
            public override Command CreateCommand() => new DontPromptUserCommand(CommandId.CheckSpelling);
        }
    }
}
