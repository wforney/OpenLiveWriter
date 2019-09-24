// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The Google data credentials class.
    /// </summary>
    public partial class GDataCredentials
    {
        /// <summary>
        /// The blogger service name
        /// </summary>
        public const string BloggerServiceName = "blogger";

        /// <summary>
        /// The client login URL
        /// </summary>
        public const string ClientLoginUrl = "https://www.google.com/accounts/ClientLogin";

        /// <summary>
        /// The Picasa web service name
        /// </summary>
        public const string PicasaWebServiceName = "lh2";

        /// <summary>
        /// The YouTube client login URL
        /// </summary>
        public const string YoutubeClientLoginUrl = "https://www.google.com/youtube/accounts/ClientLogin";

        /// <summary>
        /// The YouTube service name
        /// </summary>
        public const string YoutubeServiceName = "youtube";

        /// <summary>
        /// The authentications
        /// </summary>
        private readonly Hashtable auths = new Hashtable();

        /// <inheritdoc />
        internal GDataCredentials()
        {
        }

        /// <summary>
        /// Gets the Google data credentials from the transient credentials.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns>The <see cref="GDataCredentials"/>.</returns>
        public static GDataCredentials FromCredentials(TransientCredentials credentials)
        {
            if (!(credentials.Token is GDataCredentials cred))
            {
                credentials.Token = cred = new GDataCredentials();
            }

            return cred;
        }

        /// <summary>
        /// Attaches the credentials if valid.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="service">The service.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool AttachCredentialsIfValid(HttpWebRequest request, string username, string password, string service)
        {
            var auth = this.GetCredentialsIfValid(username, password, service);
            if (auth == null)
            {
                return false;
            }

            request.Headers.Set("Authorization", auth);
            return true;

        }

        /// <summary>
        /// Ensures the logged in.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="service">The service.</param>
        /// <param name="showUi">if set to <c>true</c> [show UI].</param>
        public void EnsureLoggedIn(string username, string password, string service, bool showUi) => this.EnsureLoggedIn(username, password, service, showUi, GDataCredentials.ClientLoginUrl);

        /// <summary>
        /// Ensures the logged in.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="service">The service.</param>
        /// <param name="showUi">if set to <c>true</c> [show UI].</param>
        /// <param name="uri">The URI.</param>
        /// <exception cref="BlogClientInvalidServerResponseException">No Auth token was present in the response.</exception>
        public void EnsureLoggedIn(string username, string password, string service, bool showUi, string uri)
        {
            try
            {
                if (this.IsValid(username, password, service))
                {
                    return;
                }

                string captchaToken = null;
                string captchaValue = null;

                var source = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}-{1}-{2}",
                    ApplicationEnvironment.CompanyName,
                    ApplicationEnvironment.ProductName,
                    ApplicationEnvironment.ProductVersion);

                while (true)
                {
                    var googleLoginRequestFactory = new GoogleLoginRequestFactory(
                        username,
                        password,
                        service,
                        source,
                        captchaToken,
                        captchaValue);
                    if (captchaToken != null && captchaValue != null)
                    {
                        captchaToken = null;
                        captchaValue = null;
                    }

                    HttpWebResponse response;
                    try
                    {
                        response = RedirectHelper.GetResponse(uri, googleLoginRequestFactory.Create);
                    }
                    catch (WebException we)
                    {
                        response = (HttpWebResponse)we.Response;
                        if (response == null)
                        {
                            Trace.Fail(we.ToString());
                            if (showUi)
                            {
                                showUi = false;
                                GDataCredentials.ShowError(MessageId.WeblogConnectionError, we.Message);
                            }

                            throw;
                        }
                    }

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            {
                                var ht = GDataCredentials.ParseAuthResponse(response.GetResponseStream());
                                if (ht.ContainsKey("Auth"))
                                {
                                    this.auths[new AuthKey(username, password, service)] = new AuthValue(
                                        (string)ht["Auth"],
                                        ht["YouTubeUser"] as string);
                                    return;
                                }

                                if (showUi)
                                {
                                    showUi = false;
                                    GDataCredentials.ShowError(MessageId.GoogleAuthTokenNotFound);
                                }

                                throw new BlogClientInvalidServerResponseException(
                                    uri,
                                    "No Auth token was present in the response.",
                                    string.Empty);
                            }
                        case HttpStatusCode.Forbidden:
                            {
                                // login failed
                                var ht = GDataCredentials.ParseAuthResponse(response.GetResponseStream());
                                var error = ht["Error"] as string;
                                if (error != null && error == "CaptchaRequired")
                                {
                                    captchaToken = (string)ht["CaptchaToken"];
                                    var captchaUrl = (string)ht["CaptchaUrl"];

                                    var helper = new GDataCaptchaHelper(
                                        new Win32WindowImpl(BlogClientUIContext.ContextForCurrentThread.Handle),
                                        captchaUrl);

                                    BlogClientUIContext.ContextForCurrentThread.Invoke(
                                        new ThreadStart(helper.ShowCaptcha),
                                        null);

                                    if (helper.DialogResult == DialogResult.OK)
                                    {
                                        captchaValue = helper.Reply;
                                        continue;
                                    }

                                    throw new BlogClientOperationCancelledException();
                                }

                                if (showUi)
                                {
                                    if (error == "NoLinkedYouTubeAccount")
                                    {
                                        if (DisplayMessage.Show(MessageId.YouTubeSignup, username) == DialogResult.Yes)
                                        {
                                            ShellHelper.LaunchUrl(GLink.Instance.YouTubeRegister);
                                        }

                                        return;
                                    }

                                    showUi = false;

                                    if (error == "BadAuthentication")
                                    {
                                        GDataCredentials.ShowError(MessageId.LoginFailed, ApplicationEnvironment.ProductNameQualified);
                                    }
                                    else
                                    {
                                        GDataCredentials.ShowError(MessageId.BloggerError, GDataCredentials.TranslateError(error));
                                    }
                                }

                                throw new BlogClientAuthenticationException(error, GDataCredentials.TranslateError(error));
                            }
                    }

                    if (showUi)
                    {
                        showUi = false;
                        GDataCredentials.ShowError(MessageId.BloggerError, $"{response.StatusCode}: {response.StatusDescription}");
                    }

                    throw new BlogClientAuthenticationException(response.StatusCode + string.Empty, response.StatusDescription);
                }
            }
            catch (BlogClientOperationCancelledException)
            {
                throw;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.ToString());
                if (showUi)
                {
                    GDataCredentials.ShowError(MessageId.UnexpectedErrorLogin, e.Message);
                }

                throw;
            }
        }

        /// <summary>
        /// Gets the credentials if valid.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="service">The service.</param>
        /// <returns>The credentials.</returns>
        public string GetCredentialsIfValid(string username, string password, string service)
        {
            if (!this.IsValid(username, password, service)
             || !(this.auths[new AuthKey(username, password, service)] is AuthValue authValue))
            {
                return null;
            }

            var auth = authValue.AuthString;
            return auth != null ? $"GoogleLogin auth={auth}" : null;
        }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="service">The service.</param>
        /// <returns>The name of the user.</returns>
        public string GetUserName(string username, string password, string service) =>
            this.auths[new AuthKey(username, password, service)] is AuthValue authValue
         && !string.IsNullOrEmpty(authValue.ReturnedUsername)
                ? authValue.ReturnedUsername
                : username;

        /// <summary>
        /// Returns true if ... is valid.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="service">The service.</param>
        /// <returns><c>true</c> if the specified username is valid; otherwise, <c>false</c>.</returns>
        public bool IsValid(string username, string password, string service)
        {
            if (!(this.auths[new AuthKey(username, password, service)] is AuthValue authValue))
            {
                return false;
            }

            var age = DateTimeHelper.UtcNow - authValue.DateCreatedUtc;
            return age >= TimeSpan.Zero && age <= TimeSpan.FromHours(23);
        }

        /// <summary>
        /// Parses the authentication response.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A <see cref="Hashtable"/>.</returns>
        private static Hashtable ParseAuthResponse(Stream stream)
        {
            var ht = CollectionsUtil.CreateCaseInsensitiveHashtable();
            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                while (!string.Equals(null, line = sr.ReadLine(), StringComparison.Ordinal))
                {
                    var chunks = line.Split(new[] { '=' }, 2);
                    if (chunks.Length == 2)
                    {
                        ht[chunks[0]] = chunks[1];
                    }
                }
            }

            return ht;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="args">The arguments.</param>
        private static void ShowError(MessageId messageId, params object[] args)
        {
            var helper = new ShowErrorHelper(BlogClientUIContext.ContextForCurrentThread, messageId, args);
            if (BlogClientUIContext.ContextForCurrentThread != null)
            {
                BlogClientUIContext.ContextForCurrentThread.Invoke(new ThreadStart(helper.Show), null);
            }
            else
            {
                helper.Show();
            }
        }

        /// <summary>
        /// Translates the error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>The error message.</returns>
        private static string TranslateError(string error)
        {
            switch (error)
            {
                case "BadAuthentication":
                    return Res.Get(StringId.BloggerBadAuthentication);
                case "NotVerified":
                    return Res.Get(StringId.BloggerNotVerified);
                case "TermsNotAgreed":
                    return Res.Get(StringId.BloggerTermsNotAgreed);
                case "Unknown":
                    return Res.Get(StringId.BloggerUnknown);
                case "AccountDeleted":
                    return Res.Get(StringId.BloggerAccountDeleted);
                case "AccountDisabled":
                    return Res.Get(StringId.BloggerAccountDisabled);
                case "ServiceUnavailable":
                    return Res.Get(StringId.BloggerServiceUnavailable);
                case "NoLinkedYouTubeAccount":
                    return Res.Get(StringId.YouTubeNoAccount);
                default:
                    return string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.BloggerGenericError), error);
            }
        }
    }
}
