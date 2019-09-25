// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using CoreServices;
    using CoreServices.Layout;
    using Interop.Com;
    using Localization;

    /// <summary>
    /// Summary description for SpellCheckerForm.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.BaseForm" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.BaseForm" />
    public class SpellCheckerForm : BaseForm
    {
        /// <summary>
        /// Default maximum suggestions to return
        /// </summary>
        private const short DefaultMaxSuggestions = 10;

        /// <summary>
        /// If we detect a gap between scores of this value or greater then
        /// we drop the score and all remaining
        /// </summary>
        private const short ScoreGapFilter = 20;

        /// <summary>
        /// Suggestion depth for searching (100 is the maximum)
        /// </summary>
        private const short SuggestionDepth = 80;

        /// <summary>
        /// The capitalization
        /// </summary>
        private readonly string capitalization = Res.Get(StringId.SpellCaps);

        /// <summary>
        /// Spelling prompts
        /// </summary>
        private readonly string notInDictionary = Res.Get(StringId.SpellNotInDict);

        /// <summary>
        /// The owner
        /// </summary>
        private readonly IWin32Window owner;

        /// <summary>
        /// The button add
        /// </summary>
        private Button buttonAdd;

        /// <summary>
        /// The button cancel
        /// </summary>
        private Button buttonCancel;

        /// <summary>
        /// The button change
        /// </summary>
        private Button buttonChange;

        /// <summary>
        /// The button change all
        /// </summary>
        private Button buttonChangeAll;

        /// <summary>
        /// The button ignore
        /// </summary>
        private Button buttonIgnore;

        /// <summary>
        /// The button ignore all
        /// </summary>
        private Button buttonIgnoreAll;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label change to
        /// </summary>
        private Label labelChangeTo;

        /// <summary>
        /// The label not in dictionary
        /// </summary>
        private Label labelNotInDictionary;

        /// <summary>
        /// The label suggestions
        /// </summary>
        private Label labelSuggestions;

        /// <summary>
        /// The label word
        /// </summary>
        private Label labelWord;

        /// <summary>
        /// The length
        /// </summary>
        private int length;

        /// <summary>
        /// The list box suggestions
        /// </summary>
        private ListBox listBoxSuggestions;

        /// <summary>
        /// The offset
        /// </summary>
        private int offset;

        /// <summary>
        /// Spelling checker used by the form
        /// </summary>
        private readonly ISpellingChecker spellingChecker;

        /// <summary>
        /// The text box change to
        /// </summary>
        private TextBox textBoxChangeTo;

        /// <summary>
        /// Word range to check
        /// </summary>
        private IWordRange wordRange;

        /// <summary>
        /// Is there a word-range highlight pending the showing of the form?
        /// </summary>
        private bool wordRangeHighlightPending;

        /// <summary>
        /// Initialize spell-checker form
        /// </summary>
        /// <param name="spellingChecker">The spelling checker.</param>
        /// <param name="owner">The owner.</param>
        public SpellCheckerForm(ISpellingChecker spellingChecker, IWin32Window owner)
            : this(spellingChecker, owner, false)
        {
        }

        /// <summary>
        /// Initialize spell-checker form
        /// </summary>
        /// <param name="spellingChecker">The spelling checker.</param>
        /// <param name="owner">The owner.</param>
        /// <param name="provideIgnoreOnce">if set to <c>true</c> [provide ignore once].</param>
        public SpellCheckerForm(ISpellingChecker spellingChecker, IWin32Window owner, bool provideIgnoreOnce)
        {
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.buttonAdd.Text = Res.Get(StringId.SpellAdd);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.labelNotInDictionary.Text = Res.Get(StringId.SpellNotInDict);
            this.labelChangeTo.Text = Res.Get(StringId.SpellChange);
            this.labelSuggestions.Text = Res.Get(StringId.SpellOptions);
            this.buttonIgnore.Text = Res.Get(StringId.SpellIgnore);
            this.buttonIgnoreAll.Text = Res.Get(StringId.SpellIgnoreAll);
            this.buttonChangeAll.Text = Res.Get(StringId.SpellChangeAll);
            this.buttonChange.Text = Res.Get(StringId.SpellChangeWord);
            this.Text = Res.Get(StringId.SpellText);

            // if we aren't providing an ignore-once option then Ignore now means
            // Ignore All (simpler for users). To effect this we need to eliminate
            // the "Ignore" button, rename the "Ignore All" button to "Ignore", and
            // move around other controls as appropriate
            if (!provideIgnoreOnce)
            {
                // hide Ignore button and replace it with Ignore All button
                // renamed as generic "Ignore"
                this.buttonIgnore.Visible = false;

                // fix up UI by moving around buttons
                this.buttonIgnoreAll.Top = this.buttonIgnore.Top;
                var buttonChangeTop = this.buttonChange.Top - this.textBoxChangeTo.Top + 1;
                this.buttonChange.Top -= buttonChangeTop;
                this.buttonChangeAll.Top -= buttonChangeTop;
                this.buttonAdd.Top -= buttonChangeTop;
            }

            // keep reference to spell-checking interface
            this.spellingChecker = spellingChecker;
            this.owner = owner;
        }

        /// <summary>
        /// Was the spell check completed?
        /// </summary>
        /// <value><c>true</c> if completed; otherwise, <c>false</c>.</value>
        public bool Completed { get; private set; }

        /// <summary>
        /// Occurs when [word ignored].
        /// </summary>
        public event EventHandler WordIgnored;

        /// <summary>
        /// Check spelling
        /// </summary>
        /// <param name="range">word range to check</param>
        public void CheckSpelling(IWordRange range)
        {
            // save reference to word-range
            this.wordRange = range;

            // initialize flags
            this.Completed = false;
            this.wordRangeHighlightPending = false;

            // enter the spell-checking loop (if there are no misspelled words
            // then the form will never show)
            this.ContinueSpellCheck();
        }

        /// <summary>
        /// Continue the spell-checking loop
        /// </summary>
        private void ContinueSpellCheck()
        {
            // provide feedback (pump events so underlying control has an
            // opportunity to update its display)
            this.RemoveHighlight();
            Application.DoEvents();

            if (!this.spellingChecker.IsInitialized)
            {
                Trace.Fail("Spellchecker was uninitialized in the middle of spellchecking after removing highlight.");
                return;
            }

            using (new WaitCursor())
            {
                // loop through all of the words in the word-range
                var currentWordMisspelled = false;
                while (this.wordRange.HasNext())
                {
                    // advance to the next word
                    this.wordRange.Next();

                    // check the spelling
                    string otherWord = null;
                    this.offset = 0;
                    this.length = this.wordRange.CurrentWord.Length;

                    var currentWord = this.wordRange.CurrentWord;
                    var result = this.wordRange.IsCurrentWordUrlPart() || WordRangeHelper.ContainsOnlySymbols(currentWord)
                                                  ? SpellCheckResult.Correct
                                                  : this.spellingChecker.CheckWord(currentWord, out otherWord, out this.offset,
                                                                                   out this.length);

                    //note: currently using this to not show any errors in smart content, since the fix isn't
                    // propagated to the underlying data structure
                    if (result != SpellCheckResult.Correct &&
                        !this.wordRange.FilterAppliesRanged(this.offset, this.length))
                    {
                        // auto-replace
                        if (result == SpellCheckResult.AutoReplace)
                        {
                            // replace word and continue loop (pump events so the document
                            // is updated w/ the new word)
                            this.wordRange.Replace(this.offset, this.length, otherWord);
                            Application.DoEvents();

                            if (!this.spellingChecker.IsInitialized)
                            {
                                Trace.Fail(
                                    "Spellchecker was uninitialized in the middle of spellchecking after auto-replace.");
                                return;
                            }
                        }

                        // some other incorrect word
                        else if (result != SpellCheckResult.Correct)
                        {
                            var misspelledWord = this.wordRange.CurrentWord;
                            if (this.offset > 0 && this.offset <= misspelledWord.Length)
                            {
                                misspelledWord = misspelledWord.Substring(this.offset);
                            }

                            if (this.length < misspelledWord.Length)
                            {
                                misspelledWord = misspelledWord.Substring(0, this.length);
                            }

                            // highlight the misspelled word
                            this.HighlightWordRange();

                            // set current misspelled word
                            this.labelWord.Text = misspelledWord;

                            // misspelling or incorrect capitalization
                            if (result == SpellCheckResult.Misspelled)
                            {
                                this.labelNotInDictionary.Text = this.notInDictionary;
                                this.ProvideSuggestions(misspelledWord);
                            }
                            else if (result == SpellCheckResult.Capitalization)
                            {
                                this.labelNotInDictionary.Text = this.capitalization;
                                this.ProvideSuggestions(misspelledWord, 1);
                            }

                            // conditional replace
                            else if (result == SpellCheckResult.ConditionalReplace)
                            {
                                this.labelNotInDictionary.Text = this.notInDictionary;
                                this.ProvideConditionalReplaceSuggestion(otherWord);
                            }

                            // update state and break out of the loop
                            currentWordMisspelled = true;
                            break;
                        }
                    }
                }

                // there is a pending misspelling, make sure the form is visible
                if (currentWordMisspelled)
                {
                    this.EnsureFormVisible();
                }

                // current word not misspelled and no more words, spell check is finished
                else if (!this.wordRange.HasNext())
                {
                    this.Completed = true;
                    this.EndSpellCheck();
                }
            }
        }

        /// <summary>
        /// Fina!
        /// </summary>
        private void EndSpellCheck()
        {
            if (this.Visible)
            {
                // close the form
                this.Close();
            }
            else
            {
                // if form never became visible make sure we reset
                this.ResetSpellingState();
            }
        }

        /// <summary>
        /// Cleanup when the form closes
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            // call base
            base.OnClosed(e);

            // reset spelling state
            this.ResetSpellingState();
            this.wordRange.PlaceCursor();
        }

        /// <summary>
        /// Removes the highlight.
        /// </summary>
        private void RemoveHighlight()
        {
            try
            {
                this.wordRange.RemoveHighlight();
            }
            catch (COMException e)
            {
                // WinLive 263776: Mail returns E_FAIL if the SubjectEdit element cannot be found.
                // We should be able to recover from that.
                if (e.ErrorCode != HRESULT.E_FAILED)
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Reset the spelling state
        /// </summary>
        private void ResetSpellingState() =>
            // remove feedback from the document
            this.RemoveHighlight();

        /// <summary>
        /// Provide suggestions for the current misspelled word
        /// </summary>
        /// <param name="word">word to provide suggestions for</param>
        private void ProvideSuggestions(string word) => this.ProvideSuggestions(word, SpellCheckerForm.DefaultMaxSuggestions);

        /// <summary>
        /// Provide suggestions for the current misspelled word
        /// </summary>
        /// <param name="word">word to provide suggestions for</param>
        /// <param name="maxSuggestions">maximum number of suggestions to provide</param>
        private void ProvideSuggestions(string word, short maxSuggestions)
        {
            // clear the existing suggestions
            this.listBoxSuggestions.Items.Clear();
            this.textBoxChangeTo.Clear();

            // retrieve suggestions
            var suggestions = this.spellingChecker.Suggest(word, maxSuggestions, SpellCheckerForm.SuggestionDepth);

            // provide suggestions
            if (suggestions.Length > 0)
            {
                // add suggestions to list (stop adding when the quality of scores
                // declines precipitously)
                var lastScore = suggestions[0].Score;
                foreach (var suggestion in suggestions)
                {
                    if (lastScore - suggestion.Score < SpellCheckerForm.ScoreGapFilter &&
                        suggestion.Suggestion != null)
                    {
                        this.listBoxSuggestions.Items.Add(suggestion.Suggestion);
                    }
                    else
                    {
                        break;
                    }

                    // update last score
                    lastScore = suggestion.Score;
                }
            }

            if (this.listBoxSuggestions.Items.Count == 0)
            {
                this.listBoxSuggestions.Items.Add(Res.Get(StringId.SpellNoSuggest));
                this.listBoxSuggestions.Enabled = false;
                this.buttonChange.Enabled = false;
                this.buttonChangeAll.Enabled = false;
            }
            else
            {
                // select first item
                this.listBoxSuggestions.SelectedIndex = 0;
                this.listBoxSuggestions.Enabled = true;
                this.buttonChange.Enabled = true;
                this.buttonChangeAll.Enabled = true;
            }

            // select and focus change-to
            this.textBoxChangeTo.SelectAll();
            this.textBoxChangeTo.Focus();
        }

        /// <summary>
        /// Provide a suggestion containing a single word
        /// </summary>
        /// <param name="suggestedWord">suggested word</param>
        private void ProvideConditionalReplaceSuggestion(string suggestedWord)
        {
            // set contents of list box to the specified word
            this.listBoxSuggestions.Items.Clear();
            this.listBoxSuggestions.Items.Add(suggestedWord);
            this.listBoxSuggestions.SelectedIndex = 0;

            // select and focus change-to
            this.textBoxChangeTo.SelectAll();
            this.textBoxChangeTo.Focus();
        }

        /// <summary>
        /// Handle Ignore button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonIgnore_Click(object sender, EventArgs e)
        {
            this.WordIgnored?.Invoke(this, EventArgs.Empty);

            // continue spell checking
            this.ContinueSpellCheck();
        }

        /// <summary>
        /// Handle Ignore All button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonIgnoreAll_Click(object sender, EventArgs e)
        {
            // notify engine that we want to ignore all instances of this word
            this.spellingChecker.IgnoreAll(this.labelWord.Text);

            // continue spell checking
            this.ContinueSpellCheck();
        }

        /// <summary>
        /// Handle Change button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonChange_Click(object sender, EventArgs e) => this.DoChange();

        /// <summary>
        /// Handle Change All button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonChangeAll_Click(object sender, EventArgs e)
        {
            // replace the word
            this.wordRange.Replace(this.offset, this.length, this.textBoxChangeTo.Text);

            // notify spell checker that we want to replace all instances of this word
            this.spellingChecker.ReplaceAll(this.labelWord.Text, this.textBoxChangeTo.Text);

            // continue spell checking
            this.ContinueSpellCheck();
        }

        /// <summary>
        /// Handle Add button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            // add this word to the user dictionary
            this.spellingChecker.AddToUserDictionary(this.labelWord.Text);

            // continue spell checking
            this.ContinueSpellCheck();
        }

        /// <summary>
        /// Handle TextChanged event to update state of buttons
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void textBoxChangeTo_TextChanged(object sender, EventArgs e)
        {
            if (this.textBoxChangeTo.Text != string.Empty)
            {
                this.buttonChange.Enabled = true;
                this.buttonChangeAll.Enabled = true;
            }
            else // no text, can't change to
            {
                this.buttonChange.Enabled = false;
                this.buttonChangeAll.Enabled = false;
            }
        }

        /// <summary>
        /// Update ChangeTo text box when the selection changes
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listBoxSuggestions_SelectedIndexChanged(object sender, EventArgs e) =>
            // update contents of change to text box
            this.textBoxChangeTo.Text = this.listBoxSuggestions.SelectedIndex == -1
                                            ? string.Empty
                                            : this.listBoxSuggestions.SelectedItem as string;

        /// <summary>
        /// Double-click of a word in suggestions results in auto-replacement
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void listBoxSuggestions_DoubleClick(object sender, EventArgs e)
        {
            // update change-to
            this.textBoxChangeTo.Text = this.listBoxSuggestions.SelectedItem as string;

            // execute the change
            this.DoChange();
        }

        /// <summary>
        /// Handle Done button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonCancel_Click(object sender, EventArgs e) => this.EndSpellCheck();

        /// <summary>
        /// Handle Options button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void buttonOptions_Click(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Execute the change
        /// </summary>
        private void DoChange()
        {
            // replace the word
            this.wordRange.Replace(this.offset, this.length, this.textBoxChangeTo.Text);

            // continue spell checking
            this.ContinueSpellCheck();
        }

        /// <summary>
        /// Highlight the current word range (delays the highlight if the form is not yet visible)
        /// </summary>
        private void HighlightWordRange()
        {
            if (this.Visible)
            {
                this.wordRange.Highlight(this.offset, this.length);
            }
            else
            {
                this.wordRangeHighlightPending = true;
            }
        }

        /// <summary>
        /// Ensure the form is loaded
        /// </summary>
        private void EnsureFormVisible()
        {
            if (!this.Visible)
            {
                this.ShowDialog(this.owner);
            }
        }

        /// <summary>
        /// Override on-load
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnLoad(EventArgs e)
        {
            /*
                        int originalWidth = buttonIgnore.Width;
                        int maxWidth = originalWidth;
                        foreach (Button button in buttons)
                            if (button.Visible)
                                maxWidth = Math.Max(maxWidth, DisplayHelper.AutoFitSystemButton(button));
                        foreach (Button button in buttons)
                            if (button.Visible)
                                button.Width = maxWidth;
                        Width += maxWidth - originalWidth;
            */

            // call base
            base.OnLoad(e);

            if (this.wordRangeHighlightPending)
            {
                this.wordRange.Highlight(this.offset, this.length);
                this.wordRangeHighlightPending = false;
            }

            Button[] buttons =
            {
                this.buttonIgnore, this.buttonIgnoreAll, this.buttonChange, this.buttonChangeAll, this.buttonAdd,
                this.buttonCancel
            };

            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                this.listBoxSuggestions.Height = this.buttonCancel.Bottom - this.listBoxSuggestions.Top;

                LayoutHelper.EqualizeButtonWidthsVert(AnchorStyles.Left, this.buttonIgnore.Width,
                                                      int.MaxValue,
                                                      buttons);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelNotInDictionary = new System.Windows.Forms.Label();
            this.labelChangeTo = new System.Windows.Forms.Label();
            this.textBoxChangeTo = new System.Windows.Forms.TextBox();
            this.labelSuggestions = new System.Windows.Forms.Label();
            this.listBoxSuggestions = new System.Windows.Forms.ListBox();
            this.buttonIgnore = new System.Windows.Forms.Button();
            this.buttonIgnoreAll = new System.Windows.Forms.Button();
            this.buttonChangeAll = new System.Windows.Forms.Button();
            this.buttonChange = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.labelWord = new System.Windows.Forms.Label();
            this.SuspendLayout();

            //
            // labelNotInDictionary
            //
            this.labelNotInDictionary.AutoSize = true;
            this.labelNotInDictionary.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelNotInDictionary.Location = new System.Drawing.Point(8, 12);
            this.labelNotInDictionary.Name = "labelNotInDictionary";
            this.labelNotInDictionary.Size = new System.Drawing.Size(90, 17);
            this.labelNotInDictionary.TabIndex = 0;
            this.labelNotInDictionary.Text = "&Not in dictionary:";

            //
            // labelChangeTo
            //
            this.labelChangeTo.AutoSize = true;
            this.labelChangeTo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelChangeTo.Location = new System.Drawing.Point(8, 55);
            this.labelChangeTo.Name = "labelChangeTo";
            this.labelChangeTo.Size = new System.Drawing.Size(59, 17);
            this.labelChangeTo.TabIndex = 2;
            this.labelChangeTo.Text = "C&hange to:";

            //
            // textBoxChangeTo
            //
            this.textBoxChangeTo.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChangeTo.Location = new System.Drawing.Point(8, 71);
            this.textBoxChangeTo.MaxLength = 100;
            this.textBoxChangeTo.Name = "textBoxChangeTo";
            this.textBoxChangeTo.Size = new System.Drawing.Size(282, 21);
            this.textBoxChangeTo.TabIndex = 3;
            this.textBoxChangeTo.Text = "";
            this.textBoxChangeTo.TextChanged += new System.EventHandler(this.textBoxChangeTo_TextChanged);

            //
            // labelSuggestions
            //
            this.labelSuggestions.AutoSize = true;
            this.labelSuggestions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelSuggestions.Location = new System.Drawing.Point(9, 99);
            this.labelSuggestions.Name = "labelSuggestions";
            this.labelSuggestions.Size = new System.Drawing.Size(68, 17);
            this.labelSuggestions.TabIndex = 4;
            this.labelSuggestions.Text = "S&uggestions:";

            //
            // listBoxSuggestions
            //
            this.listBoxSuggestions.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxSuggestions.IntegralHeight = false;
            this.listBoxSuggestions.Location = new System.Drawing.Point(8, 115);
            this.listBoxSuggestions.Name = "listBoxSuggestions";
            this.listBoxSuggestions.Size = new System.Drawing.Size(282, 95);
            this.listBoxSuggestions.TabIndex = 5;
            this.listBoxSuggestions.DoubleClick += new System.EventHandler(this.listBoxSuggestions_DoubleClick);
            this.listBoxSuggestions.SelectedIndexChanged +=
                new System.EventHandler(this.listBoxSuggestions_SelectedIndexChanged);

            //
            // buttonIgnore
            //
            this.buttonIgnore.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIgnore.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonIgnore.Location = new System.Drawing.Point(298, 27);
            this.buttonIgnore.Name = "buttonIgnore";
            this.buttonIgnore.TabIndex = 6;
            this.buttonIgnore.Text = "I&gnore";
            this.buttonIgnore.Click += new System.EventHandler(this.buttonIgnore_Click);

            //
            // buttonIgnoreAll
            //
            this.buttonIgnoreAll.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonIgnoreAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonIgnoreAll.Location = new System.Drawing.Point(298, 54);
            this.buttonIgnoreAll.Name = "buttonIgnoreAll";
            this.buttonIgnoreAll.TabIndex = 7;
            this.buttonIgnoreAll.Text = "&Ignore All";
            this.buttonIgnoreAll.Click += new System.EventHandler(this.buttonIgnoreAll_Click);

            //
            // buttonChangeAll
            //
            this.buttonChangeAll.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangeAll.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonChangeAll.Location = new System.Drawing.Point(298, 115);
            this.buttonChangeAll.Name = "buttonChangeAll";
            this.buttonChangeAll.TabIndex = 9;
            this.buttonChangeAll.Text = "Change A&ll";
            this.buttonChangeAll.Click += new System.EventHandler(this.buttonChangeAll_Click);

            //
            // buttonChange
            //
            this.buttonChange.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChange.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonChange.Location = new System.Drawing.Point(298, 88);
            this.buttonChange.Name = "buttonChange";
            this.buttonChange.TabIndex = 8;
            this.buttonChange.Text = "&Change";
            this.buttonChange.Click += new System.EventHandler(this.buttonChange_Click);

            //
            // buttonCancel
            //
            this.buttonCancel.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(298, 237);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);

            //
            // buttonAdd
            //
            this.buttonAdd.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Top |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonAdd.Location = new System.Drawing.Point(298, 149);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.TabIndex = 10;
            this.buttonAdd.Text = "&Add";
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);

            //
            // labelWord
            //
            this.labelWord.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.labelWord.Location = new System.Drawing.Point(8, 28);
            this.labelWord.Name = "labelWord";
            this.labelWord.Size = new System.Drawing.Size(282, 21);
            this.labelWord.TabIndex = 1;
            this.labelWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            //
            // SpellCheckerForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(383, 271);
            this.Controls.Add(this.labelWord);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonChangeAll);
            this.Controls.Add(this.buttonChange);
            this.Controls.Add(this.buttonIgnoreAll);
            this.Controls.Add(this.buttonIgnore);
            this.Controls.Add(this.listBoxSuggestions);
            this.Controls.Add(this.labelSuggestions);
            this.Controls.Add(this.textBoxChangeTo);
            this.Controls.Add(this.labelChangeTo);
            this.Controls.Add(this.labelNotInDictionary);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SpellCheckerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Check Spelling";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
