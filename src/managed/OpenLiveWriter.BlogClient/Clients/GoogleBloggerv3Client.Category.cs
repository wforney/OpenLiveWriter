// <copyright file="GoogleBloggerv3Client.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.BlogClient.Clients
{

    using Newtonsoft.Json;

    public partial class GoogleBloggerv3Client
    {
        /// <summary>
        /// The Category class.
        /// </summary>
        public class Category
        {
            /// <summary>
            /// Gets or sets the term.
            /// </summary>
            /// <value>The term.</value>
            [JsonProperty("term")]
            public string Term { get; set; }
        }
    }
}
