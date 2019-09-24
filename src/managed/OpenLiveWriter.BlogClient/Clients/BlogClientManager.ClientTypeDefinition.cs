// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Reflection;
    using OpenLiveWriter.Extensibility.BlogClient;

    public sealed partial class BlogClientManager
    {
        /// <summary>
        /// The ClientTypeDefinition class.
        /// </summary>
        private class ClientTypeDefinition
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ClientTypeDefinition"/> class.
            /// </summary>
            /// <param name="clientType">Type of the client.</param>
            /// <exception cref="ArgumentException">
            /// You must provide a single BlogClientAttribute for all registered blog client types.
            /// or
            /// You must implement a public constructor with the signature (Uri,IBlogCredentials,PostFormatOptions) for all registered blog client types.
            /// </exception>
            public ClientTypeDefinition(Type clientType)
            {
                // determine the name from the custom attribute
                var blogClientAttributes =
                    clientType.GetCustomAttributes(typeof(BlogClientAttribute), false) as BlogClientAttribute[];
                if (blogClientAttributes?.Length != 1)
                {
                    throw new ArgumentException(
                        "You must provide a single BlogClientAttribute for all registered blog client types.");
                }

                this.Name = blogClientAttributes[0].TypeName;

                // get the constructor used for creation
                this.Constructor = clientType.GetConstructor(new[] { typeof(Uri), typeof(IBlogCredentialsAccessor) });
                if (this.Constructor == null)
                {
                    throw new ArgumentException(
                        "You must implement a public constructor with the signature (Uri,IBlogCredentials,PostFormatOptions) for all registered blog client types.");
                }

                // record the type
                this.Type = clientType;
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
