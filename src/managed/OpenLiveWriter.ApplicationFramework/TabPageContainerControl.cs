// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>
    /// Helper control for TabLightweightControl.  The TabPageContainer provides the
    /// container control below the tabs and is the parent of the TabPage controls of the TabLightweightControl
    /// are shown/hidden.
    /// </summary>
    public class TabPageContainerControl : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the TabPageContainer class.
        /// </summary>
        public TabPageContainerControl() =>
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

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
        private void InitializeComponent() => this.components = new Container();
        #endregion

        /// <summary>
        /// Raises the PaintBackground event.
        /// </summary>
        /// <param name="e">A PaintEventArgs that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //	Nothing!  Intentional!
        }

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            //	Call the base class's method so registered delegates receive the event.
            base.OnLayout(e);

            //	No layout required.
            if (this.Parent == null)
            {
                return;
            }

            //	Layout each child control.
            var clientRectangle = this.ClientRectangle;
            foreach (Control control in this.Controls)
            {
                if (control.Bounds != clientRectangle)
                {
                    control.Bounds = clientRectangle;
                }
            }
        }
    }
}
