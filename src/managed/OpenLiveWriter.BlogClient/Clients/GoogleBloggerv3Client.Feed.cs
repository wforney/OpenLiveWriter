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
        /// The Feed class.
        /// </summary>
        public class Feed
        {
            /// <summary>
            /// Gets or sets the category array.
            /// </summary>
            /// <value>The category array.</value>
            [JsonProperty("category")]
            public Category[] CategoryArray { get; set; }
        }
    }
}
