// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using CoreServices;
    using CoreServices.Diagnostics;
    using mshtml;
    using Mshtml;

    /// <summary>
    /// The SpellingHighlighter class.
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class SpellingHighlighter : IDisposable
    {
        /// <summary>
        /// The timer interval
        /// </summary>
        private static readonly int TimerInterval = 10;

        /// <summary>
        /// The number of words to check
        /// </summary>
        private static readonly int NumberOfWordsToCheck = 30;

        /// <summary>
        /// The display services
        /// </summary>
        private readonly IDisplayServicesRaw displayServices;

        /// <summary>
        /// The fatal spelling error
        /// </summary>
        private bool fatalSpellingError;

        /// <summary>
        /// The highlight rendering services
        /// </summary>
        private readonly IHighlightRenderingServicesRaw highlightRenderingServices;

        /// <summary>
        /// Highlight the current word range
        /// </summary>
        private IHTMLRenderStyle highlightWordStyle;

        /// <summary>
        /// The HTML document
        /// </summary>
        private readonly IHTMLDocument4 htmlDocument;

        /// <summary>
        /// The markup services
        /// </summary>
        private readonly MshtmlMarkupServices markupServices;

        /// <summary>
        /// The markup services raw
        /// </summary>
        private readonly IMarkupServicesRaw markupServicesRaw;

        /// <summary>
        /// The spelling checker
        /// </summary>
        private readonly ISpellingChecker spellingChecker;

        /// <summary>
        /// The timer
        /// </summary>
        private readonly SpellingTimer timer;

        /// <summary>
        /// The tracker
        /// </summary>
        private HighlightSegmentTracker tracker;

        /// <summary>
        /// The worker queue
        /// </summary>
        private Queue workerQueue;

        /// <summary>
        /// Prevents asserts that happen within _timer_Tick from going bonkers; since
        /// they can happen reentrantly, you can end up with hundreds of assert windows.
        /// </summary>
        private bool reentrant;

        /// <summary>
        /// The staging text range
        /// </summary>
        private IHTMLTxtRange stagingTextRange;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellingHighlighter"/> class.
        /// </summary>
        /// <param name="spellingChecker">The spelling checker.</param>
        /// <param name="highlightRenderingServices">The highlight rendering services.</param>
        /// <param name="displayServices">The display services.</param>
        /// <param name="markupServices">The markup services.</param>
        /// <param name="htmlDocument">The HTML document.</param>
        public SpellingHighlighter(ISpellingChecker spellingChecker,
                                   IHighlightRenderingServicesRaw highlightRenderingServices,
                                   IDisplayServicesRaw displayServices, IMarkupServicesRaw markupServices,
                                   IHTMLDocument4 htmlDocument)
        {
            this.spellingChecker = spellingChecker;
            this.highlightRenderingServices = highlightRenderingServices;
            this.displayServices = displayServices;
            this.markupServicesRaw = markupServices;
            this.markupServices = new MshtmlMarkupServices(this.markupServicesRaw);
            this.htmlDocument = htmlDocument;
            this.tracker = new HighlightSegmentTracker();

            //the timer to handle interleaving of spell checking
            this.timer = new SpellingTimer(SpellingHighlighter.TimerInterval);
            this.timer.Start();
            this.timer.Tick += this._timer_Tick;
            this.workerQueue = new Queue();
        }

        /// <summary>
        /// Gets the highlight word style.
        /// </summary>
        /// <value>The highlight word style.</value>
        private IHTMLRenderStyle HighlightWordStyle
        {
            get
            {
                if (this.highlightWordStyle == null)
                {
                    this.highlightWordStyle = this.htmlDocument.createRenderStyle(null);
                    this.highlightWordStyle.defaultTextSelection = "false";
                    this.highlightWordStyle.textDecoration = "underline";
                    this.highlightWordStyle.textUnderlineStyle = "wave";
                    this.highlightWordStyle.textDecorationColor = "red";
                    this.highlightWordStyle.textBackgroundColor = "transparent";
                    this.highlightWordStyle.textColor = "transparent";
                }

                return this.highlightWordStyle;
            }
        }

        #region IDisposable Members

        /// <inheritdoc />
        public void Dispose()
        {
            this.timer.Stop();
            if (this.tracker != null)
            {
                this.tracker = null;
            }

            if (this.workerQueue != null)
            {
                this.workerQueue = null;
            }
        }

        #endregion

        /// <summary>
        /// Check spelling--called by the damage handler
        /// </summary>
        /// <param name="range">The range.</param>
        public void CheckSpelling(MshtmlWordRange range)
        {
            if (this.fatalSpellingError)
            {
                return;
            }

            this.timer.Enabled = true;
            this.workerQueue.Enqueue(range);
            if (this.workerQueue.Count == 1)
            {
                //if the queue had been empty, process this range right away
                this.DoWork();
            }
        }

        /// <summary>
        /// Handles the Tick event of the _timer control.
        /// </summary>
        /// <param name="o">The source of the event.</param>
        /// <param name="args">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _timer_Tick(object o, EventArgs args)
        {
            if (this.reentrant)
            {
                return;
            }

            this.reentrant = true;

            try
            {
                if (this.workerQueue.Count > 0)
                {
                    this.DoWork();
                }
                else
                {
                    this.timer.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(
                    Win32WindowImpl.ForegroundWin32Window,
                    ex,
                    "Unexpected Error Spell Checking");

                this.Reset();

                this.fatalSpellingError = true;
            }
            finally
            {
                this.reentrant = false;
            }
        }

        /// <summary>
        /// manages the queue during work
        /// </summary>
        private void DoWork()
        {
            // start processing the first word range, and pop it if we get to the end
            if (this.ProcessWordRange((MshtmlWordRange) this.workerQueue.Peek()))
            {
                this.workerQueue.Dequeue();
            }
        }

        /// <summary>
        /// iterates through a word range checking for spelling errors
        /// </summary>
        /// <param name="wordRange">The word range.</param>
        /// <returns><c>true</c> if the word range is finished, <c>false</c> otherwise.</returns>
        private bool ProcessWordRange(MshtmlWordRange wordRange)
        {
            if (!wordRange.CurrentWordRange.Positioned)
            {
                return true;
            }

            // track where we will need to clear;
            var start = this.markupServices.CreateMarkupPointer();
            start.MoveToPointer(wordRange.CurrentWordRange.End);
            var highlightWords = new ArrayList(SpellingHighlighter.NumberOfWordsToCheck);

            var i = 0;

            // to do....the word range is losing its place when it stays in the queue
            while (wordRange.HasNext() && i < SpellingHighlighter.NumberOfWordsToCheck)
            {
                // advance to the next word
                wordRange.Next();

                // check the spelling
                if (this.ProcessWord(wordRange, out var offset, out var length))
                {
                    var highlightRange = wordRange.CurrentWordRange.Clone();
                    MarkupHelpers.AdjustMarkupRange(ref this.stagingTextRange, highlightRange, offset, length);

                    //note: cannot just push the current word range here, as it moves before we get to the highlighting step
                    highlightWords.Add(highlightRange);
                }

                i++;
            }

            var end = wordRange.CurrentWordRange.End;

            // got our words, clear the checked range and then add the misspellings
            this.ClearRange(start, end);
            foreach (MarkupRange word in highlightWords)
            {
                this.HighlightWordRange(word);
            }

            return !wordRange.HasNext();
        }

        /// <summary>
        /// takes one the first word on the range, and checks it for spelling errors
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns><c>true</c> if word is misspelled, <c>false</c> otherwise.</returns>
        private bool ProcessWord(MshtmlWordRange word, out int offset, out int length)
        {
            offset = 0;
            length = 0;

            var currentWord = word.CurrentWord;
            var result = word.IsCurrentWordUrlPart() || WordRangeHelper.ContainsOnlySymbols(currentWord)
                             ? SpellCheckResult.Correct
                             : this.spellingChecker.CheckWord(currentWord, out _, out offset, out length);

            if (result == SpellCheckResult.Correct)
            {
                return false;
            }

            // note: currently using this to not show any errors in smart content, since the fix isn't
            // propagated to the underlying data structure
            return !word.FilterApplies();
        }

        /// <summary>
        /// Processes the word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns><c>true</c> if word is misspelled, <c>false</c> otherwise.</returns>
        private bool ProcessWord(string word)
        {
            var result = this.spellingChecker.CheckWord(word, out _, out _, out _);
            return result != SpellCheckResult.Correct;
        }

        /// <summary>
        /// Highlights the word range.
        /// </summary>
        /// <param name="word">The word.</param>
        private void HighlightWordRange(MarkupRange word)
        {
            try
            {
                this.displayServices.CreateDisplayPointer(out var start);
                this.displayServices.CreateDisplayPointer(out var end);
                DisplayServices.TraceMoveToMarkupPointer(start, word.Start);
                DisplayServices.TraceMoveToMarkupPointer(end, word.End);

                this.highlightRenderingServices.AddSegment(start, end, this.HighlightWordStyle, out var segment);
                this.tracker.AddSegment(
                    segment,
                    MarkupHelpers.UseStagingTextRange(
                        ref this.stagingTextRange, word, rng => rng.text),
                    this.markupServicesRaw);
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode == unchecked((int) 0x800A025E))
                {
                    return;
                }

                throw;
            }
        }

        /// <summary>
        /// remove any covered segments from the tracker and clear their highlights
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public void ClearRange(MarkupPointer start, MarkupPointer end)
        {
            var segments = this.tracker.GetSegments(start.PointerRaw, end.PointerRaw);
            if (segments == null)
            {
                return;
            }

            foreach (var segment in segments)
            {
                this.highlightRenderingServices.RemoveSegment(segment);
            }
        }

        /// <summary>
        /// remove all misspellings from tracker and clear their highlights
        /// used when turning spell checking on and off
        /// </summary>
        public void Reset()
        {
            this.timer.Enabled = false;
            this.stagingTextRange = null;
            this.workerQueue.Clear();
            var allWords = this.tracker.ClearAllSegments();
            foreach (var word in allWords)
            {
                this.highlightRenderingServices.RemoveSegment(word);
            }
        }

        /// <summary>
        /// used for ignore all, add to dictionary to remove highlights from new word
        /// </summary>
        /// <param name="word">The word.</param>
        public void UnhighlightWord(string word)
        {
            var relevantHighlights = this.tracker.GetSegments(word, this.ProcessWord);
            foreach (var segment in relevantHighlights)
            {
                this.highlightRenderingServices.RemoveSegment(segment.Segment);
                this.tracker.RemoveSegment(segment.Pointer);
            }
        }

        /// <summary>
        /// Finds the misspelling.
        /// </summary>
        /// <param name="markupPointer">The markup pointer.</param>
        /// <returns>MisspelledWordInfo.</returns>
        public MisspelledWordInfo FindMisspelling(MarkupPointer markupPointer) =>
            this.tracker.FindSegment(this.markupServices, markupPointer.PointerRaw);

        /// <summary>
        /// Removes the highlight on the range.
        /// </summary>
        /// <param name="range">The range.</param>
        public void UnhighlightRange(MarkupRange range)
        {
            var segments = this.tracker.GetSegments(range.Start.PointerRaw, range.End.PointerRaw);

            if (segments == null)
            {
                // This can happen when realtime spell checking is disabled
                return;
            }

            foreach (var segment in segments)
            {
                this.highlightRenderingServices.RemoveSegment(segment);
            }
        }
    }
}
