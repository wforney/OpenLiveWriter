// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Summary description for DynamicCommandMenuOverflowForm.
    /// </summary>
    public partial class DynamicCommandMenuOverflowForm : ApplicationDialog
    {
        private Button buttonOK;
        private Button buttonCancel;
        private ListBox listBoxCommands;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicCommandMenuOverflowForm"/> class.
        /// </summary>
        /// <param name="menuCommandObjects">The menu command objects.</param>
        public DynamicCommandMenuOverflowForm(IMenuCommandObject[] menuCommandObjects)
        {
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.buttonOK.Text = Res.Get(StringId.OKButtonText);
            this.buttonCancel.Text = Res.Get(StringId.CancelButton);

            // initialize the listbox
            foreach (var menuCommandObject in menuCommandObjects)
            {
                this.listBoxCommands.Items.Add(new MenuCommandObjectListBoxAdapter(menuCommandObject));
            }

            // select the top item in the list box
            if (this.listBoxCommands.Items.Count > 0)
            {
                this.listBoxCommands.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Gets the selected object.
        /// </summary>
        /// <value>The selected object.</value>
        public IMenuCommandObject SelectedObject
        {
            get
            {
                return this.listBoxCommands.SelectedIndex != -1
                    ? (this.listBoxCommands.SelectedItem as MenuCommandObjectListBoxAdapter).MenuCommandObject
                    : null;
            }
        }

        /// <summary>
        /// Handles the DoubleClick event of the listBoxCommands control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void listBoxCommands_DoubleClick(object sender, EventArgs e) => this.DialogResult = DialogResult.OK;

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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOK = new Button();
            this.buttonCancel = new Button();
            this.listBoxCommands = new ListBox();
            this.SuspendLayout();
            //
            // buttonOK
            //
            this.buttonOK.DialogResult = DialogResult.OK;
            this.buttonOK.FlatStyle = FlatStyle.System;
            this.buttonOK.Location = new System.Drawing.Point(88, 228);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.TabIndex = 0;
            this.buttonOK.Text = "OK";
            //
            // buttonCancel
            //
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.System;
            this.buttonCancel.Location = new System.Drawing.Point(171, 228);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            //
            // listBoxCommands
            //
            this.listBoxCommands.Location = new System.Drawing.Point(8, 8);
            this.listBoxCommands.Name = "listBoxCommands";
            this.listBoxCommands.Size = new System.Drawing.Size(238, 212);
            this.listBoxCommands.TabIndex = 2;
            this.listBoxCommands.DoubleClick += new EventHandler(this.listBoxCommands_DoubleClick);
            //
            // DynamicCommandMenuOverflowForm
            //
            this.AcceptButton = this.buttonOK;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(256, 260);
            this.Controls.Add(this.listBoxCommands);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.Location = new System.Drawing.Point(0, 0);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DynamicCommandMenuOverflowForm";
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "DynamicCommandMenuOverflowForm";
            this.ResumeLayout(false);
        }
        #endregion
    }
}
