// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.ComponentModel;
    using System.Drawing;

    /// <summary>
    ///     Primary version of the WorkspaceControl.
    /// </summary>
    public class PrimaryWorkspaceControl : WorkspaceControl
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private IContainer components;

        /// <summary>
        ///     The first CommandBarLightweightControl.
        /// </summary>
        private PrimaryWorkspaceWorkspaceCommandBarLightweightControl primaryWorkspaceFirstCommandBarLightweightControl;

        /// <summary>
        ///     The second CommandBarLightweightControl.
        /// </summary>
        private PrimaryWorkspaceWorkspaceCommandBarLightweightControl
            primaryWorkspaceSecondCommandBarLightweightControl;

        /// <summary>Initializes a new instance of the PrimaryWorkspaceControl class.</summary>
        /// <param name="commandManager">The command manager.</param>
        public PrimaryWorkspaceControl(CommandManager commandManager)
            : base(commandManager)
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            // Set the CommandManager of the CommandBarLightweightControls.
            this.primaryWorkspaceFirstCommandBarLightweightControl.CommandManager = commandManager;
            this.primaryWorkspaceSecondCommandBarLightweightControl.CommandManager = commandManager;
        }

        /// <summary>
        ///     Gets the bottom color.
        /// </summary>
        public override Color BottomColor => ApplicationManager.ApplicationStyle.PrimaryWorkspaceBottomColor;

        /// <summary>
        ///     Gets the first command bar lightweight control.
        /// </summary>
        public override CommandBarLightweightControl FirstCommandBarLightweightControl =>
            this.primaryWorkspaceFirstCommandBarLightweightControl;

        /// <summary>
        ///     Gets the second command bar lightweight control.
        /// </summary>
        public override CommandBarLightweightControl SecondCommandBarLightweightControl =>
            this.primaryWorkspaceSecondCommandBarLightweightControl;

        /// <summary>
        ///     Gets the top color.
        /// </summary>
        public override Color TopColor => ApplicationManager.ApplicationStyle.PrimaryWorkspaceTopColor;

        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.primaryWorkspaceFirstCommandBarLightweightControl.LightweightControlContainerControl = null;
                this.primaryWorkspaceSecondCommandBarLightweightControl.LightweightControlContainerControl = null;
                this.primaryWorkspaceFirstCommandBarLightweightControl.CommandBarDefinition = null;
                this.primaryWorkspaceSecondCommandBarLightweightControl.CommandBarDefinition = null;
                this.primaryWorkspaceFirstCommandBarLightweightControl.CommandManager = null;
                this.primaryWorkspaceSecondCommandBarLightweightControl.CommandManager = null;
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
            this.components = new Container();
            this.primaryWorkspaceFirstCommandBarLightweightControl =
                new PrimaryWorkspaceWorkspaceCommandBarLightweightControl(this.components);
            this.primaryWorkspaceSecondCommandBarLightweightControl =
                new PrimaryWorkspaceWorkspaceCommandBarLightweightControl(this.components);
            ((ISupportInitialize)this.primaryWorkspaceFirstCommandBarLightweightControl).BeginInit();
            ((ISupportInitialize)this.primaryWorkspaceSecondCommandBarLightweightControl).BeginInit();
            ((ISupportInitialize)this).BeginInit();

            // primaryWorkspaceFirstCommandBarLightweightControl
            this.primaryWorkspaceFirstCommandBarLightweightControl.LightweightControlContainerControl = this;

            // primaryWorkspaceSecondCommandBarLightweightControl
            this.primaryWorkspaceSecondCommandBarLightweightControl.LightweightControlContainerControl = this;

            // PrimaryWorkspaceControl
            this.AllowDrop = false;
            this.Name = "PrimaryWorkspaceControl";
            ((ISupportInitialize)this.primaryWorkspaceFirstCommandBarLightweightControl).EndInit();
            ((ISupportInitialize)this.primaryWorkspaceSecondCommandBarLightweightControl).EndInit();
            ((ISupportInitialize)this).EndInit();
        }
    }
}
