// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Text;

    using ApplicationFramework;

    using Commands;

    using HtmlEditor;

    using Localization;

    public partial class TextEditingCommandDispatcher
    {
        /// <summary>
        /// The FontFamilyCommand class.
        /// Implements the <see cref="OpenLiveWriter.PostEditor.Commands.GalleryCommand{System.String}" />
        /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IRepresentativeString" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.PostEditor.Commands.GalleryCommand{System.String}" />
        /// <seealso cref="OpenLiveWriter.ApplicationFramework.IRepresentativeString" />
        private class FontFamilyCommand : GalleryCommand<string>, IRepresentativeString
        {
            /// <summary>
            /// The post editor
            /// </summary>
            private IHtmlEditorCommandSource postEditor;

            /// <inheritdoc />
            public FontFamilyCommand()
                : base(CommandId.FontFamily)
            {
                this._invalidateGalleryRepresentation = true;
                this.ExecuteWithArgs += this.FontFamilyCommand_ExecuteWithArgs;
            }

            #region Implementation of IRepresentativeString

            /// <inheritdoc />
            public string RepresentativeString => "Times New Roman";

            #endregion

            /// <summary>
            /// Fonts the family command execute with arguments.
            /// </summary>
            /// <param name="sender">The sender.</param>
            /// <param name="args">The arguments.</param>
            private void FontFamilyCommand_ExecuteWithArgs(object sender, ExecuteEventHandlerArgs args)
            {
                try
                {
                    this.postEditor.ApplyFontFamily(this.items[args.GetInt(this.CommandId.ToString())].Label);
                }
                catch (Exception ex)
                {
                    Trace.Fail("Exception thrown when applying font family: " + ex);
                }
            }

            /// <summary>
            /// Loads the items.
            /// </summary>
            public override void LoadItems()
            {
                if (this.items.Count == 0)
                {
                    // @RIBBON TODO: Render preview images of each font family
                    var currentFontFamily = this.postEditor.SelectionFontFamily;
                    this.selectedItem = this.postEditor.SelectionFontFamily;
                    using (var fontCollection = new InstalledFontCollection())
                    {
                        var fontFamilies = fontCollection.Families;

                        for (var i = 0; i < fontFamilies.Length; i++)
                        {
                            this.items.Add(new GalleryItem(fontFamilies[i].GetName(0), null, fontFamilies[i].Name));

                            // We determine the selected index based on the font family name in English.
                            if (currentFontFamily == fontFamilies[i].Name)
                            {
                                this.selectedIndex = i;
                            }
                        }
                    }

                    base.LoadItems();
                }
                else
                {
                    // Note: that the font family drop down will not reflect changes to the set of
                    // Note: installed fonts made after initialization.
                    this.selectedIndex = GalleryCommand<string>.INVALID_INDEX;
                    var fontName = this.postEditor.SelectionFontFamily;
                    if (string.IsNullOrEmpty(fontName))
                    {
                        this.selectedItem = string.Empty;
                    }
                    else
                    {
                        try
                        {
                            // The font's primary name need not always be in english.
                            // The name returned by mshtml could be in culture neutral, so find the primary name
                            // since that is what we set as the cookie for GalleryItem
                            var fontFamily = new FontFamily(fontName);
                            this.selectedItem = fontFamily.Name;
                        }
                        catch
                        {
                            this.selectedItem = string.Empty;
                        }
                    }
                }
            }

            /// <summary>
            /// Registers the post editor.
            /// </summary>
            /// <param name="htmlEditorCommandSource">The post editor.</param>
            internal void RegisterPostEditor(IHtmlEditorCommandSource htmlEditorCommandSource) => this.postEditor = htmlEditorCommandSource;
        }
    }
}
