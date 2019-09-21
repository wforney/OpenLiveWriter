// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;

    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// Class IColorPickerSubControl.
    /// Implements the <see cref="UserControl" />
    /// </summary>
    /// <seealso cref="UserControl" />
    public abstract class IColorPickerSubControl : UserControl
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected System.ComponentModel.Container components = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="IColorPickerSubControl"/> class.
        /// </summary>
        /// <param name="colorSelected">The color selected.</param>
        /// <param name="navigate">The navigate.</param>
        public IColorPickerSubControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate)
        {
            ColorSelected += colorSelected;
            Navigate = navigate;
        }

        /// <summary>
        /// Raised by a subcontrol to inform ColorPickerForm that it should navigate
        /// (either for forward or backward) to the next control.
        /// </summary>
        public event NavigateEventHandler Navigate;

        /// <summary>
        /// Called by a subcontrol (derived class of IColorPickerSubControl) to raise the _Navigate event
        /// </summary>
        /// <param name="e">The <see cref="NavigateEventArgs"/> instance containing the event data.</param>
        protected virtual void OnNavigate(NavigateEventArgs e) => Navigate(this, e);

        /// <summary>
        /// Raised by a subcontrol to inform ColorPickerForm that the user has selected a color
        /// </summary>
        private event ColorSelectedEventHandler ColorSelected;

        /// <summary>
        /// Called by a subcontrol (derived class of IColorPickerSubControl) to raise the _ColorSelected event
        /// </summary>
        /// <param name="e">ColorSelectedEventArgs</param>
        protected virtual void OnColorSelected(ColorSelectedEventArgs e) => ColorSelected(this, e);

        /// <summary>
        /// Used by ColorPickerForm to facilitate navigation (up/down arrow keys) across the color picker menu.
        /// </summary>
        /// <param name="forward">if set to <c>true</c> [forward].</param>
        public virtual void SelectControl(bool forward) => this.Select();

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        /// Used by ColorPickerForm to inform a subcontrol of the currently selected color.
        public virtual Color Color
        {
            get => this.color;
            set => this.color = value;
        }

        /// <summary>
        /// The color
        /// </summary>
        protected Color color;

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs" /> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == '\r' || e.KeyChar == ' ')
            {
                this.OnColorSelected(new ColorSelectedEventArgs(this.Color));
                e.Handled = true;
                return;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Paint" /> event.
        /// </summary>
        /// <param name="args">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        protected override void OnPaint(PaintEventArgs args)
        {
            base.OnPaint(args);

            var g = new BidiGraphics(args.Graphics, this.ClientRectangle);
            g.DrawText(this.Text, this.Font, this.ClientRectangle, SystemColors.ControlText, this.TextFormatFlags);
        }

        /// <summary>
        /// Gets the text format flags.
        /// </summary>
        /// <value>The text format flags.</value>
        protected TextFormatFlags TextFormatFlags
        {
            get
            {
                var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                if (!this.ShowKeyboardCues)
                {
                    flags |= TextFormatFlags.HidePrefix;
                }

                return flags;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
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

        /// <summary>
        /// Naturalizes the height.
        /// </summary>
        public void NaturalizeHeight()
        {
            var size = TextRenderer.MeasureText(
                this.Text,
                this.Font,
                new Size(int.MaxValue, int.MaxValue),
                this.TextFormatFlags);
            this.Height = Math.Max(this.Height, size.Height);
            this.Width = size.Width;
        }

        /// <summary>
        /// Processes a mnemonic character.
        /// </summary>
        /// <param name="charCode">The character to process.</param>
        /// <returns><see langword="true" /> if the character was processed as a mnemonic by the control; otherwise, <see langword="false" />.</returns>
        [UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
        protected override bool ProcessMnemonic(char charCode)
        {
            if (IsMnemonic(charCode, this.Text))
            {
                this.OnColorSelected(new ColorSelectedEventArgs(this.Color));
                return true;
            }

            return base.ProcessMnemonic(charCode);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Click" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            this.OnColorSelected(new ColorSelectedEventArgs(this.Color));
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.GotFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            this.Highlight = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.LostFocus" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            this.Highlight = false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseEnter" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            this.Highlight = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseLeave" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.Highlight = false;
        }

        /// <summary>
        /// The highlight
        /// </summary>
        protected bool highlight;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IColorPickerSubControl"/> is highlight.
        /// </summary>
        /// <value><c>true</c> if highlight; otherwise, <c>false</c>.</value>
        protected virtual bool Highlight
        {
            get => this.highlight;
            set
            {
                if (this.highlight != value)
                {
                    this.highlight = value;
                    if (this.highlight)
                    {
                        this.BackColor = SystemColors.Highlight;
                        this.ForeColor = SystemColors.HighlightText;
                    }
                    else
                    {
                        this.BackColor = this.Parent.BackColor;
                        this.ForeColor = SystemColors.ControlText;
                    }

                    this.Invalidate();
                }
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the window message to process.</param>
        /// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
        /// <returns><see langword="true" /> if the character was processed by the control; otherwise, <see langword="false" />.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Up)
            {
                this.OnNavigate(new NavigateEventArgs(false));
                return true;
            }
            else if (keyData == Keys.Down)
            {
                this.OnNavigate(new NavigateEventArgs(true));
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
