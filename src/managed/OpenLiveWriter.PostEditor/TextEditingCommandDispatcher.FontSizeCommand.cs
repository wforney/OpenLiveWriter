// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using ApplicationFramework;

    using Commands;

    using HtmlEditor;
    using Interop.Com.Ribbon;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The FontSizeCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.Commands.GalleryCommand{System.Int32}" />
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IRepresentativeString" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.Commands.GalleryCommand{System.Int32}" />
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.IRepresentativeString" />
        private class FontSizeCommand : GalleryCommand<int>, IRepresentativeString
        {
            /// <summary>
            /// The font sizes
            /// </summary>
            private static readonly int[] FontSizes = {8, 10, 12, 14, 18, 24, 36};

            /// <summary>
            /// The current font size
            /// </summary>
            private float currentFontSize;

            /// <summary>
            /// The post editor
            /// </summary>
            private IHtmlEditorCommandSource postEditor;

            /// <inheritdoc />
            public FontSizeCommand()
                : base(CommandId.FontSize)
            {
                this.AllowExecuteOnInvalidIndex = true;
                this._invalidateGalleryRepresentation = true;
                this.ExecuteWithArgs += this.FontSizeCommand_ExecuteWithArgs;

                foreach (var size in FontSizeCommand.FontSizes)
                {
                    this.items.Add(new GalleryItem(size.ToString(CultureInfo.InvariantCulture), null, size));
                }

                this.UpdateInvalidationState(PropertyKeys.ItemsSource, InvalidationState.Pending);
            }

            /// <inheritdoc />
            public override string StringValue
            {
                get
                {
                    if (!(Math.Abs(this.currentFontSize) > float.Epsilon))
                    {
                        return string.Empty;
                    }

                    // Don't include the decimal places if it's just going add ".0";
                    // We allow for up to 1 decimal place, but we don't want to show it
                    // if it is just a zero.
                    var rounded = Math.Round(this.currentFontSize, 1, MidpointRounding.AwayFromZero);
                    var format = Math.Abs(Math.Truncate(rounded) - rounded) < float.Epsilon ? "F0" : "F1";

                    return rounded.ToString(format, CultureInfo.InvariantCulture);

                }
            }

            #region Implementation of IRepresentativeString

            /// <inheritdoc />
            public string RepresentativeString => Convert.ToString(
                FontSizeCommand.FontSizes[FontSizeCommand.FontSizes.Length - 1]
                               .ToString("F1", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);

            #endregion

            /// <summary>
            /// Fonts the size command execute with arguments.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The arguments.</param>
            private void FontSizeCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
            {
                try
                {
                    var index = args.GetInt(this.CommandId.ToString());
                    if (index != GalleryCommand<int>.INVALID_INDEX)
                    {
                        var fontSize = FontSizeCommand.FontSizes[index];

                        this.postEditor.ApplyFontSize(fontSize);

                        this.SetSelectedItem(fontSize);
                    }
                    else
                    {
                        // User edited the combo box edit field, but their input didn't correspond to a valid choice.
                        // We just need to update the gallery based on the current selection.
                        this.InvalidateSelectedItemProperties();
                    }
                }
                catch (Exception ex)
                {
                    Trace.Fail("Exception thrown when applying font size: " + ex);
                }
            }

            /// <summary>
            /// Loads the items.
            /// </summary>
            public override void LoadItems()
            {
                this.currentFontSize = this.postEditor.SelectionFontSize;

                // Just update the selected item.
                for (var i = 0; i < FontSizeCommand.FontSizes.Length; i++)
                {
                    if (Math.Abs(this.currentFontSize - FontSizeCommand.FontSizes[i]) < float.Epsilon)
                    {
                        this.selectedIndex = i;
                    }
                }

                this.InvalidateSelectedItemProperties();
            }

            /// <summary>
            /// Sets the selected item.
            /// </summary>
            /// <param name="item">The selected item.</param>
            public override void SetSelectedItem(int item)
            {
                this.currentFontSize = this.postEditor.SelectionFontSize;
                base.SetSelectedItem(item);
            }

            /// <summary>
            /// Registers the post editor.
            /// </summary>
            /// <param name="postEditor">The post editor.</param>
            internal void RegisterPostEditor(IHtmlEditorCommandSource postEditor) => this.postEditor = postEditor;
        }
    }
}
