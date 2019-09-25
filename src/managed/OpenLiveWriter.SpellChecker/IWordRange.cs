// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// Interface representing a word-range to be spell checked
    /// </summary>
    [Guid("F4FB57BC-5DB2-484A-8CDC-1EA270BE3821")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IWordRange
    {
        /// <summary>
        /// Is there another word in the range?
        /// </summary>
        /// <returns>true if there is another word in the range</returns>
        bool HasNext();

        /// <summary>
        /// Advance to the next word in the range
        /// </summary>
        void Next();

        /// <summary>
        /// Get the current word
        /// </summary>
        /// <value>The current word.</value>
        string CurrentWord { get; }

        /// <summary>
        /// Place the cursor
        /// </summary>
        void PlaceCursor();

        /// <summary>
        /// Highlight the current word, adjusted by the offset and length.
        /// The offset and length do not change the current word,
        /// they just affect the application of the highlight.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        void Highlight(int offset, int length);

        /// <summary>
        /// Remove highlighting from the range
        /// </summary>
        void RemoveHighlight();

        /// <summary>
        /// Replace the current word
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="newText">The new text.</param>
        void Replace(int offset, int length, string newText);

        /// <summary>
        /// Tests the current word to determine if it is part of a URL sequence.
        /// </summary>
        /// <returns><c>true</c> if [is current word URL part]; otherwise, <c>false</c>.</returns>
        bool IsCurrentWordUrlPart();

        /// <summary>
        /// Tests the current word to determine if it is contained in smart content.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool FilterApplies();

        /// <summary>
        /// Tests the current word from an offset for a given length to determine if it is contained in smart content.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        bool FilterAppliesRanged(int offset, int length);
    }
}
