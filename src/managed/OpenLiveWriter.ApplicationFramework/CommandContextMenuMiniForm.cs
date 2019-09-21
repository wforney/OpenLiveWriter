// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Drawing;
    using System.Windows.Forms;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// The command context menu mini form class.
    /// </summary>
    internal class CommandContextMenuMiniForm : BaseForm
    {
        /* NOTE: When being shown in the context of the browser (or any non .NET
         * application) this form will not handle any dialog level keyboard
         * commands (tab, enter, escape, alt-mnemonics, etc.). This is because
         * it is a modeless form that does not have its own thread/message-loop.
         * Because the form was created by our .NET code the main IE frame that
         * has the message loop has no idea it needs to route keyboard events'
         * to us. There are several possible workarounds:
         *
         *    (1) Create and show this form on its own thread with its own
         *        message loop. In this case all calls from the form back
         *        to the main UI thread would need to be marshalled.
         *
         *    (2) Manually process keyboard events in the low-level
         *        ProcessKeyPreview override (see commented out method below)
         *
         *    (3) Change the implementation of the mini-form to be a modal
         *        dialog. The only problem here is we would need to capture
         *        mouse input so that clicks outside of the modal dialog onto
         *        the IE window result in the window being dismissed. We were
         *        not able to get this to work (couldn't capture the mouse)
         *        in experimenting with this implementation.
         *
         * Our judgement was to leave it as-is for now as it is unlikely that
         * keyboard input into a mini-form will be a big deal (the only way
         * to access the mini-form is with a mouse gesture on the toolbar so
         * the user is still in "mouse-mode" when the form pops up.
         *
         */

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandContextMenuMiniForm"/> class.
        /// </summary>
        /// <param name="parentFrame">The parent frame.</param>
        /// <param name="command">The command.</param>
        public CommandContextMenuMiniForm(IWin32Window parentFrame, Command command)
        {
            // save a reference to the parent frame
            this.parentFrame = parentFrame;

            // save a reference to the command and context menu control handler
            this.command = command;
            this.contextMenuControlHandler = command.CommandBarButtonContextMenuControlHandler;

            // set to top most form (allows us to appear on top of our
            // owner if the owner is also top-most)
            this.TopMost = true;

            // other window options/configuration
            this.ShowInTaskbar = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;

            // Paint performance optimizations
            User32.SetWindowLong(this.Handle, GWL.STYLE, User32.GetWindowLong(this.Handle, GWL.STYLE) & ~WS.CLIPCHILDREN);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            // create and initialize the context menu control
            var commandContextMenuControl = this.contextMenuControlHandler.CreateControl();
            commandContextMenuControl.TabIndex = 0;
            commandContextMenuControl.BackColor = BACKGROUND_COLOR;
            commandContextMenuControl.Font = ApplicationManager.ApplicationStyle.NormalApplicationFont;
            commandContextMenuControl.Left = HORIZONTAL_INSET;
            commandContextMenuControl.Top = HEADER_INSET + HEADER_HEIGHT + HEADER_INSET + HEADER_INSET;
            this.Controls.Add(commandContextMenuControl);

            // create action button (don't add it yet)
            this.actionButton = new BitmapButton();
            this.actionButton.TabIndex = 1;
            this.actionButton.Click += new EventHandler(this._actionButton_Click);
            this.actionButton.BackColor = BACKGROUND_COLOR;
            this.actionButton.Font = ApplicationManager.ApplicationStyle.NormalApplicationFont;
            this.actionButton.BitmapDisabled = this.command.CommandBarButtonBitmapDisabled;
            this.actionButton.BitmapEnabled = this.command.CommandBarButtonBitmapEnabled;
            this.actionButton.BitmapPushed = this.command.CommandBarButtonBitmapPushed;
            this.actionButton.BitmapSelected = this.command.CommandBarButtonBitmapSelected;
            this.actionButton.ButtonText = this.contextMenuControlHandler.ButtonText;
            this.actionButton.ToolTip = this.contextMenuControlHandler.ButtonText;
            this.actionButton.AutoSizeWidth = true;
            this.actionButton.AutoSizeHeight = true;
            this.actionButton.Size = new Size(0, 0); // dummy call to force auto-size

            // size the form based on the size of the context menu control and button
            this.Width = HORIZONTAL_INSET + commandContextMenuControl.Width + HORIZONTAL_INSET;
            this.Height = commandContextMenuControl.Bottom + (BUTTON_VERTICAL_PAD * 3) + this.miniFormBevelBitmap.Height + this.actionButton.Height;

            // position the action button and add it to the form
            this.actionButton.Top = this.Height - BUTTON_VERTICAL_PAD - this.actionButton.Height;
            this.actionButton.Left = HORIZONTAL_INSET - 4;
            this.Controls.Add(this.actionButton);
        }

        /// <summary>
        /// Override out Activated event to allow parent form to retains its 'activated'
        /// look (caption bar color, etc.) even when we are active
        /// </summary>
        /// <param name="e"></param>
        protected override void OnActivated(EventArgs e)
        {
            // call base
            base.OnActivated(e);

            // send the parent form a WM_NCACTIVATE message to cause it to to retain it's
            // activated title bar appearance
            User32.SendMessage(this.parentFrame.Handle, WM.NCACTIVATE, new UIntPtr(1), IntPtr.Zero);
        }

        /// <summary>
        /// Automatically close when the form is deactivated
        /// </summary>
        /// <param name="e">event args</param>
        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            // set a timer that will result in the closing of the form
            // (we do this because if actually call Close right here it
            // will prevent the mouse event that resulted in the deactivation
            // of the form from actually triggering in the new target
            // window -- this allows the mouse event to trigger and the
            // form to go away almost instantly
            var closeDelayTimer = new Timer();
            closeDelayTimer.Tick += new EventHandler(this.closeDelayTimer_Tick);
            closeDelayTimer.Interval = 10;
            closeDelayTimer.Start();
        }

        /// <summary>
        /// Actually close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void closeDelayTimer_Tick(object sender, EventArgs e)
        {
            // stop and dispose the timer
            var closeDelayTimer = (Timer)sender;
            closeDelayTimer.Stop();
            closeDelayTimer.Dispose();

            // cancel the form
            this.Cancel();
        }

        // handle painting
        protected override void OnPaint(PaintEventArgs e)
        {
            // get reference to graphics context
            var g = e.Graphics;

            // fill background
            using (var backgroundBrush = new SolidBrush(BACKGROUND_COLOR))
            {
                g.FillRectangle(backgroundBrush, this.ClientRectangle);
            }

            // draw outer border
            var borderRectangle = this.ClientRectangle;
            borderRectangle.Width -= 1;
            borderRectangle.Height -= 1;
            using (var borderPen = new Pen(ApplicationManager.ApplicationStyle.BorderColor))
            {
                g.DrawRectangle(borderPen, borderRectangle);
            }

            // draw header region background
            using (var headerBrush = new SolidBrush(ApplicationManager.ApplicationStyle.PrimaryWorkspaceTopColor))
            {
                g.FillRectangle(headerBrush, HEADER_INSET, HEADER_INSET, this.Width - (HEADER_INSET * 2), HEADER_HEIGHT);
            }

            // draw header region text
            using (var textBrush = new SolidBrush(ApplicationManager.ApplicationStyle.ToolWindowTitleBarTextColor))
            {
                g.DrawString(
                    this.contextMenuControlHandler.CaptionText,
                    ApplicationManager.ApplicationStyle.NormalApplicationFont,
                    textBrush,
                    new PointF(HEADER_INSET + 1, HEADER_INSET + 1));
            }

            // draw bottom bevel line
            g.DrawImage(this.miniFormBevelBitmap, new Rectangle(
                HORIZONTAL_INSET - 1,
                this.Height - (2 * BUTTON_VERTICAL_PAD) - this.actionButton.Height,
                this.Width - (HORIZONTAL_INSET * 2),
                this.miniFormBevelBitmap.Height));
        }

        /// <summary>
        /// Prevent background painting (supports double-buffering)
        /// </summary>
        /// <param name="pevent"></param>
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
        }

        /*
        protected override bool ProcessKeyPreview(ref Message m)
        {
            // NOTE: this is the only keyboard "event" which appears
            // to get called when our form is shown in the browser.
            // if we want to support tab, esc, enter, mnemonics, etc.
            // without creating a new thread/message-loop for this
            // form (see comment at the top) then this is where we
            // would do the manual processing

            return base.ProcessKeyPreview (ref m);
        }
        */

        /// <summary>
        /// User clicked the action button
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void _actionButton_Click(object sender, EventArgs e) => this.Execute();

        /// <summary>
        /// Cancel the mini-form
        /// </summary>
        private void Cancel() => this.Close();

        /// <summary>
        /// Execute the command
        /// </summary>
        private void Execute()
        {
            // get the data entered by the user
            var userInput = this.contextMenuControlHandler.GetUserInput();

            // close the form
            this.Close();

            // tell the context menu control to execute using the specified user input
            this.contextMenuControlHandler.Execute(userInput);
        }

        /// <summary>
        /// Handle to parent frame window
        /// </summary>
        private readonly IWin32Window parentFrame;

        /// <summary>
        /// Command we are associated with
        /// </summary>
        private readonly Command command;

        /// <summary>
        /// Context menu control handler
        /// </summary>
        private ICommandContextMenuControlHandler contextMenuControlHandler;

        /// <summary>
        /// Button user clicks to take action
        /// </summary>
        private BitmapButton actionButton;

        /// <summary>
        /// Button face bitmap
        /// </summary>
        private readonly Bitmap miniFormBevelBitmap = ResourceHelper.LoadAssemblyResourceBitmap("Images.CommandBar.MiniFormBevel.png");

        // layout and drawing constants
        private const int HEADER_INSET = 2;
        private const int HEADER_HEIGHT = 17;
        private const int HORIZONTAL_INSET = 10;
        private const int BUTTON_VERTICAL_PAD = 3;
        private static readonly Color BACKGROUND_COLOR = Color.FromArgb(244, 243, 238);
    }
}
