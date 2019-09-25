// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.Diagnostics;
    using System.Windows.Forms;
    using ApplicationFramework;
    using CoreServices;
    using Interop.Windows;
    using Localization;
    using mshtml;
    using Mshtml;

    /// <summary>
    /// Delegate ReplaceWord
    /// </summary>
    /// <param name="start">The start.</param>
    /// <param name="end">The end.</param>
    /// <param name="newWord">The new word.</param>
    public delegate void ReplaceWord(MarkupPointer start, MarkupPointer end, string newWord);

    /// <summary>
    /// Summary description for SpellingManager.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class SpellingManager : IDisposable
    {
        /// <summary>
        /// The ignored once
        /// </summary>
        private readonly SortedMarkupRangeList ignoredOnce = new SortedMarkupRangeList();

        /// <summary>
        /// The current word information
        /// </summary>
        private MisspelledWordInfo currentWordInfo;

        /// <summary>
        /// The damage function
        /// </summary>
        private DamageFunction damageFunction;

        /// <summary>
        /// The filter
        /// </summary>
        private MarkupRangeFilter filter;

        /// <summary>
        /// The HTML document
        /// </summary>
        private IHTMLDocument2 htmlDocument;

        /// <summary>
        /// The MSHTML control
        /// </summary>
        private MshtmlControl mshtmlControl;

        /// <summary>
        /// The replace word function
        /// </summary>
        private ReplaceWord replaceWordFunction;

        /// <summary>
        /// The spelling checker
        /// </summary>
        private ISpellingChecker spellingChecker;

        /// <summary>
        /// The spelling context
        /// </summary>
        private IBlogPostSpellCheckingContext spellingContext;

        /// <summary>
        /// The spelling highlighter
        /// </summary>
        private SpellingHighlighter spellingHighlighter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellingManager"/> class.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        public SpellingManager(CommandManager commandManager)
        {
            this.CommandManager = commandManager;
            this.InitializeCommands();
        }

        /// <summary>
        /// Gets the command manager.
        /// </summary>
        /// <value>The command manager.</value>
        public CommandManager CommandManager { get; }

        /// <summary>
        /// Gets the spelling checker.
        /// </summary>
        /// <value>The spelling checker.</value>
        public ISpellingChecker SpellingChecker => this.spellingChecker;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ignore once enabled.
        /// </summary>
        /// <value><c>true</c> if this instance is ignore once enabled; otherwise, <c>false</c>.</value>
        public bool IsIgnoreOnceEnabled
        {
            get
            {
                var cmdIgnoreOnce = this.CommandManager.Get(CommandId.IgnoreOnce);
                return cmdIgnoreOnce != null && cmdIgnoreOnce.On && cmdIgnoreOnce.Enabled;
            }
            set
            {
                var cmdIgnoreOnce = this.CommandManager.Get(CommandId.IgnoreOnce);
                if (cmdIgnoreOnce != null)
                {
                    cmdIgnoreOnce.Enabled = value;
                }
            }
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.spellingHighlighter != null)
            {
                this.spellingChecker.StopChecking();
                this.spellingChecker.WordIgnored -= this._spellingChecker_WordIgnored;
                this.spellingChecker.WordAdded -= this._spellingChecker_WordAdded;
                this.spellingHighlighter.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Initializes the specified spelling checker.
        /// </summary>
        /// <param name="spellingChecker">The spelling checker.</param>
        /// <param name="mshtmlControl">The MSHTML control.</param>
        /// <param name="htmlDocument">The HTML document.</param>
        /// <param name="replaceWordFunction">The replace word function.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="damageFunction">The damage function.</param>
        public void Initialize(
            ISpellingChecker spellingChecker,
            MshtmlControl mshtmlControl,
            IHTMLDocument2 htmlDocument,
            ReplaceWord replaceWordFunction,
            MarkupRangeFilter filter,
            DamageFunction damageFunction)
        {
            this.spellingChecker = spellingChecker;
            this.mshtmlControl = mshtmlControl;
            this.htmlDocument = htmlDocument;
            this.filter = filter;
            this.replaceWordFunction = replaceWordFunction;
            this.damageFunction = damageFunction;
        }

        /// <summary>
        /// Initializes the session.
        /// </summary>
        /// <param name="spellingContext">The spelling context.</param>
        public void InitializeSession(IBlogPostSpellCheckingContext spellingContext)
        {
            this.ignoredOnce.Clear();
            this.spellingContext = spellingContext;
            this.spellingHighlighter = new SpellingHighlighter(
                this.SpellingChecker,
                this.mshtmlControl.HighlightRenderingServices,
                this.mshtmlControl.DisplayServices,
                this.mshtmlControl.MarkupServicesRaw,
                (IHTMLDocument4)this.htmlDocument);

            //start new highlighter
            this.spellingChecker.StartChecking();
            this.spellingChecker.WordIgnored += this._spellingChecker_WordIgnored;
            this.spellingChecker.WordAdded += this._spellingChecker_WordAdded;
        }

        /// <summary>
        /// Starts the session.
        /// </summary>
        public void StartSession()
        {
            Debug.Assert(this.spellingContext.CanSpellCheck, "Starting spelling session when spelling is disabled!");

            this.HighlightSpelling();
        }

        /// <summary>
        /// Stops the session.
        /// </summary>
        /// <param name="hardReset">if set to <c>true</c> [hard reset].</param>
        public void StopSession(bool hardReset)
        {
            //clear tracker
            //clear work
            this.spellingHighlighter.Reset();
            if (hardReset)
            {
                this.ignoredOnce.Clear();
                this.spellingChecker.StopChecking();
                this.spellingChecker.WordIgnored -= this._spellingChecker_WordIgnored;
                this.spellingChecker.WordAdded -= this._spellingChecker_WordAdded;
            }

            //clear listeners
        }

        /// <summary>
        /// Check the spelling of the entire document
        /// </summary>
        public void HighlightSpelling() => this.HighlightSpelling(null);

        /// <summary>
        /// Highlights the spelling.
        /// </summary>
        /// <param name="range">The range.</param>
        public void HighlightSpelling(MarkupRange range)
        {
            // check spelling
            MshtmlWordRange wordRange;
            if (range == null) //check the whole document.
            {
                wordRange = new MshtmlWordRange(this.htmlDocument, false, this.filter, this.damageFunction);
            }
            else
            {
                //range is invalid for some reason--damage committed while switching views, getting it later on the timer
                if (!range.Positioned)
                {
                    return;
                }

                if (range.Text == null || string.IsNullOrEmpty(range.Text.Trim()))
                {
                    //empty range--on a delete for instance, just clear
                    this.spellingHighlighter.ClearRange(range.Start, range.End);
                    this.ignoredOnce.ClearRange(range);
                    return;
                }

                var origRange = range.Clone();

                //here are the words to check
                wordRange = new MshtmlWordRange(this.htmlDocument, range, this.filter, this.damageFunction);

                //check for emptiness at start and end, clear those
                this.spellingHighlighter.ClearRange(origRange.Start, range.Start);
                this.spellingHighlighter.ClearRange(range.End, origRange.End);

                this.ignoredOnce.ClearRange(range);
            }

            this.spellingHighlighter.CheckSpelling(wordRange);
        }

        /// <summary>
        /// used to remove all misspellings, when turning realtime spell checking on/off
        /// </summary>
        public void ClearAll() => this.spellingHighlighter.Reset();

        /// <summary>
        /// Finds the misspelling.
        /// </summary>
        /// <param name="markupPointer">The markup pointer.</param>
        /// <returns>MisspelledWordInfo.</returns>
        public MisspelledWordInfo FindMisspelling(MarkupPointer markupPointer) =>
            this.spellingHighlighter.FindMisspelling(markupPointer);

        /// <summary>
        /// Updates the spelling context.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void UpdateSpellingContext(object sender, EventArgs e)
        {
            var context = sender as IBlogPostSpellCheckingContext;
            this.spellingContext = context;
            this.StopSession(true);
            this.InitializeSession(this.spellingContext);
        }

        /// <summary>
        /// Initializes the commands.
        /// </summary>
        private void InitializeCommands()
        {
            this.CommandManager.BeginUpdate();

            var commandAddToDictionary = new Command(CommandId.AddToDictionary);
            commandAddToDictionary.Execute += this.addToDictionary_Execute;
            this.CommandManager.Add(commandAddToDictionary);

            var commandIgnoreAll = new Command(CommandId.IgnoreAll);
            commandIgnoreAll.Execute += this.ignoreAllCommand_Execute;

            this.CommandManager.Add(commandIgnoreAll);
            this.CommandManager.Add(CommandId.IgnoreOnce, this.ignoreOnceCommand_Execute);

            var commandOpenSpellingForm = new Command(CommandId.OpenSpellingForm);
            commandOpenSpellingForm.Execute += this.openSpellingForm_Execute;
            this.CommandManager.Add(commandOpenSpellingForm);

            this.CommandManager.EndUpdate();
        }

        //return the context menu definition
        /// <summary>
        /// Creates the spell checking context menu.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns>SpellCheckingContextMenuDefinition.</returns>
        public SpellCheckingContextMenuDefinition CreateSpellCheckingContextMenu(MisspelledWordInfo word)
        {
            this.currentWordInfo = word;
            return new SpellCheckingContextMenuDefinition(this.currentWordInfo.Word, this);
        }

        //handlers for various commands
        /// <summary>
        /// Handles the Execute event of the fixSpellingApplyCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void fixSpellingApplyCommand_Execute(object sender, EventArgs e)
        {
            var command = (Command)sender;
            this.replaceWordFunction(this.currentWordInfo.WordRange.Start, this.currentWordInfo.WordRange.End,
                                      command.Tag as string);
        }

        /// <summary>
        /// Handles the Execute event of the ignoreOnceCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ignoreOnceCommand_Execute(object sender, EventArgs e) => this.IgnoreCore(this.currentWordInfo.WordRange);

        /// <summary>
        /// Ignores the core.
        /// </summary>
        /// <param name="range">The range.</param>
        private void IgnoreCore(MarkupRange range)
        {
            this.spellingHighlighter.UnhighlightRange(range);

            // Win Live 182705: Assert when ignoring misspelled words in the album title very quickly
            // It is possible to get multiple "ignores" queue up before we've been able to process them.
            if (!this.ignoredOnce.Contains(range))
            {
                this.ignoredOnce.Add(range);
            }
        }

        /// <summary>
        /// Handles the Execute event of the ignoreAllCommand control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ignoreAllCommand_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                this.spellingChecker.IgnoreAll(this.currentWordInfo.Word);
            }
        }

        /// <summary>
        /// Handles the WordIgnored event of the _spellingChecker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _spellingChecker_WordIgnored(object sender, EventArgs e) => this.spellingHighlighter.UnhighlightWord((string)sender);

        /// <summary>
        /// Handles the Execute event of the addToDictionary control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void addToDictionary_Execute(object sender, EventArgs e)
        {
            using (new WaitCursor())
            {
                this.spellingChecker.AddToUserDictionary(this.currentWordInfo.Word);
            }
        }

        /// <summary>
        /// Handles the WordAdded event of the _spellingChecker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _spellingChecker_WordAdded(object sender, EventArgs e) => this.spellingHighlighter.UnhighlightWord((string)sender);

        /// <summary>
        /// Handles the Execute event of the openSpellingForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void openSpellingForm_Execute(object sender, EventArgs e)
        {
            this.mshtmlControl.MarkupServices.BeginUndoUnit(Guid.NewGuid().ToString());

            var supportsIgnoreOnce = false;
            var cmdIgnoreOnce = this.CommandManager.Get(CommandId.IgnoreOnce);
            if (cmdIgnoreOnce != null && cmdIgnoreOnce.On)
            {
                supportsIgnoreOnce = true;
            }

            // must first force the control to lose focus so that it doesn't "lose"
            // the selection when the dialog opens
            var hPrevious = User32.SetFocus(IntPtr.Zero);

            using (var spellCheckerForm =
                new SpellCheckerForm(this.SpellingChecker, this.mshtmlControl.FindForm(), supportsIgnoreOnce))
            {
                //  center the spell-checking form over the document body
                spellCheckerForm.StartPosition = FormStartPosition.CenterParent;

                // WinLive 263320: We want to check from the current word to the end of the document.
                // TODO: Although this works fine, it would be better to only spellcheck inside editable regions.
                var rangeToSpellCheck = this.currentWordInfo.WordRange.Clone();
                rangeToSpellCheck.End.MoveAdjacentToElement(this.htmlDocument.body,
                                                            _ELEMENT_ADJACENCY.ELEM_ADJ_BeforeEnd);

                // get the word range to check
                var wordRange =
                    new MshtmlWordRange(this.htmlDocument, rangeToSpellCheck, this.filter, this.damageFunction);

                spellCheckerForm.WordIgnored += (sender2, args) => this.IgnoreOnce(wordRange.CurrentWordRange);

                // check spelling
                spellCheckerForm.CheckSpelling(wordRange);
                this.mshtmlControl.MarkupServices.EndUndoUnit();

                // restore focus to the control that had it before we spell-checked
                User32.SetFocus(hPrevious);
            }
        }

        /// <summary>
        /// Determines whether [is word ignored] [the specified range].
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns><c>true</c> if [is word ignored] [the specified range]; otherwise, <c>false</c>.</returns>
        public bool IsWordIgnored(MarkupRange range) => this.ignoredOnce.Contains(range);

        /// <summary>
        /// Ignores the once.
        /// </summary>
        /// <param name="range">The range.</param>
        public void IgnoreOnce(MarkupRange range) => this.IgnoreCore(range);

        /// <summary>
        /// Determines whether [is in ignored word] [the specified p].
        /// </summary>
        /// <param name="p">The p.</param>
        /// <returns><c>true</c> if [is in ignored word] [the specified p]; otherwise, <c>false</c>.</returns>
        public bool IsInIgnoredWord(MarkupPointer p) => this.ignoredOnce.Contains(p);

        /// <summary>
        /// Clears the ignore once.
        /// </summary>
        public void ClearIgnoreOnce() => this.ignoredOnce.Clear();

        /// <summary>
        /// Damageds the range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <param name="doSpellCheck">if set to <c>true</c> [do spell check].</param>
        public void DamagedRange(MarkupRange range, bool doSpellCheck)
        {
            if (doSpellCheck)
            {
                this.HighlightSpelling(range);
            }
            else
            {
                if (range.Text != null)
                {
                    MshtmlWordRange.ExpandRangeToWordBoundaries(range);
                }

                this.ignoredOnce.ClearRange(range);
            }
        }
    }
}
