// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    using System;
    using System.Runtime.InteropServices;

    public class COMErrorCodeDynamicExceptionMessage : ErrorCodeDynamicExceptionMessage
    {
        public COMErrorCodeDynamicExceptionMessage(int errorCode, ExceptionMessage exceptionMessage) : base(typeof(COMException), errorCode, exceptionMessage)
        {
        }

        protected override int GetErrorCode(Exception e)
        {
            return ((COMException)e).ErrorCode;
        }
    }

}
