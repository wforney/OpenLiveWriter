// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Drawing;

    /// <summary>
    ///     Provides a primary WorkspaceCommandBarLightweightControl.
    /// </summary>
    public class PrimaryWorkspaceWorkspaceCommandBarLightweightControl : CommandBarLightweightControl
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        ///     Initializes a new instance of the ApplicationWorkspaceCommandBarLightweightControl class.
        /// </summary>
        public PrimaryWorkspaceWorkspaceCommandBarLightweightControl() => this.InitializeComponent();

        /// <summary>Initializes a new instance of the ApplicationWorkspaceCommandBarLightweightControl class.</summary>
        /// <param name="container">The container.</param>
        public PrimaryWorkspaceWorkspaceCommandBarLightweightControl(IContainer container)
        {
            // Required for Windows.Forms Class Composition Designer support
            container.Add(this);
            this.InitializeComponent();
        }

        /// <summary>
        ///     Gets the bottom bevel first line color.
        /// </summary>
        public override Color BottomBevelFirstLineColor =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor;

        /// <summary>
        ///     Gets the bottom bevel second line color.
        /// </summary>
        public override Color BottomBevelSecondLineColor =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor;

        /// <summary>
        ///     Gets the bottom command bar color.
        /// </summary>
        public override Color BottomColor => ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomColor;

        /// <summary>
        ///     Gets the bottom layout margin.
        /// </summary>
        public override int BottomLayoutMargin =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarBottomLayoutMargin;

        /// <summary>
        ///     Gets the text color.
        /// </summary>
        public override Color DisabledTextColor =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarDisabledTextColor;

        /// <summary>
        ///     Gets the left layout margin.
        /// </summary>
        public override int LeftLayoutMargin =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarLeftLayoutMargin;

        /// <summary>
        ///     Gets the right layout margin.
        /// </summary>
        public override int RightLayoutMargin =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarRightLayoutMargin;

        /// <summary>
        ///     Gets the separator layout margin.
        /// </summary>
        public override int SeparatorLayoutMargin =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarSeparatorLayoutMargin;

        /// <summary>
        ///     Gets the text color.
        /// </summary>
        public override Color TextColor => ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTextColor;

        /// <summary>
        ///     Gets the top bevel first line color.
        /// </summary>
        public override Color TopBevelFirstLineColor =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopBevelFirstLineColor;

        /// <summary>
        ///     Gets the top bevel second line color.
        /// </summary>
        public override Color TopBevelSecondLineColor =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopBevelSecondLineColor;

        /// <summary>
        ///     Gets the top command bar color.
        /// </summary>
        public override Color TopColor => ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopColor;

        /// <summary>
        ///     Gets the top layout margin.
        /// </summary>
        public override int TopLayoutMargin =>
            ApplicationManager.ApplicationStyle.PrimaryWorkspaceCommandBarTopLayoutMargin;

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ((ISupportInitialize)this).BeginInit();
            ((ISupportInitialize)this).EndInit();
        }
    }
}
