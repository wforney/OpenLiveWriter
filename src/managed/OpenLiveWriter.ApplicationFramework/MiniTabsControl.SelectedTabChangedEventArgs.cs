// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    public partial class MiniTabsControl
    {
        /// <summary>
        /// Class SelectedTabChangedEventArgs.
        /// </summary>
        public class SelectedTabChangedEventArgs
        {
            /// <summary>
            /// The selected tab index
            /// </summary>
            public readonly int SelectedTabIndex;

            /// <summary>
            /// Initializes a new instance of the <see cref="SelectedTabChangedEventArgs"/> class.
            /// </summary>
            /// <param name="selectedTabIndex">Index of the selected tab.</param>
            public SelectedTabChangedEventArgs(int selectedTabIndex) => this.SelectedTabIndex = selectedTabIndex;
        }
    }
}
