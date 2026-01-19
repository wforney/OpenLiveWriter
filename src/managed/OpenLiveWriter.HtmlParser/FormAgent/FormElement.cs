// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public abstract class FormElement
    {
        public FormElement(HtmlForm parentForm, string name)
        {
            ParentForm = parentForm;
            Name = name;
            ParentForm.Add(this);
        }

        /// <summary>
        /// If true, then should be included in a form post.
        /// </summary>
        public abstract bool IsSuccessful { get; }

        public abstract void AddData(FormData data);

        public HtmlForm ParentForm { get; }

        public string Name { get; }
    }
}
