// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Reflection;

    using OpenLiveWriter.Extensibility.BlogClient;

    public partial class BlogPostContentFilters
    {
        /// <summary>
        /// The ContentFilterTypeDefinition class.
        /// </summary>
        private class ContentFilterTypeDefinition
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ContentFilterTypeDefinition"/> class.
            /// </summary>
            /// <param name="filterType">Type of the filter.</param>
            /// <exception cref="ArgumentException">
            /// ContentFilters must implement IBlogPostContentFilter.
            /// or
            /// You must provide a single BlogPostContentFilterAttribute for all registered blog post content filter types.
            /// or
            /// You must implement a public constructor with no arguments for all registered blog post content filter types.
            /// </exception>
            public ContentFilterTypeDefinition(Type filterType)
            {
                if (!typeof(IBlogPostContentFilter).IsAssignableFrom(filterType))
                {
                    throw new ArgumentException("ContentFilters must implement IBlogPostContentFilter.");
                }

                // determine the name from the custom attribute
                var blogClientAttributes = (BlogPostContentFilterAttribute[])filterType.GetCustomAttributes(
                    typeof(BlogPostContentFilterAttribute),
                    false);
                if (blogClientAttributes.Length != 1)
                {
                    throw new ArgumentException(
                        "You must provide a single BlogPostContentFilterAttribute for all registered blog post content filter types.");
                }

                this.Name = blogClientAttributes[0].TypeName;

                // get the constructor used for creation
                this.Constructor = filterType.GetConstructor(new Type[] { });
                if (this.Constructor == null)
                {
                    throw new ArgumentException(
                        "You must implement a public constructor with no arguments for all registered blog post content filter types.");
                }

                // record the type
                this.Type = filterType;
            }

            /// <summary>
            /// Gets the constructor.
            /// </summary>
            /// <value>The constructor.</value>
            public ConstructorInfo Constructor { get; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; }

            /// <summary>
            /// Gets the type.
            /// </summary>
            /// <value>The type.</value>
            public Type Type { get; }
        }
    }
}
