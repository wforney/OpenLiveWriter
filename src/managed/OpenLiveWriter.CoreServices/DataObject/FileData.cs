// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Data transfer for a group of files
    /// </summary>
    public class FileData
    {
        /// <summary>
        /// Creates a new FileData
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the new FileData</param>
        /// <returns>The FileData, null if none could be created</returns>
        public static FileData Create(IDataObject iDataObject)
        {
            foreach (var format in GetFormatsForDataObject(iDataObject)
                .Cast<IFileItemFormat>()
                // determine which file item format to use depending upon what the contents of the data object are
                .Where(format => format.CanCreateFrom(iDataObject)))
            {
                return new FileData(iDataObject, format);
            }

            return null;
        }

        /// <summary>
        /// The FileItems contained in the data object.
        /// </summary>
        public ICollection<FileItem> Files
        {
            get
            {
                // create on demand and cache
                if (this.files == null)
                {
                    this.files = this.fileItemFormat.CreateFileItems(this.dataObject);
                }

                return this.files;
            }
        }

        /// <summary>
        /// FileData Constructor
        /// </summary>
        /// <param name="iDataObject">The IDataObject from which to create the FileData</param>
        private FileData(IDataObject iDataObject, IFileItemFormat format)
        {
            // save a reference to the data object
            this.dataObject = iDataObject;
            this.fileItemFormat = format;

            // debug-only sanity check on formats
            VerifyFormatsMutuallyExclusive(iDataObject);
        }

        /// <summary>
        /// Initialize list of FileItem formats that we support
        /// </summary>
        private static readonly ArrayList fileItemFormats =
            ArrayList.Synchronized(new ArrayList(new IFileItemFormat[]
                {
                    new FileItemFileItemFormat(),
                    new FileItemFileDropFormat(),
                    new FileItemFileContentsFormat()
                }));

        /// <summary>
        /// Initialize list of FileItem formats that we support
        /// </summary>
        private static readonly ArrayList mozillaFileItemFormats =
            ArrayList.Synchronized(new ArrayList(new IFileItemFormat[]
                {
                    new FileItemFileContentsFormat(),
                    new FileItemFileItemFormat(),
                    new FileItemFileDropFormat()
                }));

        /// <summary>
        /// Returns a list of FileItemFormats for a given dataobject
        /// </summary>
        private static ArrayList GetFormatsForDataObject(IDataObject dataObject) =>
            // Mozilla often time is very sketchy about their filedrops.  They
            // may place null in the filedrop format, or they may place paths
            // to files that are highly transient and may or may not be available
            // when you need then (many images will get reset to 0 byte files)
            // As a result of this, we prefer FileContents when using Mozilla
            OleDataObjectHelper.GetDataPresentSafe(dataObject, MOZILLA_MARKER) ? mozillaFileItemFormats : fileItemFormats;

        private const string MOZILLA_MARKER = "text/_moz_htmlinfo";

        // underlying data object
        private readonly IDataObject dataObject = null;

        // file item format
        private readonly IFileItemFormat fileItemFormat = null;

        // list of files
        private ICollection<FileItem> files = null;

        // Helper to verify that our IFileItemFormat instances are mutually
        // exclusive (more than one format shouldn't be supported by the
        // data inside a single IDataObject)
        [Conditional("DEBUG")]
        private static void VerifyFormatsMutuallyExclusive(IDataObject iDataObject)
        {
            // verify there is no case where multiple file items types can
            // be created from the same IDataObject (just a sanity/assumption check)
            var matches = 0;
            var sb = new StringBuilder();
            foreach (IFileItemFormat format in fileItemFormats)
            {
                if (format.CanCreateFrom(iDataObject))
                {
                    matches++;
                    sb.AppendLine(format.ToString());
                }
            }

            Debug.Assert(
                matches == 1,
                $"More than 1 file item format can be created from a data object (potential ambiguity / order dependency\r\n{sb.ToString()}");
        }
    }
}
