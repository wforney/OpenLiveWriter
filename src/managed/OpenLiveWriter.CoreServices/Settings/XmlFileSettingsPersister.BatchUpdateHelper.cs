// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{
    using System;
    using System.Threading;

    public partial class XmlFileSettingsPersister
    {
        /// <summary>
        /// The BatchUpdateHelper class.
        /// Implements the <see cref="IDisposable" />
        /// </summary>
        /// <seealso cref="IDisposable" />
        private class BatchUpdateHelper : IDisposable
        {
            /// <summary>
            /// The parent
            /// </summary>
            private readonly XmlFileSettingsPersister parent;

            /// <summary>
            /// The disposed
            /// </summary>
            private int disposed;

            /// <summary>
            /// Initializes a new instance of the <see cref="BatchUpdateHelper"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public BatchUpdateHelper(XmlFileSettingsPersister parent)
            {
                this.parent = parent;
                parent.BeginUpdate();
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (Interlocked.CompareExchange(ref this.disposed, 1, 0) == 0)
                {
                    this.parent.EndUpdate();
                }
            }
        }
    }
}
