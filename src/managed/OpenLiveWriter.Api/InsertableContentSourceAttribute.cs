// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.Api
{
    /// <summary>
    /// Attribute applied to ContentSource and SmartContentSource classes which override the
    /// CreateContent method to enable creation of new content from an Insert dialog.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class InsertableContentSourceAttribute : Attribute
    {
        /// <summary>
        /// Initialize a new instance of an InsertableContentSourceAttribute.
        /// </summary>
        /// <param name="menuText">Text used to describe the insertable content on the Insert menu.</param>
        public InsertableContentSourceAttribute(string menuText)
        {
            this.MenuText = menuText;
            this.SidebarText = this._menuText;
        }

        /// <summary>
        /// Text used to describe the insertable content on the Insert menu.
        /// </summary>
        public string MenuText
        {
            get => this._menuText;
            set => this._menuText = value ?? throw new ArgumentNullException("InsertableContentSource.MenuText");
        }
        private string _menuText;

        /// <summary>
        /// Text used to describe the insertable content on the Sidebar. This is optional and
        /// can be specified to provide a shorter name for the insertable content (there is
        /// less room on the Sidebar than in the Insert menu).
        /// </summary>
        public string SidebarText
        {
            get => this._sidebarText;
            set => this._sidebarText = value ?? throw new ArgumentNullException("InsertableContentSource.SidebarText");
        }
        private string _sidebarText;
    }
}
