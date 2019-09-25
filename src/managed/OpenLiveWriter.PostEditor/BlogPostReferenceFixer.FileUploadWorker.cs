// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    internal partial class BlogPostReferenceFixer
    {
        /// <summary>
        /// The FileUploadWorker class.
        /// </summary>
        private partial class FileUploadWorker
        {
            /// <summary>
            /// The file list
            /// </summary>
            private readonly List<ISupportingFile> fileList = new List<ISupportingFile>();

            /// <summary>
            /// The post identifier
            /// </summary>
            private readonly string postId;

            /// <summary>
            /// The uploaded files
            /// </summary>
            private readonly Hashtable uploadedFiles = new Hashtable();

            /// <summary>
            /// Initializes a new instance of the <see cref="FileUploadWorker"/> class.
            /// </summary>
            /// <param name="postId">The post identifier.</param>
            public FileUploadWorker(string postId) => this.postId = postId;

            /// <summary>
            /// Adds the file.
            /// </summary>
            /// <param name="supportingFile">The supporting file.</param>
            public void AddFile(ISupportingFile supportingFile) => this.fileList.Add(supportingFile);

            /// <summary>
            /// Does the upload work.
            /// </summary>
            /// <param name="fileReference">The file reference.</param>
            /// <param name="fileUploader">The file uploader.</param>
            /// <param name="isLightboxCloneEnabled">if set to <c>true</c> [is lightbox clone enabled].</param>
            public void DoUploadWork(string fileReference, BlogFileUploader fileUploader, bool isLightboxCloneEnabled)
            {
                // Get both strings into the same state which is unescaped
                var unescapedFileReference = new Uri(fileReference).ToString();
                var file = this.fileList.FirstOrDefault(supportingFile => supportingFile.FileUri.ToString() == unescapedFileReference);

                if (file == null)
                {
                    var listString = this.fileList.Aggregate(string.Empty, (current, supportingFile) => $"{current}{supportingFile.FileUri}\r\n");

                    Trace.Fail(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Reference found to file that does not exist in SupportingFileService \r\nfileReference: {0}\r\n_fileList:\r\n{1}",
                            fileReference,
                            listString));
                    return;
                }

                var uploadContext = fileUploader.DestinationContext;

                var uploadInfo = file.GetUploadInfo(uploadContext);
                var fileUploadContext =
                    new FileUploadContext(fileUploader, this.postId, file, uploadInfo, isLightboxCloneEnabled);

                if (!fileUploader.DoesFileNeedUpload(file, fileUploadContext))
                {
                    return;
                }

                if (this.uploadedFiles.ContainsKey(file.FileId))
                {
                    Trace.Fail(
                        $"This file has already been uploaded during this publish operation: {file.FileName}");
                }
                else
                {
                    this.uploadedFiles[file.FileId] = file;
                    var uploadUri = fileUploader.DoUploadWorkBeforePublish(fileUploadContext);
                    if (uploadUri == null)
                    {
                        return;
                    }

                    file.MarkUploaded(uploadContext, uploadUri);
                    Debug.WriteLine(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "File Uploaded: {0}",
                            file.FileName));
                }
            }

            /// <summary>
            /// Does the after post upload work.
            /// </summary>
            /// <param name="fileUploader">The file uploader.</param>
            /// <param name="postIdentifier">The post identifier.</param>
            public void DoAfterPostUploadWork(BlogFileUploader fileUploader, string postIdentifier)
            {
                foreach (var fileUploadContext in this.fileList
                                                      .Select(
                                                           file => new
                                                           {
                                                               file,
                                                               uploadContext = fileUploader.DestinationContext
                                                           })
                                                      .Where(arg => this.uploadedFiles.ContainsKey(arg.file.FileId))
                                                      .Select(
                                                           arg => new
                                                           {
                                                               item = arg,
                                                               uploadInfo = arg.file.GetUploadInfo(arg.uploadContext)
                                                           })
                                                      .Select(
                                                           arg => new FileUploadContext(
                                                             fileUploader,
                                                             postIdentifier,
                                                             arg.item.file,
                                                             arg.uploadInfo,
                                                             false)))
                {
                    fileUploader.DoUploadWorkAfterPublish(fileUploadContext);
                }
            }
        }
    }
}
