// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class OptionInfo
    {
        public OptionInfo(string value, string label, bool selected)
        {
            Value = value;
            Label = label;
            Selected = selected;
        }

        public string Value { get; }

        public string Label { get; }

        public bool Selected { get; set; }
    }
}
