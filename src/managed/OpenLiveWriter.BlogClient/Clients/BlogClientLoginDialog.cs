// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The BlogClientLoginDialog class.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.BaseForm" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.BaseForm" />
    public class BlogClientLoginDialog : BaseForm
    {
        /// <summary>
        /// The blog client login control
        /// </summary>
        private BlogClientLoginControl blogClientLoginControl;

        /// <summary>
        /// The button cancel
        /// </summary>
        private Button buttonCancel;

        /// <summary>
        /// The button ok
        /// </summary>
        private Button buttonOk;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogClientLoginDialog"/> class.
        /// </summary>
        public BlogClientLoginDialog()
        {
            // Required for Windows Form Designer support
            this.InitializeComponent();

            this.buttonOk.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.Login);
        }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>The domain.</value>
        public ICredentialsDomain Domain
        {
            get => this.blogClientLoginControl.Domain;
            set => this.blogClientLoginControl.Domain = value;
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get => this.blogClientLoginControl.Password;
            set => this.blogClientLoginControl.Password = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [save password].
        /// </summary>
        /// <value><c>true</c> if [save password]; otherwise, <c>false</c>.</value>
        public bool SavePassword
        {
            get => this.blogClientLoginControl.SavePassword;
            set => this.blogClientLoginControl.SavePassword = value;
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName
        {
            get => this.blogClientLoginControl.UserName;
            set => this.blogClientLoginControl.UserName = value;
        }

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
            using (LayoutHelper.SuspendAnchoring(this.blogClientLoginControl, this.buttonOk, this.buttonCancel))
            {
                LayoutHelper.FixupOKCancel(this.buttonOk, this.buttonCancel);
                LayoutHelper.NaturalizeHeightAndDistribute(
                    10,
                    this.blogClientLoginControl,
                    new ControlGroup(this.buttonOk, this.buttonCancel));
                this.ClientSize = new Size(this.ClientSize.Width, this.buttonOk.Bottom + 8);
            }

            if (this.buttonOk.Left < this.blogClientLoginControl.Left)
            {
                this.Width += this.blogClientLoginControl.Left - this.buttonOk.Left;
            }
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOk = new Button();
            this.buttonCancel = new Button();
            this.blogClientLoginControl = new BlogClientLoginControl();
            this.SuspendLayout();

            // buttonOK
            this.buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonOk.DialogResult = DialogResult.OK;
            this.buttonOk.FlatStyle = FlatStyle.System;
            this.buttonOk.Location = new Point(96, 154);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";

            // buttonCancel
            this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.System;
            this.buttonCancel.Location = new Point(184, 154);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "Cancel";

            // blogClientLoginControl
            this.blogClientLoginControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.blogClientLoginControl.Domain = null;
            this.blogClientLoginControl.Location = new Point(8, 8);
            this.blogClientLoginControl.Name = "blogClientLoginControl";
            this.blogClientLoginControl.Password = string.Empty;
            this.blogClientLoginControl.SavePassword = false;
            this.blogClientLoginControl.Size = new Size(253, 165);
            this.blogClientLoginControl.TabIndex = 0;
            this.blogClientLoginControl.UserName = string.Empty;

            // BlogClientLoginDialog
            this.AcceptButton = this.buttonOk;
            this.AutoScaleBaseSize = new Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(266, 184);
            this.ControlBox = false;
            this.Controls.Add(this.blogClientLoginControl);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlogClientLoginDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "te";
            this.ResumeLayout(false);
        }
    }
}
