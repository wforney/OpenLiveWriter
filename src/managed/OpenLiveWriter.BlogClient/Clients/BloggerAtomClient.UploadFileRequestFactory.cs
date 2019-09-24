// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.IO;
    using System.Net;
    using OpenLiveWriter.CoreServices;

    public partial class BloggerAtomClient
    {
        /// <summary>
        /// The UploadFileRequestFactory class.
        /// </summary>
        private class UploadFileRequestFactory
        {
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
            private readonly BloggerAtomClient parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="UploadFileRequestFactory"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="filename">The filename.</param>
            /// <param name="method">The method.</param>
            public UploadFileRequestFactory(BloggerAtomClient parent, string filename, string method)
            {
                this.parent = parent;
                this.filename = filename;
                this.method = method;
            }

            /// <summary>
            /// Creates the specified URI.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <returns>An <see cref="HttpWebRequest"/>.</returns>
            public HttpWebRequest Create(string uri)
            {
                // TODO: choose rational timeout values
                var request = HttpRequestHelper.CreateHttpWebRequest(uri, false);

                this.parent.PicasaAuthorizationFilter(request);

                request.ContentType = MimeHelper.GetContentType(Path.GetExtension(this.filename));
                try
                {
                    request.Headers.Add("Slug", Path.GetFileNameWithoutExtension(this.filename));
                }
                catch (ArgumentException)
                {
                    request.Headers.Add("Slug", "Image");
                }

                request.Method = this.method;

                using (var s = request.GetRequestStream())
                using (Stream inS = new FileStream(this.filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    StreamHelper.Transfer(inS, s);
                }

                return request;
            }
        }
    }
}
