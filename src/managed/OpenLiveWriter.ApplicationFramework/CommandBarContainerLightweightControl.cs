// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Provides a container control for CommandBarLightweightControl.
    /// </summary>
    public partial class CommandBarContainerLightweightControl : LightweightControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// Initializes a new instance of the CommandBarContainerLightweightControl class.
        /// </summary>
        public CommandBarContainerLightweightControl(IContainer container)
        {
            /// <summary>
            /// Required for Windows.Forms Class Composition Designer support
            /// </summary>
            container.Add(this);
            this.InitializeComponent();
            this.InitializeObject();
        }

        /// <summary>
        /// Initializes a new instance of the CommandBarContainerLightweightControl class.
        /// </summary>
        public CommandBarContainerLightweightControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
            this.InitializeObject();
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
        private void InitializeComponent() => this.components = new Container();
        #endregion

        /// <summary>
        /// Common object initialization.
        /// </summary>
        private void InitializeObject() => this.AccessibleRole = System.Windows.Forms.AccessibleRole.Grouping;

        /// <summary>
        /// Gets the default virtual size of the lightweight control.
        /// </summary>
        public override Size DefaultVirtualSize
        {
            get
            {
                if (!(this.LightweightControlContainerControl is CommandBarLightweightControl commandBarLightweightControl))
                {
                    return Size.Empty;
                }

                //	Calculate the maximum width and height.
                var maximumWidth = 0;
                var maximumHeight = 0;
                foreach (var lightweightControl in this.LightweightControls)
                {
                    //	Have the lightweight control perform its layout logic.
                    lightweightControl.PerformLayout();

                    //	Account for the width of the control.
                    maximumWidth += lightweightControl.VirtualWidth;
                    if (lightweightControl is CommandBarButtonLightweightControl)
                    {
                        maximumWidth += ((CommandBarButtonLightweightControl)lightweightControl).MarginLeft;
                        maximumWidth += ((CommandBarButtonLightweightControl)lightweightControl).MarginRight;
                    }

                    //	Handle separators.
                    if (lightweightControl is CommandBarSeparatorLightweightControl)
                    {
                        maximumWidth += commandBarLightweightControl.SeparatorLayoutMargin * 2;
                    }

                    //	Note the tallest virtual control.
                    if (lightweightControl.VirtualHeight > maximumHeight)
                    {
                        maximumHeight = lightweightControl.VirtualHeight;
                    }
                }

                //	Return the default virtual size.
                return new Size(maximumWidth, maximumHeight);
            }
        }

        public int OffsetSpacing { get; set; } = 0;

        /// <summary>
        /// Raises the Layout event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLayout(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            //	Obtain our CommandBarLightweightControl.  Must have one.
            if (!(this.LightweightControlContainerControl is CommandBarLightweightControl commandBarLightweightControl))
            {
                return;
            }

            //	Layout the lightweight controls on the command bar lightweight control.
            var xOffset = 0;
            var previous = Previous.None;

            foreach (var lightweightControl in this.LightweightControls)
            {
                //	Have the lightweight control perform its layout logic.
                lightweightControl.PerformLayout();

                //	Skip the lightweight control if it's not visible.
                if (!lightweightControl.Visible)
                {
                    continue;
                }

                //	Handle separators.
                if (lightweightControl is CommandBarSeparatorLightweightControl)
                {
                    lightweightControl.VirtualHeight = this.VirtualHeight;
                    xOffset += commandBarLightweightControl.SeparatorLayoutMargin;
                }

                if (previous == Previous.None)
                {
                    xOffset += 5;
                }

                if ((previous == Previous.Button || previous == Previous.Separator) && lightweightControl is CommandBarButtonLightweightControl)
                {
                    xOffset += this.OffsetSpacing;
                }

                if (lightweightControl is CommandBarButtonLightweightControl)
                {
                    xOffset += ((CommandBarButtonLightweightControl)lightweightControl).MarginLeft;
                }

                //	Set the location of this control.
                lightweightControl.VirtualLocation = new Point(xOffset, Utility.CenterMinZero(lightweightControl.VirtualHeight, this.VirtualHeight));

                //	Have the lightweight control perform its layout logic.
                //lightweightControl.PerformLayout();

                //	Adjust the x offset for the next loop iteration.
                xOffset += lightweightControl.VirtualWidth;

                if (lightweightControl is CommandBarButtonLightweightControl)
                {
                    xOffset += ((CommandBarButtonLightweightControl)lightweightControl).MarginRight;
                }

                previous = Previous.Other;

                //	Handle separators.
                if (lightweightControl is CommandBarSeparatorLightweightControl)
                {
                    xOffset += commandBarLightweightControl.SeparatorLayoutMargin;
                    previous = Previous.Separator;
                }

                if (lightweightControl is CommandBarButtonLightweightControl)
                {
                    //if (((CommandBarButtonLightweightControl)lightweightControl).DropDownContextMenuUserInterface)
                    //    xOffset += 15;

                    previous = Previous.Button;
                }
            }

            this.VirtualWidth = xOffset;

            this.RtlLayoutFixup(false);
        }
    }
}
