// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;

    public partial class LiveJournalClient
    {
        /// <summary>
        /// The FotobilderRequestManager class.
        /// </summary>
        private class FotobilderRequestManager
        {
            /// <summary>
            /// The endpoint
            /// </summary>
            private const string Endpoint = "http://pics.livejournal.com/interface/simple";

            /// <summary>
            /// The password
            /// </summary>
            private readonly string password;

            /// <summary>
            /// The username
            /// </summary>
            private readonly string username;

            /// <summary>
            /// Initializes a new instance of the <see cref="FotobilderRequestManager"/> class.
            /// </summary>
            /// <param name="username">The username.</param>
            /// <param name="password">The password.</param>
            public FotobilderRequestManager(string username, string password)
            {
                this.username = username;
                this.password = password;
            }

            /// <summary>
            /// Performs the get.
            /// </summary>
            /// <param name="mode">The mode.</param>
            /// <param name="challenge">The challenge.</param>
            /// <param name="additionalParams">The additional parameters.</param>
            /// <returns>An <see cref="XmlDocument"/>.</returns>
            public XmlDocument PerformGet(string mode, string challenge, params string[] additionalParams)
            {
                var request = this.CreateRequest(mode, challenge, additionalParams);
                request.Method = "GET";
                return FotobilderRequestManager.GetResponse(request, mode);
            }

            /// <summary>
            /// Performs the put.
            /// </summary>
            /// <param name="mode">The mode.</param>
            /// <param name="challenge">The challenge.</param>
            /// <param name="requestBody">The request body.</param>
            /// <param name="additionalParams">The additional parameters.</param>
            /// <returns>An <see cref="XmlDocument"/>.</returns>
            public XmlDocument PerformPut(string mode, string challenge, Stream requestBody, params string[] additionalParams)
            {
                var request = this.CreateRequest(mode, challenge, additionalParams);
                request.Method = "PUT";
                using (var requestStream = request.GetRequestStream())
                {
                    StreamHelper.Transfer(requestBody, requestStream);
                }

                return FotobilderRequestManager.GetResponse(request, mode);
            }

            /// <summary>
            /// Checks for errors.
            /// </summary>
            /// <param name="doc">The document.</param>
            /// <param name="mode">The mode.</param>
            private static void CheckForErrors(XmlDocument doc, string mode)
            {
                XmlNode errorNode;
                if ((errorNode = doc.SelectSingleNode("//Error")) == null)
                {
                    return;
                }

                /*
                    Possible errors:
                    1xx: User Errors
                    100	User error
                    101	No user specified
                    102	Invalid user
                    103	Unknown user

                    2xx: Client Errors
                    200	Client error
                    201	Invalid request
                    202	Invalid mode
                    203	GetChallenge(s) is exclusive as primary mode
                    210	Unknown argument
                    211	Invalid argument
                    212	Missing required argument
                    213	Invalid image for upload

                    3xx: Access Errors
                    300	Access error
                    301	No auth specified
                    302	Invalid auth
                    303	Account status does not allow upload

                    4xx: Limit Errors
                    400	Limit error
                    401	No disk space remaining
                    402	Insufficient disk space remaining
                    403	File upload limit exceeded

                    5xx: Server Errors
                    500	Internal Server Error
                    501	Cannot connect to database
                    502	Database Error
                    503	Application Error
                    510	Error creating gpic
                    511	Error creating upic
                    512	Error creating gallery
                    513	Error adding to gallery
                 */
                var errorCode = errorNode.Attributes?["code"].Value;
                var errorString = errorNode.InnerText;
                switch (errorCode)
                {
                    case "301":
                    case "302":
                        throw new BlogClientAuthenticationException(errorCode, errorString);
                    case "303":
                        throw new BlogClientFileUploadNotSupportedException(errorCode, errorString);
                    default:
                        throw new BlogClientProviderException(errorCode, errorString);
                }
            }

            /// <summary>
            /// Gets the response.
            /// </summary>
            /// <param name="request">The request.</param>
            /// <param name="mode">The mode.</param>
            /// <returns>An <see cref="XmlDocument"/>.</returns>
            private static XmlDocument GetResponse(WebRequest request, string mode)
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseReader = new StreamReader(
                    response.GetResponseStream() ?? throw new InvalidOperationException(),
                    Encoding.UTF8))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(responseReader);
                    FotobilderRequestManager.CheckForErrors(xmlDoc, mode);
                    return xmlDoc;
                }
            }

            /// <summary>
            /// Calculates the MD5 hash of the specified string.
            /// </summary>
            /// <param name="str">The string.</param>
            /// <returns>The hash.</returns>
            private static string Md5Hash(string str)
            {
                var bytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.AppendFormat("{0:x2}", b);
                }

                return sb.ToString();
            }

            /// <summary>
            /// Creates the authentication string.
            /// </summary>
            /// <param name="challenge">The challenge.</param>
            /// <returns>The authentication string.</returns>
            private string CreateAuthString(string challenge) =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "crp:{0}:{1}",
                    challenge,
                    FotobilderRequestManager.Md5Hash(challenge + FotobilderRequestManager.Md5Hash(this.password)));

            /// <summary>
            /// Creates the request.
            /// </summary>
            /// <param name="mode">The mode.</param>
            /// <param name="challenge">The challenge.</param>
            /// <param name="additionalParams">The additional parameters.</param>
            /// <returns>An <see cref="HttpWebRequest"/>.</returns>
            private HttpWebRequest CreateRequest(string mode, string challenge, params string[] additionalParams)
            {
                var request = HttpRequestHelper.CreateHttpWebRequest(
                    FotobilderRequestManager.Endpoint,
                    true,
                    Timeout.Infinite,
                    Timeout.Infinite);
                request.Headers.Add("X-FB-User", this.username);
                if (challenge != null)
                {
                    request.Headers.Add("X-FB-Auth", this.CreateAuthString(challenge));
                }

                request.Headers.Add("X-FB-Mode", mode);

                if (additionalParams == null)
                {
                    return request;
                }

                for (var i = 0; i < additionalParams.Length; i += 2)
                {
                    var name = additionalParams[i];
                    var value = additionalParams[i + 1];
                    if (name != null)
                    {
                        request.Headers.Add($"X-FB-{name}", value);
                    }
                }

                return request;
            }
        }
    }
}
