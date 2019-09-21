// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.ApplicationFramework.Properties;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.UI;
    using OpenLiveWriter.Localization;

    /// <summary>
    ///     Class MinMaxClose.
    ///     Implements the <see cref="System.Windows.Forms.Control" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Control" />
    public class MinMaxClose : Control
    {
        /// <summary>
        ///     The close disabled
        /// </summary>
        private readonly Bitmap closeDisabled =
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.CloseDisabled.png");

        /// <summary>
        ///     The close enabled
        /// </summary>
        private readonly Bitmap closeEnabled = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.CloseEnabled.png");

        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        ///     The maximum disabled
        /// </summary>
        private readonly Bitmap maxDisabled =
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizeDisabled.png");

        /// <summary>
        ///     The maximum enabled
        /// </summary>
        private readonly Bitmap maxEnabled =
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizeEnabled.png");

        /// <summary>
        ///     The minimum disabled
        /// </summary>
        private readonly Bitmap minDisabled =
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizeDisabled.png");

        /// <summary>
        ///     The minimum enabled
        /// </summary>
        private readonly Bitmap minEnabled =
            ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizeEnabled.png");

        /// <summary>
        ///     The BTN close
        /// </summary>
        private BitmapButton btnClose;

        /// <summary>
        ///     The BTN maximize
        /// </summary>
        private BitmapButton btnMaximize;

        /// <summary>
        ///     The BTN minimize
        /// </summary>
        private BitmapButton btnMinimize;

        /// <summary>
        ///     The faded
        /// </summary>
        private bool faded;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MinMaxClose" /> class.
        /// </summary>
        public MinMaxClose()
        {
            // Turn on double buffered painting.
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            this.btnMinimize.Text = Res.Get(StringId.MinimizeButtonTooltip);
            this.btnMaximize.Text = Res.Get(StringId.MaximizeButtonTooltip);
            this.btnClose.Text = Res.Get(StringId.CloseButtonTooltip);

            this.btnMinimize.UseVirtualTransparency = true;
            this.btnMaximize.UseVirtualTransparency = true;
            this.btnClose.UseVirtualTransparency = true;

            this.btnMinimize.BitmapEnabled = this.minEnabled;
            this.btnMaximize.BitmapEnabled = this.maxEnabled;
            this.btnClose.BitmapEnabled = this.closeEnabled;

            this.btnMinimize.TabStop = false;
            this.btnMaximize.TabStop = false;
            this.btnClose.TabStop = false;
            this.TabStop = false;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="MinMaxClose" /> is faded.
        /// </summary>
        /// <value><c>true</c> if faded; otherwise, <c>false</c>.</value>
        public bool Faded
        {
            get => this.faded;
            set
            {
                this.faded = value;
                if (value)
                {
                    this.btnMinimize.BitmapEnabled = this.minDisabled;
                    this.btnMaximize.BitmapEnabled = this.maxDisabled;
                    this.btnClose.BitmapEnabled = this.closeDisabled;
                    this.Invalidate(true);
                }
                else
                {
                    this.btnMinimize.BitmapEnabled = this.minEnabled;
                    this.btnMaximize.BitmapEnabled = this.maxEnabled;
                    this.btnClose.BitmapEnabled = this.closeEnabled;
                    this.Invalidate(true);
                }
            }
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        ///     <see langword="true" /> to release both managed and unmanaged resources;
        ///     <see langword="false" /> to release only unmanaged resources.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Handles the <see cref="E:PaintBackground" /> event.
        /// </summary>
        /// <param name="e">The <see cref="PaintEventArgs" /> instance containing the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e) => VirtualTransparency.VirtualPaint(this, e);

        /// <summary>
        ///     Centers the specified bounds.
        /// </summary>
        /// <param name="bounds">The bounds.</param>
        /// <returns>A Point.</returns>
        private static Point Center(Rectangle bounds) =>
            new Point(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);

        /// <summary>
        ///     Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void btnClose_Click(object sender, EventArgs e) => this.FindForm()?.Close();

        /// <summary>
        ///     Handles the Click event of the btnMaximize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void btnMaximize_Click(object sender, EventArgs e)
        {
            var f = this.FindForm();
            switch (f?.WindowState)
            {
                case FormWindowState.Maximized:
                    f.WindowState = FormWindowState.Normal;
                    break;
                case FormWindowState.Normal:
                    f.WindowState = FormWindowState.Maximized;
                    break;
                default:
                    Debug.Fail("Maximize/Restore clicked while minimized!?");
                    break;
            }
        }

        /// <summary>
        ///     Handles the Click event of the btnMinimize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
        private void btnMinimize_Click(object sender, EventArgs e) => this.FindForm().WindowState = FormWindowState.Minimized;

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnMinimize = new BitmapButton();
            this.btnMaximize = new BitmapButton();
            this.btnClose = new BitmapButton();
            this.SuspendLayout();

            // btnMinimize
            this.btnMinimize.BackColor = Color.Transparent;
            this.btnMinimize.Location = new Point(0, 0);
            this.btnMinimize.Size = new Size(25, 17);
            this.btnMinimize.Text = Resources.Minimize;
            this.btnMinimize.ButtonStyle = ButtonStyle.Bitmap;
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizeHover.png");
            this.btnMinimize.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MinimizePushed.png");
            this.btnMinimize.TabIndex = 2;
            this.btnMinimize.Click += this.btnMinimize_Click;

            // btnMaximize
            this.btnMaximize.BackColor = Color.Transparent;
            this.btnMaximize.Location = new Point(25, 0);
            this.btnMaximize.Size = new Size(26, 17);
            this.btnMaximize.Text = Resources.Maximize;
            this.btnMaximize.ButtonStyle = ButtonStyle.Bitmap;
            this.btnMaximize.Name = "btnMaximize";
            this.btnMaximize.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizeHover.png");
            this.btnMaximize.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.MaximizePushed.png");
            this.btnMaximize.TabIndex = 3;
            this.btnMaximize.Click += this.btnMaximize_Click;

            // btnClose
            this.btnClose.BackColor = Color.Transparent;
            this.btnClose.Location = new Point(51, 0);
            this.btnClose.Size = new Size(42, 17);
            this.btnClose.Text = Resources.Close;
            this.btnClose.ButtonStyle = ButtonStyle.Bitmap;
            this.btnClose.Name = "btnClose";
            this.btnClose.BitmapSelected = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.CloseHover.png");
            this.btnClose.BitmapPushed = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.ClosePushed.png");
            this.btnClose.TabIndex = 4;
            this.btnClose.Click += this.btnClose_Click;

            // MinMaxClose
            this.BackColor = Color.Transparent;
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnMaximize);
            this.Controls.Add(this.btnMinimize);
            this.Name = "MinMaxClose";
            this.Size = new Size(93, 17);
            this.ResumeLayout(false);
        }
    }
}
