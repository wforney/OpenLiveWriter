// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Class ContainedTextBoxCommandSource.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.ISimpleTextEditorCommandSource" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.ISimpleTextEditorCommandSource" />
    public class ContainedTextBoxCommandSource : ISimpleTextEditorCommandSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainedTextBoxCommandSource"/> class.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        public ContainedTextBoxCommandSource(Control parentControl) => this.parentControl = parentControl;

        /// <summary>
        /// Gets a value indicating whether this instance has focus.
        /// </summary>
        /// <value><c>true</c> if this instance has focus; otherwise, <c>false</c>.</value>
        public bool HasFocus => this.FindFocusedTextBox() != null;

        /// <summary>
        /// Gets a value indicating whether this instance can undo.
        /// </summary>
        /// <value><c>true</c> if this instance can undo; otherwise, <c>false</c>.</value>
        public bool CanUndo
        {
            get
            {
                var textBox = this.FindFocusedTextBox();
                return textBox == null ? false : textBox.CanUndo;
            }
        }

        /// <summary>
        /// Undoes this instance.
        /// </summary>
        public void Undo()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                textBox.Undo();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can redo.
        /// </summary>
        /// <value><c>true</c> if this instance can redo; otherwise, <c>false</c>.</value>
        /// <remarks>not supported</remarks>
        public bool CanRedo => false;

        /// <summary>
        /// Redoes this instance.
        /// </summary>
        public void Redo()
        {
            // not supported
        }

        /// <summary>
        /// Gets a value indicating whether this instance can cut.
        /// </summary>
        /// <value><c>true</c> if this instance can cut; otherwise, <c>false</c>.</value>
        public bool CanCut
        {
            get
            {
                var textBox = this.FindFocusedTextBox();
                return textBox != null ? textBox.SelectionLength > 0 : false;
            }
        }

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        public void Cut()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                textBox.Cut();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can copy.
        /// </summary>
        /// <value><c>true</c> if this instance can copy; otherwise, <c>false</c>.</value>
        public bool CanCopy => this.CanCut;

        /// <summary>
        /// Copies this instance.
        /// </summary>
        public void Copy()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                textBox.Copy();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can paste.
        /// </summary>
        /// <value><c>true</c> if this instance can paste; otherwise, <c>false</c>.</value>
        public bool CanPaste => this.HasFocus;

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        public void Paste()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                textBox.Paste();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance can clear.
        /// </summary>
        /// <value><c>true</c> if this instance can clear; otherwise, <c>false</c>.</value>
        public bool CanClear => this.HasFocus;

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                var selectionStart = textBox.SelectionStart;
                if (selectionStart < textBox.Text.Length)
                {
                    textBox.Text = textBox.Text.Remove(textBox.SelectionStart, Math.Max(1, textBox.SelectionLength));
                    textBox.SelectionStart = selectionStart;
                }
            }
        }

        /// <summary>
        /// Selects all.
        /// </summary>
        public void SelectAll()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }

        /// <summary>
        /// Inserts the euro symbol.
        /// </summary>
        public void InsertEuroSymbol()
        {
            var textBox = this.FindFocusedTextBox();
            if (textBox != null)
            {
                var euro = Marshal.StringToCoTaskMemUni("\u20AC");
                try
                {
                    User32.SendMessage(textBox.Handle, WM.EM_REPLACESEL, new IntPtr(1), euro);
                }
                finally
                {
                    Marshal.FreeCoTaskMem(euro);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        public bool ReadOnly
        {
            get
            {
                var textBox = this.FindFocusedTextBox();
                return textBox != null ? textBox.ReadOnly : false;
            }
        }

        /// <summary>
        /// Occurs when [command state changed].
        /// </summary>
        public event EventHandler CommandStateChanged;

        /// <summary>
        /// Called when [command state changed].
        /// </summary>
        protected void OnCommandStateChanged() => CommandStateChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Occurs when [aggressive command state changed].
        /// </summary>
        public event EventHandler AggressiveCommandStateChanged;

        /// <summary>
        /// Called when [aggressive command state changed].
        /// </summary>
        protected void OnAggressiveCommandStateChanged() => AggressiveCommandStateChanged?.Invoke(this, EventArgs.Empty);

        /// <summary>
        /// Finds the focused text box.
        /// </summary>
        /// <returns>TextBoxBase.</returns>
        private TextBoxBase FindFocusedTextBox()
        {
            var focusedControl = ControlHelper.FindFocused(this.parentControl);
            return focusedControl as TextBoxBase;
        }

        /// <summary>
        /// The parent control
        /// </summary>
        private readonly Control parentControl;
    }
}
