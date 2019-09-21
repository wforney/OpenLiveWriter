// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;

    /// <summary>
    /// CommandBar separator lightweight control.
    /// </summary>
    public class CommandBarSeparatorLightweightControl : LightweightControl
    {
        /// <summary>
        /// The default width.
        /// </summary>
        private const int DEFAULT_WIDTH = 2;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the CommandBarSeparatorLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public CommandBarSeparatorLightweightControl(IContainer container)
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
        public CommandBarSeparatorLightweightControl()
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
            this.AccessibleRole = AccessibleRole.Separator;
            this.AccessibleName = "Separator";
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();
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

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);

            //	Paint the left line.
            using (var solidBrush = new SolidBrush(Color.FromArgb(64, 0, 0, 0)))
            {
                e.Graphics.FillRectangle(
                    solidBrush,
                    this.VirtualClientRectangle.X,
                    this.VirtualClientRectangle.Y + 1,
                    this.VirtualClientRectangle.Width / 2,
                    this.VirtualClientRectangle.Height);
            }

            //	Paint the right line.
            using (var solidBrush = new SolidBrush(Color.FromArgb(64, 255, 255, 255)))
            {
                e.Graphics.FillRectangle(
                    solidBrush,
                    this.VirtualClientRectangle.X
                    + this.VirtualClientRectangle.Width / 2,
                    this.VirtualClientRectangle.Y + 1,
                    this.VirtualClientRectangle.Width - (this.VirtualClientRectangle.Width / 2),
                    this.VirtualClientRectangle.Height);
            }
        }
    }
}
