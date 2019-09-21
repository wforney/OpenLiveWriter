// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    /// <summary>
    /// Class PreferencesForm.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.BaseForm" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.BaseForm" />
    public partial class PreferencesForm
    {
        /// <summary>
        /// Class TabSwitcher.
        /// </summary>
        internal class TabSwitcher
        {
            /// <summary>
            /// The control
            /// </summary>
            private readonly SideBarControl control;

            /// <summary>
            /// Initializes a new instance of the <see cref="TabSwitcher"/> class.
            /// </summary>
            /// <param name="control">The control.</param>
            internal TabSwitcher(SideBarControl control) => this.control = control;

            /// <summary>
            /// The tab
            /// </summary>
            public int Tab;

            /// <summary>
            /// Switches this instance.
            /// </summary>
            public void Switch() => this.control.SelectedIndex = this.Tab;
        }
    }
}
