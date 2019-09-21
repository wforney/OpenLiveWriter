// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Summary description for WebProxyPreferences.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.Preferences.Preferences" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.Preferences.Preferences" />
    public class WebProxyPreferences : Preferences
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebProxyPreferences"/> class.
        /// </summary>
        public WebProxyPreferences() : base("WebProxy")
        {
        }

        /// <summary>
        /// Gets or sets a value indicating whether [proxy enabled].
        /// </summary>
        /// <value><c>true</c> if [proxy enabled]; otherwise, <c>false</c>.</value>
        public bool ProxyEnabled
        {
            get => this.proxyEnabled;
            set { this.proxyEnabled = value; this.Modified(); }
        }
        /// <summary>
        /// The proxy enabled
        /// </summary>
        private bool proxyEnabled;

        /// <summary>
        /// Gets or sets the hostname.
        /// </summary>
        /// <value>The hostname.</value>
        public string Hostname
        {
            get => this.hostname;
            set { this.hostname = value; this.Modified(); }
        }
        /// <summary>
        /// The hostname
        /// </summary>
        private string hostname;

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get => this.port;
            set { this.port = value; this.Modified(); }
        }
        /// <summary>
        /// The port
        /// </summary>
        private int port;

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username
        {
            get => this.username;
            set { this.username = value; this.Modified(); }
        }
        /// <summary>
        /// The username
        /// </summary>
        private string username;

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get => this.password;
            set { this.password = value; this.Modified(); }
        }
        /// <summary>
        /// The password
        /// </summary>
        private string password;

        /// <summary>
        /// Loads preferences.  This method is overridden in derived classes to load the
        /// preferences of the class.
        /// </summary>
        protected override void LoadPreferences()
        {
            this.ProxyEnabled = WebProxySettings.ProxyEnabled;
            this.Hostname = WebProxySettings.Hostname;
            this.Port = WebProxySettings.Port;
            this.Username = WebProxySettings.Username;
            this.Password = WebProxySettings.Password;
        }

        /// <summary>
        /// Saves preferences.  This method is overridden in derived classes to save the
        /// preferences of the class.
        /// </summary>
        protected override void SavePreferences()
        {
            WebProxySettings.ProxyEnabled = this.ProxyEnabled;
            WebProxySettings.Hostname = this.Hostname;
            WebProxySettings.Port = this.Port;
            WebProxySettings.Username = this.Username;
            WebProxySettings.Password = this.Password;
        }
    }
}
