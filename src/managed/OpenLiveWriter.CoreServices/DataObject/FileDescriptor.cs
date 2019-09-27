// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Type used to describe FileContents files
    /// </summary>
    public struct FileDescriptor
    {
        /// <summary>
        /// Standard file metainfo
        /// </summary>
        public FILEDESCRIPTOR_HEADER header;

        /// <summary>
        /// Name of file
        /// </summary>
        public string fileName;
    }
}
