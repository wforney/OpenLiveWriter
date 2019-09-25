// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Windows.Forms;

    using ApplicationFramework;

    using HtmlEditor;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// Utility class used to make text editing command implementations fully self enclosed
        /// </summary>
        private abstract class TextEditingCommand
        {
            /// <summary>
            /// The dispatcher
            /// </summary>
            private TextEditingCommandDispatcher dispatcher;

            /// <summary>
            /// Gets the command identifier.
            /// </summary>
            /// <value>The command identifier.</value>
            public abstract CommandId CommandId { get; }

            /// <summary>
            /// Gets a value indicating whether [manage aggressively].
            /// </summary>
            /// <value><c>true</c> if [manage aggressively]; otherwise, <c>false</c>.</value>
            public virtual bool ManageAggressively => false;

            /// <summary>
            /// Gets the context menu text.
            /// </summary>
            /// <value>The context menu text.</value>
            public virtual string ContextMenuText => null;

            /// <summary>
            /// Gets the command.
            /// </summary>
            /// <value>The command.</value>
            public Command Command { get; private set; }

            /// <summary>
            /// Sets a value indicating whether this <see cref="TextEditingCommand"/> is enabled.
            /// </summary>
            /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
            protected bool Enabled
            {
                set => this.Command.Enabled = value;
            }

            /// <summary>
            /// Sets a value indicating whether this <see cref="TextEditingCommand"/> is latched.
            /// </summary>
            /// <value><c>true</c> if latched; otherwise, <c>false</c>.</value>
            protected bool Latched
            {
                set => this.Command.Latched = value;
            }

            /// <summary>
            /// Gets the active simple text editor.
            /// </summary>
            /// <value>The active simple text editor.</value>
            protected ISimpleTextEditorCommandSource ActiveSimpleTextEditor => this.dispatcher.ActiveSimpleTextEditor;

            /// <summary>
            /// Gets the post editor.
            /// </summary>
            /// <value>The post editor.</value>
            protected IHtmlEditorCommandSource PostEditor => this.dispatcher.PostEditor;

            /// <summary>
            /// Gets the owner.
            /// </summary>
            /// <value>The owner.</value>
            protected IWin32Window Owner => this.dispatcher.owner;

            /// <summary>
            /// Gets the command bar button context menu definition.
            /// </summary>
            /// <value>The command bar button context menu definition.</value>
            public virtual CommandContextMenuDefinition CommandBarButtonContextMenuDefinition => null;

            /// <summary>
            /// Sets the context.
            /// </summary>
            /// <param name="dispatcher">The dispatcher.</param>
            /// <param name="command">The command.</param>
            public void SetContext(TextEditingCommandDispatcher dispatcher, Command command)
            {
                this.dispatcher = dispatcher;
                this.Command = command;
            }

            /// <summary>
            /// Called when [all commands initialized].
            /// </summary>
            public virtual void OnAllCommandsInitialized()
            {
            }

            /// <summary>
            /// Manages this instance.
            /// </summary>
            public abstract void Manage();

            /// <summary>
            /// Manages all.
            /// </summary>
            protected void ManageAll() => this.dispatcher.ManageCommands();

            /// <summary>
            /// Executes the specified sender.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="ea">The <see cref="EventArgs"/> instance containing the event data.</param>
            public void Execute(object sender, EventArgs ea)
            {
                this.Execute();
                this.Manage();
            }

            /// <summary>
            /// Executes the with arguments.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="ea">The ea.</param>
            public void ExecuteWithArgs(object sender, ExecuteEventHandlerArgs ea)
            {
                this.ExecuteWithArgs(ea);
                this.Manage();
            }

            /// <summary>
            /// Executes this instance.
            /// </summary>
            protected abstract void Execute();

            /// <summary>
            /// Executes the with arguments.
            /// </summary>
            /// <param name="args">The arguments.</param>
            protected virtual void ExecuteWithArgs(ExecuteEventHandlerArgs args) =>
                // @RIBBON TODO: Unify the Execute and ExecuteWithArgs events.
                this.Execute();

            /// <summary>
            /// Finds the command.
            /// </summary>
            /// <param name="commandId">The command identifier.</param>
            /// <returns>Command.</returns>
            protected Command FindCommand(CommandId commandId) => this.dispatcher.FindEditingCommand(commandId);

            /// <summary>
            /// Creates the command.
            /// </summary>
            /// <returns>Command.</returns>
            public virtual Command CreateCommand() => new OverridableCommand(this.CommandId);
        }
    }
}
