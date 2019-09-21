// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Interop.Windows;
    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// Class PreferencesForm.
    /// Implements the <see cref="BaseForm" />
    /// </summary>
    /// <seealso cref="BaseForm" />
    public partial class PreferencesForm : BaseForm
    {
        /// <summary>
        /// The SideBarControl that provides our TabControl-like user interface.
        /// </summary>
        private readonly SideBarControl sideBarControl;

        /// <summary>
        /// The PreferencesPanel list.
        /// </summary>
        protected ArrayList preferencesPanelList = new ArrayList();

        /// <summary>
        /// A value which indicates whether the form is initialized.
        /// </summary>
        private bool initialized;

        #region Windows Form Designer generated code

        private Button buttonOK;
        private Button buttonCancel;
        private Button buttonApply;
        private ImageList imageList;
        private Panel panelPreferences;
        private IContainer components;

        #endregion Windows Form Designer generated code

        public PreferencesForm()
        {
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);
            this.buttonApply.Text = Res.Get(StringId.ApplyButton);
            //	Set the title of the form.
            this.Text = Res.Get(StringId.Options);

            //	Instantiate and initialize the SideBarControl.
            this.sideBarControl = new SideBarControl
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                TabStop = true,
                TabIndex = 0
            };
            this.sideBarControl.SelectedIndexChanged += new EventHandler(this.sideBarControl_SelectedIndexChanged);
            this.sideBarControl.Location = new Point(10, 10);
            this.sideBarControl.Size = new Size(151, this.ClientSize.Height - 20);
            this.Controls.Add(this.sideBarControl);

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (PreferencesPanel panel in this.preferencesPanelList)
                {
                    panel.Dispose();
                }

                if (this.components != null)
                {
                    this.components.Dispose();
                }
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
            this.components = new Container();
            var resources = new System.Resources.ResourceManager(typeof(PreferencesForm));
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.buttonApply = new Button();
            this.imageList = new ImageList(this.components);
            this.panelPreferences = new Panel();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonOK.FlatStyle = FlatStyle.System;
            this.buttonOK.Location = new Point(288, 568);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "OK";
            this.buttonOK.Click += new EventHandler(this.buttonOK_Click);
            //
            // buttonCancel
            //
            this.buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.System;
            this.buttonCancel.Location = new Point(368, 568);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(75, 23);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
            //
            // buttonApply
            //
            this.buttonApply.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.buttonApply.Enabled = false;
            this.buttonApply.FlatStyle = FlatStyle.System;
            this.buttonApply.Location = new Point(448, 568);
            this.buttonApply.Name = "buttonApply";
            this.buttonApply.Size = new Size(75, 23);
            this.buttonApply.TabIndex = 4;
            this.buttonApply.Text = "&Apply";
            this.buttonApply.Click += new EventHandler(this.buttonApply_Click);
            //
            // imageList
            //
            this.imageList.ColorDepth = ColorDepth.Depth32Bit;
            this.imageList.ImageSize = new Size(32, 32);
            this.imageList.ImageStream = (ImageListStreamer)resources.GetObject("imageList.ImageStream");
            this.imageList.TransparentColor = Color.White;
            //
            // panelPreferences
            //
            this.panelPreferences.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.panelPreferences.BackColor = SystemColors.Control;
            this.panelPreferences.Location = new Point(162, 0);
            this.panelPreferences.Name = "panelPreferences";
            this.panelPreferences.Size = new Size(370, 567);
            this.panelPreferences.TabIndex = 1;
            //
            // PreferencesForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new Size(534, 600);
            this.Controls.Add(this.panelPreferences);
            this.Controls.Add(this.buttonApply);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PreferencesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Options";
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// Event triggered when the user saves a new set of preferences (by pressing OK, or Apply).
        /// </summary>
        public event EventHandler PreferencesSaved;

        public void OnPreferencesSaved(EventArgs evt) => PreferencesSaved?.Invoke(this, evt);

        /// <summary>
        /// Gets or sets the selected index.
        /// </summary>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int SelectedIndex
        {
            get => this.sideBarControl.SelectedIndex;
            set => this.sideBarControl.SelectedIndex = value;
        }

        public IWin32Window Win32Owner
        {
            get => this.win32Owner ?? this.Owner;
            set => this.win32Owner = value;
        }
        private IWin32Window win32Owner;

        public void HideApplyButton()
        {
            this.buttonApply.Visible = false;
            var shift = this.buttonApply.Right - this.buttonCancel.Right;
            this.buttonOK.Left += shift;
            this.buttonCancel.Left += shift;
        }

        public void SelectEntry(Type panel)
        {
            for (var i = 0; i < this.preferencesPanelList.Count; i++)
            {
                if (this.preferencesPanelList[i].GetType() == panel)
                {
                    this.SelectedIndex = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Sets a PreferencesPanel.
        /// </summary>
        /// <param name="index">Index of the entry to set; zero based.</param>
        /// <param name="preferencesPanel">The PreferencesPanel to set.</param>
        public void SetEntry(int index, PreferencesPanel preferencesPanel)
        {
            //	Set the SideBarControl entry.
            this.sideBarControl.SetEntry(index, preferencesPanel.PanelBitmap, preferencesPanel.PanelName, $"btn{preferencesPanel.Name}");

            //	Set our PreferencesPanel event handlers.
            preferencesPanel.Modified += new EventHandler(this.preferencesPanel_Modified);

            //	Replace and existing PreferencesPanel.
            if (index < this.preferencesPanelList.Count)
            {
                //	Remove the existing PreferencesPanel.
                if (this.preferencesPanelList[index] != null)
                {
                    var oldPreferencesPanel = (PreferencesPanel)this.preferencesPanelList[index];
                    oldPreferencesPanel.Modified -= new EventHandler(this.preferencesPanel_Modified);
                    if (this.sideBarControl.SelectedIndex == index)
                    {
                        this.panelPreferences.Controls.Remove(oldPreferencesPanel);
                    }
                }

                //	Set the new PreferencesPabel.
                this.preferencesPanelList[index] = preferencesPanel;
            }
            //	Add a new PreferencesPanel.
            else
            {
                //	Ensure that there are entries up to the index position (make them null).  This
                //	allows the user of this control to add his entries out of order or with gaps.
                for (var i = this.preferencesPanelList.Count; i < index; i++)
                {
                    this.preferencesPanelList.Add(null);
                }

                //	Add the BitmapButton.
                this.preferencesPanelList.Add(preferencesPanel);
            }

            //	Add the Preferences panel.
            preferencesPanel.Dock = DockStyle.Fill;
            this.panelPreferences.Controls.Add(preferencesPanel);
        }

        /// <summary>
        /// Raises the Load event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            //	Call the base class's method so that registered delegates receive the event.
            base.OnLoad(e);

            //  The Collection Settings dialog looks weird when it comes up with Fields
            //  selected but Flags focused... both boxes are blue. This makes sure that
            //  doesn't happen.
            this.buttonCancel.Focus();

            this.AdjustHeightToFit();

            using (new AutoGrow(this, AnchorStyles.Right, true))
            {
                LayoutHelper.EqualizeButtonWidthsHoriz(AnchorStyles.Right, this.buttonCancel.Width, int.MaxValue,
                    this.buttonOK, this.buttonCancel, this.buttonApply);

                var oldSize = this.sideBarControl.Width;
                this.sideBarControl.AdjustSize();
                var deltaX = this.sideBarControl.Width - oldSize;
                new ControlGroup(this.panelPreferences, this.buttonOK, this.buttonCancel, this.buttonApply).Left += deltaX;

                if (this.buttonOK.Left < this.sideBarControl.Right)
                {
                    var right = this.buttonApply.Right;
                    DisplayHelper.AutoFitSystemButton(this.buttonOK);
                    DisplayHelper.AutoFitSystemButton(this.buttonCancel);
                    DisplayHelper.AutoFitSystemButton(this.buttonApply);
                    LayoutHelper.DistributeHorizontally(8, this.buttonOK, this.buttonCancel, this.buttonApply);
                    new ControlGroup(this.buttonOK, this.buttonCancel, this.buttonApply).Left += right - this.buttonApply.Right;
                }
            }

            //	We're initialized, so remove all unselected panels.  This allows AutoScale to
            //	work.
            this.initialized = true;
            this.RemoveUnselectedPanels();

            // protect against being shown directly on top of an identically sized owner
            this.OffsetFromIdenticalOwner();
        }

        private void AdjustHeightToFit()
        {
            var maxPanelHeight = 0;
            foreach (PreferencesPanel panel in this.preferencesPanelList)
            {
                maxPanelHeight = Math.Max(maxPanelHeight, this.GetPanelHeightRequired(panel));
            }

            this.panelPreferences.Height = maxPanelHeight;
            this.Height = maxPanelHeight + (int)Math.Ceiling(DisplayHelper.ScaleY(100));
        }

        private int GetPanelHeightRequired(PreferencesPanel preferencesPanel)
        {
            var maxBottom = 0;
            foreach (Control c in preferencesPanel.Controls)
            {
                maxBottom = Math.Max(maxBottom, c.Bottom);
            }

            return maxBottom;
        }

        /// <summary>
        /// Helper method to save Preferences.
        /// Returns true if saved successfully.
        /// </summary>
        protected virtual bool SavePreferences()
        {
            var tabSwitcher = new TabSwitcher(this.sideBarControl);

            for (var i = 0; i < this.preferencesPanelList.Count; i++)
            {
                var preferencesPanel = (PreferencesPanel)this.preferencesPanelList[i];
                tabSwitcher.Tab = i;
                if (!preferencesPanel.PrepareSave(new PreferencesPanel.SwitchToPanel(tabSwitcher.Switch)))
                {
                    return false;
                }
            }

            //	Save every PreferencesPanel.
            for (var i = 0; i < this.preferencesPanelList.Count; i++)
            {
                var preferencesPanel = (PreferencesPanel)this.preferencesPanelList[i];
                if (preferencesPanel != null)
                {
                    preferencesPanel.Save();
                }
            }

            //	Disable the Apply button.
            this.buttonApply.Enabled = false;

            //notify listeners that the preferences where saved.
            this.OnPreferencesSaved(EventArgs.Empty);

            return true;
        }

        /// <summary>
        /// Removes all unselected panels.
        /// </summary>
        private void RemoveUnselectedPanels()
        {
            if (!this.initialized)
            {
                return;
            }

            for (var i = 0; i < this.preferencesPanelList.Count; i++)
            {
                if (i != this.sideBarControl.SelectedIndex && this.preferencesPanelList[i] != null)
                {
                    var preferencesPanel = (PreferencesPanel)this.preferencesPanelList[i];
                    if (this.panelPreferences.Controls.Contains(preferencesPanel))
                    {
                        this.panelPreferences.Controls.Remove(preferencesPanel);
                    }
                }
            }
        }

        /// <summary>
        /// Offsets from identical owner.
        /// </summary>
        private void OffsetFromIdenticalOwner()
        {
            if (this.Win32Owner != null)
            {
                var ownerRect = new RECT();
                var prefsRect = new RECT();
                if (User32.GetWindowRect(this.Win32Owner.Handle, ref ownerRect) && User32.GetWindowRect(this.Handle, ref prefsRect))
                {
                    if ((ownerRect.right - ownerRect.left) == (prefsRect.right - prefsRect.left))
                    {
                        // adjust location
                        this.StartPosition = FormStartPosition.Manual;
                        this.Location = new Point(ownerRect.left - SystemInformation.CaptionHeight, ownerRect.top - SystemInformation.CaptionHeight);
                    }
                }
            }
        }

        /// <summary>
        /// sideBarControl_SelectedIndexChanged event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void sideBarControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            //	Make the selected PreferencesPanel visible.
            if (this.sideBarControl.SelectedIndex < this.preferencesPanelList.Count && this.preferencesPanelList[this.sideBarControl.SelectedIndex] != null)
            {
                var preferencesPanel = (PreferencesPanel)this.preferencesPanelList[this.sideBarControl.SelectedIndex];
                if (BidiHelper.IsRightToLeft && preferencesPanel.RightToLeft != RightToLeft.Yes)
                {
                    preferencesPanel.RightToLeft = RightToLeft.Yes;
                }

                BidiHelper.RtlLayoutFixup(preferencesPanel);
                this.panelPreferences.Controls.Add(preferencesPanel);
                if (this.ShowKeyboardCues)
                {
                    //fix bug 406441, if the show cues window messages have been sent to the form
                    //resend them to force the new control to show them
                    ControlHelper.HideAccelerators(this);
                    ControlHelper.ShowAccelerators(this);
                }

                if (this.ShowFocusCues)
                {
                    //fix bug 406420, if the show cues window messages have been sent to the form
                    //resend them to force the new control to show them
                    ControlHelper.HideFocus(this);
                    ControlHelper.ShowFocus(this);
                }

                preferencesPanel.BringToFront();
            }

            //	Remove unselected panels.
            this.RemoveUnselectedPanels();
        }

        /// <summary>
        /// preferencesPanel_Modified event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void preferencesPanel_Modified(object sender, EventArgs e) => this.buttonApply.Enabled = true;

        /// <summary>
        /// buttonOK_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (this.SavePreferences())
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        /// <summary>
        /// buttonCancel_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonCancel_Click(object sender, EventArgs e) => this.DialogResult = DialogResult.Cancel;

        /// <summary>
        /// buttonApply_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void buttonApply_Click(object sender, EventArgs e) => this.SavePreferences();
    }
}
