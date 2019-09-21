// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using System.Windows.Forms;

    public partial class TabPageControl
    {
        /// <summary>
        /// Class TabPageAccessibility.
        /// Implements the <see cref="Control.ControlAccessibleObject" />
        /// </summary>
        /// <seealso cref="Control.ControlAccessibleObject" />
        private class TabPageAccessibility : ControlAccessibleObject
        {
            /// <summary>
            /// The tabpage
            /// </summary>
            private readonly TabPageControl tabpage;

            /// <summary>
            /// Initializes a new instance of the <see cref="TabPageAccessibility"/> class.
            /// </summary>
            /// <param name="ownerControl">The owner control.</param>
            public TabPageAccessibility(TabPageControl ownerControl) : base(ownerControl) => this.tabpage = ownerControl;

            /// <summary>
            /// Gets the location and size of the accessible object.
            /// </summary>
            /// <value>The bounds.</value>
            public override Rectangle Bounds
            {
                get
                {
                    if (this.tabpage.Visible)
                    {
                        return base.Bounds;
                    }
                    else
                    {
                        //return empty rect when not visible to prevent bugs with MSAA info
                        //returning info on the wrong controls when mousing over tab controls
                        return Rectangle.Empty;
                    }
                }
            }
        }
    }
}
