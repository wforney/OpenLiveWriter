// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using OpenLiveWriter.CoreServices;

    public partial class GDataCredentials
    {
        /// <summary>
        /// The AuthValue class.
        /// </summary>
        private class AuthValue
        {
            /// <summary>
            /// The returned username
            /// </summary>
            public readonly string ReturnedUsername; // YouTube only - null for all others

            /// <summary>
            /// Initializes a new instance of the <see cref="AuthValue"/> class.
            /// </summary>
            /// <param name="authString">The authentication string.</param>
            /// <param name="returnedUsername">The returned username.</param>
            public AuthValue(string authString, string returnedUsername)
            {
                this.AuthString = authString;
                this.DateCreatedUtc = DateTimeHelper.UtcNow;
                this.ReturnedUsername = returnedUsername;
            }

            /// <summary>
            /// Gets the authentication string.
            /// </summary>
            /// <value>The authentication string.</value>
            public string AuthString { get; }

            /// <summary>
            /// Gets the date created UTC.
            /// </summary>
            /// <value>The date created UTC.</value>
            public DateTime DateCreatedUtc { get; }
        }
    }
}
