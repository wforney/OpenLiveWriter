// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.UI;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Class SatelliteApplicationForm.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.ApplicationForm" />
    /// Implements the <see cref="OpenLiveWriter.CoreServices.UI.IVirtualTransparencyHost" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.ApplicationForm" />
    /// <seealso cref="OpenLiveWriter.CoreServices.UI.IVirtualTransparencyHost" />
    public partial class SatelliteApplicationForm : ApplicationForm, IVirtualTransparencyHost
    {
        /// <summary>
        /// The show frame key
        /// </summary>
        internal const string SHOW_FRAME_KEY = "AlwaysShowMenu";

        /// <summary>
        /// The message perflog flush
        /// </summary>
        private const int MESSAGE_PERFLOG_FLUSH = SatelliteApplicationForm.MESSAGE_PRIVATE_BASE;

        /// <summary>
        /// The message private base
        /// </summary>
        private const int MESSAGE_PRIVATE_BASE = 0x7334;

        // protected IFrameManager _framelessManager;

        /// <summary>
        /// Pixel inset for workspace (allow derived classes to do custom layout within
        /// their workspace with knowledge of the border width)
        /// </summary>
        public static readonly int WorkspaceInset = 0;

        /// <summary>
        /// The new frame UI enabled
        /// </summary>
        private static readonly bool NEW_FRAME_UI_ENABLED = false;

        /// <summary>
        /// Reference to main control hosted on the form
        /// </summary>
        private Control mainControl;

        /// <summary>
        /// The components
        /// </summary>
        private readonly IContainer components = new Container();

        /// <summary>
        /// The initial window state
        /// </summary>
        private FormWindowState initialWindowState = FormWindowState.Normal;

        /// <summary>
        /// The restoring
        /// </summary>
        private bool restoring;

        /// <summary>
        /// The ribbon loaded
        /// </summary>
        private bool ribbonLoaded;

        /// <summary>
        /// The scale
        /// </summary>
        private PointF scale = new PointF(1f, 1f);

        /// <summary>
        /// Initializes a new instance of the <see cref="SatelliteApplicationForm"/> class.
        /// </summary>
        public SatelliteApplicationForm()
        {
            // allow windows to determine the location for new windows
            this.StartPosition = FormStartPosition.WindowsDefaultLocation;

            // use standard product icon
            this.Icon = ApplicationEnvironment.ProductIcon;

            // subscribe to resize event
            this.Resize += this.SatelliteApplicationForm_Resize;

            // subscribe to closing event
            this.Closing += this.SatelliteApplicationForm_Closing;

            // Redraw if resized (mainly for the designer).
            this.SetStyle(ControlStyles.ResizeRedraw, true);

            this.DockPadding.Bottom = 0;
        }

        /// <summary>
        /// Gets a value indicating whether [main menu visible].
        /// </summary>
        /// <value><c>true</c> if [main menu visible]; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// override to let base know if the main menu is visible
        /// </remarks>
        protected override bool MainMenuVisible => false;

        /// <summary>
        /// Create and open a satellite application form
        /// </summary>
        /// <param name="applicationFormType">Type of the application form.</param>
        /// <param name="parameters">The parameters.</param>
        public static void Open(Type applicationFormType, params object[] parameters)
        {
            var launcher = new Launcher(applicationFormType, parameters);
            launcher.OpenForm();
        }

        /// <summary>
        /// Paints the specified arguments.
        /// </summary>
        /// <param name="args">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        void IVirtualTransparencyHost.Paint(PaintEventArgs args)
        {
            // _framelessManager.PaintBackground(args);
            // OnPaint(args);
        }

        /// <summary>
        /// Updates the state of the frameless.
        /// </summary>
        /// <param name="frameless">if set to <c>true</c> [frameless].</param>
        internal void UpdateFramelessState(bool frameless)
        {
            if (SatelliteApplicationForm.NEW_FRAME_UI_ENABLED)
            {
                return;
            }

            this.SuspendLayout();
            if (frameless)
            {
                this.DockPadding.Top = 26;
                this.DockPadding.Left = 7;
                this.DockPadding.Right = 7;
            }
            else
            {
                this.DockPadding.Top = 0;
                this.DockPadding.Left = 0;
                this.DockPadding.Right = 0;
            }

            this.ResumeLayout();
        }

        ////// overridable methods used to customize the UI of the satellite form
        //// protected virtual CommandBarDefinition FirstCommandBarDefinition { get { return null; }	}

        /// <summary>
        /// Creates the main control.
        /// </summary>
        /// <returns>Control.</returns>
        protected virtual Control CreateMainControl() => null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        /////// <summary>
        /////// The primary command bar control.
        /////// </summary>
        //// protected virtual Control CommandBarControl
        //// {
        //// get{ return _commandBarControl; }
        //// }

        // overridable processing methods

        /// <summary>
        /// Called when [background timer tick].
        /// </summary>
        protected virtual void OnBackgroundTimerTick()
        {
        }

        /// <summary>
        /// Initialize the state of the workspace -- only override this for advanced
        /// customization of the workspace. The default implementation queries the
        /// the subclass for the UI to initialize with via the FirstCommandBarDefinition,
        /// SecondCommandBarDefinition, and PrimaryControl properties
        /// </summary>
        protected virtual void OnInitializeWorkspace()
        {
            // Hmm.  How to do this?
            this.mainControl = this.CreateMainControl();

            // CommandBarLightweightControl commandBar = new ApplicationCommandBarLightweightControl();
            // commandBar.CommandManager = ApplicationManager.CommandManager;
            // commandBar.LeftContainerOffSetSpacing = 0;
            // _commandBarControl = new TransparentCommandBarControl(commandBar, FirstCommandBarDefinition);
            // _commandBarControl.Height = (int)Ribbon.GetHeight() + 34;
            // _commandBarControl.Height = 34;

            // _commandBarControl.Dock = DockStyle.Top;
            this.PositionMainControl();

            // Controls.Add(_commandBarControl);
            this.Controls.Add(this.mainControl);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout" /> event.
        /// </summary>
        /// <param name="levent">The event data.</param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            // force command bar to re-layout
            // if ( _commandBarControl != null )
            // _commandBarControl.ForceLayout();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                this.SuspendLayout();

                // initialize the workspace
                this.OnInitializeWorkspace();
                this.ribbonLoaded = true;

                // restore window state
                this.RestoreWindowState();

                this.ResumeLayout();
            }
            catch (Exception ex)
            {
                UnexpectedErrorMessage.Show(Win32WindowImpl.DesktopWin32Window, ex);
                this.Close();
            }
        }

        /// <summary>
        /// Paints the background of the control.
        /// </summary>
        /// <param name="pevent">A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains information about the control to paint.</param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            base.OnPaintBackground(pevent);

            // _framelessManager.PaintBackground(pevent);
        }

        /// <summary>
        /// Positions the main control.
        /// </summary>
        protected void PositionMainControl()
        {
            var TOP_PADDING = this.ScaleY(0); // 26);
            var SIDE_PADDING = this.ScaleX(0);
            var BOTTOM_PADDING = this.ScaleY(0); // 25) ;

            if (SatelliteApplicationForm.NEW_FRAME_UI_ENABLED)
            {
                TOP_PADDING = 0;
                SIDE_PADDING = 0;
                BOTTOM_PADDING = this.ScaleY(0); // 25) ;
            }

            this.DockPadding.Top = TOP_PADDING;
            this.DockPadding.Left = SIDE_PADDING;
            this.DockPadding.Right = SIDE_PADDING;
            this.DockPadding.Bottom = BOTTOM_PADDING;

            this.mainControl.Dock = DockStyle.Fill;
        }

        /// <summary>
        /// Restores the state of the window.
        /// </summary>
        protected virtual void RestoreWindowState()
        {
        }

        /// <summary>
        /// Saves the state of the maximized window.
        /// </summary>
        protected virtual void SaveMaximizedWindowState()
        {
        }

        /// <summary>
        /// Saves the state of the normal window.
        /// </summary>
        protected virtual void SaveNormalWindowState()
        {
        }

        /// <summary>
        /// Scales the location, size, padding, and margin of a control.
        /// </summary>
        /// <param name="factor">The factor by which the height and width of the control are scaled.</param>
        /// <param name="specified">A <see cref="T:System.Windows.Forms.BoundsSpecified" /> value that specifies the bounds of the control to use when defining its size and position.</param>
        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            this.SaveScale(factor.Width, factor.Height);
            base.ScaleControl(factor, specified);
        }

        /// <summary>
        /// This method is not relevant for this class.
        /// </summary>
        /// <param name="dx">The horizontal scaling factor.</param>
        /// <param name="dy">The vertical scaling factor.</param>
        protected override void ScaleCore(float dx, float dy)
        {
            this.SaveScale(dx, dy);
            base.ScaleCore(dx, dy);
        }

        /// <summary>
        /// Scales the x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <returns>System.Int32.</returns>
        protected int ScaleX(int x) => (int)(x * this.scale.X);

        /// <summary>
        /// Scales the y.
        /// </summary>
        /// <param name="y">The y.</param>
        /// <returns>System.Int32.</returns>
        protected int ScaleY(int y) => (int)(y * this.scale.Y);

        /// <summary>
        /// Performs the work of setting the specified bounds of this control.
        /// </summary>
        /// <param name="x">The new <see cref="P:System.Windows.Forms.Control.Left" /> property value of the control.</param>
        /// <param name="y">The new <see cref="P:System.Windows.Forms.Control.Top" /> property value of the control.</param>
        /// <param name="width">The new <see cref="P:System.Windows.Forms.Control.Width" /> property value of the control.</param>
        /// <param name="height">The new <see cref="P:System.Windows.Forms.Control.Height" /> property value of the control.</param>
        /// <param name="specified">A bitwise combination of the <see cref="T:System.Windows.Forms.BoundsSpecified" /> values.</param>
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // WinLive 40828: Writer window's height keeps growing when each time Writer window is restored.
            // This is a hack.
            // For some unknown reason after we have the ribbon hooked up, the height parameter passed to this
            // method when the form is restored from a minimized/maximized state is ~30px too large (depending
            // on DPI). However, the this.Height property is correct, so we can just use it instead.
            var newHeight = this.ribbonLoaded && this.restoring ? this.Height : height;
            base.SetBoundsCore(x, y, width, newHeight, specified);
        }

        /// <summary>
        /// WNDs the proc.
        /// </summary>
        /// <param name="m">The m.</param>
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case (int)WM.QUERYENDSESSION:
                    m.Result = new IntPtr(1);
                    return;
                case (int)WM.ENDSESSION:
                    {
                        var isSessionEnding = Convert.ToBoolean(m.WParam.ToInt32())
                                           && (((uint)m.LParam.ToInt32() | ENDSESSION.ENDSESSION_CLOSEAPP) != 0
                                            || ((uint)m.LParam.ToInt32() | ENDSESSION.ENDSESSION_CRITICAL) != 0
                                            || ((uint)m.LParam.ToInt32() | ENDSESSION.ENDSESSION_LOGOFF) != 0);

                        if (isSessionEnding)
                        {
                            ((ISessionHandler)this.mainControl).OnEndSession();
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }

                    break;
                case SatelliteApplicationForm.MESSAGE_PERFLOG_FLUSH:
                    if (ApplicationPerformance.IsEnabled)
                    {
                        ApplicationPerformance.FlushLogFile();
                    }

                    return;
            }

            // if (_framelessManager != null && !_framelessManager.WndProc(ref m))
            base.WndProc(ref m);
        }

        /// <summary>
        /// Handles the Closing event of the SatelliteApplicationForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CancelEventArgs"/> instance containing the event data.</param>
        private void SatelliteApplicationForm_Closing(object sender, CancelEventArgs e)
        {
            // give the main control first crack at cancelling the form close
            if (this.mainControl is IFormClosingHandler formClosingHandler)
            {
                formClosingHandler.OnClosing(e);
                if (e.Cancel)
                {
                    return;
                }

                formClosingHandler.OnClosed();
            }

            switch (this.WindowState)
            {
                // save the current window state
                case FormWindowState.Normal:
                    this.SaveNormalWindowState();
                    break;
                case FormWindowState.Maximized:
                    this.SaveMaximizedWindowState();
                    break;
            }
        }

        /// <summary>
        /// Handles the Resize event of the SatelliteApplicationForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void SatelliteApplicationForm_Resize(object sender, EventArgs e)
        {
            this.restoring = this.WindowState == FormWindowState.Normal
                          && this.initialWindowState != FormWindowState.Normal;
            this.initialWindowState = this.WindowState;
        }

        /// <summary>
        /// Saves the scale.
        /// </summary>
        /// <param name="dx">The x value.</param>
        /// <param name="dy">The y value.</param>
        private void SaveScale(float dx, float dy) => this.scale = new PointF(this.scale.X * dx, this.scale.Y * dy);
    }
}
