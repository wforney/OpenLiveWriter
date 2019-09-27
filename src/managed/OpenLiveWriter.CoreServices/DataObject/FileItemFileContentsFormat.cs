// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    /// <summary>
    /// Definition of file item file contents format
    /// </summary>
    internal class FileItemFileContentsFormat : IFileItemFormat
    {
        /// <summary>
        /// Does the passed data object have this format?
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>true if the data object has the format, otherwise false</returns>
        public bool CanCreateFrom(IDataObject dataObject)
        {
            // GetDataPresent is not always a reliable indicator of what data is
            // actually available. For Outlook Express, if you call GetDataPresent on
            // FileGroupDescriptor it returns false however if you actually call GetData
            // you will get the FileGroupDescriptor! Therefore, we are going to
            // enumerate the available formats and check that list rather than
            // checking GetDataPresent
            var formats = new List<string>(dataObject.GetFormats());

            // check for FileContents
            return (formats.Contains(DataFormatsEx.FileGroupDescriptorFormat)
                || formats.Contains(DataFormatsEx.FileGroupDescriptorWFormat))
                && formats.Contains(DataFormatsEx.FileContentsFormat);
        }

        /// <summary>
        /// Create an array of file items based on the specified data object
        /// </summary>
        /// <param name="dataObject">data object</param>
        /// <returns>array of file items</returns>
        public ICollection<FileItem> CreateFileItems(IDataObject dataObject) => FileItemFromFileContents.CreateArrayFromDataObject(dataObject);
    }
}
