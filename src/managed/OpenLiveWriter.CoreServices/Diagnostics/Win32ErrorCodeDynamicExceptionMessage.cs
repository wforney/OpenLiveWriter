// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    using System;
    using System.ComponentModel;

    public class Win32ErrorCodeDynamicExceptionMessage : ErrorCodeDynamicExceptionMessage
    {
        public Win32ErrorCodeDynamicExceptionMessage(int errorCode, ExceptionMessage exceptionMessage) : base(typeof(Win32Exception), errorCode, exceptionMessage)
        {
        }

        protected override int GetErrorCode(Exception e)
        {
            return ((Win32Exception)e).ErrorCode;
        }
    }

}
