// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    using System;

    public abstract class ErrorCodeDynamicExceptionMessage : UnwindingDynamicExceptionMessage
    {
        private readonly Type exceptionType;
        private readonly int errorCode;
        private readonly ExceptionMessage exceptionMessage;

        public ErrorCodeDynamicExceptionMessage(Type exceptionType, int errorCode, ExceptionMessage exceptionMessage) : base(true)
        {
            this.exceptionType = exceptionType;
            this.errorCode = errorCode;
            this.exceptionMessage = exceptionMessage;
        }

        protected abstract int GetErrorCode(Exception e);

        protected override bool AppliesToInternal(Exception e)
        {
            return (exceptionType.IsInstanceOfType(e) && errorCode == GetErrorCode(e));
        }

        public override ExceptionMessage GetMessage(Exception e)
        {
            return exceptionMessage;
        }
    }

}
