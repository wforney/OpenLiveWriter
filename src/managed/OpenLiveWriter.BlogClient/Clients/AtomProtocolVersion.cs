// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Xml;

    using OpenLiveWriter.Extensibility.BlogClient;

    // see http://rakaz.nl/item/moving_from_atom_03_to_10
    /// <summary>
    /// The AtomProtocolVersion class.
    /// </summary>
    public abstract partial class AtomProtocolVersion
    {
        /// <summary>
        /// The V03
        /// </summary>
        private static AtomProtocolVersion v03 = new Atom03ProtocolVersion();

        /// <summary>
        /// The V10
        /// </summary>
        private static AtomProtocolVersion v10 = new Atom10ProtocolVersion();

        /// <summary>
        /// The V10 draft
        /// </summary>
        private static AtomProtocolVersion v10Draft = new Atom10DraftProtocolVersion();

        /// <summary>
        /// The V10 draft blogger
        /// </summary>
        private static AtomProtocolVersion v10DraftBlogger = new Atom10DraftBloggerProtocolVersion();

        /// <summary>
        /// Gets the V03.
        /// </summary>
        /// <value>The V03.</value>
        public static AtomProtocolVersion V03
        {
            get => AtomProtocolVersion.v03;
            private set => AtomProtocolVersion.v03 = value;
        }

        /// <summary>
        /// Gets the V10.
        /// </summary>
        /// <value>The V10.</value>
        public static AtomProtocolVersion V10
        {
            get => AtomProtocolVersion.v10;
            private set => AtomProtocolVersion.v10 = value;
        }

        /// <summary>
        /// Gets the V10 draft.
        /// </summary>
        /// <value>The V10 draft.</value>
        public static AtomProtocolVersion V10Draft
        {
            get => AtomProtocolVersion.v10Draft;
            private set => AtomProtocolVersion.v10Draft = value;
        }

        /// <summary>
        /// Gets the V10 draft blogger.
        /// </summary>
        /// <value>The V10 draft blogger.</value>
        public static AtomProtocolVersion V10DraftBlogger
        {
            get => AtomProtocolVersion.v10DraftBlogger;
            private set => AtomProtocolVersion.v10DraftBlogger = value;
        }

        /// <summary>
        /// Gets the atom pub namespace URI.
        /// </summary>
        /// <value>The atom pub namespace URI.</value>
        public abstract string AtomPubNamespaceUri { get; }

        /// <summary>
        /// Gets the namespace URI.
        /// </summary>
        /// <value>The namespace URI.</value>
        public abstract string NamespaceUri { get; }

        /// <summary>
        /// Gets the name of the published el.
        /// </summary>
        /// <value>The name of the published el.</value>
        public abstract string PublishedElName { get; }

        /// <summary>
        /// Gets the pub namespace URI.
        /// </summary>
        /// <value>The pub namespace URI.</value>
        public abstract string PubNamespaceUri { get; }

        /// <summary>
        /// Gets the name of the updated el.
        /// </summary>
        /// <value>The name of the updated el.</value>
        public abstract string UpdatedElName { get; }

        /// <summary>
        /// Creates the category element.
        /// </summary>
        /// <param name="ownerDoc">The owner document.</param>
        /// <param name="category">The category.</param>
        /// <param name="categoryScheme">The category scheme.</param>
        /// <param name="categoryLabel">The category label.</param>
        /// <returns>XmlElement.</returns>
        public abstract XmlElement CreateCategoryElement(
            XmlDocument ownerDoc,
            string category,
            string categoryScheme,
            string categoryLabel);

        /// <summary>
        /// Extracts the categories.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="categoryScheme">The category scheme.</param>
        /// <param name="documentUri">The document URI.</param>
        /// <returns>BlogPostCategory[].</returns>
        public abstract BlogPostCategory[] ExtractCategories(XmlElement entry, string categoryScheme, Uri documentUri);

        /// <summary>
        /// HTMLs to text node.
        /// </summary>
        /// <param name="ownerDoc">The owner document.</param>
        /// <param name="html">The HTML.</param>
        /// <returns>XmlElement.</returns>
        public abstract XmlElement HtmlToTextNode(XmlDocument ownerDoc, string html);

        /// <summary>
        /// Plaintexts to text node.
        /// </summary>
        /// <param name="ownerDoc">The owner document.</param>
        /// <param name="text">The text.</param>
        /// <returns>XmlElement.</returns>
        public abstract XmlElement PlaintextToTextNode(XmlDocument ownerDoc, string text);

        /// <summary>
        /// Removes all categories.
        /// </summary>
        /// <param name="entryNode">The entry node.</param>
        /// <param name="categoryScheme">The category scheme.</param>
        /// <param name="documentUri">The document URI.</param>
        public abstract void RemoveAllCategories(XmlNode entryNode, string categoryScheme, Uri documentUri);

        /// <summary>
        /// Texts the node to HTML.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>System.String.</returns>
        public abstract string TextNodeToHtml(XmlElement node);

        /// <summary>
        /// Texts the node to plaintext.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>System.String.</returns>
        public abstract string TextNodeToPlaintext(XmlElement node);
    }
}
