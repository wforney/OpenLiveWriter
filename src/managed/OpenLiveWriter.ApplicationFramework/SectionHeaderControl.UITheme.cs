// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Drawing;
    using System.Windows.Forms;
    using OpenLiveWriter.CoreServices.UI;
    using OpenLiveWriter.Localization;

    public partial class SectionHeaderControl
    {
        /// <summary>
        ///     Class UITheme.
        ///     Implements the <see cref="OpenLiveWriter.CoreServices.UI.ControlUITheme" />
        /// </summary>
        /// <seealso cref="OpenLiveWriter.CoreServices.UI.ControlUITheme" />
        private class UITheme : ControlUITheme
        {
            /// <summary>
            ///     The draw gradient
            /// </summary>
            public bool DrawGradient;

            /// <summary>
            ///     The text color
            /// </summary>
            public Color TextColor;

            /// <summary>
            ///     Initializes a new instance of the <see cref="UITheme" /> class.
            /// </summary>
            /// <param name="c">The c.</param>
            public UITheme(Control c)
                : base(c, true)
            {
            }

            /// <summary>
            ///     Applies the theme.
            /// </summary>
            /// <param name="highContrast">if set to <c>true</c> [high contrast].</param>
            protected override void ApplyTheme(bool highContrast)
            {
                this.DrawGradient = !highContrast;
                if (highContrast)
                {
                    this.TextColor = SystemColors.ControlText;
                    this.Control.Font = Res.GetFont(FontSize.XLarge, FontStyle.Bold);
                }
                else
                {
                    this.TextColor = Color.White;
                }

                base.ApplyTheme(highContrast);
            }
        }
    }
}
