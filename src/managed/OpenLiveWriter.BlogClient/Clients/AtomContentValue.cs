// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;

    using OpenLiveWriter.BlogClient.Properties;
    using OpenLiveWriter.HtmlParser.Parser;

    /// <summary>
    /// Encapsulates Atom content, which can be text, HTML, or XHTML,
    /// and allows easy conversion to text or HTML regardless of
    /// input type.
    /// </summary>
    internal class AtomContentValue
    {
        /// <summary>
        /// The value
        /// </summary>
        private readonly string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomContentValue"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="value">The value.</param>
        public AtomContentValue(AtomContentValueType type, string value)
        {
            this.Type = type;
            this.value = value;
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public AtomContentValueType Type { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The value.</returns>
        /// <exception cref="ArgumentException">Cannot convert text value to XHTML - type</exception>
        /// <exception cref="InvalidOperationException">Unknown text type: " + _type</exception>
        public string GetValue(AtomContentValueType type)
        {
            if (this.value == null)
            {
                return null;
            }

            switch (type)
            {
                case AtomContentValueType.Text:
                    return this.ToText();
                case AtomContentValueType.HTML:
                    return this.ToHTML();
                case AtomContentValueType.XHTML:
                    if (this.Type == AtomContentValueType.XHTML)
                    {
                        return this.value;
                    }
                    else
                    {
                        throw new ArgumentException(Resources.CannotConvertTextValueToXHTML, nameof(type));
                    }
            }

            throw new InvalidOperationException($"Unknown text type: {this.Type}");
        }

        /// <summary>
        /// Converts to HTML.
        /// </summary>
        /// <returns>The HTML.</returns>
        /// <exception cref="InvalidOperationException">Unknown text type: " + _type</exception>
        public string ToHTML()
        {
            if (this.value == null)
            {
                return null;
            }

            switch (this.Type)
            {
                case AtomContentValueType.Text:
                    return HtmlUtils.EscapeEntities(this.value);
                case AtomContentValueType.HTML:
                case AtomContentValueType.XHTML:
                    return this.value;
            }

            throw new InvalidOperationException($"Unknown text type: {this.Type}");
        }

        /// <summary>
        /// Converts to text.
        /// </summary>
        /// <returns>The text.</returns>
        /// <exception cref="InvalidOperationException">Unknown text type: " + _type</exception>
        public string ToText()
        {
            if (this.value == null)
            {
                return null;
            }

            switch (this.Type)
            {
                case AtomContentValueType.Text:
                    return this.value;
                case AtomContentValueType.HTML:
                case AtomContentValueType.XHTML:
                    return HtmlUtils.HTMLToPlainText(this.value, false);
            }

            throw new InvalidOperationException($"Unknown text type: {this.Type}");
        }
    }
}
