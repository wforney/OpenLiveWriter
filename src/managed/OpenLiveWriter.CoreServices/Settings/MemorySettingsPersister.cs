// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;
using System.Collections.Generic;

namespace OpenLiveWriter.CoreServices.Settings
{
    /// <summary>
    /// Keeps settings in memory (i.e. doesn't actually persist them)
    /// </summary>
    public class MemorySettingsPersister : ISettingsPersister
    {
        private object defaultVal;
        private readonly Hashtable data;
        private readonly DefaultHashtable children;

        public MemorySettingsPersister()
        {
            this.defaultVal = null;
            this.data = new Hashtable();
            this.children = new DefaultHashtable(new DefaultHashtable.DefaultValuePump(this.MemorySettingsPersisterPump));
        }

        private object MemorySettingsPersisterPump(object key) => new MemorySettingsPersister();

        public ICollection<string> GetNames() => this.CollectionToSortedStringArray(this.data.Keys);

        public T Get<T>(string name, T defaultValue)
        {
            if (name == null)
            {
                if (this.defaultVal == null)
                {
                    this.defaultVal = defaultValue;
                }

                return (T)this.defaultVal;
            }

            var val = this.data[name];
            if (val != null && typeof(T).IsAssignableFrom(val.GetType()))
            {
                return (T)val;
            }

            this.data[name] = defaultValue;
            return defaultValue;
        }

        public T Get<T>(string name) => name == null ? (T)this.defaultVal : (T)this.data[name];

        public void Set<T>(string name, T value)
        {
            if (name == null)
            {
                this.defaultVal = value;
            }
            else
            {
                this.data[name] = value;
            }
        }

        public void Unset(string name)
        {
            if (name == null)
            {
                this.defaultVal = null;
            }

            this.data.Remove(name);
        }

        public void UnsetSubSettingsTree(string name) => this.children.Remove(name);

        public IDisposable BatchUpdate() => null;

        public bool HasSubSettings(string subSettingsName) => this.children.ContainsKey(subSettingsName);

        public ISettingsPersister GetSubSettings(string subSettingsName) => (ISettingsPersister)this.children[subSettingsName];

        public ICollection<string> GetSubSettings() => this.CollectionToSortedStringArray(this.children.Keys);

        public void Dispose()
        {
        }

        private string[] CollectionToSortedStringArray(ICollection foo)
        {
            var names = new string[this.data.Count];
            var i = 0;
            foreach (string key in foo)
            {
                names[i++] = key;
            }
            Array.Sort(names);
            return (string[])ArrayHelper.Compact(names);
        }
    }
}
