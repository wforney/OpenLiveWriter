// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.ComponentModel;
using System.Drawing;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.ApplicationFramework.ApplicationStyles
{
    public class ApplicationStyleSkyBlue : ApplicationStyle
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public ApplicationStyleSkyBlue()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            //
            // ApplicationStyleSkyBlue
            //
            this.ActiveSelectionColor = System.Drawing.Color.FromArgb(((byte)(107)), ((byte)(140)), ((byte)(210)));
            this.ActiveTabBottomColor = System.Drawing.Color.FromArgb(((byte)(183)), ((byte)(203)), ((byte)(245)));
            this.ActiveTabHighlightColor = System.Drawing.Color.FromArgb(((byte)(243)), ((byte)(243)), ((byte)(243)));
            this.ActiveTabLowlightColor = System.Drawing.Color.FromArgb(((byte)(181)), ((byte)(193)), ((byte)(196)));
            this.ActiveTabTextColor = System.Drawing.Color.Black;
            this.ActiveTabTopColor = System.Drawing.Color.FromArgb(((byte)(207)), ((byte)(227)), ((byte)(253)));
            this.AlertControlColor = System.Drawing.Color.FromArgb(((byte)(255)), ((byte)(255)), ((byte)(194)));
            this.BorderColor = System.Drawing.Color.FromArgb(((byte)(117)), ((byte)(135)), ((byte)(179)));
            this.DisplayName = "Sky Blue";
            this.InactiveSelectionColor = System.Drawing.Color.FromArgb(((byte)(236)), ((byte)(233)), ((byte)(216)));
            this.InactiveTabBottomColor = System.Drawing.Color.FromArgb(((byte)(228)), ((byte)(228)), ((byte)(228)));
            this.InactiveTabHighlightColor = System.Drawing.Color.FromArgb(((byte)(243)), ((byte)(243)), ((byte)(243)));
            this.InactiveTabLowlightColor = System.Drawing.Color.FromArgb(((byte)(189)), ((byte)(189)), ((byte)(189)));
            this.InactiveTabTextColor = System.Drawing.Color.Black;
            this.InactiveTabTopColor = System.Drawing.Color.FromArgb(((byte)(232)), ((byte)(232)), ((byte)(232)));
            this.MenuBitmapAreaColor = System.Drawing.Color.FromArgb(((byte)(195)), ((byte)(215)), ((byte)(249)));
            this.MenuSelectionColor = System.Drawing.Color.FromArgb(((byte)(128)), ((byte)(117)), ((byte)(135)), ((byte)(179)));
            this.PrimaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((byte)(161)), ((byte)(180)), ((byte)(215)));
            this.PrimaryWorkspaceCommandBarBottomBevelFirstLineColor = System.Drawing.Color.FromArgb(((byte)(117)), ((byte)(135)), ((byte)(179)));
            this.PrimaryWorkspaceCommandBarBottomBevelSecondLineColor = System.Drawing.Color.FromArgb(((byte)(166)), ((byte)(187)), ((byte)(223)));
            this.PrimaryWorkspaceCommandBarBottomColor = System.Drawing.Color.FromArgb(((byte)(148)), ((byte)(173)), ((byte)(222)));
            this.PrimaryWorkspaceCommandBarBottomLayoutMargin = 3;
            this.PrimaryWorkspaceCommandBarDisabledTextColor = System.Drawing.Color.Gray;
            this.PrimaryWorkspaceCommandBarLeftLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarRightLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarSeparatorLayoutMargin = 2;
            this.PrimaryWorkspaceCommandBarTextColor = System.Drawing.Color.Black;
            this.PrimaryWorkspaceCommandBarTopBevelFirstLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopBevelSecondLineColor = System.Drawing.Color.Transparent;
            this.PrimaryWorkspaceCommandBarTopColor = System.Drawing.Color.FromArgb(((byte)(193)), ((byte)(213)), ((byte)(249)));
            this.PrimaryWorkspaceCommandBarTopLayoutMargin = 2;
            this.PrimaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((byte)(154)), ((byte)(174)), ((byte)(213)));
            this.SecondaryWorkspaceBottomColor = System.Drawing.Color.FromArgb(((byte)(183)), ((byte)(203)), ((byte)(245)));
            this.SecondaryWorkspaceTopColor = System.Drawing.Color.FromArgb(((byte)(183)), ((byte)(203)), ((byte)(245)));
            this.ToolWindowBackgroundColor = System.Drawing.Color.FromArgb(((byte)(94)), ((byte)(131)), ((byte)(200)));
            this.ToolWindowBorderColor = System.Drawing.Color.FromArgb(((byte)(72)), ((byte)(100)), ((byte)(165)));
            this.ToolWindowTitleBarBottomColor = System.Drawing.Color.FromArgb(((byte)(94)), ((byte)(131)), ((byte)(200)));
            this.ToolWindowTitleBarTextColor = System.Drawing.Color.White;
            this.ToolWindowTitleBarTopColor = System.Drawing.Color.FromArgb(((byte)(126)), ((byte)(166)), ((byte)(237)));
            this.WindowColor = System.Drawing.Color.White;
            this.WorkspacePaneControlColor = System.Drawing.Color.FromArgb(((byte)(214)), ((byte)(223)), ((byte)(247)));

        }
        #endregion

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public override Image PreviewImage
        {
            get
            {
                return ResourceHelper.LoadAssemblyResourceBitmap("ApplicationStyles.Images.SkyBlue.png");
            }
        }
    }
}
