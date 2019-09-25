// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using CoreServices;
    using mshtml;
    using Mshtml;

    /// <summary>
    /// Delegate DamageFunction
    /// </summary>
    /// <param name="range">The range.</param>
    public delegate void DamageFunction(MarkupRange range);

    /// <summary>
    /// Delegate MarkupRangeFilter
    /// </summary>
    /// <param name="range">The range.</param>
    /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
    public delegate bool MarkupRangeFilter(MarkupRange range);

    /// <summary>
    /// Implementation of IWordRange for an MSHTML control
    /// Implements the <see cref="OpenLiveWriter.SpellChecker.IWordRange" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.SpellChecker.IWordRange" />
    public class MshtmlWordRange : IWordRange
    {
        /// <summary>
        /// In order to fix the "vs." defect (trailing periods need to
        /// be included in spell checked words) we adjust the currentWordRange.End
        /// to include trailing periods. This has the effect of triggering
        /// the "word start jumped past word end" assert in some circumstances
        /// (try typing "foo.. bar"). The currentVirtualPosition tells us how to
        /// undo the currentWordRange.End adjustment right before attempting
        /// to navigate to the next word.
        /// </summary>
        private MarkupPointer currentVirtualPosition;

        /// <summary>
        /// The damage function
        /// </summary>
        private DamageFunction damageFunction;

        /// <summary>
        /// The filter
        /// </summary>
        private MarkupRangeFilter filter;

        /// <summary>
        /// Control we are providing a word range for
        /// </summary>
        ////private MshtmlControl mshtmlControl ;
        private IHTMLDocument htmlDocument;

        /// <summary>
        /// The staging text range
        /// </summary>
        private IHTMLTxtRange stagingTextRange;

        /// <summary>
        /// Initialize word range for the entire body of the document
        /// </summary>
        /// <param name="mshtmlControl">The MSHTML control.</param>
        public MshtmlWordRange(MshtmlControl mshtmlControl) :
            this(mshtmlControl.HTMLDocument, false, null, null)
        {
        }

        /// <summary>
        /// Initialize word range for the specified markup-range within the document
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="useDocumentSelectionRange">if set to <c>true</c> [use document selection range].</param>
        /// <param name="filter">The filter.</param>
        /// <param name="damageFunction">The damage function.</param>
        public MshtmlWordRange(IHTMLDocument document, bool useDocumentSelectionRange, MarkupRangeFilter filter,
                               DamageFunction damageFunction)
        {
            var markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            var document2 = (IHTMLDocument2)document;
            var markupRange = useDocumentSelectionRange
                                          ? markupServices.CreateMarkupRange(document2.selection)
                                          // TODO: Although this works fine, it would be better to only spell check inside editable regions.
                                          : markupServices.CreateMarkupRange(document2.body, false);

            this.Init(document, markupServices, markupRange, filter, damageFunction, useDocumentSelectionRange);
        }

        /// <summary>
        /// Initialize word range for the specified markup-range within the document
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="markupRange">The markup range.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="damageFunction">The damage function.</param>
        public MshtmlWordRange(IHTMLDocument document, MarkupRange markupRange, MarkupRangeFilter filter,
                               DamageFunction damageFunction)
        {
            var markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            this.Init(document, markupServices, markupRange, filter, damageFunction, true);
        }

        /// <summary>
        /// Markup services for mshtml control
        /// </summary>
        /// <value>The markup services.</value>
        private MshtmlMarkupServices MarkupServices { get; set; }

        /// <summary>
        /// Gets the selection range.
        /// </summary>
        /// <value>The selection range.</value>
        public MarkupRange SelectionRange { get; private set; }

        /// <summary>
        /// Gets the current word range.
        /// </summary>
        /// <value>The current word range.</value>
        public MarkupRange CurrentWordRange { get; private set; }

        /// <inheritdoc />
        public bool IsCurrentWordUrlPart() => this.IsRangeInUrl(this.CurrentWordRange);

        /// <inheritdoc />
        public bool FilterApplies() => this.filter != null && this.filter(this.CurrentWordRange);

        /// <inheritdoc />
        public bool FilterAppliesRanged(int offset, int length)
        {
            var adjustedRange = this.CurrentWordRange.Clone();
            MarkupHelpers.AdjustMarkupRange(ref this.stagingTextRange, adjustedRange, offset, length);
            return this.filter != null && this.filter(adjustedRange);
        }

        /// <summary>
        /// Do we have another word in our range?
        /// </summary>
        /// <returns>true if there is another word in the range</returns>
        public bool HasNext() => this.CurrentWordRange.End.IsLeftOf(this.SelectionRange.End);

        /// <summary>
        /// Advance to next word
        /// </summary>
        public void Next()
        {
            this.CurrentWordRange.End.MoveToPointer(this.currentVirtualPosition);

            do
            {
                //advance the start pointer to the beginning of next word
                if (!this.CurrentWordRange.End.IsEqualTo(this.SelectionRange.Start)) //avoids skipping over the first word
                {
                    //fix bug 1848 - move the start to the end pointer before advancing to the next word
                    //this ensures that the "next begin" is after the current selection.
                    this.CurrentWordRange.Start.MoveToPointer(this.CurrentWordRange.End);
                    this.CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);

                    this.CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDBEGIN);
                }
                else
                {
                    //Special case for putting the start pointer at the beginning of the
                    //correct word when the selection may or may not be already adjacent
                    //to the the beginning of the word.
                    //Note: theoretically, the selection adjustment in the constructor
                    //guarantees that we will be flush against the first word, so we could
                    //probably do nothing, but it works, so we'll keep it.
                    this.CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
                    this.CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                }

                //advance the end pointer to the end of next word
                this.CurrentWordRange.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);

                if (!this.CurrentWordRange.Start.IsRightOf(this.CurrentWordRange.End))
                {
                    continue;
                }

                //Note: this was a condition that caused several bugs that caused us to stop
                //spell-checking correctly, so this logic is here in case we still have edge
                //cases that trigger it.
                //This should not occur anymore since we fixed several causes of this
                //condition by setting the currentWordRange gravity, and detecting case where
                //the selection may or may-not start at the beginning of a word.
                Debug.Fail("word start jumped past word end");

                //if this occurred, it was probably because start was already at the beginning
                //of the correct word before it was moved.  To resolve this situation, we
                //move the start pointer back to the beginning of the word that the end pointer
                //is at. Since the End pointer always advances on each iteration, this should not
                //cause an infinite loop. The worst case scenario is that we check the same word
                //more than once.
                this.CurrentWordRange.Start.MoveToPointer(this.CurrentWordRange.End);
                this.CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            } while (MarkupHelpers.GetRangeTextFast(this.CurrentWordRange) == null &&
                     this.CurrentWordRange.End.IsLeftOf(this.SelectionRange.End));

            this.currentVirtualPosition.MoveToPointer(this.CurrentWordRange.End);

            if (this.CurrentWordRange.End.IsRightOf(this.SelectionRange.End))
            {
                //then collapse the range so that CurrentWord returns Empty;
                this.CurrentWordRange.Start.MoveToPointer(this.CurrentWordRange.End);
            }
            else
            {
                var testRange = this.CurrentWordRange.Clone();
                testRange.Collapse(false);
                testRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_NEXTCHAR, this.SelectionRange.End);
                if (MarkupHelpers.GetRangeHtmlFast(testRange) == ".")
                {
                    this.CurrentWordRange.End.MoveToPointer(testRange.End);
                }
            }
        }

        /// <summary>
        /// Get the text of the current word
        /// </summary>
        /// <value>The current word.</value>
        public string CurrentWord => this.CurrentWordRange.Text ?? string.Empty;

        /// <inheritdoc />
        public void PlaceCursor()
        {
            this.CurrentWordRange.Collapse(false);
            this.CurrentWordRange.ToTextRange().select();
        }

        /// <summary>
        /// Highlight the current word
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        public void Highlight(int offset, int length)
        {
            // select word
            var highlightRange = this.CurrentWordRange.Clone();
            MarkupHelpers.AdjustMarkupRange(highlightRange, offset, length);

            try
            {
                highlightRange.ToTextRange().select();
            }
            catch (COMException ce)
            {
                // Fix bug 772709: This error happens when we try to select un-selectable objects.
                if (ce.ErrorCode != unchecked((int)0x800A025E))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Remove highlighting from the document
        /// </summary>
        public void RemoveHighlight()
        {
            // clear document selection
            try
            {
                ((IHTMLDocument2)this.htmlDocument).selection.empty();
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode != unchecked((int)0x800A025E))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Replace the text of the current word
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="newText">The new text.</param>
        public void Replace(int offset, int length, string newText)
        {
            var origRange = this.CurrentWordRange.Clone();

            // set the text
            this.CurrentWordRange.Text = StringHelper.Replace(this.CurrentWordRange.Text, offset, length, newText);
            this.damageFunction(origRange);
        }

        /// <summary>
        /// Initializes the specified document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="markupServices">The markup services.</param>
        /// <param name="selectionRange">The selection range.</param>
        /// <param name="filter">The filter.</param>
        /// <param name="damageFunction">The damage function.</param>
        /// <param name="expandRange">if set to <c>true</c> [expand range].</param>
        private void Init(IHTMLDocument document, MshtmlMarkupServices markupServices, MarkupRange selectionRange,
                          MarkupRangeFilter filter, DamageFunction damageFunction, bool expandRange)
        {
            // save references
            this.htmlDocument = document;
            this.MarkupServices = markupServices;
            this.SelectionRange = selectionRange;
            this.filter = filter;
            this.damageFunction = damageFunction;

            // If the range is already the body, don't expand it or else it will be the whole document
            if (expandRange)
            {
                MshtmlWordRange.ExpandRangeToWordBoundaries(selectionRange);
            }

            // initialize pointer to beginning of selection range
            var wordStart = this.MarkupServices.CreateMarkupPointer(selectionRange.Start);
            var wordEnd = this.MarkupServices.CreateMarkupPointer(selectionRange.Start);

            //create the range for holding the current word.
            //Be sure to set its gravity so that it stays around text that get replaced.
            this.CurrentWordRange = this.MarkupServices.CreateMarkupRange(wordStart, wordEnd);
            this.CurrentWordRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            this.CurrentWordRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

            this.currentVirtualPosition = this.CurrentWordRange.End.Clone();
        }

        /// <summary>
        /// Expands the range to word boundaries.
        /// </summary>
        /// <param name="range">The range.</param>
        public static void ExpandRangeToWordBoundaries(MarkupRange range)
        {
            //adjust the selection so that it entirely covers the first and last words.

            range.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
            range.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            range.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            range.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
        }

        /// <summary>
        /// Determines whether [is range in URL] [the specified range].
        /// </summary>
        /// <param name="range">The range.</param>
        /// <returns><c>true</c> if [is range in URL] [the specified range]; otherwise, <c>false</c>.</returns>
        private bool IsRangeInUrl(MarkupRange range)
        {
            //must have this range cloned, otherwise in some cases IsInsideURL call
            // was MOVING the current word range! if "www.foo.com" was in the editor,
            // when the final "\"" was the current word, this call MOVED the current
            // word range BACK to www.foo.com, then nextWord would get "\"" and a loop
            // would occur (bug 411528)
            range = range.Clone();
            var p2StartRaw = (IMarkupPointer2Raw)range.Start.PointerRaw;
            p2StartRaw.IsInsideURL(range.End.PointerRaw, out var insideUrl);
            return insideUrl;
        }
    }
}
