// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface IStatusBar
    /// </summary>
    public interface IStatusBar
    {
        /// <summary>
        /// Sets the word count message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        void SetWordCountMessage(string msg);

        /// <summary>
        /// Pushes the status message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        void PushStatusMessage(string msg);

        /// <summary>
        /// Pops the status message.
        /// </summary>
        void PopStatusMessage();

        /// <summary>
        /// Sets the status message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        void SetStatusMessage(string msg);
    }
}


