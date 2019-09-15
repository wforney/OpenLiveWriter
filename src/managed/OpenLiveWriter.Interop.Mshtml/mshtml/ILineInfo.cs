// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The line information interface.
    /// </summary>
    [ComImport, InterfaceType((short)1), Guid("3050F7E2-98B5-11CF-BB82-00AA00BDCE0B")]
    public interface ILineInfo
    {
        /// <summary>
        /// Gets the x.
        /// </summary>
        /// <value>The x.</value>
        [DispId(0x3e9)]
        int x
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        /// <summary>
        /// Gets the base line.
        /// </summary>
        /// <value>The base line.</value>
        [DispId(0x3ea)]
        int baseLine
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        /// <summary>
        /// Gets the text descent.
        /// </summary>
        /// <value>The text descent.</value>
        [DispId(0x3eb)]
        int textDescent
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        /// <summary>
        /// Gets the height of the text.
        /// </summary>
        /// <value>The height of the text.</value>
        [DispId(0x3ec)]
        int textHeight
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }

        /// <summary>
        /// Gets the line direction.
        /// </summary>
        /// <value>The line direction.</value>
        [DispId(0x3ed)]
        int lineDirection
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            get;
        }
    }
}
