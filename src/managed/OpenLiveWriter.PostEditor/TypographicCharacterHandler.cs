// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text.RegularExpressions;

    using Autoreplace;

    using CoreServices;

    using Emoticons;

    using mshtml;

    using Mshtml;

    using PostHtmlEditing;

    /// <summary>
    /// Delegate InsertHtml
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="html">The HTML.</param>
    public delegate void InsertHtml(MarkupPointer start, MarkupPointer end, string html);

    /// <summary>
    /// The TypographicCharacterHandler class.
    /// </summary>
    internal class TypographicCharacterHandler
    {
        /// <summary>
        /// The special characters
        /// </summary>
        public static readonly List<string> SpecialCharacters = new List<string> {"...", "(c)", "(r)", "(tm)"};

        /// <summary>
        /// The current selection
        /// </summary>
        private readonly MarkupRange currentSelection;

        /// <summary>
        /// The image editing context
        /// </summary>
        private readonly IBlogPostImageEditingContext imageEditingContext;

        /// <summary>
        /// The insert HTML
        /// </summary>
        private readonly InsertHtml insertHtml;

        /// <summary>
        /// The post body element
        /// </summary>
        private readonly IHTMLElement postBodyElement;

        /// <summary>
        /// The block boundary
        /// </summary>
        private readonly MarkupPointer blockBoundary;

        /// <summary>
        /// The character
        /// </summary>
        private readonly char c;

        /// <summary>
        /// The HTML text
        /// </summary>
        private readonly string htmlText;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypographicCharacterHandler"/> class.
        /// </summary>
        /// <param name="currentSelection">The current selection.</param>
        /// <param name="insertHtml">The insert HTML.</param>
        /// <param name="imageEditingContext">The image editing context.</param>
        /// <param name="postBodyElement">The post body element.</param>
        /// <param name="c">The c.</param>
        /// <param name="htmlText">The HTML text.</param>
        /// <param name="blockBoundary">The block boundary.</param>
        public TypographicCharacterHandler(MarkupRange currentSelection, InsertHtml insertHtml,
                                           IBlogPostImageEditingContext imageEditingContext,
                                           IHTMLElement postBodyElement, char c, string htmlText,
                                           MarkupPointer blockBoundary)
        {
            this.currentSelection = currentSelection.Clone();
            this.currentSelection.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            this.currentSelection.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            this.insertHtml = insertHtml;
            this.imageEditingContext = imageEditingContext;
            this.postBodyElement = postBodyElement;

            this.c = c;
            this.htmlText = htmlText;
            this.blockBoundary = blockBoundary;
        }

        /// <summary>
        /// Gets the maximum length hint.
        /// </summary>
        /// <value>The maximum length hint.</value>
        public static int MaxLengthHint => 3;

        /// <summary>
        /// Handles the typographic replace.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool HandleTypographicReplace()
        {
            // We're doing typographic replacement _after_ MSHTML has handled the key event.
            // There may by formatting tags that were added, e.g. <font>.
            // We only want the text, so we exclude any surrounding tags from the selection.
            this.currentSelection.SelectInner();

            this.ReplaceDashes(this.blockBoundary);
            return this.ReplaceTypographic(this.c, this.htmlText);
        }

        /// <summary>
        /// Replaces the typographic.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="theHtmlText">The HTML text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ReplaceTypographic(char character, string theHtmlText)
        {
            if ((character == '\"' || character == '\'') && AutoreplaceSettings.EnableSmartQuotes) // Handle smart quotes
            {
                var isOpenQuote = true;
                if (theHtmlText.Length > 0)
                {
                    switch (theHtmlText[theHtmlText.Length - 1])
                    {
                        case '-':
                        case '{':
                        case '[':
                        case '(':
                            isOpenQuote = true;
                            break;
                        default:
                            if (!char.IsWhiteSpace(theHtmlText[theHtmlText.Length - 1]))
                            {
                                isOpenQuote = false;
                            }

                            break;
                    }
                }

                switch (character)
                {
                    case '\'':
                        this.ReplaceValue("'", isOpenQuote ? "&#8216;" : "&#8217;");
                        break;
                    case '\"':
                        this.ReplaceValue("\"", isOpenQuote ? "&#8220;" : "&#8221;");
                        break;
                }

                return true;
            }

            if ((character != ')' && character != '.') || !AutoreplaceSettings.EnableSpecialCharacterReplacement)
            {
                return this.ReplaceEmoticon(character, theHtmlText);
            }

            // Handling for (c), (r), ...
            string replaceValue = null;
            string originalHtml = null;

            if (character == ')' && theHtmlText.EndsWith("(c", true, CultureInfo.InvariantCulture))
            {
                replaceValue = "&#0169;";
                originalHtml = "(c)";
            }
            else if (character == ')' && theHtmlText.EndsWith("(r", true, CultureInfo.InvariantCulture))
            {
                replaceValue = "&#0174;";
                originalHtml = "(r)";
            }
            else if (character == ')' && theHtmlText.EndsWith("(tm", true, CultureInfo.InvariantCulture))
            {
                replaceValue = "&#x2122;";
                originalHtml = "(tm)";
            }
            else if (character == '.' && theHtmlText.EndsWith("..", true, CultureInfo.InvariantCulture) &&
                     GlobalEditorOptions.SupportsFeature(ContentEditorFeature.UnicodeEllipsis))
            {
                replaceValue = "&#8230;";
                originalHtml = "...";
            }

            if (replaceValue == null)
            {
                return this.ReplaceEmoticon(character, theHtmlText);
            }

            this.ReplaceValue(originalHtml, replaceValue);
            return true;

        }

        /// <summary>
        /// Replaces the emoticon.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <param name="htmlText">The HTML text.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ReplaceEmoticon(char c, string htmlText)
        {
            if (!this.imageEditingContext.EmoticonsManager.CanInsertEmoticonImage ||
                !AutoreplaceSettings.EnableEmoticonsReplacement || !EmoticonsManager.IsAutoReplaceEndingCharacter(c))
            {
                return false;
            }

            var lastChar = c.ToString();
            foreach (var emoticon in this.imageEditingContext.EmoticonsManager.PopularEmoticons)
            {
                foreach (var autoReplaceText in emoticon.AutoReplaceText)
                {
                    if (!autoReplaceText.EndsWith(lastChar))
                    {
                        continue;
                    }

                    var remainingAutoReplaceText = autoReplaceText.Remove(autoReplaceText.Length - 1);
                    if (!htmlText.EndsWith(remainingAutoReplaceText))
                    {
                        continue;
                    }

                    // Emoticons generally start with a colon, and therefore might interfere with a user attempting to type a URL.
                    var url = StringHelper.GetLastWord(htmlText);
                    if (UrlHelper.StartsWithKnownScheme(url) || !this.IsValidEmoticonInsertionPoint())
                    {
                        return false;
                    }

                    this.ReplaceValue(autoReplaceText,
                                      this.imageEditingContext.EmoticonsManager.GetHtml(emoticon));
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is valid emoticon insertion point].
        /// </summary>
        /// <returns><c>true</c> if [is valid emoticon insertion point]; otherwise, <c>false</c>.</returns>
        private bool IsValidEmoticonInsertionPoint()
        {
            var selection = this.currentSelection.Clone();

            // Check to make sure the target is not in an edit field
            if (InlineEditField.IsWithinEditField(selection.ParentElement()))
            {
                return false;
            }

            // Check to make sure the target is in the body of the post
            selection.MoveToElement(this.postBodyElement, false);
            return selection.InRange(this.currentSelection);
        }

        /// <summary>
        /// Replaces the value.
        /// </summary>
        /// <param name="undoValue">The undo value.</param>
        /// <param name="replacementValue">The replacement value.</param>
        private void ReplaceValue(string undoValue, string replacementValue)
        {
            this.currentSelection.Collapse(false);
            for (var i = 0; i < undoValue.Length; i++)
            {
                this.currentSelection.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);
            }

            this.insertHtml(this.currentSelection.Start, this.currentSelection.End, replacementValue);
        }

        /// <summary>
        /// Replaces the dashes.
        /// </summary>
        /// <param name="blockBoundary">The block boundary.</param>
        private void ReplaceDashes(MarkupPointer blockBoundary)
        {
            if (!AutoreplaceSettings.EnableHyphenReplacement)
            {
                return;
            }

            var emRange = this.currentSelection.Clone();
            for (var i = 0; i < 3 && emRange.Start.IsRightOf(blockBoundary); i++)
            {
                emRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            }

            var emText = emRange.Text ?? "";

            if (!emText.Contains("-"))
            {
                return;
            }

            // \u00A0 = non breaking space
            var regex = new Regex(@"[^\s\u00A0\-]([ \u00A0]?(?>--?)[ \u00A0]?)[^\s\u00A0\-]");
            var match = regex.Match(emText);
            if (!match.Success)
            {
                return;
            }

            Debug.Assert(match.Groups.Count == 2,
                         "Matched more than one set of dashes. Expecting only one match.");
            var matchValue = match.Groups[1].Value.Replace((char) 160, ' ');
            var findRange = this.currentSelection.Clone();

            // Since we're now doing this matching AFTER a character has been added, we need to jump back.
            findRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
            findRange.Collapse(false);
            for (var i = 0; i < matchValue.Length; i++)
            {
                findRange.Start.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
            }

            for (var i = 0; i < emText.Length; i++)
            {
                if (findRange.Text == matchValue)
                {
                    string replaceText = null;
                    switch (matchValue)
                    {
                        case "--":
                        case "-- ":
                            replaceText = "&#8212;";
                            break;
                        case " --":
                        case " -":
                            replaceText = "&#160;&#8211;";
                            break;
                        case " -- ":
                        case " - ":
                            replaceText = "&#160;&#8211; ";
                            break;
                        case "-":
                        case "- ":
                            break;
                    }

                    if (replaceText != null)
                    {
                        this.insertHtml(findRange.Start, findRange.End, replaceText);
                        break;
                    }
                }

                findRange.Start.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
                findRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR, emRange.Start);
            }
        }

        /// <summary>
        /// Sets the gravity right.
        /// </summary>
        public void SetGravityRight()
        {
            this.currentSelection.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
            this.currentSelection.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
        }
    }
}
