// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System.Windows.Forms;

    /// <summary>
    /// The SpellingTimer class.
    /// Implements the <see cref="System.Windows.Forms.Timer" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Timer" />
    public class SpellingTimer : Timer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellingTimer"/> class.
        /// </summary>
        /// <param name="interval">The interval.</param>
        public SpellingTimer(int interval)
        {
            this.Interval = interval;
        }
    }
}
