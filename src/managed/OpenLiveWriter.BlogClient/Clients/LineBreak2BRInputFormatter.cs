// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// Summary description for SimpleTextLineFormatter.
    /// Implements the <see cref="IBlogPostContentFilter" />
    /// </summary>
    /// <seealso cref="IBlogPostContentFilter" />
    [BlogPostContentFilter("LineBreak2BR")]
    internal class LineBreak2BRInputFormatter : IBlogPostContentFilter
    {
        /// <summary>
        /// The uses HTML line breaks
        /// </summary>
        private static readonly Regex UsesHtmlLineBreaks = new Regex(
            @"<(br)(\s|/?>)",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <inheritdoc />
        public string OpenFilter(string content) => LineBreak2BRInputFormatter.ReplaceLineFormattedBreaks(content);

        /// <inheritdoc />
        public string PublishFilter(string content) => content;

        /// <summary>
        /// Replaces the line formatted breaks.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns>The string.</returns>
        private static string ReplaceLineFormattedBreaks(string html)
        {
            if (LineBreak2BRInputFormatter.UsesHtmlLineBreaks.IsMatch(html))
            {
                return html;
            }

            var sb = new StringBuilder();
            using (var reader = new StringReader(html))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    sb.Append(line);
                    sb.Append("<br>");
                    line = reader.ReadLine();
                }
            }

            var newHtml = sb.ToString();
            return newHtml;
        }
    }
}
