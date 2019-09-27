// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.CoreServices.Settings
{
    internal class XmlChildSettingsPersister : XmlSettingsPersister
    {
        private readonly XmlSettingsPersister parent;

        public XmlChildSettingsPersister(XmlSettingsPersister parent, Hashtable values, Hashtable subsettings)
            : base(values, subsettings) => this.parent = parent;

        protected internal override object SyncRoot => this.parent.SyncRoot;

        public override IDisposable BatchUpdate() => this.parent.BatchUpdate();

        internal override void Persist() => this.parent.Persist();
    }
}
