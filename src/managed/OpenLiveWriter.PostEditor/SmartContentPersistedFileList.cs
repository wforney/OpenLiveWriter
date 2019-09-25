// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using Api;
    using CoreServices;

    /// <summary>
    /// This class helps store an ordered list of files in a SmartContent object.
    /// The file contents can optionally be stored in the SmartContent.
    /// </summary>
    internal class SmartContentPersistedFileList
    {
        /// <summary>
        /// The internal external separator
        /// </summary>
        private const char InternalExternalSeparator = '|';

        /// <summary>
        /// The file separator
        /// </summary>
        private const string FileSeparator = "?";

        /// <summary>
        /// The list identifier
        /// </summary>
        private readonly string listId;

        /// <summary>
        /// The smart content
        /// </summary>
        private readonly ISmartContent smartContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmartContentPersistedFileList"/> class.
        /// </summary>
        /// <param name="smartContent">Content of the smart.</param>
        /// <param name="listId">The list identifier.</param>
        public SmartContentPersistedFileList(ISmartContent smartContent, string listId)
        {
            this.smartContent = smartContent;
            this.listId = listId;
        }

        /// <summary>
        /// Gets the files.
        /// </summary>
        /// <value>The files.</value>
        public List<string> Files
        {
            get
            {
                var tempDir = new LazyLoader<string>(
                    () => TempFileManager.Instance.CreateTempDir());

                var results = new List<string>();

                foreach (var pf in this.GetPersistedFileList())
                {
                    // If the file was not persisted in the wpost, or the
                    // file still exists at the original path, use that.

                    if (File.Exists(pf.Path))
                    {
                        results.Add(pf.Path);
                    }
                    else if (pf.PersistToPostFile)
                    {
                        using (var inStream = this.smartContent.Files.Open(pf.SmartContentName, false))
                        {
                            Trace.WriteLineIf(inStream == null,
                                              string.Format(CultureInfo.InvariantCulture,
                                                            "Failed to find smartcontent persisted file, {0}",
                                                            pf.SmartContentName));
                            if (inStream == null)
                            {
                                continue;
                            }

                            var filePath = TempFileManager.CreateNewFile(tempDir, pf.SmartContentName, false);
                            using (Stream outStream = File.OpenWrite(filePath))
                            {
                                StreamHelper.Transfer(inStream, outStream);
                            }

                            results.Add(filePath);
                        }
                    }
                }

                return results;
            }
        }

        /// <summary>
        /// Clears the files.
        /// </summary>
        public void ClearFiles()
        {
            foreach (var pf in this.GetPersistedFileList())
            {
                if (pf.PersistToPostFile)
                {
                    this.smartContent.Files.Remove(pf.SmartContentName);
                }
            }

            this.smartContent.Properties.Remove(this.listId);
        }

        /// <summary>
        /// Sets the files.
        /// </summary>
        /// <param name="files">The files.</param>
        public void SetFiles(IEnumerable<PersistedFile> files)
        {
            var result = new StringBuilder();

            this.ClearFiles();
            var i = 0;
            foreach (var file in files)
            {
                if (file.PersistToPostFile)
                {
                    var name = (i++).ToString(CultureInfo.InvariantCulture) + Path.GetExtension(file.Path);
                    this.smartContent.Files.Add(name, file.Path);
                    result.AppendFormat(
                        CultureInfo.InvariantCulture,
                        $"I{SmartContentPersistedFileList.InternalExternalSeparator}{{0}}{SmartContentPersistedFileList.InternalExternalSeparator}{{1}}{SmartContentPersistedFileList.FileSeparator}", name, file.Path);
                }
                else
                {
                    result.Append(
                        $"E{SmartContentPersistedFileList.InternalExternalSeparator}{file.Path}{SmartContentPersistedFileList.FileSeparator}");
                }
            }

            this.smartContent.Properties.SetString(this.listId, result.ToString());
        }

        /// <summary>
        /// Gets the persisted file list.
        /// </summary>
        /// <returns>The persisted file list.</returns>
        private IEnumerable<PersistedFile> GetPersistedFileList()
        {
            var str = this.smartContent.Properties.GetString(this.listId, null);
            if (string.IsNullOrEmpty(str))
            {
                return new List<PersistedFile>();
            }

            var results = new List<PersistedFile>();

            var files = StringHelper.Split(str, SmartContentPersistedFileList.FileSeparator);
            foreach (var file in files)
            {
                var chunks = file.Split(SmartContentPersistedFileList.InternalExternalSeparator);
                switch (chunks[0])
                {
                    case "I":
                        var persistedFile = new PersistedFile(true, chunks[2])
                        {
                            SmartContentName = chunks[1]
                        };
                        results.Add(persistedFile);
                        break;
                    case "E":
                        results.Add(new PersistedFile(false, chunks[1]));
                        break;
                }
            }

            return results;
        }
    }
}
