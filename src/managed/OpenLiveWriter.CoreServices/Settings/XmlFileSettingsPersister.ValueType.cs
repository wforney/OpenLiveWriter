// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices.Settings
{

    public partial class XmlFileSettingsPersister
    {
        enum ValueType
        {
            Char,
            String,
            Bool,
            SByte,
            Byte,
            Int16,
            UInt16,
            Int32,
            UInt32,
            Int64,
            UInt64,
            Double,
            Float,
            Decimal,
            DateTime,
            Rectangle,
            Point,
            Size,
            SizeF,
            Strings,
            ByteArray
        }
    }
}
