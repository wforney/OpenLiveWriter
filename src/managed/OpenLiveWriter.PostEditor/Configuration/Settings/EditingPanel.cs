// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor.Configuration.Settings
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    using OpenLiveWriter.BlogClient;
    using OpenLiveWriter.BlogClient.Detection;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Summary description for EditingPanel.
    /// </summary>
    public class EditingPanel : WeblogSettingsPanel
    {
        private System.Windows.Forms.GroupBox groupBoxWeblogStyle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelEditUsingStyle;
        private System.Windows.Forms.Button buttonUpdateStyle;
        private System.Windows.Forms.Panel panelBrowserParent;
        private GroupBox groupBoxRTL;
        private Label labelRTL;
        private ComboBox comboBoxRTL;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public EditingPanel()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
            this.UpdateStrings();
        }

        public EditingPanel(TemporaryBlogSettings targetBlogSettings, TemporaryBlogSettings editableBlogSettings)
            : base(targetBlogSettings, editableBlogSettings)
        {
            this.InitializeComponent();
            this.UpdateStrings();
            this.PanelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Configuration.Settings.Images.EditingPanelBitmap.png");

            this.labelEditUsingStyle.Text = String.Format(CultureInfo.CurrentCulture, this.labelEditUsingStyle.Text, ApplicationEnvironment.ProductName);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!this.DesignMode)
            {
                DisplayHelper.AutoFitSystemButton(this.buttonUpdateStyle);
                // add some padding to the button, otherwise it looks crowded
                this.buttonUpdateStyle.Width += (int)Math.Ceiling(DisplayHelper.ScaleX(14));
                this.comboBoxRTL.Left = this.buttonUpdateStyle.Left + 1;

                LayoutHelper.FixupGroupBox(this.groupBoxWeblogStyle);
                LayoutHelper.FixupGroupBox(this.groupBoxRTL);

                LayoutHelper.DistributeVertically(8, this.groupBoxWeblogStyle, this.groupBoxRTL);

                int maxWidth = this.groupBoxRTL.Width - this.comboBoxRTL.Left - 8;
                DisplayHelper.AutoFitSystemCombo(this.comboBoxRTL, this.comboBoxRTL.Width, maxWidth, false);
            }
        }

        private void UpdateStrings()
        {
            this.groupBoxWeblogStyle.Text = Res.Get(StringId.EditingStyle);
            this.label1.Text = Res.Get(StringId.EditingText);
            this.labelEditUsingStyle.Text = Res.Get(StringId.EditingUsing);
            this.buttonUpdateStyle.Text = Res.Get(StringId.EditingUpdate);
            this.PanelName = Res.Get(StringId.EditingName);
            this.groupBoxRTL.Text = Res.Get(StringId.EditingRTLName);
            this.labelRTL.Text = Res.Get(StringId.EditingRTLExplanation);

            bool? useRTL = this.TemporaryBlogSettings.HomePageOverrides.Keys.Contains(BlogClientOptions.TEMPLATE_IS_RTL)
                ? (bool?)StringHelper.ToBool(this.TemporaryBlogSettings.HomePageOverrides[BlogClientOptions.TEMPLATE_IS_RTL].ToString(), false)
                : null;

            useRTL = this.TemporaryBlogSettings.OptionOverrides.Keys.Contains(BlogClientOptions.TEMPLATE_IS_RTL)
                ? StringHelper.ToBool(this.TemporaryBlogSettings.OptionOverrides[BlogClientOptions.TEMPLATE_IS_RTL].ToString(), false)
                : useRTL;

            // The default setting only comes from the homepage or manifest
            this.comboBoxRTL.Items.Add(String.Format(CultureInfo.CurrentUICulture, Res.Get(StringId.EditingRTLDefault), (useRTL == true ? Res.Get(StringId.EditingRTLYes) : Res.Get(StringId.EditingRTLNo))));
            this.comboBoxRTL.Items.Add(Res.Get(StringId.EditingRTLYes));
            this.comboBoxRTL.Items.Add(Res.Get(StringId.EditingRTLNo));

            // Though the value of the combo box can come from homepage/manifest/user options
            if (this.TemporaryBlogSettings.UserOptionOverrides.Keys.Contains(BlogClientOptions.TEMPLATE_IS_RTL))
            {
                // Select the correct option from the combobox
                if (StringHelper.ToBool(this.TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.TEMPLATE_IS_RTL].ToString(), false))
                {
                    this.comboBoxRTL.SelectedIndex = 1;
                }
                else
                {
                    this.comboBoxRTL.SelectedIndex = 2;
                }
            }
            else
            {
                this.comboBoxRTL.SelectedIndex = 0;
            }
        }

        private void buttonUpdateStyle_Click(object sender, System.EventArgs e)
        {
            try
            {
                using (Blog blog = new Blog(this.TemporaryBlogSettings))
                {
                    if (blog.VerifyCredentials())
                    {
                        BlogClientUIContextImpl uiContext = new BlogClientUIContextImpl(this.FindForm());
                        Color? backgroundColor;
                        BlogEditingTemplateFile[] editingTemplates = BlogEditingTemplateDetector.DetectTemplate(
                            uiContext,
                            this.panelBrowserParent,
                            this.TemporaryBlogSettings,
                            !this.BlogIsAutoUpdatable(blog),
                            out backgroundColor); // only probe for manifest if blog is not auto-updatable
                        if (editingTemplates.Length != 0)
                        {
                            this.TemporaryBlogSettings.TemplateFiles = editingTemplates;
                            if (backgroundColor != null)
                            {
                                this.TemporaryBlogSettings.UpdatePostBodyBackgroundColor(backgroundColor.Value);
                            }

                            this.TemporaryBlogSettingsModified = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(this.FindForm(), ex, "Unexpected Error Updating Style");
            }
        }

        private bool BlogIsAutoUpdatable(Blog blog)
        {
            return PostEditorSettings.AllowSettingsAutoUpdate && blog.ClientOptions.SupportsAutoUpdate;
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
        private void InitializeComponent()
        {
            this.groupBoxWeblogStyle = new System.Windows.Forms.GroupBox();
            this.panelBrowserParent = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.labelEditUsingStyle = new System.Windows.Forms.Label();
            this.buttonUpdateStyle = new System.Windows.Forms.Button();
            this.groupBoxRTL = new System.Windows.Forms.GroupBox();
            this.comboBoxRTL = new System.Windows.Forms.ComboBox();
            this.labelRTL = new System.Windows.Forms.Label();
            this.groupBoxWeblogStyle.SuspendLayout();
            this.groupBoxRTL.SuspendLayout();
            this.SuspendLayout();
            //
            // groupBoxWeblogStyle
            //
            this.groupBoxWeblogStyle.Controls.Add(this.panelBrowserParent);
            this.groupBoxWeblogStyle.Controls.Add(this.label1);
            this.groupBoxWeblogStyle.Controls.Add(this.labelEditUsingStyle);
            this.groupBoxWeblogStyle.Controls.Add(this.buttonUpdateStyle);
            this.groupBoxWeblogStyle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxWeblogStyle.Location = new System.Drawing.Point(8, 32);
            this.groupBoxWeblogStyle.Name = "groupBoxWeblogStyle";
            this.groupBoxWeblogStyle.Size = new System.Drawing.Size(352, 152);
            this.groupBoxWeblogStyle.TabIndex = 1;
            this.groupBoxWeblogStyle.TabStop = false;
            this.groupBoxWeblogStyle.Text = "Weblog Style";
            //
            // panelBrowserParent
            //
            this.panelBrowserParent.Location = new System.Drawing.Point(232, 110);
            this.panelBrowserParent.Name = "panelBrowserParent";
            this.panelBrowserParent.Size = new System.Drawing.Size(32, 24);
            this.panelBrowserParent.TabIndex = 7;
            this.panelBrowserParent.Visible = false;
            //
            // label1
            //
            this.label1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.label1.Location = new System.Drawing.Point(16, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(320, 32);
            this.label1.TabIndex = 5;
            this.label1.Text = "If you have changed the visual style of your weblog you can update your local edi" +
                "ting template by using the button below.";
            //
            // labelEditUsingStyle
            //
            this.labelEditUsingStyle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelEditUsingStyle.Location = new System.Drawing.Point(16, 21);
            this.labelEditUsingStyle.Name = "labelEditUsingStyle";
            this.labelEditUsingStyle.Size = new System.Drawing.Size(320, 40);
            this.labelEditUsingStyle.TabIndex = 4;
            this.labelEditUsingStyle.Text = "{0} enables you to edit using the visual style of your weblog. This enables you t" +
                "o see what your posts will look like online while you are editing them.";
            //
            // buttonUpdateStyle
            //
            this.buttonUpdateStyle.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonUpdateStyle.Location = new System.Drawing.Point(16, 110);
            this.buttonUpdateStyle.Name = "buttonUpdateStyle";
            this.buttonUpdateStyle.Size = new System.Drawing.Size(131, 23);
            this.buttonUpdateStyle.TabIndex = 6;
            this.buttonUpdateStyle.Text = "&Update Style";
            this.buttonUpdateStyle.Click += new System.EventHandler(this.buttonUpdateStyle_Click);
            //
            // groupBoxRTL
            //
            this.groupBoxRTL.Controls.Add(this.comboBoxRTL);
            this.groupBoxRTL.Controls.Add(this.labelRTL);
            this.groupBoxRTL.Location = new System.Drawing.Point(8, 191);
            this.groupBoxRTL.Name = "groupBoxRTL";
            this.groupBoxRTL.Size = new System.Drawing.Size(348, 91);
            this.groupBoxRTL.TabIndex = 2;
            this.groupBoxRTL.TabStop = false;
            this.groupBoxRTL.Text = "Text Direction";
            //
            // comboBoxRTL
            //
            this.comboBoxRTL.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRTL.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxRTL.FormattingEnabled = true;
            this.comboBoxRTL.Location = new System.Drawing.Point(16, 54);
            this.comboBoxRTL.Name = "comboBoxRTL";
            this.comboBoxRTL.Size = new System.Drawing.Size(195, 23);
            this.comboBoxRTL.TabIndex = 2;
            this.comboBoxRTL.SelectedIndexChanged += new System.EventHandler(this.comboBoxRTL_SelectedIndexChanged);
            //
            // labelRTL
            //
            this.labelRTL.Location = new System.Drawing.Point(16, 21);
            this.labelRTL.Name = "labelRTL";
            this.labelRTL.Size = new System.Drawing.Size(320, 40);
            this.labelRTL.TabIndex = 0;
            this.labelRTL.Text = "Writer detects whether the template for your blog uses right-to-left text.";
            //
            // EditingPanel
            //
            this.AccessibleName = "Editing";
            this.Controls.Add(this.groupBoxRTL);
            this.Controls.Add(this.groupBoxWeblogStyle);
            this.Name = "EditingPanel";
            this.PanelName = "Editing";
            this.Controls.SetChildIndex(this.groupBoxWeblogStyle, 0);
            this.Controls.SetChildIndex(this.groupBoxRTL, 0);
            this.groupBoxWeblogStyle.ResumeLayout(false);
            this.groupBoxRTL.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private void comboBoxRTL_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool? useRTL = null;

            // Check to see if it is anything besides default
            if (this.comboBoxRTL.Text == Res.Get(StringId.EditingRTLYes))
            {
                useRTL = true;
            }
            else if (this.comboBoxRTL.Text == Res.Get(StringId.EditingRTLNo))
            {
                useRTL = false;
            }

            // If the setting is default we remove it
            if (useRTL != null)
            {
                this.TemporaryBlogSettings.UserOptionOverrides[BlogClientOptions.TEMPLATE_IS_RTL] = useRTL.ToString();
            }
            else
            {
                this.TemporaryBlogSettings.UserOptionOverrides.Remove(BlogClientOptions.TEMPLATE_IS_RTL);
            }

            this.TemporaryBlogSettingsModified = true;
        }

    }
}
