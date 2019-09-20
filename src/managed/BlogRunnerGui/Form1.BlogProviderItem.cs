// <copyright file="Form1.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunnerGui
{
    using System;

    public partial class Form1
    {
        ////private const string SETTING_PROVIDERS = "providers";
        ////private const string SETTING_CONFIG = "config";
        ////private const string SETTING_OUTPUT = "output";
        ////private const string SETTING_PROVIDER = "provider";

        /// <summary>
        /// The blog provider item class.
        /// </summary>
        internal class BlogProviderItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlogProviderItem"/> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="name">The name.</param>
            public BlogProviderItem(string id, string name)
            {
                this.Id = id;
                this.Name = name;
            }

            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public string Id { get; private set; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; private set; }

            /// <summary>
            /// Returns a <see cref="string" /> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="string" /> that represents this instance.</returns>
            public override string ToString() => this.Name;

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public override int GetHashCode() => this.Id.GetHashCode();

            /// <summary>
            /// Determines whether the specified <see cref="object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals(object obj) =>
                obj is BlogProviderItem other ? string.Equals(this.Id, other.Id, StringComparison.Ordinal) : false;
        }
    }
}
