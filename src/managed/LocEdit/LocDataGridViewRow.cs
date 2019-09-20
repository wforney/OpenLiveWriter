namespace LocEdit
{
    using System.Windows.Forms;

    internal class LocDataGridViewRow : DataGridViewRow
    {
        private readonly DataGridViewCell keyCell;
        private readonly DataGridViewCell valueCell;
        private readonly DataGridViewCell commentCell;

        public LocDataGridViewRow(string key, string value, string comment) : base()
        {
            this.keyCell = new DataGridViewTextBoxCell() { Value = key };
            this.valueCell = new DataGridViewTextBoxCell() { Value = value };
            this.commentCell = new DataGridViewTextBoxCell() { Value = comment };

            this.Cells.Add(this.keyCell);
            this.Cells.Add(this.valueCell);
            this.Cells.Add(this.commentCell);
        }
    }
}
