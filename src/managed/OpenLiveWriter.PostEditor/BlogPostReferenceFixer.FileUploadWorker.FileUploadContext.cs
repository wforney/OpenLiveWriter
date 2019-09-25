// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.IO;
    using Api;
    using Extensibility.BlogClient;

    internal partial class BlogPostReferenceFixer
    {

        private partial class FileUploadWorker
        {
            /// <summary>
            /// The FileUploadContext class.
            /// Implements the <see cref="OpenLiveWriter.Extensibility.BlogClient.IFileUploadContext" />
            /// </summary>
            /// <seealso cref="OpenLiveWriter.Extensibility.BlogClient.IFileUploadContext" />
            private class FileUploadContext : IFileUploadContext
            {
                /// <summary>
                /// The file uploader
                /// </summary>
                private readonly BlogFileUploader fileUploader;

                /// <summary>
                /// The supporting file
                /// </summary>
                private readonly ISupportingFile supportingFile;

                /// <summary>
                /// The upload information
                /// </summary>
                private readonly ISupportingFileUploadInfo uploadInfo;

                /// <summary>
                /// Initializes a new instance of the <see cref="FileUploadContext"/> class.
                /// </summary>
                /// <param name="fileUploader">The file uploader.</param>
                /// <param name="postId">The post identifier.</param>
                /// <param name="supportingFile">The supporting file.</param>
                /// <param name="uploadInfo">The upload information.</param>
                /// <param name="forceDirectImageLink">if set to <c>true</c> [force direct image link].</param>
                public FileUploadContext(BlogFileUploader fileUploader, string postId, ISupportingFile supportingFile,
                                         ISupportingFileUploadInfo uploadInfo, bool forceDirectImageLink)
                {
                    this.fileUploader = fileUploader;
                    this.BlogId = fileUploader.BlogId;
                    this.PostId = postId;
                    this.supportingFile = supportingFile;
                    this.uploadInfo = uploadInfo;
                    this.ForceDirectImageLink = forceDirectImageLink;
                }

                /// <summary>
                /// Gets the blog identifier.
                /// </summary>
                /// <value>The blog identifier.</value>
                public string BlogId { get; }

                /// <summary>
                /// Gets the post identifier.
                /// </summary>
                /// <value>The post identifier.</value>
                public string PostId { get; }

                /// <summary>
                /// Gets the name of the preferred file.
                /// </summary>
                /// <value>The name of the preferred file.</value>
                public string PreferredFileName => this.supportingFile.FileName;

                /// <summary>
                /// Gets the role.
                /// </summary>
                /// <value>The role.</value>
                public FileUploadRole Role
                {
                    get
                    {
                        var data = new ImageFileData(this.supportingFile);
                        switch (data.Relationship)
                        {
                            case ImageFileRelationship.Inline:
                                return FileUploadRole.InlineImage;
                            case ImageFileRelationship.Linked:
                                return FileUploadRole.LinkedImage;
                            default:
                                return FileUploadRole.File;
                        }
                    }
                }

                /// <summary>
                /// Gets the contents.
                /// </summary>
                /// <returns>Stream.</returns>
                public Stream GetContents() =>
                    new FileStream(this.supportingFile.FileUri.LocalPath, FileMode.Open, FileAccess.Read);

                /// <summary>
                /// Gets the contents local file path.
                /// </summary>
                /// <returns>System.String.</returns>
                public string GetContentsLocalFilePath() => this.supportingFile.FileUri.LocalPath;

                /// <summary>
                /// Formats the name of the file.
                /// </summary>
                /// <param name="filename">The filename.</param>
                /// <returns>System.String.</returns>
                public string FormatFileName(string filename)
                {
                    var conflictToken = filename == this.supportingFile.FileName
                                            ? this.supportingFile.FileNameUniqueToken
                                            : Guid.NewGuid().ToString();

                    if (conflictToken == "0") //don't include the conflict token for the first version of the file.
                    {
                        conflictToken = null;
                    }

                    return this.fileUploader.FormatUploadFileName(filename, conflictToken);
                }

                /// <summary>
                /// Gets the settings.
                /// </summary>
                /// <value>The settings.</value>
                public IProperties Settings => this.uploadInfo.UploadSettings;

                /// <summary>
                /// Gets a value indicating whether [force direct image link].
                /// </summary>
                /// <value><c>true</c> if [force direct image link]; otherwise, <c>false</c>.</value>
                public bool ForceDirectImageLink { get; }
            }
        }
    }
}
