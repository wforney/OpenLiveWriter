// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.Collections;
    using System.Globalization;

    using Mshtml;

    //used to store the highlight segments in a post for tracking spelling changes
    /// <summary>
    /// The HighlightSegmentTracker class.
    /// </summary>
    public partial class HighlightSegmentTracker
    {
        /// <summary>
        /// Delegate CheckWordSpelling
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns><c>true</c> if word is spelled correctly, <c>false</c> otherwise.</returns>
        public delegate bool CheckWordSpelling(string word);

        /// <summary>
        /// The list
        /// </summary>
        private readonly SortedList list;

        /// <inheritdoc />
        /// <remarks>
        /// this needs to be on a post by post basis
        /// </remarks>
        public HighlightSegmentTracker() => this.list = new SortedList(new MarkupPointerComparer());

        /// <summary>
        /// adds a segment to the list
        /// used when a misspelled word is found
        /// </summary>
        /// <param name="segment">The segment.</param>
        /// <param name="wordHere">The word here.</param>
        /// <param name="markupServices">The markup services.</param>
        public void AddSegment(IHighlightSegmentRaw segment, string wordHere, IMarkupServicesRaw markupServices)
        {
            markupServices.CreateMarkupPointer(out var start);
            markupServices.CreateMarkupPointer(out var end);
            segment.GetPointers(start, end);
            if (!this.list.ContainsKey(start))
            {
                this.list.Add(start, new SegmentDef(segment, start, end, wordHere));
            }
        }

        //find all the segments in a specific range
        //used to clear out a section when it is getting rechecked
        //need to expand selection from these bounds out around full words
        /// <summary>
        /// Gets the segments.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>IHighlightSegmentRaw[].</returns>
        public IHighlightSegmentRaw[] GetSegments(IMarkupPointerRaw start, IMarkupPointerRaw end)
        {
            if (this.list.Count == 0)
            {
                return null;
            }

            var firstSegmentInd = -1;
            var lastSegmentInd = this.list.Count;
            bool test;

            //find the first segment after the start pointer
            do
            {
                firstSegmentInd++;

                //check if we have gone through the whole selection
                if (firstSegmentInd >= lastSegmentInd)
                {
                    return null;
                }

                var x = (SegmentDef)this.list.GetByIndex(firstSegmentInd);
                start.IsRightOf(x.StartPointer, out test);
            } while (test);

            do
            {
                lastSegmentInd--;

                //check if we have gone through the whole selection
                if (lastSegmentInd < firstSegmentInd)
                {
                    return null;
                }

                var x = (SegmentDef)this.list.GetByIndex(lastSegmentInd);
                end.IsLeftOf(x.StartPointer, out test);
            } while (test);

            return this.Subarray(firstSegmentInd, lastSegmentInd);
        }

        /// <summary>
        /// Clears all segments.
        /// </summary>
        /// <returns>IHighlightSegmentRaw[].</returns>
        public IHighlightSegmentRaw[] ClearAllSegments() => this.Subarray(0, this.list.Count - 1);

        //find all the segments with a specific misspelled word
        //used to clear for ignore all, add to dictionary
        /// <summary>
        /// Gets the segments.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="checkSpelling">The check spelling.</param>
        /// <returns>MatchingSegment[].</returns>
        public MatchingSegment[] GetSegments(string word, CheckWordSpelling checkSpelling)
        {
            var segments = new ArrayList();
            for (var i = 0; i < this.list.Count; i++)
            {
                var x = (SegmentDef)this.list.GetByIndex(i);

                //TODO: Change with new cultures added!!!
                if (string.Compare(word, x.Word, true, CultureInfo.InvariantCulture) != 0)
                {
                    continue;
                }

                //check spelling--capitalized word may be ok, but not mixed case, etc.
                if (!checkSpelling(x.Word))
                {
                    segments.Add(new MatchingSegment(x.Segment, x.StartPointer));
                }
            }

            return (MatchingSegment[])segments.ToArray(typeof(MatchingSegment));
        }

        /// <summary>
        /// Removes the segment.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        public void RemoveSegment(IMarkupPointerRaw pointer) => this.list.Remove(pointer);

        /// <summary>
        /// Finds the segment.
        /// </summary>
        /// <param name="markupServices">The markup services.</param>
        /// <param name="current">The current.</param>
        /// <returns>MisspelledWordInfo.</returns>
        public MisspelledWordInfo FindSegment(MshtmlMarkupServices markupServices, IMarkupPointerRaw current)
        {
            // binary search
            var start = 0;
            var end = this.list.Count - 1;
            var i = HighlightSegmentTracker.Middle(start, end);
            while (-1 != i)
            {
                var x = (SegmentDef)this.list.GetByIndex(i);
                current.IsRightOfOrEqualTo(x.StartPointer, out var startTest);
                if (startTest)
                {
                    current.IsLeftOfOrEqualTo(x.EndPointer, out var endTest);
                    if (endTest)
                    {
                        var pStart = markupServices.CreateMarkupPointer(x.StartPointer);
                        var pEnd = markupServices.CreateMarkupPointer(x.EndPointer);
                        var range = markupServices.CreateMarkupRange(pStart, pEnd);

                        //this could be a "phantom" range...no more content due to uncommitted damage or other reasons
                        //if it is phantom, remove it from the tracker and return null
                        if (range.Text == null)
                        {
                            this.list.RemoveAt(i);
                            return null;
                        }

                        return new MisspelledWordInfo(range, x.Word);
                    }

                    start = i + 1;
                }
                else
                {
                    end = i - 1;
                }

                i = HighlightSegmentTracker.Middle(start, end);
            }

            return null;
        }

        /// <summary>
        /// Middles the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>System.Int32.</returns>
        private static int Middle(int start, int end) =>
            start <= end ? (int)Math.Floor(Convert.ToDouble((start + end) / 2)) : -1;

        /// <summary>
        /// Subarrays the specified start.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>IHighlightSegmentRaw[].</returns>
        private IHighlightSegmentRaw[] Subarray(int start, int end)
        {
            var count = end - start + 1;
            var segments = new IHighlightSegmentRaw[count];

            //fill in array by removing from the list starting at the end, so that
            // deleting from the list doesn't change the other indices
            for (var i = end; i >= start; i--)
            {
                segments[--count] = ((SegmentDef)this.list.GetByIndex(i)).Segment;
                this.list.RemoveAt(i);
            }

            return segments;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear() => this.list.Clear();
    }
}
