// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Interface used to manipulate the state, layout, and supporting files of SmartContent objects.
    /// </summary>
    public interface ISmartContent
    {
        /// <summary>
        /// Property-set that represents the state of a SmartContent object.
        /// </summary>
        IProperties Properties { get; }

        /// <summary>
        /// Supporting-files used by the SmartContent object.
        /// </summary>
        ISupportingFiles Files { get; }

        /// <summary>
        /// Layout options for SmartContent object.
        /// </summary>
        ILayoutStyle Layout { get; }
    }
}
