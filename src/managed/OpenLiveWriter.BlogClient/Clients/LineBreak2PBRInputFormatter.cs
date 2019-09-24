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
    [BlogPostContentFilter("LineBreak2PBR")]
    internal class LineBreak2PBRInputFormatter : IBlogPostContentFilter
    {
        /// <summary>
        /// The uses HTML line breaks
        /// </summary>
        private static readonly Regex UsesHtmlLineBreaks = new Regex(
            @"<(p|br)(\s|/?>)",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);

        /// <inheritdoc />
        public string OpenFilter(string content) => LineBreak2PBRInputFormatter.ReplaceLineFormattedBreaks(content);

        /// <inheritdoc />
        public string PublishFilter(string content) => content;

        /// <summary>
        /// Reads to paragraph end.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="paragraphBuilder">The paragraph builder.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        private static bool ReadToParagraphEnd(TextReader reader, StringBuilder paragraphBuilder)
        {
            var line = reader.ReadLine();
            var pendingLineBreaks = 0;
            while (line != null)
            {
                if (pendingLineBreaks == 1 && line == string.Empty)
                {
                    return true;
                }

                if (pendingLineBreaks == 1 && line != string.Empty)
                {
                    paragraphBuilder.Append("<br>");
                    pendingLineBreaks = 0;
                }

                paragraphBuilder.Append(line);

                pendingLineBreaks++;
                line = reader.ReadLine();
            }

            return false;
        }

        /// <summary>
        /// Replaces the line formatted breaks.
        /// </summary>
        /// <param name="html">The HTML.</param>
        /// <returns>The result.</returns>
        private static string ReplaceLineFormattedBreaks(string html)
        {
            if (LineBreak2PBRInputFormatter.UsesHtmlLineBreaks.IsMatch(html))
            {
                return html;
            }

            var sb = new StringBuilder();
            var paragraphBuilder = new StringBuilder();
            var paragraphsAdded = false;
            using (var reader = new StringReader(html))
            {
                while (LineBreak2PBRInputFormatter.ReadToParagraphEnd(reader, paragraphBuilder))
                {
                    paragraphsAdded = true;
                    sb.AppendFormat("<p>{0}</p>", paragraphBuilder);
                    paragraphBuilder.Length = 0;
                }
            }

            if (paragraphBuilder.Length > 0)
            {
                if (paragraphsAdded)
                {
                    // only wrap the last paragraph in <p> if other paragraphs where present in the post.
                    sb.AppendFormat("<p>{0}</p>", paragraphBuilder);
                }
                else
                {
                    sb.Append(paragraphBuilder);
                }
            }

            var newHtml = sb.ToString();
            return newHtml;
        }
    }
}
