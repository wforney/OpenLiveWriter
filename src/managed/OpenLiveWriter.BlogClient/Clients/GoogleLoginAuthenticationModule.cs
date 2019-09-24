// <copyright file="GenericAtomClient.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.BlogClient.Clients
{
    /*
     * TODO
     *
     * Make sure all required fields are filled out.
     * Remove the HTML title from the friendly error message
     * Test ETags where HEAD not supported
     * Test experience when no media collection configured
     * Add command line option for preferring Atom over RSD
     * See if blogproviders.xml can override Atom vs. RSD preference
     */
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The GoogleLoginAuthenticationModule class.
    /// Implements the <see cref="IAuthenticationModule" />
    /// </summary>
    /// <seealso cref="IAuthenticationModule" />
    public class GoogleLoginAuthenticationModule : IAuthenticationModule
    {
        /// <summary>
        /// The Google data credentials
        /// </summary>
        private static readonly GDataCredentials GoogleDataCredentials = new GDataCredentials();

        /// <inheritdoc />
        public string AuthenticationType => "GoogleLogin";

        /// <inheritdoc />
        public bool CanPreAuthenticate => false;

        /// <inheritdoc />
        public Authorization Authenticate(string challenge, WebRequest request, ICredentials credentials)
        {
            if (!challenge.StartsWith("GoogleLogin ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            GoogleLoginAuthenticationModule.ParseChallenge(challenge, out var realm, out var service);
            if (realm != "http://www.google.com/accounts/ClientLogin")
            {
                return null;
            }

            var cred = credentials.GetCredential(request.RequestUri, this.AuthenticationType);

            var auth = GoogleLoginAuthenticationModule.GoogleDataCredentials.GetCredentialsIfValid(
                cred.UserName,
                cred.Password,
                service);
            if (auth != null)
            {
                return new Authorization(auth, true);
            }

            try
            {
                GoogleLoginAuthenticationModule.GoogleDataCredentials.EnsureLoggedIn(
                    cred.UserName,
                    cred.Password,
                    service,
                    !BlogClientUIContext.SilentModeForCurrentThread);
                auth = GoogleLoginAuthenticationModule.GoogleDataCredentials.GetCredentialsIfValid(
                    cred.UserName,
                    cred.Password,
                    service);
                return auth == null ? null : new Authorization(auth, true);
            }
            catch (Exception e)
            {
                Trace.Fail(e.ToString());
                return null;
            }
        }

        /// <inheritdoc />
        public Authorization PreAuthenticate(WebRequest request, ICredentials credentials) =>
            throw new NotImplementedException();

        /// <summary>
        /// Parses the challenge.
        /// </summary>
        /// <param name="challenge">The challenge.</param>
        /// <param name="realm">The realm.</param>
        /// <param name="service">The service.</param>
        private static void ParseChallenge(string challenge, out string realm, out string service)
        {
            var m = Regex.Match(challenge, @"\brealm=""([^""]*)""");
            realm = m.Groups[1].Value;
            var m2 = Regex.Match(challenge, @"\bservice=""([^""]*)""");
            service = m2.Groups[1].Value;
        }
    }
}
