// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Collections.Generic;

    public sealed partial class BlogClientManager
    {
        /// <summary>
        /// The OptionOverrideReader class.
        /// </summary>
        private class OptionOverrideReader
        {
            /// <summary>
            /// The option overrides
            /// </summary>
            private readonly IDictionary<string, string> optionOverrides;

            /// <summary>
            /// Initializes a new instance of the <see cref="OptionOverrideReader"/> class.
            /// </summary>
            /// <param name="optionOverrides">The option overrides.</param>
            public OptionOverrideReader(IDictionary<string, string> optionOverrides) => this.optionOverrides = optionOverrides;

            /// <summary>
            /// Reads the specified option.
            /// </summary>
            /// <param name="optionName">Name of the option.</param>
            /// <returns>The specified option.</returns>
            public string Read(string optionName) => this.optionOverrides[optionName];
        }
    }
}
