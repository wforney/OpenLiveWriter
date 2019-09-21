// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using System.Windows.Forms;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Delegate GridNavigateEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="GridNavigateEventArgs"/> instance containing the event data.</param>
    public delegate void GridNavigateEventHandler(object sender, GridNavigateEventArgs args);

    /// <summary>
    /// Summary description for ColorPresetControl.
    /// Implements the <see cref="IColorPickerSubControl" />
    /// </summary>
    /// <seealso cref="IColorPickerSubControl" />
    public class ColorPresetControl : IColorPickerSubControl
    {
        /// <summary>
        /// The table layout
        /// </summary>
        private TableLayoutPanel tableLayout;

        /// <summary>
        /// The number rows
        /// </summary>
        private int NUM_ROWS = 3;

        /// <summary>
        /// The number columns
        /// </summary>
        private int NUM_COLUMNS = 4;

        /// <summary>
        /// The colors
        /// </summary>
        private ColorButton[] colors =
            {
                new ColorButton(Color.FromArgb(58, 177, 222)),
                new ColorButton(Color.FromArgb(166, 166, 166)),
                new ColorButton(Color.FromArgb(110, 158, 255)),
                new ColorButton(Color.FromArgb(112, 217, 235)),

                new ColorButton(Color.FromArgb(242, 57, 57)),
                new ColorButton(Color.FromArgb(255, 123, 4)),
                new ColorButton(Color.FromArgb(255, 172, 247)),
                new ColorButton(Color.FromArgb(145, 226, 71)),

                new ColorButton(Color.FromArgb(102, 117, 140)),
                new ColorButton(Color.FromArgb(132, 117, 217)),
                new ColorButton(Color.FromArgb(201, 198, 184)),
                new ColorButton(Color.FromArgb(244, 62, 131))
            };

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPresetControl"/> class.
        /// </summary>
        /// <param name="colorSelected">The color selected.</param>
        /// <param name="navigate">The navigate.</param>
        public ColorPresetControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate) : base(colorSelected, navigate)
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
            this.AccessibleName = Res.Get(StringId.ColorPickerColor);

            System.Diagnostics.Debug.Assert(this.colors.Length == 12, "Unexpected number of buttons.  If you change the number of buttons, then you'll need to ensure that the navigation code is updated as well.");

            var i = 0;
            foreach (var button in this.colors)
            {
                button.ColorSelected += colorSelected;
                button.Navigate += new GridNavigateEventHandler(this.ColorPresetControl_Navigate);
                button.TabIndex = i;
                button.Margin = new Padding(0);
                this.tableLayout.Controls.Add(button);
                this.tableLayout.SetCellPosition(button, new TableLayoutPanelCellPosition(i % (this.NUM_ROWS + 1), i / (this.NUM_COLUMNS)));
                i++;
            }

            foreach (RowStyle style in this.tableLayout.RowStyles)
            {
                style.SizeType = SizeType.Percent;
                style.Height = 1 / (float)this.tableLayout.RowCount;
            }
            foreach (ColumnStyle style in this.tableLayout.ColumnStyles)
            {
                style.SizeType = SizeType.Percent;
                style.Width = 1 / (float)this.tableLayout.ColumnCount;
            }

            this.Controls.Add(this.tableLayout);
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        /// Used by ColorPickerForm to inform a subcontrol of the currently selected color.
        public override Color Color
        {
            get => this.color;

            set
            {
                foreach (var button in this.colors)
                {
                    this.color = value;
                    button.Selected = (button.Color == value);
                }
            }
        }

        /// <summary>
        /// Handles the Navigate event of the ColorPresetControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="GridNavigateEventArgs"/> instance containing the event data.</param>
        void ColorPresetControl_Navigate(object sender, GridNavigateEventArgs args)
        {
            var c = (Control)sender;

            var navFrom = this.tableLayout.GetCellPosition(c);
            var navTo = navFrom;

            var navigateOffPresets = false;
            var forward = false;

            switch (args.Navigate)
            {
                case GridNavigateEventArgs.Direction.Up:
                    if (navFrom.Row <= 0)
                    {
                        navigateOffPresets = true;
                    }
                    else
                    {
                        navTo.Row--;
                    }

                    break;
                case GridNavigateEventArgs.Direction.Down:
                    if (navFrom.Row >= this.tableLayout.RowCount - 1)
                    {
                        navigateOffPresets = true;
                        ////forward = true;
                    }
                    else
                    {
                        navTo.Row++;
                    }

                    forward = true;
                    break;
                case GridNavigateEventArgs.Direction.Left:
                    if (navFrom.Column <= 0)
                    {
                        if (navFrom.Row <= 0)
                        {
                            navigateOffPresets = true;
                        }
                        else
                        {
                            navTo.Row--;
                            navTo.Column = this.tableLayout.ColumnCount - 1;
                        }
                    }
                    else
                    {
                        navTo.Column--;
                    }

                    break;
                case GridNavigateEventArgs.Direction.Right:
                    if (navFrom.Column >= this.tableLayout.ColumnCount - 1)
                    {
                        if (navFrom.Row >= this.tableLayout.RowCount - 1)
                        {
                            forward = true;
                            navigateOffPresets = true;
                        }
                        else
                        {
                            navTo.Column = 0;
                            navTo.Row++;
                        }
                    }
                    else
                    {
                        navTo.Column++;
                    }
                    break;
                default:
                    break;
            }

            if (navigateOffPresets)
            {
                this.OnNavigate(new NavigateEventArgs(forward));
            }
            else
            {
                this.tableLayout.GetControlFromPosition(navTo.Column, navTo.Row).Select();
            }
        }

        /// <summary>
        /// Used by ColorPickerForm to facilitate navigation (up/down arrow keys) across the color picker menu.
        /// </summary>
        /// <param name="forward">if set to <c>true</c> [forward].</param>
        public override void SelectControl(bool forward)
        {
            if (forward)
            {
                this.tableLayout.GetControlFromPosition(0, 0).Select();
            }
            else
            {
                this.tableLayout.GetControlFromPosition(this.tableLayout.ColumnCount - 1, this.tableLayout.RowCount - 1).Select();
            }
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ColorPresetControl
            //
            this.Name = "ColorPresetControl";
            this.Size = new Size(96, 72);
            this.tableLayout = new TableLayoutPanel();
            this.SuspendLayout();

            //
            // tableLayout
            //
            this.tableLayout.RowCount = 3;
            this.tableLayout.ColumnCount = 4;
            this.tableLayout.Location = new Point(0, 0);
            this.tableLayout.Name = "tableLayout";
            this.tableLayout.TabIndex = 0;
            this.tableLayout.TabStop = true;
            this.tableLayout.Size = new Size(96, 72);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
