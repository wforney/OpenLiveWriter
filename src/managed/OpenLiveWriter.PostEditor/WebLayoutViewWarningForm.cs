// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    using Controls;

    using CoreServices.Layout;

    using Localization;

    /// <summary>
    /// The WebLayoutViewWarningForm class.
    /// Implements the <see cref="OpenLiveWriter.Controls.ApplicationDialog" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.ApplicationDialog" />
    public class WebLayoutViewWarningForm : ApplicationDialog
    {
        /// <summary>
        /// The button ok
        /// </summary>
        private Button buttonOK;

        /// <summary>
        /// The check box dont show message again
        /// </summary>
        private CheckBox checkBoxDontShowMessageAgain;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label caption1
        /// </summary>
        private Label labelCaption1;

        /// <summary>
        /// The label caption2
        /// </summary>
        private Label labelCaption2;

        /// <summary>
        /// The picture box1
        /// </summary>
        private PictureBox pictureBox1;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebLayoutViewWarningForm"/> class.
        /// </summary>
        public WebLayoutViewWarningForm()
        {
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.checkBoxDontShowMessageAgain.Text = Res.Get(StringId.DontShowAgain);
            this.labelCaption1.Text = Res.Get(StringId.WebLayoutWarning1);
            this.labelCaption2.Text = Res.Get(StringId.WebLayoutWarning2);
            this.Text = Res.Get(StringId.WebLayoutWarningTitle);
        }

        /// <summary>
        /// Gets a value indicating whether [dont show message again].
        /// </summary>
        /// <value><c>true</c> if [dont show message again]; otherwise, <c>false</c>.</value>
        public bool DontShowMessageAgain => this.checkBoxDontShowMessageAgain.Checked;

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, true))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(12, this.labelCaption1, this.labelCaption2,
                                                           this.checkBoxDontShowMessageAgain);
                this.buttonOK.Top = this.checkBoxDontShowMessageAgain.Bottom + 16;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources =
                new System.Resources.ResourceManager(typeof(WebLayoutViewWarningForm));
            this.buttonOK = new System.Windows.Forms.Button();
            this.checkBoxDontShowMessageAgain = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.labelCaption1 = new System.Windows.Forms.Label();
            this.labelCaption2 = new System.Windows.Forms.Label();
            this.SuspendLayout();

            //
            // buttonOK
            //
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(116, 152);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";

            //
            // checkBoxDontShowMessageAgain
            //
            this.checkBoxDontShowMessageAgain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxDontShowMessageAgain.Location = new System.Drawing.Point(56, 112);
            this.checkBoxDontShowMessageAgain.Name = "checkBoxDontShowMessageAgain";
            this.checkBoxDontShowMessageAgain.Size = new System.Drawing.Size(240, 24);
            this.checkBoxDontShowMessageAgain.TabIndex = 1;
            this.checkBoxDontShowMessageAgain.Text = "Don\'t show this message again";

            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image) (resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(39, 40);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;

            //
            // labelCaption1
            //
            this.labelCaption1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption1.Location = new System.Drawing.Point(56, 8);
            this.labelCaption1.Name = "labelCaption1";
            this.labelCaption1.Size = new System.Drawing.Size(240, 56);
            this.labelCaption1.TabIndex = 3;
            this.labelCaption1.Text =
                "You may have problems with editing content in Web Layout view with the current We" +
                "blog (for example, incorrect post layout or problems selecting and resi" +
                "zing content).";

            //
            // labelCaption2
            //
            this.labelCaption2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelCaption2.Location = new System.Drawing.Point(56, 72);
            this.labelCaption2.Name = "labelCaption2";
            this.labelCaption2.Size = new System.Drawing.Size(240, 24);
            this.labelCaption2.TabIndex = 4;
            this.labelCaption2.Text = "If you do experience problems you should switch back to the Normal view.";

            //
            // WebLayoutViewWarningForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(306, 184);
            this.Controls.Add(this.labelCaption2);
            this.Controls.Add(this.labelCaption1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.checkBoxDontShowMessageAgain);
            this.Controls.Add(this.buttonOK);
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WebLayoutViewWarningForm";
            this.Text = "Web Layout View";
            this.ResumeLayout(false);
        }

        #endregion
    }
}
