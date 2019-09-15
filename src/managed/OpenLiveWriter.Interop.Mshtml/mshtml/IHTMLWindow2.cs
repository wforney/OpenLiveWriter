// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace mshtml
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    /// <summary>
    /// The HTML window 2 interface.
    /// Implements the <see cref="mshtml.IHTMLFramesCollection2" />
    /// </summary>
    /// <seealso cref="mshtml.IHTMLFramesCollection2" />
    [ComImport]
    [DefaultMember("item")]
    [Guid("332C4427-26CB-11D0-B483-00C04FD90119")]
    [TypeLibType(0x1040)]
    public interface IHTMLWindow2 : IHTMLFramesCollection2
    {
        /// <summary>
        /// Items the specified pvar index.
        /// </summary>
        /// <param name="pvarIndex">Index of the pvar.</param>
        /// <returns>System.Object.</returns>
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0)]
        new object item([In, MarshalAs(UnmanagedType.Struct)] ref object pvarIndex);

        /// <summary>
        /// Gets the length.
        /// </summary>
        /// <value>The length.</value>
        [DispId(0x3e9)]
        new int length
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x3e9)]
            get;
        }

        /// <summary>
        /// Gets the frames.
        /// </summary>
        /// <value>The frames.</value>
        [DispId(0x44c)]
        FramesCollection frames
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x44c)]
            get;
        }

        /// <summary>
        /// Gets or sets the default status.
        /// </summary>
        /// <value>The default status.</value>
        [DispId(0x44d)]
        string defaultStatus
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x44d)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x44d)]
            set;
        }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>The status.</value>
        [DispId(0x44e)]
        string status
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x44e)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x44e)]
            set;
        }

        /// <summary>
        /// Sets the timeout.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="msec">The msec.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Int32.</returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x494)]
        int setTimeout([In, MarshalAs(UnmanagedType.BStr)] string expression, [In] int msec, [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object language);

        /// <summary>
        /// Clears the timeout.
        /// </summary>
        /// <param name="timerID">The timer identifier.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x450)]
        void clearTimeout([In] int timerID);

        /// <summary>
        /// Alerts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x451)]
        void alert([In, Optional, MarshalAs(UnmanagedType.BStr)] string message /* = "" */);

        /// <summary>
        /// Confirms the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x456)]
        bool confirm([In, Optional, MarshalAs(UnmanagedType.BStr)] string message /* = "" */);

        /// <summary>
        /// Prompts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="defstr">The defstr.</param>
        /// <returns>System.Object.</returns>
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x457)]
        object prompt([In, Optional, MarshalAs(UnmanagedType.BStr)] string message /* = "" */, [In, Optional, MarshalAs(UnmanagedType.BStr)] string defstr /* = "undefined" */);

        /// <summary>
        /// Gets the image.
        /// </summary>
        /// <value>The image.</value>
        [DispId(0x465)]
        HTMLImageElementFactory Image
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x465)]
            get;
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        [DispId(14)]
        HTMLLocation location
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(14)]
            get;
        }

        /// <summary>
        /// Gets the history.
        /// </summary>
        /// <value>The history.</value>
        [DispId(2)]
        HTMLHistory history
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(2)]
            get;
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(3)]
        void close();

        /// <summary>
        /// Gets or sets the opener.
        /// </summary>
        /// <value>The opener.</value>
        [DispId(4)]
        object opener
        {
            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(4)]
            set;
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(4)]
            get;
        }

        /// <summary>
        /// Gets the navigator.
        /// </summary>
        /// <value>The navigator.</value>
        [DispId(5)]
        HTMLNavigator navigator
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(5)]
            get;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        [DispId(11)]
        string name
        {
            [return: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(11)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.BStr)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(11)]
            set;
        }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        [DispId(12)]
        IHTMLWindow2 parent
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(12)]
            get;
        }

        /// <summary>
        /// Opens the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="name">The name.</param>
        /// <param name="features">The features.</param>
        /// <param name="replace">if set to <c>true</c> [replace].</param>
        /// <returns>IHTMLWindow2.</returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(13)]
        IHTMLWindow2 open(
            [In, Optional, MarshalAs(UnmanagedType.BStr)] string url /* = "" */,
            [In, Optional, MarshalAs(UnmanagedType.BStr)] string name /* = "" */,
            [In, Optional, MarshalAs(UnmanagedType.BStr)] string features /* = "" */,
            [In, Optional] bool replace /* = false */);

        /// <summary>
        /// Gets the self.
        /// </summary>
        /// <value>The self.</value>
        [DispId(20)]
        IHTMLWindow2 self
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(20)]
            get;
        }

        /// <summary>
        /// Gets the top.
        /// </summary>
        /// <value>The top.</value>
        [DispId(0x15)]
        IHTMLWindow2 top
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x15)]
            get;
        }

        /// <summary>
        /// Gets the window.
        /// </summary>
        /// <value>The window.</value>
        [DispId(0x16)]
        IHTMLWindow2 window
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x16)]
            get;
        }

        /// <summary>
        /// Navigates the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x19)]
        void navigate([In, MarshalAs(UnmanagedType.BStr)] string url);

        /// <summary>
        /// Gets or sets the onfocus.
        /// </summary>
        /// <value>The onfocus.</value>
        [DispId(-2147412098)]
        object onfocus
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412098)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412098)]
            [TypeLibFunc((short)20)]
            set;
        }

        /// <summary>
        /// Gets or sets the onblur.
        /// </summary>
        /// <value>The onblur.</value>
        [DispId(-2147412097)]
        object onblur
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412097)]
            [TypeLibFunc((short)20)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412097)]
            set;
        }

        /// <summary>
        /// Gets or sets the onload.
        /// </summary>
        /// <value>The onload.</value>
        [DispId(-2147412080)]
        object onload
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412080)]
            [TypeLibFunc((short)20)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412080)]
            set;
        }

        /// <summary>
        /// Gets or sets the onbeforeunload.
        /// </summary>
        /// <value>The onbeforeunload.</value>
        [DispId(-2147412073)]
        object onbeforeunload
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412073)]
            [TypeLibFunc((short)20)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412073)]
            [TypeLibFunc((short)20)]
            set;
        }

        /// <summary>
        /// Gets or sets the onunload.
        /// </summary>
        /// <value>The onunload.</value>
        [DispId(-2147412079)]
        object onunload
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412079)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412079)]
            set;
        }

        /// <summary>
        /// Gets or sets the onhelp.
        /// </summary>
        /// <value>The onhelp.</value>
        [DispId(-2147412099)]
        object onhelp
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412099)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412099)]
            [TypeLibFunc((short)20)]
            set;
        }

        /// <summary>
        /// Gets or sets the onerror.
        /// </summary>
        /// <value>The onerror.</value>
        [DispId(-2147412083)]
        object onerror
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412083)]
            [TypeLibFunc((short)20)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412083)]
            [TypeLibFunc((short)20)]
            set;
        }

        /// <summary>
        /// Gets or sets the onresize.
        /// </summary>
        /// <value>The onresize.</value>
        [DispId(-2147412076)]
        object onresize
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412076)]
            [TypeLibFunc((short)20)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412076)]
            [TypeLibFunc((short)20)]
            set;
        }

        /// <summary>
        /// Gets or sets the onscroll.
        /// </summary>
        /// <value>The onscroll.</value>
        [DispId(-2147412081)]
        object onscroll
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(-2147412081)]
            [TypeLibFunc((short)20)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)20)]
            [DispId(-2147412081)]
            set;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>The document.</value>
        [DispId(0x47f)]
        IHTMLDocument2 document
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)2)]
            [DispId(0x47f)]
            get;
        }

        /// <summary>
        /// Gets the event.
        /// </summary>
        /// <value>The event.</value>
        [DispId(0x480)]
        IHTMLEventObj @event
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x480)]
            get;
        }

        /// <summary>
        /// Gets the new enum.
        /// </summary>
        /// <value>The new enum.</value>
        [DispId(0x481)]
        object _newEnum
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [TypeLibFunc((short)0x41)]
            [DispId(0x481)]
            get;
        }

        /// <summary>
        /// Shows the modal dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="varArgIn">The variable argument in.</param>
        /// <param name="varOptions">The variable options.</param>
        /// <returns>System.Object.</returns>
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x482)]
        object showModalDialog(
            [In, MarshalAs(UnmanagedType.BStr)] string dialog,
            [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object varArgIn,
            [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object varOptions);

        /// <summary>
        /// Shows the help.
        /// </summary>
        /// <param name="helpURL">The help URL.</param>
        /// <param name="helpArg">The help argument.</param>
        /// <param name="features">The features.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x483)]
        void showHelp(
            [In, MarshalAs(UnmanagedType.BStr)] string helpURL,
            [In, Optional, MarshalAs(UnmanagedType.Struct)] object helpArg,
            [In, Optional, MarshalAs(UnmanagedType.BStr)] string features /* = "" */);

        /// <summary>
        /// Gets the screen.
        /// </summary>
        /// <value>The screen.</value>
        [DispId(0x484)]
        IHTMLScreen screen
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x484)]
            get;
        }

        /// <summary>
        /// Gets the option.
        /// </summary>
        /// <value>The option.</value>
        [DispId(0x485)]
        HTMLOptionElementFactory Option
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x485)]
            get;
        }

        /// <summary>
        /// Focuses this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x486)]
        void focus();

        /// <summary>
        /// Gets a value indicating whether this <see cref="IHTMLWindow2"/> is closed.
        /// </summary>
        /// <value><c>true</c> if closed; otherwise, <c>false</c>.</value>
        [DispId(0x17)]
        bool closed
        {
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x17)]
            get;
        }

        /// <summary>
        /// Blurs this instance.
        /// </summary>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x487)]
        void blur();

        /// <summary>
        /// Scrolls the specified x.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x488)]
        void scroll([In] int x, [In] int y);

        /// <summary>
        /// Gets the client information.
        /// </summary>
        /// <value>The client information.</value>
        [DispId(0x489)]
        HTMLNavigator clientInformation
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x489)]
            get;
        }

        /// <summary>
        /// Sets the interval.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="msec">The msec.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Int32.</returns>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x495)]
        int setInterval(
            [In, MarshalAs(UnmanagedType.BStr)] string expression,
            [In] int msec, [In, Optional, MarshalAs(UnmanagedType.Struct)] ref object language);

        /// <summary>
        /// Clears the interval.
        /// </summary>
        /// <param name="timerID">The timer identifier.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x48b)]
        void clearInterval([In] int timerID);

        /// <summary>
        /// Gets or sets the offscreen buffering.
        /// </summary>
        /// <value>The offscreen buffering.</value>
        [DispId(0x48c)]
        object offscreenBuffering
        {
            [return: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x48c)]
            get;

            [param: In]
            [param: MarshalAs(UnmanagedType.Struct)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x48c)]
            set;
        }

        /// <summary>
        /// Executes the script.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="language">The language.</param>
        /// <returns>System.Object.</returns>
        [return: MarshalAs(UnmanagedType.Struct)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x48d)]
        object execScript(
            [In, MarshalAs(UnmanagedType.BStr)] string code,
            [In, Optional, MarshalAs(UnmanagedType.BStr)] string language /* = "JScript" */);

        /// <summary>
        /// To the string.
        /// </summary>
        /// <returns>System.String.</returns>
        [return: MarshalAs(UnmanagedType.BStr)]
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x48e)]
        string toString();

        /// <summary>
        /// Scrolls the by.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x48f)]
        void scrollBy([In] int x, [In] int y);

        /// <summary>
        /// Scrolls to.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(0x490)]
        void scrollTo([In] int x, [In] int y);

        /// <summary>
        /// Moves to.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(6)]
        void moveTo([In] int x, [In] int y);

        /// <summary>
        /// Moves the by.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(7)]
        void moveBy([In] int x, [In] int y);

        /// <summary>
        /// Resizes to.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(9)]
        void resizeTo([In] int x, [In] int y);

        /// <summary>
        /// Resizes the by.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        [DispId(8)]
        void resizeBy([In] int x, [In] int y);

        /// <summary>
        /// Gets the external.
        /// </summary>
        /// <value>The external.</value>
        [DispId(0x491)]
        object external
        {
            [return: MarshalAs(UnmanagedType.IDispatch)]
            [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
            [DispId(0x491)]
            get;
        }
    }
}
