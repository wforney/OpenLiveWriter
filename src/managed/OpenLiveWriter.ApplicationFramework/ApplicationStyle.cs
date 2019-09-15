// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;

    using OpenLiveWriter.Localization;

    /// <summary>
    /// The ApplicationStyle class.  Defines common application style elements.
    /// </summary>
    public class ApplicationStyle : Component
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        /// <summary>
        /// The display name of the ApplicationStyle.
        /// </summary>
        private string displayName;

        /// <summary>
        /// Gets or sets the display name of the ApplicationStyle.
        /// </summary>
        public string DisplayName
        {
            get => this.displayName;
            set
            {
                if (this.displayName != value)
                {
                    this.displayName = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets the preview image of the ApplicationStyle.
        /// </summary>
        public virtual Image PreviewImage => null;

        /// <summary>
        /// Gets the normal application font.
        /// </summary>
        /// <value>The normal application font.</value>
        public virtual Font NormalApplicationFont => Res.DefaultFont;

        /// <summary>
        /// The active selection color.
        /// </summary>
        private Color activeSelectionColor;

        /// <summary>
        /// Gets or sets the active selection color.
        /// </summary>
        [
            Category("Appearance.Selection"),
                Localizable(false),
                Description("Specifies the active selection color.")
        ]
        public virtual Color ActiveSelectionColor
        {
            get => this.activeSelectionColor;
            set
            {
                if (this.activeSelectionColor != value)
                {
                    this.activeSelectionColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive selection color.
        /// </summary>
        private Color inactiveSelectionColor;

        /// <summary>
        /// Gets or sets the inactive selection color.
        /// </summary>
        [
            Category("Appearance.Selection"),
                Localizable(false),
                Description("Specifies the inactive selection color.")
        ]
        public virtual Color InactiveSelectionColor
        {
            get => this.inactiveSelectionColor;
            set
            {
                if (this.inactiveSelectionColor != value)
                {
                    this.inactiveSelectionColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The menu bitmap area color.
        /// </summary>
        private Color menuBitmapAreaColor;

        /// <summary>
        /// Gets or sets the menu bitmap area color.
        /// </summary>
        [
            Category("Appearance.Menu"),
                Localizable(false),
                Description("Specifies the menu bitmap area color.")
        ]
        public virtual Color MenuBitmapAreaColor
        {
            get => this.menuBitmapAreaColor;
            set
            {
                if (this.menuBitmapAreaColor != value)
                {
                    this.menuBitmapAreaColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The menu selection color.
        /// </summary>
        private Color menuSelectionColor;

        /// <summary>
        /// Gets or sets the menu selection color.
        /// </summary>
        [
            Category("Appearance.Selection"),
                Localizable(false),
                Description("Specifies the menu selection color.")
        ]
        public virtual Color MenuSelectionColor
        {
            get => this.menuSelectionColor;
            set
            {
                if (this.menuSelectionColor != value)
                {
                    this.menuSelectionColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace top color.
        /// </summary>
        private Color primaryWorkspaceTopColor;

        /// <summary>
        /// Gets or sets primary workspace color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace top color.")
        ]
        public virtual Color PrimaryWorkspaceTopColor
        {
            get => this.primaryWorkspaceTopColor;
            set
            {
                if (this.primaryWorkspaceTopColor != value)
                {
                    this.primaryWorkspaceTopColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace bottom color.
        /// </summary>
        private Color primaryWorkspaceBottomColor;

        /// <summary>
        /// Gets or sets primary workspace bottom color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace bottom color.")
        ]
        public virtual Color PrimaryWorkspaceBottomColor
        {
            get => this.primaryWorkspaceBottomColor;
            set
            {
                if (this.primaryWorkspaceBottomColor != value)
                {
                    this.primaryWorkspaceBottomColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The secondary workspace top color.
        /// </summary>
        private Color secondaryWorkspaceTopColor;

        /// <summary>
        /// Gets or sets secondary workspace top color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the secondary workspace top color.")
        ]
        public virtual Color SecondaryWorkspaceTopColor
        {
            get => this.secondaryWorkspaceTopColor;
            set
            {
                if (this.secondaryWorkspaceTopColor != value)
                {
                    this.secondaryWorkspaceTopColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The secondary workspace bottom color.
        /// </summary>
        private Color secondaryWorkspaceBottomColor;

        /// <summary>
        /// Gets or sets secondary workspace color.
        /// </summary>
        [
            Category("Appearance.ApplicationWorkspace"),
                Localizable(false),
                Description("Specifies the secondary workspace bottom color.")
        ]
        public virtual Color SecondaryWorkspaceBottomColor
        {
            get => this.secondaryWorkspaceBottomColor;
            set
            {
                if (this.secondaryWorkspaceBottomColor != value)
                {
                    this.secondaryWorkspaceBottomColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        #region Primary Workspace Command Bar

        /// <summary>
        /// The primary workspace command bar top color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTopColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar top color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top color.")
        ]
        public virtual Color PrimaryWorkspaceCommandBarTopColor
        {
            get => this.primaryWorkspaceCommandBarTopColor;
            set
            {
                if (this.primaryWorkspaceCommandBarTopColor != value)
                {
                    this.primaryWorkspaceCommandBarTopColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar bottom color.
        /// </summary>
        private Color primaryWorkspaceCommandBarBottomColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom color.")
        ]
        public virtual Color PrimaryWorkspaceCommandBarBottomColor
        {
            get => this.primaryWorkspaceCommandBarBottomColor;
            set
            {
                if (this.primaryWorkspaceCommandBarBottomColor != value)
                {
                    this.primaryWorkspaceCommandBarBottomColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar top bevel first line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTopBevelFirstLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar top bevel first line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top bevel first line color.")
        ]
        public Color PrimaryWorkspaceCommandBarTopBevelFirstLineColor
        {
            get => this.primaryWorkspaceCommandBarTopBevelFirstLineColor;
            set
            {
                if (this.primaryWorkspaceCommandBarTopBevelFirstLineColor != value)
                {
                    this.primaryWorkspaceCommandBarTopBevelFirstLineColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar top bevel second line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTopBevelSecondLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar top bevel second line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top bevel second line color.")
        ]
        public Color PrimaryWorkspaceCommandBarTopBevelSecondLineColor
        {
            get => this.primaryWorkspaceCommandBarTopBevelSecondLineColor;
            set
            {
                if (this.primaryWorkspaceCommandBarTopBevelSecondLineColor != value)
                {
                    this.primaryWorkspaceCommandBarTopBevelSecondLineColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar bottom bevel first line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarBottomBevelFirstLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom bevel first line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom bevel first line color.")
        ]
        public Color PrimaryWorkspaceCommandBarBottomBevelFirstLineColor
        {
            get => this.primaryWorkspaceCommandBarBottomBevelFirstLineColor;
            set
            {
                if (this.primaryWorkspaceCommandBarBottomBevelFirstLineColor != value)
                {
                    this.primaryWorkspaceCommandBarBottomBevelFirstLineColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar bottom bevel second line color.
        /// </summary>
        private Color primaryWorkspaceCommandBarBottomBevelSecondLineColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom bevel second line color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom bevel second line color.")
        ]
        public Color PrimaryWorkspaceCommandBarBottomBevelSecondLineColor
        {
            get => this.primaryWorkspaceCommandBarBottomBevelSecondLineColor;
            set
            {
                if (this.primaryWorkspaceCommandBarBottomBevelSecondLineColor != value)
                {
                    this.primaryWorkspaceCommandBarBottomBevelSecondLineColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar text color.
        /// </summary>
        private Color primaryWorkspaceCommandBarTextColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar text color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar text color.")
        ]
        public Color PrimaryWorkspaceCommandBarTextColor
        {
            get => this.primaryWorkspaceCommandBarTextColor;
            set
            {
                if (this.primaryWorkspaceCommandBarTextColor != value)
                {
                    this.primaryWorkspaceCommandBarTextColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The primary workspace command bar disabled text color.
        /// </summary>
        private Color primaryWorkspaceCommandBarDisabledTextColor;

        /// <summary>
        /// Gets or sets the primary workspace command bar disabled text color.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar disabled text color.")
        ]
        public Color PrimaryWorkspaceCommandBarDisabledTextColor
        {
            get => this.primaryWorkspaceCommandBarDisabledTextColor;
            set
            {
                if (this.primaryWorkspaceCommandBarDisabledTextColor != value)
                {
                    this.primaryWorkspaceCommandBarDisabledTextColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar top layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarTopLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar top layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar top layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarTopLayoutMargin
        {
            get => this.primaryWorkspaceCommandBarTopLayoutMargin;
            set
            {
                if (this.primaryWorkspaceCommandBarTopLayoutMargin != value)
                {
                    this.primaryWorkspaceCommandBarTopLayoutMargin = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar left layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarLeftLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar left layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar left layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarLeftLayoutMargin
        {
            get => this.primaryWorkspaceCommandBarLeftLayoutMargin;
            set
            {
                if (this.primaryWorkspaceCommandBarLeftLayoutMargin != value)
                {
                    this.primaryWorkspaceCommandBarLeftLayoutMargin = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar bottom layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarBottomLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar bottom layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar bottom layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarBottomLayoutMargin
        {
            get => this.primaryWorkspaceCommandBarBottomLayoutMargin;
            set
            {
                if (this.primaryWorkspaceCommandBarBottomLayoutMargin != value)
                {
                    this.primaryWorkspaceCommandBarBottomLayoutMargin = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar right layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarRightLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar right layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary workspace command bar right layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarRightLayoutMargin
        {
            get => this.primaryWorkspaceCommandBarRightLayoutMargin;
            set
            {
                if (this.primaryWorkspaceCommandBarRightLayoutMargin != value)
                {
                    this.primaryWorkspaceCommandBarRightLayoutMargin = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///	The primary workspace command bar separator layout margin.
        /// </summary>
        private int primaryWorkspaceCommandBarSeparatorLayoutMargin;

        /// <summary>
        /// Gets or sets the primary workspace command bar separator layout margin.
        /// </summary>
        [
            Category("Appearance.PrimaryWorkspace"),
                Localizable(false),
                Description("Specifies the primary primary workspace command bar separator layout margin.")
        ]
        public int PrimaryWorkspaceCommandBarSeparatorLayoutMargin
        {
            get => this.primaryWorkspaceCommandBarSeparatorLayoutMargin;
            set
            {
                if (this.primaryWorkspaceCommandBarSeparatorLayoutMargin != value)
                {
                    this.primaryWorkspaceCommandBarSeparatorLayoutMargin = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        #endregion Primary Workspace Command Bar

        /// <summary>
        /// The border color.
        /// </summary>
        private Color borderColor;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        [
            Category("Appearance.Border"),
                Localizable(false),
                Description("Specifies the border color.")
        ]
        public Color BorderColor
        {
            get => this.borderColor;
            set
            {
                if (this.borderColor != value)
                {
                    this.borderColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The window color.
        /// </summary>
        private Color windowColor;

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        [
            Category("Appearance.Window"),
                Localizable(false),
                Description("Specifies the window color.")
        ]
        public Color WindowColor
        {
            get => this.windowColor;
            set
            {
                if (this.windowColor != value)
                {
                    this.windowColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The workspace pane control color.
        /// </summary>
        private Color workspacePaneControlColor;

        /// <summary>
        /// Gets or sets the workspace pane control color.
        /// </summary>
        [
            Category("Appearance.Workspace"),
                Localizable(false),
                Description("Specifies the workspace pane control color.")
        ]
        public Color WorkspacePaneControlColor
        {
            get => this.workspacePaneControlColor;
            set
            {
                if (this.workspacePaneControlColor != value)
                {
                    this.workspacePaneControlColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The alert control color.
        /// </summary>
        private Color alertControlColor;

        /// <summary>
        /// Gets or sets the alert control color.
        /// </summary>
        [
            Category("Appearance.Alert"),
                Localizable(false),
                Description("Specifies the alert control color.")
        ]
        public Color AlertControlColor
        {
            get => this.alertControlColor;
            set
            {
                if (this.alertControlColor != value)
                {
                    this.alertControlColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab top color.
        /// </summary>
        private Color activeTabTopColor;

        /// <summary>
        /// Gets or sets the active tab color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab top color.")
        ]
        public Color ActiveTabTopColor
        {
            get => this.activeTabTopColor;
            set
            {
                if (this.activeTabTopColor != value)
                {
                    this.activeTabTopColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab bottom color.
        /// </summary>
        private Color activeTabBottomColor;

        /// <summary>
        /// Gets or sets the active tab color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab bottom color.")
        ]
        public Color ActiveTabBottomColor
        {
            get => this.activeTabBottomColor;
            set
            {
                if (this.activeTabBottomColor != value)
                {
                    this.activeTabBottomColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab highlight color.
        /// </summary>
        private Color activeTabHighlightColor;

        /// <summary>
        /// Gets or sets the active tab highlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab highlight color.")
        ]
        public Color ActiveTabHighlightColor
        {
            get => this.activeTabHighlightColor;
            set
            {
                if (this.activeTabHighlightColor != value)
                {
                    this.activeTabHighlightColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab lowlight color.
        /// </summary>
        private Color activeTabLowlightColor;

        /// <summary>
        /// Gets or sets the active tab lowlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab lowlight color.")
        ]
        public Color ActiveTabLowlightColor
        {
            get => this.activeTabLowlightColor;
            set
            {
                if (this.activeTabLowlightColor != value)
                {
                    this.activeTabLowlightColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The active tab text color.
        /// </summary>
        private Color activeTabTextColor;

        /// <summary>
        /// Gets or sets the active tab text color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the active tab text color.")
        ]
        public Color ActiveTabTextColor
        {
            get => this.activeTabTextColor;
            set
            {
                if (this.activeTabTextColor != value)
                {
                    this.activeTabTextColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab top color.
        /// </summary>
        private Color inactiveTabTopColor;

        /// <summary>
        /// Gets or sets the inactive tab color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab top color.")
        ]
        public Color InactiveTabTopColor
        {
            get => this.inactiveTabTopColor;
            set
            {
                if (this.inactiveTabTopColor != value)
                {
                    this.inactiveTabTopColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab bottom color.
        /// </summary>
        private Color inactiveTabBottomColor;

        /// <summary>
        /// Gets or sets the inactive tab bottom color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab bottom color.")
        ]
        public Color InactiveTabBottomColor
        {
            get => this.inactiveTabBottomColor;
            set
            {
                if (this.inactiveTabBottomColor != value)
                {
                    this.inactiveTabBottomColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab highlight color.
        /// </summary>
        private Color inactiveTabHighlightColor;

        /// <summary>
        /// Gets or sets the inactive tab highlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab highlight color.")
        ]
        public Color InactiveTabHighlightColor
        {
            get => this.inactiveTabHighlightColor;
            set
            {
                if (this.inactiveTabHighlightColor != value)
                {
                    this.inactiveTabHighlightColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab lowlight color.
        /// </summary>
        private Color inactiveTabLowlightColor;

        /// <summary>
        /// Gets or sets the inactive tab lowlight color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab lowlight color.")
        ]
        public Color InactiveTabLowlightColor
        {
            get => this.inactiveTabLowlightColor;
            set
            {
                if (this.inactiveTabLowlightColor != value)
                {
                    this.inactiveTabLowlightColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The inactive tab text color.
        /// </summary>
        private Color inactiveTabTextColor;

        /// <summary>
        /// Gets or sets the inactive tab text color.
        /// </summary>
        [
            Category("Appearance.Tab"),
                Localizable(false),
                Description("Specifies the inactive tab text color.")
        ]
        public Color InactiveTabTextColor
        {
            get => this.inactiveTabTextColor;
            set
            {
                if (this.inactiveTabTextColor != value)
                {
                    this.inactiveTabTextColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window border color.
        /// </summary>
        private Color toolWindowBorderColor;

        /// <summary>
        /// Gets or sets the tool window border color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window border color.")
        ]
        public Color ToolWindowBorderColor
        {
            get => this.toolWindowBorderColor;
            set
            {
                if (this.toolWindowBorderColor != value)
                {
                    this.toolWindowBorderColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window title bar top color.
        /// </summary>
        private Color toolWindowTitleBarTopColor;

        /// <summary>
        /// Gets or sets the tool window title bar top color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window title bar top color.")
        ]
        public Color ToolWindowTitleBarTopColor
        {
            get => this.toolWindowTitleBarTopColor;
            set
            {
                if (this.toolWindowTitleBarTopColor != value)
                {
                    this.toolWindowTitleBarTopColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window title bar bottom color.
        /// </summary>
        private Color toolWindowTitleBarBottomColor;

        /// <summary>
        /// Gets or sets the tool window title bar bottom color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window title bar bottom color.")
        ]
        public Color ToolWindowTitleBarBottomColor
        {
            get => this.toolWindowTitleBarBottomColor;
            set
            {
                if (this.toolWindowTitleBarBottomColor != value)
                {
                    this.toolWindowTitleBarBottomColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window title bar text color.
        /// </summary>
        private Color toolWindowTitleBarTextColor;

        /// <summary>
        /// Gets or sets the tool window title bar text color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window title bar text color.")
        ]
        public Color ToolWindowTitleBarTextColor
        {
            get => this.toolWindowTitleBarTextColor;
            set
            {
                if (this.toolWindowTitleBarTextColor != value)
                {
                    this.toolWindowTitleBarTextColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The tool window background color.
        /// </summary>
        private Color toolWindowBackgroundColor;

        /// <summary>
        /// Gets or sets the tool window background color.
        /// </summary>
        [
            Category("Appearance.ToolWindows"),
                Localizable(false),
                Description("Specifies the tool window background color.")
        ]
        public Color ToolWindowBackgroundColor
        {
            get => this.toolWindowBackgroundColor;
            set
            {
                if (this.toolWindowBackgroundColor != value)
                {
                    this.toolWindowBackgroundColor = value;
                    this.OnChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when a setting in ApplicationStyle changes.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Initializes a new instance of the ApplicationStyle class.
        /// </summary>
        public ApplicationStyle() =>
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() => this.components = new Container();

        #endregion

        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        [
            Browsable(false),
                DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected virtual void OnChanged(EventArgs e) => Changed?.Invoke(this, e);
    }
}
