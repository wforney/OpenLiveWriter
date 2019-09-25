// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using CoreServices;
    using CoreServices.Diagnostics;

    public partial class UpdateWeblogAsyncOperation
    {
        /// <summary>
        /// The PingHelper class.
        /// </summary>
        private class PingHelper
        {
            /// <summary>
            /// The blog name
            /// </summary>
            private readonly XmlRpcString blogName;

            /// <summary>
            /// The blog URL
            /// </summary>
            private readonly XmlRpcString blogUrl;

            /// <summary>
            /// The ping urls
            /// </summary>
            private readonly string[] pingUrls;

            /// <summary>
            /// Initializes a new instance of the <see cref="PingHelper"/> class.
            /// </summary>
            /// <param name="blogName">Name of the blog.</param>
            /// <param name="blogUrl">The blog URL.</param>
            /// <param name="pingUrls">The ping urls.</param>
            public PingHelper(string blogName, string blogUrl, string[] pingUrls)
            {
                this.blogName = new XmlRpcString(blogName);
                this.blogUrl = new XmlRpcString(blogUrl);
                this.pingUrls = pingUrls;
            }

            /// <summary>
            /// Threads the start.
            /// </summary>
            public void ThreadStart()
            {
                foreach (var url in this.pingUrls)
                {
                    try
                    {
                        var uri = new Uri(url);
                        if (uri.Scheme.ToLower(CultureInfo.InvariantCulture) != "http" &&
                            uri.Scheme.ToLower(CultureInfo.InvariantCulture) != "https")
                        {
                            continue;
                        }

                        var client = new XmlRpcClient(url, ApplicationEnvironment.UserAgent);
                        client.CallMethod("weblogUpdates.ping", this.blogName, this.blogUrl);
                    }
                    catch (Exception e)
                    {
                        if (ApplicationDiagnostics.VerboseLogging)
                        {
                            Trace.Fail($"Failure while pinging {url}: {e}");
                        }
                    }
                }
            }
        }
    }
}
