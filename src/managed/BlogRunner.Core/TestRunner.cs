// <copyright file="TestRunner.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using BlogRunner.Core.Config;
    using OpenLiveWriter.BlogClient;
    using OpenLiveWriter.BlogClient.Clients;
    using OpenLiveWriter.PostEditor.Configuration;
    using Blog = BlogRunner.Core.Config.Blog;

    /// <summary>
    /// The test runner class.
    /// </summary>
    public class TestRunner
    {
        private readonly IEnumerable<Test> tests;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunner"/> class.
        /// </summary>
        /// <param name="tests">The tests.</param>
        public TestRunner(IEnumerable<Test> tests) => this.tests = tests;

        /// <summary>
        /// Runs the tests.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="blog">The blog.</param>
        /// <param name="providerEl">The provider element.</param>
        public void RunTests(Provider provider, Blog blog, XmlElement providerEl)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (blog == null)
            {
                throw new ArgumentNullException(nameof(blog));
            }

            using (new BlogClientUIContextSilentMode()) // suppress prompting for credentials
            {
                var credentials = new TemporaryBlogCredentials
                {
                    Username = blog.Username,
                    Password = blog.Password,
                };
                var credentialsAccessor = new BlogCredentialsAccessor(Guid.NewGuid().ToString(), credentials);
                var client = BlogClientManager.CreateClient(provider.ClientType, blog.ApiUrl, credentialsAccessor);

                if (blog.BlogId == null)
                {
                    var blogs = client.GetUsersBlogs();
                    if (blogs.Length == 1)
                    {
                        blog.BlogId = blogs[0].Id;
                        credentialsAccessor = new BlogCredentialsAccessor(blog.BlogId, credentials);
                        client = BlogClientManager.CreateClient(provider.ClientType, blog.ApiUrl, credentialsAccessor);
                    }
                }

                foreach (var test in this.tests)
                {
                    try
                    {
                        Console.WriteLine($"Running test {test.ToString()}");
                        test.DoTest(blog, client, providerEl);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Console.Error.WriteLine($"Error: Test {test.GetType().Name} failed for provider {provider.Name}:");
                        Console.Error.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
