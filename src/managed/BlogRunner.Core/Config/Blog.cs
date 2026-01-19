// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Xml.Serialization;

namespace BlogRunner.Core.Config
{
    public class Blog
    {
        [XmlElement(ElementName = "homepageUrl")]
        public string HomepageUrl { get; set; }

        [XmlElement(ElementName = "username")]
        public string Username { get; set; }

        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        [XmlElement(ElementName = "apiUrl")]
        public string ApiUrl { get; set; }

        [XmlElement(ElementName = "blogId")]
        public string BlogId { get; set; }
    }

    public enum BlogApi { XmlRpc, AtomPub }
}
