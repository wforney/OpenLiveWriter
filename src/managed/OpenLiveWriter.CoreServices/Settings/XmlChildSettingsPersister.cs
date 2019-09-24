// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{
    using System;
    using System.Collections;

    /// <summary>
    /// The XmlChildSettingsPersister class.
    /// Implements the <see cref="OpenLiveWriter.CoreServices.Settings.XmlSettingsPersister" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.CoreServices.Settings.XmlSettingsPersister" />
    internal class XmlChildSettingsPersister : XmlSettingsPersister
    {
        /// <summary>
        /// The parent
        /// </summary>
        private readonly XmlSettingsPersister parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlChildSettingsPersister"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="values">The values.</param>
        /// <param name="subsettings">The subsettings.</param>
        public XmlChildSettingsPersister(XmlSettingsPersister parent, Hashtable values, Hashtable subsettings)
            : base(values, subsettings) =>
            this.parent = parent;

        /// <summary>
        /// Gets the synchronize root.
        /// </summary>
        /// <value>The synchronize root.</value>
        protected internal override object SyncRoot => this.parent.SyncRoot;

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <returns>IDisposable.</returns>
        public override IDisposable BatchUpdate() => this.parent.BatchUpdate();

        /// <summary>
        /// Persists this instance.
        /// </summary>
        internal override void Persist() => this.parent.Persist();
    }
}
