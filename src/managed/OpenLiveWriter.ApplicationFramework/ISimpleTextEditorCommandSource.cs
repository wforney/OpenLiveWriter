// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    /// Interface ISimpleTextEditorCommandSource
    /// </summary>
    public interface ISimpleTextEditorCommandSource
    {
        /// <summary>
        /// Gets a value indicating whether this instance has focus.
        /// </summary>
        /// <value><c>true</c> if this instance has focus; otherwise, <c>false</c>.</value>
        bool HasFocus { get; }

        /// <summary>
        /// Gets a value indicating whether this instance can undo.
        /// </summary>
        /// <value><c>true</c> if this instance can undo; otherwise, <c>false</c>.</value>
        bool CanUndo { get; }

        /// <summary>
        /// Undoes this instance.
        /// </summary>
        void Undo();

        /// <summary>
        /// Gets a value indicating whether this instance can redo.
        /// </summary>
        /// <value><c>true</c> if this instance can redo; otherwise, <c>false</c>.</value>
        bool CanRedo { get; }

        /// <summary>
        /// Redoes this instance.
        /// </summary>
        void Redo();

        /// <summary>
        /// Gets a value indicating whether this instance can cut.
        /// </summary>
        /// <value><c>true</c> if this instance can cut; otherwise, <c>false</c>.</value>
        bool CanCut { get; }

        /// <summary>
        /// Cuts this instance.
        /// </summary>
        void Cut();

        /// <summary>
        /// Gets a value indicating whether this instance can copy.
        /// </summary>
        /// <value><c>true</c> if this instance can copy; otherwise, <c>false</c>.</value>
        bool CanCopy { get; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        void Copy();

        /// <summary>
        /// Gets a value indicating whether this instance can paste.
        /// </summary>
        /// <value><c>true</c> if this instance can paste; otherwise, <c>false</c>.</value>
        bool CanPaste { get; }

        /// <summary>
        /// Pastes this instance.
        /// </summary>
        void Paste();

        /// <summary>
        /// Gets a value indicating whether this instance can clear.
        /// </summary>
        /// <value><c>true</c> if this instance can clear; otherwise, <c>false</c>.</value>
        bool CanClear { get; }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        void Clear();

        /// <summary>
        /// Selects all.
        /// </summary>
        void SelectAll();

        /// <summary>
        /// Inserts the euro symbol.
        /// </summary>
        void InsertEuroSymbol();

        /// <summary>
        /// Gets a value indicating whether [read only].
        /// </summary>
        /// <value><c>true</c> if [read only]; otherwise, <c>false</c>.</value>
        bool ReadOnly { get; }

        /// <summary>
        /// Occurs when [command state changed].
        /// </summary>
        event EventHandler CommandStateChanged;

        /// <summary>
        /// Occurs when [aggressive command state changed].
        /// </summary>
        event EventHandler AggressiveCommandStateChanged;
    }
}
