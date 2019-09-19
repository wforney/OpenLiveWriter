// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient
{
    using System;
    using System.Diagnostics;
    using OpenLiveWriter.CoreServices.Settings;
    using System.Linq;

    /// <summary>
    /// Class BlogCredentials.
    /// Implements the <see cref="OpenLiveWriter.BlogClient.IBlogCredentials" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.BlogClient.IBlogCredentials" />
    /// <seealso cref="System.IDisposable" />
    public class BlogCredentials : IBlogCredentials, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogCredentials"/> class.
        /// </summary>
        /// <param name="settingsRoot">The settings root.</param>
        /// <param name="domain">The domain.</param>
        public BlogCredentials(SettingsPersisterHelper settingsRoot, ICredentialsDomain domain)
        {
            this.settingsRoot = settingsRoot;
            this.Domain = domain;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get => this.GetUsername();
            set => this.CredentialsSettings.SetString(USERNAME, value);
        }

        /// <summary>
        /// The username
        /// </summary>
        private const string USERNAME = "Username";

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get => this.GetPassword() ?? string.Empty;
            set
            {
                // save an encrypted password
                try
                {
                    this.CredentialsSettings.SetEncryptedString(PASSWORD, value);
                }
                catch (Exception e)
                {
                    Trace.Fail("Failed to encrypt weblog password: " + e.Message, e.StackTrace);
                }
            }
        }

        /// <summary>
        /// The password
        /// </summary>
        private const string PASSWORD = "Password";

        /// <summary>
        /// Gets the custom values.
        /// </summary>
        /// <value>The custom values.</value>
        public string[] CustomValues => this.CredentialsSettings.GetNames()
            .Where(name => name != USERNAME && name != PASSWORD)
            .ToArray();

        /// <summary>
        /// Gets the custom value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        public string GetCustomValue(string name) => this.CredentialsSettings.GetString(name, string.Empty);

        /// <summary>
        /// Sets the custom value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void SetCustomValue(string name, string value) => this.CredentialsSettings.SetString(name, value);

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            this.Username = string.Empty;
            this.Password = string.Empty;
            foreach (var name in this.CredentialsSettings.GetNames())
            {
                this.CredentialsSettings.SetString(name, null);
            }
        }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>The domain.</value>
        public ICredentialsDomain Domain { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.credentialsSettingsRoot != null)
            {
                this.credentialsSettingsRoot.Dispose();
            }
        }

        /// <summary>
        /// Gets the credentials settings.
        /// </summary>
        /// <value>The credentials settings.</value>
        private SettingsPersisterHelper CredentialsSettings
        {
            get
            {
                if (this.credentialsSettingsRoot == null)
                {
                    this.credentialsSettingsRoot = this.settingsRoot.GetSubSettings("Credentials");
                }

                return this.credentialsSettingsRoot;
            }
        }

        /// <summary>
        /// Get Username from either the credentials key or the root key
        /// (seamless migration of accounts that existed prior to us moving
        /// the credentials into their own subkey)
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetUsername()
        {
            var username = this.CredentialsSettings.GetString(USERNAME, null);
            return username ?? this.settingsRoot.GetString(USERNAME, string.Empty);
        }

        /// <summary>
        /// Get Password from either the credentials key or the root key
        /// (seamless migration of accounts that existed prior to us moving
        /// the credentials into their own subkey)
        /// </summary>
        /// <returns>System.String.</returns>
        private string GetPassword()
        {
            var password = this.CredentialsSettings.GetEncryptedString(PASSWORD);
            return password ?? this.settingsRoot.GetEncryptedString(PASSWORD);
        }

        /// <summary>
        /// The credentials settings root
        /// </summary>
        private SettingsPersisterHelper credentialsSettingsRoot;

        /// <summary>
        /// The settings root
        /// </summary>
        private SettingsPersisterHelper settingsRoot;
    }
}
