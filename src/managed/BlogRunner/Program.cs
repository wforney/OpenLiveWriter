// <copyright file="Program.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    using BlogRunner.Core;
    using BlogRunner.Core.Config;
    using BlogRunner.Core.Tests;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;

    /// <summary>
    /// Class Program.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// The test filter delegate.
        /// </summary>
        /// <param name="tests">The tests.</param>
        /// <returns>A filtered test array.</returns>
        private delegate Test[] TestFilter(params Test[] tests);

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The return code.</returns>
        public static int Main(string[] args)
        {
            try
            {
                ChangeErrorColors(ConsoleColor.Red);

                var options = new BlogRunnerCommandLineOptions();
                options.Parse(args, true);

                try
                {
                    if (options.GetFlagValue(BlogRunnerCommandLineOptions.OptionVerbose, false))
                    {
                        Debug.Listeners.Add(new ConsoleTraceListener(true));
                    }

                    var providersPath = Path.GetFullPath((string)options.GetValue(BlogRunnerCommandLineOptions.OptionProviders, null));
                    var configPath = Path.GetFullPath((string)options.GetValue(BlogRunnerCommandLineOptions.OptionConfig, null));
                    var outputPath = Path.GetFullPath((string)options.GetValue(BlogRunnerCommandLineOptions.OptionOutput, providersPath));
                    var providerIds = new List<string>(options.UnnamedArgs);
                    var errorLogPath = (string)options.GetValue(BlogRunnerCommandLineOptions.OptionErrorLog, null);
                    if (errorLogPath != null)
                    {
                        errorLogPath = Path.GetFullPath(errorLogPath);
#pragma warning disable CA2000 // Dispose objects before losing scope
                        Console.SetError(
                            new CompositeTextWriter(
                                Console.Error,
                                File.CreateText(errorLogPath)));
#pragma warning restore CA2000 // Dispose objects before losing scope
                    }

                    ApplicationEnvironment.Initialize(
                        Assembly.GetExecutingAssembly(),
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                            @"\Windows Live\Writer\"));
                    ApplicationDiagnostics.VerboseLogging = true;

                    var config = Config.Load(configPath, providersPath);
                    var providers = new XmlDocument
                    {
                        XmlResolver = null
                    };
                    var xmlContent = File.ReadAllText(providersPath);
                    var sreader = new StringReader(xmlContent);
                    using (var reader = XmlReader.Create(sreader, new XmlReaderSettings() { XmlResolver = null }))
                    {
                        providers.Load(reader);

                        foreach (XmlElement provider in providers.SelectNodes("/providers/provider"))
                        {
                            var providerId = provider.SelectSingleNode("id/text()").Value;
                            var clientType = provider.SelectSingleNode("clientType/text()").Value;

                            if (providerIds.Count > 0 && !providerIds.Contains(providerId))
                            {
                                continue;
                            }

                            var p = config.GetProviderById(providerId);
                            if (p == null)
                            {
                                continue;
                            }

                            p.ClientType = clientType;

                            var results = new TestResultImpl();

                            var b = p.Blog;
                            if (b != null)
                            {
                                Console.Write(provider.SelectSingleNode("name/text()").Value);
                                Console.Write($" {Properties.Resources.LeftParenthesis}");
                                Console.Write(b.HomepageUrl);
                                Console.WriteLine(Properties.Resources.RightParenthesis);

                                var tests = new List<Test>();
                                AddTests(tests, delegate (Test[] testArr)
                                                    {
                                                        const string TestString = "Test";
                                                        for (var i = 0; i < testArr.Length; i++)
                                                        {
                                                            var t = testArr[i];
                                                            var testName = t.GetType().Name;
                                                            if (testName.EndsWith(TestString, StringComparison.OrdinalIgnoreCase))
                                                            {
                                                                testName = testName.Substring(0, testName.Length - 4);
                                                            }

                                                            if (p.Exclude != null && Array.IndexOf(p.Exclude, testName) >= 0)
                                                            {
                                                                testArr[i] = null;
                                                            }
                                                        }

                                                        return ArrayHelper.Compact(testArr);
                                                    });
                                var tr = new TestRunner(tests);
                                tr.RunTests(p, b, provider);
                            }
                        }

                        using (var xw = new XmlTextWriter(outputPath, Encoding.UTF8))
                        {
                            xw.Formatting = Formatting.Indented;
                            xw.Indentation = 1;
                            xw.IndentChar = '\t';
                            providers.WriteTo(xw);
                        }
                    }

                    return 0;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    Console.Error.WriteLine(e.ToString());
                    return 1;
                }
                finally
                {
                    if (options.GetFlagValue(BlogRunnerCommandLineOptions.OptionPause, false))
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.Write(Properties.Resources.PressAnyKeyToContinue);
                        Console.ReadKey(true);
                    }
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Console.Error.WriteLine(e.ToString());
                return 1;
            }
        }

        /// <summary>
        /// Adds the tests.
        /// </summary>
        /// <param name="tests">The tests.</param>
        /// <param name="filter">The filter.</param>
        private static void AddTests(List<Test> tests, TestFilter filter)
        {
            // New tests go here
            tests.AddRange(
                filter(
                    new SupportsMultipleCategoriesTest(),
                    new SupportsPostAsDraftTest(),
                    new SupportsFuturePostTest(),
                    new SupportsEmptyTitlesTest()));

            tests.Add(CreateCompositePostTest(
                filter,
                new TitleEncodingTest(),
                new SupportsEmbedsTest(),
                new SupportsScriptsTest()));
        }

        /// <summary>
        /// Creates the composite post test.
        /// </summary>
        /// <param name="filter">The filter.</param>
        /// <param name="tests">The tests.</param>
        /// <returns>A test.</returns>
        private static Test CreateCompositePostTest(TestFilter filter, params PostTest[] tests) =>
            new CompositePostTest(
                (PostTest[])ArrayHelper.Narrow(
                                 filter(tests),
                                 typeof(PostTest)));

        /// <summary>
        /// Changes the error colors.
        /// </summary>
        /// <param name="color">The color.</param>
#pragma warning disable CA2000 // Dispose objects before losing scope
        private static void ChangeErrorColors(ConsoleColor color) => Console.SetError(new ColorChangeTextWriter(Console.Error, color));
#pragma warning restore CA2000 // Dispose objects before losing scope
    }
}
