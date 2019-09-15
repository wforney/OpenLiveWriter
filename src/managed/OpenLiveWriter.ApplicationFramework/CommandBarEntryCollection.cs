// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing.Design;

    /// <summary>
    /// Represents a collection of CommandBarEntry objects.
    /// </summary>
    [Editor(typeof(CommandBarEntryCollectionEditor), typeof(UITypeEditor))]
    public class CommandBarEntryCollection : List<CommandBarEntry>
    {
    }
}
