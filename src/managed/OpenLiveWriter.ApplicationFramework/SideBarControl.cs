// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Windows.Forms;

    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;

    /// <summary>
    ///     A side-bar control similar to the one appearing in FireBird's preferences dialog.  We use
    ///     this control to build user interfaces similar to a TabControl, but without all the bugs
    ///     from Microsoft.
    /// </summary>
    public class SideBarControl : BorderControl
    {
        /// <summary>
        ///     The pad constant.  Used to provide a bit of air around visual components.
        /// </summary>
        private const int PAD = 2;

        /// <summary>
        ///     BitmapButton list of the entries in the SideBarControl.
        /// </summary>
        private readonly ArrayList bitmapButtonList = new ArrayList();

        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        /// <summary>
        ///     The selected index of the BitmapButton that is selected.
        /// </summary>
        private int selectedIndex = -1;

        /// <summary>
        ///     Initializes a new instance of the SideBarControl class.
        /// </summary>
        public SideBarControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();

            // Set the control to a simple UserControl that will contain the BitmapButtons.
            this.Control = new UserControl();
            this.Control.GotFocus += this.Control_GotFocus;
        }

        /// <summary>
        ///     Occurs when the SelectedIndex property changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Occurs when the SelectedIndex property changes.")]
        public event EventHandler SelectedIndexChanged;

        /// <summary>
        ///     Gets or sets the selected index.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SelectedIndex
        {
            get => this.selectedIndex;
            set
            {
                if (this.selectedIndex == value)
                {
                    return;
                }

                // Ensure that the value is valid.
                Debug.Assert(
                    value == -1 || (value >= 0 && value < this.bitmapButtonList.Count),
                    "SelectedIndex out of range");
                if (!(value == -1 || (value >= 0 && value < this.bitmapButtonList.Count)))
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                // Deselect the currently selected BitmapButton, if there is one.
                if (this.selectedIndex >= 0 && this.selectedIndex < this.bitmapButtonList.Count)
                {
                    var bitmapButton = (BitmapButton)this.bitmapButtonList[this.selectedIndex];
                    bitmapButton.Latched = false;
                }

                // Set the new selected index.
                this.selectedIndex = value;

                // Select the new BitmapButton.
                if (this.selectedIndex != -1)
                {
                    var bitmapButton = (BitmapButton)this.bitmapButtonList[this.selectedIndex];
                    bitmapButton.Latched = true;
                }

                // Raise the SelectedIndexChanged event.
                this.OnSelectedIndexChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Adjusts the size.
        /// </summary>
        public void AdjustSize()
        {
            var maxWidth = this.Width;
            foreach (BitmapButton button in this.bitmapButtonList)
            {
                button.AutoSizeWidth = true;
                button.AutoSizeHeight = true;

                // HACK: AutoSizeWidth doesn't quite work right; it doesn't
                // take effect until SetBoundsCore gets called, so I have
                // to "change" the width to force the SetBoundsCore call.
                // Yuck!!
                button.Width += 1;
                button.Height += 1;
                maxWidth = Math.Max(maxWidth, button.Width);
            }

            foreach (BitmapButton button in this.bitmapButtonList)
            {
                button.AutoSizeWidth = false;
                button.AutoSizeHeight = false;
                button.Width = maxWidth;
                button.Height = (int)Math.Ceiling(button.Height / (DisplayHelper.PixelsPerLogicalInchY / 96f))
                              + (button.BitmapEnabled == null
                                     ? DisplayHelper.ScaleYCeil(10)
                                     : 0); // Add a 10 pixel vertical padding when text-only
            }

            this.Width = maxWidth + (SideBarControl.PAD * 2);
        }

        /// <summary>
        ///     Sets a SideBarControl entry.
        /// </summary>
        /// <param name="index">Index of the entry to set; zero based.</param>
        /// <param name="bitmap">Bitmap of the entry to set.</param>
        /// <param name="text">Text of the entry to set.</param>
        /// <param name="name">The name.</param>
        public void SetEntry(int index, Bitmap bitmap, string text, string name)
        {
            // Instantiate and initialize the BitmapButton.
            var bitmapButton = new BitmapButton
                                   {
                                       Tag = index,
                                       AutoSizeHeight = false,
                                       AutoSizeWidth = false,
                                       ButtonStyle = ButtonStyle.Flat,
                                       TextAlignment = TextAlignment.Right,
                                       ButtonText = text,
                                       BitmapEnabled = bitmap,
                                       BitmapSelected = bitmap,
                                       ClickSetsFocus = true,
                                       Size = new Size(this.Control.Width - (SideBarControl.PAD * 2), 52),
                                       TabStop = false,
                                       AccessibleName = text,
                                       Name = name
                                   };
            bitmapButton.Click += this.bitmapButton_Click;
            this.Control.Controls.Add(bitmapButton);

            // Replace and existing BitmapButton.
            if (index < this.bitmapButtonList.Count)
            {
                // Remove the existing BitmapButton.
                if (this.bitmapButtonList[index] != null)
                {
                    var oldBitmapButton = (BitmapButton)this.bitmapButtonList[index];
                    oldBitmapButton.Click -= this.bitmapButton_Click;
                }

                // Set the new BitmapButton.
                this.bitmapButtonList[index] = bitmapButton;
            }
            else
            {
                // Add a new BitmapButton.
                // Ensure that there are entries up to the index position (make them null).  This
                // allows the user of this control to add his entries out of order or with gaps.
                for (var i = this.bitmapButtonList.Count; i < index; i++)
                {
                    this.bitmapButtonList.Add(null);
                }

                // Add the BitmapButton.
                this.bitmapButtonList.Add(bitmapButton);
            }
        }

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
        ///     Raises the Layout event.
        /// </summary>
        /// <param name="e">A LayoutEventArgs that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            // Call the base class's method so that registered delegates receive the event.
            base.OnLayout(e);

            // Layout each of the bitmap buttons in the entry list.
            var yOffset = SideBarControl.PAD;
            foreach (BitmapButton bitmapButton in this.bitmapButtonList)
            {
                if (bitmapButton == null)
                {
                    continue;
                }

                bitmapButton.Location = new Point(
                    Utility.CenterMinZero(bitmapButton.Width, this.Control.Width),
                    yOffset);
                yOffset += bitmapButton.Height + SideBarControl.PAD;
            }
        }

        /// <summary>
        ///     Raises the SelectedIndexChanged event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelectedIndexChanged(EventArgs e)
        {
            this.SelectedIndexChanged?.Invoke(this, e);
        }

        /// <summary>
        ///     bitmapButton_Click event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void bitmapButton_Click(object sender, EventArgs e)
        {
            // Get the sending BitmapButton.
            var bitmapButton = sender as BitmapButton;
            Debug.Assert(bitmapButton != null, "What??");
            if (bitmapButton == null)
            {
                return;
            }

            // Set the SelectedIndex.
            this.SelectedIndex = (int)bitmapButton.Tag;
        }

        /// <summary>
        ///     Control_GotFocus event handler.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An EventArgs that contains the event data.</param>
        private void Control_GotFocus(object sender, EventArgs e)
        {
            // Drive focus to the correct BitmapButton.
            if (this.selectedIndex != -1)
            {
                var bitmapButton = (BitmapButton)this.bitmapButtonList[this.selectedIndex];
                bitmapButton.Focus();
            }
        }

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() =>

            // SideBarControl
            this.Name = "SideBarControl";
    }
}
