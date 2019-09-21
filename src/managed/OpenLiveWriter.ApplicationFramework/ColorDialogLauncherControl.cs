// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Security.Permissions;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Summary description for ColorDialogLauncherControl.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IColorPickerSubControl" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.IColorPickerSubControl" />
    public class ColorDialogLauncherControl : IColorPickerSubControl
    {
        /// <summary>
        /// Occurs when [close].
        /// </summary>
        public event EventHandler Close;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDialogLauncherControl"/> class.
        /// </summary>
        /// <param name="colorSelected">The color selected.</param>
        /// <param name="navigate">The navigate.</param>
        public ColorDialogLauncherControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate) : base(colorSelected, navigate)
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
            this.Text = Res.Get(StringId.ColorPickerMoreColors);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ColorDialogLauncherControl
            //
            this.Name = "ColorDialogLauncherControl";
            this.Size = new System.Drawing.Size(108, 24);
            this.Text = "&More Colors…";
        }
        #endregion

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
                this.ShowColorDialog();
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
            this.ShowColorDialog();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress" /> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs" /> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' | e.KeyChar == ' ')
            {
                this.ShowColorDialog();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Shows the color dialog.
        /// </summary>
        private void ShowColorDialog()
        {
            using (var colorDialog = new ColorDialog())
            {
                colorDialog.FullOpen = true;
                colorDialog.CustomColors = ApplicationEnvironment.CustomColors;
                colorDialog.Color = this.Color != Color.Empty ? this.Color : Color.Black;

                Close(this, EventArgs.Empty);
                var result = colorDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    ApplicationEnvironment.CustomColors = colorDialog.CustomColors;
                    this.OnColorSelected(new ColorSelectedEventArgs(colorDialog.Color));
                }
            }
        }
    }
}
