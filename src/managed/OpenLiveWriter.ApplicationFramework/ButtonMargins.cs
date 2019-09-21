// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    public struct ButtonMargins
    {
        public readonly int LeftOfImage;
        public readonly int LeftOfText;
        public readonly int LeftOfArrow;
        public readonly int RightPadding;
        public readonly int RightMargin;

        public ButtonMargins(int leftOfImage, int leftOfText, int leftOfArrow, int rightPadding, int rightMargin)
        {
            LeftOfImage = leftOfImage;
            LeftOfText = leftOfText;
            LeftOfArrow = leftOfArrow;
            RightPadding = rightPadding;
            RightMargin = rightMargin;
        }
    }

}
