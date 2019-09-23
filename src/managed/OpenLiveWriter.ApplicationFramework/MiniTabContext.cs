// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    ///     Class MiniTabContext.
    /// </summary>
    public class MiniTabContext
    {
        /// <summary>
        ///     The parent
        /// </summary>
        private readonly Control parent;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MiniTabContext" /> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        public MiniTabContext(Control parent)
        {
            this.parent = parent;
            var suffix = SystemInformation.HighContrast ? "-hi.png" : ".png";
            this.LeftOn = ResourceHelper.LoadAssemblyResourceBitmap($"Images.TabLeftSelected{suffix}");
            this.CenterOn = ResourceHelper.LoadAssemblyResourceBitmap($"Images.TabCenterSelected{suffix}");
            this.RightOn = ResourceHelper.LoadAssemblyResourceBitmap($"Images.TabRightSelected{suffix}");
            this.LeftOff = ResourceHelper.LoadAssemblyResourceBitmap($"Images.TabLeft{suffix}");
            this.CenterOff = ResourceHelper.LoadAssemblyResourceBitmap($"Images.TabCenter{suffix}");
            this.RightOff = ResourceHelper.LoadAssemblyResourceBitmap($"Images.TabRight{suffix}");

            parent.FontChanged += this.parent_FontChanged;
            this.RefreshFonts();
        }

        /// <summary>
        ///     Gets the center off.
        /// </summary>
        /// <value>The center off.</value>
        public Bitmap CenterOff { get; }

        /// <summary>
        ///     Gets the center on.
        /// </summary>
        /// <value>The center on.</value>
        public Bitmap CenterOn { get; }

        /// <summary>
        ///     Gets the font.
        /// </summary>
        /// <value>The font.</value>
        public Font Font { get; private set; }

        /// <summary>
        ///     Gets the font selected.
        /// </summary>
        /// <value>The font selected.</value>
        public Font FontSelected { get; private set; }

        /// <summary>
        ///     Gets the left off.
        /// </summary>
        /// <value>The left off.</value>
        public Bitmap LeftOff { get; }

        /// <summary>
        ///     Gets the left on.
        /// </summary>
        /// <value>The left on.</value>
        public Bitmap LeftOn { get; }

        /// <summary>
        ///     Gets the right off.
        /// </summary>
        /// <value>The right off.</value>
        public Bitmap RightOff { get; }

        /// <summary>
        ///     Gets the right on.
        /// </summary>
        /// <value>The right on.</value>
        public Bitmap RightOn { get; }

        /// <summary>
        ///     Handles the FontChanged event of the parent control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void parent_FontChanged(object sender, EventArgs e) => this.RefreshFonts();

        /// <summary>
        ///     Refreshes the fonts.
        /// </summary>
        private void RefreshFonts()
        {
            this.Font = this.parent.Font;
            this.FontSelected = new Font(this.Font, FontStyle.Bold);
        }
    }
}
