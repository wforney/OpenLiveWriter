// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class ImageButton : SubmitButton
    {
        public ImageButton(HtmlForm parentForm, string name, string value, string src) : base(parentForm, name, value)
        {
            Src = src;
        }

        public string Src { get; }

        public override void AddData(FormData data)
        {
            if (Name != null)
            {
                data.Add(Name + ".x", "1");
                data.Add(Name + ".y", "1");
            }
            else
            {
                data.Add("x", "1");
                data.Add("y", "1");
            }

            base.AddData(data);
        }
    }
}
