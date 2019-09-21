// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Layout;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Summary description for ColorPickerForm.
    /// Implements the <see cref="OpenLiveWriter.Controls.MiniForm" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.MiniForm" />
    public class ColorPickerForm : MiniForm
    {
        /// <summary>
        /// The color presets
        /// </summary>
        private ColorPresetControl colorPresets;

        /// <summary>
        /// The color default color control
        /// </summary>
        private ColorDefaultColorControl colorDefaultColorControl;

        /// <summary>
        /// The color dialog launcher control
        /// </summary>
        private ColorDialogLauncherControl colorDialogLauncherControl;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// The current sub control
        /// </summary>
        private int currentSubControl;

        /// <summary>
        /// The subctrls
        /// </summary>
        private IColorPickerSubControl[] subctrls;

        /// <summary>
        /// The m color
        /// </summary>
        private Color m_color;

        /// <summary>
        /// Occurs when [color selected].
        /// </summary>
        public event ColorSelectedEventHandler ColorSelected;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPickerForm" /> class.
        /// </summary>
        public ColorPickerForm()
        {
            //
            // Required for Windows Form Designer support
            //
            this.RightToLeftLayout = false; // prevents the single-pixel border from drawing in a funky way (right side drops out, even when using BidiGraphics)
            this.InitializeComponent();
            this.DismissOnDeactivate = true;
            this.subctrls = new IColorPickerSubControl[] { this.colorDefaultColorControl, this.colorPresets, this.colorDialogLauncherControl };

            // The ColorDialogLauncherControl needs to Close the ColorPickerForm before the ColorDialog can be shown.
            // If not, the Color Dialog will return DialogResult.Cancel immediately.
            this.colorDialogLauncherControl.Close += new EventHandler(this.colorDialogLauncherControl_Close);

            this.colorDefaultColorControl.AccessibleName = ControlHelper.ToAccessibleName(Res.Get(StringId.ColorPickerDefaultColor));
        }

        /// <summary>
        /// Handles the Close event of the colorDialogLauncherControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void colorDialogLauncherControl_Close(object sender, EventArgs e) => this.Close();

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            using (new AutoGrow(this, AnchorStyles.Right | AnchorStyles.Bottom, true))
            {
                var origHeight = this.colorDefaultColorControl.Height;
                var origWidth = this.colorDefaultColorControl.Width;
                this.colorDefaultColorControl.NaturalizeHeight();
                var deltaY = this.colorDefaultColorControl.Height - origHeight;

                new ControlGroup(this.colorPresets, this.colorDialogLauncherControl).Top += deltaY;

                this.colorDialogLauncherControl.NaturalizeHeight();

                this.colorDialogLauncherControl.Width = this.colorDefaultColorControl.Width =
                    Math.Max(origWidth, Math.Max(this.colorDialogLauncherControl.Width, this.colorDefaultColorControl.Width));
            }

            this.colorPresets.Left = (this.ClientSize.Width - this.colorPresets.Width) / 2;

            this.Focus();
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        public Color Color
        {
            get => this.m_color;
            set
            {
                this.m_color = value;

                foreach (var sc in this.subctrls)
                {
                    sc.Color = value;
                }

                ColorSelected?.Invoke(this, new ColorSelectedEventArgs(value));
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
            this.colorPresets = new ColorPresetControl(this.colorPickerForm_ColorSelected, this.sc_Navigate);
            this.colorDefaultColorControl = new ColorDefaultColorControl(this.colorPickerForm_ColorSelected, this.sc_Navigate);
            this.colorDialogLauncherControl = new ColorDialogLauncherControl(this.colorPickerForm_ColorSelected, this.sc_Navigate);
            this.SuspendLayout();
            //
            // colorPresets
            //
            this.colorPresets.Location = new Point(11, 39);
            this.colorPresets.Name = "colorPresets";
            this.colorPresets.Size = new Size(96, 72);
            this.colorPresets.TabIndex = 1;
            this.colorPresets.TabStop = true;
            //
            // colorDefaultColorControl
            //
            this.colorDefaultColorControl.Location = new Point(5, 5);
            this.colorDefaultColorControl.Name = "colorDefaultColorControl";
            this.colorDefaultColorControl.Size = new Size(108, 32);
            this.colorDefaultColorControl.TabIndex = 0;
            this.colorDefaultColorControl.TabStop = true;
            //
            // colorDialogLauncherControl
            //
            this.colorDialogLauncherControl.Location = new Point(5, 112);
            this.colorDialogLauncherControl.Name = "colorDialogLauncherControl";
            this.colorDialogLauncherControl.Size = new Size(108, 24);
            this.colorDialogLauncherControl.TabIndex = 2;
            this.colorDialogLauncherControl.TabStop = true;

            //
            // ColorPickerForm
            //
            this.AutoScaleMode = AutoScaleMode.None;
            this.AutoScaleBaseSize = new Size(5, 14);
            this.BackColor = SystemColors.Window;
            this.ClientSize = new Size(118, 141);
            this.ControlBox = false;
            this.Controls.Add(this.colorDialogLauncherControl);
            this.Controls.Add(this.colorDefaultColorControl);
            this.Controls.Add(this.colorPresets);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorPickerForm";
            this.Text = "ColorPickerForm";
            this.ResumeLayout(false);
        }
        #endregion

        /// <summary>
        /// Gets the create parameters.
        /// </summary>
        /// <value>The create parameters.</value>
        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;

                // Borderless windows show in the alt+tab window, so this fakes
                // out windows into thinking its a tool window (which doesn't
                // show up in the alt+tab window).
                createParams.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW

                return createParams;
            }
        }

        /// <summary>
        /// Selects the next sub control.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="forward">if set to <c>true</c> [forward].</param>
        private void SelectNextSubControl(int current, bool forward)
        {
            var nextSubControl = forward ? current + 1 : current - 1;
            nextSubControl = (nextSubControl + this.subctrls.Length) % this.subctrls.Length;

            this.currentSubControl = nextSubControl;
            this.subctrls[this.currentSubControl].SelectControl(forward);
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the Win32 message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns><see langword="true" /> if the keystroke was processed and consumed by the control; otherwise, <see langword="false" /> to allow further processing.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            else if (keyData == Keys.Tab)
            {
                this.Close();
                return true;
            }
            else if (keyData == (Keys.Tab | Keys.Shift))
            {
                this.Close();
                return true;
            }
            else if (keyData == Keys.Up | keyData == Keys.Left)
            {
                this.SelectNextSubControl(this.currentSubControl, false);
                return true;
            }
            else if (keyData == Keys.Down | keyData == Keys.Right)
            {
                this.SelectNextSubControl(this.currentSubControl, true);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;

            using (Brush b = new SolidBrush(this.BackColor))
                g.FillRectangle(b, this.ClientRectangle);
            using (var p = new Pen(SystemColors.Highlight, 1))
                g.DrawRectangle(p, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
        }

        /// <summary>
        /// Handles the ColorSelected event of the colorPickerForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ColorSelectedEventArgs"/> instance containing the event data.</param>
        private void colorPickerForm_ColorSelected(object sender, ColorSelectedEventArgs e)
        {
            this.Color = e.SelectedColor;
            this.Close();
        }

        /// <summary>
        /// Handles the Navigate event of the sc control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NavigateEventArgs"/> instance containing the event data.</param>
        private void sc_Navigate(object sender, NavigateEventArgs e)
        {
            var navFrom = -1;
            for (var i = 0; i < this.subctrls.Length; i++)
            {
                if (ReferenceEquals(sender, this.subctrls[i]))
                {
                    navFrom = i;
                    break;
                }
            }

            this.SelectNextSubControl(navFrom, e.Forward);
        }
    }

    /// <summary>
    /// Delegate NavigateEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="NavigateEventArgs"/> instance containing the event data.</param>
    public delegate void NavigateEventHandler(object sender, NavigateEventArgs args);
    /// <summary>
    /// Delegate ColorSelectedEventHandler
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="ColorSelectedEventArgs"/> instance containing the event data.</param>
    public delegate void ColorSelectedEventHandler(object sender, ColorSelectedEventArgs args);
}
