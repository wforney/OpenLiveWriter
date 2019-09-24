// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{
    public partial class XmlFileSettingsPersister
    {
        /// <summary>
        /// The ValueType enumeration.
        /// </summary>
        private enum ValueType
        {
            /// <summary>
            /// The character
            /// </summary>
            Char,

            /// <summary>
            /// The string
            /// </summary>
            String,

            /// <summary>
            /// The bool
            /// </summary>
            Bool,

            /// <summary>
            /// The s byte
            /// </summary>
            SByte,

            /// <summary>
            /// The byte
            /// </summary>
            Byte,

            /// <summary>
            /// The int16
            /// </summary>
            Int16,

            /// <summary>
            /// The u int16
            /// </summary>
            UInt16,

            /// <summary>
            /// The int32
            /// </summary>
            Int32,

            /// <summary>
            /// The u int32
            /// </summary>
            UInt32,

            /// <summary>
            /// The int64
            /// </summary>
            Int64,

            /// <summary>
            /// The u int64
            /// </summary>
            UInt64,

            /// <summary>
            /// The double
            /// </summary>
            Double,

            /// <summary>
            /// The float
            /// </summary>
            Float,

            /// <summary>
            /// The decimal
            /// </summary>
            Decimal,

            /// <summary>
            /// The date time
            /// </summary>
            DateTime,

            /// <summary>
            /// The rectangle
            /// </summary>
            Rectangle,

            /// <summary>
            /// The point
            /// </summary>
            Point,

            /// <summary>
            /// The size
            /// </summary>
            Size,

            /// <summary>
            /// The size f
            /// </summary>
            SizeF,

            /// <summary>
            /// The strings
            /// </summary>
            Strings,

            /// <summary>
            /// The byte array
            /// </summary>
            ByteArray
        }
    }
}
