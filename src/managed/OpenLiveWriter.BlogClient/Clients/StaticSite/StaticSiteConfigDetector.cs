// <copyright file="StaticSiteConfigDetector.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System;
    using System.IO;
    using System.Linq;

    using OpenLiveWriter.CoreServices;

    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// The StaticSiteConfigDetector class.
    /// </summary>
    public class StaticSiteConfigDetector
    {
        /// <summary>
        /// The image directory candidates
        /// </summary>
        private static readonly string[] ImgDirCandidates =
            {
                "images", "image", "img", "assets/img", "assets/images"
            };

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly StaticSiteConfig config;

        /// <summary>
        /// The local site path
        /// </summary>
        private readonly string localSitePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteConfigDetector"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public StaticSiteConfigDetector(StaticSiteConfig config)
        {
            this.config = config;
            this.localSitePath = config.LocalSitePath;
        }

        /// <summary>
        /// Attempts the automatic detect.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns><c>true</c> if detected, <c>false</c> otherwise.</returns>
        public static bool AttemptAutoDetect(StaticSiteConfig config) =>
            new StaticSiteConfigDetector(config).DoDetect();

        /// <summary>
        /// Does the detect.
        /// </summary>
        /// <returns><c>true</c> if detected, <c>false</c> otherwise.</returns>
        public bool DoDetect() =>

            // More detection methods would be added here for the various static site generators
            this.DoJekyllDetect();

        /// <summary>
        /// Attempt detection for a Jekyll project
        /// </summary>
        /// <returns>True if Jekyll detection succeeded</returns>
        public bool DoJekyllDetect()
        {
            // First, check for a Gemfile specifying jekyll
            var gemfilePath = Path.Combine(this.localSitePath, "Gemfile");
            if (!File.Exists(gemfilePath))
            {
                return false;
            }

            if (!File.ReadAllText(gemfilePath).Contains("jekyll"))
            {
                return false;
            }

            // Find the config file
            var configPath = Path.Combine(this.localSitePath, "_config.yml");
            if (!File.Exists(configPath))
            {
                return false;
            }

            // Jekyll site detected, set defaults
            // Posts path is almost always _posts, check that it exists before setting
            if (Directory.Exists(Path.Combine(this.localSitePath, "_posts")))
            {
                this.config.PostsPath = "_posts";
            }

            // Pages enabled and in root dir
            this.config.PagesEnabled = true;
            this.config.PagesPath = ".";

            // If a _site dir exists, assume site is locally built
            if (Directory.Exists(Path.Combine(this.localSitePath, "_site")))
            {
                this.config.BuildingEnabled = true;
                this.config.OutputPath = "_site";
            }

            // Check for all possible image upload directories
            foreach (var dir in StaticSiteConfigDetector.ImgDirCandidates)
            {
                if (Directory.Exists(Path.Combine(this.localSitePath, dir)))
                {
                    this.config.ImagesEnabled = true;
                    this.config.ImagesPath = dir;
                    break;
                }
            }

            var yaml = new YamlStream();
            try
            {
                // Attempt to load the YAML document
                yaml.Load(new StringReader(File.ReadAllText(configPath)));
                var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;

                // Fill values from config
                // Site title
                var titleNode = mapping.Where(kv => kv.Key.ToString() == "title").ToList();
                if (titleNode.Any())
                {
                    this.config.SiteTitle = titleNode.First().Value.ToString();
                }

                // Homepage
                // Check for url node first
                var urlNode = mapping.Where(kv => kv.Key.ToString() == "url").ToList();
                if (urlNode.Any())
                {
                    this.config.SiteUrl = urlNode.First().Value.ToString();

                    // Now check for base url to apply to url
                    var baseurlNode = mapping.Where(kv => kv.Key.ToString() == "baseurl").ToList();

                    // Combine base url
                    if (baseurlNode.Any())
                    {
                        this.config.SiteUrl = UrlHelper.UrlCombine(
                            this.config.SiteUrl,
                            baseurlNode.First().Value.ToString());
                    }
                }

                // Destination
                // If specified, local site building can be safely assumed to be enabled
                var destinationNode = mapping.Where(kv => kv.Key.ToString() == "destination").ToList();
                if (destinationNode.Any())
                {
                    this.config.BuildingEnabled = true;
                    this.config.OutputPath = destinationNode.First().Value.ToString();
                }
            }
            catch (Exception)
            {
                // YAML may be malformed, defaults are still set from above so return true
            }

            return true;
        }
    }
}
