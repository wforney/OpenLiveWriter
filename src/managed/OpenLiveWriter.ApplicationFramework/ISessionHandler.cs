// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface ISessionHandler
    /// </summary>
    public interface ISessionHandler
    {
        /// <summary>
        /// Called when [end session].
        /// </summary>
        void OnEndSession();
    }
}
