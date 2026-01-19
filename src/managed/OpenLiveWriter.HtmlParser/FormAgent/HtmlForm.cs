// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Collections;
using System.Globalization;

namespace OpenLiveWriter.HtmlParser.Parser.FormAgent
{
    public class HtmlForm : IEnumerable
    {
        private readonly ArrayList elements;

        public HtmlForm(string name, string action, string method)
        {
            Name = name;
            Action = action;
            Method = method;
            this.elements = new ArrayList();
        }

        public string Name { get; }

        public string Action { get; }

        public string Method { get; }

        internal void Add(FormElement el)
        {
            if (el != null)
                elements.Add(el);
        }

        public int ElementCount
        {
            get
            {
                return elements.Count;
            }
        }

        public FormElement GetElementByIndex(int index)
        {
            return (FormElement)elements[index];
        }

        public FormElement[] GetElementsByName(string name)
        {
            ArrayList results = new ArrayList();
            foreach (FormElement el in elements)
            {
                if (el.Name != null && el.Name.ToLowerInvariant() == name)
                    results.Add(el);
            }

            return (FormElement[])results.ToArray(typeof(FormElement));
        }

        public FormElement GetSingleElementByName(string name)
        {
            FormElement[] results = GetElementsByName(name);
            return results.Length == 0 ? null : results[0];
        }

        public IEnumerator GetEnumerator()
        {
            return elements.GetEnumerator();
        }

        public FormData Submit(SubmitButton button)
        {
            FormData formData = new FormData();

            foreach (FormElement el in elements)
            {
                if (el.IsSuccessful || object.ReferenceEquals(button, el))
                {
                    el.AddData(formData);
                }
            }

            return formData;
        }
    }
}
