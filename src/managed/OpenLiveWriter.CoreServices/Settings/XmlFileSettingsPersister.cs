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
    using System.Text;
    using System.Xml;

    /// <summary>
    /// The XmlFileSettingsPersister class.
    /// Implements the <see cref="XmlSettingsPersister" />
    /// </summary>
    /// <seealso cref="XmlSettingsPersister" />
    public partial class XmlFileSettingsPersister : XmlSettingsPersister
    {
        /// <summary>
        /// The stream
        /// </summary>
        private readonly Stream stream;

        /// <summary>
        /// The synchronize root
        /// </summary>
        private readonly object syncRoot = new object();

        /// <summary>
        /// The batch update reference count
        /// </summary>
        private int batchUpdateRefCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlFileSettingsPersister"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="values">The values.</param>
        /// <param name="subsettings">The sub-settings.</param>
        private XmlFileSettingsPersister(Stream stream, Hashtable values, Hashtable subsettings)
            : base(values, subsettings) =>
            this.stream = stream;

        /// <summary>
        /// Gets the synchronize root.
        /// </summary>
        /// <value>The synchronize root.</value>
        protected internal override object SyncRoot => this.syncRoot;

        /// <summary>
        /// Opens the specified filename.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>An <see cref="XmlFileSettingsPersister"/>.</returns>
        public static XmlFileSettingsPersister Open(string filename)
        {
            Stream s = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            if (s.Length == 0)
            {
                return new XmlFileSettingsPersister(s, new Hashtable(), new Hashtable());
            }

            XmlFileSettingsPersister.Parse(s, out var values, out var subsettings);
            return new XmlFileSettingsPersister(s, values, subsettings);
        }

        /// <summary>
        /// Batches the update.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/>.</returns>
        public override IDisposable BatchUpdate() => new BatchUpdateHelper(this);

        /// <summary>
        /// Persists this instance.
        /// </summary>
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
                XmlFileSettingsPersister.ToXml(settings, this.Values, this.Subsettings);

                this.stream.Position = 0;
                this.stream.SetLength(0);
                xmlDoc.Save(this.stream);
                this.stream.Flush();
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.stream.Dispose();
            }

            base.Dispose(false);
        }

        /// <summary>
        /// Parses the specified stream.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <param name="values">The values.</param>
        /// <param name="subsettings">The sub-settings.</param>
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
            XmlFileSettingsPersister.Parse(root, values, subsettings);
        }

        /// <summary>
        /// Parses the specified node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="values">The values.</param>
        /// <param name="subsettings">The sub-settings.</param>
        private static void Parse(XmlNode node, IDictionary values, IDictionary subsettings)
        {
            foreach (var valueNode in node.SelectNodes("value")?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                var name = valueNode.GetAttribute("name");
                var type = valueNode.GetAttribute("type");
                var value = valueNode.InnerText;
                values[name] = XmlFileSettingsPersister.ParseValue(type, value);
            }

            foreach (var settingsNode in node.SelectNodes("settings")?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                var name = settingsNode.GetAttribute("name");
                var subvalues = new Hashtable();
                var subsubsettings = new Hashtable();
                XmlFileSettingsPersister.Parse(settingsNode, subvalues, subsubsettings);
                subsettings[name] = new[] { subvalues, subsubsettings };
            }
        }

        /// <summary>
        /// Parses the value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value object.</returns>
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
                        var ints = StringHelper.Split(value, ",").ToArray();
                        return new Rectangle(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture),
                            int.Parse(ints[2], CultureInfo.InvariantCulture),
                            int.Parse(ints[3], CultureInfo.InvariantCulture));
                    }

                case (int)ValueType.Point:
                    {
                        var ints = StringHelper.Split(value, ",").ToArray();
                        return new Point(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture));
                    }

                case (int)ValueType.Size:
                    {
                        var ints = StringHelper.Split(value, ",").ToArray();
                        return new Size(
                            int.Parse(ints[0], CultureInfo.InvariantCulture),
                            int.Parse(ints[1], CultureInfo.InvariantCulture));
                    }

                case (int)ValueType.SizeF:
                    {
                        var floats = StringHelper.Split(value, ",").ToArray();
                        return new SizeF(
                            float.Parse(floats[0], CultureInfo.InvariantCulture),
                            float.Parse(floats[1], CultureInfo.InvariantCulture));
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

        /// <summary>
        /// Converts to xml.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="values">The values.</param>
        /// <param name="subsettings">The subsettings.</param>
        private static void ToXml(XmlElement settings, Hashtable values, Hashtable subsettings)
        {
            var valueKeys = new ArrayList(values.Keys);
            valueKeys.Sort();
            foreach (string key in valueKeys)
            {
                if (settings.OwnerDocument == null)
                {
                    continue;
                }

                var el = settings.OwnerDocument.CreateElement("value");
                el.SetAttribute("name", key);
                var value = values[key];
                if (value == null)
                {
                    continue;
                }

                XmlFileSettingsPersister.UnparseValue(value, out var valueType, out var output);
                el.SetAttribute("type", valueType.ToString());
                if (output != null)
                {
                    el.InnerText = output;
                }

                settings.AppendChild(el);
            }

            var subsettingsKeys = new List<string>(subsettings.Keys.Cast<string>());
            subsettingsKeys.Sort();
            foreach (var key in subsettingsKeys)
            {
                if (settings.OwnerDocument == null)
                {
                    continue;
                }

                var el = settings.OwnerDocument.CreateElement("settings");
                el.SetAttribute("name", key);
                var hashTables = (Hashtable[])subsettings[key];
                if (hashTables == null)
                {
                    continue;
                }

                settings.AppendChild(el);
                XmlFileSettingsPersister.ToXml(el, hashTables[0], hashTables[1]);
            }
        }

        /// <summary>
        /// Unparses the value.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="valueType">Type of the value.</param>
        /// <param name="output">The output.</param>
        /// <exception cref="ArgumentNullException">input - Null input is not allowed here</exception>
        /// <exception cref="ArgumentException">Unexpected valueType: " + input.GetType().FullName</exception>
        private static void UnparseValue(object input, out ValueType valueType, out string output)
        {
            if (input == null)
            {
                Trace.Fail(Exceptions.NullInputIsNotAllowedHere);
                throw new ArgumentNullException(nameof(input), Exceptions.NullInputIsNotAllowedHere);
            }

            if (input is char)
            {
                valueType = ValueType.Char;
                output = input.ToString();
            }
            else if (input is string outputString)
            {
                valueType = ValueType.String;
                output = outputString;
            }
            else if (input is bool)
            {
                valueType = ValueType.Bool;
                output = input.ToString();
            }
            else if (input is sbyte sbyteOutput)
            {
                valueType = ValueType.SByte;
                output = sbyteOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is byte byteOutput)
            {
                valueType = ValueType.Byte;
                output = byteOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is short shortOutput)
            {
                valueType = ValueType.Int16;
                output = shortOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is ushort ushortOutput)
            {
                valueType = ValueType.UInt16;
                output = ushortOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is int intOutput)
            {
                valueType = ValueType.Int32;
                output = intOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is uint uintOutput)
            {
                valueType = ValueType.UInt32;
                output = uintOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is long longOutput)
            {
                valueType = ValueType.Int64;
                output = longOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is ulong ulongOutput)
            {
                valueType = ValueType.UInt64;
                output = ulongOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is double doubleOutput)
            {
                valueType = ValueType.Double;
                output = doubleOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is float floatOutput)
            {
                valueType = ValueType.Float;
                output = floatOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is decimal decimalOutput)
            {
                valueType = ValueType.Decimal;
                output = decimalOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is DateTime dateTimeOutput)
            {
                valueType = ValueType.DateTime;
                output = dateTimeOutput.ToString(CultureInfo.InvariantCulture);
            }
            else if (input is Rectangle rect)
            {
                valueType = ValueType.Rectangle;
                output = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0},{1},{2},{3}",
                    rect.Left.ToString(CultureInfo.InvariantCulture),
                    rect.Top.ToString(CultureInfo.InvariantCulture),
                    rect.Width.ToString(CultureInfo.InvariantCulture),
                    rect.Height.ToString(CultureInfo.InvariantCulture));
            }
            else if (input is Point pt)
            {
                valueType = ValueType.Point;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}", pt.X, pt.Y);
            }
            else if (input is Size sz1)
            {
                valueType = ValueType.Size;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}", sz1.Width, sz1.Height);
            }
            else if (input is SizeF sz)
            {
                valueType = ValueType.SizeF;
                output = string.Format(CultureInfo.InvariantCulture, "{0},{1}", sz.Width, sz.Height);
            }
            else if (input is string[] stringArrayOutput)
            {
                valueType = ValueType.Strings;
                var sb = new StringBuilder();
                var values = new string[stringArrayOutput.Length];
                for (var i = 0; i < values.Length; i++)
                {
                    values[i] = stringArrayOutput[i].Replace("\\", "\\\\").Replace(",", "\\,");
                }

                output = StringHelper.Join(values, ",");
            }
            else if (input is byte[] bytes)
            {
                valueType = ValueType.ByteArray;
                output = Convert.ToBase64String(bytes);
            }
            else
            {
                throw new ArgumentException($"Unexpected valueType: {input.GetType().FullName}");
            }
        }

        /// <summary>
        /// Begins the update.
        /// </summary>
        private void BeginUpdate()
        {
            lock (this.SyncRoot)
            {
                this.batchUpdateRefCount++;
            }
        }

        /// <summary>
        /// Ends the update.
        /// </summary>
        private void EndUpdate()
        {
            lock (this.SyncRoot)
            {
                this.batchUpdateRefCount--;
                this.Persist();
                Trace.Assert(this.batchUpdateRefCount >= 0, "batchUpdateRefCount is less than zero");
            }
        }
    }
}
