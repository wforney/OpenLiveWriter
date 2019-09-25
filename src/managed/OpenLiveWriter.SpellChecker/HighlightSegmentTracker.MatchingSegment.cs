// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{

    using Mshtml;

    public partial class HighlightSegmentTracker
    {
        /// <summary>
        /// The MatchingSegment class.
        /// </summary>
        public class MatchingSegment
        {
            /// <summary>
            /// The pointer
            /// </summary>
            public IMarkupPointerRaw Pointer;

            /// <summary>
            /// The segment
            /// </summary>
            public IHighlightSegmentRaw Segment;

            /// <summary>
            /// Initializes a new instance of the <see cref="MatchingSegment"/> class.
            /// </summary>
            /// <param name="seg">The seg.</param>
            /// <param name="pointer">The pointer.</param>
            public MatchingSegment(IHighlightSegmentRaw seg, IMarkupPointerRaw pointer)
            {
                this.Segment = seg;
                this.Pointer = pointer;
            }
        }
    }
}
