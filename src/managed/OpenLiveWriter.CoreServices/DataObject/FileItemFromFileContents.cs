// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.StructuredStorage;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// FileItem that refers to a 'virtual' file as implemented by the ole data
    /// transfer FileContents types
    /// </summary>
    internal class FileItemFromFileContents : FileItem
    {
        /// <summary>
        /// Create an array of FileItem objects based on a data object that contains
        /// FileContents
        /// </summary>
        /// <param name="dataObject">data object containing FileContents</param>
        /// <returns>Array of FileItem objects. Returns null if the FileContents
        /// could not be extracted from the passed dataObject</returns>
        public static ICollection<FileItem> CreateArrayFromDataObject(IDataObject dataObject)
        {
            try
            {
                // get the ole data object
                var oleDataObject = OleDataObject.CreateFrom(dataObject);
                if (oleDataObject == null)
                {
                    Debug.Fail("Unable to access OleDataObject!");
                    return null;
                }

                // get an array of file descriptors specifying the file contents
                var fileDescriptors = FileContentsHelper.GetFileDescriptors(dataObject).ToArray();

                // allocate a FileItem object for each descriptor
                var fileItems = new FileItem[fileDescriptors.Length];

                // initialize the file items
                for (var i = 0; i < fileDescriptors.Length; i++)
                {
                    fileItems[i] = new FileItemFromFileContents(fileDescriptors[i], oleDataObject, i);
                }

                // return the file items
                return fileItems;
            }
            // this is a sketchy operation involving all kinds of interop voodoo,
            // if we fail in release mode it is likely due to a drop source giving
            // us invalid or unexpected data -- handle this gracefully while
            // logging the error
            catch (Exception e)
            {
                Debug.Fail("Unexpected error accessing FileContents", e.Message);
                return null;
            }
        }

        /// <summary>
        /// Initialize using file meta-info and the OleDataObject from which
        /// to fetch the file contents when it is time to write them
        /// </summary>
        /// <param name="fileDescriptor"></param>
        /// <param name="dataObject"></param>
        /// <param name="fileIndex"></param>
        private FileItemFromFileContents(
            FileDescriptor fileDescriptor, OleDataObject oleDataObject, int fileIndex)
            : base(fileDescriptor.fileName)
        {
            this.fileDescriptor = fileDescriptor;
            this.oleDataObject = oleDataObject;
            this.fileIndex = fileIndex;
        }

        /// <summary>
        /// Determines whether this file is a directory. FileContents does not
        /// support directories so this always returns false.
        /// </summary>
        public override bool IsDirectory => false;

        /// <summary>
        /// Path where the contents of the file can be found
        /// </summary>
        public override string ContentsPath
        {
            get
            {
                if (this.contentsPath == null)
                {
                    var tempDirectory = TempFileManager.Instance.CreateTempDir();
                    this.contentsPath = Path.Combine(tempDirectory, FileHelper.GetValidFileName(this.fileDescriptor.fileName));
                    this.Write(this.contentsPath);
                }
                return this.contentsPath;
            }
        }
        private string contentsPath = null;

        /// <summary>
        /// Write the file contents to the specified path
        /// </summary>
        /// <param name="filePath">path to write file contents to</param>
        private void Write(string filePath)
        {
            // get the data in any format that it might be rendered in
            var storage = this.oleDataObject.GetData(
                                                     this.fileIndex, DataFormatsEx.FileContentsFormat,
                                                     TYMED.ISTORAGE | TYMED.ISTREAM | TYMED.FILE | TYMED.HGLOBAL);

            // check for no storage
            if (storage == null)
            {
                throw new ApplicationException(
                    "FileContents used unexpected storage type");
            }

            // copy the data to a file (implemented differently for each storage type)
            using (storage)
            {
                // structured storage
                if (storage is OleStgMediumISTORAGE istorage)
                {
                    this.CopyStorageToFile(istorage.Storage, filePath);
                }

                // stream
                else if (storage is OleStgMediumISTREAM istream)
                {
                    this.CopyStreamToFile(istream.Stream, filePath);
                }

                // temporary file
                else if (storage is OleStgMediumFILE file)
                {
                    this.CopyFileToFile(file.Path, filePath);
                }

                // global memory
                else if (storage is OleStgMediumHGLOBAL hglobal)
                {
                    this.CopyHGLOBALToFile(hglobal.Handle, filePath);
                }
                else
                {
                    throw new ApplicationException("FileContents used unexpected storage type");
                }
            }
        }

        /// <summary>
        /// Copy a structured storage into a file
        /// </summary>
        /// <param name="storage">structured storage</param>
        /// <param name="destFileName">file to copy storage into</param>
        private void CopyStorageToFile(Storage storage, string destFileName)
        {
            var destination = new Storage(destFileName, StorageMode.Create, true);
            using (destination)
            {
                storage.Copy(destination);
                destination.Commit();
            }
        }

        /// <summary>
        /// Copy a stream into a file
        /// </summary>
        /// <param name="stream">stream</param>
        /// <param name="destFileName">file to copy stream into</param>
        private void CopyStreamToFile(Stream stream, string destFileName)
        {
            var destination = new FileStream(destFileName, FileMode.Create, FileAccess.ReadWrite);
            using (destination)
            {
                var bytesRead = 0;
                var ioBuffer = new byte[4096];
                while ((bytesRead = stream.Read(ioBuffer, 0, ioBuffer.Length)) != 0)
                {
                    destination.Write(ioBuffer, 0, bytesRead);
                }
            }
        }

        /// <summary>
        /// Copy a file into another file
        /// </summary>
        /// <param name="srcFileName">source file</param>
        /// <param name="destFileName">destination file</param>
        private void CopyFileToFile(string srcFileName, string destFileName) => File.Copy(srcFileName, destFileName);

        /// <summary>
        /// Copy a global memory block into a file
        /// </summary>
        /// <param name="hGlobal">global memory block</param>
        /// <param name="destFileName">file to copy global memory into</param>
        private void CopyHGLOBALToFile(IntPtr hGlobal, string destFileName)
        {
            var globalMem = new HGlobalLock(hGlobal);
            using (globalMem)
            {
                var destination = new FileStream(destFileName, FileMode.Create, FileAccess.ReadWrite);
                using (destination)
                {
                    // use Win32 WriteFile so we can blast the entire unmanaged memory
                    // block in a single call (if we wanted to use managed file io
                    // methods we would have to copy the entire memory block into
                    // unmanaged memory first)
                    var success = Kernel32.WriteFile(
                        destination.SafeFileHandle, globalMem.Memory, globalMem.Size.ToUInt32(),
                        out var bytesWritten, IntPtr.Zero);
                    if (!success)
                    {
                        throw new Win32Exception(
                            Marshal.GetLastWin32Error(),
                            $"Error occured attempting to write file: {destination.Name}");
                    }
                }
            }
        }

        /// <summary>
        /// Structure that contains meta-data about the file contents
        /// </summary>
        private FileDescriptor fileDescriptor;

        /// <summary>
        /// Underlying OleDataObject that the FileContents will be fetched from
        /// </summary>
        private readonly OleDataObject oleDataObject;

        /// <summary>
        /// Index of file to be fetched
        /// </summary>
        private readonly int fileIndex;
    }
}
