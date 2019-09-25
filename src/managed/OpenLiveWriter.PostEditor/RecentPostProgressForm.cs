// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Windows.Forms;

    using Controls;

    using CoreServices;
    using CoreServices.Layout;

    using Localization;

    /// <summary>
    /// The RecentPostProgressForm class.
    /// Implements the <see cref="OpenLiveWriter.Controls.DelayedAnimatedProgressDialog" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.Controls.DelayedAnimatedProgressDialog" />
    public class RecentPostProgressForm : DelayedAnimatedProgressDialog
    {
        /// <summary>
        /// The animation bitmaps
        /// </summary>
        private Bitmap[] animationBitmaps;

        /// <summary>
        /// The button cancel form
        /// </summary>
        private Button buttonCancelForm;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        /// The label retrieving post
        /// </summary>
        private Label labelRetrievingPost;

        /// <summary>
        /// The picture box separator
        /// </summary>
        private PictureBox pictureBoxSeparator;

        /// <summary>
        /// The progress animated bitmap
        /// </summary>
        private AnimatedBitmapControl progressAnimatedBitmap;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecentPostProgressForm"/> class.
        /// </summary>
        /// <param name="entityName">Name of the entity.</param>
        public RecentPostProgressForm(string entityName)
        {
            //
            // Required for Windows Form Designer support
            //
            this.InitializeComponent();

            this.labelRetrievingPost.Text = Res.Get(StringId.RetrievingFromWeblog);
            this.buttonCancelForm.Text = Res.Get(StringId.CancelButton);
            this.Text = Res.Get(StringId.Retrieving);

            this.Text = string.Format(CultureInfo.CurrentCulture, this.Text, entityName);
            this.labelRetrievingPost.Text = string.Format(CultureInfo.CurrentCulture, this.labelRetrievingPost.Text,
                                                          entityName.ToLower(CultureInfo.CurrentCulture));

            this.progressAnimatedBitmap.Bitmaps = this.AnimationBitmaps;
            this.progressAnimatedBitmap.Interval = 2000 / this.AnimationBitmaps.Length;
            this.SetAnimatatedBitmapControl(this.progressAnimatedBitmap);
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
                    this.animationBitmaps = Enumerable.Range(0, 12)
                                                      .Select(
                                                           i => ResourceHelper.LoadAssemblyResourceBitmap(
                                                               string.Format(
                                                                   CultureInfo.InvariantCulture,
                                                                   "OpenPost.Images.GetRecentPostsAnimation.GetRecentPostsAnimation{0:00}.png",
                                                                   i)))
                                                      .ToArray();
                }

                return this.animationBitmaps;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DisplayHelper.AutoFitSystemButton(this.buttonCancelForm, this.buttonCancelForm.Width, int.MaxValue);
            using (new AutoGrow(this, AnchorStyles.Bottom, false))
            {
                LayoutHelper.NaturalizeHeightAndDistribute(8, this.labelRetrievingPost, this.buttonCancelForm);
            }
        }

        /// <summary>
        /// Handles the Click event of the buttonCancelForm control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void buttonCancelForm_Click(object sender, EventArgs e) => this.Cancel();

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Resources.ResourceManager resources =
                new System.Resources.ResourceManager(typeof(RecentPostProgressForm));
            this.labelRetrievingPost = new System.Windows.Forms.Label();
            this.progressAnimatedBitmap = new OpenLiveWriter.Controls.AnimatedBitmapControl();
            this.pictureBoxSeparator = new System.Windows.Forms.PictureBox();
            this.buttonCancelForm = new System.Windows.Forms.Button();
            this.SuspendLayout();

            //
            // labelRetrievingPost
            //
            this.labelRetrievingPost.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.labelRetrievingPost.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelRetrievingPost.Location = new System.Drawing.Point(10, 91);
            this.labelRetrievingPost.Name = "labelRetrievingPost";
            this.labelRetrievingPost.Size = new System.Drawing.Size(295, 17);
            this.labelRetrievingPost.TabIndex = 1;
            this.labelRetrievingPost.Text = "Retrieving {0} from weblog...";

            //
            // progressAnimatedBitmap
            //
            this.progressAnimatedBitmap.Bitmaps = null;
            this.progressAnimatedBitmap.Interval = 100;
            this.progressAnimatedBitmap.Location = new System.Drawing.Point(38, 3);
            this.progressAnimatedBitmap.Name = "progressAnimatedBitmap";
            this.progressAnimatedBitmap.Running = false;
            this.progressAnimatedBitmap.Size = new System.Drawing.Size(240, 72);
            this.progressAnimatedBitmap.TabIndex = 4;
            this.progressAnimatedBitmap.UseVirtualTransparency = false;

            //
            // pictureBoxSeparator
            //
            this.pictureBoxSeparator.Anchor =
                ((System.Windows.Forms.AnchorStyles) (((System.Windows.Forms.AnchorStyles.Top |
                                                        System.Windows.Forms.AnchorStyles.Left)
                                                     | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBoxSeparator.Image =
                ((System.Drawing.Image) (resources.GetObject("pictureBoxSeparator.Image")));
            this.pictureBoxSeparator.Location = new System.Drawing.Point(10, 84);
            this.pictureBoxSeparator.Name = "pictureBoxSeparator";
            this.pictureBoxSeparator.Size = new System.Drawing.Size(293, 3);
            this.pictureBoxSeparator.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pictureBoxSeparator.TabIndex = 5;
            this.pictureBoxSeparator.TabStop = false;

            //
            // buttonCancelForm
            //
            this.buttonCancelForm.Anchor =
                ((System.Windows.Forms.AnchorStyles) ((System.Windows.Forms.AnchorStyles.Bottom |
                                                       System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancelForm.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.buttonCancelForm.Location = new System.Drawing.Point(228, 112);
            this.buttonCancelForm.Name = "buttonCancelForm";
            this.buttonCancelForm.TabIndex = 6;
            this.buttonCancelForm.Text = "Cancel";
            this.buttonCancelForm.Click += new System.EventHandler(this.buttonCancelForm_Click);

            //
            // RecentPostProgressForm
            //
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.ClientSize = new System.Drawing.Size(317, 141);
            this.Controls.Add(this.buttonCancelForm);
            this.Controls.Add(this.pictureBoxSeparator);
            this.Controls.Add(this.progressAnimatedBitmap);
            this.Controls.Add(this.labelRetrievingPost);
            this.Name = "RecentPostProgressForm";
            this.Text = "Retrieving {0}";
            this.CancelButton = buttonCancelForm;
            this.ResumeLayout(false);
        }

        #endregion
    }
}
