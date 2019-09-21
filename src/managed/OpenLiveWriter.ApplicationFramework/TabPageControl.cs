// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// The TabPageControl control provides a convenient means of getting a control and an optional
    /// TabPageCommandBarLightweightControl to appear as a page on a TabLightweightControl.
    /// Implements the <see cref="UserControl" />
    /// </summary>
    /// <seealso cref="UserControl" />
    public partial class TabPageControl : UserControl
    {
        /// <summary>
        /// Required designer cruft.
        /// </summary>
        private IContainer components;

        /// <summary>
        /// The tab text.
        /// </summary>
        private string tabText;

        /// <summary>
        /// Occurs when the TabPageControl is selected.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the TabPageControl is selected.")
        ]
        public event EventHandler Selected;

        /// <summary>
        /// Occurs when the TabPageControl is selected.
        /// </summary>
        [
            Category("Action"),
                Description("Occurs when the TabPageControl is unselected.")
        ]
        public event EventHandler Unselected;

        /// <summary>
        /// Initializes a new instance of the TabPage class.
        /// </summary>
        public TabPageControl()
        {
            // This call is required by the Windows.Forms Form Designer.
            this.InitializeComponent();
            this.Visible = false;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
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
        private void InitializeComponent()
        {
            this.components = new Container();
            //
            // TabPageControl
            //
            this.Name = "TabPageControl";
            this.Size = new Size(268, 298);

        }
        #endregion

        /// <summary>
        /// Gets or sets the tab bitmap.
        /// </summary>
        /// <value>The tab bitmap.</value>
        [
            Category("Appearance"),
                Localizable(false),
                DefaultValue(null),
                Description("Specifies the tab bitmap.")
        ]
        public Bitmap TabBitmap { get; set; }

        /// <summary>
        /// Gets or sets the tab text.
        /// </summary>
        /// <value>The tab text.</value>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the tab text.")
        ]
        public string TabText
        {
            get => this.tabText;
            set
            {
                this.tabText = value;
                this.AccessibleName = value;
            }
        }

        /// <summary>
        /// Gets or sets the tab ToolTip text.
        /// </summary>
        /// <value>The tab tool tip text.</value>
        [
            Category("Appearance"),
                Localizable(true),
                DefaultValue(null),
                Description("Specifies the tab ToolTip text.")
        ]
        public string TabToolTipText { get; set; }

        /// <summary>
        /// Gets or sets a value which indicates whether the tab is drag-and-drop selectable.
        /// </summary>
        /// <value><c>true</c> if [drag drop selectable]; otherwise, <c>false</c>.</value>
        [
            Category("Behavior"),
                Localizable(false),
                DefaultValue(true),
                Description("Specifies whether the tab is drag-and-drop selectable.")
        ]
        public bool DragDropSelectable { get; set; } = true;

        /// <summary>
        /// Gets the application style.
        /// </summary>
        /// <value>The application style.</value>
        public virtual ApplicationStyle ApplicationStyle => ApplicationManager.ApplicationStyle;

        /// <summary>
        /// Gets a value indicating whether this instance is selected.
        /// </summary>
        /// <value><c>true</c> if this instance is selected; otherwise, <c>false</c>.</value>
        public bool IsSelected { get; private set; } = false;

        /// <summary>
        /// Raises the Selected event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnSelected(EventArgs e)
        {
            this.IsSelected = true;
            Selected?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the Unselected event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnUnselected(EventArgs e)
        {
            this.IsSelected = false;
            Unselected?.Invoke(this, e);
        }

        /// <summary>
        /// Helper method to raise the Selected event.
        /// </summary>
        internal void RaiseSelected() => this.OnSelected(EventArgs.Empty);

        /// <summary>
        /// Helper method to raise the Unselected event.
        /// </summary>
        internal void RaiseUnselected() => this.OnUnselected(EventArgs.Empty);

        /// <summary>
        /// The accessible object
        /// </summary>
        private TabPageAccessibility accessibleObject;

        /// <summary>
        /// Creates a new accessibility object for the control.
        /// </summary>
        /// <returns>A new <see cref="T:System.Windows.Forms.AccessibleObject" /> for the control.</returns>
        protected override AccessibleObject CreateAccessibilityInstance()
        {
            if (this.accessibleObject == null)
            {
                this.accessibleObject = new TabPageAccessibility(this);
            }

            return this.accessibleObject;
        }
    }
}
