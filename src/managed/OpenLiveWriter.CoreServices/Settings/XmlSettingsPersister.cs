// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;

    public abstract class XmlSettingsPersister : ISettingsPersister
    {
        protected internal Hashtable values;
        protected internal Hashtable subsettings;

        protected XmlSettingsPersister(Hashtable values, Hashtable subsettings)
        {
            this.values = values;
            this.subsettings = subsettings;
        }

        internal abstract void Persist();
        public abstract IDisposable BatchUpdate();

        protected internal abstract object SyncRoot { get; }

        public ICollection<string> GetNames()
        {
            lock (this.SyncRoot)
            {
                var nameList = new List<string>(this.values.Keys.Cast<string>());
                nameList.Sort();
                return nameList;
            }
        }

        public T Get<T>(string name, T defaultValue)
        {
            lock (this.SyncRoot)
            {
                var o = this.Get<T>(name);
                if (typeof(T).IsInstanceOfType(o))
                {
                    return (T)o;
                }
                else if (defaultValue != null)
                {
                    this.values[name] = defaultValue;
                    this.Persist();
                    return defaultValue;
                }
                else
                {
                    return default;
                }
            }
        }

        public T Get<T>(string name)
        {
            lock (this.SyncRoot)
            {
                return (T)this.values[name];
            }
        }

        public void Set<T>(string name, T value)
        {
            lock (this.SyncRoot)
            {
                this.values[name] = value;
                this.Persist();
            }
        }

        public void Unset(string name)
        {
            lock (this.SyncRoot)
            {
                this.values.Remove(name);
                this.Persist();
            }
        }

        public void UnsetSubSettingsTree(string name)
        {
            lock (this.SyncRoot)
            {
                this.subsettings.Remove(name);
                this.Persist();
            }
        }

        public bool HasSubSettings(string subSettingsName)
        {
            lock (this.SyncRoot)
            {
                return this.subsettings.ContainsKey(subSettingsName);
            }
        }

        public ISettingsPersister GetSubSettings(string subSettingsName)
        {
            lock (this.SyncRoot)
            {
                if (!this.subsettings.ContainsKey(subSettingsName))
                {
                    this.subsettings[subSettingsName] = new Hashtable[] { new Hashtable(), new Hashtable() };
                }

                var tables = (Hashtable[])this.subsettings[subSettingsName];
                return new XmlChildSettingsPersister(this, tables[0], tables[1]);
            }
        }

        public ICollection<string> GetSubSettings()
        {
            lock (this.SyncRoot)
            {
                var keyList = new List<string>(this.subsettings.Keys.Cast<string>());
                keyList.Sort();
                return keyList;
            }
        }

        public virtual void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing) => GC.SuppressFinalize(this);

        ~XmlSettingsPersister()
        {
            this.Dispose(false);
            //Debug.Fail("Failed to dispose XmlSettingsPersister");
        }
    }

    public partial class XmlFileSettingsPersister : XmlSettingsPersister
    {
        private readonly Stream stream;
        private readonly object syncRoot = new object();
        private int batchUpdateRefCount = 0;

        private XmlFileSettingsPersister(Stream stream, Hashtable values, Hashtable subsettings)
            : base(values, subsettings) => this.stream = stream;

        protected internal override object SyncRoot => this.syncRoot;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stream.Dispose();
            }
            base.Dispose(false);

        }

        public static XmlFileSettingsPersister Open(string filename)
        {
            Stream s = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (s.Length == 0)
            {
                return new XmlFileSettingsPersister(s, new Hashtable(), new Hashtable());
            }
            else
            {
                Parse(s, out var values, out var subsettings);
                return new XmlFileSettingsPersister(s, values, subsettings);
            }
        }

        private static void Parse(Stream s, out Hashtable values, out Hashtable subsettings)
        {
            values = new Hashtable();
            subsettings = new Hashtable();

            var doc = new XmlDocument();
            try
            {
                doc.Load(s);
            }
            catch (Exception e)
            {
                // Deal with legacy corruption caused by bug 762601
                Trace.WriteLine(e.ToString());
                return;
            }
            var root = doc.DocumentElement;
            Parse(root, values, subsettings);
        }

        private static void Parse(XmlElement node, Hashtable values, Hashtable subsettings)
        {
            foreach (XmlElement valueNode in node.SelectNodes("value"))
            {
                var name = valueNode.GetAttribute("name");
                var type = valueNode.GetAttribute("type");
                var value = valueNode.InnerText;
                values[name] = ParseValue(type, value);
            }

            foreach (XmlElement settingsNode in node.SelectNodes("settings"))
            {
                var name = settingsNode.GetAttribute("name");
                var subvalues = new Hashtable();
                var subsubsettings = new Hashtable();
                Parse(settingsNode, subvalues, subsubsettings);
                subsettings[name] = new Hashtable[] { subvalues, subsubsettings };
            }
        }

        private static object ParseValue(string type, string value)
        {
            switch ((int)Enum.Parse(typeof(ValueType), type))
            {
                case (int)ValueType.Char:
                    return char.Parse(value);
                case (int)ValueType.String:
                    return value;
                case (int)ValueType.Bool:
                    return bool.Parse(value);
                case (int)ValueType.SByte:
                    return sbyte.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Byte:
                    return byte.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Int16:
                    return short.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.UInt16:
                    return ushort.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Int32:
                    return int.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.UInt32:
                    return uint.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Int64:
                    return long.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.UInt64:
                    return ulong.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Double:
                    return double.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Float:
                    return float.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Decimal:
                    return decimal.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.DateTime:
                    return DateTime.Parse(value, CultureInfo.InvariantCulture);
                case (int)ValueType.Rectangle:
                    {
                        var ints = StringHelper.Split(value, ",");
                        return new Rectangle(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture),
                            int.Parse(ints[2], CultureInfo.InvariantCulture),
                            int.Parse(ints[3], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.Point:
                    {
                        var ints = StringHelper.Split(value, ",");
                        return new Point(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.Size:
                    {
                        var ints = StringHelper.Split(value, ",");
                        return new Size(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.SizeF:
                    {
                        var floats = StringHelper.Split(value, ",");
                        return new SizeF(
                            float.Parse(floats[0], CultureInfo.InvariantCulture),
                            float.Parse(floats[1], CultureInfo.InvariantCulture)
                            );
                    }
                case (int)ValueType.Strings:
                    return StringHelper.SplitWithEscape(value, ',', '\\');
                case (int)ValueType.ByteArray:
                    return Convert.FromBase64String(value);
                default:
                    Trace.Fail("Unknown type " + type);
                    return null;
            }
        }

        private static void UnparseValue(object input, out ValueType valueType, out string output)
        {
            if (input == null)
            {
                Trace.Fail("Null input is not allowed here");
                throw new ArgumentNullException("input", "Null input is not allowed here");
            }

            if (input is char)
            {
                valueType = ValueType.Char;
                output = input.ToString();
            }
            else if (input is string)
            {
                valueType = ValueType.String;
                output = (string)input;
            }
            else if (input is bool)
            {
                valueType = ValueType.Bool;
                output = input.ToString();
            }
            else if (input is sbyte)
            {
                valueType = ValueType.SByte;
                output = ((sbyte)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is byte)
            {
                valueType = ValueType.Byte;
                output = ((byte)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is short)
            {
                valueType = ValueType.Int16;
                output = ((short)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is ushort)
            {
                valueType = ValueType.UInt16;
                output = ((ushort)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is int)
            {
                valueType = ValueType.Int32;
                output = ((int)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is uint)
            {
                valueType = ValueType.UInt32;
                output = ((uint)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is long)
            {
                valueType = ValueType.Int64;
                output = ((long)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is ulong)
            {
                valueType = ValueType.UInt64;
                output = ((ulong)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is double)
            {
                valueType = ValueType.Double;
                output = ((double)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is float)
            {
                valueType = ValueType.Float;
                output = ((float)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is decimal)
            {
                valueType = ValueType.Decimal;
                output = ((decimal)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is DateTime)
            {
                valueType = ValueType.DateTime;
                output = ((DateTime)input).ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Rectangle)
            {
                valueType = ValueType.Rectangle;
                var rect = (Rectangle)input;
                output = string.Format(CultureInfo.InvariantCulture,
                                       "{0},{1},{2},{3}",
                                       rect.Left.ToString(CultureInfo.InvariantCulture),
                                       rect.Top.ToString(CultureInfo.InvariantCulture),
                                       rect.Width.ToString(CultureInfo.InvariantCulture),
                                       rect.Height.ToString(CultureInfo.InvariantCulture));
            }
            else if (input is Point)
            {
                valueType = ValueType.Point;
                var pt = (Point)input;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       pt.X,
                                       pt.Y);
            }
            else if (input is Size)
            {
                valueType = ValueType.Size;
                var sz = (Size)input;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       sz.Width,
                                       sz.Height);
            }
            else if (input is SizeF)
            {
                valueType = ValueType.SizeF;
                var sz = (SizeF)input;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                                       sz.Width,
                                       sz.Height);
            }
            else if (input is string[])
            {
                valueType = ValueType.Strings;
                var values = new string[((string[])input).Length];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = ((string[])input)[i].Replace("\\", "\\\\").Replace(",", "\\,");
                }
                output = StringHelper.Join(values, ",");
            }
            else if (input is byte[])
            {
                valueType = ValueType.ByteArray;
                output = Convert.ToBase64String((byte[])input);
            }
            else
            {
                throw new ArgumentException("Unexpected valueType: " + input.GetType().FullName);
            }
        }

        public override IDisposable BatchUpdate() => new BatchUpdateHelper(this);

        private void BeginUpdate()
        {
            lock (this.SyncRoot)
            {
                this.batchUpdateRefCount++;
            }
        }

        private void EndUpdate()
        {
            lock (this.SyncRoot)
            {
                this.batchUpdateRefCount--;
                this.Persist();
                Trace.Assert(this.batchUpdateRefCount >= 0, "batchUpdateRefCount is less than zero");
            }
        }

        internal override void Persist()
        {
            lock (this.SyncRoot)
            {
                if (this.batchUpdateRefCount > 0)
                {
                    return;
                }

                var xmlDoc = new XmlDocument();
                var settings = xmlDoc.CreateElement("settings");
                xmlDoc.AppendChild(settings);
                ToXml(settings, this.values, this.subsettings);

                this.stream.Position = 0;
                this.stream.SetLength(0);
                xmlDoc.Save(this.stream);
                this.stream.Flush();
            }
        }

        private static void ToXml(XmlElement settings, Hashtable values, Hashtable subsettings)
        {
            var valueKeys = new ArrayList(values.Keys);
            valueKeys.Sort();
            foreach (string key in valueKeys)
            {
                var el = settings.OwnerDocument.CreateElement("value");
                el.SetAttribute("name", key);
                var value = values[key];
                if (value != null)
                {
                    UnparseValue(value, out var valueType, out var output);
                    el.SetAttribute("type", valueType.ToString());
                    el.InnerText = output;
                    settings.AppendChild(el);
                }
            }

            var subsettingsKeys = new ArrayList(subsettings.Keys);
            subsettingsKeys.Sort();
            foreach (string key in subsettingsKeys)
            {
                var el = settings.OwnerDocument.CreateElement("settings");
                el.SetAttribute("name", key);
                var hashtables = (Hashtable[])subsettings[key];
                if (hashtables != null)
                {
                    settings.AppendChild(el);
                    ToXml(el, hashtables[0], hashtables[1]);
                }
            }
        }
    }
}
