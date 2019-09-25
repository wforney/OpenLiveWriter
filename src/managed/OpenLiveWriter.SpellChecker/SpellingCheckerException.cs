// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;

    /// <summary>
    /// Exception that occurs during spell checking
    /// </summary>
    public class SpellingCheckerException : ApplicationException
    {
        /// <summary>
        /// Initialize with just an error message
        /// </summary>
        /// <param name="message"></param>
        public SpellingCheckerException(string message)
            : this(message, 0)
        {
        }

        /// <summary>
        /// Initialize with a message and native error
        /// </summary>
        /// <param name="message">error message</param>
        /// <param name="nativeError">native error code</param>
        public SpellingCheckerException(string message, int nativeError)
            : base(message) =>
            this.NativeError = nativeError;

        /// <summary>
        /// Underlying error code from native implementation
        /// </summary>
        public int NativeError { get; }
    }
}
