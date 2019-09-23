// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Xml;

    public abstract partial class AtomProtocolVersion
    {
        /// <summary>
        /// The Atom10DraftBloggerProtocolVersion class.
        /// Implements the <see cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion.Atom10DraftProtocolVersion" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.BlogClient.Clients.AtomProtocolVersion.Atom10DraftProtocolVersion" />
        private class Atom10DraftBloggerProtocolVersion : Atom10DraftProtocolVersion
        {
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
                // Blogger doesn't support category labels and will error out if you pass them
                var el = base.CreateCategoryElement(ownerDoc, category, categoryScheme, categoryLabel);
                el.RemoveAttribute("label");
                return el;
            }
        }
    }
}
