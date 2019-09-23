// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The AtomEntry class.
    /// </summary>
    internal class AtomEntry
    {
        /// <summary>
        /// The atom namespace
        /// </summary>
        private readonly Namespace atomNamespace;

        /// <summary>
        /// The atom version
        /// </summary>
        private readonly AtomProtocolVersion atomVersion;

        /// <summary>
        /// The category scheme
        /// </summary>
        private readonly string categoryScheme;

        /// <summary>
        /// The document URI
        /// </summary>
        private readonly Uri documentUri;

        /// <summary>
        /// The entry node
        /// </summary>
        private readonly XmlElement entryNode;

        /// <summary>The namespace manager</summary>
        private readonly XmlNamespaceManager namespaceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomEntry"/> class.
        /// </summary>
        /// <param name="atomVersion">The atom version.</param>
        /// <param name="atomNamespace">The atom namespace.</param>
        /// <param name="categoryScheme">The category scheme.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <param name="documentUri">The document URI.</param>
        /// <param name="entryNode">The entry node.</param>
        public AtomEntry(
            AtomProtocolVersion atomVersion,
            Namespace atomNamespace,
            string categoryScheme,
            XmlNamespaceManager namespaceManager,
            Uri documentUri,
            XmlElement entryNode)
        {
            this.atomVersion = atomVersion;
            this.categoryScheme = categoryScheme;
            this.namespaceManager = namespaceManager;
            this.documentUri = documentUri;
            this.entryNode = entryNode;
            this.atomNamespace = atomNamespace;
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>The categories.</value>
        public BlogPostCategory[] Categories =>
            this.atomVersion.ExtractCategories(this.entryNode, this.categoryScheme, this.documentUri);

        /// <summary>
        /// Gets or sets the content HTML.
        /// </summary>
        /// <value>The content HTML.</value>
        public string ContentHtml
        {
            get
            {
                var contentEl = this.GetElement("atom:content");
                if (contentEl != null)
                {
                    return this.atomVersion.TextNodeToHtml(contentEl);
                }

                return string.Empty;
            }

            set
            {
                this.RemoveNodes("content", this.atomNamespace);
                var contentNode = this.atomVersion.HtmlToTextNode(this.entryNode.OwnerDocument, value + " ");
                this.entryNode.AppendChild(contentNode);
            }
        }

        /// <summary>
        /// Gets the edit URI.
        /// </summary>
        /// <value>The edit URI.</value>
        public string EditUri => this.GetUrl("atom:link[@rel='edit']/@href", string.Empty);

        /// <summary>
        /// Gets or sets the excerpt.
        /// </summary>
        /// <value>The excerpt.</value>
        public string Excerpt
        {
            get => this.GetTextNodePlaintext("atom:summary", string.Empty);
            set => this.PopulateElement(value, this.atomNamespace, "summary");
        }

        /// <summary>
        /// Gets the permalink.
        /// </summary>
        /// <value>The permalink.</value>
        public string Permalink =>
            this.GetUrl("atom:link[@rel='alternate' and (@type='text/html' or not(@type))]/@href", string.Empty);

        /// <summary>
        /// Gets or sets the publish date.
        /// </summary>
        /// <value>The publish date.</value>
        public DateTime PublishDate
        {
            get => this.GetPublishDate(DateTime.MinValue);
            set
            {
                if (value == DateTime.MinValue)
                {
                    this.RemoveNodes(this.atomVersion.PublishedElName, this.atomNamespace);
                }
                else
                {
                    this.PopulateElement(this.FormatRfc3339(value), this.atomNamespace, this.atomVersion.PublishedElName);
                }
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            get => this.GetTextNodePlaintext("atom:title", string.Empty);
            set => this.PopulateElement(value, this.atomNamespace, "title");
        }

        /// <summary>Gets the link.</summary>
        /// <param name="entryElement">The entry element.</param>
        /// <param name="namespaceManager">The namespace manager.</param>
        /// <param name="relative">The relative.</param>
        /// <param name="contentType">e.g. application/atom+xml</param>
        /// <param name="contentSubType">e.g. "entry"</param>
        /// <param name="baseUri">The base URI.</param>
        /// <returns>The link.</returns>
        public static string GetLink(
            XmlElement entryElement,
            XmlNamespaceManager namespaceManager,
            string relative,
            string contentType,
            string contentSubType,
            Uri baseUri)
        {
            Debug.Assert(
                contentSubType == null || contentType != null,
                "contentSubType is only used if contentType is also provided");

            var xpath = string.Format(CultureInfo.InvariantCulture, @"atom:link[@rel='{0}']", relative);
            foreach (XmlElement link in entryElement.SelectNodes(xpath, namespaceManager))
            {
                if (contentType != null)
                {
                    var mimeType = link.GetAttribute("type");
                    if (string.IsNullOrEmpty(mimeType))
                    {
                        continue;
                    }

                    var mimeData = MimeHelper.ParseContentType(mimeType, true);
                    if (contentType != (string)mimeData[string.Empty])
                    {
                        continue;
                    }

                    if (contentSubType != null && contentSubType != (string)mimeData["type"])
                    {
                        continue;
                    }
                }

                return XmlHelper.GetUrl(link, "@href", baseUri);
            }

            return string.Empty;
        }

        /// <summary>
        /// Adds the category.
        /// </summary>
        /// <param name="category">The category.</param>
        public void AddCategory(BlogPostCategory category)
        {
            var catEl = this.atomVersion.CreateCategoryElement(
                this.entryNode.OwnerDocument,
                category.Id,
                this.categoryScheme,
                category.Name);
            this.entryNode.AppendChild(catEl);
        }

        /// <summary>
        /// Clears the categories.
        /// </summary>
        public void ClearCategories() => this.atomVersion.RemoveAllCategories(this.entryNode, this.categoryScheme, this.documentUri);

        /// <summary>
        /// Generates the identifier.
        /// </summary>
        public void GenerateId()
        {
            if (this.entryNode.SelectSingleNode("atom:id", this.namespaceManager) == null)
            {
                this.PopulateElement("urn:uuid:" + Guid.NewGuid().ToString("d"), this.atomNamespace, "id");
            }
        }

        /// <summary>
        /// Generates the updated.
        /// </summary>
        public void GenerateUpdated()
        {
            if (this.entryNode.SelectSingleNode("atom:updated", this.namespaceManager) == null)
            {
                this.PopulateElement(this.FormatRfc3339(DateTime.Now), this.atomNamespace, "updated");
            }
        }

        /// <summary>
        /// Formats the RFC3339.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>System.String.</returns>
        private string FormatRfc3339(DateTime dateTime)
        {
            var offset = DateTimeHelper.GetUtcOffset(dateTime);
            var dt = new StringBuilder(
                (dateTime + offset).ToString(@"yyyy-MM-ddTHH:mm:ss", DateTimeFormatInfo.InvariantInfo));
            char direction;
            if (offset >= TimeSpan.Zero)
            {
                direction = '+';
            }
            else
            {
                direction = '-';
                offset = -offset;
            }

            dt.AppendFormat(CultureInfo.InvariantCulture, "{0}{1:d2}:{2:d2}", direction, offset.Hours, offset.Minutes);
            return dt.ToString();
        }

        /// <summary>
        /// Gets the element.
        /// </summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns>XmlElement.</returns>
        private XmlElement GetElement(string xpath) => this.entryNode.SelectSingleNode(xpath, this.namespaceManager) as XmlElement;

        /// <summary>
        /// Gets the publish date.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>DateTime.</returns>
        private DateTime GetPublishDate(DateTime defaultValue)
        {
            var target = this.entryNode.SelectSingleNode("atom:published", this.namespaceManager);
            if (target == null)
            {
                return defaultValue;
            }

            var val = target.InnerText;
            if (val == null)
            {
                return defaultValue;
            }

            val = val.Trim();
            if (val.Length == 0)
            {
                return defaultValue;
            }

            return this.ParseRfc3339(val);
        }

        /// <summary>
        /// Gets the text node plaintext.
        /// </summary>
        /// <param name="xpath">The xpath.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>The text.</returns>
        private string GetTextNodePlaintext(string xpath, string defaultValue)
        {
            var el = this.GetElement(xpath);
            return el == null ? defaultValue : this.atomVersion.TextNodeToPlaintext(el);
        }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        /// <param name="xpath">The xpath.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>System.String.</returns>
        private string GetUrl(string xpath, string defaultValue)
        {
            var val = XmlHelper.GetUrl(this.entryNode, xpath, this.namespaceManager, this.documentUri);
            return val ?? defaultValue;
        }

        /// <summary>
        /// Parses the match int.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <param name="label">The label.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private int ParseMatchInt(Match m, string label, int min, int max)
        {
            var val = int.Parse(m.Groups[label].Value, CultureInfo.InvariantCulture);
            if (val < min || val > max)
            {
                throw new ArgumentOutOfRangeException(label);
            }

            return val;
        }

        /// <summary>
        /// Parses the RFC3339.
        /// </summary>
        /// <param name="dateTimeString">The date time string.</param>
        /// <returns>The <see cref="DateTime"/>.</returns>
        private DateTime ParseRfc3339(string dateTimeString)
        {
            var m = Regex.Match(
                dateTimeString,
                @"
^
(?<year>\d{4})
-
(?<month>\d{2})
-
(?<day>\d{2})
T
(?<hour>\d{2})
\:
(?<minute>\d{2})
\:
(?<second>\d{2})
(\. (?<fraction>\d+) )?
(?<timezone>
    (?<utc>Z)
    |
    (?<offset>
        (?<offdir>[+-])
        (?<offhour>\d{2})
        (?:
            \:?
            (?<offmin>\d{2})
        )?
    )
)
$",
                RegexOptions.IgnorePatternWhitespace);

            var year = this.ParseMatchInt(m, "year", 0, 9999);
            var month = this.ParseMatchInt(m, "month", 1, 12);
            var day = this.ParseMatchInt(m, "day", 1, 31);
            var hour = this.ParseMatchInt(m, "hour", 0, 23);
            var minute = this.ParseMatchInt(m, "minute", 0, 59);
            var second = this.ParseMatchInt(m, "second", 0, 60); // leap seconds

            var millis = 0;
            if (m.Groups["fraction"].Success)
            {
                millis = (int)(1000 * double.Parse("0." + m.Groups["fraction"].Value, CultureInfo.InvariantCulture));
            }

            var utc = m.Groups["utc"].Success;
            var dt = new DateTime(year, month, day, hour, minute, second, millis);

            if (!utc)
            {
                var direction = m.Groups["offdir"].Value == "-" ? 1 : -1; // reverse because we are trying to get to UTC
                var offhour = int.Parse(m.Groups["offhour"].Value, CultureInfo.InvariantCulture) * direction;
                var offmin = int.Parse(m.Groups["offmin"].Value, CultureInfo.InvariantCulture) * direction;
                dt = dt.AddHours(offhour);
                dt = dt.AddMinutes(offmin);
            }

            return dt;
        }

        /// <summary>
        /// Populates the element.
        /// </summary>
        /// <param name="val">The value.</param>
        /// <param name="ns">The namespace.</param>
        /// <param name="localName">Name of the local.</param>
        /// <returns>An <see cref="XmlNode"/>.</returns>
        private XmlNode PopulateElement(string val, Namespace ns, string localName)
        {
            this.RemoveNodes(localName, ns);

            if (val == null)
            {
                return null;
            }

            XmlNode newNode = this.entryNode.OwnerDocument?.CreateElement(ns.Prefix, localName, ns.Uri);
            if (newNode == null)
            {
                return null;
            }

            newNode.InnerText = val;
            this.entryNode.AppendChild(newNode);
            return newNode;
        }

        /// <summary>
        /// Removes the nodes.
        /// </summary>
        /// <param name="localName">Name of the local.</param>
        /// <param name="ns">The namespace.</param>
        private void RemoveNodes(string localName, Namespace ns)
        {
            var nodes = this.entryNode.SelectNodes($"./{ns.Prefix}:{localName}", this.namespaceManager)
                           ?.Cast<XmlNode>().ToList() ?? new List<XmlNode>();

            foreach (var n in nodes)
            {
                n.ParentNode?.RemoveChild(n);
            }
        }
    }
}
