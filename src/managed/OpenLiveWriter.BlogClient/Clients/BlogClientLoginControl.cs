// <copyright file="BlogClientLoginControl.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// The BlogClientLoginControl class.
    /// Implements the <see cref="System.Windows.Forms.UserControl" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    public class BlogClientLoginControl : UserControl
    {
        /// <summary>
        /// The domain
        /// </summary>
        private ICredentialsDomain domain;

        /// <summary>
        /// The domain icon
        /// </summary>
        private Icon domainIcon;

        /// <summary>
        /// The domain image
        /// </summary>
        private Image domainImage;

        /// <summary>
        /// The domain image size
        /// </summary>
        private Size domainImageSize;

        /// <summary>
        /// The check box save password
        /// </summary>
        private CheckBox checkBoxSavePassword;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label1
        /// </summary>
        private Label label1;

        /// <summary>
        /// The label2
        /// </summary>
        private Label label2;

        /// <summary>
        /// The scale
        /// </summary>
        private PointF scale = new PointF(1f, 1f);

        /// <summary>
        /// The text box email
        /// </summary>
        private TextBox textBoxEmail;

        /// <summary>
        /// The textbox login domain
        /// </summary>
        private Label textboxLoginDomain;

        /// <summary>
        /// The textbox login domain description
        /// </summary>
        private Label textboxLoginDomainDescription;

        /// <summary>
        /// The text box password
        /// </summary>
        private TextBox textBoxPassword;

        /// <inheritdoc />
        public BlogClientLoginControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            this.textBoxPassword.PasswordChar = Res.PasswordChar;

            this.label1.Text = Res.Get(StringId.UsernameLabel);
            this.label2.Text = Res.Get(StringId.PasswordLabel);
            this.checkBoxSavePassword.Text = Res.Get(StringId.RememberPassword);

            if (!this.DesignMode)
            {
                this.textboxLoginDomain.Text = string.Empty;
                this.textboxLoginDomainDescription.Text = string.Empty;
            }
        }

        /// <summary>
        /// Gets or sets the domain.
        /// </summary>
        /// <value>The domain.</value>
        public ICredentialsDomain Domain
        {
            get => this.domain;
            set
            {
                this.domain = value;
                this.domainIcon?.Dispose();

                this.domainIcon = null;

                this.domainImage?.Dispose();

                this.domainImage = null;

                if (this.domain != null)
                {
                    this.textboxLoginDomain.Text = this.domain.Name ?? string.Empty;
                    this.textboxLoginDomainDescription.Text = this.domain.Description ?? string.Empty;
                }
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password
        {
            get => this.textBoxPassword.Text;
            set => this.textBoxPassword.Text = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [save password].
        /// </summary>
        /// <value><c>true</c> if [save password]; otherwise, <c>false</c>.</value>
        public bool SavePassword
        {
            get => this.checkBoxSavePassword.Checked;
            set => this.checkBoxSavePassword.Checked = value;
        }

        /// <summary>
        /// Gets or sets the name of the user.
        /// </summary>
        /// <value>The name of the user.</value>
        public string UserName
        {
            get => this.textBoxEmail.Text;
            set => this.textBoxEmail.Text = value;
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

                this.domainIcon?.Dispose();

                this.domainImage?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc />
        protected override void OnLayout(LayoutEventArgs layoutEventArgs)
        {
            base.OnLayout(layoutEventArgs);

            this.domainImageSize = new Size((int)(16 * this.scale.X), (int)(16 * this.scale.Y));

            if (this.domain != null)
            {
                this.checkBoxSavePassword.Visible = this.domain.AllowsSavePassword;

                if (this.domainImage == null && this.domainIcon == null)
                {
                    try
                    {
                        if (this.domain.Image != null)
                        {
                            this.domainImage = new Bitmap(new MemoryStream(this.domain.Image));
                        }
                        else if (this.domain.Icon != null)
                        {
                            this.domainIcon = new Icon(
                                new MemoryStream(this.domain.Icon),
                                this.domainImageSize.Width,
                                this.domainImageSize.Height);
                        }
                        else
                        {
                            var ico = ApplicationEnvironment.ProductIconSmall;
                            this.domainIcon = new Icon(ico, ico.Size);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            if (this.domainImage == null && this.domainIcon == null)
            {
                var appIcon = ApplicationEnvironment.ProductIconSmall;
                this.domainIcon = new Icon(appIcon, appIcon.Width, appIcon.Height);
            }
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(3, this.label1, this.textBoxEmail);
                LayoutHelper.NaturalizeHeightAndDistribute(
                    3,
                    this.label2,
                    this.textBoxPassword,
                    this.checkBoxSavePassword);
                LayoutHelper.NaturalizeHeightAndDistribute(
                    8,
                    new ControlGroup(this.label1, this.textBoxEmail),
                    new ControlGroup(this.label2, this.textBoxPassword, this.checkBoxSavePassword));
            }

            // force the initial focus to the password control if there is already a user name
            if (!string.IsNullOrEmpty(this.textBoxEmail.Text))
            {
                this.textBoxPassword.Select();
            }
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = new BidiGraphics(e.Graphics, this.ClientRectangle);

            // draw icon
            if (this.domainImage != null)
            {
                g.DrawImage(
                    false,
                    this.domainImage,
                    new Rectangle(0, 0, this.domainImageSize.Width, this.domainImageSize.Height));
            }
            else if (this.domainIcon != null)
            {
                g.DrawIcon(
                    false,
                    this.domainIcon,
                    new Rectangle(0, 0, this.domainImageSize.Width, this.domainImageSize.Height));
            }
        }

        /// <inheritdoc />
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        /// <inheritdoc />
        protected override void ScaleCore(float dx, float dy)
        {
            this.SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.label2 = new Label();
            this.textBoxEmail = new TextBox();
            this.textBoxPassword = new TextBox();
            this.checkBoxSavePassword = new CheckBox();
            this.textboxLoginDomain = new Label();
            this.textboxLoginDomainDescription = new Label();
            this.SuspendLayout();

            // label1
            this.label1.FlatStyle = FlatStyle.System;
            this.label1.Location = new Point(24, 40);
            this.label1.Name = "label1";
            this.label1.Size = new Size(189, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Username:";
            this.label1.TextAlign = ContentAlignment.MiddleLeft;

            // label2
            this.label2.FlatStyle = FlatStyle.System;
            this.label2.Location = new Point(24, 80);
            this.label2.Name = "label2";
            this.label2.Size = new Size(200, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "&Password:";
            this.label2.TextAlign = ContentAlignment.MiddleLeft;

            // textBoxEmail
            this.textBoxEmail.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textBoxEmail.Location = new Point(24, 56);
            this.textBoxEmail.Name = "textBoxEmail";
            this.textBoxEmail.Size = new Size(189, 20);
            this.textBoxEmail.TabIndex = 1;

            // textBoxPassword
            this.textBoxPassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textBoxPassword.Location = new Point(24, 96);
            this.textBoxPassword.Name = "textBoxPassword";
            this.textBoxPassword.PasswordChar = '*';
            this.textBoxPassword.Size = new Size(189, 20);
            this.textBoxPassword.TabIndex = 3;

            // checkBoxSavePassword
            this.checkBoxSavePassword.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.checkBoxSavePassword.FlatStyle = FlatStyle.System;
            this.checkBoxSavePassword.Location = new Point(24, 120);
            this.checkBoxSavePassword.Name = "checkBoxSavePassword";
            this.checkBoxSavePassword.Size = new Size(189, 32);
            this.checkBoxSavePassword.TabIndex = 4;
            this.checkBoxSavePassword.Text = "&Remember my password";
            this.checkBoxSavePassword.TextAlign = ContentAlignment.TopLeft;

            // textboxLoginDomain
            this.textboxLoginDomain.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textboxLoginDomain.FlatStyle = FlatStyle.System;
            this.textboxLoginDomain.Location = new Point(24, 0);
            this.textboxLoginDomain.Name = "textboxLoginDomain";
            this.textboxLoginDomain.Size = new Size(189, 16);
            this.textboxLoginDomain.TabIndex = 4;
            this.textboxLoginDomain.Text = "Blog Service";

            // textboxLoginDomainDescription
            this.textboxLoginDomainDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.textboxLoginDomainDescription.FlatStyle = FlatStyle.System;
            this.textboxLoginDomainDescription.ForeColor = SystemColors.GrayText;
            this.textboxLoginDomainDescription.Location = new Point(24, 16);
            this.textboxLoginDomainDescription.Name = "textboxLoginDomainDescription";
            this.textboxLoginDomainDescription.Size = new Size(189, 24);
            this.textboxLoginDomainDescription.TabIndex = 5;
            this.textboxLoginDomainDescription.Text = "(Blog Title)";

            // BlogClientLoginControl
            this.Controls.Add(this.textboxLoginDomainDescription);
            this.Controls.Add(this.textboxLoginDomain);
            this.Controls.Add(this.checkBoxSavePassword);
            this.Controls.Add(this.textBoxPassword);
            this.Controls.Add(this.textBoxEmail);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "BlogClientLoginControl";
            this.Size = new Size(216, 168);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        /// <summary>
        /// Saves the scale.
        /// </summary>
        /// <param name="dx">The x.</param>
        /// <param name="dy">The y.</param>
        private void SaveScale(float dx, float dy) => this.scale = new PointF(this.scale.X * dx, this.scale.Y * dy);
    }
}
