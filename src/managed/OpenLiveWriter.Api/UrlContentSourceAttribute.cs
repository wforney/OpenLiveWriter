// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContentFromUrl method to enable creation of new content from URLs. The source of
    /// this URL can either be the page the user was navigated to when they pressed the "Blog This"
    /// button or a URL that is pasted or dragged into the editor.
    /// Plugin classes which override this method must also be declared with the UrlContentSourceAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UrlContentSourceAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of a UrlContentSourceAttribute
        /// </summary>
        /// <param name="urlPattern">Regular expression which indicates which URL this content source can handle</param>
        public UrlContentSourceAttribute(string urlPattern) => this.UrlPattern = urlPattern;

        /// <summary>
        /// Regular expression which indicates which URL this content source can handle
        /// </summary>
        public string UrlPattern
        {
            get => this.urlPattern;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("UrlContentSource.UrlPattern");
                }

                if (!this.ValidateRegex(value))
                {
                    throw new ArgumentException(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            "The regular expression \"{0}\" is invalid.",
                            value),
                        "UrlContentSource.UrlPattern");
                }

                this.urlPattern = value;
            }
        }

        private string urlPattern;

        /// <summary>
        /// Indicates that the UrlContentSource requires a progress dialog during the execution of its CreateContentFromUrl
        /// method. This value should be specified if the content source performs network operations during content creation.
        /// Defaults to false.
        /// </summary>
        public bool RequiresProgress { get; set; } = false;

        /// <summary>
        /// Optional caption used in progress message.
        /// </summary>
        public string ProgressCaption
        {
            get => this.progressCaption;
            set => this.progressCaption = value ?? throw new ArgumentNullException("UrlContentSource.ProgressCaption");
        }

        private string progressCaption = string.Empty;

        /// <summary>
        /// Optional descriptive text used in progress message.
        /// </summary>
        public string ProgressMessage
        {
            get => this.progressMessage;
            set => this.progressMessage = value ?? throw new ArgumentNullException("UrlContentSource.ProgressMessage");
        }
        private string progressMessage = string.Empty;

        /// <summary>
        /// Validates the regex.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool ValidateRegex(string pattern)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
