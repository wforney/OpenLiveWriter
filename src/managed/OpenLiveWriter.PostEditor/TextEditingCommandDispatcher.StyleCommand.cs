// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;

    using Commands;
    using HtmlEditor.Controls;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The StyleCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.TextEditingCommandDispatcher.TextEditingCommand" />
        private class StyleCommand : TextEditingCommand
        {
            /// <summary>
            /// The focus callback
            /// </summary>
            private readonly TextEditingFocusHandler focusCallback;

            /// <summary>
            /// The style picker
            /// </summary>
            private readonly IHtmlStylePicker stylePicker;

            /// <summary>
            /// Initializes a new instance of the <see cref="StyleCommand"/> class.
            /// </summary>
            /// <param name="stylePicker">The style picker.</param>
            /// <param name="focusCallback">The focus callback.</param>
            public StyleCommand(IHtmlStylePicker stylePicker, TextEditingFocusHandler focusCallback)
            {
                this.stylePicker = stylePicker;
                this.focusCallback = focusCallback;
                this.stylePicker.HtmlStyleChanged += this._stylePicker_StyleChanged;
            }

            /// <inheritdoc />
            public override CommandId CommandId => CommandId.Style;

            /// <inheritdoc />
            public override bool ManageAggressively => false;

            /// <inheritdoc />
            protected override void Execute()
            {
                this.PostEditor.ApplyHtmlFormattingStyle(this.stylePicker.SelectedStyle);

                //Bug 244868 - restore focus back to the editor when a new style is selected
                this.focusCallback();
            }

            // @RIBBON TODO: Rationalize existing StyleCommand with SemanticHtmlStyleCommand

            /// <inheritdoc />
            public override void Manage()
            {
                var enabled = this.PostEditor.CanApplyFormatting(CommandId.Style);
                this.Enabled = enabled;
                this.stylePicker.Enabled = enabled;

                var semanticHtmlGalleryCommand =
                    (SemanticHtmlGalleryCommand) this.PostEditor.CommandManager.Get(CommandId.SemanticHtmlGallery);
                if (semanticHtmlGalleryCommand != null)
                {
                    semanticHtmlGalleryCommand.SelectedStyle = this.PostEditor.SelectionStyleName;
                    semanticHtmlGalleryCommand.Enabled = enabled;
                }

                this.stylePicker.SelectStyleByElementName(this.PostEditor.SelectionStyleName);
            }

            /// <summary>
            /// Handles the StyleChanged event of the _stylePicker control.
            /// </summary>
            /// <param name="sender">The source of the event.</param>
            /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
            private void _stylePicker_StyleChanged(object sender, EventArgs e) => this.Command.PerformExecute();
        }
    }
}
