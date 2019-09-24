// <copyright file="GDataCaptchaForm.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The Google data captcha form class.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.BaseForm" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.BaseForm" />
    public class GDataCaptchaForm : BaseForm
    {
        /// <summary>
        /// The BTN cancel
        /// </summary>
        private Button btnCancel;

        /// <summary>
        /// The BTN ok
        /// </summary>
        private Button btnOk;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label1
        /// </summary>
        private Label label1;

        /// <summary>
        /// The link label1
        /// </summary>
        private LinkLabel linkLabel1;

        /// <summary>
        /// The pic captcha
        /// </summary>
        private PictureBox picCaptcha;

        /// <summary>
        /// The text captcha
        /// </summary>
        private TextBox txtCaptcha;

        /// <summary>
        /// Initializes a new instance of the <see cref="GDataCaptchaForm"/> class.
        /// </summary>
        public GDataCaptchaForm()
        {
            // Required for Windows Form Designer support
            this.InitializeComponent();

            this.label1.Text = Res.Get(StringId.GDataCaptchaPrompt);
            this.btnCancel.Text = Res.Get(StringId.CancelButton);
            this.btnOk.Text = Res.Get(StringId.OKButtonText);
            this.linkLabel1.Text = Res.Get(StringId.GDataCaptchaAlternate);
            this.Text = Res.Get(StringId.GDataCaptchaTitle);
        }

        /// <summary>
        /// Gets the reply.
        /// </summary>
        /// <value>The reply.</value>
        public string Reply => this.txtCaptcha.Text;

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="img">The image.</param>
        public void SetImage(Image img) => this.picCaptcha.Image = img;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(4, this.label1, this.linkLabel1);
                LayoutHelper.DistributeVertically(12, false, this.linkLabel1, this.picCaptcha);
                LayoutHelper.DistributeVertically(4, this.picCaptcha, this.txtCaptcha);
                LayoutHelper.DistributeVertically(
                    12,
                    false,
                    this.txtCaptcha,
                    new ControlGroup(this.btnOk, this.btnCancel));
                LayoutHelper.FixupOKCancel(this.btnOk, this.btnCancel);
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.picCaptcha = new PictureBox();
            this.txtCaptcha = new TextBox();
            this.label1 = new Label();
            this.btnCancel = new Button();
            this.btnOk = new Button();
            this.linkLabel1 = new LinkLabel();
            ((ISupportInitialize)this.picCaptcha).BeginInit();
            this.SuspendLayout();

            // picCaptcha
            this.picCaptcha.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.picCaptcha.BorderStyle = BorderStyle.Fixed3D;
            this.picCaptcha.Location = new Point(10, 91);
            this.picCaptcha.Name = "picCaptcha";
            this.picCaptcha.Size = new Size(197, 80);
            this.picCaptcha.TabIndex = 0;
            this.picCaptcha.TabStop = false;

            // txtCaptcha
            this.txtCaptcha.Location = new Point(10, 178);
            this.txtCaptcha.Name = "txtCaptcha";
            this.txtCaptcha.Size = new Size(197, 23);
            this.txtCaptcha.TabIndex = 0;

            // label1
            this.label1.Location = new Point(10, 9);
            this.label1.Name = "label1";
            this.label1.Size = new Size(194, 37);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type the characters you see in the picture below.";

            // btnCancel
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(130, 190);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(77, 26);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";

            // btnOK
            this.btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnOk.DialogResult = DialogResult.OK;
            this.btnOk.Location = new Point(43, 190);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new Size(77, 26);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";

            // linkLabel1
            this.linkLabel1.LinkBehavior = LinkBehavior.HoverUnderline;
            this.linkLabel1.LinkColor = SystemColors.HotTrack;
            this.linkLabel1.Location = new Point(10, 46);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new Size(197, 36);
            this.linkLabel1.TabIndex = 4;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "Visually impaired users can access an audio version of the test here.";
            this.linkLabel1.LinkClicked += this.linkLabel1_LinkClicked;

            // GDataCaptchaForm
            this.AcceptButton = this.btnOk;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new Size(216, 224);
            this.ControlBox = false;
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.txtCaptcha);
            this.Controls.Add(this.picCaptcha);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.Name = "GDataCaptchaForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Unlock Google Account";
            ((ISupportInitialize)this.picCaptcha).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// Handles the LinkClicked event of the linkLabel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="LinkLabelLinkClickedEventArgs"/> instance containing the event data.</param>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.google.com/accounts/DisplayUnlockCaptcha?service=blogger");
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
