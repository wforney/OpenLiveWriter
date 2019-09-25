// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Runtime.InteropServices;

    /// <summary>
    /// The IWordRangeProvider interface.
    /// </summary>
    [Guid("F4F06001-99F6-448F-9199-E863D771066B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComVisible(true)]
    public interface IWordRangeProvider
    {
        /// <summary>
        /// Gets the subject spell check word range.
        /// </summary>
        /// <returns>IWordRange.</returns>
        IWordRange GetSubjectSpellcheckWordRange();

        /// <summary>
        /// Closes the subject spell check word range.
        /// </summary>
        void CloseSubjectSpellcheckWordRange();
    }
}
