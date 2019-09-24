// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The XmlSettingsPersister class.
    /// Implements the <see cref="ISettingsPersister" />
    /// </summary>
    /// <seealso cref="ISettingsPersister" />
    public abstract class XmlSettingsPersister : ISettingsPersister
    {
        /// <summary>
        /// The subsettings
        /// </summary>
        protected internal Hashtable Subsettings;

        /// <summary>
        /// The values
        /// </summary>
        protected internal Hashtable Values;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlSettingsPersister"/> class.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="subsettings">The subsettings.</param>
        protected XmlSettingsPersister(Hashtable values, Hashtable subsettings)
        {
            this.Values = values;
            this.Subsettings = subsettings;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="XmlSettingsPersister"/> class.
        /// </summary>
        ~XmlSettingsPersister()
        {
            this.Dispose(false);

            // Debug.Fail("Failed to dispose XmlSettingsPersister");
        }

        /// <summary>
        /// Gets the synchronize root.
        /// </summary>
        /// <value>The synchronize root.</value>
        protected internal abstract object SyncRoot { get; }

        /// <inheritdoc />
        public abstract IDisposable BatchUpdate();

        /// <inheritdoc />
        public virtual void Dispose() => this.Dispose(true);

        /// <inheritdoc />
        public object Get(string name, Type desiredType, object defaultValue)
        {
            lock (this.SyncRoot)
            {
                var o = this.Get(name);
                if (desiredType.IsInstanceOfType(o))
                {
                    return o;
                }

                if (defaultValue == null)
                {
                    return null;
                }

                this.Values[name] = defaultValue;
                this.Persist();
                return defaultValue;
            }
        }

        /// <inheritdoc />
        public object Get(string name)
        {
            lock (this.SyncRoot)
            {
                return this.Values[name];
            }
        }

        /// <inheritdoc />
        public string[] GetNames()
        {
            lock (this.SyncRoot)
            {
                var nameList = new List<string>(this.Values.Keys.Cast<string>());
                nameList.Sort();
                return nameList.ToArray();
            }
        }

        /// <inheritdoc />
        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            lock (this.SyncRoot)
            {
                if (!this.Subsettings.ContainsKey(subSettingsName))
                {
                    this.Subsettings[subSettingsName] = new[] { new Hashtable(), new Hashtable() };
                }

                var tables = (Hashtable[])this.Subsettings[subSettingsName];
                return new XmlChildSettingsPersister(this, tables[0], tables[1]);
            }
        }

        /// <inheritdoc />
        public string[] GetSubSettings()
        {
            lock (this.SyncRoot)
            {
                var keyList = new List<string>(this.Subsettings.Keys.Cast<string>());
                keyList.Sort();
                return keyList.ToArray();
            }
        }

        /// <inheritdoc />
        public bool HasSubSettings(string subSettingsName)
        {
            lock (this.SyncRoot)
            {
                return this.Subsettings.ContainsKey(subSettingsName);
            }
        }

        /// <inheritdoc />
        public void Set(string name, object value)
        {
            lock (this.SyncRoot)
            {
                this.Values[name] = value;
                this.Persist();
            }
        }

        /// <inheritdoc />
        public void Unset(string name)
        {
            lock (this.SyncRoot)
            {
                this.Values.Remove(name);
                this.Persist();
            }
        }

        /// <inheritdoc />
        public void UnsetSubSettingsTree(string name)
        {
            lock (this.SyncRoot)
            {
                this.Subsettings.Remove(name);
                this.Persist();
            }
        }

        /// <summary>
        /// Persists this instance.
        /// </summary>
        internal abstract void Persist();

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing) => GC.SuppressFinalize(this);
    }
}
