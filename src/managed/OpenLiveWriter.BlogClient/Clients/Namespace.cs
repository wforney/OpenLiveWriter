// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    /// <summary>
    /// The Namespace structure.
    /// </summary>
    internal struct Namespace
    {
        /// <summary>
        /// The URI
        /// </summary>
        public readonly string Uri;

        /// <summary>
        /// The prefix
        /// </summary>
        public readonly string Prefix;

        /// <summary>
        /// Initializes a new instance of the <see cref="Namespace"/> struct.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="prefix">The prefix.</param>
        public Namespace(string uri, string prefix)
        {
            this.Uri = uri;
            this.Prefix = prefix;
        }
    }
}
