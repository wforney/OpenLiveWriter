// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Diagnostics;

    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The blog post content filters class.
    /// </summary>
    public partial class BlogPostContentFilters
    {
        /// <summary>
        /// The content filters
        /// </summary>
        private static readonly Hashtable ContentFilters = new Hashtable();

        /// <inheritdoc />
        static BlogPostContentFilters()
        {
            BlogPostContentFilters.AddContentFilter(typeof(LineBreak2PBRInputFormatter));
            BlogPostContentFilters.AddContentFilter(typeof(LineBreak2BRInputFormatter));
            BlogPostContentFilters.AddContentFilter(typeof(WordPressInputFormatter));
        }

        /// <inheritdoc />
        private BlogPostContentFilters()
        {
        }

        /// <summary>
        /// Adds the content filter.
        /// </summary>
        /// <param name="filter">The filter.</param>
        public static void AddContentFilter(Type filter)
        {
            try
            {
                var definition = new ContentFilterTypeDefinition(filter);
                BlogPostContentFilters.ContentFilters.Add(definition.Name, definition);
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception adding Content Filter: " + ex);
            }
        }

        /// <summary>
        /// Creates the content filter.
        /// </summary>
        /// <param name="filterName">Name of the filter.</param>
        /// <returns>An <see cref="IBlogPostContentFilter"/>.</returns>
        public static IBlogPostContentFilter CreateContentFilter(string filterName)
        {
            var cftDefinition = BlogPostContentFilters.ContentFilters[filterName] as ContentFilterTypeDefinition;
            return (IBlogPostContentFilter)cftDefinition.Constructor.Invoke(new object[0]);
        }
    }
}
