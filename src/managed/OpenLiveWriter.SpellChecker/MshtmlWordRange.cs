// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Mshtml;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.SpellChecker
{
    public delegate void DamageFunction(MarkupRange range);

    public delegate bool MarkupRangeFilter(MarkupRange range);

    /// <summary>
    /// Implementation of IWordRange for an MSHTML control
    /// </summary>
    public class MshtmlWordRange : IWordRange
    {
        MarkupRangeFilter filter = null;
        DamageFunction damageFunction = null;

        /// <summary>
        /// Initialize word range for the entire body of the document
        /// </summary>
        /// <param name="mshtml">mshtml control</param>
        public MshtmlWordRange(MshtmlControl mshtmlControl)
            : this(mshtmlControl.HTMLDocument, false, null, null)
        {
        }

        /// <summary>
        /// Initialize word range for the specified markup-range within the document
        /// </summary>
        public MshtmlWordRange(IHTMLDocument document, bool useDocumentSelectionRange, MarkupRangeFilter filter, DamageFunction damageFunction)
        {
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            IHTMLDocument2 document2 = (IHTMLDocument2)document;
            MarkupRange markupRange;
            if (useDocumentSelectionRange)
            {
                markupRange = markupServices.CreateMarkupRange(document2.selection);
            }
            else
            {
                // TODO: Although this works fine, it would be better to only spellcheck inside editable regions.
                markupRange = markupServices.CreateMarkupRange(document2.body, false);
            }

            Init(document, markupServices, markupRange, filter, damageFunction, useDocumentSelectionRange);
        }

        /// <summary>
        /// Initialize word range for the specified markup-range within the document
        /// </summary>
        public MshtmlWordRange(IHTMLDocument document, MarkupRange markupRange, MarkupRangeFilter filter, DamageFunction damageFunction)
        {
            MshtmlMarkupServices markupServices = new MshtmlMarkupServices((IMarkupServicesRaw)document);
            Init(document, markupServices, markupRange, filter, damageFunction, true);
        }

        private void Init(IHTMLDocument document, MshtmlMarkupServices markupServices, MarkupRange selectionRange, MarkupRangeFilter filter, DamageFunction damageFunction, bool expandRange)
        {
            // save references
            this.htmlDocument = document;
            MarkupServices = markupServices;
            SelectionRange = selectionRange;
            this.filter = filter;
            this.damageFunction = damageFunction;

            // If the range is already the body, don't expand it or else it will be the whole document
            if (expandRange)
                ExpandRangeToWordBoundaries(selectionRange);

            // initialize pointer to beginning of selection range
            MarkupPointer wordStart = MarkupServices.CreateMarkupPointer(selectionRange.Start);
            MarkupPointer wordEnd = MarkupServices.CreateMarkupPointer(selectionRange.Start);

            //create the range for holding the current word.
            //Be sure to set its gravity so that it stays around text that get replaced.
            CurrentWordRange = MarkupServices.CreateMarkupRange(wordStart, wordEnd);
            CurrentWordRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
            CurrentWordRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;

            currentVirtualPosition = CurrentWordRange.End.Clone();
        }

        public static void ExpandRangeToWordBoundaries(MarkupRange range)
        {
            //adjust the selection so that it entirely covers the first and last words.

            range.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
            range.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            range.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
            range.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
        }

        public bool IsCurrentWordUrlPart()
        {
            return IsRangeInUrl(CurrentWordRange);
        }

        public bool FilterApplies()
        {
            return filter != null && filter(CurrentWordRange);
        }
        public bool FilterAppliesRanged(int offset, int length)
        {
            MarkupRange adjustedRange = CurrentWordRange.Clone();
            MarkupHelpers.AdjustMarkupRange(ref stagingTextRange, adjustedRange, offset, length);
            return filter != null && filter(adjustedRange);
        }

        /// <summary>
        /// Do we have another word in our range?
        /// </summary>
        /// <returns></returns>
        public bool HasNext()
        {
            return CurrentWordRange.End.IsLeftOf(SelectionRange.End);
        }

        /// <summary>
        /// Advance to next word
        /// </summary>
        public void Next()
        {
            CurrentWordRange.End.MoveToPointer(currentVirtualPosition);

            do
            {
                //advance the start pointer to the beginning of next word
                if(!CurrentWordRange.End.IsEqualTo(SelectionRange.Start)) //avoids skipping over the first word
                {
                    //fix bug 1848 - move the start to the end pointer before advancing to the next word
                    //this ensures that the "next begin" is after the current selection.
                    CurrentWordRange.Start.MoveToPointer(CurrentWordRange.End);
                    CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);

                    CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDBEGIN);
                }
                else
                {
                    //Special case for putting the start pointer at the beginning of the
                    //correct word when the selection may or may not be already adjacent
                    //to the the beginning of the word.
                    //Note: theoretically, the selection adjustment in the constructor
                    //guarantees that we will be flush against the first word, so we could
                    //probably do nothing, but it works, so we'll keep it.
                    CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);
                    CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                }

                //advance the end pointer to the end of next word
                CurrentWordRange.End.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_NEXTWORDEND);

                if(CurrentWordRange.Start.IsRightOf(CurrentWordRange.End))
                {
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
                    CurrentWordRange.Start.MoveToPointer(CurrentWordRange.End);
                    CurrentWordRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN);
                }
            } while( MarkupHelpers.GetRangeTextFast(CurrentWordRange) == null &&
                     CurrentWordRange.End.IsLeftOf(SelectionRange.End));

            currentVirtualPosition.MoveToPointer(CurrentWordRange.End);

            if(CurrentWordRange.End.IsRightOf(SelectionRange.End))
            {
                //then collapse the range so that CurrentWord returns Empty;
                CurrentWordRange.Start.MoveToPointer(CurrentWordRange.End);
            }
            else
            {
                MarkupRange testRange = CurrentWordRange.Clone();
                testRange.Collapse(false);
                testRange.End.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_NEXTCHAR, SelectionRange.End);
                if (MarkupHelpers.GetRangeHtmlFast(testRange) == ".")
                {
                    CurrentWordRange.End.MoveToPointer(testRange.End);
                }
            }
        }

        private bool IsRangeInUrl(MarkupRange range)
        {
            //must have this range cloned, otherwise in some cases IsInsideURL call
            // was MOVING the current word range! if "www.foo.com" was in the editor,
            // when the final "\"" was the current word, this call MOVED the current
            // word range BACK to www.foo.com, then nextWord would get "\"" and a loop
            // would occur (bug 411528)
            range = range.Clone();
            IMarkupPointer2Raw p2StartRaw = (IMarkupPointer2Raw)range.Start.PointerRaw;
            p2StartRaw.IsInsideURL(range.End.PointerRaw, out bool insideUrl);
            return insideUrl;
        }

        /// <summary>
        /// Get the text of the current word
        /// </summary>
        public string CurrentWord
        {
            get
            {
                return CurrentWordRange.Text ?? "";
            }
        }

        public void PlaceCursor()
        {
            CurrentWordRange.Collapse(false);
            CurrentWordRange.ToTextRange().select();
        }

        /// <summary>
        /// Highlight the current word
        /// </summary>
        public void Highlight(int offset, int length)
        {
            // select word
            MarkupRange highlightRange = CurrentWordRange.Clone();
            MarkupHelpers.AdjustMarkupRange(highlightRange, offset, length);

            try
            {
                highlightRange.ToTextRange().select();
            }
            catch (COMException ce)
            {
                // Fix bug 772709: This error happens when we try to select un-selectable objects.
                if (ce.ErrorCode != unchecked((int)0x800A025E))
                    throw;
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
                ((IHTMLDocument2) htmlDocument).selection.empty();
            }
            catch (COMException ce)
            {
                if (ce.ErrorCode != unchecked((int)0x800A025E))
                    throw;
            }
        }

        /// <summary>
        /// Replace the text of the current word
        /// </summary>
        public void Replace(int offset, int length, string newText)
        {
            MarkupRange origRange = CurrentWordRange.Clone();
            // set the text
            CurrentWordRange.Text = StringHelper.Replace(CurrentWordRange.Text, offset, length, newText);
            damageFunction(origRange);
        }

        /// <summary>
        /// Markup services for mshtml control
        /// </summary>
        private MshtmlMarkupServices MarkupServices { get; set; }

        /// <summary>
        /// Control we are providing a word range for
        /// </summary>
        //private MshtmlControl mshtmlControl ;
        private IHTMLDocument htmlDocument;

        public MarkupRange SelectionRange { get; private set; }

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

        private IHTMLTxtRange stagingTextRange;

        public MarkupRange CurrentWordRange { get; private set; }
    }
}
