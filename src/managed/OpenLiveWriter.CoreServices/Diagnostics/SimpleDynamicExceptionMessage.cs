// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    using System;

    public class SimpleDynamicExceptionMessage : UnwindingDynamicExceptionMessage
    {
        protected readonly Type exceptionType;
        private readonly ExceptionMessage exceptionMessage;
        protected readonly bool unwind;

        public SimpleDynamicExceptionMessage(Type exceptionType, ExceptionMessage exceptionMessage, bool recurseInnerExceptions) : base(recurseInnerExceptions)
        {
            this.exceptionType = exceptionType;
            this.exceptionMessage = exceptionMessage;
        }

        protected override bool AppliesToInternal(Exception e)
        {
            return this.exceptionType.IsInstanceOfType(e);
        }

        public override ExceptionMessage GetMessage(Exception e)
        {
            return exceptionMessage;
        }

    }

}
