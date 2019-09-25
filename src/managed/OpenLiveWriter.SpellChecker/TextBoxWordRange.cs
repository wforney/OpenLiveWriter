// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Diagnostics;
    using System.Windows.Forms;
    using CoreServices;

    /// <summary>
    /// IWordRange implementation for Windows Forms text boxes.
    /// Implements the <see cref="OpenLiveWriter.SpellChecker.IWordRange" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.SpellChecker.IWordRange" />
    public class TextBoxWordRange : IWordRange
    {
        /// <summary>
        /// The text box
        /// </summary>
        private readonly TextBox textBox;

        /// <summary>
        /// pointer for end of current word.
        /// </summary>
        private int endPos;

        /// <summary>
        /// The highlighted
        /// </summary>
        private bool highlighted;

        /// <summary>
        /// the index of the end of the text box or selection.
        /// </summary>
        private int limit;

        /// <summary>
        /// pointer for start of current word.
        /// </summary>
        private int startPos;

        /// <summary>
        /// The text
        /// </summary>
        private string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBoxWordRange"/> class.
        /// </summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="respectExistingSelection">if set to <c>true</c> [respect existing selection].</param>
        public TextBoxWordRange(TextBox textBox, bool respectExistingSelection)
        {
            this.textBox = textBox;
            this.text = textBox.Text;

            this.startPos = -1;
            if (textBox.SelectionLength == 0 || !respectExistingSelection)
            {
                this.endPos = 0;
                this.limit = textBox.TextLength;
            }
            else
            {
                this.endPos = this.StartPosForSelection(textBox.SelectionStart);
                this.limit = textBox.SelectionStart + textBox.SelectionLength;
            }

            this.highlighted = false;
        }

        /// <summary>
        /// Is there another word in the range?
        /// </summary>
        /// <returns>true if there is another word in the range</returns>
        public bool HasNext() => this.NextWordStart() != -1;

        /// <summary>
        /// Advance to the next word in the range
        /// </summary>
        public void Next()
        {
            this.startPos = this.NextWordStart();
            this.endPos = this.FindEndOfWord(this.startPos);
        }

        /// <summary>
        /// Get the current word
        /// </summary>
        /// <value>The current word.</value>
        public string CurrentWord => this.text.Substring(this.startPos, this.endPos - this.startPos);

        /// <inheritdoc />
        public void PlaceCursor()
        {
            this.textBox.SelectionStart = this.endPos;
            this.textBox.SelectionLength = 0;
        }

        /// <summary>
        /// Highlight the current word
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void Highlight(int offset, int length)
        {
            this.textBox.SelectionStart = this.startPos + offset;
            this.textBox.SelectionLength = length;
            this.highlighted = true;
        }

        /// <summary>
        /// Remove highlighting from the range
        /// </summary>
        public void RemoveHighlight() => this.textBox.SelectionLength = 0;

        /// <summary>
        /// Replace the current word
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="newText">text to replace word with</param>
        public void Replace(int offset, int length, string newText)
        {
            this.text = this.text.Substring(0, this.startPos) +
                        StringHelper.Replace(this.CurrentWord, offset, length, newText) +
                        this.text.Substring(this.endPos);

            var delta = newText.Length - (this.endPos - this.startPos);
            this.limit += delta;
            this.endPos += delta;

            this.textBox.Text = this.text;

            this.textBox.SelectionStart = this.startPos;
            this.textBox.SelectionLength = this.highlighted ? this.endPos - this.startPos : 0;
        }

        /// <inheritdoc />
        public bool IsCurrentWordUrlPart() => false;

        /// <inheritdoc />
        public bool FilterApplies() => false;

        /// <inheritdoc />
        public bool FilterAppliesRanged(int offset, int length) => false;

        /// <summary>
        /// Starts the position for selection.
        /// </summary>
        /// <param name="selectionStart">The selection start.</param>
        /// <returns>System.Int32.</returns>
        private int StartPosForSelection(int selectionStart)
        {
            if (selectionStart == 0)
            {
                return 0;
            }

            if (!(TextBoxWordRange.IsWordChar(this.text[selectionStart - 1]) && TextBoxWordRange.IsWordChar(this.text[selectionStart])))
            {
                return selectionStart;
            }

            var lastNonWord = 0;
            for (var i = 0; i < selectionStart; i++)
            {
                if (!TextBoxWordRange.IsWordChar(this.text[i]))
                {
                    lastNonWord = i;
                }
            }

            return lastNonWord + 1;
        }

        /// <summary>
        /// Returns the next word start.
        /// </summary>
        /// <returns>The next word start position.</returns>
        private int NextWordStart()
        {
            for (var i = this.endPos; i < this.limit; i++)
            {
                if (TextBoxWordRange.IsWordChar(this.text[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Finds the end of word.
        /// </summary>
        /// <param name="startIndex">The start index.</param>
        /// <returns>The end of the word.</returns>
        private int FindEndOfWord(int startIndex)
        {
            if (startIndex == -1 || startIndex >= this.limit)
            {
                return -1;
            }

            Trace.Assert(
                TextBoxWordRange.IsWordChar(this.text[startIndex]),
                $"Asked to find end of word starting from invalid char {this.text[startIndex]}");

            for (var i = startIndex + 1; i < this.text.Length; i++)
            {
                if (!TextBoxWordRange.IsWordChar(this.text[i]))
                {
                    return i;
                }
            }

            return this.text.Length;
        }

        /// <summary>
        /// Determines whether [is word character] [the specified c].
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns><c>true</c> if [is word character] [the specified c]; otherwise, <c>false</c>.</returns>
        private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '\'';
    }
}
