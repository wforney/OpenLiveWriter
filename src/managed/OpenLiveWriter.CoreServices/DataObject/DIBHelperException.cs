// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Exception thrown by the DIBHelper
    /// </summary>
    public class DIBHelperException : Exception
    {
        public DIBHelperException() : base()
        {
        }

        public DIBHelperException(string message) : base(message)
        {
        }

        public DIBHelperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
