// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Diagnostics;
    using System.Net;
    using System.Text;

    public abstract partial class AtomClient
    {
        /// <summary>
        /// The NewPostRequest class.
        /// </summary>
        private class NewPostRequest
        {
            /// <summary>
            /// The parent
            /// </summary>
            private readonly AtomClient parent;

            /// <summary>
            /// The slug
            /// </summary>
            private readonly string slug;

            /// <summary>
            /// Initializes a new instance of the <see cref="NewPostRequest"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            /// <param name="slug">The slug.</param>
            public NewPostRequest(AtomClient parent, string slug)
            {
                this.parent = parent;
                this.slug = slug;
            }

            /// <summary>
            /// Gets the slug header value.
            /// </summary>
            /// <value>The slug header value.</value>
            private string SlugHeaderValue
            {
                get
                {
                    var bytes = Encoding.UTF8.GetBytes(this.slug);
                    var sb = new StringBuilder(bytes.Length * 2);
                    foreach (var b in bytes)
                    {
                        if (b > 0x7F || b == '%')
                        {
                            sb.AppendFormat("%{0:X2}", b);
                        }
                        else if (b == '\r' || b == '\n')
                        {
                            // no \r or \n allowed in slugs
                        }
                        else if (b == 0)
                        {
                            Debug.Fail("null byte in slug string, this should never happen");
                        }
                        else
                        {
                            sb.Append((char)b);
                        }
                    }

                    return sb.ToString();
                }
            }

            /// <summary>
            /// Requests the filter.
            /// </summary>
            /// <param name="request">The request.</param>
            public void RequestFilter(HttpWebRequest request)
            {
                this.parent.RequestFilter(request);
                if (this.parent.Options.SupportsSlug && this.slug != null && this.slug.Length > 0)
                {
                    request.Headers.Add("Slug", this.SlugHeaderValue);
                }
            }
        }
    }
}
