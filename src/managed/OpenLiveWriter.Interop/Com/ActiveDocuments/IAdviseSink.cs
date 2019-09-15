// <copyright file="IAdviseSink.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Interop.Com.ActiveDocuments
{
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    /// <summary>
    /// The IAdviseSink interface enables containers and other objects to receive
    /// notifications of data changes, view changes, and compound-document changes
    /// occurring in objects of interest. Container applications, for example,
    /// require such notifications to keep cached presentations of their linked
    /// and embedded objects up-to-date. Calls to IAdviseSink methods are
    /// asynchronous, so the call is sent and then the next instruction is
    /// executed without waiting for the call's return.
    /// </summary>
    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010f-0000-0000-C000-000000000046")]
    public interface IAdviseSink
    {
        /// <summary>
        /// Called when [data change].
        /// </summary>
        /// <param name="pFormatetc">The p formatetc.</param>
        /// <param name="pStgmed">The p stgmed.</param>
        [PreserveSig]
        void OnDataChange(
            [In] ref FORMATETC pFormatetc,
            [In] ref STGMEDIUM pStgmed);

        /// <summary>
        /// Called when [view change].
        /// </summary>
        /// <param name="dwAspect">The dw aspect.</param>
        /// <param name="lindex">The lindex.</param>
        [PreserveSig]
        void OnViewChange(
            [In] DVASPECT dwAspect,
            [In] int lindex);

        /// <summary>
        /// Called when [rename].
        /// </summary>
        /// <param name="pmk">The PMK.</param>
        [PreserveSig]
        void OnRename(
            [In] IMoniker pmk);

        /// <summary>
        /// Called when [save].
        /// </summary>
        [PreserveSig]
        void OnSave();

        /// <summary>
        /// Raises the Close event.
        /// </summary>
        [PreserveSig]
        void OnClose();
    }
}
