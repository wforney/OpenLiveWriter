// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    #region Public Enumerations

    #endregion Public Enumerations

    public interface ICommandTextDisplayProperties
    {
        string LabelTitle { get; set; }
        string LabelDescription { get; set; }
        string TooltipTitle { get; set; }
        string TooltipDescription { get; set; }
        string Keytip { get; set; }
    }
}
