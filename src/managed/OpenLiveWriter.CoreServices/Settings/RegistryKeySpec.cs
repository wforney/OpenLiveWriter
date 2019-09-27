// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{

    using Microsoft.Win32;

    public class RegistryKeySpec
    {
        internal readonly RegistryKey baseKey;
        internal readonly string subkey;
        internal readonly string requiredValue;

        /// <summary>
        /// Specifies a particular key.
        /// </summary>
        public RegistryKeySpec(RegistryKey baseKey, string subkey) : this(baseKey, subkey, null)
        {
        }

        /// <summary>
        /// Specifies a particular key AND that a value name exists in that key.
        /// </summary>
        public RegistryKeySpec(RegistryKey baseKey, string subkey, string requiredValue)
        {
            this.baseKey = baseKey;
            this.subkey = subkey;
            this.requiredValue = requiredValue;
        }
    }
}
