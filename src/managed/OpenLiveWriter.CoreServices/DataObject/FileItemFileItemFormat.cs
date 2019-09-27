// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections.Generic;
using System.Windows.Forms;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Definition of file item file drop format
    /// </summary>
    internal class FileItemFileItemFormat : IFileItemFormat
    {
        /// <summary>
        /// Does the passed data object have this format?
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>true if the data object has the format, otherwise false</returns>
        public bool CanCreateFrom(IDataObject dataObject) =>
            OleDataObjectHelper.GetDataPresentSafe(dataObject, typeof(FileItem[]));

        /// <summary>
        /// Create an array of file items based on the specified data object
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>array of file items</returns>
        public ICollection<FileItem> CreateFileItems(IDataObject dataObject) =>
            FileItemFromFileItem.CreateArrayFromDataObject(dataObject);
    }
}

