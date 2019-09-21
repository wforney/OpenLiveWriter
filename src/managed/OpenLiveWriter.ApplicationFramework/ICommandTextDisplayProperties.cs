// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Interface ICommandTextDisplayProperties
    /// </summary>
    public interface ICommandTextDisplayProperties
    {
        /// <summary>
        /// Gets or sets the label title.
        /// </summary>
        /// <value>The label title.</value>
        string LabelTitle { get; set; }

        /// <summary>
        /// Gets or sets the label description.
        /// </summary>
        /// <value>The label description.</value>
        string LabelDescription { get; set; }

        /// <summary>
        /// Gets or sets the tooltip title.
        /// </summary>
        /// <value>The tooltip title.</value>
        string TooltipTitle { get; set; }

        /// <summary>
        /// Gets or sets the tooltip description.
        /// </summary>
        /// <value>The tooltip description.</value>
        string TooltipDescription { get; set; }

        /// <summary>
        /// Gets or sets the keytip.
        /// </summary>
        /// <value>The keytip.</value>
        string Keytip { get; set; }
    }
}
