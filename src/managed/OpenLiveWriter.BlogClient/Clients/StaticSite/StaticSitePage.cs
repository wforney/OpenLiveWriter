// <copyright file="StaticSitePage.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The StaticSitePage class.
    /// Implements the <see cref="StaticSiteItem" />
    /// </summary>
    /// <seealso cref="StaticSiteItem" />
    public class StaticSitePage : StaticSiteItem
    {
        /// <summary>
        /// The filename slug regex
        /// </summary>
        /// <remarks>
        /// Matches the published slug out of a on-disk page
        /// page-test_sub-page-test.html -> sub-page-test
        /// 0001-01-01-page-test.html -> 0001-01-01-page-test
        /// _pages\my-page.html -> my-page
        /// </remarks>
        private static readonly Regex FilenameSlugRegex =
            new Regex(@"^(?:(?:.*?)(?:\\|\/|_))*(.*?)\" + StaticSiteItem.PublishFileExtension + "$");

        /// <summary>
        /// The parent crawl maximum levels
        /// </summary>
        private static readonly int ParentCrawlMaxLevels = 32;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSitePage"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public StaticSitePage(StaticSiteConfig config)
            : base(config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSitePage"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="blogPost">The blog post.</param>
        public StaticSitePage(StaticSiteConfig config, BlogPost blogPost)
            : base(config, blogPost)
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
                    Path.Combine(this.SiteConfig.LocalSitePath, this.SiteConfig.PagesPath),
                    "*.html").Where(
                    pageFile =>
                        {
                            try
                            {
                                var page = StaticSitePage.LoadFromFile(
                                    Path.Combine(
                                        this.SiteConfig.LocalSitePath,
                                        this.SiteConfig.PagesPath,
                                        pageFile),
                                    this.SiteConfig);
                                if (page.Id == this.Id)
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
                                                    this.SiteConfig.PagesPath,
                                                    foundFile);
            }

            protected set => this.FilePathByIdentifier = value;
        }

        /// <summary>
        /// Gets the front matter.
        /// </summary>
        /// <value>The front matter.</value>
        public override StaticSiteItemFrontMatter FrontMatter
        {
            get
            {
                var fm = base.FrontMatter;
                fm.Permalink = this.SitePath;
                return fm;
            }
        }

        /// <summary>
        /// Gets the page information.
        /// </summary>
        /// <value>The page information.</value>
        public PageInfo PageInfo =>
            new PageInfo(this.BlogPost.Id, this.BlogPost.Title, this.DatePublished, this.BlogPost.PageParent?.Id);

        /// <summary>
        /// Get the site path ("permalink") for the published page
        /// e.g. /about/, /page/sub-page/
        /// </summary>
        /// <value>The site path.</value>
        public override string SitePath
        {
            get
            {
                // Get slug for all parent posts and prepend
                var parentSlugs = string.Join("/", this.GetParentSlugs());
                if (parentSlugs != string.Empty)
                {
                    parentSlugs += "/"; // If parent slugs were collected, append slug separator
                }

                return $"/{parentSlugs}{this.Slug}/"; // parentSlugs will include tailing slash
            }
        }

        /// <summary>
        /// Get all valid pages in PagesPath
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>An IEnumerable of StaticSitePage</returns>
        public static IEnumerable<StaticSitePage> GetAllPages(StaticSiteConfig config) =>
            Directory.GetFiles(Path.Combine(config.LocalSitePath, config.PagesPath), "*.html").Select(
                pageFile =>
                    {
                        try
                        {
                            return StaticSitePage.LoadFromFile(
                                Path.Combine(config.LocalSitePath, config.PagesPath, pageFile),
                                config);
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(p => p != null);

        /// <summary>
        /// Gets the page by identifier.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="id">The identifier.</param>
        /// <returns>A <see cref="StaticSitePage"/>.</returns>
        public static StaticSitePage GetPageById(StaticSiteConfig config, string id) =>
            StaticSitePage.GetAllPages(config).Where(page => page.Id == id).DefaultIfEmpty(null).FirstOrDefault();

        /// <summary>
        /// Load published page from a specified file path
        /// </summary>
        /// <param name="pageFilePath">Path to published page file</param>
        /// <param name="config">StaticSiteConfig to instantiate page with</param>
        /// <returns>A loaded StaticSitePage</returns>
        public static StaticSitePage LoadFromFile(string pageFilePath, StaticSiteConfig config)
        {
            var page = new StaticSitePage(config);
            page.LoadFromFile(pageFilePath);
            return page;
        }

        /// <summary>
        /// Resolves the parent.
        /// </summary>
        /// <returns>A <see cref="StaticSitePage"/>.</returns>
        public StaticSitePage ResolveParent()
        {
            if (this.BlogPost.PageParent.IsEmpty)
            {
                return null;
            }

            // Attempt to locate and load parent
            var parent = StaticSitePage.GetPageById(this.SiteConfig, this.BlogPost.PageParent.Id);

            this.BlogPost.PageParent = parent == null
                ? PostIdAndNameField.Empty // Parent not found, set PageParent to empty
                : new PostIdAndNameField(parent.Id, parent.BlogPost.Title); // Populate Name field

            return parent;
        }

        /// <summary>
        /// Gets on-disk filename based on slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>File name with prepended date</returns>
        protected override string GetFileNameForProvidedSlug(string slug)
        {
            var parentSlugs = string.Join("_", this.GetParentSlugs());
            if (parentSlugs != string.Empty)
            {
                parentSlugs += "_"; // If parent slugs were collected, append slug separator
            }

            return $"{parentSlugs}{slug}{StaticSiteItem.PublishFileExtension}";
        }

        /// <summary>
        /// Gets a path based on file name and posts path
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>Path containing pages path</returns>
        protected override string GetFilePathForProvidedSlug(string slug) =>
            Path.Combine(
                this.SiteConfig.LocalSitePath,
                this.SiteConfig.PagesPath,
                this.GetFileNameForProvidedSlug(slug));

        /// <summary>
        /// Gets the name of the slug from publish file.
        /// </summary>
        /// <param name="publishFileName">Name of the publish file.</param>
        /// <returns>The slug.</returns>
        protected override string GetSlugFromPublishFileName(string publishFileName) =>
            StaticSitePage.FilenameSlugRegex.Match(publishFileName).Groups[1].Value;

        /// <summary>
        /// Crawl parent tree and collect all slugs
        /// </summary>
        /// <returns>An array of strings containing the slugs of all parents, in order.</returns>
        /// <exception cref="BlogClientException">Page parent not found - Could not locate parent for page '{0}' with specified parent ID.</exception>
        private string[] GetParentSlugs()
        {
            var parentSlugs = new List<string>();

            var parentId = this.BlogPost.PageParent.Id;
            var level = 0;
            while (!string.IsNullOrEmpty(parentId) && level < StaticSitePage.ParentCrawlMaxLevels)
            {
                var parent = StaticSitePage.GetPageById(this.SiteConfig, parentId);
                if (parent == null)
                {
                    throw new BlogClientException(
                        "Page parent not found",
                        "Could not locate parent for page '{0}' with specified parent ID.",
                        this.BlogPost.Title);
                }

                parentSlugs.Insert(0, parent.Slug);

                parentId = parent.BlogPost.PageParent.Id;
                level++;
            }

            return parentSlugs.ToArray();
        }
    }
}
