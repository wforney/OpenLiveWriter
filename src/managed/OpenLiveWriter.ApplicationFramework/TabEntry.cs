// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Windows.Forms;

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    /// Represents a tab entry.
    /// </summary>
    internal class TabEntry
    {
        /// <summary>
        /// Gets the tab lightweight control.
        /// </summary>
        public TabLightweightControl TabLightweightControl { get; }

        /// <summary>
        /// Gets the tab page control.
        /// </summary>
        public TabPageControl TabPageControl { get; }

        /// <summary>
        /// Gets the tab control.
        /// </summary>
        public TabSelectorLightweightControl TabSelectorLightweightControl { get; }

        /// <summary>
        /// Gets a value indicating whether the tab entry is the first tab entry or not.
        /// </summary>
        public bool IsFirstTabEntry
        {
            get
            {
                return TabLightweightControl.FirstTabEntry == this;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the tab entry is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return TabLightweightControl.SelectedTabEntry == this;
            }
        }

        public bool Hidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
                TabSelectorLightweightControl.Visible = value;
                TabPageControl.Visible = value;
            }
        }
        private bool _hidden;

        public void Selected()
        {
            if (TabPageControl != null)
            {
                if (_hidden)
                    Hidden = false;
                TabPageControl.Visible = true;
                TabPageControl.TabStop = true;
                TabPageControl.RaiseSelected();
                TabPageControl.SelectNextControl(null, true, true, true, false);
            }
        }

        public void Unselected()
        {
            if (TabPageControl != null)
            {
                TabPageControl.Visible = false;
                TabPageControl.TabStop = false;
                TabPageControl.RaiseUnselected();
            }
        }

        /// <summary>
        /// Initializes a new instance of the TabEntry class.
        /// </summary>
        /// <param name="tabControl">The tab page control.</param>
        public TabEntry(TabLightweightControl tabLightweightControl, TabPageControl tabPageControl)
        {
            //	Set the the tab control and the tab lightweight control.
            TabPageControl = tabPageControl;
            TabPageControl.TabStop = false;
            TabLightweightControl = tabLightweightControl;

            //	Instantiate the tab selector lightweight control.
            TabSelectorLightweightControl = new TabSelectorLightweightControl(this);
        }
    }
}
