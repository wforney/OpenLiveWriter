// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    public abstract partial class AtomProtocolVersion
    {
        /// <summary>
        /// The Atom10DraftProtocolVersion class.
        /// Implements the <see cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion.Atom10ProtocolVersion" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion.Atom10ProtocolVersion" />
        private class Atom10DraftProtocolVersion : Atom10ProtocolVersion
        {
            /// <summary>
            /// Gets the atom pub namespace URI.
            /// </summary>
            /// <value>The atom pub namespace URI.</value>
            public override string AtomPubNamespaceUri => "http://purl.org/atom/app#";

            /// <summary>
            /// Gets the pub namespace URI.
            /// </summary>
            /// <value>The pub namespace URI.</value>
            public override string PubNamespaceUri => "http://purl.org/atom/app#";
        }
    }
}
