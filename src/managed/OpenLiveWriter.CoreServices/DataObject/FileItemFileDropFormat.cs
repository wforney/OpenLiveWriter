// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    /// <summary>
    /// Definition of file item file drop format
    /// </summary>
    internal class FileItemFileDropFormat : IFileItemFormat
    {
        /// <summary>
        /// Does the passed data object have this format?
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>true if the data object has the format, otherwise false</returns>
        public bool CanCreateFrom(IDataObject dataObject) =>
            OleDataObjectHelper.GetDataPresentSafe(dataObject, DataFormats.FileDrop) && dataObject.GetData(DataFormats.FileDrop) != null;

        /// <summary>
        /// Create an array of file items based on the specified data object
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>array of file items</returns>
        public ICollection<FileItem> CreateFileItems(IDataObject dataObject) => FileItemFromFileDrop.CreateArrayFromDataObject(dataObject);
    }
}

