// <copyright file="StaticSiteItem.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The StaticSiteItem class.
    /// </summary>
    public abstract class StaticSiteItem
    {
        /// <summary>
        /// The file path by identifier
        /// </summary>
        protected string FilePathByIdentifier;

        /// <summary>
        /// The site configuration
        /// </summary>
        protected StaticSiteConfig SiteConfig;

        /// <summary>
        /// The post parse regex
        /// </summary>
        private static readonly Regex PostParseRegex =
            new Regex("^---\r?\n((?:.*\r?\n)*?)---\r?\n\r?\n((?:.*\r?\n?)*)");

        /// <summary>
        /// Confirmed safe slug; does not conflict with any existing post on disk or points to this post on disk.
        /// </summary>
        private string safeSlug;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteItem"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        protected StaticSiteItem(StaticSiteConfig config)
        {
            this.SiteConfig = config;
            this.BlogPost = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteItem"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="blogPost">The blog post.</param>
        protected StaticSiteItem(StaticSiteConfig config, BlogPost blogPost)
        {
            this.SiteConfig = config;
            this.BlogPost = blogPost;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteItem"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="blogPost">The blog post.</param>
        /// <param name="isDraft">if set to <c>true</c> [is draft].</param>
        protected StaticSiteItem(StaticSiteConfig config, BlogPost blogPost, bool isDraft)
        {
            this.SiteConfig = config;
            this.BlogPost = blogPost;
            this.IsDraft = isDraft;
        }

        /// <summary>
        /// Gets the extension of published posts to the site project, including dot.
        /// </summary>
        public static string PublishFileExtension { get; } = ".html";

        /// <summary>
        /// Gets the blog post.
        /// </summary>
        /// <value>The blog post.</value>
        public BlogPost BlogPost { get; private set; }

        /// <summary>
        /// Gets or sets the date published.
        /// </summary>
        /// <value>The date published.</value>
        public DateTime DatePublished
        {
            get =>
                this.BlogPost.HasDatePublishedOverride
                    ? this.BlogPost.DatePublishedOverride
                    : this.BlogPost.DatePublished;

            set => this.BlogPost.DatePublished = this.BlogPost.DatePublishedOverride = value;
        }

        /// <summary>
        /// Get the current on-disk slug from the on-disk post with this ID
        /// </summary>
        /// <value>The disk slug from file path by identifier.</value>
        public string DiskSlugFromFilePathById =>
            this.FilePathById == null ? null : this.GetSlugFromPublishFileName(this.FilePathById);

        /// <summary>
        /// Gets or sets the on-disk file path for the published post, based on ID
        /// </summary>
        /// <value>The file path by identifier.</value>
        public abstract string FilePathById { get; protected set; }

        /// <summary>
        /// Get the on-disk file path for the published post, based on slug
        /// </summary>
        /// <value>The file path by slug.</value>
        public string FilePathBySlug => this.GetFilePathForProvidedSlug(this.Slug);

        /// <summary>
        /// Gets the front matter.
        /// </summary>
        /// <value>The front matter.</value>
        public virtual StaticSiteItemFrontMatter FrontMatter =>
            StaticSiteItemFrontMatter.GetFromBlogPost(this.SiteConfig.FrontMatterKeys, this.BlogPost);

        /// <summary>
        /// Gets or sets the Unique ID of the BlogPost
        /// </summary>
        /// <value>The identifier.</value>
        public string Id
        {
            get => this.BlogPost.Id;
            set => this.BlogPost.Id = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is draft.
        /// </summary>
        /// <value><c>true</c> if this instance is draft; otherwise, <c>false</c>.</value>
        public bool IsDraft { get; set; }

        /// <summary>
        /// Gets the site path for the published item
        /// e.g. /2019/01/slug.html
        /// </summary>
        /// <value>The site path.</value>
        public abstract string SitePath { get; }

        /// <summary>
        /// Gets or sets the safe slug for the post
        /// </summary>
        /// <value>The slug.</value>
        public string Slug
        {
            get => this.safeSlug;
            set => this.BlogPost.Slug = this.safeSlug = value;
        }

        /// <summary>
        /// If the item is a Post and a Draft, returns the Drafts directory, otherwise returns the regular directory
        /// </summary>
        /// <value>The item relative directory.</value>
        protected string ItemRelativeDir =>
            this.IsDraft && !this.BlogPost.IsPage && this.SiteConfig.DraftsEnabled ? this.SiteConfig.DraftsPath :
            this.BlogPost.IsPage ? this.SiteConfig.PagesPath : this.SiteConfig.PostsPath;

        /// <summary>
        /// Set post published DateTime to current DateTime if one isn't already set, or current one is default.
        /// </summary>
        /// <returns>The current or new DatePublished.</returns>
        public DateTime EnsureDatePublished()
        {
            if (this.DatePublished == new DateTime(1, 1, 1))
            {
                this.DatePublished = DateTime.UtcNow;
            }

            return this.DatePublished;
        }

        /// <summary>
        /// Generate a new Id and save it to the BlogPost if required. Returns the current or new Id.
        /// </summary>
        /// <returns>The current or new Id</returns>
        public string EnsureId()
        {
            if (string.IsNullOrEmpty(this.Id))
            {
                this.Id = Guid.NewGuid().ToString();
            }

            return this.Id;
        }

        /// <summary>
        /// Generate a safe slug if the post doesn't already have one. Returns the current or new Slug.
        /// </summary>
        /// <returns>The current or new Slug.</returns>
        public string EnsureSafeSlug()
        {
            if (string.IsNullOrEmpty(this.safeSlug))
            {
                this.Slug = this.FindNewSlug(this.BlogPost.Slug, true);
            }

            return this.Slug;
        }

        /// <summary>
        /// Generate a slug for this post based on it's title or a preferred slug
        /// </summary>
        /// <param name="preferredSlug">The text to base the preferred slug off of. default: post title</param>
        /// <param name="safe">Safe mode; if true the returned slug will not conflict with any existing file</param>
        /// <returns>An on-disk slug for this post</returns>
        public string FindNewSlug(string preferredSlug, bool safe)
        {
            // Try the filename without a duplicate identifier, then duplicate identifiers up until 999 before throwing an exception
            for (var i = 0; i < 1000; i++)
            {
                // "Hello World!" -> "hello-world"
                var slug = StaticSiteClient.WebUnsafeChars.Replace(
                    (preferredSlug == string.Empty ? this.BlogPost.Title : preferredSlug)
                   .ToLower(),
                    string.Empty).Replace(" ", "-");
                if (!safe)
                {
                    return slug; // If unsafe mode, return the generated slug immediately.
                }

                if (i > 0)
                {
                    slug += $"-{i}";
                }

                if (!File.Exists(this.GetFilePathForProvidedSlug(slug)))
                {
                    return slug;
                }
            }

            // Couldn't find an available filename, use the post's ID.
            return StaticSiteClient.WebUnsafeChars.Replace(this.EnsureId(), string.Empty).Replace(" ", "-");
        }

        /// <summary>
        /// Load published post from a specified file path
        /// </summary>
        /// <param name="postFilePath">Path to published post file</param>
        public virtual void LoadFromFile(string postFilePath)
        {
            // Attempt to load file contents
            var fileContents = File.ReadAllText(postFilePath);

            // Parse out everything between triple-hyphens into front matter parser
            var frontMatterMatchResult = StaticSiteItem.PostParseRegex.Match(fileContents);
            if (!frontMatterMatchResult.Success || frontMatterMatchResult.Groups.Count < 3)
            {
                throw new BlogClientException(
                    Res.Get(StringId.SSGErrorItemLoadTitle),
                    Res.Get(StringId.SSGErrorItemLoadTextFM));
            }

            var frontMatterYaml = frontMatterMatchResult.Groups[1].Value;
            var postContent = frontMatterMatchResult.Groups[2].Value;

            // Create a new BlogPost
            this.BlogPost = new BlogPost();

            // Parse front matter and save in
            StaticSiteItemFrontMatter.GetFromYaml(this.SiteConfig.FrontMatterKeys, frontMatterYaml)
                                     .SaveToBlogPost(this.BlogPost);

            // Throw error if post does not have an ID
            if (string.IsNullOrEmpty(this.Id))
            {
                throw new BlogClientException(
                    Res.Get(StringId.SSGErrorItemLoadTitle),
                    Res.Get(StringId.SSGErrorItemLoadTextId));
            }

            // FilePathById will be the path we loaded this post from
            this.FilePathById = postFilePath;

            // Load the content into blog post
            this.BlogPost.Contents = postContent;

            // Set slug to match file name
            this.Slug = this.GetSlugFromPublishFileName(Path.GetFileName(postFilePath));
        }

        /// <summary>
        /// Save the post to the correct directory
        /// </summary>
        /// <param name="postFilePath">The post file path.</param>
        public void SaveToFile(string postFilePath) => File.WriteAllText(postFilePath, this.ToString());

        /// <summary>
        /// Converts the post to a string, ready to be written to disk
        /// </summary>
        /// <returns>String representation of the post, including front-matter, lines separated by LF</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("---");
            builder.Append(this.FrontMatter);
            builder.AppendLine("---");
            builder.AppendLine();
            builder.Append(this.BlogPost.Contents);
            return builder.ToString().Replace("\r\n", "\n");
        }

        /// <summary>
        /// Get the on-disk filename for the provided slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>The on-disk filename</returns>
        protected abstract string GetFileNameForProvidedSlug(string slug);

        /// <summary>
        /// Get the on-disk path for the provided slug
        /// </summary>
        /// <param name="slug">Post slug</param>
        /// <returns>The on-disk path, including filename from GetFileNameForProvidedSlug</returns>
        protected abstract string GetFilePathForProvidedSlug(string slug);

        /// <summary>
        /// Gets the name of the slug from publish file.
        /// </summary>
        /// <param name="publishFileName">Name of the publish file.</param>
        /// <returns>The slug.</returns>
        protected abstract string GetSlugFromPublishFileName(string publishFileName);
    }
}
