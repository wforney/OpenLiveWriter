// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    [Flags]
    public enum ButtonFeatures
    {
        None = 0,
        Image = 1,
        Text = 2,
        Menu = 4,
        SplitMenu = 8
    };

}
