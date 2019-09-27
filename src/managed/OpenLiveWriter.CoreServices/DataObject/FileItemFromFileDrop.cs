// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;

    /// <summary>
    /// A file item that is based on a physical path
    /// </summary>
    internal class FileItemFromFileDrop : FileItem
    {
        /// <summary>
        /// Create an array of FileItem objects based on a data object that contains a FileDrop
        /// </summary>
        /// <param name="dataObject">data object containing the file drop</param>
        /// <returns></returns>
        public static FileItem[] CreateArrayFromDataObject(IDataObject dataObject)
        {
            string[] filePaths = (string[])dataObject.GetData(DataFormats.FileDrop);
            return CreateArrayFromPaths(filePaths);
        }

        /// <summary>
        /// Creates an array of FileInfo objects from an array of paths
        /// </summary>
        /// <param name="paths">array of file system paths</param>
        /// <returns>array of FileInfo objects corresponding to the specified paths</returns>
        public static FileItem[] CreateArrayFromPaths(string[] paths)
        {
            ArrayList fileList = new ArrayList();
            // build an array of FileItem objects based on the specified paths
            foreach (string path in paths)
            {
                if (File.Exists(path) || Directory.Exists(path)) //bugfix 1850 - don't return files that no longer exist.
                    fileList.Add(new FileItemFromFileDrop(path));
            }

            // return the array
            return (FileItem[])fileList.ToArray(typeof(FileItem));
        }

        /// <summary>
        /// Initialize with a full physical path
        /// </summary>
        /// <param name="physicalPath">physical path</param>
        private FileItemFromFileDrop(string physicalPath)
            : base(Path.GetFileName(physicalPath))
        {
            // verify that the file exists
            if (!File.Exists(physicalPath) && !Directory.Exists(physicalPath))
            {
                throw new FileNotFoundException(
                    "PhysicalPathFileItem could not find file", physicalPath);
            }

            // initialize members
            this.physicalPath = physicalPath;
            this.fileInfo = new FileInfo(physicalPath);
        }

        /// <summary>
        /// Determines whether this file is a directory
        /// </summary>
        public override bool IsDirectory => (this.fileInfo.Attributes & FileAttributes.Directory) > 0;

        /// <summary>
        /// If this file is a directory, the children contained in the directory
        /// (if there are no children an empty array is returned)
        /// </summary>
        public override ICollection<FileItem> Children =>
            this.IsDirectory ? CreateArrayFromPaths(Directory.GetFileSystemEntries(this.physicalPath)) : noChildren;

        /// <summary>
        /// Path where the contents of the file can be found
        /// </summary>
        public override string ContentsPath => this.physicalPath;

        /// <summary>
        /// The full path to the file containing the FileItem
        /// </summary>
        private readonly string physicalPath = null;

        /// <summary>
        /// FileInfo corresponding to the underlying physical file
        /// </summary>
        private readonly FileInfo fileInfo = null;
    }
}

