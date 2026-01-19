// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Interop.Windows;

namespace OpenLiveWriter.ApplicationFramework
{
    public class ContainedTextBoxCommandSource : ISimpleTextEditorCommandSource
    {
        public ContainedTextBoxCommandSource(Control parentControl)
        {
            _parentControl = parentControl;
        }

        public bool HasFocus
        {
            get
            {
                return FindFocusedTextBox() != null;
            }
        }

        public bool CanUndo
        {
            get
            {
                TextBoxBase textBox = FindFocusedTextBox();
                return textBox != null && textBox.CanUndo;
            }
        }

        public void Undo()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            textBox?.Undo();
        }

        public bool CanRedo
        {
            get
            {
                // not supported
                return false;
            }
        }

        public void Redo()
        {
            // not supported
        }

        public bool CanCut
        {
            get
            {
                TextBoxBase textBox = FindFocusedTextBox();
                return textBox != null && textBox.SelectionLength > 0;
            }
        }

        public void Cut()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            textBox?.Cut();
        }

        public bool CanCopy
        {
            get
            {
                return CanCut;
            }
        }

        public void Copy()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            textBox?.Copy();
        }

        public bool CanPaste
        {
            get
            {
                return HasFocus;
            }
        }

        public void Paste()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            textBox?.Paste();
        }

        public bool CanClear
        {
            get
            {
                return HasFocus;
            }
        }

        public void Clear()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            if (textBox != null)
            {
                int selectionStart = textBox.SelectionStart;
                if (selectionStart < textBox.Text.Length)
                {
                    textBox.Text = textBox.Text.Remove(textBox.SelectionStart, Math.Max(1, textBox.SelectionLength));
                    textBox.SelectionStart = selectionStart;
                }
            }
        }

        public void SelectAll()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            textBox?.SelectAll();
        }

        public void InsertEuroSymbol()
        {
            TextBoxBase textBox = FindFocusedTextBox();
            if (textBox != null)
            {
                IntPtr euro = Marshal.StringToCoTaskMemUni("\u20AC");
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

        public bool ReadOnly
        {
            get
            {
                TextBoxBase textBox = FindFocusedTextBox();
                return textBox != null && textBox.ReadOnly;
            }
        }

        public event EventHandler CommandStateChanged;

        protected void OnCommandStateChanged()
        {
            CommandStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler AggressiveCommandStateChanged;

        protected void OnAggressiveCommandStateChanged()
        {
            AggressiveCommandStateChanged?.Invoke(this, EventArgs.Empty);
        }

        private TextBoxBase FindFocusedTextBox()
        {
            Control focusedControl = ControlHelper.FindFocused(_parentControl);
            return focusedControl as TextBoxBase;
        }

        private readonly Control _parentControl;
    }
}
