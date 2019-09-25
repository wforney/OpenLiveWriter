// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Threading;
    using System.Windows.Forms;

    using ApplicationFramework.Skinning;

    using BlogClient;

    using Controls;

    using CoreServices;
    using CoreServices.Layout;

    using Interop.Windows;

    using Localization;
    using Localization.Bidi;

    /// <summary>
    /// The UpdateWeblogProgressForm class.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.BaseForm" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.BaseForm" />
    public partial class UpdateWeblogProgressForm : BaseForm
    {
        /// <summary>
        /// Delegate PublishHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="PublishEventArgs"/> instance containing the event data.</param>
        public delegate void PublishHandler(object sender, PublishEventArgs args);

        /// <summary>
        /// Delegate PublishingHandler
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The <see cref="PublishingEventArgs"/> instance containing the event data.</param>
        public delegate void PublishingHandler(object sender, PublishingEventArgs args);

        /// <summary>
        /// The animation bitmaps
        /// </summary>
        private Bitmap[] animationBitmaps;

        /// <summary>
        /// The default progress message
        /// </summary>
        private readonly string defaultProgressMessage;

        /// <summary>
        /// The ok to close
        /// </summary>
        private bool okToClose;

        /// <summary>
        /// The parent frame
        /// </summary>
        private readonly IWin32Window parentFrame;

        /// <summary>
        /// The progress message
        /// </summary>
        private string progressMessage;

        /// <summary>
        /// The publish
        /// </summary>
        private readonly bool publish;

        /// <summary>
        /// The publishing context
        /// </summary>
        private readonly IBlogPostPublishingContext publishingContext;

        /// <summary>
        /// The republish on success
        /// </summary>
        private bool republishOnSuccess;

        /// <summary>
        /// The update weblog asynchronous operation
        /// </summary>
        private UpdateWeblogAsyncOperation updateWeblogAsyncOperation;

        /// <summary>
        /// The bottom bevel bitmap
        /// </summary>
        private Bitmap bottomBevelBitmap =
            ResourceHelper.LoadAssemblyResourceBitmap("Images.PublishAnimation.BottomBevel.png");

        /// <summary>
        /// The check box view post
        /// </summary>
        private CheckBox checkBoxViewPost;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label publishing to
        /// </summary>
        private Label labelPublishingTo;

        /// <summary>
        /// The progress animated bitmap
        /// </summary>
        private AnimatedBitmapControl progressAnimatedBitmap;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateWeblogProgressForm"/> class.
        /// </summary>
        /// <param name="parentFrame">The parent frame.</param>
        /// <param name="publishingContext">The publishing context.</param>
        /// <param name="isPage">if set to <c>true</c> [is page].</param>
        /// <param name="destinationName">Name of the destination.</param>
        /// <param name="publish">If false, the publishing operation will post as draft</param>
        public UpdateWeblogProgressForm(IWin32Window parentFrame, IBlogPostPublishingContext publishingContext,
                                        bool isPage, string destinationName, bool publish)
        {
            this.InitializeComponent();

            this.checkBoxViewPost.Text = Res.Get(StringId.UpdateWeblogViewPost);

            // reference to parent frame and editing context
            this.parentFrame = parentFrame;
            this.publishingContext = publishingContext;
            this.publish = publish;

            // look and feel (no form border and theme derived background color)
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = ColorizedResources.Instance.FrameGradientLight;

            // bitmaps for animation
            this.progressAnimatedBitmap.Bitmaps = this.AnimationBitmaps;

            // initialize controls
            var entityName = isPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post);
            this.Text = this.FormatFormCaption(entityName, publish);
            this.ProgressMessage = this.defaultProgressMessage =
                                       this.FormatPublishingToCaption(destinationName, entityName, publish);
            this.checkBoxViewPost.Visible = publish;
            this.checkBoxViewPost.Checked = PostEditorSettings.ViewPostAfterPublish;

            // hookup event handlers
            this.checkBoxViewPost.CheckedChanged += this.checkBoxViewPost_CheckedChanged;
        }

        /// <summary>
        /// Makes available the name of the plug-in that caused the publish
        /// operation to be canceled
        /// </summary>
        /// <value>The cancel reason.</value>
        public string CancelReason { get; private set; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception => this.updateWeblogAsyncOperation.Exception;

        /// <inheritdoc />
        protected override CreateParams CreateParams
        {
            get
            {
                var createParams = base.CreateParams;

                // add system standard drop shadow
                const int classStyleDropShadow = 0x20000;
                createParams.ClassStyle |= classStyleDropShadow;

                return createParams;
            }
        }

        /// <summary>
        /// Gets the animation bitmaps.
        /// </summary>
        /// <value>The animation bitmaps.</value>
        private Bitmap[] AnimationBitmaps
        {
            get
            {
                if (this.animationBitmaps == null)
                {
                    var list = new ArrayList();
                    for (var i = 1; i <= 26; i++)
                    {
                        var resourceName = string.Format(CultureInfo.InvariantCulture,
                                                         "Images.PublishAnimation.post{0:00}.png", i);

                        // Add the scaled animation frame bitmap
                        list.Add(
                            DisplayHelper.ScaleBitmap(ResourceHelper.LoadAssemblyResourceBitmap(resourceName))
                        );
                    }

                    this.animationBitmaps = (Bitmap[]) list.ToArray(typeof(Bitmap));
                }

                return this.animationBitmaps;
            }
        }

        /// <summary>
        /// Gets or sets the progress message.
        /// </summary>
        /// <value>The progress message.</value>
        private string ProgressMessage
        {
            get => this.progressMessage;
            set
            {
                this.progressMessage = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// Occurs when [pre publish].
        /// </summary>
        public event PublishHandler PrePublish;

        /// <summary>
        /// Occurs when [publishing].
        /// </summary>
        public event PublishingHandler Publishing;

        /// <summary>
        /// Occurs when [post publish].
        /// </summary>
        public event PublishHandler PostPublish;

        /// <summary>
        /// Handles the <see cref="E:Shown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            this.Update();

            var args = new PublishEventArgs(this.publish);
            this.PrePublish?.Invoke(this, args);

            if (args.Cancel)
            {
                this.CancelReason = args.CancelReason;
                this.SetDialogResult(DialogResult.Abort);
                return;
            }

            this.StartUpdate();
        }

        /// <summary>
        /// Starts the update.
        /// </summary>
        private void StartUpdate()
        {
            var args = new PublishingEventArgs(this.publish);
            this.Publishing?.Invoke(this, args);

            this.republishOnSuccess = args.RepublishOnSuccess;

            // kickoff weblog update
            // Blogger drafts don't have permalinks, therefore, we must do a full publish twice
            var doPublish =
                this.publish; // && (!_republishOnSuccess || !_publishingContext.Blog.ClientOptions.SupportsPostAsDraft);
            this.updateWeblogAsyncOperation =
                new UpdateWeblogAsyncOperation(new BlogClientUIContextImpl(this), this.publishingContext, doPublish);
            this.updateWeblogAsyncOperation.Completed += this._updateWeblogAsyncOperation_Completed;
            this.updateWeblogAsyncOperation.Cancelled += this._updateWeblogAsyncOperation_Cancelled;
            this.updateWeblogAsyncOperation.Failed += this._updateWeblogAsyncOperation_Failed;
            this.updateWeblogAsyncOperation.ProgressUpdated += this._updateWeblogAsyncOperation_ProgressUpdated;
            this.updateWeblogAsyncOperation.Start();
        }

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // fix up layout
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeight(this.checkBoxViewPost);
            }

            // position form
            var parentRect = new RECT();
            User32.GetWindowRect(this.parentFrame.Handle, ref parentRect);
            var parentBounds = RectangleHelper.Convert(parentRect);
            this.Location = new Point(parentBounds.Left + (parentBounds.Width - this.Width) / 2,
                                      parentBounds.Top + (int) (1.5 * this.Height));
        }

        /// <summary>
        /// Formats the form caption.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <returns>System.String.</returns>
        private string FormatFormCaption(string entityName, bool publish) => string.Format(
            CultureInfo.CurrentCulture, Res.Get(StringId.UpdateWeblogPublish1),
            publish ? entityName : Res.Get(StringId.UpdateWeblogDraft));

        /// <summary>
        /// Formats the publishing to caption.
        /// </summary>
        /// <param name="destinationName">Name of the destination.</param>
        /// <param name="entityName">Name of the entity.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <returns>System.String.</returns>
        private string FormatPublishingToCaption(string destinationName, string entityName, bool publish) =>
            string.Format(CultureInfo.CurrentCulture, Res.Get(StringId.UpdateWeblogPublish2),
                          publish ? entityName : Res.Get(StringId.UpdateWeblogDraft), destinationName);

        /// <summary>
        /// Handles the Completed event of the _updateWeblogAsyncOperation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ea">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _updateWeblogAsyncOperation_Completed(object sender, EventArgs ea)
        {
            Debug.Assert(!this.InvokeRequired);

            if (this.republishOnSuccess)
            {
                this.republishOnSuccess = false;
                Debug.Assert(this.publishingContext.BlogPost.Id != null);
                this.StartUpdate();
            }
            else
            {
                this.PostPublish?.Invoke(this, new PublishEventArgs(this.publish));

                this.SetDialogResult(DialogResult.OK);
            }
        }

        /// <summary>
        /// Handles the Cancelled event of the _updateWeblogAsyncOperation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="ea">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void _updateWeblogAsyncOperation_Cancelled(object sender, EventArgs ea)
        {
            Debug.Assert(!this.InvokeRequired);
            Debug.Fail("Cancel not supported for UpdateWeblogAsyncOperation!");
            this.SetDialogResult(DialogResult.OK);
        }

        /// <summary>
        /// Handles the Failed event of the _updateWeblogAsyncOperation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ThreadExceptionEventArgs"/> instance containing the event data.</param>
        private void _updateWeblogAsyncOperation_Failed(object sender, ThreadExceptionEventArgs e)
        {
            Debug.Assert(!this.InvokeRequired);
            this.SetDialogResult(DialogResult.Cancel);
        }

        /// <summary>
        /// Sets the dialog result.
        /// </summary>
        /// <param name="result">The result.</param>
        private void SetDialogResult(DialogResult result)
        {
            this.okToClose = true;
            this.DialogResult = result;
        }

        /// <inheritdoc />
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (this.okToClose)
            {
                this.progressAnimatedBitmap.Stop();
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Override out Activated event to allow parent form to retains its 'activated'
        /// look (caption bar color, etc.) even when we are active
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            // start the animation if necessary (don't start until we are activated so that the
            // loading of the form is not delayed)
            if (!this.progressAnimatedBitmap.Running)
            {
                this.progressAnimatedBitmap.Start();
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the checkBoxViewPost control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void checkBoxViewPost_CheckedChanged(object sender, EventArgs e) =>
            PostEditorSettings.ViewPostAfterPublish = this.checkBoxViewPost.Checked;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkBoxViewPost = new System.Windows.Forms.CheckBox();
            this.progressAnimatedBitmap = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.labelPublishingTo = new System.Windows.Forms.Label();
            this.SuspendLayout();

            //
            // checkBoxViewPost
            //
            this.checkBoxViewPost.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxViewPost.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.checkBoxViewPost.Location = new System.Drawing.Point(19, 136);
            this.checkBoxViewPost.Name = "checkBoxViewPost";
            this.checkBoxViewPost.Size = new System.Drawing.Size(325, 18);
            this.checkBoxViewPost.TabIndex = 1;
            this.checkBoxViewPost.Text = "View in browser after publishing";
            this.checkBoxViewPost.TextAlign = System.Drawing.ContentAlignment.TopLeft;

            //
            // progressAnimatedBitmap
            //
            this.progressAnimatedBitmap.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.progressAnimatedBitmap.Bitmaps = null;
            this.progressAnimatedBitmap.Interval = 100;
            this.progressAnimatedBitmap.Location = new System.Drawing.Point(19, 25);
            this.progressAnimatedBitmap.Name = "progressAnimatedBitmap";
            this.progressAnimatedBitmap.Running = false;
            this.progressAnimatedBitmap.Size = new System.Drawing.Size(321, 71);
            this.progressAnimatedBitmap.TabIndex = 2;
            this.progressAnimatedBitmap.UseVirtualTransparency = false;

            //
            // labelPublishingTo
            //
            this.labelPublishingTo.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Bottom |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.labelPublishingTo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelPublishingTo.Location = new System.Drawing.Point(19, 105);
            this.labelPublishingTo.Name = "labelPublishingTo";
            this.labelPublishingTo.Size = new System.Drawing.Size(317, 18);
            this.labelPublishingTo.TabIndex = 3;
            this.labelPublishingTo.Text = "Publishing to: My Random Ramblings";
            this.labelPublishingTo.UseMnemonic = false;
            this.labelPublishingTo.Visible = false;

            //
            // UpdateWeblogProgressForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(360, 164);
            this.ControlBox = false;
            this.Controls.Add(this.labelPublishingTo);
            this.Controls.Add(this.progressAnimatedBitmap);
            this.Controls.Add(this.checkBoxViewPost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateWeblogProgressForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Publishing {0} to Weblog";
            this.ResumeLayout(false);
        }

        #endregion

        /// <summary>
        /// Handles the ProgressUpdated event of the _updateWeblogAsyncOperation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="progressUpdatedHandler">The <see cref="ProgressUpdatedEventArgs"/> instance containing the event data.</param>
        private void _updateWeblogAsyncOperation_ProgressUpdated(object sender,
                                                                 ProgressUpdatedEventArgs progressUpdatedHandler)
        {
            var msg = progressUpdatedHandler.ProgressMessage;
            if (msg != null)
            {
                this.ProgressMessage = msg;
            }
        }

        /// <inheritdoc />
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = new BidiGraphics(e.Graphics, this.ClientSize, false);

            var colRes = ColorizedResources.Instance;

            // draw the outer border
            using (var p = new Pen(colRes.BorderDarkColor, 1))
            {
                g.DrawRectangle(p, new Rectangle(0, 0, this.ClientSize.Width - 1, this.ClientSize.Height - 1));
            }

            // draw the caption
            using (var f = Res.GetFont(FontSize.Large, FontStyle.Bold))
            {
                g.DrawText(this.Text, f, new Rectangle(19, 8, this.ClientSize.Width - 1, this.ClientSize.Height - 1),
                           SystemColors.WindowText, TextFormatFlags.NoPrefix);
            }

            GdiTextHelper.DrawString(this, this.labelPublishingTo.Font, this.progressMessage,
                                     this.labelPublishingTo.Bounds, false, GdiTextDrawMode.EndEllipsis);
        }

        /// <summary>
        /// Sets the progress message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void SetProgressMessage(string msg) => this.ProgressMessage = msg ?? this.defaultProgressMessage;
    }
}
