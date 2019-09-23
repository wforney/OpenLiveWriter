// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Text;
    using System.Xml;

    using OpenLiveWriter.Extensibility.BlogClient;

    public abstract partial class AtomProtocolVersion
    {
        /// <summary>
        /// The Atom03ProtocolVersion class.
        /// Implements the <see cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion" />
        private class Atom03ProtocolVersion : AtomProtocolVersion
        {
            /// <summary>
            /// The dc URI
            /// </summary>
            private const string DC_URI = "http://purl.org/dc/elements/1.1/";

            /// <summary>
            /// Gets the atom pub namespace URI.
            /// </summary>
            /// <value>The atom pub namespace URI.</value>
            public override string AtomPubNamespaceUri => "http://purl.org/atom/app#";

            /// <summary>
            /// Gets the namespace URI.
            /// </summary>
            /// <value>The namespace URI.</value>
            public override string NamespaceUri => "http://purl.org/atom/ns#";

            /// <summary>
            /// Gets the name of the published el.
            /// </summary>
            /// <value>The name of the published el.</value>
            public override string PublishedElName => "issued";

            /// <summary>
            /// Gets the pub namespace URI.
            /// </summary>
            /// <value>The pub namespace URI.</value>
            public override string PubNamespaceUri => "http://example.net/appns/";

            /// <summary>
            /// Gets the name of the updated el.
            /// </summary>
            /// <value>The name of the updated el.</value>
            public override string UpdatedElName => "modified";

            /// <summary>
            /// Creates the category element.
            /// </summary>
            /// <param name="ownerDoc">The owner document.</param>
            /// <param name="category">The category.</param>
            /// <param name="categoryScheme">The category scheme.</param>
            /// <param name="categoryLabel">The category label.</param>
            /// <returns>XmlElement.</returns>
            public override XmlElement CreateCategoryElement(
                XmlDocument ownerDoc,
                string category,
                string categoryScheme,
                string categoryLabel)
            {
                var element = ownerDoc.CreateElement("dc", "subject", Atom03ProtocolVersion.DC_URI);
                element.InnerText = category;
                return element;
            }

            /// <summary>
            /// Extracts the categories.
            /// </summary>
            /// <param name="entry">The entry.</param>
            /// <param name="categoryScheme">The category scheme.</param>
            /// <param name="documentUri">The document URI.</param>
            /// <returns>BlogPostCategory[].</returns>
            public override BlogPostCategory[] ExtractCategories(
                XmlElement entry,
                string categoryScheme,
                Uri documentUri)
            {
                var nsMgr = new XmlNamespaceManager(new NameTable());
                nsMgr.AddNamespace("dc", Atom03ProtocolVersion.DC_URI);

                var results = new ArrayList();

                foreach (XmlElement el in entry.SelectNodes("dc:subject", nsMgr))
                {
                    var subject = el.InnerText;
                    if (subject != string.Empty)
                    {
                        results.Add(new BlogPostCategory(subject, subject));
                    }
                }

                return (BlogPostCategory[])results.ToArray(typeof(BlogPostCategory));
            }

            /// <summary>
            /// HTMLs to text node.
            /// </summary>
            /// <param name="ownerDoc">The owner document.</param>
            /// <param name="html">The HTML.</param>
            /// <returns>XmlElement.</returns>
            public override XmlElement HtmlToTextNode(XmlDocument ownerDoc, string html)
            {
                var el = ownerDoc.CreateElement("atom", "content", this.NamespaceUri);
                el.SetAttribute("type", "text/html");
                el.SetAttribute("mode", "escaped");
                el.InnerText = html;
                return el;
            }

            /// <summary>
            /// Plaintexts to text node.
            /// </summary>
            /// <param name="ownerDoc">The owner document.</param>
            /// <param name="text">The text.</param>
            /// <returns>XmlElement.</returns>
            public override XmlElement PlaintextToTextNode(XmlDocument ownerDoc, string text)
            {
                var el = ownerDoc.CreateElement("atom", "content", this.NamespaceUri);
                el.SetAttribute("type", "text/plain");
                el.SetAttribute("mode", "escaped");
                el.InnerText = text;
                return el;
            }

            /// <summary>
            /// Removes all categories.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <param name="categoryScheme">The category scheme.</param>
            /// <param name="documentUri">The document URI.</param>
            public override void RemoveAllCategories(XmlNode node, string categoryScheme, Uri documentUri)
            {
                var nsMgr = new XmlNamespaceManager(new NameTable());
                nsMgr.AddNamespace("dc", Atom03ProtocolVersion.DC_URI);
                XmlNode category;
                while (null != (category = node.SelectSingleNode("dc:subject", nsMgr)))
                {
                    category.ParentNode.RemoveChild(category);
                }
            }

            /// <summary>
            /// Texts the node to HTML.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns>System.String.</returns>
            public override string TextNodeToHtml(XmlElement node) => Atom03ProtocolVersion.ToTextValue(node).ToHTML();

            /// <summary>
            /// Texts the node to plaintext.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns>System.String.</returns>
            public override string TextNodeToPlaintext(XmlElement node) =>
                Atom03ProtocolVersion.ToTextValue(node).ToText();

            /// <summary>
            /// Converts to textvalue.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns>AtomContentValue.</returns>
            private static AtomContentValue ToTextValue(XmlElement node)
            {
                var type = node.GetAttribute("type");

                var mode = node.GetAttribute("mode");
                if (mode == string.Empty)
                {
                    mode = "xml";
                }

                string content;
                switch (mode)
                {
                    case "escaped":
                        content = node.InnerText;
                        break;
                    case "base64":
                        content = Encoding.UTF8.GetString(Convert.FromBase64String(node.InnerText));
                        break;
                    case "xml":
                    default:
                        content = node.InnerXml;
                        if (type == string.Empty && node.SelectSingleNode("./*") != null)
                        {
                            type = "application/xhtml+xml";
                        }

                        break;
                }

                AtomContentValue tv;
                switch (type)
                {
                    case "text/html":
                        tv = new AtomContentValue(AtomContentValueType.HTML, content);
                        break;
                    case "application/xhtml+xml":
                        var nsMgr = new XmlNamespaceManager(new NameTable());
                        nsMgr.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");

                        if (mode == "xml")
                        {
                            var div = node.SelectSingleNode("xhtml:div", nsMgr);
                            tv = div == null
                                ? new AtomContentValue(AtomContentValueType.XHTML, string.Empty)
                                : new AtomContentValue(AtomContentValueType.XHTML, div.InnerXml);
                        }
                        else
                        {
                            tv = new AtomContentValue(AtomContentValueType.XHTML, content);
                        }

                        break;
                    case "text/plain":
                    default:
                        tv = new AtomContentValue(AtomContentValueType.Text, content);
                        break;
                }

                return tv;
            }
        }
    }
}
