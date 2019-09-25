// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Windows.Forms;
    using ApplicationFramework.Preferences;
    using CoreServices;
    using CoreServices.Layout;
    using Localization;

    /// <summary>
    /// The SpellingPreferencesPanel class.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.Preferences.PreferencesPanel" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.Preferences.PreferencesPanel" />
    public partial class SpellingPreferencesPanel : PreferencesPanel
    {
        /// <summary>
        /// The spelling image path
        /// </summary>
        private const string SpellingImagePath = "Images.";

        //private Bitmap spellingDictionariesBitmap = ResourceHelper.LoadAssemblyResourceBitmap( SPELLING_IMAGE_PATH + "SpellingDictionaries.png") ;
        /// <summary>
        /// The spelling panel bitmap
        /// </summary>
        private readonly Bitmap spellingPanelBitmap =
            ResourceHelper.LoadAssemblyResourceBitmap(SpellingPreferencesPanel.SpellingImagePath +
                                                      "SpellingPanelBitmapSmall.png");

        /// <summary>
        /// The check box automatic correct
        /// </summary>
        private CheckBox checkBoxAutoCorrect;

        /// <summary>
        /// The check box check before publish
        /// </summary>
        private CheckBox checkBoxCheckBeforePublish;

        /// <summary>
        /// The check box real time checking
        /// </summary>
        private CheckBox checkBoxRealTimeChecking;

        /// <summary>
        /// The combo box language
        /// </summary>
        private ComboBox comboBoxLanguage;

        /// <summary>
        /// The group box general options
        /// </summary>
        private GroupBox groupBoxGeneralOptions;

        /// <summary>
        /// The label dictionary language
        /// </summary>
        private Label labelDictionaryLanguage;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The spelling preferences
        /// </summary>
        private readonly SpellingPreferences spellingPreferences;

        /// <inheritdoc />
        public SpellingPreferencesPanel()
            : this(new SpellingPreferences())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellingPreferencesPanel"/> class.
        /// </summary>
        /// <param name="preferences">The preferences.</param>
        public SpellingPreferencesPanel(SpellingPreferences preferences)
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            this.labelDictionaryLanguage.Text = Res.Get(StringId.DictionaryLanguageLabel);
            this.groupBoxGeneralOptions.Text = Res.Get(StringId.SpellingPrefOptions);
            this.checkBoxRealTimeChecking.Text = Res.Get(StringId.SpellingPrefReal);
            this.checkBoxCheckBeforePublish.Text = Res.Get(StringId.SpellingPrefPub);
            this.checkBoxAutoCorrect.Text = Res.Get(StringId.SpellingPrefAuto);
            this.PanelName = Res.Get(StringId.SpellingPrefName);

            // set panel bitmap
            this.PanelBitmap = this.spellingPanelBitmap;

            // initialize preferences
            this.spellingPreferences = preferences;
            this.spellingPreferences.PreferencesModified += this.spellingPreferences_PreferencesModified;

            // core options
            this.checkBoxCheckBeforePublish.Checked = this.spellingPreferences.CheckSpellingBeforePublish;
            this.checkBoxRealTimeChecking.Checked = this.spellingPreferences.RealTimeSpellChecking;
            this.checkBoxAutoCorrect.Checked = this.spellingPreferences.EnableAutoCorrect;

            // initialize language combo
            this.comboBoxLanguage.BeginUpdate();
            this.comboBoxLanguage.Items.Clear();
            var currentLanguage = this.spellingPreferences.Language;

            var languages = SpellingSettings.GetInstalledLanguages();
            Array.Sort(languages, new SentryLanguageEntryComparer(CultureInfo.CurrentUICulture));

            this.comboBoxLanguage.Items.Add(
                new SpellingLanguageEntry(string.Empty, Res.Get(StringId.DictionaryLanguageNone)));

            foreach (var language in languages)
            {
                var index = this.comboBoxLanguage.Items.Add(language);
                if (language.BCP47Code == currentLanguage)
                {
                    this.comboBoxLanguage.SelectedIndex = index;
                }
            }

            // defend against invalid value
            if (this.comboBoxLanguage.SelectedIndex == -1)
            {
                if (!string.IsNullOrEmpty(currentLanguage))
                {
                    Debug.Fail("Language in registry not supported!");
                }

                this.comboBoxLanguage.SelectedIndex = 0; // "None"
            }

            this.comboBoxLanguage.EndUpdate();

            this.ManageSpellingOptions();

            // hookup to changed events to update preferences
            this.checkBoxCheckBeforePublish.CheckedChanged += this.checkBoxCheckBeforePublish_CheckedChanged;
            this.checkBoxRealTimeChecking.CheckedChanged += this.checkBoxRealTimeChecking_CheckedChanged;
            this.checkBoxAutoCorrect.CheckedChanged += this.checkBoxAutoCorrect_CheckedChanged;

            this.comboBoxLanguage.SelectedIndexChanged += this.comboBoxLanguage_SelectedIndexChanged;
        }

        /// <summary>
        /// Manages the spelling options.
        /// </summary>
        private void ManageSpellingOptions()
        {
            var enabled = this.comboBoxLanguage.SelectedIndex != 0; // "None"
            this.checkBoxCheckBeforePublish.Enabled = enabled;
            this.checkBoxRealTimeChecking.Enabled = enabled;
            this.checkBoxAutoCorrect.Enabled = enabled;
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DisplayHelper.AutoFitSystemCombo(this.comboBoxLanguage, this.comboBoxLanguage.Width,
                                             this.groupBoxGeneralOptions.Width - this.comboBoxLanguage.Left - 8,
                                             false);
            LayoutHelper.FixupGroupBox(8, this.groupBoxGeneralOptions);
        }

        /// <summary>
        /// Save data
        /// </summary>
        public override void Save()
        {
            if (this.spellingPreferences.IsModified())
            {
                this.spellingPreferences.Save();
            }
        }

        /// <summary>
        /// flagsPreferences_PreferencesModified event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void spellingPreferences_PreferencesModified(object sender, EventArgs e) => this.OnModified(EventArgs.Empty);

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxGeneralOptions = new System.Windows.Forms.GroupBox();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.labelDictionaryLanguage = new System.Windows.Forms.Label();
            this.checkBoxRealTimeChecking = new System.Windows.Forms.CheckBox();
            this.checkBoxCheckBeforePublish = new System.Windows.Forms.CheckBox();
            this.checkBoxAutoCorrect = new System.Windows.Forms.CheckBox();
            this.groupBoxGeneralOptions.SuspendLayout();
            this.SuspendLayout();

            //
            // _groupBoxGeneralOptions
            //
            this.groupBoxGeneralOptions.Anchor =
                ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxGeneralOptions.Controls.Add(this.comboBoxLanguage);
            this.groupBoxGeneralOptions.Controls.Add(this.labelDictionaryLanguage);
            this.groupBoxGeneralOptions.Controls.Add(this.checkBoxRealTimeChecking);
            this.groupBoxGeneralOptions.Controls.Add(this.checkBoxCheckBeforePublish);
            this.groupBoxGeneralOptions.Controls.Add(this.checkBoxAutoCorrect);
            this.groupBoxGeneralOptions.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.groupBoxGeneralOptions.Location = new System.Drawing.Point(8, 32);
            this.groupBoxGeneralOptions.Name = "groupBoxGeneralOptions";
            this.groupBoxGeneralOptions.Size = new System.Drawing.Size(345, 189);
            this.groupBoxGeneralOptions.TabIndex = 1;
            this.groupBoxGeneralOptions.TabStop = false;
            this.groupBoxGeneralOptions.Text = "General options";

            //
            // _comboBoxLanguage
            //
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.Location = new System.Drawing.Point(48, 37);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(195, 21);
            this.comboBoxLanguage.TabIndex = 1;

            //
            // _labelDictionaryLanguage
            //
            this.labelDictionaryLanguage.Anchor =
                ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDictionaryLanguage.AutoSize = true;
            this.labelDictionaryLanguage.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelDictionaryLanguage.Location = new System.Drawing.Point(16, 18);
            this.labelDictionaryLanguage.Name = "labelDictionaryLanguage";
            this.labelDictionaryLanguage.Size = new System.Drawing.Size(106, 13);
            this.labelDictionaryLanguage.TabIndex = 0;
            this.labelDictionaryLanguage.Text = "Dictionary &language:";

            //
            // _checkBoxRealTimeChecking
            //
            this.checkBoxRealTimeChecking.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxRealTimeChecking.Location = new System.Drawing.Point(16, 65);
            this.checkBoxRealTimeChecking.Name = "checkBoxRealTimeChecking";
            this.checkBoxRealTimeChecking.Size = new System.Drawing.Size(323, 18);
            this.checkBoxRealTimeChecking.TabIndex = 2;
            this.checkBoxRealTimeChecking.Text = "Use &real time spell checking (squiggles)";

            //
            // _checkBoxCheckBeforePublish
            //
            this.checkBoxCheckBeforePublish.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxCheckBeforePublish.Location = new System.Drawing.Point(16, 88);
            this.checkBoxCheckBeforePublish.Name = "checkBoxCheckBeforePublish";
            this.checkBoxCheckBeforePublish.Size = new System.Drawing.Size(323, 18);
            this.checkBoxCheckBeforePublish.TabIndex = 5;
            this.checkBoxCheckBeforePublish.Text = "Check spelling before &publishing";

            //
            // _checkBoxAutoCorrect
            //
            this.checkBoxAutoCorrect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxAutoCorrect.Location = new System.Drawing.Point(16, 111);
            this.checkBoxAutoCorrect.Name = "checkBoxAutoCorrect";
            this.checkBoxAutoCorrect.Size = new System.Drawing.Size(323, 18);
            this.checkBoxAutoCorrect.TabIndex = 6;
            this.checkBoxAutoCorrect.Text = "Automatically &correct common capitalization and spelling mistakes";

            //
            // SpellingPreferencesPanel
            //
            this.AccessibleName = "Spelling";
            this.Controls.Add(this.groupBoxGeneralOptions);
            this.Name = "SpellingPreferencesPanel";
            this.PanelName = "Spelling";
            this.Controls.SetChildIndex(this.groupBoxGeneralOptions, 0);
            this.groupBoxGeneralOptions.ResumeLayout(false);
            this.groupBoxGeneralOptions.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        /// <summary>
        /// Handles the CheckedChanged event of the checkBoxCheckBeforePublish control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void checkBoxCheckBeforePublish_CheckedChanged(object sender, EventArgs e) =>
            this.spellingPreferences.CheckSpellingBeforePublish = this.checkBoxCheckBeforePublish.Checked;

        /// <summary>
        /// Handles the CheckedChanged event of the checkBoxRealTimeChecking control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void checkBoxRealTimeChecking_CheckedChanged(object sender, EventArgs e) =>
            this.spellingPreferences.RealTimeSpellChecking = this.checkBoxRealTimeChecking.Checked;

        /// <summary>
        /// Handles the CheckedChanged event of the checkBoxAutoCorrect control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void checkBoxAutoCorrect_CheckedChanged(object sender, EventArgs e) =>
            this.spellingPreferences.EnableAutoCorrect = this.checkBoxAutoCorrect.Checked;

        /// <summary>
        /// Handles the SelectedIndexChanged event of the comboBoxLanguage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.spellingPreferences.Language =
                (this.comboBoxLanguage.SelectedItem as SpellingLanguageEntry)?.BCP47Code;
            this.ManageSpellingOptions();
        }
    }
}
