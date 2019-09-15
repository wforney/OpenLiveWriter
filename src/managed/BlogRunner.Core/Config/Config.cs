// <copyright file="Config.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// The configuration class.
    /// </summary>
    [XmlRoot(ElementName = "config")]
    public class Config
    {
        /// <summary>
        /// Gets or sets the providers.
        /// </summary>
        /// <value>The providers.</value>
        [XmlArray(ElementName = "providers")]
        [XmlArrayItem(ElementName = "provider")]
        public Provider[] Providers { get; set; }

        /// <summary>
        /// Loads the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="providersPath">The providers path.</param>
        /// <returns>Config.</returns>
        /// <exception cref="System.ArgumentException">Unknown provider ID: " + providerId.</exception>
        public static Config Load(string path, string providersPath)
        {
            var ser = new XmlSerializer(typeof(Config));
            Config config;
            using (Stream s = File.OpenRead(path))
            {
                config = (Config)ser.Deserialize(s);
            }

            var providersXml = new XmlDocument();
            providersXml.Load(providersPath);

            foreach (var p in config.Providers)
            {
                var providerId = p.Id;
                var el = (XmlText)providersXml.SelectSingleNode("/providers/provider/id[text()='" + providerId + "']/../clientType/text()");
                if (el == null)
                {
                    Console.Error.WriteLine("Unknown provider ID: " + providerId);
                    throw new ArgumentException("Unknown provider ID: " + providerId);
                }

                p.ClientType = el.Value;
            }

            return config;
        }

        /// <summary>
        /// Gets the provider by identifier.
        /// </summary>
        /// <param name="providerId">The provider identifier.</param>
        /// <returns>Provider.</returns>
        public Provider GetProviderById(string providerId)
        {
            foreach (var p in this.Providers)
            {
                if (p.Id == providerId)
                {
                    return p;
                }
            }

            return null;
        }
    }
}
