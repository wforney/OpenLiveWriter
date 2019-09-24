// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    public partial class GDataCredentials
    {
        /// <summary>
        /// The AuthKey class.
        /// </summary>
        internal class AuthKey
        {
            /// <summary>
            /// The password
            /// </summary>
            private readonly string password;

            /// <summary>
            /// The service
            /// </summary>
            private readonly string service;

            /// <summary>
            /// The username
            /// </summary>
            private readonly string username;

            /// <summary>
            /// Initializes a new instance of the <see cref="AuthKey"/> class.
            /// </summary>
            /// <param name="username">The username.</param>
            /// <param name="password">The password.</param>
            /// <param name="service">The service.</param>
            public AuthKey(string username, string password, string service)
            {
                this.username = username;
                this.password = password;
                this.service = service;
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (this == obj)
                {
                    return true;
                }

                if (!(obj is AuthKey authKey))
                {
                    return false;
                }

                if (!object.Equals(this.username, authKey.username))
                {
                    return false;
                }

                if (!object.Equals(this.password, authKey.password))
                {
                    return false;
                }

                if (!object.Equals(this.service, authKey.service))
                {
                    return false;
                }

                return true;
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                var result = this.username.GetHashCode();
                result = (29 * result) + this.password.GetHashCode();
                result = (29 * result) + this.service.GetHashCode();
                return result;
            }
        }
    }
}
