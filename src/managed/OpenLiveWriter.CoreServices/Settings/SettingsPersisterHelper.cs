// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;

    /// <summary>
    /// Convenience class which allows one to use an ISettingsPersister more easily.  Method naming
    /// follows the convention of the System.Convert class for consistency.
    /// </summary>
    public class SettingsPersisterHelper : IDisposable
    {
        /// <summary>
        /// Dispose the settings persister helper
        /// </summary>
        public void Dispose() => this.SettingsPersister.Dispose();

        /// <summary>
        /// Gets the ISettingsPersister that this SettingsPersisterHelper is bound to.  This is an
        /// escape hatch which allows one to access the ISettingsPersister directly.
        /// </summary>
        public ISettingsPersister SettingsPersister { get; }

        /// <summary>
        /// Initializes a new instance of the SettingsPersisterHelper class.
        /// </summary>
        /// <param name="settingsPersister">The ISettingsPersister that this SettingsPersisterHelper is bound to.</param>
        public SettingsPersisterHelper(ISettingsPersister settingsPersister) =>
            //	Set the settings persister.
            this.SettingsPersister = settingsPersister;

        /// <summary>
        /// Copy the contents of the source settings into this settings
        /// </summary>
        /// <param name="sourceSettings"></param>
        public void CopyFrom(SettingsPersisterHelper sourceSettings, bool recursive, bool overWrite)
        {
            // copy root-level values (to work with types generically we need references
            // to the underlying settings persister objects)
            var source = sourceSettings.SettingsPersister;
            var destination = this.SettingsPersister;
            foreach (var name in source.GetNames())
            {
                if (overWrite || destination.Get<object>(name) == null)
                {
                    destination.Set(name, source.Get<object>(name));
                }
            }

            // if this is recursive then copy all of the sub-settings
            if (recursive)
            {
                foreach (var subSetting in sourceSettings.GetSubSettingNames())
                {
                    using (SettingsPersisterHelper
                                sourceSubSetting = sourceSettings.GetSubSettings(subSetting),
                                destinationSubSetting = this.GetSubSettings(subSetting))
                    {
                        destinationSubSetting.CopyFrom(sourceSubSetting, recursive, overWrite);
                    }
                }
            }
        }

        /// <summary>
        /// Get the names of available settings.
        /// </summary>
        public ICollection<string> GetNames() => this.SettingsPersister.GetNames();

        public bool HasValue(string name) => this.SettingsPersister.Get<object>(name) != null;

        public bool HasSubSettings(string subSettingsName) => this.SettingsPersister.HasSubSettings(subSettingsName);

        public SettingsPersisterHelper GetSubSettings(string subSettingsName) => new SettingsPersisterHelper(this.SettingsPersister.GetSubSettings(subSettingsName));

        /// <summary>
        /// Returns the names of the available subsettings.
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetSubSettingNames() => this.SettingsPersister.GetSubSettings();

        /// <summary>
        /// Gets the Unicode character setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public char GetChar(string name, char defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the string setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public string GetString(string name, string defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        public string GetEncryptedString(string name)
        {
            var encrypted = this.GetByteArray(name, null);
            if (encrypted == null)
            {
                return null;
            }

            try
            {
                return CryptHelper.Decrypt(encrypted);
            }
            catch (Exception e)
            {
                Trace.Fail("Failure during decrypt: " + e);
                return null;
            }
        }

        public string[] GetStrings(string name, params string[] defaultValues) => this.SettingsPersister.Get(name, defaultValues);

        /// <summary>
        /// Gets the enum value setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="enumType">The type of enumeration.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public object GetEnumValue(string name, Type enumType, object defaultValue)
        {
            var val = this.GetString(name, defaultValue.ToString());
            if (val == null)
            {
                return defaultValue;
            }

            try
            {
                return Enum.Parse(enumType, val, true);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets the boolean setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public bool GetBoolean(string name, bool defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 8-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public sbyte GetSByte(string name, sbyte defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 8-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public byte GetByte(string name, byte defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 16-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public short GetInt16(string name, short defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 16-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public ushort GetUInt16(string name, ushort defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 32-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public int GetInt32(string name, int defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 32-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public uint GetUInt32(string name, uint defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 64-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public long GetInt64(string name, long defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the 64-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public ulong GetUInt64(string name, ulong defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the double-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public double GetDouble(string name, double defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the single-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public float GetFloat(string name, float defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the decimal number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public decimal GetDecimal(string name, decimal defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the DateTime setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public DateTime GetDateTime(string name, DateTime defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the Rectangle setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public Rectangle GetRectangle(string name, Rectangle defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the Point setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public Point GetPoint(string name, Point defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the SizeF setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public SizeF GetSizeF(string name, SizeF defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the Size setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public Size GetSize(string name, Size defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// Gets the DateTime setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to get.</param>
        /// <param name="defaultValue">The default value to return in the event that a setting
        /// with the specified name cannot be found, or an exception occurs.</param>
        /// <returns>The value of the setting, or the default value.</returns>
        public byte[] GetByteArray(string name, byte[] defaultValue) => this.SettingsPersister.Get(name, defaultValue);

        /// <summary>
        /// See ISettingsPersister.BatchUpdate()
        /// </summary>
        public IDisposable BatchUpdate() => this.SettingsPersister.BatchUpdate();

        /// <summary>
        /// Sets the Unicode character setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetChar(string name, char value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the string setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetString(string name, string value) => this.SettingsPersister.Set(name, value);

        public void SetEncryptedString(string name, string value) => this.SetByteArray(name, CryptHelper.Encrypt(value));

        public void SetStrings(string name, params string[] values) => this.SettingsPersister.Set(name, values);

        /// <summary>
        /// Sets the boolean setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetBoolean(string name, bool value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 8-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetSByte(string name, sbyte value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 8-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetByte(string name, byte value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 16-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetInt16(string name, short value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 16-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetUInt16(string name, ushort value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 32-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetInt32(string name, int value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 32-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetUInt32(string name, uint value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 64-bit signed integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetInt64(string name, long value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the 64-bit unsigned integer setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetUInt64(string name, ulong value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the double-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDouble(string name, double value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the single-precision floating point number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetFloat(string name, float value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the decimal number setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDecimal(string name, decimal value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the DateTime setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetDateTime(string name, DateTime value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the Rectangle setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetRectangle(string name, Rectangle value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the Point setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetPoint(string name, Point value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the Size setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetSize(string name, Size value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the SizeF setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetSizeF(string name, SizeF value) => this.SettingsPersister.Set(name, value);

        /// <summary>
        /// Sets the byte array setting with the specified name.
        /// </summary>
        /// <param name="name">Name of the setting to set.</param>
        /// <param name="value">The value to set.</param>
        public void SetByteArray(string name, byte[] value) => this.SettingsPersister.Set(name, value);

        public void Unset(string name) => this.SettingsPersister.Unset(name);

        public void UnsetSubsettingTree(string name)
        {
            if (this.SettingsPersister.HasSubSettings(name))
            {
                this.SettingsPersister.UnsetSubSettingsTree(name);
            }
        }

        /// <summary>
        /// Returns a SettingsPersisterHelper for the first RegistryKeySpec that exists
        /// in the registry, or null if the registry doesn't contain any of them.
        /// </summary>
        public static SettingsPersisterHelper OpenFirstRegistryKey(params RegistryKeySpec[] keySpecs)
        {
            foreach (var spec in keySpecs)
            {
                using (var key = spec.baseKey.OpenSubKey(spec.subkey, false))
                {
                    if (key != null)
                    {
                        if (spec.requiredValue != null && key.GetValue(spec.requiredValue, null) == null)
                        {
                            continue;
                        }

                        return new SettingsPersisterHelper(new RegistrySettingsPersister(spec.baseKey, spec.subkey));
                    }
                }
            }
            return null;
        }
    }
}
