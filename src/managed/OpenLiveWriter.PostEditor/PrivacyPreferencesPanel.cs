// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using ApplicationFramework.Preferences;
    using CoreServices;
    using CoreServices.Layout;
    using Localization;

    internal class PrivacyPreferencesPanel : PreferencesPanel
    {
        private readonly bool loading;

        private Label labelPrivacyExplanation;
        private LinkLabel linkLabelCodeOfConduct;
        private LinkLabel linkLabelPrivacyStatement;

        public PrivacyPreferencesPanel()
        {
            this.loading = true;
            this.InitializeComponent();

            this.PanelName = Res.Get(StringId.PrivacyPreferencesPanelName);
            this.PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.PreferencesPrivacy.png");
            this.labelPrivacyExplanation.Text = Res.Get(StringId.PrivacyPreferencesPrivacyExplanation);
            this.linkLabelPrivacyStatement.Text = Res.Get(StringId.PrivacyPreferencesPrivacyStatement);
            this.linkLabelCodeOfConduct.Text = Res.Get(StringId.PrivacyPreferencesCodeOfConduct);

            this.loading = false;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LayoutHelper.NaturalizeHeightAndDistribute(10, this.labelPrivacyExplanation,
                                                       this.linkLabelPrivacyStatement);
            LayoutHelper.NaturalizeHeightAndDistribute(2, this.linkLabelPrivacyStatement,
                                                       this.linkLabelCodeOfConduct);
        }

        private void InitializeComponent()
        {
            this.labelPrivacyExplanation = new Label();
            this.linkLabelPrivacyStatement = new LinkLabel();
            this.linkLabelCodeOfConduct = new LinkLabel();
            this.SuspendLayout();

            //
            // _labelPrivacyExplanation
            //
            this.labelPrivacyExplanation.FlatStyle = FlatStyle.System;
            this.labelPrivacyExplanation.Location = new Point(8, 32);
            this.labelPrivacyExplanation.Name = "labelPrivacyExplanation";
            this.labelPrivacyExplanation.Size = new Size(343, 57);
            this.labelPrivacyExplanation.TabIndex = 1;
            this.labelPrivacyExplanation.Text =
                "Your privacy is important. For more information about how Open Live Writer helps to protect it, see the:";

            //
            // _linkLabelPrivacyStatement
            //
            this.linkLabelPrivacyStatement.Location = new Point(32, 32);
            this.linkLabelPrivacyStatement.Name = "linkLabelPrivacyStatement";
            this.linkLabelPrivacyStatement.Size = new Size(319, 15);
            this.linkLabelPrivacyStatement.TabIndex = 2;
            this.linkLabelPrivacyStatement.TabStop = true;
            this.linkLabelPrivacyStatement.Text = "Microsoft Privacy statement";
            this.linkLabelPrivacyStatement.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelPrivacyStatement.LinkColor = SystemColors.HotTrack;
            this.linkLabelPrivacyStatement.LinkClicked +=
                PrivacyPreferencesPanel.linkLabelPrivacyStatement_LinkClicked;

            //
            // _linkLabelCodeOfConduct
            //
            this.linkLabelCodeOfConduct.Location = new Point(32, 32);
            this.linkLabelCodeOfConduct.Name = "linkLabelCodeOfConduct";
            this.linkLabelCodeOfConduct.Size = new Size(319, 15);
            this.linkLabelCodeOfConduct.TabIndex = 4;
            this.linkLabelCodeOfConduct.TabStop = true;
            this.linkLabelCodeOfConduct.Text = "Code of Conduct";
            this.linkLabelCodeOfConduct.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabelCodeOfConduct.LinkColor = SystemColors.HotTrack;
            this.linkLabelCodeOfConduct.LinkClicked += PrivacyPreferencesPanel.linkLabelCodeOfConduct_LinkClicked;

            //
            // PrivacyPreferencesPanel
            //
            this.Controls.Add(this.labelPrivacyExplanation);
            this.Controls.Add(this.linkLabelPrivacyStatement);
            this.Controls.Add(this.linkLabelCodeOfConduct);
            this.Name = "PrivacyPreferencesPanel";
            this.Controls.SetChildIndex(this.labelPrivacyExplanation, 0);
            this.Controls.SetChildIndex(this.linkLabelPrivacyStatement, 0);
            this.Controls.SetChildIndex(this.linkLabelCodeOfConduct, 0);
            this.ResumeLayout(false);
        }

        private void checkBoxOptIn_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.loading)
            {
                this.OnModified(EventArgs.Empty);
            }
        }

        private static void linkLabelPrivacyStatement_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
            ShellHelper.LaunchUrl("http://www.dotnetfoundation.org/privacy-policy");

        private static void linkLabelCodeOfConduct_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e) =>
            ShellHelper.LaunchUrl("http://www.dotnetfoundation.org/code-of-conduct");
    }
}
