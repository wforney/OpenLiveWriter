// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;

    using OpenLiveWriter.Localization;

    /// <summary>
    /// Summary description for ColorDefaultColorControl.
    /// Implements the <see cref="IColorPickerSubControl" />
    /// </summary>
    /// <seealso cref="IColorPickerSubControl" />
    public class ColorDefaultColorControl : IColorPickerSubControl
    {
        /// <summary>
        /// The selected
        /// </summary>
        private bool selected = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorDefaultColorControl"/> class.
        /// </summary>
        /// <param name="colorSelected">The color selected.</param>
        /// <param name="navigate">The navigate.</param>
        public ColorDefaultColorControl(ColorSelectedEventHandler colorSelected, NavigateEventHandler navigate) : base(colorSelected, navigate)
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
            this.Text = Res.Get(StringId.ColorPickerDefaultColor);
            this.Color = Color.Empty;
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ColorDefaultColorControl
            //
            this.Name = "ColorDefaultColorControl";
            this.Size = new Size(108, 24);
        }

        #endregion

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        /// <value>The color.</value>
        /// Used by ColorPickerForm to inform a subcontrol of the currently selected color.
        public override Color Color
        {
            get => Color.Empty;

            set => this.selected = value == Color.Empty;
        }
    }
}
