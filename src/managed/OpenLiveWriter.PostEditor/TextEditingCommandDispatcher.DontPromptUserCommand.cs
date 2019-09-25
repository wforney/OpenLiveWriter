// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    using ApplicationFramework;

    using Interop.Com;
    using Interop.Com.Ribbon;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The DontPromptUserCommand class.
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.Command" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.Command" />
        internal class DontPromptUserCommand : Command
        {
            /// <inheritdoc />
            public DontPromptUserCommand(CommandId commandId)
                : base(commandId)
            {
            }

            /// <inheritdoc />
            public override int PerformExecute(CommandExecutionVerb verb, PropertyKeyRef key,
                                               PropVariantRef currentValue,
                                               IUISimplePropertySet commandExecutionProperties)
            {
                // Mail will call us with a special parameter sometimes
                // to let us know we should silence the spellchecker if possible
                Debug.Assert(this.CommandId == CommandId.CheckSpelling);
                if (commandExecutionProperties != null)
                {
                    var doNotPromptKey = new PropertyKey(3001, VarEnum.VT_BOOL);
                    var returnValue = commandExecutionProperties.GetValue(ref doNotPromptKey, out var doNotPromptValue);
                    if (returnValue == 0)
                    {
                        var eventArgs =
                            new ExecuteEventHandlerArgs("OLECMDEXECOPT_DONTPROMPTUSER",
                                                        doNotPromptValue.Value.ToString());
                        this.PerformExecuteWithArgs(eventArgs);

                        // The user cancelled, HRESULT.S_FALSE will tell Mail to stop sending the email
                        return eventArgs.Cancelled ? HRESULT.S_FALSE : HRESULT.S_OK;
                    }
                }

                return base.PerformExecute(verb, key, currentValue, commandExecutionProperties);
            }
        }
    }
}
