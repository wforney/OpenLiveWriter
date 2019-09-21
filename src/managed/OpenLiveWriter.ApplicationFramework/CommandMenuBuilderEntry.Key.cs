// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;

    /// <summary>
    /// Class CommandMenuBuilderEntry. This class cannot be inherited.
    /// </summary>
    internal sealed partial class CommandMenuBuilderEntry
    {
        /// <summary>
        /// Class Key.
        /// Implements the <see cref="System.IComparable" />
        /// </summary>
        /// <seealso cref="System.IComparable" />
        private class Key : IComparable
        {
            /// <summary>
            /// The position
            /// </summary>
            private readonly int position;

            /// <summary>
            /// The text
            /// </summary>
            private readonly string text;

            /// <summary>
            /// Initializes a new instance of the <see cref="Key"/> class.
            /// </summary>
            /// <param name="position">The position.</param>
            /// <param name="text">The text.</param>
            public Key(int position, string text)
            {
                this.position = position;
                this.text = text;
            }

            /// <summary>
            /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
            /// </summary>
            /// <param name="obj">An object to compare with this instance.</param>
            /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance precedes <paramref name="obj" /> in the sort order. Zero This instance occurs in the same position in the sort order as <paramref name="obj" />. Greater than zero This instance follows <paramref name="obj" /> in the sort order.</returns>
            public int CompareTo(object obj)
            {
                var other = (Key)obj;

                var result = this.position - other.position;
                return result == 0 ? this.text.CompareTo(other.text) : result;
            }

            /// <summary>
            /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals(object obj)
            {
                var other = (Key)obj;
                return this.position == other.position && this.text.Equals(other.text);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public override int GetHashCode() => this.position + this.text.GetHashCode();

        }
    }
}
