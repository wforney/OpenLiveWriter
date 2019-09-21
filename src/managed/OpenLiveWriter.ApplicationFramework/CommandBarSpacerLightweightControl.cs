// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Drawing;

    using OpenLiveWriter.Controls;

    public class CommandBarSpacerLightweightControl : LightweightControl
    {
        /// <summary>
        /// The default width.
        /// </summary>
        private const int DEFAULT_WIDTH = 10;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarSpacerLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
            this.InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        public CommandBarSpacerLightweightControl()
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            this.InitializeComponent();
            this.InitializeObject();
        }

        private void InitializeObject()
        {
            this.VirtualSize = this.DefaultVirtualSize;
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.Separator;
            this.AccessibleName = "Spacer";
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new Container();
        }
        #endregion

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size DefaultVirtualSize => new Size(DEFAULT_WIDTH, 0);

        /// <summary>
        /// Gets the minimum virtual size of the lightweight control.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public override Size MinimumVirtualSize => this.DefaultVirtualSize;
    }
}
