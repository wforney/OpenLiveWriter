// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class Radio : FormElementWithValue
    {
        public Radio(HtmlForm parentForm, string name, string value, bool isChecked) : base(parentForm, name, value)
        {
            Checked = isChecked;
        }

        public bool Checked { get; private set; }

        public void Select()
        {
            if (Checked)
                return;

            foreach (FormElement el in ParentForm.GetElementsByName(Name))
            {
                if (el is Radio radio && radio.Checked)
                    radio.Checked = false;
            }

            Checked = true;
        }

        public override bool IsSuccessful
        {
            get { return Checked; }
        }
    }
}
