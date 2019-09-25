// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Windows.Forms;

    using HtmlParser.Parser;

    /// <summary>
    /// Some test cases:
    /// [empty string]
    /// all correct words
    /// mzispalled wordz
    /// "Punctuation."
    /// Apostrophe's
    /// 'Apostrophe's'
    /// numb3rs
    /// 1312
    /// Good
    /// Limitations:
    /// Doesn't handle mid-word markup (e.g. t<i>e</i>st)
    /// Doesn't correct ALT attributes
    /// Doesn't know to ignore http:// and e-mail addresses
    /// Implements the <see cref="OpenLiveWriter.SpellChecker.IWordRange" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.SpellChecker.IWordRange" />
    public partial class HtmlTextBoxWordRange : IWordRange
    {
        /// <summary>
        /// The end at
        /// </summary>
        private readonly int endAt;

        /// <summary>
        /// The source
        /// </summary>
        private readonly WordSource src;

        /// <summary>
        /// The start at
        /// </summary>
        private readonly int startAt;

        /// <summary>
        /// The text box
        /// </summary>
        private readonly TextBox textBox;

        /// <summary>
        /// The current word
        /// </summary>
        private TextWithOffsetAndLen currentWord;

        /// <summary>
        /// The drift
        /// </summary>
        private int drift;

        /// <summary>
        /// The next word
        /// </summary>
        private TextWithOffsetAndLen nextWord;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlTextBoxWordRange"/> class.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        public HtmlTextBoxWordRange(TextBox textBox)
        {
            this.textBox = textBox;
            this.src = new WordSource(new HtmlTextSource(new SimpleHtmlParser(this.textBox.Text)));

            if (this.textBox.SelectionLength > 0)
            {
                this.startAt = this.textBox.SelectionStart;
                this.endAt = this.startAt + this.textBox.SelectionLength;
            }
            else
            {
                this.startAt = 0;
                this.endAt = this.textBox.TextLength;
            }

            this.AdvanceToStart();
        }

        /// <inheritdoc />
        public bool HasNext() => this.nextWord != null;

        /// <inheritdoc />
        public void Next()
        {
            this.currentWord = this.nextWord;
            this.nextWord = this.src.Next();
            this.CheckForNextWordPastEnd();
        }

        /// <inheritdoc />
        public string CurrentWord => this.currentWord.Text;

        /// <inheritdoc />
        public void PlaceCursor() => this.textBox.Select(this.currentWord.Offset - this.drift + this.currentWord.Len, 0);

        /// <inheritdoc />
        public void Highlight(int offset, int length) => this.textBox.Select(this.currentWord.Offset - this.drift + offset, length);

        /// <inheritdoc />
        public void RemoveHighlight() => this.textBox.Select(this.textBox.SelectionStart, 0);

        /// <inheritdoc />
        public void Replace(int offset, int length, string newText)
        {
            newText = HtmlUtils.EscapeEntities(newText);
            this.Highlight(offset, length);
            this.textBox.SelectedText = newText;
            this.drift += this.currentWord.Len - newText.Length;
        }

        /// <inheritdoc />
        public bool IsCurrentWordUrlPart() => false;

        /// <inheritdoc />
        public bool FilterApplies() => false;

        /// <inheritdoc />
        public bool FilterAppliesRanged(int offset, int length) => false;

        /// <summary>
        /// Advances to start.
        /// </summary>
        private void AdvanceToStart()
        {
            while ((this.nextWord = this.src.Next()) != null // not at EOD
                && this.nextWord.Offset + this.nextWord.Len <= this.startAt) // word is entirely before startAt
            {
            }

            this.CheckForNextWordPastEnd();
        }

        /// <summary>
        /// Checks for next word past end.
        /// </summary>
        private void CheckForNextWordPastEnd()
        {
            if (this.nextWord != null && this.nextWord.Offset >= this.endAt)
            {
                this.nextWord = null;
            }
        }
    }
}
