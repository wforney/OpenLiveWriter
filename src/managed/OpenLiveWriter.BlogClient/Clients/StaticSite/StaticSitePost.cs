// <copyright file="StaticSitePost.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The StaticSitePost class.
    /// Implements the <see cref="StaticSiteItem" />
    /// </summary>
    /// <seealso cref="StaticSiteItem" />
    public class StaticSitePost : StaticSiteItem
    {
        /// <summary>
        /// The filename slug regex
        /// </summary>
        /// <remarks>
        /// Matches the published slug out of a on-disk post
        /// 2014-02-02-test.html -> test
        /// _posts\2014-02-02-my-post-test.html -> my-post-test
        /// </remarks>
        private static readonly Regex FilenameSlugRegex = new Regex(
            @"^(?:(?:.*?)(?:\\|\/))*(?:\d\d\d\d-\d\d-\d\d-)(.*?)\" + StaticSiteItem.PublishFileExtension + "$");

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSitePost"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public StaticSitePost(StaticSiteConfig config)
            : base(config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSitePost"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="blogPost">The blog post.</param>
        public StaticSitePost(StaticSiteConfig config, BlogPost blogPost)
            : base(config, blogPost)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSitePost"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="isDraft">if set to <c>true</c> [is draft].</param>
        public StaticSitePost(StaticSiteConfig config, BlogPost blogPost, bool isDraft)
            : base(config, blogPost, isDraft)
        {
        }

        /// <inheritdoc />
        public override string FilePathById
        {
            get
            {
                if (this.FilePathByIdentifier != null)
                {
                    return this.FilePathByIdentifier;
                }

                var foundFile = Directory.GetFiles(
                    Path.Combine(this.SiteConfig.LocalSitePath, this.ItemRelativeDir),
                    "*.html").Where(
                    postFile =>
                        {
                            try
                            {
                                var post = StaticSitePost.LoadFromFile(
                                    Path.Combine(
                                        this.SiteConfig.LocalSitePath,
                                        this.ItemRelativeDir,
                                        postFile),
                                    this.SiteConfig);
                                if (post.Id == this.Id)
                                {
                                    return true;
                                }
                            }
                            catch
                            {
                                // ignored
                            }

                            return false;
                        }).DefaultIfEmpty(null).FirstOrDefault();
                return this.FilePathByIdentifier = foundFile == null
                                                ? null
                                                : Path.Combine(
                                                    this.SiteConfig.LocalSitePath,
                                                    this.ItemRelativeDir,
                                                    foundFile);
            }

            protected set => this.FilePathByIdentifier = value;
        }

        /// <summary>
        /// We currently do not take configuration for specifying a post path format
        /// </summary>
        /// <value>The site path.</value>
        public override string SitePath => throw new NotImplementedException();

        /// <summary>
        /// Get all valid posts in PostsPath
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="includeDrafts">if set to <c>true</c> [include drafts].</param>
        /// <returns>An IEnumerable of StaticSitePost</returns>
        public static IEnumerable<StaticSiteItem> GetAllPosts(StaticSiteConfig config, bool includeDrafts) =>
            Directory.GetFiles(Path.Combine(config.LocalSitePath, config.PostsPath), "*.html")
                     .Select(
                          fileName => Path.Combine(
                              config.LocalSitePath,
                              config.PostsPath,
                              fileName)) // Create full paths
                     .Concat(
                          includeDrafts && config.DraftsEnabled
                              ? // Collect drafts if they're enabled
                              Directory.GetFiles(Path.Combine(config.LocalSitePath, config.DraftsPath), "*.html")
                                       .Select(
                                            fileName => Path.Combine(
                                                config.LocalSitePath,
                                                config.DraftsPath,
                                                fileName)) // Create full paths
                              : Array.Empty<string>()) // Drafts are not enabled or were not requested
                      .Select(
                          postFile =>
                              {
                                  try
                                  {
                                      return StaticSitePost.LoadFromFile(postFile, config);
                                  }
                                  catch
                                  {
                                      return null;
                                  }
                              }).Where(p => p != null);

        /// <summary>
        /// Gets the post by identifier.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="StaticSiteItem"/>.</returns>
        public static StaticSiteItem GetPostById(StaticSiteConfig config, string id) =>
            StaticSitePost.GetAllPosts(config, true).Where(post => post.Id == id).DefaultIfEmpty(null).FirstOrDefault();

        /// <summary>
        /// Load published post from a specified file path
        /// </summary>
        /// <param name="postFilePath">Path to published post file</param>
        /// <param name="config">StaticSiteConfig to instantiate post with</param>
        /// <returns>A loaded StaticSitePost</returns>
        public static StaticSitePost LoadFromFile(string postFilePath, StaticSiteConfig config)
        {
            var post = new StaticSitePost(config);
            post.LoadFromFile(postFilePath);
            return post;
        }

        /// <summary>
        /// Gets filename based on slug with prepended date
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>File name with prepended date</returns>
        protected override string GetFileNameForProvidedSlug(string slug) =>
            $"{this.DatePublished.ToString("yyyy-MM-dd")}-{slug}{StaticSiteItem.PublishFileExtension}";

        /// <summary>
        /// Gets a path based on file name and posts path
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>Path containing posts path</returns>
        protected override string GetFilePathForProvidedSlug(string slug) =>
            Path.Combine(this.SiteConfig.LocalSitePath, this.ItemRelativeDir, this.GetFileNameForProvidedSlug(slug));

        /// <summary>
        /// Gets the name of the slug from publish file.
        /// </summary>
        /// <param name="publishFileName">Name of the publish file.</param>
        /// <returns>The slug.</returns>
        protected override string GetSlugFromPublishFileName(string publishFileName) =>
            StaticSitePost.FilenameSlugRegex.Match(publishFileName).Groups[1].Value;
    }
}
