// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Diagnostics;

    /// <summary>
    /// Class NullStatusBar.
    /// Implements the <see cref="IStatusBar" />
    /// </summary>
    /// <seealso cref="IStatusBar" />
    public class NullStatusBar : IStatusBar
    {
        /// <summary>
        /// The MSG count
        /// </summary>
        private int msgCount;

        /// <summary>
        /// Sets the word count message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void SetWordCountMessage(string msg)
        {
        }

        /// <summary>
        /// Pushes the status message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void PushStatusMessage(string msg) => this.msgCount++;

        /// <summary>
        /// Pops the status message.
        /// </summary>
        public void PopStatusMessage()
        {
            this.msgCount--;
            Debug.Assert(this.msgCount >= 0);
        }

        /// <summary>
        /// Sets the status message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void SetStatusMessage(string msg)
        {
        }
    }
}


