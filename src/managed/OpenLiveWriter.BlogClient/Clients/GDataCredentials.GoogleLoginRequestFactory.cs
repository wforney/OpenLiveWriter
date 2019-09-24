// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Net;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.HtmlParser.Parser.FormAgent;

    public partial class GDataCredentials
    {
        /// <summary>
        /// The GoogleLoginRequestFactory class.
        /// </summary>
        private class GoogleLoginRequestFactory
        {
            /// <summary>
            /// The captcha token
            /// </summary>
            private readonly string captchaToken;

            /// <summary>
            /// The captcha value
            /// </summary>
            private readonly string captchaValue;

            /// <summary>
            /// The password
            /// </summary>
            private readonly string password;

            /// <summary>
            /// The service
            /// </summary>
            private readonly string service;

            /// <summary>
            /// The source
            /// </summary>
            private readonly string source;

            /// <summary>
            /// The username
            /// </summary>
            private readonly string username;

            /// <summary>
            /// Initializes a new instance of the <see cref="GoogleLoginRequestFactory"/> class.
            /// </summary>
            /// <param name="username">The username.</param>
            /// <param name="password">The password.</param>
            /// <param name="service">The service.</param>
            /// <param name="source">The source.</param>
            /// <param name="captchaToken">The captcha token.</param>
            /// <param name="captchaValue">The captcha value.</param>
            public GoogleLoginRequestFactory(
                string username,
                string password,
                string service,
                string source,
                string captchaToken,
                string captchaValue)
            {
                this.username = username;
                this.password = password;
                this.service = service;
                this.source = source;
                this.captchaToken = captchaToken;
                this.captchaValue = captchaValue;
            }

            /// <summary>
            /// Creates the specified URI.
            /// </summary>
            /// <param name="uri">The URI.</param>
            /// <returns>An <see cref="HttpWebRequest"/>.</returns>
            public HttpWebRequest Create(string uri)
            {
                var formData = new FormData(
                    false,
                    "Email",
                    this.username,
                    "Passwd",
                    this.password,
                    "service",
                    this.service,
                    "source",
                    this.source);

                if (this.captchaToken != null && this.captchaValue != null)
                {
                    formData.Add("logintoken", this.captchaToken);
                    formData.Add("logincaptcha", this.captchaValue);
                }

                var request = HttpRequestHelper.CreateHttpWebRequest(uri, true);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";

                using (var inStream = formData.ToStream())
                using (var outStream = request.GetRequestStream())
                {
                    StreamHelper.Transfer(inStream, outStream);
                }

                return request;
            }
        }
    }
}
