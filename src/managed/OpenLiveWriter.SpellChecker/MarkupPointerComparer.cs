namespace OpenLiveWriter.SpellChecker
{
    using System.Collections;
    using System.Collections.Generic;
    using Mshtml;

    /// <summary>
    /// The MarkupPointerComparer class.
    /// Implements the <see cref="System.Collections.IComparer" />
    /// Implements the <see cref="System.Collections.Generic.IComparer{OpenLiveWriter.Mshtml.IMarkupPointerRaw}" />
    /// </summary>
    /// <seealso cref="System.Collections.IComparer" />
    /// <seealso cref="System.Collections.Generic.IComparer{OpenLiveWriter.Mshtml.IMarkupPointerRaw}" />
    internal class MarkupPointerComparer : IComparer, IComparer<IMarkupPointerRaw>
    {
        /// <inheritdoc />
        public int Compare(object x, object y)
        {
            var a = (IMarkupPointerRaw)x;
            var b = (IMarkupPointerRaw)y;
            return this.Compare(a, b);
        }

        /// <summary>
        /// Compares the specified a.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>System.Int32.</returns>
        public int Compare(IMarkupPointerRaw a, IMarkupPointerRaw b)
        {
            if (a == null)
            {
                return 1;
            }

            a.IsEqualTo(b, out var test);
            if (test)
            {
                return 0;
            }

            a.IsLeftOf(b, out test);
            if (test)
            {
                return -1;
            }

            return 1;
        }
    }
}
