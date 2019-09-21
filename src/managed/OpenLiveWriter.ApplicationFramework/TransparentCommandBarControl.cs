// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices.UI;

    /// <summary>
    /// Class TransparentCommandBarControl.
    /// Implements the <see cref="CommandBarControl" />
    /// </summary>
    /// <seealso cref="CommandBarControl" />
    public class TransparentCommandBarControl : CommandBarControl
    {
        /// <summary>
        /// The parent
        /// </summary>
        private Control parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransparentCommandBarControl"/> class.
        /// </summary>
        /// <param name="commandBar">The command bar.</param>
        /// <param name="commandBarDefinition">The command bar definition.</param>
        public TransparentCommandBarControl(CommandBarLightweightControl commandBar, CommandBarDefinition commandBarDefinition) : base(commandBar, commandBarDefinition)
        {
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.parent != null && !this.parent.IsDisposed)
                {
                    this.parent.Invalidated -= new InvalidateEventHandler(this.TransparentCommandBarControl_Invalidated);
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Forces the layout.
        /// </summary>
        public void ForceLayout() => this.CommandBarLightweightControl.PerformLayout();

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="pevent">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains information about the control to paint.</param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (this.parent == null)
            {
                this.parent = (Control)VirtualTransparency.VirtualParent(this);
                this.parent.Invalidated += new InvalidateEventHandler(this.TransparentCommandBarControl_Invalidated);
            }

            VirtualTransparency.VirtualPaint((IVirtualTransparencyHost)this.parent, this, pevent);
        }

        /// <summary>
        /// Handles the Invalidated event of the TransparentCommandBarControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="InvalidateEventArgs"/> instance containing the event data.</param>
        private void TransparentCommandBarControl_Invalidated(object sender, InvalidateEventArgs e)
        {
            var absLoc = this.parent.PointToScreen(e.InvalidRect.Location);
            var relLoc = this.PointToClient(absLoc);
            var relRect = new Rectangle(relLoc, e.InvalidRect.Size);
            if (this.ClientRectangle.IntersectsWith(relRect))
            {
                relRect.Intersect(this.ClientRectangle);
                this.Invalidate(relRect);
            }
        }
    }
}
