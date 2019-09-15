// <copyright file="Test.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;
    using System.Xml;
    using BlogRunner.Core.Config;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The test class.
    /// </summary>
    public abstract class Test
    {
        /// <summary>
        /// The constant for the string "Yes".
        /// </summary>
        protected const string YES = "Yes";

        /// <summary>
        /// The constant for the string "No".
        /// </summary>
        protected const string NO = "No";

        /// <summary>
        /// The namespace blogrunner.
        /// </summary>
        private const string NamespaceBlogrunner = "http://writer.live.com/blogrunner/2007";

        /// <summary>
        /// Initializes static members of the <see cref="Test"/> class.
        /// </summary>
        static Test() => CleanUpPosts = true;

        /// <summary>
        /// The timeout action delegate.
        /// </summary>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected delegate bool TimeoutAction();

        /// <summary>
        /// Gets or sets a value indicating whether to clean up posts.
        /// </summary>
        protected static bool CleanUpPosts { get; set; }

        /// <summary>
        /// Does the test.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="results">The results.</param>
        public abstract void DoTest(Blog blog, IBlogClient blogClient, ITestResults results);

        /// <summary>
        /// Does the test.
        /// </summary>
        /// <param name="blog">The blog.</param>
        /// <param name="blogClient">The blog client.</param>
        /// <param name="providerEl">The provider element.</param>
        public virtual void DoTest(Blog blog, IBlogClient blogClient, XmlElement providerEl)
        {
            var results = new TestResultImpl();
            this.DoTest(blog, blogClient, results);
            results.ForEach(
                (string key, string value) =>
                {
                    var optionsEl = (XmlElement)providerEl.SelectSingleNode("options");
                    if (optionsEl == null)
                    {
                        optionsEl = providerEl.OwnerDocument.CreateElement("options");
                        providerEl.AppendChild(optionsEl);
                    }

                    var el = (XmlElement)optionsEl.SelectSingleNode(key);
                    if (el == null)
                    {
                        el = providerEl.OwnerDocument.CreateElement(key);
                        optionsEl.AppendChild(el);
                    }

                    if (!el.HasAttribute("readonly", NamespaceBlogrunner))
                    {
                        el.InnerText = value;
                    }
                });
        }

        /// <summary>
        /// Retries action until either it returns true or the timeout time elapses.
        /// </summary>
        /// <param name="timeout">The timeout time span.</param>
        /// <param name="action">The timeout action.</param>
        protected static void RetryUntilTimeout(TimeSpan timeout, TimeoutAction action)
        {
            var due = DateTime.UtcNow + timeout;
            do
            {
                if (action())
                {
                    return;
                }
            }
            while (DateTime.UtcNow < due);
            throw new TimeoutException("The operation has timed out");
        }
    }
}
