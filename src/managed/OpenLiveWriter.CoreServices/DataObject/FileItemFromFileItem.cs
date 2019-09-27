// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Collections.Generic;
    using System.Windows.Forms;

    /// <summary>
    /// A file item that is based on another file item
    /// </summary>
    internal class FileItemFromFileItem : FileItem
    {
        /// <summary>
        /// Create an array of FileItem objects based on a data object that contains a FileDrop
        /// </summary>
        /// <param name="dataObject">data object containing the file drop</param>
        /// <returns></returns>
        public static FileItem[] CreateArrayFromDataObject(IDataObject dataObject) => (FileItem[])dataObject.GetData(typeof(FileItem[]));

        /// <summary>
        /// Initialize with a full physical path
        /// </summary>
        /// <param name="physicalPath">physical path</param>
        private FileItemFromFileItem(FileItem fileItem)
            : base(fileItem.Name)
        {
            // initialize members
            this.fileItem = fileItem;
        }

        /// <summary>
        /// Determines whether this file is a directory
        /// </summary>
        public override bool IsDirectory
        {
            get { return fileItem.IsDirectory; }
        }

        /// <summary>
        /// If this file is a directory, the children contained in the directory
        /// (if there are no children an empty array is returned)
        /// </summary>
        public override ICollection<FileItem> Children
        {
            get { return fileItem.Children; }
        }

        /// <summary>
        /// Path where the contents of the file can be found
        /// </summary>
        public override string ContentsPath
        {
            get { return fileItem.ContentsPath; }
        }

        /// <summary>
        /// FileItem we are enclosing/delegating to
        /// </summary>
        private FileItem fileItem = null;
    }
}

