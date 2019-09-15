﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Types of Html which can be generated by an AdaptiveHtmlObject.
    /// </summary>
    public enum HtmlType
    {
        /// <summary>
        /// Html which contains only a preview-image that navigates to the preview-link when clicked.
        /// </summary>
        PreviewHtml,

        /// <summary>
        /// Html which contains only the object tag which was passed to the constructor of the AdaptiveHtmlObject.
        /// </summary>
        ObjectHtml,

        /// <summary>
        /// Adaptive Html which attempts to render the object tag but falls back to PreviewHtml
        /// if the rendering environment does not support script.
        /// </summary>
        AdaptiveHtml
    }
}
