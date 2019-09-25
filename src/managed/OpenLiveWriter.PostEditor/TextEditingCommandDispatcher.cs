// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Linq;
    using System.Windows.Forms;

    using ApplicationFramework;

    using HtmlEditor;
    using HtmlEditor.Controls;
    using Interop.Com.Ribbon;

    using Localization;

    /// <summary>
    /// Delegate TextEditingFocusHandler
    /// </summary>
    public delegate void TextEditingFocusHandler();

    /// <summary>
    /// The TextEditingCommandDispatcher class.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public partial class TextEditingCommandDispatcher : IDisposable
    {
        /// <summary>
        /// The command manager
        /// </summary>
        private readonly CommandManager commandManager;

        /// <summary>
        /// The command align center
        /// </summary>
        private Command commandAlignCenter;

        /// <summary>
        /// The command align justify
        /// </summary>
        private Command commandAlignJustify;

        /// <summary>
        /// The command align left
        /// </summary>
        private Command commandAlignLeft;

        /// <summary>
        /// The command align right
        /// </summary>
        private Command commandAlignRight;

        /// <summary>
        /// The focus callback
        /// </summary>
        private TextEditingFocusHandler focusCallback;

        /// <summary>
        /// The owner
        /// </summary>
        private readonly IWin32Window owner;

        /// <summary>
        /// The simple text editors
        /// </summary>
        private readonly ArrayList simpleTextEditors = new ArrayList();

        /// <summary>
        /// The style picker
        /// </summary>
        private readonly IHtmlStylePicker stylePicker;

        /// <summary>
        /// The text editing commands
        /// </summary>
        private readonly ArrayList textEditingCommands = new ArrayList();

        /// <summary>
        /// The command font family
        /// </summary>
        private FontFamilyCommand commandFontFamily;

        /// <summary>
        /// The command font size
        /// </summary>
        private FontSizeCommand commandFontSize;

        /// <summary>
        /// The components
        /// </summary>
        private readonly IContainer components = new Container();

        /// <summary>
        /// The font color picker command
        /// </summary>
        private FontColorPickerCommand fontColorPickerCommand;

        /// <summary>
        /// The highlight color picker command
        /// </summary>
        private FontColorPickerCommand highlightColorPickerCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextEditingCommandDispatcher"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="stylePicker">The style picker.</param>
        /// <param name="commandManager">The command manager.</param>
        public TextEditingCommandDispatcher(IWin32Window owner, IHtmlStylePicker stylePicker,
                                            CommandManager commandManager)
        {
            this.commandManager = commandManager;
            this.owner = owner;
            this.stylePicker = stylePicker;
            this.InitializeCommands();
        }

        /// <summary>
        /// Gets the active simple text editor.
        /// </summary>
        /// <value>The active simple text editor.</value>
        private ISimpleTextEditorCommandSource ActiveSimpleTextEditor
        {
            get
            {
                // if an editor has focus then it is considered active
                foreach (ISimpleTextEditorCommandSource simpleTextEditor in this.simpleTextEditors)
                {
                    if (simpleTextEditor.HasFocus)
                    {
                        return simpleTextEditor;
                    }
                }

                // otherwise return the main content editor
                return this.PostEditor;
            }
        }

        /// <summary>
        /// Gets the post editor.
        /// </summary>
        /// <value>The post editor.</value>
        private IHtmlEditorCommandSource PostEditor { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (ISimpleTextEditorCommandSource commandSource in this.simpleTextEditors)
            {
                commandSource.CommandStateChanged -= this.simpleTextEditor_CommandStateChanged;
                commandSource.AggressiveCommandStateChanged -=
                    this.simpleTextEditor_AggressiveCommandStateChanged;
            }

            this.components?.Dispose();
        }

        /// <summary>
        /// Registers the post editor.
        /// </summary>
        /// <param name="postEditor">The post editor.</param>
        /// <param name="componentContext">The component context.</param>
        /// <param name="textEditingFocusHandler">The focus callback.</param>
        public void RegisterPostEditor(IHtmlEditorCommandSource postEditor,
                                       IHtmlEditorComponentContextDelegate componentContext,
                                       TextEditingFocusHandler textEditingFocusHandler)
        {
            this.PostEditor = postEditor;
            this.focusCallback = textEditingFocusHandler;
            this.RegisterSimpleTextEditor(postEditor);
            this.commandFontSize.RegisterPostEditor(this.PostEditor);
            this.commandFontSize.ComponentContext = componentContext;
            this.commandFontSize.Invalidate();

            this.commandFontFamily.RegisterPostEditor(this.PostEditor);
            this.commandFontFamily.ComponentContext = componentContext;
            this.commandFontFamily.Invalidate();

            this.fontColorPickerCommand.ComponentContext = componentContext;
            this.highlightColorPickerCommand.ComponentContext = componentContext;
            this.highlightColorPickerCommand.Invalidate();
        }

        /// <summary>
        /// Registers the simple text editor.
        /// </summary>
        /// <param name="simpleTextEditor">The simple text editor.</param>
        public void RegisterSimpleTextEditor(ISimpleTextEditorCommandSource simpleTextEditor)
        {
            // add to our list of editors
            this.simpleTextEditors.Add(simpleTextEditor);

            // subscribe to command state changed event
            simpleTextEditor.CommandStateChanged += this.simpleTextEditor_CommandStateChanged;
            simpleTextEditor.AggressiveCommandStateChanged += this.simpleTextEditor_AggressiveCommandStateChanged;
        }

        /// <summary>
        /// Manages the commands.
        /// </summary>
        public void ManageCommands()
        {
            this.commandManager.BeginUpdate();
            try
            {
                // @RIBBON TODO: Perhaps we could be more efficient than just entirely invalidating here...
                this.commandFontFamily.Enabled = this.commandFontSize.Enabled =
                                                     this.fontColorPickerCommand.Enabled =
                                                         this.highlightColorPickerCommand.Enabled =
                                                             this.PostEditor.CanApplyFormatting(CommandId.Bold);
                this.commandFontFamily.Invalidate();
                this.commandFontSize.Invalidate();

                this.fontColorPickerCommand.SetSelectedColor(Color.FromArgb(this.PostEditor.SelectionForeColor),
                                                             SwatchColorType.RGB);

                var alignment = this.PostEditor.GetSelectionAlignment();
                this.commandAlignLeft.Enabled = this.PostEditor.CanApplyFormatting(CommandId.AlignLeft);
                this.commandAlignLeft.Latched = alignment == EditorTextAlignment.Left;
                this.commandAlignCenter.Enabled = this.PostEditor.CanApplyFormatting(CommandId.AlignCenter);
                this.commandAlignCenter.Latched = alignment == EditorTextAlignment.Center;
                this.commandAlignRight.Enabled = this.PostEditor.CanApplyFormatting(CommandId.AlignRight);
                this.commandAlignRight.Latched = alignment == EditorTextAlignment.Right;
                this.commandAlignJustify.Enabled = this.PostEditor.CanApplyFormatting(null);
                this.commandAlignJustify.Latched = alignment == EditorTextAlignment.Justify;

                this.PostEditor.CommandManager.Invalidate(CommandId.AlignLeft);
                this.PostEditor.CommandManager.Invalidate(CommandId.AlignCenter);
                this.PostEditor.CommandManager.Invalidate(CommandId.AlignRight);
                this.PostEditor.CommandManager.Invalidate(CommandId.Justify);

                foreach (TextEditingCommand textEditingCommand in this.textEditingCommands)
                {
                    textEditingCommand.Manage();
                }
            }
            finally
            {
                this.commandManager.EndUpdate();
            }
        }

        /// <summary>
        /// Aggressives the manage commands.
        /// </summary>
        public void AggressiveManageCommands()
        {
            this.commandManager.BeginUpdate();
            try
            {
                foreach (TextEditingCommand textEditingCommand in this.textEditingCommands)
                {
                    if (textEditingCommand.ManageAggressively)
                    {
                        textEditingCommand.Manage();
                    }
                }
            }
            finally
            {
                this.commandManager.EndUpdate();
            }
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.commandManager.BeginUpdate();

            this.InitializeCommand(new UndoCommand());
            this.InitializeCommand(new RedoCommand());
            this.InitializeCommand(new CutCommand());
            this.InitializeCommand(new CopyCommand());
            this.InitializeCommand(new PasteCommand());
            if (GlobalEditorOptions.SupportsFeature(ContentEditorFeature.SpecialPaste))
            {
                this.InitializeCommand(new PasteSpecialCommand());
            }

            this.InitializeCommand(new ClearCommand());
            this.InitializeCommand(new SelectAllCommand());
            this.InitializeCommand(new BoldCommand());
            this.InitializeCommand(new ItalicCommand());
            this.InitializeCommand(new UnderlineCommand());
            this.InitializeCommand(new StrikethroughCommand());
            this.InitializeCommand(new StyleCommand(this.stylePicker, this.FocusEditor));
            this.InitializeCommand(new AlignLeftCommand());
            this.InitializeCommand(new AlignCenterCommand());
            this.InitializeCommand(new AlignJustifyCommand());
            this.InitializeCommand(new AlignRightCommand());
            this.InitializeCommand(new NumbersCommand());
            this.InitializeCommand(new BulletsCommand());
            this.InitializeCommand(new BlockquoteCommand());
            this.InitializeCommand(new PrintCommand());
            this.InitializeCommand(new PrintPreviewCommand());
            this.InitializeCommand(new IndentCommand());
            this.InitializeCommand(new OutdentCommand());
            this.InitializeCommand(new LTRTextBlockCommand());
            this.InitializeCommand(new RTLTextBlockCommand());
            this.InitializeCommand(new InsertLinkCommand());
            this.InitializeCommand(new FindCommand());
            this.InitializeCommand(new CheckSpellingCommand());
            this.InitializeCommand(new EditLinkCommand());
            this.InitializeCommand(new RemoveLinkCommand());
            this.InitializeCommand(new RemoveLinkAndClearFormattingCommand());
            this.InitializeCommand(new OpenLinkCommand());
            this.InitializeCommand(new AddToGlossaryCommand());
            this.InitializeCommand(new SuperscriptCommand());
            this.InitializeCommand(new SubscriptCommand());
            this.InitializeCommand(new ClearFormattingCommand());

            this.commandFontSize = new FontSizeCommand();
            this.commandManager.Add(this.commandFontSize);

            this.commandFontFamily = new FontFamilyCommand();
            this.commandManager.Add(this.commandFontFamily);

            this.fontColorPickerCommand = new FontColorPickerCommand(CommandId.FontColorPicker, Color.Black);
            this.commandManager.Add(this.fontColorPickerCommand, this.fontColorPickerCommand_Execute);

            this.highlightColorPickerCommand = new FontHighlightColorPickerCommand(Color.Yellow);
            this.commandManager.Add(this.highlightColorPickerCommand, this.highlightColorPickerCommand_Execute);

            this.commandAlignLeft = this.FindEditingCommand(CommandId.AlignLeft);
            this.commandAlignCenter = this.FindEditingCommand(CommandId.AlignCenter);
            this.commandAlignRight = this.FindEditingCommand(CommandId.AlignRight);
            this.commandAlignJustify = this.FindEditingCommand(CommandId.Justify);

            this.commandManager.EndUpdate();

            // notify all of our commands that initialization is complete
            foreach (TextEditingCommand textEditingCommand in this.textEditingCommands)
            {
                textEditingCommand.OnAllCommandsInitialized();
            }
        }

        /// <summary>
        /// Fonts the color picker command execute.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void fontColorPickerCommand_Execute(object sender, ExecuteEventHandlerArgs e)
        {
            //if (fontColorPickerCommand.Automatic)
            //    PostEditor.ApplyAutomaticFontForeColor();
            //else
            var color = e.GetColor("SelectedColor");
            this.PostEditor.ApplyFontForeColor(color.ToArgb());
        }

        /// <summary>
        /// Highlights the color picker command execute.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void highlightColorPickerCommand_Execute(object sender, ExecuteEventHandlerArgs e)
        {
            Color? color = null;
            if (e.HasArg("SelectedColor"))
            {
                color = e.GetColor("SelectedColor");
                this.highlightColorPickerCommand.SetSelectedColor(color.Value, SwatchColorType.RGB);
            }
            else
            {
                this.highlightColorPickerCommand.SetSelectedColor(Color.White, SwatchColorType.NoColor);
            }

            this.PostEditor.ApplyFontBackColor(color?.ToArgb());
        }

        /// <summary>
        /// Initializes the command.
        /// </summary>
        /// <param name="textEditingCommand">The text editing command.</param>
        private void InitializeCommand(TextEditingCommand textEditingCommand)
        {
            // create the command instance
            var command = textEditingCommand.CreateCommand();

            // hookup the command implementation to the dispatcher and command instance
            textEditingCommand.SetContext(this, command);

            // add optional context menu text override
            if (textEditingCommand.ContextMenuText != null)
            {
                command.MenuText = textEditingCommand.ContextMenuText;
            }

            // hookup the command to its execute handler
            command.Execute += textEditingCommand.Execute;
            command.ExecuteWithArgs += textEditingCommand.ExecuteWithArgs;

            command.CommandBarButtonContextMenuDefinition = textEditingCommand.CommandBarButtonContextMenuDefinition;

            // add to the command manager
            this.commandManager.Add(command);

            // add to our internal list
            this.textEditingCommands.Add(textEditingCommand);
        }

        /// <summary>
        /// Focuses the editor.
        /// </summary>
        private void FocusEditor() => this.focusCallback();

        /// <summary>
        /// Handles the BeforeShowInMenu event of the editingCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ea">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void editingCommand_BeforeShowInMenu(object sender, EventArgs ea) => this.ManageCommands();

        /// <summary>
        /// Handles the CommandStateChanged event of the simpleTextEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void simpleTextEditor_CommandStateChanged(object sender, EventArgs e) => this.ManageCommands();

        /// <summary>
        /// Handles the AggressiveCommandStateChanged event of the simpleTextEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void simpleTextEditor_AggressiveCommandStateChanged(object sender, EventArgs e) => this.AggressiveManageCommands();

        /// <summary>
        /// Titles the focus changed.
        /// </summary>
        public void TitleFocusChanged() => this.ManageCommands();

        /// <summary>
        /// Finds the editing command.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <returns>Command.</returns>
        private Command FindEditingCommand(CommandId commandId) =>
            // null if none found
            this.textEditingCommands.Cast<TextEditingCommand>()
                .Where(textEditingCommand => textEditingCommand.CommandId == commandId)
                .Select(textEditingCommand => textEditingCommand.Command).FirstOrDefault();

        // Win Live 182580: Edit options in the ribbon like Font formatting etc should be disabled for photo albums
        // Here's the set of commands that we will disable when a photo album is selected.
        /// <summary>
        /// Determines whether [is font formatting command] [the specified identifier].
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns><c>true</c> if [is font formatting command] [the specified identifier]; otherwise, <c>false</c>.</returns>
        public static bool IsFontFormattingCommand(CommandId? id)
        {
            switch (id)
            {
                case CommandId.Bold:
                case CommandId.Italic:
                case CommandId.Underline:
                case CommandId.Strikethrough:
                case CommandId.Superscript:
                case CommandId.Subscript:
                case CommandId.FontColorPicker:
                case CommandId.FontBackgroundColor:
                case CommandId.ClearFormatting:
                case CommandId.FontSize:
                case CommandId.FontFamily:
                case CommandId.Numbers:
                case CommandId.Bullets:
                case CommandId.Indent:
                case CommandId.Outdent:
                    return true;
                default:
                    return false;
            }
        }
    }
}
