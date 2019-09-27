// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents information about a file received in a FileDrop or FileContents
    /// data transfer operation. This file should be construed as a "logical"
    /// file where the file's name is represented by the Name property and the
    /// files contents are represented by the ContentsPath property.
    /// </summary>
    public abstract class FileItem
    {
        private readonly string name = null;

        /// <summary>
        /// Initialize a FileInfo for a file of the specified name
        /// </summary>
        /// <param name="name"></param>
        protected FileItem(string name) => this.name = name;

        /// <summary>
        /// Name of the file
        /// </summary>
        public string Name => FileHelper.StripInvalidChars(this.name);

        /// <summary>
        /// Clsid of the file (defines the type of the file)
        /// </summary>
        public virtual Guid Clsid => Guid.Empty;

        /// <summary>
        /// Determines whether this file is a directory
        /// </summary>
        public abstract bool IsDirectory { get; }

        /// <summary>
        /// If this file is a directory, the children contained in
        /// the directory (if there are no children an empty array is
        /// returned)
        /// </summary>
        public virtual ICollection<FileItem> Children => (ICollection<FileItem>)noChildren.ToArray().Clone();

        /// <summary>
        /// Path where the contents of the file can be found
        /// </summary>
        public abstract string ContentsPath { get; }

        /// <summary>
        /// Default array returned for no children
        /// </summary>
        protected internal static ICollection<FileItem> noChildren = new FileItem[0];
    }
}

