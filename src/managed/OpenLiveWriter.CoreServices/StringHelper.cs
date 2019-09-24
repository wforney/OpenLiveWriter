// <copyright company=".NET Foundation" file="StringHelper.cs">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenLiveWriter.Localization;

    /// <summary>
    /// The StringHelper class.
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        /// The gigabyte
        /// </summary>
        private const long GB = StringHelper.KB * StringHelper.KB * StringHelper.KB;

        /// <summary>
        /// The kilobyte
        /// </summary>
        private const long KB = 1024L;

        /// <summary>
        /// The megabyte
        /// </summary>
        private const long MB = StringHelper.KB * StringHelper.KB;

        /// <summary>
        /// The pattern
        /// </summary>
        private const string Pattern = "{0:#,##0.0000}";

        /// <summary>
        /// The whitespace
        /// </summary>
        private static readonly char[] Whitespace = { ' ', '\r', '\n', '\t' };

        /// <summary>
        /// turns single hard return into a space
        /// </summary>
        private static readonly LazyLoader<Regex> stripSingleLineFeeds;

        /// <summary>
        /// strips spaces and tabs from around hard returns
        /// </summary>
        private static readonly LazyLoader<Regex> stripSpaces;

        /// <inheritdoc />
        static StringHelper()
        {
            StringHelper.stripSingleLineFeeds = new LazyLoader<Regex>(() => new Regex(@"(?<=\S)\r?\n(?=\S)"));
            StringHelper.stripSpaces = new LazyLoader<Regex>(() => new Regex(@"[ \t]*\r?\n[ \t]*"));
        }

        /// <summary>
        /// Gets the strip single line feeds regex.
        /// </summary>
        /// <value>The strip single line feeds regex.</value>
        private static Regex StripSingleLineFeedsRegex => StringHelper.stripSingleLineFeeds.Value;

        /// <summary>
        /// Gets the strip spaces regex.
        /// </summary>
        /// <value>The strip spaces regex.</value>
        private static Regex StripSpacesRegex => StringHelper.stripSpaces.Value;

        /// <summary>
        /// Returns the longest common prefix that is shared by the two strings.
        /// </summary>
        /// <param name="one">The one.</param>
        /// <param name="two">The two.</param>
        /// <returns>The prefix.</returns>
        public static string CommonPrefix(string one, string two)
        {
            var minLen = Math.Min(one.Length, two.Length);

            for (var i = 0; i < minLen; i++)
            {
                if (one[i] != two[i])
                {
                    return one.Substring(0, i);
                }
            }

            return one.Length < two.Length ? one : two;
        }

        /// <summary>
        /// Compresses the excess whitespace.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The compressed string.</returns>
        public static string CompressExcessWhitespace(string str)
        {
            // RegexOptions.Multiline only works with \n, not \r\n
            str = LineEndingHelper.Normalize(str, LineEnding.LF);

            // trim lines made up of only spaces and tabs (but keep the line breaks)
            // str = Regex.Replace(str, @"[ \t]+(?=\r\n|$)", "");
            str = Regex.Replace(str, @"[ \t]+$", string.Empty, RegexOptions.Multiline);

            // str = Regex.Replace(str, @"(?<=^|\n)[ \t]+(?=\r\n|$)", "");
            str = str.TrimStart('\n');
            str = str.TrimEnd('\n');
            str = Regex.Replace(str, @"\n{3,}", "\n\n");

            str = LineEndingHelper.Normalize(str, LineEnding.CRLF);
            return str;
        }

        /// <summary>
        /// Add ellipsis to the specified value.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="threshold">The threshold.</param>
        /// <returns>The string.</returns>
        public static string Ellipsis(string val, int threshold)
        {
            /*
             * Some edge cases that are somewhat bogus:
             * Ellipsis(".........", 8) => "..."
             * Ellipsis(". . . . .", 8) => "..."
             */
            if (val.Length > threshold)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Res.Get(StringId.WithEllipses),
                    val.Substring(0, threshold).TrimEnd(' ', '\r', '\n', '\t', '.'));
            }

            return val;
        }

        /// <summary>
        /// Format a byte count in a nice, pretty way.  Similar to the
        /// way Windows Explorer displays sizes.
        /// 1) Decide what the units will be.
        /// 2) Scale the byte count to the chosen unit, as a double.
        /// 3) Format the double, to a relatively high degree of precision.
        /// 4) Truncate the number of digits shown to the greater of:
        /// a) the number of digits in the whole-number part
        /// b) 3
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The string.</returns>
        public static string FormatByteCount(long bytes)
        {
            string num;
            string format;
            if (bytes < StringHelper.KB)
            {
                num = bytes.ToString("N", CultureInfo.CurrentCulture);
                format = Res.Get(StringId.BytesFormat);
            }
            else if (bytes < StringHelper.MB * 0.97)
            {
                // this is what Windows does
                num = StringHelper.FormatNum((double)bytes / StringHelper.KB);
                format = Res.Get(StringId.KilobytesFormat);
            }
            else if (bytes < StringHelper.GB * 0.97)
            {
                // this is what Windows does
                num = StringHelper.FormatNum((double)bytes / StringHelper.MB);
                format = Res.Get(StringId.MegabytesFormat);
            }
            else
            {
                num = StringHelper.FormatNum((double)bytes / StringHelper.GB);
                format = Res.Get(StringId.GigabytesFormat);
            }

            return string.Format(CultureInfo.CurrentCulture, format, num);
        }

        /// <summary>
        /// Same as FormatByteCount, but negative values will be
        /// interpreted as (not available).
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="notAvailableString">The not available string.</param>
        /// <returns>The result.</returns>
        public static string FormatByteCountNoNegatives(long bytes, string notAvailableString) =>
            bytes < 0 ? notAvailableString : StringHelper.FormatByteCount(bytes);

        /// <summary>
        /// Gets the encoding.
        /// </summary>
        /// <param name="charset">The charset.</param>
        /// <param name="defaultEncoding">The default encoding.</param>
        /// <returns>The <see cref="Encoding"/>.</returns>
        public static Encoding GetEncoding(string charset, Encoding defaultEncoding)
        {
            if (string.IsNullOrEmpty(charset))
            {
                return defaultEncoding;
            }

            if (string.Compare(Encoding.UTF8.WebName, charset, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return new UTF8Encoding(false, false);
            }

            try
            {
                return Encoding.GetEncoding(charset);
            }
            catch (ArgumentException e)
            {
                Debug.WriteLine($"BUG: Failed getting encoding for charset {charset}with error: {e}");
            }

            return defaultEncoding;
        }

        /// <summary>
        /// Gets a hash code for a string that is stable across multiple versions of .NET
        /// This implementation was taken from .NET 2.0
        /// <seealso cref="http://msdn.microsoft.com/en-us/library/system.string.gethashcode.aspx"/>
        /// </summary>
        /// <param name="stringToHash">The string to hash.</param>
        /// <returns>The hash code.</returns>
        public static unsafe int GetHashCodeStable(string stringToHash)
        {
            fixed (char* str = stringToHash)
            {
                var characterPointer = str;
                var num = 0x15051505;
                var num2 = num;
                var numPtr = (int*)characterPointer;
                for (var i = stringToHash.Length; i > 0; i -= 4)
                {
                    num = ((num << 5) + num + (num >> 0x1b)) ^ numPtr[0];
                    if (i <= 2)
                    {
                        break;
                    }

                    num2 = ((num2 << 5) + num2 + (num2 >> 0x1b)) ^ numPtr[1];
                    numPtr += 2;
                }

                return num + (num2 * 0x5d588b65);
            }
        }

        /// <summary>
        /// Gets the last word.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The last word.</returns>
        public static string GetLastWord(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            // Any whitespace at the end is not considered part of the last word.
            content = content.TrimEnd(StringHelper.Whitespace);

            if (string.IsNullOrEmpty(content))
            {
                return string.Empty;
            }

            var beforeLastWord = content.LastIndexOfAny(StringHelper.Whitespace);
            if (beforeLastWord > -1 && beforeLastWord < content.Length - 1)
            {
                content = content.Substring(beforeLastWord + 1);
            }

            return content;
        }

        /// <summary>
        /// Strips any occurrences of A, An, The, and whitespace from the beginning of the string.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns>The substring.</returns>
        public static string GetSignificantSubstring(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return title;
            }

            const string SignificantSubstringPattern = @"^\s*((the|a|an)($|\s+))*\s*";
            var match = Regex.Match(title, SignificantSubstringPattern, RegexOptions.IgnoreCase);
            return match.Success ? title.Substring(match.Length) : title;
        }

        /// <summary>
        /// Joins the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="collection">An collection.</param>
        /// <returns>The string.</returns>
        public static string Join<T>(ICollection<T> collection) =>
            collection == null ? null : StringHelper.Join(collection, ", ");

        /// <summary>
        /// Joins the specified collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="collection">An collection.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>The string.</returns>
        public static string Join<T>(ICollection<T> collection, string delimiter) => StringHelper.Join(collection, delimiter, false);

        /// <summary>
        /// Joins the specified an array.
        /// </summary>
        /// <typeparam name="T">The type of the items in the collection.</typeparam>
        /// <param name="collection">An array.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="removeEmpty">if set to <c>true</c> [remove empty].</param>
        /// <returns>The string.</returns>
        public static string Join<T>(ICollection<T> collection, string delimiter, bool removeEmpty)
        {
            var o = new StringBuilder();
            var delimited = string.Empty;
            foreach (object obj in collection)
            {
                var str = string.Empty;

                if (obj != null)
                {
                    str = obj.ToString().Trim();
                }

                if (removeEmpty && str == string.Empty)
                {
                    continue;
                }

                o.Append(delimited);
                if (obj == null)
                {
                    continue;
                }

                o.Append(str);
                delimited = delimiter;
            }

            return o.ToString();
        }

        /// <summary>
        /// Replaces the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="length">The length.</param>
        /// <param name="replacement">The replacement.</param>
        /// <returns>The output.</returns>
        public static string Replace(string value, int offset, int length, string replacement)
        {
            var output = new StringBuilder(value.Length + (replacement.Length - length));
            if (offset > 0)
            {
                output.Append(value.Substring(0, offset));
            }

            output.Append(replacement);
            if (offset + length < value.Length)
            {
                output.Append(value.Substring(offset + length));
            }

            return output.ToString();
        }

        /// <summary>
        /// Restricts the length.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="maxLen">The maximum length.</param>
        /// <returns>The string.</returns>
        public static string RestrictLength(string val, int maxLen) =>
            val.Length <= maxLen ? val : val.Substring(0, maxLen);

        /// <summary>
        /// Restricts the length at words.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="maxSize">The maximum size.</param>
        /// <returns>The restricted string.</returns>
        public static string RestrictLengthAtWords(string content, int maxSize)
        {
            if (maxSize == 0)
            {
                return string.Empty;
            }

            // chop off the word at the last space before index maxSize.
            if (content == null || content.Length <= maxSize)
            {
                return content;
            }

            var lastWordIndex = content.LastIndexOfAny(StringHelper.Whitespace, maxSize - 1, maxSize);
            return lastWordIndex == -1 ? content : content.Substring(0, lastWordIndex).TrimEnd(StringHelper.Whitespace);
        }

        /// <summary>
        /// Reverses the specified string value.
        /// </summary>
        /// <param name="strVal">The string value.</param>
        /// <returns>The reversed string value.</returns>
        public static string Reverse(string strVal)
        {
            var chars = strVal.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        /// <summary>
        /// Splits the specified string.
        /// </summary>
        /// <param name="input">The input string.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <returns>The split string collection.</returns>
        public static ICollection<string> Split(string input, string delimiter)
        {
            if (input.Length == 0)
            {
                return new string[0];
            }

            var list = new List<string>();
            var start = 0;
            int next;
            var delimiterLength = delimiter.Length;
            while (start < input.Length && (next = input.IndexOf(delimiter, start, StringComparison.CurrentCulture)) != -1)
            {
                var chunk = input.Substring(start, next - start).Trim();
                if (chunk != string.Empty)
                {
                    list.Add(chunk);
                }

                start = next + delimiterLength;
            }

            if (start == 0)
            {
                // short circuit when none found
                return new[] { input };
            }

            if (start < input.Length)
            {
                var chunk = input.Substring(start).Trim();
                if (chunk != string.Empty)
                {
                    list.Add(chunk);
                }
            }

            return list;
        }

        /// <summary>
        /// Splits a string at a specified delimiter, with escape logic.  For example:
        /// SplitWithEscape("one/two/three", '/', '_') =&gt; ["one", "two", "three"]
        /// SplitWithEscape("one_/two/three_/four", '/', '_') =&gt; ["one/two", "three/four"]
        /// SplitWithEscape("one__two/three", '/', '_') =&gt; ["one_two", "three"]
        /// SplitWithEscape("one/two/three_", '/', '_') =&gt; ["one", "two", "three_"]
        /// SplitWithEscape("", '/', '_') =&gt; []
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <returns>The split string collection.</returns>
        /// <exception cref="ArgumentException">Delimiter and escape characters cannot be identical.</exception>
        public static ICollection<string> SplitWithEscape(string val, char delimiter, char escapeChar)
        {
            if (delimiter == escapeChar)
            {
                throw new ArgumentException("Delimiter and escape characters cannot be identical.");
            }

            var results = new List<string>();
            var buffer = new char[val.Length]; // no element can be longer than the original string
            var pos = 0; // our pointer into the buffer

            var escaped = false;
            foreach (var thisChar in val)
            {
                if (escaped)
                {
                    // the last character was the escape char; this char
                    // should not be evaluated, just written
                    buffer[pos++] = thisChar;
                    escaped = false;
                }
                else
                {
                    if (thisChar == escapeChar)
                    {
                        // encountering escape char; do nothing, just make
                        // sure next character is written
                        escaped = true;
                    }
                    else if (thisChar == delimiter)
                    {
                        // encountering delimiter; add current buffer to results
                        results.Add(new string(buffer, 0, pos));
                        pos = 0;
                    }
                    else
                    {
                        // normal character; just print
                        buffer[pos++] = thisChar;
                    }
                }
            }

            // If last char was the escape char, add it to the end of the last string.
            // If this happens, the string was actually malformed, but whatever.
            if (escaped)
            {
                buffer[pos++] = escapeChar;
            }

            // add the last string to the collection
            if (pos != 0)
            {
                results.Add(new string(buffer, 0, pos));
            }

            return results;
        }

        /// <summary>
        /// Should be faster than string.StartsWith(string) because this ignores culture info.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <param name="prefix">The prefix.</param>
        /// <returns><c>true</c> if the string value starts with the specified prefix, <c>false</c> otherwise.</returns>
        public static bool StartsWith(string stringValue, string prefix) =>
            stringValue.Length >= prefix.Length && !prefix.Where((t, i) => stringValue[i] != t).Any();

        /// <summary>
        /// Strips the single line feeds.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <returns>The stripped string.</returns>
        public static string StripSingleLineFeeds(string val) =>
            StringHelper.StripSingleLineFeedsRegex.Replace(StringHelper.StripSpacesRegex.Replace(val, "\r\n"), " ");

        /// <summary>
        /// Converts the specified string to boolean or the specified default value.
        /// </summary>
        /// <param name="boolValue">The boolean value.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <returns><c>true</c> if successfully converted, <c>false</c> otherwise.</returns>
        public static bool ToBool(string boolValue, bool defaultValue)
        {
            if (boolValue == null)
            {
                return defaultValue;
            }

            switch (boolValue.Trim().ToUpperInvariant())
            {
                case "YES":
                case "TRUE":
                case "1":
                    return true;

                case "NO":
                case "FALSE":
                case "0":
                    return false;
            }

            return defaultValue;
        }

        /// <summary>
        /// Formats the number.
        /// </summary>
        /// <param name="num">The number.</param>
        /// <returns>The formatted number.</returns>
        private static string FormatNum(double num)
        {
            var nf = (NumberFormatInfo)NumberFormatInfo.CurrentInfo.Clone();
            if (num >= 100)
            {
                nf.NumberDecimalDigits = 0;
            }
            else if (num >= 10)
            {
                nf.NumberDecimalDigits = 1;
            }
            else if (num >= 1)
            {
                nf.NumberDecimalDigits = 2;
            }
            else
            {
                nf.NumberDecimalDigits = 3;
            }

            return num.ToString("N", nf);
        }

        /// <summary>
        /// Gets the indent.
        /// </summary>
        /// <param name="strVal">The string value.</param>
        /// <returns>The indent.</returns>
        private static string GetIndent(string strVal)
        {
            int pos;
            for (pos = 0; pos < strVal.Length; pos++)
            {
                switch (strVal[pos])
                {
                    case ' ':
                    case '\t':
                        break;
                    default:
                        return strVal.Substring(0, pos);
                }
            }

            return strVal; // all whitespace
        }
    }
}
