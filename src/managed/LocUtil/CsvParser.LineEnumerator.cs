// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public partial class CsvParser
    {
        /// <summary>
        /// Class LineEnumerator.
        /// Implements the <see cref="System.Collections.IEnumerator{Array{string}}" />
        /// </summary>
        /// <seealso cref="System.Collections.IEnumerator{Array{string}}" />
        private class LineEnumerator : IEnumerator<string[]>
        {
            /// <summary>
            /// The parent
            /// </summary>
            private readonly CsvParser parent;

            /// <summary>
            /// Initializes a new instance of the <see cref="LineEnumerator"/> class.
            /// </summary>
            /// <param name="parent">The parent.</param>
            public LineEnumerator(CsvParser parent) => this.parent = parent;

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns><see langword="true" /> if the enumerator was successfully advanced to the next element; <see langword="false" /> if the enumerator has passed the end of the collection.</returns>
            public bool MoveNext()
            {
                start:
                if (this.Current != null)
                {
                    if (!this.parent.NextLine())
                    {
                        return false;
                    }
                }

                var words = new List<string>();
                string word;
                while (null != (word = this.parent.NextWord()))
                {
                    words.Add(word);
                }

                this.Current = words.ToArray();
                if (this.Current.Length == 0)
                {
                    goto start;
                }

                return true;
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="System.NotSupportedException"></exception>
            public void Reset() => throw new NotSupportedException();

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            /// <exception cref="System.NotImplementedException"></exception>
            public void Dispose() => throw new NotImplementedException();

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The current.</value>
            public string[] Current { get; private set; }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value>The current.</value>
            object IEnumerator.Current => this.Current;
        }
    }
}
