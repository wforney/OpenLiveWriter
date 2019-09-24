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
        /// The CategoryResponse class.
        /// </summary>
        public class CategoryResponse
        {
            /// <summary>
            /// Gets or sets the feed.
            /// </summary>
            /// <value>The feed.</value>
            [JsonProperty("feed")]
            public Feed Feed { get; set; }
        }
    }
}
