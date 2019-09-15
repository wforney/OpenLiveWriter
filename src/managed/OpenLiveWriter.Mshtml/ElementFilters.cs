// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Mshtml
{
    using System;
    using System.Collections;
    using System.Globalization;
    using mshtml;
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Utility class for implementing IHTMLElementFilter methods (useful for grabbing elements out of MarkupRanges).
    /// </summary>
    public class ElementFilters
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="ElementFilters"/> class from being created.
        /// </summary>
        private ElementFilters()
        {
            // no instances
        }

        /// <summary>
        /// Creates the tag identifier filter.
        /// </summary>
        /// <param name="tagId">The tag identifier.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateTagIdFilter(string tagId) => new IHTMLElementFilter(new TagIdElementFilter(tagId).Filter);

        /// <summary>
        /// Creates the identifier filter.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateIdFilter(string id) => new IHTMLElementFilter(new IdElementFilter(id).Filter);

        /// <summary>
        /// Creates the equal filter.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateEqualFilter(IHTMLElement element) => new IHTMLElementFilter(new EqualElementFilter(element).Filter);

        /// <summary>
        /// Creates the control element filter.
        /// </summary>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateControlElementFilter() => new IHTMLElementFilter(new ControlElementFilter().Filter);

        /// <summary>
        /// Creates the class filter.
        /// </summary>
        /// <param name="className">Name of the class.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateClassFilter(string className) => new IHTMLElementFilter(new ClassElementFilter(className).Filter);

        /// <summary>
        /// Creates the element name filter.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateElementNameFilter(string name) => new IHTMLElementFilter(new ElementNameFilter(name).Filter);

        /// <summary>
        /// Creates the element attribute filter.
        /// </summary>
        /// <param name="attributeName">Name of the attribute.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateElementAttributeFilter(string attributeName) => new IHTMLElementFilter(new ElementAttributeFilter(attributeName).Filter);

        /// <summary>
        /// Creates the element background color inline style filter.
        /// </summary>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateElementBackgroundColorInlineStyleFilter() => new IHTMLElementFilter(new ElementBackgroundColorInlineStyleFilter().Filter);

        /// <summary>
        /// Creates the element equals filter.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateElementEqualsFilter(IHTMLElement e) => new IHTMLElementFilter(new ElementEqualsFilter(e).Filter);

        /// <summary>
        /// Creates the element pass filter.
        /// </summary>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateElementPassFilter() => new IHTMLElementFilter(new ElementPassFilter().Filter);

        /// <summary>
        /// Creates the compound element filter.
        /// </summary>
        /// <param name="filters">The filters.</param>
        /// <returns>IHTMLElementFilter.</returns>
        public static IHTMLElementFilter CreateCompoundElementFilter(params IHTMLElementFilter[] filters) => new IHTMLElementFilter(new CompoundElementFilter(filters).MergeElementFilters);

        /// <summary>
        /// Class TagIdElementFilter.
        /// </summary>
        private class TagIdElementFilter
        {
            /// <summary>
            /// The tag identifier.
            /// </summary>
            private readonly string tagId;

            /// <summary>
            /// Initializes a new instance of the <see cref="TagIdElementFilter"/> class.
            /// </summary>
            /// <param name="tagId">The tag identifier.</param>
            public TagIdElementFilter(string tagId) => this.tagId = tagId;

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => string.Compare(e.tagName, this.tagId, StringComparison.OrdinalIgnoreCase) == 0;
        }

        /// <summary>
        /// Class IdElementFilter.
        /// </summary>
        private class IdElementFilter
        {
            /// <summary>
            /// The identifier.
            /// </summary>
            private readonly string id;

            /// <summary>
            /// Initializes a new instance of the <see cref="IdElementFilter"/> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            public IdElementFilter(string id) => this.id = id;

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => e.id == this.id;
        }

        /// <summary>
        /// Class ElementPassFilter.
        /// </summary>
        private class ElementPassFilter
        {
            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => true;
        }

        /// <summary>
        /// Class EqualElementFilter.
        /// </summary>
        private class EqualElementFilter
        {
            /// <summary>
            /// The element.
            /// </summary>
            private readonly IHTMLElement element;

            /// <summary>
            /// Initializes a new instance of the <see cref="EqualElementFilter"/> class.
            /// </summary>
            /// <param name="element">The element.</param>
            public EqualElementFilter(IHTMLElement element) => this.element = element;

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => HTMLElementHelper.ElementsAreEqual(e, this.element);
        }

        /// <summary>
        /// Class ControlElementFilter.
        /// </summary>
        private class ControlElementFilter
        {
            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => (e as IHTMLControlElement) != null;
        }

        /// <summary>
        /// Class ClassElementFilter.
        /// </summary>
        private class ClassElementFilter
        {
            /// <summary>
            /// The class name.
            /// </summary>
            private readonly string className;

            /// <summary>
            /// Initializes a new instance of the <see cref="ClassElementFilter"/> class.
            /// </summary>
            /// <param name="className">Name of the class.</param>
            public ClassElementFilter(string className) => this.className = className;

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => e.className == this.className;
        }

        /// <summary>
        /// Class ElementNameFilter.
        /// </summary>
        private class ElementNameFilter
        {
            /// <summary>
            /// The name.
            /// </summary>
            private readonly string name;

            /// <summary>
            /// Initializes a new instance of the <see cref="ElementNameFilter"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public ElementNameFilter(string name) => this.name = name;

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => e.tagName.Equals(this.name, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Class ElementAttributeFilter.
        /// </summary>
        private class ElementAttributeFilter
        {
            /// <summary>
            /// The attribute name.
            /// </summary>
            private readonly string attributeName;

            /// <summary>
            /// Initializes a new instance of the <see cref="ElementAttributeFilter"/> class.
            /// </summary>
            /// <param name="attributeName">Name of the attribute.</param>
            public ElementAttributeFilter(string attributeName) => this.attributeName = attributeName.ToUpper(CultureInfo.InvariantCulture);

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e)
            {
                string attributeName = e.getAttribute(this.attributeName, 2) as string;
                return !string.IsNullOrEmpty(attributeName);
            }
        }

        /// <summary>
        /// Class ElementBackgroundColorInlineStyleFilter.
        /// </summary>
        private class ElementBackgroundColorInlineStyleFilter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ElementBackgroundColorInlineStyleFilter"/> class.
            /// </summary>
            public ElementBackgroundColorInlineStyleFilter()
            {
            }

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => !string.IsNullOrEmpty((string)e.style.backgroundColor);
        }

        /// <summary>
        /// Class ElementEqualsFilter.
        /// </summary>
        private class ElementEqualsFilter
        {
            /// <summary>
            /// The element.
            /// </summary>
            private readonly IHTMLElement element;

            /// <summary>
            /// Initializes a new instance of the <see cref="ElementEqualsFilter"/> class.
            /// </summary>
            /// <param name="e">The e.</param>
            public ElementEqualsFilter(IHTMLElement e) => this.element = e;

            /// <summary>
            /// Filters the specified e.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool Filter(IHTMLElement e) => e.sourceIndex == this.element.sourceIndex;
        }

        /// <summary>
        /// Class CompoundElementFilter.
        /// </summary>
        private class CompoundElementFilter
        {
            /// <summary>
            /// The filters.
            /// </summary>
            private readonly IHTMLElementFilter[] filters;

            /// <summary>
            /// Initializes a new instance of the <see cref="CompoundElementFilter"/> class.
            /// </summary>
            /// <param name="filters">The filters.</param>
            public CompoundElementFilter(IHTMLElementFilter[] filters) => this.filters = filters;

            /// <summary>
            /// Merges the element filters.
            /// </summary>
            /// <param name="e">The e.</param>
            /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
            public bool MergeElementFilters(IHTMLElement e)
            {
                foreach (var filter in this.filters)
                {
                    if (filter(e))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Returns true if this is an element that triggers a paragraph break.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is table element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsTableElement(IHTMLElement e) => TableTagNames[e.tagName] != null;

        /// <summary>
        /// The table elements.
        /// </summary>
        public static IHTMLElementFilter TABLE_ELEMENTS = new IHTMLElementFilter(IsTableElement);

        /// <summary>
        /// Returns true if this element is the body element.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is body element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsBodyElement(IHTMLElement e) => (e as IHTMLBodyElement) != null;

        /// <summary>
        /// The body element.
        /// </summary>
        public static IHTMLElementFilter BODY_ELEMENT = new IHTMLElementFilter(IsBodyElement);

        /// <summary>
        /// Returns true if this is an element that triggers a paragraph break.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is block element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsBlockElement(IHTMLElement e) => BlockTagNames[e.tagName] != null;

        /// <summary>
        /// The block elements.
        /// </summary>
        public static IHTMLElementFilter BLOCK_ELEMENTS = new IHTMLElementFilter(IsBlockElement);

        /// <summary>
        /// Returns true if this is one of the header element.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is header element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsHeaderElement(IHTMLElement e) => HeaderTagNames[e.tagName] != null;

        /// <summary>
        /// The header elements.
        /// </summary>
        public static IHTMLElementFilter HEADER_ELEMENTS = new IHTMLElementFilter(IsHeaderElement);

        /// <summary>
        /// Determines whether [is block or table element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is block or table element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsBlockOrTableElement(IHTMLElement e) => IsBlockElement(e) || IsTableElement(e);

        /// <summary>
        /// Determines whether [is block or table cell element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is block or table cell element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsBlockOrTableCellElement(IHTMLElement e) => (BlockTagNames[e.tagName] != null) || IsTableCellElement(e);

        /// <summary>
        /// The block or table cell elements.
        /// </summary>
        public static IHTMLElementFilter BLOCK_OR_TABLE_CELL_ELEMENTS = new IHTMLElementFilter(IsBlockOrTableCellElement);

        /// <summary>
        /// Determines whether [is block or table cell or body element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is block or table cell or body element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsBlockOrTableCellOrBodyElement(IHTMLElement e) => (BlockTagNames[e.tagName] != null) || IsTableCellElement(e) || e is IHTMLBodyElement;

        /// <summary>
        /// Determines whether [is block quote element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is block quote element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsBlockQuoteElement(IHTMLElement e) => e.tagName.ToUpperInvariant() == "BLOCKQUOTE";

        /// <summary>
        /// The blockquote element.
        /// </summary>
        public static IHTMLElementFilter BLOCKQUOTE_ELEMENT = new IHTMLElementFilter(IsBlockQuoteElement);

        /// <summary>
        /// Determines whether [is table cell element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is table cell element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsTableCellElement(IHTMLElement e) => e is IHTMLTableCell;

        /// <summary>
        /// The table cell element.
        /// </summary>
        public static IHTMLElementFilter TABLE_CELL_ELEMENT = new IHTMLElementFilter(IsTableCellElement);

        /// <summary>
        /// Determines whether [is inline element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is inline element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsInlineElement(IHTMLElement e) => InlineTagNames[e.tagName] != null;

        /// <summary>
        /// The inline elements.
        /// </summary>
        public static IHTMLElementFilter INLINE_ELEMENTS = new IHTMLElementFilter(IsInlineElement);

        /// <summary>
        /// Determines whether [is anchor element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is anchor element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsAnchorElement(IHTMLElement e) => e.tagName.Equals("A");

        /// <summary>
        /// The anchor elements.
        /// </summary>
        public static IHTMLElementFilter ANCHOR_ELEMENTS = new IHTMLElementFilter(IsAnchorElement);

        /// <summary>
        /// Determines whether [is paragraph element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is paragraph element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsParagraphElement(IHTMLElement e) => e.tagName.Equals("P");

        /// <summary>
        /// The paragraph elements.
        /// </summary>
        public static IHTMLElementFilter PARAGRAPH_ELEMENTS = new IHTMLElementFilter(IsParagraphElement);

        /// <summary>
        /// Determines whether [is LTR element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is LTR element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsLTRElement(IHTMLElement e) => IsDirElement(e, "ltr");

        /// <summary>
        /// Determines whether [is RTL element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is RTL element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsRTLElement(IHTMLElement e) => IsDirElement(e, "rtl");

        /// <summary>
        /// Determines whether [is dir element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="direction">The direction.</param>
        /// <returns><c>true</c> if [is dir element] [the specified e]; otherwise, <c>false</c>.</returns>
        private static bool IsDirElement(IHTMLElement e, string direction)
        {
            if (IsBlockOrTableCellElement(e))
            {
                var e2 = (IHTMLElement2)e;
                var dir = e2.currentStyle.direction;
                if (dir != null)
                {
                    return string.Compare(dir, direction, StringComparison.OrdinalIgnoreCase) == 0;
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is unordered list element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is unordered list element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsUnorderedListElement(IHTMLElement e) => e.tagName.Equals("UL");

        /// <summary>
        /// The unordered list elements.
        /// </summary>
        public static IHTMLElementFilter UNORDERED_LIST_ELEMENTS = new IHTMLElementFilter(IsUnorderedListElement);

        /// <summary>
        /// Determines whether [is list element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is list element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsListElement(IHTMLElement e) => IsUnorderedListElement(e) || IsOrderedListElement(e);

        /// <summary>
        /// The list elements.
        /// </summary>
        public static IHTMLElementFilter LIST_ELEMENTS = new IHTMLElementFilter(IsListElement);

        /// <summary>
        /// Determines whether [is list item element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is list item element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsListItemElement(IHTMLElement e) => e.tagName.Equals("LI");

        /// <summary>
        /// The list item elements.
        /// </summary>
        public static IHTMLElementFilter LIST_ITEM_ELEMENTS = new IHTMLElementFilter(IsListItemElement);

        /// <summary>
        /// Determines whether [is ordered list element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is ordered list element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsOrderedListElement(IHTMLElement e) => e.tagName.Equals("OL");

        /// <summary>
        /// The ordered list elements.
        /// </summary>
        public static IHTMLElementFilter ORDERED_LIST_ELEMENTS = new IHTMLElementFilter(IsOrderedListElement);

        /// <summary>
        /// Determines whether [is image element] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is image element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsImageElement(IHTMLElement e) => e.tagName.Equals("IMG");

        /// <summary>
        /// The image elements.
        /// </summary>
        public static IHTMLElementFilter IMAGE_ELEMENTS = new IHTMLElementFilter(IsImageElement);

        /// <summary>
        /// Returns true if the specified element triggers something to be visible in the document
        /// when it contains no text.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is visible empty element] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsVisibleEmptyElement(IHTMLElement e) => e.tagName.Equals("HR") || e.tagName.Equals("IMG") || e.tagName.Equals("EMBED") ||
                   e.tagName.Equals("OBJECT") || IsBlockElement(e) || IsTableElement(e);

        /// <summary>
        /// The visible empty elements.
        /// </summary>
        public static IHTMLElementFilter VISIBLE_EMPTY_ELEMENTS = new IHTMLElementFilter(IsVisibleEmptyElement);

        /// <summary>
        /// Requireses the end tag.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RequiresEndTag(string tagName) => NoEndTagRequired[tagName.ToUpper(CultureInfo.InvariantCulture)] == null;

        /// <summary>
        /// Returns true if the specified tag needs an end tag in its text representation.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool RequiresEndTag(IHTMLElement e) => NoEndTagRequired[e.tagName] == null && !(e is IHTMLUnknownElement);

        /// <summary>
        /// The end tag required.
        /// </summary>
        public static IHTMLElementFilter END_TAG_REQUIRED = new IHTMLElementFilter(RequiresEndTag);

        /// <summary>
        /// Returns true if this element is supposed to have an end tag, but IE will still render it correctly
        /// without one.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is end tag optional] [the specified e]; otherwise, <c>false</c>.</returns>
        public static bool IsEndTagOptional(IHTMLElement e) => EndTagOptionalNames[e.tagName] != null;

        /// <summary>
        /// The end tag optional elements.
        /// </summary>
        public static IHTMLElementFilter END_TAG_OPTIONAL_ELEMENTS = new IHTMLElementFilter(IsEndTagOptional);

        /// <summary>
        /// Gets the look up table holding the names of all block-formatted tags.
        /// </summary>
        /// <value>The table tag names.</value>
        private static Hashtable TableTagNames
        {
            get
            {
                if (tableTagNames == null)
                {
                    tableTagNames = new Hashtable();
                    tableTagNames["TABLE"] = "TABLE";
                    tableTagNames["TR"] = "TR";
                    tableTagNames["TD"] = "TD";
                    tableTagNames["TH"] = "TH";
                    tableTagNames["CAPTION"] = "CAPTION";
                    tableTagNames["COL"] = "COL";
                    tableTagNames["COLGROUP"] = "COLGROUP";
                    tableTagNames["THEAD"] = "THEAD";
                    tableTagNames["TBODY"] = "TBODY";
                    tableTagNames["TFOOT"] = "TFOOT";
                }

                return tableTagNames;
            }
        }

        /// <summary>
        /// The table tag names.
        /// </summary>
        private static Hashtable tableTagNames;

        /// <summary>
        /// Look up table holding the names of all block-formatted tags.
        /// </summary>
        /// <value>The block tag names.</value>
        private static Hashtable BlockTagNames
        {
            get
            {
                if (blockTagNames == null)
                {
                    blockTagNames = new Hashtable();
                    blockTagNames["DIV"] = "DIV";
                    blockTagNames["BODY"] = "BODY";
                    blockTagNames["P"] = "P";
                    blockTagNames["PRE"] = "PRE";
                    blockTagNames["BR"] = "BR";
                    blockTagNames["H1"] = "H1";
                    blockTagNames["H2"] = "H2";
                    blockTagNames["H3"] = "H3";
                    blockTagNames["H4"] = "H4";
                    blockTagNames["H5"] = "H5";
                    blockTagNames["H6"] = "H6";
                    blockTagNames["HR"] = "HR";
                    blockTagNames["BLOCKQUOTE"] = "BLOCKQUOTE";
                    ////_blockTagNames["TABLE"] = "TABLE";
                    ////_blockTagNames["TR"] = "TR";
                    ////_blockTagNames["TD"] = "TD";
                    ////_blockTagNames["TH"] = "TH";
                    ////_blockTagNames["UL"] = "UL";
                    ////_blockTagNames["OL"] = "OL";
                    blockTagNames["LI"] = "LI";
                }

                return blockTagNames;
            }
        }

        /// <summary>
        /// The block tag names.
        /// </summary>
        private static Hashtable blockTagNames;

        /// <summary>
        /// Look up table holding the names of all tags displayed "inline" (they don't force line breaks).
        /// </summary>
        /// <value>The inline tag names.</value>
        private static Hashtable InlineTagNames
        {
            get
            {
                if (inlineTagNames == null)
                {
                    inlineTagNames = new Hashtable();
                    inlineTagNames["A"] = "A";
                    inlineTagNames["ABBR"] = "ABBR";
                    inlineTagNames["ACRONYM"] = "ACRONYM";
                    inlineTagNames["APPLET"] = "APPLET";
                    inlineTagNames["B"] = "B";
                    inlineTagNames["BASEFONT"] = "BASEFONT";
                    inlineTagNames["BDO"] = "BDO";
                    inlineTagNames["BIG"] = "BIG";
                    inlineTagNames["BUTTON"] = "BUTTON";
                    inlineTagNames["CITE"] = "CITE";
                    inlineTagNames["CODE"] = "CODE";
                    inlineTagNames["DEL"] = "DEL";
                    inlineTagNames["DFN"] = "DFN";
                    inlineTagNames["EM"] = "EM";
                    inlineTagNames["FONT"] = "FONT";
                    inlineTagNames["I"] = "I";
                    inlineTagNames["IFRAME"] = "IFRAME";
                    inlineTagNames["IMG"] = "IMG";
                    inlineTagNames["INPUT"] = "INPUT";
                    inlineTagNames["INS"] = "INS";
                    inlineTagNames["KBD"] = "KBD";
                    inlineTagNames["LABEL"] = "LABEL";
                    inlineTagNames["MAP"] = "MAP";
                    inlineTagNames["OBJECT"] = "OBJECT";
                    inlineTagNames["Q"] = "Q";
                    inlineTagNames["S"] = "S";
                    inlineTagNames["SAMP"] = "SAMP";
                    inlineTagNames["SCRIPT"] = "SCRIPT";
                    inlineTagNames["SELECT"] = "SELECT";
                    inlineTagNames["SMALL"] = "SMALL";
                    inlineTagNames["SPAN"] = "SPAN";
                    inlineTagNames["STRONG"] = "STRONG";
                    inlineTagNames["STRIKE"] = "STRIKE";
                    inlineTagNames["STYLE"] = "STYLE";
                    inlineTagNames["SUB"] = "SUB";
                    inlineTagNames["SUP"] = "SUP";
                    inlineTagNames["TT"] = "TT";
                    inlineTagNames["TEXTAREA"] = "TEXTAREA";
                    inlineTagNames["U"] = "U";
                    inlineTagNames["VAR"] = "VAR";
                }

                return inlineTagNames;
            }
        }

        /// <summary>
        /// The inline tag names.
        /// </summary>
        private static Hashtable inlineTagNames;

        /// <summary>
        /// Look up table holding the names of all header tags.
        /// </summary>
        /// <value>The header tag names.</value>
        private static Hashtable HeaderTagNames
        {
            get
            {
                if (headerTagNames == null)
                {
                    headerTagNames = new Hashtable();
                    headerTagNames["H1"] = "H1";
                    headerTagNames["H2"] = "H2";
                    headerTagNames["H3"] = "H3";
                    headerTagNames["H4"] = "H4";
                    headerTagNames["H5"] = "H5";
                    headerTagNames["H6"] = "H6";
                }

                return headerTagNames;
            }
        }

        /// <summary>
        /// The header tag names.
        /// </summary>
        private static Hashtable headerTagNames;

        /// <summary>
        /// Look up table holding the names of all tags that render correctly in IE even if they don't have a
        /// corresponding end tag.
        /// </summary>
        /// <value>The end tag optional names.</value>
        private static Hashtable EndTagOptionalNames
        {
            get
            {
                if (endTagOptionalNames == null)
                {
                    endTagOptionalNames = new Hashtable();
                    endTagOptionalNames["P"] = "P";
                    endTagOptionalNames["LI"] = "LI";
                    endTagOptionalNames["H1"] = "H1";
                    endTagOptionalNames["H2"] = "H2";
                    endTagOptionalNames["H3"] = "H3";
                    endTagOptionalNames["H4"] = "H4";
                    endTagOptionalNames["H5"] = "H5";
                    endTagOptionalNames["H6"] = "H6";
                    endTagOptionalNames["TR"] = "TR";
                    endTagOptionalNames["TD"] = "TD";
                    endTagOptionalNames["TH"] = "TH";
                    endTagOptionalNames["COLGROUP"] = "COLGROUP";
                    endTagOptionalNames["THEAD"] = "THEAD";
                    endTagOptionalNames["TBODY"] = "TBODY";
                    endTagOptionalNames["TFOOT"] = "TFOOT";
                    endTagOptionalNames["OPTION"] = "OPTION";
                    endTagOptionalNames["DT"] = "DT";
                    endTagOptionalNames["DD"] = "DD";
                }

                return endTagOptionalNames;
            }
        }

        /// <summary>
        /// The end tag optional names.
        /// </summary>
        private static Hashtable endTagOptionalNames;

        /// <summary>
        /// A table of tags that require don't support end tags.
        /// This operation useful for testing elements that don't need explicit end tags in XHTML.
        /// </summary>
        /// <value>The no end tag required.</value>
        private static Hashtable NoEndTagRequired
        {
            get
            {
                if (m_noEndTagRequired == null)
                {
                    m_noEndTagRequired = new Hashtable();
                    m_noEndTagRequired.Add(HTMLTokens.Area, HTMLTokens.Area);
                    m_noEndTagRequired.Add(HTMLTokens.Base, HTMLTokens.Base);
                    m_noEndTagRequired.Add(HTMLTokens.BaseFont, HTMLTokens.BaseFont);
                    m_noEndTagRequired.Add(HTMLTokens.Br, HTMLTokens.Br);
                    m_noEndTagRequired.Add(HTMLTokens.Col, HTMLTokens.Col);
                    m_noEndTagRequired.Add(HTMLTokens.Embed, HTMLTokens.Embed);
                    m_noEndTagRequired.Add(HTMLTokens.Hr, HTMLTokens.Hr);
                    m_noEndTagRequired.Add(HTMLTokens.Img, HTMLTokens.Img);
                    m_noEndTagRequired.Add(HTMLTokens.Input, HTMLTokens.Input);
                    m_noEndTagRequired.Add(HTMLTokens.Isindex, HTMLTokens.Isindex);
                    m_noEndTagRequired.Add(HTMLTokens.Link, HTMLTokens.Link);
                    m_noEndTagRequired.Add(HTMLTokens.Meta, HTMLTokens.Meta);
                    m_noEndTagRequired.Add(HTMLTokens.Param, HTMLTokens.Param);
                }

                return m_noEndTagRequired;
            }
        }

        /// <summary>
        /// The m no end tag required.
        /// </summary>
        private static Hashtable m_noEndTagRequired;
    }
}
