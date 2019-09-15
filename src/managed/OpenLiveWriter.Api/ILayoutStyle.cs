// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    /// <summary>
    ///  Layout options for SmartContent object.
    /// </summary>
    public interface ILayoutStyle
    {
        /// <summary>
        /// Alignment of object relative to text.
        /// </summary>
        Alignment Alignment
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) above the object.
        /// </summary>
        int TopMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) to the right of object.
        /// </summary>
        int RightMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) below the object.
        /// </summary>
        int BottomMargin
        {
            get;
            set;
        }

        /// <summary>
        /// Margin (in pixels) to the left of the object.
        /// </summary>
        int LeftMargin
        {
            get;
            set;
        }

    }

}

