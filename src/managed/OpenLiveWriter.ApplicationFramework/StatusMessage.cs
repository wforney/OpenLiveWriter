// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;

    /// <summary>
    ///     Class StatusMessage.
    /// </summary>
    public class StatusMessage
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusMessage" /> class.
        /// </summary>
        /// <param name="blogPostStatus">The blog post status.</param>
        public StatusMessage(string blogPostStatus)
            : this(null, blogPostStatus, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="StatusMessage" /> class.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <param name="blogPostStatus">The blog post status.</param>
        /// <param name="wordCountValue">The word count value.</param>
        public StatusMessage(Image icon, string blogPostStatus, string wordCountValue)
        {
            this.Icon = icon;
            this.BlogPostStatus = blogPostStatus;
            this.WordCountValue = wordCountValue;
        }

        /// <summary>
        ///     Gets or sets the icon.
        /// </summary>
        /// <value>The icon.</value>
        public Image Icon { get; set; }

        /// <summary>
        ///     Gets or sets the blog post status.
        /// </summary>
        /// <value>The blog post status.</value>
        public string BlogPostStatus { get; set; }

        /// <summary>
        ///     Gets or sets the word count value.
        /// </summary>
        /// <value>The word count value.</value>
        public string WordCountValue { get; set; }

        /// <summary>
        ///     Consumes the values.
        /// </summary>
        /// <param name="externalMessage">The external message.</param>
        public void ConsumeValues(StatusMessage externalMessage)
        {
            if (this.BlogPostStatus == null)
            {
                this.BlogPostStatus = externalMessage.BlogPostStatus;
                this.Icon = externalMessage.Icon;
            }

            if (this.WordCountValue == null)
            {
                this.WordCountValue = externalMessage.WordCountValue;
            }
        }
    }
}