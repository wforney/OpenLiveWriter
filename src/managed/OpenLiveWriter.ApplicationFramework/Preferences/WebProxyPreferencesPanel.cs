// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Summary description for WebProxyPreferencesPanel.
    /// Implements the <see cref="PreferencesPanel" />
    /// </summary>
    /// <seealso cref="PreferencesPanel" />
    public class WebProxyPreferencesPanel : PreferencesPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The group box proxy
        /// </summary>
        private GroupBox groupBoxProxy;

        /// <summary>
        /// The panel proxy settings
        /// </summary>
        private Panel panelProxySettings;

        /// <summary>
        /// The proxy password
        /// </summary>
        private TextBox proxyPassword;

        /// <summary>
        /// The proxy password label
        /// </summary>
        private Label proxyPasswordLabel;

        /// <summary>
        /// The proxy username
        /// </summary>
        private TextBox proxyUsername;

        /// <summary>
        /// The proxy username label
        /// </summary>
        private Label proxyUsernameLabel;

        /// <summary>
        /// The proxy port
        /// </summary>
        private TextBox proxyPort;

        /// <summary>
        /// The proxy port label
        /// </summary>
        private Label proxyPortLabel;

        /// <summary>
        /// The proxy server
        /// </summary>
        private TextBox proxyServer;

        /// <summary>
        /// The proxy server label
        /// </summary>
        private Label proxyServerLabel;

        /// <summary>
        /// The proxy enabled
        /// </summary>
        private CheckBox proxyEnabled;

        /// <summary>
        /// The web proxy preferences
        /// </summary>
        private WebProxyPreferences webProxyPreferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebProxyPreferencesPanel"/> class.
        /// </summary>
        public WebProxyPreferencesPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            this.groupBoxProxy.Text = Res.Get(StringId.ProxyPrefCustom);
            this.proxyPasswordLabel.Text = Res.Get(StringId.ProxyPrefPassword);
            this.proxyPassword.AccessibleName = ControlHelper.ToAccessibleName(this.proxyPasswordLabel.Text);
            this.proxyUsernameLabel.Text = Res.Get(StringId.ProxyPrefUsername);
            this.proxyUsername.AccessibleName = ControlHelper.ToAccessibleName(this.proxyUsernameLabel.Text);
            this.proxyPortLabel.Text = Res.Get(StringId.ProxyPrefPort);
            this.proxyPort.AccessibleName = ControlHelper.ToAccessibleName(this.proxyPortLabel.Text);
            this.proxyServerLabel.Text = Res.Get(StringId.ProxyPrefServer);
            this.proxyServer.AccessibleName = ControlHelper.ToAccessibleName(this.proxyServerLabel.Text);
            this.proxyEnabled.Text = Res.Get(StringId.ProxyPrefEnabled);
            this.PanelName = Res.Get(StringId.ProxyPrefName);

            this.PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Preferences.Images.WebProxyPanelBitmap.png");

            this.webProxyPreferences = new WebProxyPreferences();
            this.webProxyPreferences.PreferencesModified += new EventHandler(this._connectionsPreferences_PreferencesModified);

            this.proxyEnabled.Checked = this.webProxyPreferences.ProxyEnabled;
            this.proxyServer.Text = this.webProxyPreferences.Hostname;
            this.proxyPort.Text = this.webProxyPreferences.Port.ToString(CultureInfo.CurrentCulture);
            this.proxyUsername.Text = this.webProxyPreferences.Username;
            this.proxyPassword.Text = this.webProxyPreferences.Password;

            // give password field nice round circle
            this.proxyPassword.PasswordChar = Res.PasswordChar;

        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this.groupBoxProxy, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeight(this.proxyEnabled);
                LayoutHelper.FitControlsBelow(8, this.proxyEnabled);
                LayoutHelper.NaturalizeHeight(this.proxyServerLabel, this.proxyPortLabel);
                AlignHelper.AlignBottom(this.proxyServerLabel, this.proxyPortLabel);
                LayoutHelper.FitControlsBelow(3, this.proxyServerLabel);
            }
        }

        /// <summary>
        /// Saves the PreferencesPanel.
        /// </summary>
        public override void Save()
        {
            this.ApplyProxyPortToPreferences();

            //the no proxy address is set, then disable the proxy settings.
            if (this.proxyEnabled.Checked)
            {
                if (this.proxyServer.Text == string.Empty)
                {
                    this.proxyEnabled.Checked = false;
                }
            }

            if (this.proxyServer.Text == string.Empty)
            {
                this.proxyEnabled.Checked = false;
            }

            if (this.webProxyPreferences.IsModified())
            {
                this.webProxyPreferences.Save();
            }
        }

        /// <summary>
        /// Handles the PreferencesModified event of the _connectionsPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _connectionsPreferences_PreferencesModified(object sender, EventArgs e) => this.OnModified(EventArgs.Empty);

        /// <summary>
        /// Handles the CheckedChanged event of the proxyEnabled control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void proxyEnabled_CheckedChanged(object sender, EventArgs e)
        {
            this.panelProxySettings.Enabled = this.proxyEnabled.Checked;
            this.webProxyPreferences.ProxyEnabled = this.proxyEnabled.Checked;
        }

        /// <summary>
        /// Handles the TextChanged event of the proxyServer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void proxyServer_TextChanged(object sender, EventArgs e) =>
            this.webProxyPreferences.Hostname = this.proxyServer.Text;

        /// <summary>
        /// Handles the TextChanged event of the proxyPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void proxyPort_TextChanged(object sender, EventArgs e) =>
            //don't use the value in the control yet since it may not yet be a number.
            //set the modified flag so that the apply button gets enabled.
            this.OnModified(EventArgs.Empty);

        /// <summary>
        /// Handles the Leave event of the proxyPort control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void proxyPort_Leave(object sender, EventArgs e) => this.ApplyProxyPortToPreferences();

        /// <summary>
        /// Applies the proxy port to preferences.
        /// </summary>
        private void ApplyProxyPortToPreferences()
        {
            try
            {
                var portValue = int.Parse(this.proxyPort.Text, CultureInfo.CurrentCulture);
                if (this.webProxyPreferences.Port != portValue)
                {
                    this.webProxyPreferences.Port = portValue;
                }
            }
            catch (Exception)
            {
                //the number is malformed, so revert the value
                this.proxyPort.Text = this.webProxyPreferences.Port.ToString(CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the proxyUsername control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void proxyUsername_TextChanged(object sender, EventArgs e) =>
            this.webProxyPreferences.Username = this.proxyUsername.Text;

        /// <summary>
        /// Handles the TextChanged event of the proxyPassword control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void proxyPassword_TextChanged(object sender, EventArgs e) =>
            this.webProxyPreferences.Password = this.proxyPassword.Text;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.webProxyPreferences.PreferencesModified -= new EventHandler(this._connectionsPreferences_PreferencesModified);

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
        private void InitializeComponent()
        {
            this.groupBoxProxy = new GroupBox();
            this.panelProxySettings = new Panel();
            this.proxyPassword = new TextBox();
            this.proxyPasswordLabel = new Label();
            this.proxyUsername = new TextBox();
            this.proxyUsernameLabel = new Label();
            this.proxyPort = new TextBox();
            this.proxyPortLabel = new Label();
            this.proxyServer = new TextBox();
            this.proxyServerLabel = new Label();
            this.proxyEnabled = new CheckBox();
            this.groupBoxProxy.SuspendLayout();
            this.panelProxySettings.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxProxy
            //
            this.groupBoxProxy.Controls.Add(this.panelProxySettings);
            this.groupBoxProxy.Controls.Add(this.proxyEnabled);
            this.groupBoxProxy.FlatStyle = FlatStyle.System;
            this.groupBoxProxy.Location = new System.Drawing.Point(8, 32);
            this.groupBoxProxy.Name = "groupBoxProxy";
            this.groupBoxProxy.Size = new System.Drawing.Size(345, 155);
            this.groupBoxProxy.TabIndex = 1;
            this.groupBoxProxy.TabStop = false;
            this.groupBoxProxy.Text = "Custom proxy settings";
            //
            // panelProxySettings
            //
            this.panelProxySettings.Controls.Add(this.proxyPassword);
            this.panelProxySettings.Controls.Add(this.proxyPasswordLabel);
            this.panelProxySettings.Controls.Add(this.proxyUsername);
            this.panelProxySettings.Controls.Add(this.proxyUsernameLabel);
            this.panelProxySettings.Controls.Add(this.proxyPort);
            this.panelProxySettings.Controls.Add(this.proxyPortLabel);
            this.panelProxySettings.Controls.Add(this.proxyServer);
            this.panelProxySettings.Controls.Add(this.proxyServerLabel);
            this.panelProxySettings.Enabled = false;
            this.panelProxySettings.Location = new System.Drawing.Point(26, 42);
            this.panelProxySettings.Name = "panelProxySettings";
            this.panelProxySettings.Size = new System.Drawing.Size(303, 95);
            this.panelProxySettings.TabIndex = 2;
            //
            // proxyPassword
            //
            this.proxyPassword.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.proxyPassword.Location = new System.Drawing.Point(152, 64);
            this.proxyPassword.Name = "proxyPassword";
            this.proxyPassword.PasswordChar = '*';
            this.proxyPassword.Size = new System.Drawing.Size(142, 20);
            this.proxyPassword.TabIndex = 7;
            this.proxyPassword.Text = "";
            this.proxyPassword.Leave += new EventHandler(this.proxyPassword_TextChanged);
            //
            // label12
            //
            this.proxyPasswordLabel.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.proxyPasswordLabel.FlatStyle = FlatStyle.System;
            this.proxyPasswordLabel.Location = new System.Drawing.Point(152, 49);
            this.proxyPasswordLabel.Name = "proxyPasswordLabel";
            this.proxyPasswordLabel.Size = new System.Drawing.Size(135, 18);
            this.proxyPasswordLabel.TabIndex = 6;
            this.proxyPasswordLabel.Text = "Pass&word:";
            this.proxyPasswordLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyUsername
            //
            this.proxyUsername.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left)
                | AnchorStyles.Right)));
            this.proxyUsername.Location = new System.Drawing.Point(0, 64);
            this.proxyUsername.Name = "proxyUsername";
            this.proxyUsername.Size = new System.Drawing.Size(144, 20);
            this.proxyUsername.TabIndex = 5;
            this.proxyUsername.Text = "";
            this.proxyUsername.Leave += new EventHandler(this.proxyUsername_TextChanged);
            //
            // label10
            //
            this.proxyUsernameLabel.FlatStyle = FlatStyle.System;
            this.proxyUsernameLabel.Location = new System.Drawing.Point(0, 49);
            this.proxyUsernameLabel.Name = "proxyUsernameLabel";
            this.proxyUsernameLabel.Size = new System.Drawing.Size(137, 18);
            this.proxyUsernameLabel.TabIndex = 4;
            this.proxyUsernameLabel.Text = "User&name:";
            this.proxyUsernameLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyPort
            //
            this.proxyPort.Location = new System.Drawing.Point(218, 18);
            this.proxyPort.Name = "proxyPort";
            this.proxyPort.Size = new System.Drawing.Size(76, 20);
            this.proxyPort.TabIndex = 3;
            this.proxyPort.Text = "8080";
            this.proxyPort.TextChanged += new EventHandler(this.proxyPort_TextChanged);
            this.proxyPort.Leave += new EventHandler(this.proxyPort_Leave);
            //
            // label9
            //
            this.proxyPortLabel.FlatStyle = FlatStyle.System;
            this.proxyPortLabel.Location = new System.Drawing.Point(218, 3);
            this.proxyPortLabel.Name = "proxyPortLabel";
            this.proxyPortLabel.Size = new System.Drawing.Size(76, 18);
            this.proxyPortLabel.TabIndex = 2;
            this.proxyPortLabel.Text = "&Port:";
            this.proxyPortLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyServer
            //
            this.proxyServer.Location = new System.Drawing.Point(0, 18);
            this.proxyServer.Name = "proxyServer";
            this.proxyServer.Size = new System.Drawing.Size(210, 20);
            this.proxyServer.TabIndex = 1;
            this.proxyServer.Text = "";
            this.proxyServer.TextChanged += new EventHandler(this.proxyServer_TextChanged);
            //
            // label8
            //
            this.proxyServerLabel.FlatStyle = FlatStyle.System;
            this.proxyServerLabel.Location = new System.Drawing.Point(0, 3);
            this.proxyServerLabel.Name = "proxyServerLabel";
            this.proxyServerLabel.Size = new System.Drawing.Size(208, 18);
            this.proxyServerLabel.TabIndex = 0;
            this.proxyServerLabel.Text = "Proxy &server address:";
            this.proxyServerLabel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            //
            // proxyEnabled
            //
            this.proxyEnabled.FlatStyle = FlatStyle.System;
            this.proxyEnabled.Location = new System.Drawing.Point(10, 20);
            this.proxyEnabled.Name = "proxyEnabled";
            this.proxyEnabled.Size = new System.Drawing.Size(324, 18);
            this.proxyEnabled.TabIndex = 0;
            this.proxyEnabled.Text = "&Specify custom proxy server settings";
            this.proxyEnabled.CheckedChanged += new EventHandler(this.proxyEnabled_CheckedChanged);
            //
            // WebProxyPreferencesPanel
            //
            this.AccessibleName = "Web Proxy";
            this.Controls.Add(this.groupBoxProxy);
            this.Name = "WebProxyPreferencesPanel";
            this.PanelName = "Web Proxy";
            this.Controls.SetChildIndex(this.groupBoxProxy, 0);
            this.groupBoxProxy.ResumeLayout(false);
            this.panelProxySettings.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
    }
}
