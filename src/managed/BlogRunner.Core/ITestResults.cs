// <copyright file="ITestResults.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    /// <summary>
    /// The test results interface.
    /// </summary>
    public interface ITestResults
    {
        /// <summary>
        /// Adds the result.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        void AddResult(string key, string value);
    }
}
