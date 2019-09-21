// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Windows.Forms;

    /// <summary>
    /// Base class of CommandBarLightweightControls for the WorkspaceControl.
    /// </summary>
    internal class WorkspaceCommandBarLightweightControl : CommandBarLightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// Initializes a new instance of the ApplicationCommandBarLightweightControl class.
        /// </summary>
        public WorkspaceCommandBarLightweightControl() =>
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

        /// <summary>
        /// Initializes a new instance of the ApplicationCommandBarLightweightControl class.
        /// </summary>
        /// <param name="container"></param>
        public WorkspaceCommandBarLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.components != null)
                {
                    this.components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((ISupportInitialize)(this)).BeginInit();
            ((ISupportInitialize)(this)).EndInit();

        }
        #endregion

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //	Our LightweightControlContainerControl will be a WorkspaceControl.  Get it.
            if (!(this.LightweightControlContainerControl is WorkspaceControl))
            {
                return;
            }

            //	Based on the type of workspace control, select the colors to use when painting.
            Color topColor;
            Color bottomColor;
            Color topBevelFirstLineColor;
            Color topBevelSecondLineColor;
            Color bottomBevelFirstLineColor;
            Color bottomBevelSecondLineColor;
            topColor = SystemColors.Control;
            bottomColor = SystemColors.Control;
            topBevelFirstLineColor = SystemColors.Control;
            topBevelSecondLineColor = SystemColors.Control;
            bottomBevelFirstLineColor = SystemColors.Control;
            bottomBevelSecondLineColor = SystemColors.Control;

            //	Fill the background.
            if (topColor == bottomColor)
            {
                using (var solidBrush = new SolidBrush(topColor))
                {
                    e.Graphics.FillRectangle(solidBrush, this.VirtualClientRectangle);
                }
            }
            else
            {
                using (var linearGradientBrush = new LinearGradientBrush(this.VirtualClientRectangle, topColor, bottomColor, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(linearGradientBrush, this.VirtualClientRectangle);
                }
            }

            //	Draw the first line of the top bevel.
            using (var solidBrush = new SolidBrush(topBevelFirstLineColor))
            {
                e.Graphics.FillRectangle(solidBrush, 0, 0, this.VirtualWidth, 1);
            }

            //	Draw the second line of the top bevel.
            using (var solidBrush = new SolidBrush(topBevelSecondLineColor))
            {
                e.Graphics.FillRectangle(solidBrush, 0, 1, this.VirtualWidth, 1);
            }

            //	Draw the first line of the bottom bevel.
            using (var solidBrush = new SolidBrush(bottomBevelFirstLineColor))
            {
                e.Graphics.FillRectangle(solidBrush, 0, this.VirtualHeight - 2, this.VirtualWidth, 1);
            }

            //	Draw the first line of the bottom bevel.
            using (var solidBrush = new SolidBrush(bottomBevelSecondLineColor))
            {
                e.Graphics.FillRectangle(solidBrush, 0, this.VirtualHeight - 1, this.VirtualWidth, 1);
            }

            //	Call the base class's method so that registered delegates receive the event.
            base.OnPaint(e);
        }
    }
}
