// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using OpenLiveWriter.Localization;

    public class CommandManagerExecuteEventArgs : EventArgs
    {
        public CommandId CommandId { get; set; }

        public CommandManagerExecuteEventArgs(CommandId commandId) => this.CommandId = commandId;
    }
}
