// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;

    public abstract partial class AtomClient
    {
        /// <summary>
        /// The DuplicateEntryIdException class.
        /// Implements the <see cref="System.Exception" />
        /// </summary>
        /// <seealso cref="System.Exception" />
        private class DuplicateEntryIdException : Exception
        {
        }
    }
}
