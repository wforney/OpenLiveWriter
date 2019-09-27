// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Class which contains helper methods used for cracking CF_FILECONTENTS
    /// </summary>
    public class FileContentsHelper
    {
        /// <summary>
        /// Utility function to extract an array of file contents file descriptors from
        /// an IDataObject instance
        /// </summary>
        /// <param name="dataObject">data object to extract descriptors from</param>
        /// <returns>array of file descriptors</returns>
        public static ICollection<FileDescriptor> GetFileDescriptors(IDataObject dataObject)
        {
            // Use an OleDataObject
            var oleDataObject = OleDataObject.CreateFrom(dataObject);
            if (oleDataObject == null)
            {
                const string message = "DataObject not valid for FileContents!";
                Debug.Fail(message);
                throw new InvalidOperationException(message);
            }

            // Try to get the data as FileGroupDescriptorW then try to get it
            // as FileGroupDescriptor
            bool bFileNameIsWide;
            var stgMedium = (OleStgMediumHGLOBAL)oleDataObject.GetData(DataFormatsEx.FileGroupDescriptorWFormat, TYMED.HGLOBAL);
            if (stgMedium != null)
            {
                bFileNameIsWide = true;
            }
            else
            {
                stgMedium = (OleStgMediumHGLOBAL)oleDataObject.GetData(DataFormatsEx.FileGroupDescriptorFormat, TYMED.HGLOBAL);

                if (stgMedium == null)
                {
                    const string message = "File group descriptor not available!";
                    Debug.Fail(message);
                    throw new InvalidOperationException(message);
                }
                else
                {
                    bFileNameIsWide = false;
                }
            }

            // Copy the descriptors
            using (stgMedium)
            {
                using (var globalMem = new HGlobalLock(stgMedium.Handle))
                {
                    // get a pointer to the count
                    var pCount = globalMem.Memory;

                    // determine the number of file descriptors
                    var count = Marshal.ReadInt32(pCount);

                    // get a pointer to the descriptors
                    var pDescriptors = new IntPtr(globalMem.Memory.ToInt32() + Marshal.SizeOf(count));

                    // allocate the array of structures that will be returned
                    var descriptors = new FileDescriptor[count];

                    // determine the sizes of the various data elements
                    const int FILENAME_BUFFER_SIZE = 260;
                    var headerSize = Marshal.SizeOf(typeof(FILEDESCRIPTOR_HEADER));
                    var fileNameSize = bFileNameIsWide ? FILENAME_BUFFER_SIZE * 2 : FILENAME_BUFFER_SIZE;
                    var totalSize = headerSize + fileNameSize;

                    // iterate through the memory block copying the FILEDESCRIPTOR structures
                    for (var i = 0; i < count; i++)
                    {
                        // determine the addresses of the various data elements
                        var pAddr = new IntPtr(pDescriptors.ToInt32() + (i * totalSize));
                        var pFileNameAddr = new IntPtr(pAddr.ToInt32() + headerSize);

                        // copy the header
                        descriptors[i].header = (FILEDESCRIPTOR_HEADER)
                                                Marshal.PtrToStructure(pAddr, typeof(FILEDESCRIPTOR_HEADER));

                        // copy the file name (use Unicode or Ansi depending upon descriptor type)
                        descriptors[i].fileName = bFileNameIsWide
                            ? Marshal.PtrToStringUni(pFileNameAddr)
                            : Marshal.PtrToStringAnsi(pFileNameAddr);
                    }

                    // return the descriptors
                    return descriptors;
                }
            }
        }

        /// <summary>
        /// TYMEDs supported by CF_FILECONTENTS
        /// </summary>
        private const TYMED FILE_CONTENTS_TYMEDS = TYMED.ISTORAGE | TYMED.ISTREAM | TYMED.FILE | TYMED.HGLOBAL;
    }
}
