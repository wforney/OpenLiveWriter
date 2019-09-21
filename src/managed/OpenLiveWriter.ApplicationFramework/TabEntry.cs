// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    /// <summary>
    ///     Represents a tab entry.
    /// </summary>
    internal class TabEntry
    {
        private bool hidden;

        /// <summary>Initializes a new instance of the TabEntry class.</summary>
        /// <param name="tabLightweightControl">The tab lightweight control.</param>
        /// <param name="tabPageControl">The tab page control.</param>
        public TabEntry(TabLightweightControl tabLightweightControl, TabPageControl tabPageControl)
        {
            //	Set the the tab control and the tab lightweight control.
            this.TabPageControl = tabPageControl;
            this.TabPageControl.TabStop = false;
            this.TabLightweightControl = tabLightweightControl;

            //	Instantiate the tab selector lightweight control.
            this.TabSelectorLightweightControl = new TabSelectorLightweightControl(this);
        }

        /// <summary>
        ///     Gets the tab lightweight control.
        /// </summary>
        public TabLightweightControl TabLightweightControl { get; }

        /// <summary>
        ///     Gets the tab page control.
        /// </summary>
        public TabPageControl TabPageControl { get; }

        /// <summary>
        ///     Gets the tab control.
        /// </summary>
        public TabSelectorLightweightControl TabSelectorLightweightControl { get; }

        /// <summary>
        ///     Gets a value indicating whether the tab entry is the first tab entry or not.
        /// </summary>
        public bool IsFirstTabEntry => this.TabLightweightControl.FirstTabEntry == this;

        /// <summary>
        ///     Gets a value indicating whether the tab entry is selected or not.
        /// </summary>
        public bool IsSelected => this.TabLightweightControl.SelectedTabEntry == this;

        public bool Hidden
        {
            get => this.hidden;
            set
            {
                this.hidden = value;
                this.TabSelectorLightweightControl.Visible = value;
                this.TabPageControl.Visible = value;
            }
        }

        public void Selected()
        {
            if (this.TabPageControl == null)
            {
                return;
            }

            if (this.hidden)
            {
                this.Hidden = false;
            }

            this.TabPageControl.Visible = true;
            this.TabPageControl.TabStop = true;
            this.TabPageControl.RaiseSelected();
            this.TabPageControl.SelectNextControl(null, true, true, true, false);
        }

        public void Unselected()
        {
            if (this.TabPageControl == null)
            {
                return;
            }

            this.TabPageControl.Visible = false;
            this.TabPageControl.TabStop = false;
            this.TabPageControl.RaiseUnselected();
        }
    }
}