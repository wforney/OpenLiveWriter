// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Windows.Forms;

    using OpenLiveWriter.Localization;
    using OpenLiveWriter.Localization.Bidi;

    /// <summary>
    /// <para>Sidebar editor for SmartContent</para>
    /// <para>There is a single instance of a given SmartContentEditor created for each Open Live Writer
    /// post editor window. The implementation of SmartContentEditor objects must therefore be
    /// stateless and assume that they will be the editor for multiple distince SmartContent objects.</para>
    /// </summary>
    public class SmartContentEditor : UserControl
    {
        private bool layedOut = false;

        /// <summary>
        /// Initialize a new SmartContentEditor instance.
        /// </summary>
        public SmartContentEditor()
        {
            this.InitializeComponent();
            this.Font = Res.DefaultFont;
        }

        /// <summary>
        /// Get or set the currently selected SmartContent object. The editor should adapt
        /// its state to the current selection when this property changes (notification
        /// of the change is provided via the SelectedContentChanged event).
        /// </summary>
        public virtual ISmartContent SelectedContent
        {
            get => this.selectedContent;
            set
            {
                this.selectedContent = value;
                this.OnSelectedContentChanged();
            }
        }
        private ISmartContent selectedContent;

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed. The editor should adapt its state to the current
        /// selection when this event is fired.
        /// </summary>
        public event EventHandler SelectedContentChanged;

        /// <summary>
        /// Notification that the currently selected SmartContent object
        /// has changed. The editor should adapt its state to the current
        /// selection when this event is fired.
        /// </summary>
        protected virtual void OnSelectedContentChanged() => SelectedContentChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        public event EventHandler ContentEdited;

        /// <summary>
        /// Event fired by the SmartContentEditor whenever it makes a change
        /// to the underlying properties of the SmartContent.
        /// </summary>
        protected virtual void OnContentEdited() => ContentEdited?.Invoke(this, EventArgs.Empty);

        private void InitializeComponent()
        {
            //
            // SmartContentEditor
            //
            this.Name = "SmartContentEditor";
            this.Size = new System.Drawing.Size(200, 500);

        }

        /// <summary>
        /// Raises the CreateControl method and reverses
        /// the location of any child controls if running
        /// in a right-to-left culture.
        /// </summary>
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!this.layedOut)
            {
                this.layedOut = true;
                BidiHelper.RtlLayoutFixup(this);
            }
        }
    }
}
