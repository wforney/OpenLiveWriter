// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Xml;

    using OpenLiveWriter.Extensibility.BlogClient;

    public abstract partial class AtomProtocolVersion
    {
        /// <summary>
        /// The Atom10ProtocolVersion class.
        /// Implements the <see cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion" />
        private class Atom10ProtocolVersion : AtomProtocolVersion
        {
            /// <summary>
            /// Gets the atom pub namespace URI.
            /// </summary>
            /// <value>The atom pub namespace URI.</value>
            public override string AtomPubNamespaceUri => "http://www.w3.org/2007/app";

            /// <summary>
            /// Gets the namespace URI.
            /// </summary>
            /// <value>The namespace URI.</value>
            public override string NamespaceUri => "http://www.w3.org/2005/Atom";

            /// <summary>
            /// Gets the name of the published el.
            /// </summary>
            /// <value>The name of the published el.</value>
            public override string PublishedElName => "published";

            /// <summary>
            /// Gets the pub namespace URI.
            /// </summary>
            /// <value>The pub namespace URI.</value>
            public override string PubNamespaceUri => "http://www.w3.org/2007/app";

            /// <summary>
            /// Gets the name of the updated el.
            /// </summary>
            /// <value>The name of the updated el.</value>
            public override string UpdatedElName => "updated";

            /// <summary>
            /// Creates the category element.
            /// </summary>
            /// <param name="ownerDoc">The owner document.</param>
            /// <param name="category">The category.</param>
            /// <param name="categoryScheme">The category scheme.</param>
            /// <param name="categoryLabel">The category label.</param>
            /// <returns>XmlElement.</returns>
            /// <exception cref="ArgumentException">Null category scheme not supported</exception>
            public override XmlElement CreateCategoryElement(
                XmlDocument ownerDoc,
                string category,
                string categoryScheme,
                string categoryLabel)
            {
                if (categoryScheme == null)
                {
                    throw new ArgumentException("Null category scheme not supported");
                }

                var element = ownerDoc.CreateElement("atom", "category", this.NamespaceUri);
                element.SetAttribute("term", category);
                if (categoryScheme.Length > 0)
                {
                    element.SetAttribute("scheme", categoryScheme);
                }

                element.SetAttribute("label", categoryLabel);
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
                if (categoryScheme == null)
                {
                    return new BlogPostCategory[0];
                }

                var nsMgr = new XmlNamespaceManager(new NameTable());
                nsMgr.AddNamespace("atom", this.NamespaceUri);

                var results = new ArrayList();
                foreach (XmlElement el in entry.SelectNodes("atom:category", nsMgr))
                {
                    if (!this.SchemesEqual(el.GetAttribute("scheme"), categoryScheme))
                    {
                        continue;
                    }

                    var term = el.GetAttribute("term");
                    var label = el.GetAttribute("label");

                    var noTerm = term == null || term == string.Empty;
                    var noLabel = label == null || label == string.Empty;

                    if (noTerm && noLabel)
                    {
                        continue;
                    }

                    if (noTerm)
                    {
                        term = label;
                    }

                    if (noLabel)
                    {
                        label = term;
                    }

                    results.Add(new BlogPostCategory(term, label));
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
                el.SetAttribute("type", "html");
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
                el.SetAttribute("type", "text");
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
                if (categoryScheme == null)
                {
                    return;
                }

                var nsMgr = new XmlNamespaceManager(new NameTable());
                nsMgr.AddNamespace("atom", this.NamespaceUri);
                var nodesToRemove = new ArrayList();
                foreach (XmlElement categoryEl in node.SelectNodes("atom:category", nsMgr))
                {
                    var scheme = categoryEl.GetAttribute("scheme");
                    if (this.SchemesEqual(scheme, categoryScheme))
                    {
                        nodesToRemove.Add(categoryEl);
                    }
                }

                foreach (XmlElement categoryEl in nodesToRemove)
                {
                    categoryEl.ParentNode.RemoveChild(categoryEl);
                }
            }

            /// <summary>
            /// Texts the node to HTML.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns>System.String.</returns>
            public override string TextNodeToHtml(XmlElement node) => Atom10ProtocolVersion.ToTextValue(node).ToHTML();

            /// <summary>
            /// Texts the node to plaintext.
            /// </summary>
            /// <param name="node">The node.</param>
            /// <returns>System.String.</returns>
            public override string TextNodeToPlaintext(XmlElement node) =>
                Atom10ProtocolVersion.ToTextValue(node).ToText();

            /// <summary>
            /// Converts to textvalue.
            /// </summary>
            /// <param name="target">The target.</param>
            /// <returns>AtomContentValue.</returns>
            private static AtomContentValue ToTextValue(XmlElement target)
            {
                var type = "text";
                var attrType = target.Attributes["type"];
                if (attrType != null)
                {
                    type = attrType.Value;
                }

                switch (type)
                {
                    case "html":
                        return new AtomContentValue(AtomContentValueType.HTML, target.InnerText.Trim());
                    case "xhtml":
                        {
                            var nsMgr = new XmlNamespaceManager(new NameTable());
                            nsMgr.AddNamespace("xhtml", "http://www.w3.org/1999/xhtml");

                            var div = target.SelectSingleNode("xhtml:div", nsMgr);
                            if (div != null)
                            {
                                return new AtomContentValue(AtomContentValueType.XHTML, div.InnerXml);
                            }

                            return new AtomContentValue(AtomContentValueType.XHTML, string.Empty);
                        }

                    default:
                    case "text":
                        return new AtomContentValue(AtomContentValueType.Text, target.InnerText.Trim());
                }
            }

            /// <summary>
            /// Schemeses the equal.
            /// </summary>
            /// <param name="scheme1">The scheme1.</param>
            /// <param name="scheme2">The scheme2.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            private bool SchemesEqual(string scheme1, string scheme2) => string.Equals(scheme1, scheme2);
        }
    }
}
