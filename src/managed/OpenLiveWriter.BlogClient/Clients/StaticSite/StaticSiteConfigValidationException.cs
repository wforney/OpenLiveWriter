// <copyright file="StaticSiteConfigValidationException.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The StaticSiteConfigValidationException class.
    /// Implements the <see cref="BlogClientException" />
    /// Implements the <see cref="OpenLiveWriter.Extensibility.BlogClient.BlogClientException" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Extensibility.BlogClient.BlogClientException" />
    /// <seealso cref="BlogClientException" />
    public class StaticSiteConfigValidationException : BlogClientException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteConfigValidationException" /> class.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="text">The text.</param>
        /// <param name="textFormatArgs">The text format arguments.</param>
        public StaticSiteConfigValidationException(string title, string text, params object[] textFormatArgs)
            : base(title, text, textFormatArgs)
        {
        }
    }
}
