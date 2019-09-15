// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Parser that is suitable for parsing HTML.
    ///
    /// The HTML does not need to be well-formed XML (i.e. mismatched tags are fine)
    /// or even well-formed HTML. In all but the most pathological cases, the parser
    /// will behave in a reasonable way that is similar to IE and Firefox.
    /// </summary>
    public class SimpleHtmlParser : IElementSource
    {
        private bool supportTrailingEnd = false;

        private readonly Stack<Element> elementStack = new Stack<Element>(5);
        private readonly List<Element> peekElements = new List<Element>();

        private readonly string data;  // the complete, raw HTML string
        private int pos = 0;  // the current parsing position

        private static readonly Regex comment = new Regex(@"<!--.*?--\s*>", RegexOptions.Compiled | RegexOptions.Singleline);
        // private static readonly Regex directive = new Regex(@"<![a-z][a-z0-9\.\-_:]*.*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static readonly Regex directive = new Regex(@"<!(?!--).*?>", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private static readonly Regex endScript = new Regex(@"</script\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex endStyle = new Regex(@"</style\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex begin = new Regex(
            @"<(?<tagname>[a-z][a-z0-9\.\-_:]*)",
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);

        private static readonly Regex attrName = new Regex(@"\s*([a-z][a-z0-9\.\-_:]*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex quotedAttrValue = new Regex(@"\s*=\s*([""'])(.*?)\1", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex unquotedAttrValue = new Regex(@"\s*=\s*([^\s>]+)", RegexOptions.Compiled);
        private static readonly Regex endBeginTag = new Regex(@"\s*(/)?>", RegexOptions.Compiled);
        private static readonly Regex end = new Regex(@"</([a-z][a-z0-9\.\-_:]*)\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly StatefulMatcher commentMatcher;
        private readonly StatefulMatcher directiveMatcher;
        private readonly StatefulMatcher beginMatcher;
        private readonly StatefulMatcher attrNameMatcher;
        private readonly StatefulMatcher quotedAttrValueMatcher;
        private readonly StatefulMatcher unquotedAttrValueMatcher;
        private readonly StatefulMatcher endBeginTagMatcher;
        private readonly StatefulMatcher endMatcher;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleHtmlParser"/> class.
        /// </summary>
        /// <param name="data">The HTML string.</param>
        public SimpleHtmlParser(string data)
        {
            this.data = data;

            this.commentMatcher = new StatefulMatcher(data, comment);
            this.directiveMatcher = new StatefulMatcher(data, directive);
            this.beginMatcher = new StatefulMatcher(data, begin);
            this.endMatcher = new StatefulMatcher(data, end);
            this.attrNameMatcher = new StatefulMatcher(data, attrName);
            this.quotedAttrValueMatcher = new StatefulMatcher(data, quotedAttrValue);
            this.unquotedAttrValueMatcher = new StatefulMatcher(data, unquotedAttrValue);
            this.endBeginTagMatcher = new StatefulMatcher(data, endBeginTag);
        }

        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position => this.peekElements.Count == 0
                    ? this.elementStack.Count == 0 ? this.pos : this.elementStack.Peek().Offset
                    : this.peekElements[0].Offset;

        /// <summary>
        /// Peeks the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns>Element.</returns>
        public Element Peek(int offset)
        {
            Element e;
            while (this.peekElements.Count <= offset && (e = this.Next(false)) != null)
            {
                this.peekElements.Add(e);
            }

            return this.peekElements.Count > offset ? this.peekElements[offset] : null;
        }

        /// <summary>
        /// Nexts this instance.
        /// </summary>
        /// <returns>Element.</returns>
        public Element Next() => this.Next(true);

        /// <summary>
        /// Retrieves the next element from the stream, or null
        /// if the end of the stream has been reached.
        /// </summary>
        private Element Next(bool allowPeekElement)
        {
            if (allowPeekElement && this.peekElements.Count > 0)
            {
                var peekElement = this.peekElements[0];
                this.peekElements.RemoveAt(0);
                return peekElement;
            }

            if (this.elementStack.Count != 0)
            {
                return this.elementStack.Pop();
            }

            var dataLen = this.data.Length;
            if (dataLen == this.pos)
            {
                // If we're at EOF, return
                return null;
            }

            // None of the special cases are true.  Start consuming characters
            var tokenStart = this.pos;

            while (true)
            {
                // Consume everything until a tag-looking thing
                while (this.pos < dataLen && this.data[this.pos] != '<')
                {
                    this.pos++;
                }

                if (this.pos >= dataLen)
                {
                    // EOF has been reached.
                    return tokenStart != this.pos ? new Text(this.data, tokenStart, this.pos - tokenStart) : null;
                }

                // We started parsing right on a tag-looking thing.  Try
                // parsing it as such.  If it doesn't turn out to be a tag,
                // we'll return it as text
                var oldPos = this.pos;

                var len = this.ParseMarkup(out var element, out var trailingEnd);
                if (len >= 0)
                {
                    this.pos += len;

                    if (trailingEnd != null)
                    {
                        // empty-element tag detected, add implicit end tag
                        this.elementStack.Push(trailingEnd);
                    }
                    else if (element is BeginTag)
                    {
                        // look for <script> or <style> body
                        Regex consumeTextUntil = null;

                        var tag = (BeginTag)element;
                        if (tag.NameEquals("script"))
                        {
                            consumeTextUntil = endScript;
                        }
                        else if (tag.NameEquals("style"))
                        {
                            consumeTextUntil = endStyle;
                        }

                        if (consumeTextUntil != null)
                        {
                            var structuredTextLen = this.ConsumeStructuredText(this.data, this.pos, consumeTextUntil);
                            this.pos += structuredTextLen;
                        }
                    }

                    this.elementStack.Push(element);
                    if (oldPos != tokenStart)
                    {
                        this.elementStack.Push(new Text(this.data, tokenStart, oldPos - tokenStart));
                    }

                    return this.elementStack.Pop();
                }
                else
                {
                    // '<' didn't begin a tag after all;
                    // consume it and continue
                    this.pos++;
                    continue;
                }
            }
        }

        /// <summary>
        /// Collects text between the current parsing position and
        /// the matching endTagName.  (Nested instances of the tag
        /// will not stop the collection.)
        /// </summary>
        /// <param name="endTagName">The name of the end tag, e.g. "div"
        /// (do NOT include angle brackets).</param>
        /// <returns>
        /// The text.
        /// </returns>
        public string CollectTextUntil(string endTagName)
        {
            var tagCount = 1;
            var buf = new StringBuilder();

            while (true)
            {
                var el = this.Next();

                if (el == null)
                {
                    break;
                }

                if (el is BeginTag && ((BeginTag)el).NameEquals(endTagName))
                {
                    tagCount++;
                }
                else if (el is EndTag && ((EndTag)el).NameEquals(endTagName))
                {
                    if (--tagCount == 0)
                    {
                        break;
                    }
                }
                else if (el is Text)
                {
                    // TODO: Instead of adding a single space
                    // between text nodes, we could add the appropriate space as
                    // we encounter space-influencing nodes.  i.e. when we encounter
                    // <p>, append "\r\n\r\n" to the buffer.
                    //
                    // Alternatively, we could add all of the tags to the buffer,
                    // and then call HTMLDocumentHelper.HTMLToPlainText() on the
                    // buf before returning.
                    if (buf.Length != 0)
                    {
                        buf.Append(' ');
                    }

                    buf.Append(((Text)el).ToString());
                }
            }

            return buf.ToString();
        }

        /// <summary>
        /// Collects all HTML between the current parsing position and
        /// the matching endTagName.  (Nested instances of the tag
        /// will not stop the collection.)
        /// </summary>
        /// <param name="endTagName">The name of the end tag, e.g. "div"
        /// (do NOT include angle brackets).</param>
        /// <returns>
        /// The text.
        /// </returns>
        public string CollectHtmlUntil(string endTagName)
        {
            var tagCount = 1;
            var buf = new StringBuilder();

            while (true)
            {
                var el = this.Next();

                if (el == null)
                {
                    break;
                }

                if (el is BeginTag && ((BeginTag)el).NameEquals(endTagName))
                {
                    tagCount++;
                }
                else if (el is EndTag && ((EndTag)el).NameEquals(endTagName))
                {
                    if (--tagCount == 0)
                    {
                        break;
                    }
                }

                buf.Append(this.data, el.Offset, el.Length);
            }

            return buf.ToString();
        }

        /// <summary>
        /// Parses the markup.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="trailingEnd">The trailing end.</param>
        /// <returns>System.Int32.</returns>
        private int ParseMarkup(out Element element, out EndTag trailingEnd)
        {
            trailingEnd = null;

            Match m;

            // commentMatcher MUST be checked before directiveMatcher!
            m = this.commentMatcher.Match(this.pos);
            if (m != null)
            {
                element = new Comment(this.data, this.pos, m.Length);
                return m.Length;
            }

            // commentMatcher MUST be checked before directiveMatcher!
            m = this.directiveMatcher.Match(this.pos);
            if (m != null)
            {
                element = new MarkupDirective(this.data, this.pos, m.Length);
                return m.Length;
            }

            m = this.endMatcher.Match(this.pos);
            if (m != null)
            {
                element = new EndTag(this.data, this.pos, m.Length, m.Groups[1].Value);
                return m.Length;
            }

            m = this.beginMatcher.Match(this.pos);
            if (m != null)
            {
                return this.ParseBeginTag(m, out element, out trailingEnd);
            }

            element = null;
            return -1;
        }

        /// <summary>
        /// Parses the begin tag.
        /// </summary>
        /// <param name="beginMatch">The begin match.</param>
        /// <param name="element">The element.</param>
        /// <param name="trailingEnd">The trailing end.</param>
        /// <returns>System.Int32.</returns>
        private int ParseBeginTag(Match beginMatch, out Element element, out EndTag trailingEnd)
        {
            trailingEnd = null;

            var tagNameGroup = beginMatch.Groups["tagname"];
            var tagName = tagNameGroup.Value;

            var tagPos = tagNameGroup.Index + tagNameGroup.Length;

            ArrayList attributes = null;
            LazySubstring extraResidue = null;
            var isComplete = false;

            while (true)
            {
                var match = this.endBeginTagMatcher.Match(tagPos);
                if (match != null)
                {
                    tagPos += match.Length;
                    if (match.Groups[1].Success)
                    {
                        isComplete = true;
                        if (this.supportTrailingEnd)
                        {
                            trailingEnd = new EndTag(this.data, tagPos, 0, tagName, true);
                        }
                    }

                    break;
                }

                match = this.attrNameMatcher.Match(tagPos);
                if (match == null)
                {
                    var residueStart = tagPos;
                    int residueEnd;

                    residueEnd = tagPos = this.data.IndexOfAny(new char[] { '<', '>' }, tagPos);
                    if (tagPos == -1)
                    {
                        residueEnd = tagPos = this.data.Length;
                    }
                    else if (this.data[tagPos] == '>')
                    {
                        tagPos++;
                    }
                    else
                    {
                        Debug.Assert(this.data[tagPos] == '<', "The tag position should point to the start tag.");
                    }

                    extraResidue = residueStart < residueEnd ? new LazySubstring(this.data, residueStart, residueEnd - residueStart) : null;
                    break;
                }
                else
                {
                    tagPos += match.Length;
                    var attrName = new LazySubstring(this.data, match.Groups[1].Index, match.Groups[1].Length);
                    LazySubstring attrValue = null;
                    match = this.quotedAttrValueMatcher.Match(tagPos);
                    if (match != null)
                    {
                        attrValue = new LazySubstring(this.data, match.Groups[2].Index, match.Groups[2].Length);
                        tagPos += match.Length;
                    }
                    else
                    {
                        match = this.unquotedAttrValueMatcher.Match(tagPos);
                        if (match != null)
                        {
                            attrValue = new LazySubstring(this.data, match.Groups[1].Index, match.Groups[1].Length);
                            tagPos += match.Length;
                        }
                    }

                    // no attribute value; that's OK
                    if (attributes == null)
                    {
                        attributes = new ArrayList();
                    }

                    attributes.Add(new Attr(attrName, attrValue));
                }
            }

            var len = tagPos - beginMatch.Index;
            element = new BeginTag(this.data, beginMatch.Index, len, tagName, attributes == null ? null : (Attr[])attributes.ToArray(typeof(Attr)), isComplete, extraResidue);
            return len;
        }

        private int ConsumeStructuredText(string data, int offset, Regex stopAt)
        {
            var match = stopAt.Match(data, offset);
            /*
                        if (!match.Success)
                        {
                            // Failure.  If an end tag is never encountered, the
                            // begin tag does not count.
                            // We can remove this whole clause if we want to behave
                            // more like IE than Gecko.
                            retval = string.Empty;
                            return 0;
                        }
            */

            var end = match.Success ? match.Index : data.Length;

            // HACK: this code should not be aware of parser types
            var source = (stopAt == endScript) ? (IElementSource)new JavascriptParser(data, offset, end - offset) : (IElementSource)new CssParser(data, offset, end - offset);
            var stack = new Stack();
            Element element;
            var last = this.pos;
            while ((element = source.Next()) != null)
            {
                stack.Push(element);
            }

            foreach (Element el in stack)
            {
                this.elementStack.Push(el);
            }

            return end - offset;
        }

        /// <summary>
        /// Class StatefulMatcher.
        /// </summary>
        private class StatefulMatcher
        {
            private readonly string input;
            private readonly Regex regex;
#if DEBUG
            private bool warned;
#endif
            private int lastStartOffset;
            private Match lastMatch;

            /// <summary>
            /// Initializes a new instance of the <see cref="StatefulMatcher"/> class.
            /// </summary>
            /// <param name="input">The input.</param>
            /// <param name="regex">The regex.</param>
            public StatefulMatcher(string input, Regex regex)
            {
                this.input = input;
                this.regex = regex;
                this.lastStartOffset = int.MaxValue;
                this.lastMatch = null;
#if DEBUG
                this.warned = false;
#endif
            }

            /// <summary>
            /// Matches the specified position.
            /// </summary>
            /// <param name="pos">The position.</param>
            /// <returns>Match.</returns>
            public Match Match(int pos)
            {
                /* We need to reexecute the search under any of these three conditions:
                 *
                 * 1) The search has never been run
                 * 2) The last search successfully matched before it got to the desired position
                 * 3) The last search was started past the desired position
                 */
                if (this.lastMatch == null || (this.lastMatch.Success && this.lastMatch.Index < pos) || this.lastStartOffset > pos)
                {
#if DEBUG
                    if (this.lastStartOffset > pos && this.lastStartOffset != int.MaxValue)
                    {
                        Debug.Assert(!this.warned, "StatefulMatcher moving backwards; this will work but is inefficient");
                        this.warned = true;
                    }
#endif
                    this.PerformMatch(pos);
                }

                return this.lastMatch.Success && pos == this.lastMatch.Index ? this.lastMatch : null;
            }

            /// <summary>
            /// Performs the match.
            /// </summary>
            /// <param name="pos">The position.</param>
            private void PerformMatch(int pos)
            {
                this.lastStartOffset = pos;
                this.lastMatch = this.regex.Match(this.input, pos);
            }
        }

        /// <summary>
        /// Creates this instance.
        /// </summary>
        public static void Create()
        {
            // Touch a static variable to make sure all static variables are created
            if (comment == null)
            {
            }
        }
    }
}
