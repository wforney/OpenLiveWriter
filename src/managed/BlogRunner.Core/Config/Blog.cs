﻿// <copyright file="Blog.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Serialization;

    /// <summary>
    /// The blog class.
    /// </summary>
    public class Blog
    {
        /// <summary>
        /// Gets or sets the homepage URL.
        /// </summary>
        /// <value>The homepage URL.</value>
        [XmlElement(ElementName = "homepageUrl")]
        [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string HomepageUrl { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        [XmlElement(ElementName = "username")]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        [XmlElement(ElementName = "password")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the API URL.
        /// </summary>
        /// <value>The API URL.</value>
        [XmlElement(ElementName = "apiUrl")]
        [SuppressMessage("Design", "CA1056:Uri properties should not be strings", Justification = "<Pending>")]
        public string ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets the blog identifier.
        /// </summary>
        /// <value>The blog identifier.</value>
        [XmlElement(ElementName = "blogId")]
        public string BlogId { get; set; }
    }
}
