// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient
{
    using System;
    using OpenLiveWriter.CoreServices.Settings;

    /// <summary>
    /// Class BlogFileUploadSettings.
    /// Implements the <see cref="OpenLiveWriter.BlogClient.IBlogFileUploadSettings" />
    /// Implements the <see cref="System.IDisposable" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.BlogClient.IBlogFileUploadSettings" />
    /// <seealso cref="System.IDisposable" />
    public class BlogFileUploadSettings : IBlogFileUploadSettings, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlogFileUploadSettings"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public BlogFileUploadSettings(SettingsPersisterHelper settings) => this.settings = settings;

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>System.String.</returns>
        public string GetValue(string name) => this.settings.GetString(name, string.Empty);

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public void SetValue(string name, string value) => this.settings.SetString(name, value);

        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <value>The names.</value>
        public string[] Names => this.settings.GetNames();

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this.settings != null)
            {
                this.settings.Dispose();
                this.settings = null;
            }
        }

        /// <summary>
        /// The settings
        /// </summary>
        private SettingsPersisterHelper settings;
    }
}
