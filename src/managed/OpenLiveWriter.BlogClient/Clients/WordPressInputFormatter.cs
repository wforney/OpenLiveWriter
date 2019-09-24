// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The WordPressInputFormatter class.
    /// Implements the <see cref="IBlogPostContentFilter" />
    /// </summary>
    /// <seealso cref="IBlogPostContentFilter" />
    [BlogPostContentFilter("WordPress")]
    public class WordPressInputFormatter : IBlogPostContentFilter
    {
        /// <summary>
        /// Opens the filter.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>The filter.</returns>
        public string OpenFilter(string c)
        {
            // Find the script/style/pre regions, and don't transform those.
            // Everything else goes through DoBreakFormatting.
            var result = new StringBuilder();
            var pos = 0;
            for (var m = Regex.Match(c, @"<(pre|script|style)\b.*?<\/\1>", RegexOptions.Singleline);
                 m.Success;
                 m = m.NextMatch())
            {
                result.Append(this.DoBreakFormatting(c.Substring(pos, m.Index - pos))).Append("\r\n");
                pos = m.Index + m.Length;
                result.Append(m.Value).Append("\r\n");
            }

            if (pos < c.Length)
            {
                result.Append(this.DoBreakFormatting(c.Substring(pos)));
            }

            return result.ToString();
        }

        /// <inheritdoc />
        public string PublishFilter(string content) => content;

        /// <summary>
        /// Does the break formatting.
        /// </summary>
        /// <param name="c">The c.</param>
        /// <returns>The string.</returns>
        private string DoBreakFormatting(string c)
        {
            const string Blocks =
                @"(?:address|blockquote|caption|colgroup|dd|div|dl|dt|embed|form|h1|h2|h3|h4|h5|h6|li|math|object|ol|p|param|table|tbody|td|tfoot|th|thead|tr|ul)(?=\s|>)";

            // Normalize hard returns
            this.Gsub(ref c, @"\r\n", "\n");

            // Normalize <br>
            this.Gsub(ref c, @"<br\s*\/?>", "\n");

            // Normalize <p> and </p>
            this.Gsub(ref c, @"<\/?p>", "\n\n");

            // Insert \n\n before each block start tag
            this.Gsub(ref c, $@"<{Blocks}", "\n\n$&");

            // Insert \n\n after each block end tag
            this.Gsub(ref c, $@"<\/{Blocks}>", "$&\n\n");

            // Coalesce 3 or more hard returns into one
            this.Gsub(ref c, @"(\s*\n){3,}\s*", "\n\n");

            // Now split the string into blocks, which are now delimited
            // by \n\n. Some blocks will be enclosed by block tags, which
            // we generally leave alone. Blocks that are not enclosed by
            // block tags will generally have <p>...</p> added to them.
            var chunks = StringHelper.Split(c, "\n\n").ToArray();

            for (var i = 0; i < chunks.Length; i++)
            {
                var chunk = chunks[i];

                if (!Regex.IsMatch(chunk, $@"^<\/?{Blocks}[^>]*>$"))
                {
                    // Special case for blockquote. Blockquotes are the only blocks
                    // as far as I can tell that will wrap their contents in <p> if
                    // they don't already immediately contain a block.
                    this.Gsub(ref chunk, @"^<blockquote(?:\s[^>]*)?>(?!$)", "$&<p>");
                    this.Gsub(ref chunk, @"(?<!^)<\/blockquote>$", "</p>$&");

                    // If this chunk doesn't start with a block, add a <p>
                    if (!Regex.IsMatch(chunk, $@"^<{Blocks}"))
                    {
                        // && !Regex.IsMatch(chunk, @"^<\/" + blocks + ">"))
                        chunk = $"<p>{chunk}";
                    }

                    // If this chunk starts with a <p> tag--either because
                    // it always did (like <p class="foo">, which doesn't get
                    // dropped either by WordPress or by our regexs above), or
                    // because we added one just now--we want to end it with
                    // a </p> if necessary.
                    if (Regex.IsMatch(chunk, @"<p(?:\s|>)") && !Regex.IsMatch(chunk, @"</p>"))
                    {
                        var m = Regex.Match(chunk, $@"<\/{Blocks}>$");
                        if (m.Success)
                        {
                            chunk = $"{chunk.Substring(0, m.Index)}</p>{chunk.Substring(m.Index)}";
                        }
                        else
                        {
                            chunk += "</p>";
                        }
                    }

                    // Convert all remaining hard returns to <br />
                    this.Gsub(ref chunk, @"\n", "<br />\r\n");
                }

                chunks[i] = chunk;
            }

            // Put the blocks back together before returning.
            return StringHelper.Join(chunks, "\r\n");
        }

        /// <summary>
        /// Executes the regex pattern on the value with the specified replacement.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="replacement">The replacement.</param>
        private void Gsub(ref string val, string pattern, string replacement) =>
            val = Regex.Replace(val, pattern, replacement);
    }
}
