// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    public partial class Command
    {
        public enum InvalidationState
        {
            Pending, // We have not yet set or invalidated the ribbon
            WaitingForUpdateProperty, // We have called InvalidateUICommand and are waiting for an UpdateProperty callback.
            Error // The ribbon APIs to set and invalidate this command have return an failing error code.
        }
    }
}
