// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Drawing;
using OpenLiveWriter.Localization;

namespace OpenLiveWriter.ApplicationFramework
{
    public struct ColorPickerColor
    {
        public ColorPickerColor(Color color, StringId id)
        {
            this.Color = color;
            this.StringId = id;
        }

        public Color Color;
        public StringId StringId;
    }
}
