// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The markup pointer interface.
    /// </summary>
    [ComImport]
    [InterfaceType((short)1)]
    [Guid("3050F49F-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface IMarkupPointer
    {
        /// <summary>
        /// Ownings the document.
        /// </summary>
        /// <param name="ppDoc">The pp document.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void OwningDoc([MarshalAs(UnmanagedType.Interface)] out IHTMLDocument2 ppDoc);

        /// <summary>
        /// Gravities the specified p gravity.
        /// </summary>
        /// <param name="pGravity">The p gravity.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Gravity(out _POINTER_GRAVITY pGravity);

        /// <summary>
        /// Sets the gravity.
        /// </summary>
        /// <param name="Gravity">The gravity.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetGravity([In] _POINTER_GRAVITY Gravity);

        /// <summary>
        /// Clings the specified pf cling.
        /// </summary>
        /// <param name="pfCling">The pf cling.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Cling(out int pfCling);

        /// <summary>
        /// Sets the cling.
        /// </summary>
        /// <param name="fCLing">The f c ling.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetCling([In] int fCLing);

        /// <summary>
        /// Unpositions this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Unposition();

        /// <summary>
        /// Determines whether the specified pf positioned is positioned.
        /// </summary>
        /// <param name="pfPositioned">The pf positioned.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsPositioned(out int pfPositioned);

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <param name="ppContainer">The pp container.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetContainer([MarshalAs(UnmanagedType.Interface)] out IMarkupContainer ppContainer);

        /// <summary>
        /// Moves the adjacent to element.
        /// </summary>
        /// <param name="pElement">The p element.</param>
        /// <param name="eAdj">The e adj.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveAdjacentToElement([In, MarshalAs(UnmanagedType.Interface)] IHTMLElement pElement, [In] _ELEMENT_ADJACENCY eAdj);

        /// <summary>
        /// Moves to pointer.
        /// </summary>
        /// <param name="pPointer">The p pointer.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveToPointer([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointer);

        /// <summary>
        /// Moves to container.
        /// </summary>
        /// <param name="pContainer">The p container.</param>
        /// <param name="fAtStart">The f at start.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveToContainer([In, MarshalAs(UnmanagedType.Interface)] IMarkupContainer pContainer, [In] int fAtStart);

        /// <summary>
        /// Lefts the specified f move.
        /// </summary>
        /// <param name="fMove">The f move.</param>
        /// <param name="pContext">The p context.</param>
        /// <param name="ppElement">The pp element.</param>
        /// <param name="pcch">The PCCH.</param>
        /// <param name="pchText">The PCH text.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void left([In] int fMove, out _MARKUP_CONTEXT_TYPE pContext, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement, [In, Out] ref int pcch, out ushort pchText);

        /// <summary>
        /// Rights the specified f move.
        /// </summary>
        /// <param name="fMove">The f move.</param>
        /// <param name="pContext">The p context.</param>
        /// <param name="ppElement">The pp element.</param>
        /// <param name="pcch">The PCCH.</param>
        /// <param name="pchText">The PCH text.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void right([In] int fMove, out _MARKUP_CONTEXT_TYPE pContext, [MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElement, [In, Out] ref int pcch, out ushort pchText);

        /// <summary>
        /// Currents the scope.
        /// </summary>
        /// <param name="ppElemCurrent">The pp elem current.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void CurrentScope([MarshalAs(UnmanagedType.Interface)] out IHTMLElement ppElemCurrent);

        /// <summary>
        /// Determines whether [is left of] [the specified p pointer that].
        /// </summary>
        /// <param name="pPointerThat">The p pointer that.</param>
        /// <param name="pfResult">The pf result.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsLeftOf([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);

        /// <summary>
        /// Determines whether [is left of or equal to] [the specified p pointer that].
        /// </summary>
        /// <param name="pPointerThat">The p pointer that.</param>
        /// <param name="pfResult">The pf result.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsLeftOfOrEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);

        /// <summary>
        /// Determines whether [is right of] [the specified p pointer that].
        /// </summary>
        /// <param name="pPointerThat">The p pointer that.</param>
        /// <param name="pfResult">The pf result.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsRightOf([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);

        /// <summary>
        /// Determines whether [is right of or equal to] [the specified p pointer that].
        /// </summary>
        /// <param name="pPointerThat">The p pointer that.</param>
        /// <param name="pfResult">The pf result.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsRightOfOrEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfResult);

        /// <summary>
        /// Determines whether [is equal to] [the specified p pointer that].
        /// </summary>
        /// <param name="pPointerThat">The p pointer that.</param>
        /// <param name="pfAreEqual">The pf are equal.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void IsEqualTo([In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pPointerThat, out int pfAreEqual);

        /// <summary>
        /// Moves the unit.
        /// </summary>
        /// <param name="muAction">The mu action.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void MoveUnit([In] _MOVEUNIT_ACTION muAction);

        /// <summary>
        /// Finds the text.
        /// </summary>
        /// <param name="pchFindText">The PCH find text.</param>
        /// <param name="dwFlags">The dw flags.</param>
        /// <param name="pIEndMatch">The p i end match.</param>
        /// <param name="pIEndSearch">The p i end search.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void findText([In] ref ushort pchFindText, [In] uint dwFlags, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndMatch, [In, MarshalAs(UnmanagedType.Interface)] IMarkupPointer pIEndSearch);
    }
}
