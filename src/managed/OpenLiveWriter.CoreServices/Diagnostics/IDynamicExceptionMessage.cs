// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Diagnostics
{
    using System;

    public interface IDynamicExceptionMessage
    {
        bool AppliesTo(Type exceptionType, Exception e);
        ExceptionMessage GetMessage(Exception e);
    }
}
