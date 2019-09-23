// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Xml;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The AtomMediaUploader class.
    /// </summary>
    public class AtomMediaUploader
    {
        /// <summary>
        /// The edit media entry link
        /// </summary>
        protected const string EditMediaEntryLink = "EditMediaLinkEntryLink";

        /// <summary>
        /// The edit media link
        /// </summary>
        protected const string EditMediaLink = "EditMediaLink";

        /// <summary>
        /// The media etag
        /// </summary>
        protected const string MediaEtag = "MediaEtag";

        /// <summary>
        /// The collection URI
        /// </summary>
        protected readonly string CollectionUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomMediaUploader"/> class.
        /// </summary>
        /// <param name="namespaceManager">The ns MGR.</param>
        /// <param name="requestFilter">The request filter.</param>
        /// <param name="collectionUri">The collection URI.</param>
        /// <param name="options">The options.</param>
        public AtomMediaUploader(
            XmlNamespaceManager namespaceManager,
            HttpRequestFilter requestFilter,
            string collectionUri,
            IBlogClientOptions options)
            : this(namespaceManager, requestFilter, collectionUri, options, new XmlRestRequestHelper())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomMediaUploader"/> class.
        /// </summary>
        /// <param name="namespaceManager">The ns MGR.</param>
        /// <param name="requestFilter">The request filter.</param>
        /// <param name="collectionUri">The collection URI.</param>
        /// <param name="options">The options.</param>
        /// <param name="xmlRestRequestHelper">The XML rest request helper.</param>
        public AtomMediaUploader(
            XmlNamespaceManager namespaceManager,
            HttpRequestFilter requestFilter,
            string collectionUri,
            IBlogClientOptions options,
            XmlRestRequestHelper xmlRestRequestHelper)
        {
            this.NsMgr = namespaceManager;
            this.RequestFilter = requestFilter;
            this.CollectionUri = collectionUri;
            this.Options = options;
            this.XmlRestRequestHelper = xmlRestRequestHelper;
        }

        /// <summary>
        /// Gets the ns MGR.
        /// </summary>
        /// <value>The ns MGR.</value>
        protected XmlNamespaceManager NsMgr { get; }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        protected IBlogClientOptions Options { get; }

        /// <summary>
        /// Gets the request filter.
        /// </summary>
        /// <value>The request filter.</value>
        protected HttpRequestFilter RequestFilter { get; }

        /// <summary>
        /// Gets the XML rest request helper.
        /// </summary>
        /// <value>The XML rest request helper.</value>
        protected XmlRestRequestHelper XmlRestRequestHelper { get; }

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result.</returns>
        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            var path = uploadContext.GetContentsLocalFilePath();
            string srcUrl;
            var editUri = uploadContext.Settings.GetString(AtomMediaUploader.EditMediaLink, null);
            var editEntryUri = uploadContext.Settings.GetString(AtomMediaUploader.EditMediaEntryLink, null);
            var etag = uploadContext.Settings.GetString(AtomMediaUploader.MediaEtag, null);
            if (string.IsNullOrEmpty(editUri))
            {
                this.PostNewImage(path, false, out srcUrl, out editUri, out editEntryUri);
            }
            else
            {
                try
                {
                    this.UpdateImage(ref editUri, path, editEntryUri, etag, true, out srcUrl);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());

                    var success = false;
                    srcUrl = null; // compiler complains without this line
                    try
                    {
                        // couldn't update existing image? try posting a new one
                        this.PostNewImage(path, false, out srcUrl, out editUri, out editEntryUri);
                        success = true;

                        if (e is WebException exception)
                        {
                            Trace.WriteLine("Image PUT failed, but POST succeeded. PUT exception follows.");
                            HttpRequestHelper.LogException(exception);
                        }
                    }
                    catch
                    {
                        // ignored
                    }

                    if (!success)
                    {
                        throw; // rethrow the exception from the update, not the post
                    }
                }
            }

            uploadContext.Settings.SetString(AtomMediaUploader.EditMediaLink, editUri);
            uploadContext.Settings.SetString(AtomMediaUploader.EditMediaEntryLink, editEntryUri);
            uploadContext.Settings.SetString(AtomMediaUploader.MediaEtag, null);

            this.UpdateETag(uploadContext, editUri);
            return srcUrl;
        }

        /// <summary>
        /// Posts the new image.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="allowWriteStreamBuffering">if set to <c>true</c> [allow write stream buffering].</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="editMediaUri">The edit media URI.</param>
        /// <param name="editEntryUri">The edit entry URI.</param>
        /// <exception cref="BlogClientFileUploadNotSupportedException"></exception>
        public virtual void PostNewImage(
            string path,
            bool allowWriteStreamBuffering,
            out string srcUrl,
            out string editMediaUri,
            out string editEntryUri)
        {
            var mediaCollectionUri = this.CollectionUri;
            if (string.IsNullOrEmpty(mediaCollectionUri))
            {
                throw new BlogClientFileUploadNotSupportedException();
            }

            HttpWebResponse response = null;
            try
            {
                response = RedirectHelper.GetResponse(
                    mediaCollectionUri,
                    new ImageUploadHelper(this, path, "POST", null, allowWriteStreamBuffering).Create);

                var xmlDoc = this.GetCreatedEntity(response, out var entryUri, out var etag);
                this.ParseResponse(xmlDoc, out srcUrl, out editMediaUri, out editEntryUri, out var selfPage);
            }
            catch (WebException we)
            {
                // The error may have been due to the server requiring stream buffering (WinLive 114314, 252175)
                // Try again with stream buffering.
                if (we.Status == WebExceptionStatus.ProtocolError && !allowWriteStreamBuffering)
                {
                    this.PostNewImage(path, true, out srcUrl, out editMediaUri, out editEntryUri);
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                response?.Close();
            }
        }

        /// <summary>
        /// Parses the response.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="editUri">The edit URI.</param>
        /// <param name="editEntryUri">The edit entry URI.</param>
        /// <param name="selfPage">The self page.</param>
        /// <param name="thumbnailSmall">The thumbnail small.</param>
        /// <param name="thumbnailLarge">The thumbnail large.</param>
        protected virtual void ParseResponse(
            XmlDocument xmlDoc,
            out string srcUrl,
            out string editUri,
            out string editEntryUri,
            out string selfPage,
            out string thumbnailSmall,
            out string thumbnailLarge)
        {
            thumbnailSmall = null;
            thumbnailLarge = null;
            this.ParseResponse(xmlDoc, out srcUrl, out editUri, out editEntryUri, out selfPage);
        }

        /// <summary>
        /// Parses the response.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="editUri">The edit URI.</param>
        /// <param name="editEntryUri">The edit entry URI.</param>
        /// <param name="selfPage">The self page.</param>
        protected virtual void ParseResponse(
            XmlDocument xmlDoc,
            out string srcUrl,
            out string editUri,
            out string editEntryUri,
            out string selfPage)
        {
            var contentEl = xmlDoc.SelectSingleNode("/atom:entry/atom:content", this.NsMgr) as XmlElement;
            srcUrl = XmlHelper.GetUrl(contentEl, "@src", null);
            editUri = AtomEntry.GetLink(
                xmlDoc.SelectSingleNode("/atom:entry", this.NsMgr) as XmlElement,
                this.NsMgr,
                "edit-media",
                null,
                null,
                null);
            editEntryUri = AtomEntry.GetLink(
                xmlDoc.SelectSingleNode("/atom:entry", this.NsMgr) as XmlElement,
                this.NsMgr,
                "edit",
                null,
                null,
                null);
            selfPage = AtomEntry.GetLink(
                xmlDoc.SelectSingleNode("/atom:entry", this.NsMgr) as XmlElement,
                this.NsMgr,
                "alternate",
                null,
                null,
                null);
        }

        /// <summary>
        /// Updates the e tag.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <param name="editUri">The edit URI.</param>
        protected virtual void UpdateETag(IFileUploadContext uploadContext, string editUri)
        {
            try
            {
                var newEtag = AtomClient.GetEtag(editUri, this.RequestFilter);
                uploadContext.Settings.SetString(AtomMediaUploader.MediaEtag, newEtag);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Updates the image.
        /// </summary>
        /// <param name="editMediaUri">The edit media URI.</param>
        /// <param name="path">The path.</param>
        /// <param name="editEntryUri">The edit entry URI.</param>
        /// <param name="etag">The etag.</param>
        /// <param name="getEditInfo">if set to <c>true</c> [get edit information].</param>
        /// <param name="srcUrl">The source URL.</param>
        protected virtual void UpdateImage(
            ref string editMediaUri,
            string path,
            string editEntryUri,
            string etag,
            bool getEditInfo,
            out string srcUrl) => this.UpdateImage(
                false,
                ref editMediaUri,
                path,
                editEntryUri,
                etag,
                getEditInfo,
                out srcUrl,
                out var thumbnailSmall,
                out var thumbnailLarge);

        /// <summary>
        /// Updates the image.
        /// </summary>
        /// <param name="allowWriteStreamBuffering">if set to <c>true</c> [allow write stream buffering].</param>
        /// <param name="editMediaUri">The edit media URI.</param>
        /// <param name="path">The path.</param>
        /// <param name="editEntryUri">The edit entry URI.</param>
        /// <param name="etag">The etag.</param>
        /// <param name="getEditInfo">if set to <c>true</c> [get edit information].</param>
        /// <param name="srcUrl">The source URL.</param>
        protected virtual void UpdateImage(
            bool allowWriteStreamBuffering,
            ref string editMediaUri,
            string path,
            string editEntryUri,
            string etag,
            bool getEditInfo,
            out string srcUrl) => this.UpdateImage(
                allowWriteStreamBuffering,
                ref editMediaUri,
                path,
                editEntryUri,
                etag,
                getEditInfo,
                out srcUrl,
                out var thumbnailSmall,
                out var thumbnailLarge);

        /// <summary>
        /// Updates the image.
        /// </summary>
        /// <param name="allowWriteStreamBuffering">if set to <c>true</c> [allow write stream buffering].</param>
        /// <param name="editMediaUri">The edit media URI.</param>
        /// <param name="path">The path.</param>
        /// <param name="editEntryUri">The edit entry URI.</param>
        /// <param name="etag">The etag.</param>
        /// <param name="getEditInfo">if set to <c>true</c> [get edit information].</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="thumbnailSmall">The thumbnail small.</param>
        /// <param name="thumbnailLarge">The thumbnail large.</param>
        /// <exception cref="BlogClientOperationCancelledException"></exception>
        protected virtual void UpdateImage(
            bool allowWriteStreamBuffering,
            ref string editMediaUri,
            string path,
            string editEntryUri,
            string etag,
            bool getEditInfo,
            out string srcUrl,
            out string thumbnailSmall,
            out string thumbnailLarge)
        {
            HttpWebResponse response = null;
            try
            {
                response = RedirectHelper.GetResponse(
                    editMediaUri,
                    new ImageUploadHelper(this, path, "PUT", etag, allowWriteStreamBuffering).Create);
                response.Close();
            }
            catch (WebException we)
            {
                var recovered = false;

                if (we.Status == WebExceptionStatus.ProtocolError && we.Response != null)
                {
                    if (we.Response is HttpWebResponse errResponse && errResponse.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        var newEtag = AtomClient.GetEtag(editMediaUri, this.RequestFilter);
                        if (!string.IsNullOrEmpty(newEtag) && newEtag != etag)
                        {
                            if (!AtomClient.ConfirmOverwrite())
                            {
                                throw new BlogClientOperationCancelledException();
                            }

                            try
                            {
                                response = RedirectHelper.GetResponse(
                                    editMediaUri,
                                    new ImageUploadHelper(
                                        this,
                                        path,
                                        "PUT",
                                        newEtag,
                                        allowWriteStreamBuffering).Create);
                            }
                            finally
                            {
                                response?.Close();
                            }

                            recovered = true;
                        }
                    }
                    else if (!allowWriteStreamBuffering)
                    {
                        // The error may have been due to the server requiring stream buffering (WinLive 114314, 252175)
                        // Try again with stream buffering.
                        this.UpdateImage(
                            true,
                            ref editMediaUri,
                            path,
                            editEntryUri,
                            etag,
                            getEditInfo,
                            out srcUrl,
                            out thumbnailSmall,
                            out thumbnailLarge);
                        recovered = true;
                    }
                }

                if (!recovered)
                {
                    throw;
                }
            }

            // Check to see if we are going to get the src url and the etag, in most cases we will want to get this
            // information, but in the case of a photo album, since we never edit the image or link directly to them
            // we don't need the information and it can saves an http request.
            if (getEditInfo)
            {
                var uri = new Uri(editEntryUri);
                var mediaLinkEntry = this.XmlRestRequestHelper.Get(ref uri, this.RequestFilter);
                this.ParseResponse(
                    mediaLinkEntry,
                    out srcUrl,
                    out editMediaUri,
                    out editEntryUri,
                    out var selfPage,
                    out thumbnailSmall,
                    out thumbnailLarge);
            }
            else
            {
                thumbnailSmall = null;
                thumbnailLarge = null;
                srcUrl = null;
            }
        }

        /// <summary>
        /// Gets the created entity.
        /// </summary>
        /// <param name="postResponse">The post response.</param>
        /// <param name="editUri">The edit URI.</param>
        /// <param name="etag">The etag.</param>
        /// <returns>An <see cref="XmlDocument"/>.</returns>
        private XmlDocument GetCreatedEntity(WebResponse postResponse, out string editUri, out string etag)
        {
            editUri = postResponse.Headers["Location"];
            var contentLocation = postResponse.Headers["Content-Location"];
            if (string.IsNullOrEmpty(editUri) || editUri != contentLocation)
            {
                var uri = postResponse.ResponseUri;
                if (!string.IsNullOrEmpty(editUri))
                {
                    uri = new Uri(editUri);
                }

                var doc = this.XmlRestRequestHelper.Get(ref uri, this.RequestFilter, out var responseHeaders);
                etag = responseHeaders["ETag"];
                return doc;
            }

            etag = postResponse.Headers["ETag"];
            var xmlDoc = new XmlDocument();
            using (var s = postResponse.GetResponseStream())
            {
                xmlDoc.Load(s ?? throw new InvalidOperationException());
            }

            XmlHelper.ApplyBaseUri(xmlDoc, postResponse.ResponseUri);
            return xmlDoc;
        }

        /// <summary>
        /// The ImageUploadHelper class.
        /// </summary>
        protected class ImageUploadHelper
        {
            /// <summary>
            /// The allow write stream buffering
            /// </summary>
            private readonly bool allowWriteStreamBuffering;

            /// <summary>
            /// The e-tag
            /// </summary>
            private readonly string etag;

            /// <summary>
            /// The filename
            /// </summary>
            private readonly string filename;

            /// <summary>
            /// The method
            /// </summary>
            private readonly string method;

            /// <summary>
            /// The parent
            /// </summary>
            private readonly AtomMediaUploader parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="ImageUploadHelper"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="filename">The filename.</param>
            /// <param name="method">The method.</param>
            /// <param name="etag">The etag.</param>
            /// <param name="allowWriteStreamBuffering">if set to <c>true</c> [allow write stream buffering].</param>
            public ImageUploadHelper(
                AtomMediaUploader parent,
                string filename,
                string method,
                string etag,
                bool allowWriteStreamBuffering)
            {
                this.parent = parent;
                this.filename = filename;
                this.method = method;
                this.etag = etag;
                this.allowWriteStreamBuffering = allowWriteStreamBuffering;
            }

            /// <summary>
            /// Creates the specified URI.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <returns>The <see cref="HttpWebRequest"/>.</returns>
            public HttpWebRequest Create(string uri)
            {
                // TODO: ETag support required??
                // TODO: choose rational timeout values
                var request = HttpRequestHelper.CreateHttpWebRequest(uri, false);

                request.ContentType = MimeHelper.GetContentType(Path.GetExtension(this.filename));
                if (this.parent.Options != null && this.parent.Options.SupportsSlug)
                {
                    request.Headers.Add("Slug", Path.GetFileNameWithoutExtension(this.filename));
                }

                request.Method = this.method;

                request.AllowWriteStreamBuffering = this.allowWriteStreamBuffering;

                if (!string.IsNullOrEmpty(this.etag))
                {
                    request.Headers.Add("If-match", this.etag);
                }

                this.parent.RequestFilter(request);

                using (Stream inS = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var cs = new CancelableStream(inS))
                    {
                        request.ContentLength = cs.Length;
                        using (var s = request.GetRequestStream())
                        {
                            StreamHelper.Transfer(cs, s);
                        }
                    }
                }

                return request;
            }
        }
    }
}
