// <copyright file="ITestResults.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    /// <summary>
    /// The test result implementation class.
    /// Implements the <see cref="BlogRunner.Core.ITestResults" />
    /// </summary>
    /// <seealso cref="BlogRunner.Core.ITestResults" />
    public class TestResultImpl : ITestResults
    {
        public delegate void Func(string key, string val);

        private Dictionary<string, string> results = new Dictionary<string, string>();

        /// <summary>
        /// Adds the result.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddResult(string key, string value) => this.results.Add(key, value);

        /// <summary>
        /// Fors the each.
        /// </summary>
        /// <param name="func">The function.</param>
        public void ForEach(Func func)
        {
            var keys = new List<string>(this.results.Keys);
            keys.Sort(StringComparer.CurrentCultureIgnoreCase);
            foreach (var key in keys)
            {
                func(key, this.results[key]);
            }
        }

        /// <summary>
        /// Dumps the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        public void Dump(TextWriter output)
        {
            this.ForEach(delegate (string key, string value)
            {
                output.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", key, value));
            });
        }
    }
}
