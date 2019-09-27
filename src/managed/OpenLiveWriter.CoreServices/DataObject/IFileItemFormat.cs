// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    /// <summary>
    /// Generic interface used to represent a file item format
    /// </summary>
    internal interface IFileItemFormat
    {
        /// <summary>
        /// Does the passed data object have this format?
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>true if the data object has the format, otherwise false</returns>
        bool CanCreateFrom(IDataObject dataObject);

        /// <summary>
        /// Create an array of file items based on the specified data object
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>array of file items</returns>
        ICollection<FileItem> CreateFileItems(IDataObject dataObject);
    }
}
