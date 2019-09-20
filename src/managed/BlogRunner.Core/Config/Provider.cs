// <copyright file="Provider.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The provider class.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the blog.
        /// </summary>
        /// <value>The blog.</value>
        [XmlElement(ElementName = "blog")]
        public Blog Blog { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the client.
        /// </summary>
        /// <value>The type of the client.</value>
        /// <remarks>
        /// This comes from the BlogProviders.xml definitions instead.
        /// </remarks>
        [XmlIgnore]
        public string ClientType { get; set; }

        /// <summary>
        /// Gets or sets the overrides.
        /// </summary>
        /// <value>The overrides.</value>
        [XmlElement(ElementName = "overrides")]
        public XmlElement Overrides { get; set; }

        /// <summary>
        /// Gets or sets the exclude.
        /// </summary>
        /// <value>The exclude.</value>
        [XmlElement(ElementName = "exclude")]
#pragma warning disable CA1819 // Properties should not return arrays
        public string[] Exclude { get; set; } = Array.Empty<string>();
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
