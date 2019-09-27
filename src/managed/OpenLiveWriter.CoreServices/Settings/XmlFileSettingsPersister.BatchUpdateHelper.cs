// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Threading;

namespace OpenLiveWriter.CoreServices.Settings
{

    public partial class XmlFileSettingsPersister
    {
        private class BatchUpdateHelper : IDisposable
        {
            private readonly XmlFileSettingsPersister parent;
            private int disposed = 0;

            public BatchUpdateHelper(XmlFileSettingsPersister parent)
            {
                this.parent = parent;
                parent.BeginUpdate();
            }

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
