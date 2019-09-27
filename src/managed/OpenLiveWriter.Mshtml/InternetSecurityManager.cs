// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using OpenLiveWriter.Mshtml.Mshtml_Interop;

namespace OpenLiveWriter.Mshtml
{
    /// <summary>
    /// Base InternetSecurityManager implementation.  All operations delegate to the default
    /// security manager. Subclass can override methods to customize the security of the browser.
    /// </summary>
    public class InternetSecurityManager : IInternetSecurityManager
    {
        #region IInternetSecurityManager Members

        private IntPtr securitySite;
        public virtual int SetSecuritySite(IntPtr pSite)
        {
            securitySite = pSite;
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int GetSecuritySite(out IntPtr pSite)
        {
            pSite = securitySite;
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int MapUrlToZone(string pwszUrl, ref int pdwZone, int dwFlags)
        {
            pdwZone = 0;
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int GetSecurityId(string pwszUrl, out byte pbSecurityId, ref int pcbSecurityId, IntPtr dwReserved)
        {
            pbSecurityId = new byte();
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int ProcessUrlAction(string pwszUrl, int dwAction, out byte pPolicy, int cbPolicy, IntPtr pContext, int cbContext, int dwFlags, int dwReserved)
        {
            pPolicy = new byte();
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int QueryCustomPolicy(string pwszUrl, ref Guid guidKey, out byte ppPolicy, out int pcbPolicy, byte pContext, int cbContext, int dwReserved)
        {
            ppPolicy = new byte();
            pcbPolicy = 0;
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int SetZoneMapping(int dwZone, string lpszPattern, int dwFlags)
        {
            return INET_E.DEFAULT_ACTION;
        }

        public virtual int GetZoneMappings(int dwZone, out IEnumString ppenumString, int dwFlags)
        {
            ppenumString = null;
            return INET_E.DEFAULT_ACTION;
        }

        #endregion
    }
}
