// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    public abstract partial class HeaderFooterSource
    {
        /// <summary>
        /// Determines whether content should be placed above or below the
        /// post body.
        /// </summary>
        public enum Position
        {
            /// <summary>
            /// Places content above the post body.
            /// </summary>
            Header,
            /// <summary>
            /// Places content below the post body.
            /// </summary>
            Footer
        };
    }
}
