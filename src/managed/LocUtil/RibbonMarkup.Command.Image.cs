// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    /// <summary>
    /// The ribbon markup class.
    /// </summary>
    public static partial class RibbonMarkup
    {
        /// <summary>
        /// The command class.
        /// </summary>
        public static partial class Command
        {
            /// <summary>
            /// The image class.
            /// </summary>
            /// <remarks>
            /// Child nodes of Command --> Command.LargeImages, Command.SmallImages, Command.LargeHighContrastImages, Command.SmallHighContrastImages
            /// </remarks>
            public static class Image
            {
                /// <summary>
                /// The source
                /// </summary>
                public const string Source = "Source";

                /// <summary>
                /// The identifier
                /// </summary>
                public const string Id = "Id";

                /// <summary>
                /// The minimum dpi
                /// </summary>
                public const string MinDPI = "MinDPI";

                /// <summary>
                /// The symbol
                /// </summary>
                public const string Symbol = "Symbol";
            }
        }
    }
}
