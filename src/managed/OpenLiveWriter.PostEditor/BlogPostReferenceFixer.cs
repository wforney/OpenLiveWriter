namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Globalization;
    using CoreServices;
    using CoreServices.HTML;
    using HtmlParser.Parser;
    using SupportingFiles;

    /// <summary>
    /// The BlogPostReferenceFixer class.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.LightWeightHTMLDocumentIterator" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.LightWeightHTMLDocumentIterator" />
    internal partial class BlogPostReferenceFixer : LightWeightHTMLDocumentIterator
    {
        /// <summary>
        /// The file reference list
        /// </summary>
        private readonly SupportingFileReferenceList fileReferenceList;

        /// <summary>
        /// The file upload worker
        /// </summary>
        private readonly FileUploadWorker fileUploadWorker;

        /// <summary>
        /// The upload context
        /// </summary>
        private readonly IBlogPostPublishingContext uploadContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogPostReferenceFixer"/> class.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <param name="publishingContext">The publishing context.</param>
        internal BlogPostReferenceFixer(string html, IBlogPostPublishingContext publishingContext)
            : base(html)
        {
            this.uploadContext = publishingContext;
            this.fileUploadWorker = new FileUploadWorker(this.uploadContext.BlogPost.Id);
            this.fileReferenceList =
                SupportingFileReferenceList.CalculateReferencesForPublish(publishingContext.EditingContext);
        }

        /// <summary>
        /// Uploads the files after publish.
        /// </summary>
        /// <param name="postId">The post identifier.</param>
        /// <param name="fileUploader">The file uploader.</param>
        public void UploadFilesAfterPublish(string postId, BlogFileUploader fileUploader) =>
            this.fileUploadWorker.DoAfterPostUploadWork(fileUploader, postId);

        /// <summary>
        /// Gets the file upload reference fixer.
        /// </summary>
        /// <param name="uploader">The uploader.</param>
        /// <returns>ReferenceFixer.</returns>
        public ReferenceFixer GetFileUploadReferenceFixer(BlogFileUploader uploader) =>
            new LocalFileTransformer(this, uploader).Transform;

        /// <summary>
        /// Called when [begin tag].
        /// </summary>
        /// <param name="tag">The tag.</param>
        protected override void OnBeginTag(BeginTag tag)
        {
            if (tag != null &&
                LightWeightHTMLDocument.AllUrlElements.ContainsKey(tag.Name.ToUpper(CultureInfo.InvariantCulture)))
            {
                var attr = tag.GetAttribute(
                    (string)LightWeightHTMLDocument.AllUrlElements[tag.Name.ToUpper(CultureInfo.InvariantCulture)]);
                if (attr?.Value != null)
                {
                    if (UrlHelper.IsUrl(attr.Value))
                    {
                        var reference = new Uri(attr.Value);
                        if (this.fileReferenceList.IsReferenced(reference))
                        {
                            var supportingFile = this.fileReferenceList.GetSupportingFileByUri(reference);
                            if (supportingFile.Embedded)
                            {
                                this.AddFileReference(supportingFile);
                            }
                        }
                    }
                }
            }

            base.OnBeginTag(tag);
        }

        /// <summary>
        /// Adds the file reference.
        /// </summary>
        /// <param name="supportingFile">The supporting file.</param>
        private void AddFileReference(ISupportingFile supportingFile) => this.fileUploadWorker.AddFile(supportingFile);
    }
}
