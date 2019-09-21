// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    using OpenLiveWriter.ApplicationFramework.Skinning;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    ///     Class SidebarHeaderControl.
    ///     Implements the <see cref="System.Windows.Forms.UserControl" />
    ///     Implements the <see cref="OpenLiveWriter.Localization.Bidi.IRtlAware" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    /// <seealso cref="OpenLiveWriter.Localization.Bidi.IRtlAware" />
    public partial class SidebarHeaderControl : UserControl, IRtlAware
    {
        /// <summary>
        ///     The sep bottom padding
        /// </summary>
        private const int SEP_BOTTOM_PADDING = 12;

        /// <summary>
        ///     The sep top padding
        /// </summary>
        private const int SEP_TOP_PADDING = 15;

        /// <summary>
        ///     The doing layout
        /// </summary>
        private bool doingLayout;

        /// <summary>
        ///     The second URL
        /// </summary>
        private string secondUrl;

        /// <summary>
        ///     The URL
        /// </summary>
        private string url;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SidebarHeaderControl" /> class.
        /// </summary>
        public SidebarHeaderControl()
        {
            this.InitializeComponent();
            this.BackColor = ColorizedResources.Instance.SidebarHeaderBackgroundColor;
            this.labelHeading.Font = Res.GetFont(FontSize.XLarge, FontStyle.Regular);
            this.labelHeading.ForeColor = ColorizedResources.Instance.SidebarHeaderTextColor;
            this.linkLabelOptional.Font = this.linkLabel.Font = Res.GetFont(FontSize.Normal, FontStyle.Regular);
            this.linkLabelOptional.LinkColor = this.linkLabel.LinkColor = ColorizedResources.Instance.SidebarLinkColor;
            this.linkLabelOptional.LinkArea = this.linkLabel.LinkArea = new LinkArea(0, 0);
            this.linkLabelOptional.Visible = false;
            this.linkLabelOptional.FlatStyle = FlatStyle.System;
            this.linkLabel.FlatStyle = FlatStyle.System;
            this.linkLabel.AutoEllipsis = true;
            this.labelHeading.FlatStyle = FlatStyle.Standard;
            this.labelHeading.AutoEllipsis = true;
            this.labelHeading.AutoSize = false;
            this.linkLabelOptional.Visible = false;

            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        }

        /// <summary>
        ///     Gets or sets the header text.
        /// </summary>
        /// <value>The header text.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string HeaderText
        {
            get => this.labelHeading.Text;
            set
            {
                var accessibleName = ControlHelper.ToAccessibleName(value);
                this.labelHeading.AccessibleName = accessibleName;
                this.AccessibleName = accessibleName;

                // TabStop = false;
                this.labelHeading.Text = StringHelper.Ellipsis(value, 100);
            }
        }

        /// <summary>
        ///     Gets or sets the link text.
        /// </summary>
        /// <value>The link text.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LinkText
        {
            get => this.linkLabel.Text;
            set
            {
                this.linkLabel.Text = StringHelper.Ellipsis(value, 100);
                this.linkLabel.Visible = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        ///     Gets or sets the link URL.
        /// </summary>
        /// <value>The link URL.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LinkUrl
        {
            get => this.url;
            set
            {
                this.url = value;
                this.linkLabel.LinkArea = new LinkArea(0, new StringInfo(this.LinkText).LengthInTextElements);
            }
        }

        /// <summary>
        ///     Gets or sets the second link text.
        /// </summary>
        /// <value>The second link text.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SecondLinkText
        {
            get => this.linkLabelOptional.Text;
            set
            {
                this.linkLabelOptional.Text = StringHelper.Ellipsis(value, 100);
                this.linkLabelOptional.Visible = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        ///     Gets or sets the second link URL.
        /// </summary>
        /// <value>The second link URL.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SecondLinkUrl
        {
            get => this.secondUrl;
            set
            {
                this.secondUrl = value;
                this.linkLabelOptional.LinkArea = new LinkArea(
                    0,
                    new StringInfo(this.SecondLinkText).LengthInTextElements);
            }
        }

        /// <summary>
        ///     Refreshes the layout.
        /// </summary>
        public void RefreshLayout() => ((IRtlAware)this).Layout();

        /// <summary>
        ///     Layouts this instance.
        /// </summary>
        void IRtlAware.Layout()
        {
            this.doingLayout = true;
            this.SuspendLayout();
            this.separatorControl.Width = this.labelHeading.Width =
                                              this.linkLabelOptional.Width = this.linkLabel.Width = this.Width;
            LayoutHelper.NaturalizeHeightAndDistribute(2, this.labelHeading, this.linkLabel, this.linkLabelOptional);

            // linkLabel.Width = Width;
            Control lastVisibleControl = this.linkLabel;
            if (this.linkLabelOptional.Visible)
            {
                lastVisibleControl = this.linkLabelOptional;
            }

            this.Height = lastVisibleControl.Bottom + this.separatorControl.Height
                                                    + SidebarHeaderControl.SEP_TOP_PADDING
                                                    + SidebarHeaderControl.SEP_BOTTOM_PADDING;
            this.separatorControl.Top = lastVisibleControl.Bottom + SidebarHeaderControl.SEP_TOP_PADDING;

            if (BidiHelper.IsRightToLeft)
            {
                this.labelHeading.Left = this.Width - this.labelHeading.Width;
                this.linkLabel.Left = this.Width - this.linkLabel.Width - 1;
                this.linkLabelOptional.Left = this.Width - this.linkLabelOptional.Width - 1;
            }
            else
            {
                this.labelHeading.Left = -1;
                this.linkLabel.Left = this.linkLabelOptional.Left = 0;
            }

            this.ResumeLayout();
            this.doingLayout = false;
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Enter)
            {
                if (this.linkLabel.Focused)
                {
                    SidebarHeaderControl.LaunchUrl(this.url);
                }
                else if (this.linkLabelOptional.Focused)
                {
                    SidebarHeaderControl.LaunchUrl(this.secondUrl);
                }
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            if (this.doingLayout)
            {
                return;
            }

            this.RefreshLayout();
            base.OnSizeChanged(e);
        }

        /// <summary>
        ///     Launches the URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        private static void LaunchUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            ShellHelper.LaunchUrl(url);
        }

        /// <summary>
        ///     Handles the LinkClicked event of the linkLabel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LinkLabelLinkClickedEventArgs" /> instance containing the event data.</param>
        private void linkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => SidebarHeaderControl.LaunchUrl(this.url);

        /// <summary>
        ///     Handles the LinkClicked event of the linkLabelOptional control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LinkLabelLinkClickedEventArgs" /> instance containing the event data.</param>
        private void linkLabelOptional_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) => SidebarHeaderControl.LaunchUrl(this.secondUrl);
    }
}