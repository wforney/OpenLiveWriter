// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Xml;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// The MultipartMimeRequestHelper class.
    /// </summary>
    public class MultipartMimeRequestHelper
    {
        /// <summary>
        /// The request body bottom
        /// </summary>
        protected MemoryStream RequestBodyBottom = new MemoryStream();

        /// <summary>
        /// The request body top
        /// </summary>
        protected MemoryStream RequestBodyTop = new MemoryStream();

        /// <summary>
        /// The UTF8 no bom encoding
        /// </summary>
        protected UTF8Encoding Utf8NoBomEncoding = new UTF8Encoding(false);

        /// <summary>
        /// The boundary
        /// </summary>
        private string boundary;

        /// <summary>
        /// The request
        /// </summary>
        private HttpWebRequest request;

        /// <summary>
        /// The request stream
        /// </summary>
        private Stream requestStream;

        /// <summary>
        /// Adds the boundary.
        /// </summary>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        /// <param name="stream">The stream.</param>
        public virtual void AddBoundary(bool newLine, MemoryStream stream) =>
            this.Write("--" + this.boundary + (newLine ? Environment.NewLine : string.Empty), stream);

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void AddFile(string filePath) => throw new NotImplementedException();

        /// <summary>
        /// Adds the XML request.
        /// </summary>
        /// <param name="xmlDocument">The XML document.</param>
        /// <exception cref="NotImplementedException"></exception>
        public virtual void AddXmlRequest(XmlDocument xmlDocument) => throw new NotImplementedException();

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public virtual void Close()
        {
            this.AddBoundary(false, this.RequestBodyBottom);
            this.Write("--" + Environment.NewLine, this.RequestBodyBottom);
        }

        /// <summary>
        /// Initializes the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        public virtual void Init(HttpWebRequest request)
        {
            this.boundary = string.Format(
                CultureInfo.InvariantCulture,
                "============{0}==",
                Guid.NewGuid().ToString().Replace("-", string.Empty));
            this.request = request;
            this.request.Method = "POST";
            this.request.ContentType = string.Format(
                CultureInfo.InvariantCulture,
                @"multipart/related; boundary=""{0}""; type = ""application/atom+xml""",
                this.boundary);
        }

        /// <summary>
        /// Opens this instance.
        /// </summary>
        public virtual void Open() => this.AddBoundary(true, this.RequestBodyTop);

        /// <summary>
        /// Sends the request.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>HttpWebRequest.</returns>
        public virtual HttpWebRequest SendRequest(CancelableStream stream)
        {
            this.request.ContentLength = this.RequestBodyTop.Length + stream.Length + this.RequestBodyBottom.Length;
            this.request.AllowWriteStreamBuffering = false;
            this.requestStream = this.request.GetRequestStream();
            this.requestStream.Write(this.RequestBodyTop.ToArray(), 0, (int)this.RequestBodyTop.Length);
            StreamHelper.Transfer(stream, this.requestStream, 8192, true);
            this.requestStream.Write(this.RequestBodyBottom.ToArray(), 0, (int)this.RequestBodyBottom.Length);
            this.requestStream.Close();
            return this.request;
        }

        /// <summary>
        /// Writes the specified s.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="stream">The stream.</param>
        protected virtual void Write(string s, MemoryStream stream)
        {
            var newText = this.Utf8NoBomEncoding.GetBytes(s);
            stream.Write(newText, 0, newText.Length);
        }
    }
}
