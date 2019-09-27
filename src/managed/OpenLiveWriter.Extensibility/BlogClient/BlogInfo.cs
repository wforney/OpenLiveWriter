// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    public class BlogInfo
    {
        public BlogInfo(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public BlogInfo(string id, string name, string homepageUrl)
        {
            Id = id;
            Name = name;
            HomepageUrl = homepageUrl;
        }
        public readonly string Id = string.Empty;
        public readonly string Name = string.Empty;

        /// <summary>
        /// Note: Specification of this field is not required, do not depend
        /// upon it having a valid URL
        /// </summary>
        public readonly string HomepageUrl = string.Empty;

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return (obj as BlogInfo).Id == Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public class Comparer : IComparer
        {
            public int Compare(object x, object y)
            {
                return string.Compare((x as BlogInfo).Name, (y as BlogInfo).Name, StringComparison.Ordinal);
            }
        }
    }
}
