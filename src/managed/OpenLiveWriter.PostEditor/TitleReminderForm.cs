// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;

    using Controls;

    using CoreServices;
    using CoreServices.Layout;

    using Localization;

    /// <summary>
    /// Summary description for TitleReminderForm.
    /// Implements the <see cref="OpenLiveWriter.Controls.ApplicationDialog" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.ApplicationDialog" />
    public class TitleReminderForm : ApplicationDialog
    {
        /// <summary>
        /// The button no
        /// </summary>
        private Button buttonNo;

        /// <summary>
        /// The button yes
        /// </summary>
        private Button buttonYes;

        /// <summary>
        /// The cb dont show again
        /// </summary>
        private CheckBox cbDontShowAgain;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label error
        /// </summary>
        private Label labelError;

        /// <summary>
        /// The label explanation
        /// </summary>
        private Label labelExplanation;

        /// <summary>
        /// The picture box1
        /// </summary>
        private PictureBox pictureBox1;

        /// <summary>
        /// Initializes a new instance of the <see cref="TitleReminderForm"/> class.
        /// </summary>
        /// <param name="isPage">if set to <c>true</c> [is page].</param>
        public TitleReminderForm(bool isPage)
        {
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.cbDontShowAgain.Text = Res.Get(StringId.TitleRemindDontShowAgain);
            this.labelExplanation.Text = Res.Get(StringId.TitleRemindExplanation);
            this.labelError.Text = Res.Get(StringId.TitleRemindError);
            this.buttonYes.Text = Res.Get(StringId.Yes);
            this.buttonNo.Text = Res.Get(StringId.No);
            this.Text = Res.Get(StringId.TitleRemindTitle);

            this.labelError.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
            this.labelError.Text = string.Format(CultureInfo.CurrentCulture, this.labelError.Text,
                                                 isPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post));
            this.labelExplanation.Text = string.Format(CultureInfo.CurrentCulture, this.labelExplanation.Text,
                                                       isPage
                                                           ? Res.Get(StringId.PageLower)
                                                           : Res.Get(StringId.PostLower));
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(12, this.labelError, this.labelExplanation,
                                                           new ControlGroup(this.buttonYes, this.buttonNo),
                                                           this.cbDontShowAgain);
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

        /// <summary>
        /// Handles the <see cref="E:Closed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (this.DialogResult != DialogResult.Cancel)
            {
                PostEditorSettings.TitleReminder = !this.cbDontShowAgain.Checked;
            }
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources =
                new System.ComponentModel.ComponentResourceManager(typeof(TitleReminderForm));
            this.cbDontShowAgain = new System.Windows.Forms.CheckBox();
            this.labelExplanation = new System.Windows.Forms.Label();
            this.labelError = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonYes = new System.Windows.Forms.Button();
            this.buttonNo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).BeginInit();
            this.SuspendLayout();

            //
            // cbDontShowAgain
            //
            this.cbDontShowAgain.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                      )));
            this.cbDontShowAgain.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.cbDontShowAgain.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cbDontShowAgain.Location = new System.Drawing.Point(72, 128);
            this.cbDontShowAgain.Name = "cbDontShowAgain";
            this.cbDontShowAgain.Size = new System.Drawing.Size(208, 32);
            this.cbDontShowAgain.TabIndex = 4;
            this.cbDontShowAgain.Text = "&Don\'t remind me about missing titles in the future";
            this.cbDontShowAgain.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            //
            // labelExplanation
            //
            this.labelExplanation.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                      )));
            this.labelExplanation.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelExplanation.Location = new System.Drawing.Point(72, 48);
            this.labelExplanation.Name = "labelExplanation";
            this.labelExplanation.Size = new System.Drawing.Size(216, 44);
            this.labelExplanation.TabIndex = 1;
            this.labelExplanation.Text =
                "You have not specified a title for this {0}. Do you still want to continue with p" +
                "ublishing?";

            //
            // labelError
            //
            this.labelError.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                      )));
            this.labelError.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelError.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelError.Location = new System.Drawing.Point(72, 16);
            this.labelError.Name = "labelError";
            this.labelError.Size = new System.Drawing.Size(216, 23);
            this.labelError.TabIndex = 0;
            this.labelError.Text = "{0} Title Not Specified";

            //
            // pictureBox1
            //
            this.pictureBox1.Image = ((System.Drawing.Image) (resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(8, 8);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(53, 42);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;

            //
            // buttonYes
            //
            this.buttonYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.buttonYes.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonYes.Location = new System.Drawing.Point(71, 97);
            this.buttonYes.Name = "buttonYes";
            this.buttonYes.Size = new System.Drawing.Size(75, 23);
            this.buttonYes.TabIndex = 3;
            this.buttonYes.Text = "Yes";

            //
            // buttonNo
            //
            this.buttonNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.buttonNo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonNo.Location = new System.Drawing.Point(151, 97);
            this.buttonNo.Name = "buttonNo";
            this.buttonNo.Size = new System.Drawing.Size(75, 23);
            this.buttonNo.TabIndex = 2;
            this.buttonNo.Text = "No";

            //
            // TitleReminderForm
            //
            this.AcceptButton = this.buttonNo;
            this.CancelButton = this.buttonNo;
            this.ClientSize = new System.Drawing.Size(314, 168);
            this.Controls.Add(this.buttonNo);
            this.Controls.Add(this.buttonYes);
            this.Controls.Add(this.cbDontShowAgain);
            this.Controls.Add(this.labelExplanation);
            this.Controls.Add(this.labelError);
            this.Controls.Add(this.pictureBox1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TitleReminderForm";
            this.Text = "Title Reminder";
            ((System.ComponentModel.ISupportInitialize) (this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
