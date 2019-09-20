namespace LocEdit
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;

    using LocUtil;

    /// <summary>
    /// Class MainForm.
    /// Implements the <see cref="System.Windows.Forms.Form" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class MainForm : Form
    {
        private string loadedFile;
        private bool modified = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainForm"/> class.
        /// </summary>
        public MainForm() => this.InitializeComponent();

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MainForm"/> is modified.
        /// </summary>
        /// <value><c>true</c> if modified; otherwise, <c>false</c>.</value>
        public bool Modified
        {
            get => this.modified;
            set
            {
                this.modified = value;
                this.Text = $"LocEdit: {this.loadedFile}{(this.modified ? "*" : "")}";
            }
        }

        /// <summary>
        /// Loads the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void LoadFile(string filePath)
        {
            this.loadedFile = filePath;
            this.Modified = false;

            this.dataGridView.Rows.Clear();
            using (var csvParser = new CsvParser(
                new StreamReader(
                    new FileStream(
                        Path.GetFullPath(filePath),
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite),
                    Encoding.Default),
                true))
            {
                foreach (string[] line in csvParser)
                {
                    var value = line[1];
                    value = value.Replace($"{(char)8230}", "..."); // undo ellipses
                    var comment = (line.Length > 2) ? line[2] : string.Empty;

                    this.dataGridView.Rows.Add(new LocDataGridViewRow(line[0], value, comment));
                }
            }

            this.dataGridView.AutoResizeRows();
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void SaveFile(string filePath)
        {
            this.loadedFile = filePath;
            this.Modified = false;

            var sb = new StringBuilder();
            sb.AppendLine("Name,Value,Comment");

            foreach (DataGridViewRow row in this.dataGridView.Rows)
            {
                if (row.Cells.Count < 3)
                {
                    continue;
                }

                var key = (string)row.Cells[0].Value;
                var value = (string)row.Cells[1].Value;
                var comment = (string)row.Cells[2].Value;

                if (key == null || value == null)
                {
                    continue;
                }

                sb.Append($"{Helpers.CsvizeString(key)},");
                sb.Append($"{Helpers.CsvizeString(value)},");
                sb.Append($"{Helpers.CsvizeString(comment ?? string.Empty)}");
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.Default);
        }

        /// <summary>
        /// Finds the next.
        /// </summary>
        /// <param name="query">The query.</param>
        public void FindNext(string query)
        {
            var y = this.dataGridView.CurrentCell != null ? this.dataGridView.CurrentCell.RowIndex + 1 : 0;
            this.dataGridView.ClearSelection();

            while (y < this.dataGridView.Rows.Count - 1)
            {
                if (((string)this.dataGridView.Rows[y].Cells[0].Value).ToLower().Contains(query.ToLower()))
                {
                    this.dataGridView.CurrentCell = this.dataGridView.Rows[y].Cells[0];
                    return;
                }

                y++;
            }

            MessageBox.Show("No results found, returning to start.");
            if (this.dataGridView.Rows.Count == 0 || this.dataGridView.Rows[0].Cells.Count == 0)
            {
                return;
            }

            this.dataGridView.CurrentCell = this.dataGridView.Rows[0].Cells[0];
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns><see langword="true" /> if the keystroke was processed and consumed by the control; otherwise, <see langword="false" /> to allow further processing.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.O))
            {
                this.ShowOpenDialog();
                return true;
            }

            if (keyData == (Keys.Control | Keys.S))
            {
                this.ShowSaveDialog();
                return true;
            }

            if (keyData == (Keys.Control | Keys.F))
            {
                this.textBoxFind.Focus();
                return true;
            }

            if (keyData == (Keys.Control | Keys.A))
            {
                if (this.dataGridView.CurrentRow != null)
                {
                    this.dataGridView.CurrentRow.Selected = true;
                }

                return true;
            }

            if (keyData == Keys.F3)
            {
                this.FindNext(this.textBoxFind.Text);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Shows the open dialog.
        /// </summary>
        private void ShowOpenDialog()
        {
            using (var openFileDialog1 = new OpenFileDialog())
            {
                openFileDialog1.Filter = "Localization CSV File (*.csv)|*.csv";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.LoadFile(openFileDialog1.FileName);
                }
            }
        }

        /// <summary>
        /// Shows the save dialog.
        /// </summary>
        private void ShowSaveDialog()
        {
            using (var saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.FileName = this.loadedFile;
                saveFileDialog1.Filter = "Localization CSV File (*.csv)|*.csv";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.SaveFile(saveFileDialog1.FileName);
                }
            }
        }

        /// <summary>
        /// Handles the Paint event of the TableLayoutPanel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void TableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
        }

        /// <summary>
        /// Handles the Click event of the ToolStripButtonLoad control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripButtonLoad_Click(object sender, EventArgs e)
            => this.ShowOpenDialog();

        /// <summary>
        /// Handles the Click event of the ButtonFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonFind_Click(object sender, EventArgs e)
            => this.FindNext(this.textBoxFind.Text);

        /// <summary>
        /// Handles the KeyUp event of the TextBoxFind control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void TextBoxFind_KeyUp(object sender, KeyEventArgs e)
        {
            if (this.textBoxFind.Focused && e.KeyCode == Keys.Enter)
            {
                this.FindNext(this.textBoxFind.Text);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the ToolStripButtonSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripButtonSave_Click(object sender, EventArgs e)
            => this.ShowSaveDialog();

        /// <summary>
        /// Handles the CellValueChanged event of the DataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellEventArgs"/> instance containing the event data.</param>
        private void DataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
            => this.Modified = true;

        /// <summary>
        /// Handles the FormClosing event of the MainForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.Modified)
            {
                return;
            }

            var result = MessageBox.Show(this, "There are unsaved changes. Are you sure you want to exit?", "LocEdit", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the ToolStripLabel1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Handles the Click event of the ToolStripButtonInsertAbove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripButtonInsertAbove_Click(object sender, EventArgs e)
        {
            if (this.dataGridView.CurrentCell != null)
            {
                this.dataGridView.Rows.Insert(this.dataGridView.CurrentCell.RowIndex);
            }
        }

        /// <summary>
        /// Handles the Click event of the ToolStripButtonInsertBelow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ToolStripButtonInsertBelow_Click(object sender, EventArgs e)
        {
            if (this.dataGridView.CurrentCell != null && this.dataGridView.CurrentCell.RowIndex < this.dataGridView.Rows.Count - 1)
            {
                this.dataGridView.Rows.Insert(this.dataGridView.CurrentCell.RowIndex + 1);
            }
        }
    }
}
