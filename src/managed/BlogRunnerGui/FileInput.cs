// <copyright file="FileInput.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunnerGui
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    /// <summary>
    /// The dialog style enumeration.
    /// </summary>
    public enum DialogStyle
    {
        /// <summary>
        /// An open dialog.
        /// </summary>
        Open,

        /// <summary>
        /// A save dialog.
        /// </summary>
        Save,
    }

    /// <summary>
    /// Class FileInput.
    /// Implements the <see cref="System.Windows.Forms.UserControl" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.UserControl" />
    [DefaultEvent("PathChanged")]
    public partial class FileInput : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileInput"/> class.
        /// </summary>
        public FileInput() => this.InitializeComponent();

        /// <summary>
        /// Occurs when [path changed].
        /// </summary>
        public event EventHandler PathChanged;

        /// <summary>
        /// Gets or sets the text associated with this control.
        /// </summary>
        /// <value>The text.</value>
        [Bindable(true)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override string Text
        {
            get => this.label1.Text;
            set => this.label1.Text = value;
        }

        /// <summary>
        /// Gets or sets the dialog style.
        /// </summary>
        public DialogStyle DialogStyle { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string Path
        {
            get => this.textBox1.Text;
            set => this.textBox1.Text = value;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e) => this.textBox1.Focus();

        /// <summary>
        /// Handles the Click event of the button1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void button1_Click(object sender, EventArgs e)
        {
            var fd = this.DialogStyle == DialogStyle.Open ? this.openFileDialog1 : (FileDialog)this.saveFileDialog1;
            if (fd.ShowDialog(this) == DialogResult.OK)
            {
                this.textBox1.Text = fd.FileName;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the textBox1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var path = this.textBox1.Text.Trim('"');
            if (path != this.textBox1.Text)
            {
                this.textBox1.Text = path;
                this.textBox1.SelectionStart = this.textBox1.TextLength;
                return;
            }

            this.PathChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
