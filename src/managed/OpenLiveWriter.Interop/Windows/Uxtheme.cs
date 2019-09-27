// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;

namespace OpenLiveWriter.Interop.Windows
{
    /// <summary>
    /// Imports from uxtheme.dll.  Just enough to get the theme border color for now.
    /// </summary>
    public class Uxtheme
    {
        public static string CLASS_EDIT = "Edit";

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern bool IsAppThemed();

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern int DrawThemeBackground(IntPtr hTheme,
                                                        IntPtr hdc,
                                                        int iPartId,
                                                        int iStateId,
                                                        ref RECT pRect,
                                                        ref RECT pClipRect);

        [DllImport("uxtheme.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr OpenThemeData(IntPtr hwnd, string classes);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int CloseThemeData(IntPtr htheme);

        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        public static extern int GetThemeColor(IntPtr hTheme,
                                                    int partID,
                                                    int stateID,
                                                    int propID,
                                                    out int color);

        /// <summary>
        /// Window Parts.
        /// </summary>
        public struct WP
        {
            public const int CAPTION = 0x00000001;
            public const int FRAMELEFT = 0x00000007;
            public const int FRAMERIGHT = 0x00000008;
            public const int FRAMEBOTTOM = 0x00000009;
        }

        /// <summary>
        /// Caption State.
        /// </summary>
        public struct CS
        {
            public const int ACTIVE = 0x00000001;
            public const int INACTIVE = 0x00000002;
        }

        /// <summary>
        /// Frame State.
        /// </summary>
        public struct FS
        {
            public const int ACTIVE = 0x00000001;
            public const int INACTIVE = 0x00000002;
        }

        /// <summary>
        /// Edit parts.
        /// </summary>
        public struct EP
        {
            public const int EDITTEXT = 0x00000001;
        }

        /// <summary>
        /// EDITTEXT states.
        /// </summary>
        public struct ETS
        {
            public const int NORMAL = 0x00000001;
        }

        /// <summary>
        /// Theme metrics.
        /// </summary>
        public struct TMT
        {
            public const int BORDERCOLOR = 0x00000ED9;
        }
    }
}
