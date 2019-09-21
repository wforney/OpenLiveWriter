// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System.Collections;
    using System.Drawing;
    using System.Resources;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class CommandResourceLoader.
    /// </summary>
    public partial class CommandResourceLoader
    {
        /// <summary>
        /// The resourced property loader
        /// </summary>
        private static readonly ResourcedPropertyLoader resourcedPropertyLoader = new ResourcedPropertyLoader(
            typeof(Command),
            new ResourceManager("OpenLiveWriter.Localization.Properties", typeof(CommandId).Assembly),
            new ResourceManager("OpenLiveWriter.Localization.PropertiesNonLoc", typeof(CommandId).Assembly));

        /// <summary>
        /// The command main menu paths
        /// </summary>
        private static readonly IDictionary commandMainMenuPaths;

        /// <summary>
        /// Initializes static members of the <see cref="CommandResourceLoader"/> class.
        /// </summary>
        static CommandResourceLoader()
        {
            using (new QuickTimer("Parse main menu structure"))
            {
                commandMainMenuPaths = new MainMenuParser(
                    resourcedPropertyLoader.LocalizedResources,
                    resourcedPropertyLoader.NonLocalizedResources).Parse();
            }
        }

        /// <summary>
        /// Applies the resources.
        /// </summary>
        /// <param name="command">The command.</param>
        public static void ApplyResources(Command command) => resourcedPropertyLoader.ApplyResources(command, command.Identifier);

        /// <summary>
        /// Gets the missing large.
        /// </summary>
        /// <value>The missing large.</value>
        public static Bitmap MissingLarge => LoadCommandBitmap("Missing", "LargeImage");

        /// <summary>
        /// Gets the missing small.
        /// </summary>
        /// <value>The missing small.</value>
        public static Bitmap MissingSmall => LoadCommandBitmap("Missing", "SmallImage");

        /// <summary>
        /// Loads the command bitmap.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="bitmapType">Type of the bitmap.</param>
        /// <returns>Bitmap.</returns>
        public static Bitmap LoadCommandBitmap(string commandId, string bitmapType) => ResourceHelper.LoadBitmap($"{commandId}_{bitmapType}");
    }
}
