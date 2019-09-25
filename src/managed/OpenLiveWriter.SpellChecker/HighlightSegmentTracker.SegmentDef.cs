// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{

    using Mshtml;

    public partial class HighlightSegmentTracker
    {
        /// <summary>
        /// The SegmentDef class.
        /// </summary>
        private class SegmentDef
        {
            /// <summary>
            /// The end PTR
            /// </summary>
            public readonly IMarkupPointerRaw EndPointer;

            /// <summary>
            /// The segment
            /// </summary>
            public readonly IHighlightSegmentRaw Segment;

            /// <summary>
            /// The start PTR
            /// </summary>
            public readonly IMarkupPointerRaw StartPointer;

            /// <summary>
            /// The word
            /// </summary>
            public readonly string Word;

            /// <summary>
            /// Initializes a new instance of the <see cref="SegmentDef"/> class.
            /// </summary>
            /// <param name="seg">The seg.</param>
            /// <param name="start">The start.</param>
            /// <param name="end">The end.</param>
            /// <param name="wd">The wd.</param>
            public SegmentDef(IHighlightSegmentRaw seg, IMarkupPointerRaw start, IMarkupPointerRaw end, string wd)
            {
                this.Segment = seg;
                this.StartPointer = start;
                this.EndPointer = end;
                this.Word = wd;
            }
        }
    }
}
