// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Drawing;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// A command bar styled for the top of the app.
    /// </summary>
    public class ApplicationCommandBarLightweightControl : CommandBarLightweightControl
    {
        private readonly Bitmap contextMenuArrowBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.WhiteDropArrow.png");
        private readonly Bitmap contextMenuArrowBitmapDisabled = ImageHelper.MakeDisabled(ResourceHelper.LoadAssemblyResourceBitmap("Images.HIG.WhiteDropArrow.png"));

        /// <summary>
        /// Initializes a new instance of the CommandBarLightweightControl class.
        /// </summary>
        /// <param name="container">The container.</param>
        public ApplicationCommandBarLightweightControl(IContainer container) : base(container)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCommandBarLightweightControl"/> class.
        /// </summary>
        public ApplicationCommandBarLightweightControl()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationCommandBarLightweightControl"/> class.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        public ApplicationCommandBarLightweightControl(CommandManager commandManager)
            : this() => this.CommandManager = commandManager;

        /// <summary>
        /// Gets the button margins.
        /// </summary>
        /// <param name="features">The features.</param>
        /// <param name="rightAligned">if set to <c>true</c> [right aligned].</param>
        /// <returns>System.Nullable&lt;ButtonMargins&gt;.</returns>
        public override ButtonMargins? GetButtonMargins(ButtonFeatures features, bool rightAligned)
        {
            if (!rightAligned)
            {
                switch (features)
                {
                    case ButtonFeatures.Image:
                        return new ButtonMargins(6, 0, 0, 6, 0);
                    case ButtonFeatures.Text:
                        return new ButtonMargins(0, 15, 0, 15, 0);
                    case ButtonFeatures.Image | ButtonFeatures.Text:
                        return new ButtonMargins(15, 7, 0, 15, 0);
                    case ButtonFeatures.Text | ButtonFeatures.Menu:
                        return new ButtonMargins(0, 15, 5, 15, 0);
                    case ButtonFeatures.Text | ButtonFeatures.SplitMenu:
                        return new ButtonMargins(0, 15, 9, 5, 10);
                    case ButtonFeatures.Image | ButtonFeatures.Menu:
                        return new ButtonMargins(6, 0, 5, 6, 0);
                    default:
                        return null;
                }
            }
            else
            {
                switch (features)
                {
                    case ButtonFeatures.Text | ButtonFeatures.Menu:
                        return new ButtonMargins(0, 10, 5, 10, 0);
                    case ButtonFeatures.Image:
                        return new ButtonMargins(6, 0, 0, 6, 0);
                    case ButtonFeatures.Image | ButtonFeatures.Menu:
                        return new ButtonMargins(6, 0, 5, 6, 0);
                    default:
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets the text color.
        /// </summary>
        /// <value>The color of the text.</value>
        public override Color TextColor => !this.UseHighContrastMode ? Color.FromArgb(53, 90, 136) : SystemColors.ControlText;

        /// <summary>
        /// Gets the left layout margin.
        /// </summary>
        /// <value>The left layout margin.</value>
        public override int LeftLayoutMargin => 0;

        /// <summary>
        /// Gets the right layout margin.
        /// </summary>
        /// <value>The right layout margin.</value>
        public override int RightLayoutMargin => 3;

        /// <summary>
        /// Gets the top bevel first line color.
        /// </summary>
        /// <value>The color of the top bevel first line.</value>
        public override Color TopBevelFirstLineColor => Color.Transparent;

        /// <summary>
        /// Gets the top bevel second line color.
        /// </summary>
        /// <value>The color of the top bevel second line.</value>
        public override Color TopBevelSecondLineColor => Color.Transparent;

        /// <summary>
        /// Gets the top command bar color.
        /// </summary>
        /// <value>The color of the top.</value>
        public override Color TopColor => Color.Transparent;

        /// <summary>
        /// Gets the bottom command bar color.
        /// </summary>
        /// <value>The color of the bottom.</value>
        public override Color BottomColor => Color.Transparent;

        /// <summary>
        /// Gets the bottom bevel first line color.
        /// </summary>
        /// <value>The color of the bottom bevel first line.</value>
        public override Color BottomBevelFirstLineColor => Color.Transparent;

        /// <summary>
        /// Gets the bottom bevel second line color.
        /// </summary>
        /// <value>The color of the bottom bevel second line.</value>
        public override Color BottomBevelSecondLineColor => Color.Transparent;

        /// <summary>
        /// Gets the context menu arrow bitmap.
        /// </summary>
        /// <value>The context menu arrow bitmap.</value>
        public override Bitmap ContextMenuArrowBitmap => this.contextMenuArrowBitmap;

        /// <summary>
        /// Gets the context menu arrow bitmap disabled.
        /// </summary>
        /// <value>The context menu arrow bitmap disabled.</value>
        public override Bitmap ContextMenuArrowBitmapDisabled => this.contextMenuArrowBitmapDisabled;
    }
}
