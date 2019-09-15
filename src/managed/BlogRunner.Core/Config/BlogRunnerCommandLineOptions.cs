// <copyright file="BlogRunnerCommandLineOptions.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core.Config
{
    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// Class BlogRunnerCommandLineOptions.
    /// Implements the <see cref="CommandLineOptions" />.
    /// </summary>
    /// <seealso cref="CommandLineOptions" />
    public class BlogRunnerCommandLineOptions : CommandLineOptions
    {
        /// <summary>
        /// The option providers.
        /// </summary>
        public const string OptionProviders = "providers";

        /// <summary>
        /// The option configuration.
        /// </summary>
        public const string OptionConfig = "config";

        /// <summary>
        /// The option output.
        /// </summary>
        public const string OptionOutput = "output";

        /// <summary>
        /// The option verbose.
        /// </summary>
        public const string OptionVerbose = "verbose";

        /// <summary>
        /// The option pause.
        /// </summary>
        public const string OptionPause = "pause";

        /// <summary>
        /// The option errorlog.
        /// </summary>
        public const string OptionErrorLog = "errorlog";

        /// <summary>
        /// Initializes a new instance of the <see cref="BlogRunnerCommandLineOptions"/> class.
        /// </summary>
        public BlogRunnerCommandLineOptions()
            : base(
                  false,
                  0,
                  int.MaxValue,
                  new ArgSpec(OptionProviders, ArgSpec.Options.Required, "Path to BlogProviders.xml"),
                  new ArgSpec(OptionConfig, ArgSpec.Options.Required, "Path to BlogRunner config file"),
                  new ArgSpec(OptionOutput, ArgSpec.Options.Required, "Path to output file"),
                  new ArgSpec(OptionVerbose, ArgSpec.Options.Flag, "Verbose logging"),
                  new ArgSpec(OptionErrorLog, ArgSpec.Options.Default, "Log errors to specified file"),
                  new ArgSpec(OptionPause, ArgSpec.Options.Flag, "Pause before exiting"))
        {
        }
    }
}
