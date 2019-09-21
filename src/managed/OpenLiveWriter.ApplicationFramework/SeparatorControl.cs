// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Windows.Forms;

    /// <summary>
    /// Class SeparatorControl.
    /// Implements the <see cref="System.Windows.Forms.UserControl" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    public partial class SeparatorControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeparatorControl"/> class.
        /// </summary>
        public SeparatorControl()
        {
            this.InitializeComponent();
            this.TabStop = false;
        }
    }
}
