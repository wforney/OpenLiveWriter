// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Option
    {
        private bool selected;

        public Option(Select parentSelect, OptionInfo optionInfo)
        {
            ParentSelect = parentSelect;
            Label = optionInfo.Label;
            Value = optionInfo.Value;
            if (Value == null)
                Value = Label;
            this.selected = optionInfo.Selected;
        }

        public Select ParentSelect { get; }

        public string Value { get; }

        public string Label { get; }

        public bool Selected
        {
            get { return selected; }
            set
            {
                if (ParentSelect.Multiple)
                    selected = value;
                else
                {
                    if (!value)
                    {
                        throw new InvalidOperationException("Cannot unselect an option that is in a non-multiple select");
                    }
                    else
                    {
                        foreach (Option opt in ParentSelect)
                        {
                            opt.selected = false;
                        }

                        selected = true;
                    }
                }
            }
        }
    }
}
