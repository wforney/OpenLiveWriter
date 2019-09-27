// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenLiveWriter.Interop.Windows
{

    /// <summary>
    /// Imports from User32.dll
    /// </summary>
    public class User32
    {
        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern int GetKeyboardLayout(int dwLayout);

        [DllImport("user32.dll")]
        public static extern bool SetMenu(IntPtr hWnd, IntPtr hMenu);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, [Out] StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        [StructLayout(LayoutKind.Sequential)]
        public struct ICONINFO
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr hbmMask;
            public IntPtr hbmColor;
        }

        [DllImport("User32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("User32.dll")]
        public static extern IntPtr GetCapture();

        [DllImport("User32.dll")]
        public static extern IntPtr SetCapture(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr LoadCursor(
            IntPtr hInstance,
            IntPtr lpCursorName
            );

        [DllImport("User32.dll")]
        public static extern bool OpenClipboard(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern int CountClipboardFormats();

        [DllImport("User32.dll")]
        public static extern bool EmptyClipboard();

        [DllImport("User32.dll")]
        public static extern bool CloseClipboard();

        [DllImport("User32.dll")]
        public static extern bool SetSystemCursor(IntPtr hcur, uint id);

        [DllImport("User32.dll")]
        public static extern IntPtr SetCursor(IntPtr hcur);

        [DllImport("User32.dll")]
        public static extern bool EnableWindow(IntPtr hcur, bool bEnable);

        [DllImport("User32.dll")]
        public static extern uint MsgWaitForMultipleObjects(
            uint nCount,
            IntPtr[] pHandles,
            bool bWaitAll,
            uint dwMilliseconds,
            QS dwWakeMask
        );

        [DllImport("User32.dll")]
        public static extern bool InvalidateRect(
            IntPtr hWnd,           // handle to window
            ref RECT lpRect,  // rectangle coordinates
                              //IntPtr lpRect,
            bool bErase          // erase state
            );

        [DllImport("User32.dll", EntryPoint = "InvalidateRect")]
        public static extern bool InvalidateWindow(
            IntPtr hWnd,           // handle to window
            IntPtr lpRect,       // pass IntPtr.Zero to invalidate entire window
            bool bErase          // erase state
            );

        [DllImport("User32.dll")]
        public static extern bool UpdateWindow(
            IntPtr hWnd   // handle to window
            );

        [DllImport("User32.dll")]
        public static extern bool UpdateLayeredWindow(
            IntPtr hwnd,
            IntPtr hdcDst,
            ref POINT pptDst,
            ref SIZE psize,
            IntPtr hdcSrc,
            ref POINT pptSrc,
            uint crKey,
            ref BLENDFUNCTION pblend,
            uint dwFlags);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [DllImport("User32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool SetWindowText(
            IntPtr hWnd,
            [MarshalAs(UnmanagedType.LPTStr)] string lpString
            );

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern uint RegisterWindowMessage(
            [MarshalAs(UnmanagedType.LPTStr)] string lpString
            );

        [DllImport("User32.dll")]
        public static extern bool DestroyIcon(IntPtr hIcon);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindowEx(
            IntPtr hwndParent, IntPtr hwndChildAfter,
            [In, MarshalAs(UnmanagedType.LPTStr)] string lpszClass,
            [In, MarshalAs(UnmanagedType.LPTStr)] string lpszWindow
            );

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(
            [In, MarshalAs(UnmanagedType.LPTStr)] string lpszClass,
            [In, MarshalAs(UnmanagedType.LPTStr)] string lpszWindow
            );

        /// <summary>
        /// Set's the input focus to the specified window.
        /// </summary>
        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>
        /// Changes the parent window of the specified child window.
        /// </summary>
        [DllImport("User32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("User32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        /// <summary>
        /// Returns the active window of the calling thread
        /// </summary>
        [DllImport("User32.dll")]
        public static extern IntPtr GetActiveWindow();

        [DllImport("User32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("User32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("User32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
            int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll")]
        public static extern bool MoveWindow(
            IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("User32.dll")]
        public static extern bool ScreenToClient(
            IntPtr hWnd,        // handle to window
            ref POINT lpPoint   // screen coordinates
            );

        //If the function succeeds, the low-order word of the return value is the number of pixels added
        //to the horizontal coordinate of each source point in order to compute the horizontal
        //coordinate of each destination point; the high-order word is the number of pixels added
        //to the vertical coordinate of each source point in order to compute the vertical coordinate
        //of each destination point.
        //If the function fails, the return value is zero. Call SetLastError prior to calling this method
        //to differentiate an error return value from a legitimate "0" return value.

        //If hWndFrom or hWndTo (or both) are mirrored windows (that is, have WS_EX_LAYOUTRTL extended style),
        //MapWindowPoints will automatically adjust mirrored coordinates
        //if you pass two or less points in lpPoints.
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int MapWindowPoints(
            IntPtr hwndFrom,  //handle to window mapping from
            IntPtr hwndTo,   //handle to window mapping to
            ref POINT lpPoints, //array of points to map
            [MarshalAs(UnmanagedType.U4)] int cPoints //size of the array
            );

        public static readonly int GWL_EXSTYLE = (-20);
        public const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring

        [DllImport("User32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern IntPtr GetAncestor(IntPtr hWnd, GA gaFlags);

        [DllImport("User32.dll")]
        public static extern bool AnimateWindow(IntPtr hWnd, uint dwTime, AW dwFlags);

        /// <summary>
        /// The IsWindow function determines whether the specified
        /// window handle identifies an existing window.
        /// </summary>
        /// <param name="hWnd">
        /// [in] Handle to the window to test.
        /// </param>
        /// <returns>
        /// If the window handle identifies an existing window, the return value is nonzero.
        /// If the window handle does not identify an existing window, the return value is zero.
        /// </returns>
        [DllImport("User32.dll")]
        public static extern bool IsWindow(IntPtr hWnd);

        [DllImport("User32.dll")]
        public static extern uint WaitForInputIdle(IntPtr hProcess, uint dwMilliseconds);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);

        /// <summary>
        /// The GetDCEx function retrieves a handle to a display device context (DC) for the client
        /// area of a specified window or for the entire screen. You can use the returned handle in
        /// subsequent GDI functions to draw in the DC.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr region, uint dw);

        /// <summary>
        /// The ReleaseDC function releases a device context (DC), freeing it for use by other
        /// applications. The effect of the ReleaseDC function depends on the type of DC. It frees
        /// only common and window DCs. It has no effect on class or private DCs.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool ReleaseDC(IntPtr hWnd, IntPtr dc);

        /// <summary>
        /// The GetWindowLong function retrieves information about the specified window. The
        /// function also retrieves the 32-bit (long) value at the specified offset into the extra
        /// window memory.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// The SetWindowLong function changes an attribute of the specified window. The function
        /// also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, [Out] StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("User32.dll")]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        /// <summary>
        /// Translates virtual-key messages into character messages. The character
        /// messages are posted to the calling thread's message queue, to be read
        /// the next time the thread calls the GetMessage or PeekMessage function.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool TranslateMessage([In] ref MSG msg);

        /// <summary>
        /// Dispatches a message to a window procedure.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage([In] ref MSG msg);

        /// <summary>
        /// Delegate used for SetWindowsHookEx
        /// </summary>
        public delegate IntPtr HookDelegate(int nCode, UIntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Installs a windows hook procedure (used for low level filtering of
        /// keyboard, mouse, events, etc.)
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(
            WH hookType, HookDelegate lpfn, IntPtr hMod, uint dwThreadId);

        /// <summary>
        /// Uninstalls a windows hook procedure
        /// </summary>
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        /// <summary>
        /// Calls the next hook in a hook chain
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr CallNextHookEx(
            IntPtr hhk, int nCode, UIntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window
        /// and, optionally, the identifier of the process that created the window
        /// </summary>
        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(
            IntPtr hWnd, IntPtr lpdwProcessId);

        /// <summary>
        /// Retrieves the status of the specified virtual key. The status specifies
        /// whether the key is up, down, or toggled (on, off—alternating each time
        /// the key is pressed).
        /// </summary>
        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(
            IntPtr hWnd,
            uint Msg,
            UIntPtr wParam,
            IntPtr lParam
            );

        [DllImport("user32.dll")]
        public static extern uint GetMenuItemID(IntPtr hMenu, int nPos);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool TrackMouseEvent(ref TRACKMOUSEEVENT lpEventTrack);

        public const uint TME_NONCLIENT = 0x00000010;
        public const uint TME_HOVER = 0x00000001;
        public const uint TME_QUERY = 0x40000000;
        public const uint TME_CANCEL = 0x80000000;
        public const uint TME_LEAVE = 0x00000010;

        [DllImport("user32.dll")]
        public static extern int TrackPopupMenu(
            IntPtr hMenu,
            uint uFlags,
            int x,
            int y,
            int nReserved,
            IntPtr hWnd,
            IntPtr prcRect);

        [DllImport("User32.dll")]
        public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("User32.dll")]
        public static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("user32.dll")]
        public static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);

        /// <summary>
        /// Registers a new clipboard format. This format can then be used as a valid
        /// clipboard format. Return value is an integer id representing the format.
        /// </summary>
        [DllImport("User32.dll")]
        public static extern uint RegisterClipboardFormat(string lpszFormat);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        public static extern int SetWindowProc(IntPtr hWnd, int nIndex, WndProcDelegate lpWndProc);

        // SetWindowLong - Conventional declaration with integer value parameter
        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Unicode)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int nValue);

        // CallWindowProc
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CallWindowProc(IntPtr lpWndProc, IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        //get a title bar structure for a given hWnd
        [DllImport("user32.dll")]
        public static extern bool GetTitleBarInfo(IntPtr hWnd, ref TITLEBARINFO pti);

        //get window info, such as border width, for a given hWnd
        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hWnd, ref WINDOWINFO pwi);

        // Gets the text from a text box
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int GetText(IntPtr hWnd, int Msg, ref GETTEXTEX wParam, StringBuilder lParam);

        [StructLayout(LayoutKind.Sequential)]
        public struct GETTEXTEX
        {
            public int cb;
            public int flags;
            public int codepage;
            public IntPtr lpDefaultChar;
            public IntPtr lpUsedDefChar;
        }

        public const int EM_GETTEXTEX = 0x0400 + 94;

        [DllImport("user32.dll")]
        public static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("user32.dll")]
        public static extern bool FlashWindowEx(ref FLASHWINFO pfwi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public FLASHWINFO(IntPtr hwnd, int dwFlags, int uCount, int dwTimeout)
            {
                this.hwnd = hwnd;
                this.dwFlags = dwFlags;
                this.uCount = uCount;
                this.dwTimeout = dwTimeout;

                // The size of the IntPtr + 4 Int32s
                cbSize = IntPtr.Size + 16;
            }

            public int cbSize;
            public IntPtr hwnd;
            public int dwFlags;
            public int uCount;
            public int dwTimeout;
        }

        public struct TRACKMOUSEEVENT
        {
            public int cbSize;
            public uint dwFlags;
            public IntPtr hwndTrack;
            public uint dwHoverTime;
        }

        public struct FlashStatus
        {
            public static int FLASHW_STOP = 0;
            public static int FLASHW_CAPTION = 1;
            public static int FLASHW_TRAY = 2;
            public static int FLASHW_ALL = (FlashStatus.FLASHW_CAPTION | FlashStatus.FLASHW_TRAY);
            public static int FLASHW_TIMER = 4;
            public static int FLASHW_TIMERNOFG = 12;
        };

        // declaration for EnumWindows
        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsDelegate lpEnumFunc, GCHandle lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumChildWindows(IntPtr hWndParent, EnumWindowsDelegate lpEnumFunc, GCHandle lParam);

        // Gets the text length from a text box
        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern int GetTextLength(IntPtr hWnd, int Msg, ref GETTEXTLENGTHEX wParam, StringBuilder lParam);

        public struct GETTEXTLENGTHEX
        {
            public int flags;
            public int codepage;
        }

        public const int EM_GETTEXTLENGTHEX = 0x0400 + 95;

        public const int GTL_DEFAULT = 0;	// Do default (return # of chars)
        public const int GTL_USECRLF = 1;	// Compute answer using CRLFs for paragraphs
        public const int GTL_PRECISE = 2;	// Compute a precise answer
        public const int GTL_CLOSE = 4;	// Fast computation of a "close" answer
        public const int GTL_NUMCHARS = 8;	// Return number of characters
        public const int GTL_NUMBYTES = 16;	// Return number of _bytes_

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int DrawTextEx(IntPtr hdc, StringBuilder lpchText, int cchText,
            ref RECT lprc, DT dwDTFormat, ref DRAWTEXTPARAMS lpDTParams);

        [StructLayout(LayoutKind.Sequential)]
        public struct DRAWTEXTPARAMS
        {
            public uint cbSize;
            public int iTabLength;
            public int iLeftMargin;
            public int iRightMargin;
            public uint uiLengthDrawn;
        }

        public enum DT : uint
        {
            TOP = 0x00000000,
            LEFT = 0x00000000,
            CENTER = 0x00000001,
            RIGHT = 0x00000002,
            VCENTER = 0x00000004,
            BOTTOM = 0x00000008,
            WORDBREAK = 0x00000010,
            SINGLELINE = 0x00000020,
            EXPANDTABS = 0x00000040,
            TABSTOP = 0x00000080,
            NOCLIP = 0x00000100,
            EXTERNALLEADING = 0x00000200,
            CALCRECT = 0x00000400,
            NOPREFIX = 0x00000800,
            INTERNAL = 0x00001000,
            EDITCONTROL = 0x00002000,
            PATH_ELLIPSIS = 0x00004000,
            END_ELLIPSIS = 0x00008000,
            MODIFYSTRING = 0x00010000,
            RTLREADING = 0x00020000,
            WORD_ELLIPSIS = 0x00040000,
            NOFULLWIDTHCHARBREAK = 0x00080000,
            HIDEPREFIX = 0x00100000,
            PREFIXONLY = 0x00200000
        }

        /////////////////////////////////////////////////////////////////////////////
        /// Active Accessibility API -- Available in Win98, Win2K, and WinXP
        /// (available as a redistributable component for Win95 and NT4/SP6)
        ///

        /// <summary>
        /// Application-defined callback (or hook) function that the system calls
        /// in response to events generated by an accessible object.
        /// </summary>
        public delegate void WinEventProc(IntPtr hWinEventHook,
            EVENT_SYSTEM evt, IntPtr hwnd,
            int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime);

        /// <summary>
        /// Sets an event hook function for a range of events.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
            EVENT_SYSTEM eventMin, EVENT_SYSTEM eventMax,
            IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc,
            uint idProcess, uint idThread,
            WINEVENT dwFlags);

        /// <summary>
        /// Removes an event hook function created by a call to SetWinEventHook.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        /// <summary>
        /// Retrieves the time of the last input event.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool GetLastInputInfo(out LASTINPUTINFO plii);

        //common dialogs

        [DllImport("ComDlg32.dll", CharSet = CharSet.Unicode)]
        public static extern bool GetOpenFileName(ref OpenFileName ofn);

        [DllImport("ComDlg32.dll", CharSet = CharSet.Unicode)]
        public static extern int CommDlgExtendedError();

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr GetDlgItem(IntPtr hWndDlg, int Id);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr GetDlgCtrlID(IntPtr hwndCtl);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool AllowSetForegroundWindow(int procId);

        // We can overload this definition, since that's in effect what the unmanaged
        // API does anyway.
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int uAction,
            int uParam, ref NONCLIENTMETRICS lpvParam, int fuWinIni);

        [DllImport("user32.dll")]
        public static extern int DrawMenuBar(IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern int SetMenuInfo(IntPtr hmenu, ref MENUINFO mi);
    }

    public struct SM
    {
        public const int CXSIZEFRAME = 32;
        public const int CYSIZEFRAME = 33;
    }

    /// <summary>
    /// The MENUINFO structure contains information about a menu.
    /// </summary>
    public struct MENUINFO
    {
        public int cbSize;
        public int fMask;
        public int dwStyle;
        public int cyMax;
        public IntPtr hbrBack;
        public int dwContextHelpID;
        public int dwMenuData;
    }

    public struct MIM
    {
        public const int BACKGROUND = 0x2;
    }

    public struct SPI
    {
        public const int GETNONCLIENTMETRICS = 41;
    }

    // A "logical font" used by old-school windows
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct LOGFONT
    {
        public int lfHeight;
        public int lfWidth;
        public int lfEscapement;
        public int lfOrientation;
        public int lfWeight;
        public byte lfItalic;
        public byte lfUnderline;
        public byte lfStrikeOut;
        public byte lfCharSet;
        public byte lfOutPrecision;
        public byte lfClipPrecision;
        public byte lfQuality;
        public byte lfPitchAndFamily;

        /// <summary>
        /// <see cref="UnmanagedType.ByValTStr"/> means that the string
        /// should be marshalled as an array of TCHAR embedded in the
        /// structure.  This implies that the font names can be no larger
        /// than <see cref="LF_FACESIZE"/> including the terminating '\0'.
        /// That works out to 31 characters.
        /// </summary>
        private const int LF_FACESIZE = 32;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
        public string lfFaceName;

        // to shut it up about the warnings
        public LOGFONT(string lfFaceName)
        {
            this.lfFaceName = lfFaceName;
            lfHeight = lfWidth = lfEscapement = lfOrientation = lfWeight = 0;
            lfItalic = lfUnderline = lfStrikeOut = lfCharSet = lfOutPrecision
                = lfClipPrecision = lfQuality = lfPitchAndFamily = 0;
        }
    }

    public struct NONCLIENTMETRICS
    {
        public int cbSize;
        public int iBorderWidth;
        public int iScrollWidth;
        public int iScrollHeight;
        public int iCaptionWidth;
        public int iCaptionHeight;
        /// <summary>
        /// Since <see cref="LOGFONT"/> is a struct instead of a class,
        /// we don't have to do any special marshalling here.  Much
        /// simpler this way.
        /// </summary>
        public LOGFONT lfCaptionFont;
        public int iSMCaptionWidth;
        public int iSMCaptionHeight;
        public LOGFONT lfSMCaptionFont;
        public int iMenuWidth;
        public int iMenuHeight;
        public LOGFONT lfMenuFont;
        public LOGFONT lfStatusFont;
        public LOGFONT lfMessageFont;
    }

    /// <summary>
    /// Delegate to which messages for subclassed control will be redirected.
    /// redirected. The delegate should invoke CallBaseWindowProc when it
    /// wishes to forward a message on to the underlying window.
    /// </summary>
    public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    public struct WINDOWPLACEMENT
    {
        public uint length;
        public uint flags;
        public uint showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    };

    [Flags]
    public enum QS : uint
    {
        KEY = 0x0001,
        MOUSEMOVE = 0x0002,
        MOUSEBUTTON = 0x0004,
        POSTMESSAGE = 0x0008,
        TIMER = 0x0010,
        PAINT = 0x0020,
        SENDMESSAGE = 0x0040,
        HOTKEY = 0x0080,
        ALLPOSTMESSAGE = 0x0100,
        RAWINPUT = 0x0400,
        MOUSE = MOUSEMOVE | MOUSEBUTTON,
        INPUT = MOUSE | KEY | RAWINPUT,
        ALLEVENTS = INPUT | POSTMESSAGE | TIMER | PAINT | HOTKEY,
        ALLINPUT = INPUT | POSTMESSAGE | TIMER | PAINT | HOTKEY | SENDMESSAGE
    }

    public struct MF
    {
        public const uint BYCOMMAND = 0x00000000;
        public const uint BYPOSITION = 0x00000400;
        public const uint ENABLED = 0x00000000;
        public const uint DISABLED = 0x00000002;
    }

    public struct PM
    {
        public const uint NOREMOVE = 0x0000;
        public const uint REMOVE = 0x0001;
    }

    // cursor values
    public class IDC
    {
        public static readonly IntPtr APPSTARTING = new IntPtr(32650);
        public static readonly IntPtr ARROW = new IntPtr(32512);
    }

    public struct OCR
    {
        public const uint NORMAL = 32512;
    }

    /// <summary>
    /// A structure representing a windows message
    /// </summary>
    public struct MSG
    {
        /// <summary>
        /// The handle of the window receiving the message
        /// </summary>
        public IntPtr hwnd;

        /// <summary>
        /// The message number
        /// </summary>
        public uint message;

        /// <summary>
        /// First message parameter
        /// </summary>
        public uint wParam;

        /// <summary>
        /// Second message parameter
        /// </summary>
        public int lParam;

        /// <summary>
        /// The time the message was posted
        /// </summary>
        public uint time;

        /// <summary>
        /// A POINT structure containing the cursor position in screen coordinates
        /// at the time the message was posted
        /// </summary>
        public POINT pt;
    }

    /// <summary>
    /// POINT used in MSG structure
    /// </summary>
    public struct POINT
    {
        /// <summary>
        /// x-coordinate
        /// </summary>
        public int x;

        /// <summary>
        /// y-coordinate
        /// </summary>
        public int y;
    }

    /// <summary>
    /// Windows RECT structure
    /// </summary>
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public static implicit operator Rectangle(RECT rect)
        {
            return Rectangle.FromLTRB(rect.left, rect.top, rect.right, rect.bottom);
        }

        public static implicit operator RECT(Rectangle rectangle)
        {
            RECT rect = new RECT();
            rect.left = rectangle.Left;
            rect.top = rectangle.Top;
            rect.right = rectangle.Right;
            rect.bottom = rectangle.Bottom;
            return rect;
        }

        public int Width { get { return right - left; } }
        public int Height { get { return bottom - top; } }

    }

    /// <summary>
    /// Windows SIZE structure
    /// </summary>
    public struct SIZE
    {
        /// <summary>
        /// Width
        /// </summary>
        public int cx;

        /// <summary>
        /// Height
        /// </summary>
        public int cy;
    }

    /// <summary>
    /// Structure used with COPYDATASTRUCT
    /// </summary>
    public struct COPYDATASTRUCT
    {
        public uint dwData;
        public uint cbData;
        public IntPtr lpData;
    }

    // delegate signature for EnumWindows callback
    public delegate bool EnumWindowsDelegate(IntPtr hwnd, GCHandle lParam);

    /// <summary>
    /// Constants for window styles
    /// </summary>
    public struct WS
    {
        public const uint CHILD = 0x40000000;
        public const uint VISIBLE = 0x10000000;
        public const uint CLIPCHILDREN = 0x02000000;
        public const uint CLIPSIBLINGS = 0x04000000;
        public const uint HSCROLL = 0x00100000;
        public const uint POPUP = 0x80000000;
        public const uint TABSTOP = 0x00010000;
        public const uint VSCROLL = 0x00200000;
        public const uint EX_TOOLWINDOW = 0x00000080;
        public const uint EX_APPWINDOW = 0x00040000;
        public const uint EX_TOPMOST = 0x00000008;

        public const uint BORDER = 0x00800000;
        public const uint THICKFRAME = 0x00040000;
        public const uint SYSMENU = 0x00080000;
        public const uint MINIMIZEBOX = 0x00020000;
        public const uint MAXIMIZEBOX = 0x00010000;
    }

    public struct MOD
    {
        public const uint ALT = 0x0001;
        public const uint CONTROL = 0x0002;
        public const uint SHIFT = 0x0004;
        public const uint WIN = 0x0008;
    }

    /// <summary>
    /// Constants for ShowWindow
    /// </summary>
    public struct SW
    {
        public const int HIDE = 0;
        public const int SHOWNORMAL = 1;
        public const int NORMAL = 1;
        public const int SHOWMINIMIZED = 2;
        public const int SHOWMAXIMIZED = 3;
        public const int MAXIMIZE = 3;
        public const int SHOWNOACTIVATE = 4;
        public const int SHOW = 5;
        public const int MINIMIZE = 6;
        public const int SHOWMINNOACTIVE = 7;
        public const int SHOWNA = 8;
        public const int RESTORE = 9;
        public const int SHOWDEFAULT = 10;
        public const int FORCEMINIMIZE = 11;
        public const int MAX = 11;
    }

    /// <summary>
    /// MessageBox return codes
    /// </summary>
    public struct MSGBOX_ID
    {
        public const int OK = 1;
        public const int CANCEL = 2;
        public const int ABORT = 3;
        public const int RETRY = 4;
        public const int IGNORE = 5;
        public const int YES = 6;
        public const int NO = 7;
        public const int CLOSE = 8;
        public const int HELP = 9;
    }

    public struct WPF
    {
        public const int RESTORETOMAXIMIZED = 0x0002;
    }

    [Flags]
    public enum AW : uint
    {
        HOR_POSITIVE = 0x00000001,
        HOR_NEGATIVE = 0x00000002,
        VER_POSITIVE = 0x00000004,
        VER_NEGATIVE = 0x00000008,
        CENTER = 0x00000010,
        HIDE = 0x00010000,
        ACTIVATE = 0x00020000,
        SLIDE = 0x00040000,
        BLEND = 0x00080000
    }

    public enum GA
    {
        PARENT = 1,
        ROOT = 2,
        ROOTOWNER = 3
    }

    /// <summary>
    /// Constants for SetWindowPos
    /// </summary>
    public struct SWP
    {
        public const uint NOSIZE = 0x0001;
        public const uint NOMOVE = 0x0002;
        public const uint NOZORDER = 0x0004;

        public const uint SHOWWINDOW = 0x0040;
        public const uint NOACTIVATE = 0x0010;
        public const uint HIDEWINDOW = 0x0080;

        public const uint NOOWNERZORDER = 0x0200;
    }

    /// <summary>
    /// TrackPopupMenu flags
    /// </summary>
    public struct TPM
    {
        public const uint LEFTBUTTON = 0x0000;
        public const uint RIGHTBUTTON = 0x0002;
        public const uint LEFTALIGN = 0x0000;
        public const uint CENTERALIGN = 0x0004;
        public const uint RIGHTALIGN = 0x0008;
        public const uint NONOTIFY = 0x0080;
        public const uint RETURNCMD = 0x0100;
        public const uint VERNEGANIMATION = 0x2000;
        public const uint LAYOUTRTL = 0x8000;
    }


    /// <summary>
    /// Window field offsets for GetWindowLong().
    /// </summary>
    public struct GWL
    {
        public const int WNDPROC = -4;
        public const int HINSTANCE = -6;
        public const int HWNDPARENT = -8;
        public const int STYLE = -16;
        public const int EXSTYLE = -20;
        public const int USERDATA = -21;
        public const int ID = -12;
    }

    /// <summary>
    /// GetDCEx() flags.
    /// </summary>
    public struct DCX
    {
        public const uint WINDOW = 0x00000001;
        public const uint CACHE = 0x00000002;
        public const uint NORESETATTRS = 0x00000004;
        public const uint CLIPCHILDREN = 0x00000008;
        public const uint CLIPSIBLINGS = 0x00000010;
        public const uint PARENTCLIP = 0x00000020;
        public const uint EXCLUDERGN = 0x00000040;
        public const uint INTERSECTRGN = 0x00000080;
        public const uint EXCLUDEUPDATE = 0x00000100;
        public const uint INTERSECTUPDATE = 0x00000200;
        public const uint LOCKWINDOWUPDATE = 0x00000400;
        public const uint VALIDATE = 0x00200000;
    }

    /// <summary>
    /// Constants for button messages
    /// </summary>
    public struct BM
    {
        public const uint SETSTYLE = 0x000000F4;
    }

    public struct ButtonStyles
    {
        public const long BS_PUSHBUTTON = 0x00000000L;
        public const long BS_DEFPUSHBUTTON = 0x00000001L;
    }

    /// <summary>
    /// Constants for combo box notifications
    /// </summary>
    public struct CBN
    {
        public const int DROPDOWN = 7;
    }

    /// <summary>
    /// Constants for window messages
    /// </summary>
    public struct WM
    {
        public const uint COMMAND = 0x0111;
        public const uint DESTROY = 0x0002;
        public const uint SIZE = 0x0005;
        public const uint CLOSE = 0x0010;

        public const uint QUERYENDSESSION = 0x0011;
        public const uint ENDSESSION = 0x0016;

        public const uint VSCROLL = 0x0115;
        public const uint SETCURSOR = 0x0020;
        public const uint MOUSEACTIVATE = 0x0021;
        public const uint NOTIFY = 0x004E;
        public const uint INITDIALOG = 0x0110;
        public const uint GETTEXT = 0x000D;
        public const uint GETTEXTLENGTH = 0x000E;
        public const uint USER = 0x0400;
        public const uint NULL = 0x0000;
        public const uint GETMINMAXINFO = 0x0024;
        public const uint HOTKEY = 0x0312;
        public const uint SETFOCUS = 0x0007;
        public const uint KEYDOWN = 0x100;
        public const uint KEYUP = 0x0101;
        public const uint CHAR = 0x0102;
        public const uint SYSKEYDOWN = 0x104;
        public const uint SYSKEYUP = 0x105;
        public const uint SYSCHAR = 0x106;
        public const uint NCDESTROY = 0x0082;
        public const uint NCPAINT = 0x0085;
        public const uint NCACTIVATE = 0x0086;
        public const uint ERASEBKGND = 0x0014;
        public const uint NCHITTEST = 0x0084;
        public const uint SYSCOMMAND = 0x0112;
        public const uint MOUSEMOVE = 0x0200;
        public const uint LBUTTONDOWN = 0x0201;
        public const uint LBUTTONUP = 0x0202;
        public const uint LBUTTONDBLCLK = 0x0203;
        public const uint RBUTTONDOWN = 0x0204;
        public const uint RBUTTONUP = 0x0205;
        public const uint CHANGEUISTATE = 0x0127;
        public const uint UPDATEUISTATE = 0x0128;
        public const uint ENTERMENULOOP = 0x0211;
        public const uint EXITMENULOOP = 0x0212;
        public const uint CUT = 0x0300;
        public const uint COPY = 0x0301;
        public const uint PASTE = 0x0302;
        public const uint CLEAR = 0x0303;
        public const uint ACTIVATE = 0x0006;
        public const uint COPYDATA = 0x004A;
        public const uint CONTEXTMENU = 0x007B;
        public const uint WININICHANGE = 0x001A;
        public const uint ENTERSIZEMOVE = 0x0231;
        public const uint EXITSIZEMOVE = 0x0232;
        public const uint NCMOUSEMOVE = 0x00A0;
        public const uint NCLBUTTONDOWN = 0x00A1;
        public const uint NCLBUTTONUP = 0x00A2;
        public const uint NCLBUTTONDBLCLK = 0x00A3;
        public const uint NCRBUTTONDOWN = 0x00A4;
        public const uint NCRBUTTONUP = 0x00A5;
        public const uint NCRBUTTONDBLCLK = 0x00A6;
        public const uint NCMBUTTONDOWN = 0x00A7;
        public const uint NCMBUTTONUP = 0x00A8;
        public const uint NCMBUTTONDBLCLK = 0x00A9;
        public const uint NCMOUSEHOVER = 0x02A0;
        public const uint NCMOUSELEAVE = 0x02A2;
        public const uint DM_SETDEFID = WM.USER + 1;
        public const uint EM_REPLACESEL = 0x00C2;
        public const uint EM_POSFROMCHAR = 0x00D6;
        public const uint EM_SETMARGINS = 0x00D3;
        public const uint EM_GETMARGINS = 0x00D4;

        public const uint REFLECT = 0x2000;
    }

    public struct EC
    {
        public const uint RIGHTMARGIN = 2;
        public const uint LEFTMARGIN = 1;
    }

    public struct UIS
    {
        public const int SET = 1;
        public const int CLEAR = 2;
        public const int INITIALIZE = 3;
    }

    public struct UISF
    {
        public const int HIDEFOCUS = 0x1;
        public const int HIDEACCEL = 0x2;
        public const int ACTIVE = 0x4;
    }

    /// <summary>
    /// Constants for return value from NCHITTEST
    /// </summary>
    public struct HT
    {
        public const int ERROR = (-2);
        public const int TRANSPARENT = (-1);
        public const int NOWHERE = 0;
        public const int CLIENT = 1;
        public const int CAPTION = 2;
        public const int SYSMENU = 3;
        public const int GROWBOX = 4;
        public const int SIZE = GROWBOX;
        public const int MENU = 5;
        public const int HSCROLL = 6;
        public const int VSCROLL = 7;
        public const int MINBUTTON = 8;
        public const int MAXBUTTON = 9;
        public const int LEFT = 10;
        public const int RIGHT = 11;
        public const int TOP = 12;
        public const int TOPLEFT = 13;
        public const int TOPRIGHT = 14;
        public const int BOTTOM = 15;
        public const int BOTTOMLEFT = 16;
        public const int BOTTOMRIGHT = 17;
        public const int BORDER = 18;
        public const int REDUCE = MINBUTTON;
        public const int ZOOM = MAXBUTTON;
        public const int SIZEFIRST = LEFT;
        public const int SIZELAST = BOTTOMRIGHT;
        public const int OBJECT = 19;
        public const int CLOSE = 20;
        public const int HELP = 21;
    }

    /// <summary>
    /// Constants for WM_SYSCOMMAND
    /// </summary>
    public struct SC
    {
        public static readonly UIntPtr SIZE = new UIntPtr(0xF000);
        public static readonly UIntPtr MOVE = new UIntPtr(0xF010);
        public static readonly UIntPtr MINIMIZE = new UIntPtr(0xF020);
        public static readonly UIntPtr MAXIMIZE = new UIntPtr(0xF030);
        public static readonly UIntPtr NEXTWINDOW = new UIntPtr(0xF040);
        public static readonly UIntPtr PREVWINDOW = new UIntPtr(0xF050);
        public static readonly UIntPtr CLOSE = new UIntPtr(0xF060);
        public static readonly UIntPtr VSCROLL = new UIntPtr(0xF070);
        public static readonly UIntPtr HSCROLL = new UIntPtr(0xF080);
        public static readonly UIntPtr MOUSEMENU = new UIntPtr(0xF090);
        public static readonly UIntPtr KEYMENU = new UIntPtr(0xF100);
        public static readonly UIntPtr ARRANGE = new UIntPtr(0xF110);
        public static readonly UIntPtr RESTORE = new UIntPtr(0xF120);
        public static readonly UIntPtr TASKLIST = new UIntPtr(0xF130);
        public static readonly UIntPtr SCREENSAVE = new UIntPtr(0xF140);
        public static readonly UIntPtr HOTKEY = new UIntPtr(0xF150);
        public static readonly UIntPtr DEFAULT = new UIntPtr(0xF160);
        public static readonly UIntPtr MONITORPOWER = new UIntPtr(0xF170);
        public static readonly UIntPtr CONTEXTHELP = new UIntPtr(0xF180);
        public static readonly UIntPtr SEPARATOR = new UIntPtr(0xF00F);
    }

    /// <summary>
    /// Constants for virtual key codes
    /// </summary>
    public struct VK
    {
        public const int RETURN = 0x0D;
        public const int BACK = 0x08;
        public const int TAB = 0x09;
        public const int SHIFT = 0x10;
        public const int CONTROL = 0x11;
        public const int MENU = 0x12;
        public const int LMENU = 0xA4;
        public const int RMENU = 0xA5;
        public const int END = 0x23;
        public const int HOME = 0x24;
        public const int LEFT = 0x25;
        public const int UP = 0x26;
        public const int RIGHT = 0x27;
        public const int DOWN = 0x28;
        public const int INSERT = 0x2D;
        public const int DELETE = 0x2E;
    }

    /// <summary>
    /// Constants used for testing values returned from GetKeyState
    /// </summary>
    public struct VK_STATE
    {
        public const short PRESSED = 0xF0;
    }

    /// <summary>
    /// Modifiers for mouse-events
    /// </summary>
    [Flags]
    public enum MK : uint
    {
        LBUTTON = 0x0001,
        RBUTTON = 0x0002,
        SHIFT = 0x0004,
        CONTROL = 0x0008,
        MBUTTON = 0x0010,
        XBUTTON1 = 0x0020,
        XBUTTON2 = 0x0040
    }

    /// <summary>
    /// Enumeration for windows hook types
    /// </summary>
    public enum WH : int
    {
        MSGFILTER = -1,
        JOURNALRECORD = 0,
        JOURNALPLAYBACK = 1,
        KEYBOARD = 2,
        GETMESSAGE = 3,
        CALLWNDPROC = 4,
        CBT = 5,
        SYSMSGFILTER = 6,
        MOUSE = 7,
        HARDWARE = 8,
        DEBUG = 9,
        SHELL = 10,
        FOREGROUNDIDLE = 11,
        CALLWNDPROCRET = 12,
        KEYBOARD_LL = 13,
        MOUSE_LL = 14
    }

    public struct HWND
    {
        public static readonly IntPtr DESKTOP = new IntPtr(0);
        public static readonly IntPtr TOP = new IntPtr(0);
        public static readonly IntPtr BOTTOM = new IntPtr(1);
        public static readonly IntPtr TOPMOST = new IntPtr(-1);
        public static readonly IntPtr NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr MESSAGE = new IntPtr(-3);
    }

    /// <summary>
    /// Hook codes passed to HookDelegate
    /// </summary>
    public struct HC
    {
        public const int ACTION = 0;
        public const int GETNEXT = 1;
        public const int SKIP = 2;
        public const int NOREMOVE = 3;
        public const int SYSMODALON = 4;
        public const int SYSMODALOFF = 5;
    }

    public struct ENDSESSION
    {
        public const uint ENDSESSION_CLOSEAPP = 0x00000001;
        public const uint ENDSESSION_CRITICAL = 0x40000000;
        public const uint ENDSESSION_LOGOFF = 0x80000000;
    }

    /// <summary>
    /// Key flags used to extract extended key information from lParam
    /// </summary>
    public struct KF
    {
        public static readonly IntPtr EXTENDED = new IntPtr(0x0100);
        public static readonly IntPtr DLGMODE = new IntPtr(0x0800);
        public static readonly IntPtr MENUMODE = new IntPtr(0x1000);
        public static readonly IntPtr ALTDOWN = new IntPtr(0x2000);
        public static readonly IntPtr REPEAT = new IntPtr(0x4000);
        public static readonly IntPtr UP = new IntPtr(0x8000);
    }

    /// <summary>
    /// Active Accessibility event constants (note: only the constants we are
    /// currently using are defined -- there are many more available in Winuser.h)
    /// </summary>
    public enum EVENT_SYSTEM : uint
    {
        /*
         * EVENT_SYSTEM_CAPTURESTART
         * EVENT_SYSTEM_CAPTUREEND
         * Sent when a window takes the capture and releases the capture.
         */
        CAPTURESTART = 0x0008,
        CAPTUREEND = 0x0009,

        /*
        * Drag & Drop
        * EVENT_SYSTEM_DRAGDROPSTART
        * EVENT_SYSTEM_DRAGDROPEND
        * Send the START notification just before going into drag&drop loop.  Send
        * the END notification just after canceling out.
        * Note that it is up to apps and OLE to generate this, since the system
        * doesn't know.  Like EVENT_SYSTEM_SOUND, it will be a while before this
        * is prevalent.
        */
        DRAGDROPSTART = 0x000E,
        DRAGDROPEND = 0x000F
    }

    [Flags]
    public enum WINEVENT : uint
    {
        OUTOFCONTEXT = 0x0000,  // Events are ASYNC
        SKIPOWNTHREAD = 0x0001,  // Don't call back for events on installer's thread
        SKIPOWNPROCESS = 0x0002,  // Don't call back for events on installer's process
        INCONTEXT = 0x0004   // Events are SYNC (invalid in .NET because it requires
        // the client dll to be injected into every process)
    }

    public struct TTM
    {
        public const uint ADDTOOL = WM.USER + 50;
        public const uint TRACKACTIVATE = WM.USER + 17;
        public const uint TRACKPOSITION = WM.USER + 18;
    }

    public struct TTF
    {
        public const uint TRACK = 0x0020;
        public const uint ABSOLUTE = 0x0080;
    }

    public struct TTS
    {
        public const uint ALWAYSTIP = 0x01;
        public const uint NOPREFIX = 0x02;
    }

    public struct TOOLINFO
    {
        public uint cbSize;
        public uint uFlags;
        public IntPtr hwnd;
        public UIntPtr uId;
        public RECT rect;
        public IntPtr hinst;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpszText;
        public IntPtr lParam;
    }

    public struct WINDOW_CLASS
    {
        public const string TOOLTIPS = "tooltips_class32";
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LASTINPUTINFO
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

        [MarshalAs(UnmanagedType.U4)]
        public int cbSize;
        [MarshalAs(UnmanagedType.U4)]
        public int dwTime;
    }

    //title bar structure with size info
    [StructLayout(LayoutKind.Sequential)]
    public struct TITLEBARINFO
    {
        public uint cbSize;
        public RECT rcTitleBar;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5 + 1)]
        public uint[] rgstate;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWINFO
    {
        public uint cbSize;
        public RECT rcWindow;
        public RECT rcClient;
        public uint dwStyle;
        public uint dwExStyle;
        public uint dwWindowStatus;
        public uint cxWindowBorders;
        public uint cyWindowBorders;
        public ushort atomWindowType;
        public ushort wCreatorVersion;
    }

    //for common dialogs
    /// <summary>
    /// See the documentation for OPENFILENAME
    /// </summary>
    public struct OpenFileName
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public IntPtr lpstrFilter;
        public IntPtr lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;
        public IntPtr lpstrFile;
        public int nMaxFile;
        public IntPtr lpstrFileTitle;
        public int nMaxFileTitle;
        public IntPtr lpstrInitialDir;
        public IntPtr lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        public IntPtr lpstrDefExt;
        public int lCustData;
        public OfnHookProc lpfnHook;
        public IntPtr lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int FlagsEx;
    };

    public delegate IntPtr OfnHookProc(IntPtr hWnd, uint msg, int wParam, int lParam);

    /// <summary>
    /// Values that can be placed in the OPENFILENAME structure, we don't use all of them
    /// </summary>
    public class OpenFileNameFlags
    {
        public const int ReadOnly = 0x00000001;
        public const int OverWritePrompt = 0x00000002;
        public const int HideReadOnly = 0x00000004;
        public const int NoChangeDir = 0x00000008;
        public const int ShowHelp = 0x00000010;
        public const int EnableHook = 0x00000020;
        public const int EnableTemplate = 0x00000040;
        public const int EnableTemplateHandle = 0x00000080;
        public const int NoValidate = 0x00000100;
        public const int AllowMultiSelect = 0x00000200;
        public const int ExtensionDifferent = 0x00000400;
        public const int PathMustExist = 0x00000800;
        public const int FileMustExist = 0x00001000;
        public const int CreatePrompt = 0x00002000;
        public const int ShareAware = 0x00004000;
        public const int NoReadOnlyReturn = 0x00008000;
        public const int NoTestFileCreate = 0x00010000;
        public const int NoNetworkButton = 0x00020000;
        public const int NoLongNames = 0x00040000;
        public const int Explorer = 0x00080000;
        public const int NoDereferenceLinks = 0x00100000;
        public const int LongNames = 0x00200000;
        public const int EnableIncludeNotify = 0x00400000;
        public const int EnableSizing = 0x00800000;
        public const int DontAddToRecent = 0x02000000;
        public const int ForceShowHidden = 0x10000000;
    };

    /// <summary>
    /// Values that can be placed in the FlagsEx field of the OPENFILENAME structure
    /// </summary>
    public class OpenFileNameFlagsEx
    {
        public const int NoPlacesBar = 0x00000001;
    };

    /// <summary>
    /// Win32 window style constants
    /// We use them to set up our child window
    /// </summary>
    internal class DlgStyle
    {
        public const int DsSetFont = 0x00000040;
        public const int Ds3dLook = 0x00000004;
        public const int DsControl = 0x00000400;
        public const int WsChild = 0x40000000;
        public const int WsClipSiblings = 0x04000000;
        public const int WsVisible = 0x10000000;
        public const int WsGroup = 0x00020000;
        public const int SsNotify = 0x00000100;
    };

    /// <summary>
    /// Win32 "extended" window style constants
    /// </summary>
    internal class ExStyle
    {
        public const int WsExNoParentNotify = 0x00000004;
        public const int WsExControlParent = 0x00010000;
    };

    /// <summary>
    /// An in-memory Win32 dialog template
    /// Note: this has a very specific structure with a single static "label" control
    /// See documentation for DLGTEMPLATE and DLGITEMTEMPLATE
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class DlgTemplate
    {
        // The dialog template - see documentation for DLGTEMPLATE
        public int style = DlgStyle.Ds3dLook | DlgStyle.DsControl | DlgStyle.WsChild | DlgStyle.WsClipSiblings | DlgStyle.SsNotify;
        public int extendedStyle = ExStyle.WsExControlParent;
        public short numItems = 1;
        public short x = 0;
        public short y = 0;
        public short cx = 0;
        public short cy = 0;
        public short reservedMenu = 0;
        public short reservedClass = 0;
        public short reservedTitle = 0;

        // Single dlg item, must be dword-aligned - see documentation for DLGITEMTEMPLATE
        public int itemStyle = DlgStyle.WsChild;
        public int itemExtendedStyle = ExStyle.WsExNoParentNotify;
        public short itemX = 0;
        public short itemY = 0;
        public short itemCx = 0;
        public short itemCy = 0;
        public short itemId = 0;
        public ushort itemClassHdr = 0xffff;	// we supply a constant to indicate the class of this control
        public short itemClass = 0x0082;	// static label control
        public short itemText = 0x0000;	// no text for this control
        public short itemData = 0x0000;	// no creation data for this control
    };

    /// <summary>
    /// The possible notification messages that can be generated by the OpenFileDialog
    /// We only look for CDN_SELCHANGE
    /// </summary>
    public class CommonDlgNotification
    {
        //this was original definition but it errors in corext due asmmeta storing it as negative value
        //private const UInt16 First =			unchecked((UInt16)((UInt16)0 - (UInt16)601));
        //this is the same value
        private const short First = -601;

        public const short InitDone = (First - 0x0000);
        public const short SelChange = (First - 0x0001);
        public const short FolderChange = (First - 0x0002);
        public const short ShareViolation = (First - 0x0003);
        public const short Help = (First - 0x0004);
        public const short FileOk = (First - 0x0005);
        public const short TypeChange = (First - 0x0006);
        public const short IncludeItem = (First - 0x0007);
    }

    /// <summary>
    /// Messages that can be send to the common dialogs
    /// We only use CDM_GETFILEPATH
    /// </summary>
    public class CommonDlgMessage
    {
        private const ushort User = 0x0400;
        private const ushort First = User + 100;

        public const ushort GetFilePath = First + 0x0001;
        public const ushort GetFolderPath = First + 0x0002;
    };

    /// <summary>
    /// Part of the notification messages sent by the common dialogs
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct NMHDR
    {
        [FieldOffset(0)]
        public IntPtr hWndFrom;
        [FieldOffset(4)]
        public ushort idFrom;
        [FieldOffset(8)]
        public ushort code;
    };

    /// <summary>
    /// Part of the notification messages sent by the common dialogs
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct OfNotify
    {
        [FieldOffset(0)]
        public NMHDR hdr;
        [FieldOffset(12)]
        public IntPtr ipOfn;
        [FieldOffset(16)]
        public IntPtr ipFile;
    };

}
