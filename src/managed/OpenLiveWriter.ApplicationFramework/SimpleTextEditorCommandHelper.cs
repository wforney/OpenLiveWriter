// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Windows.Forms;

    /// <summary>
    ///     Class SimpleTextEditorCommandHelper.
    /// </summary>
    public partial class SimpleTextEditorCommandHelper
    {
        /// <summary>
        ///     Call this method to ensure that the passed control
        ///     gets to handle cut, copy, paste, undo, redo, and del
        ///     natively instead of through the SimpleTextEditorCommand
        ///     system.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        /// <param name="controls">The controls.</param>
        /// <returns>An <see cref="IDisposable"/>.</returns>
        public static IDisposable UseNativeBehaviors(CommandManager commandManager, params Control[] controls) =>
            new NativeBehaviors(commandManager, controls);
    }
}