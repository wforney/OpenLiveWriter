// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    using System;

    public abstract class UnwindingDynamicExceptionMessage : IDynamicExceptionMessage
    {
        private readonly bool unwind;

        public UnwindingDynamicExceptionMessage(bool unwind)
        {
            this.unwind = unwind;
        }

        protected abstract bool AppliesToInternal(Exception e);

        public bool AppliesTo(Type exceptionType, Exception e)
        {
            if (unwind)
                return Unwind(e) != null;
            else
                return AppliesToInternal(e);
        }

        public abstract ExceptionMessage GetMessage(Exception e);

        protected Exception Unwind(Exception e)
        {
            while (e != null)
            {
                if (AppliesToInternal(e))
                    return e;
                e = e.InnerException;
            }
            return null;
        }
    }

}
