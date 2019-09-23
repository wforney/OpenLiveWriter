// <copyright file="StaticSiteItemFrontMatter.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using OpenLiveWriter.Extensibility.BlogClient;

    using YamlDotNet.RepresentationModel;

    /// <summary>
    /// The StaticSiteItemFrontMatter class.
    /// </summary>
    public class StaticSiteItemFrontMatter
    {
        /// <summary>
        /// The front matter keys
        /// </summary>
        private readonly StaticSiteConfigFrontMatterKeys frontMatterKeys;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteItemFrontMatter"/> class.
        /// </summary>
        /// <param name="frontMatterKeys">The front matter keys.</param>
        public StaticSiteItemFrontMatter(StaticSiteConfigFrontMatterKeys frontMatterKeys)
        {
            this.frontMatterKeys = frontMatterKeys;
            this.Tags = Array.Empty<string>(); // Initialize Tags to empty array
        }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        /// <value>The date.</value>
        public string Date { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the layout.
        /// </summary>
        /// <value>The layout.</value>
        public string Layout { get; set; } = "post";

        /// <summary>
        /// Gets or sets the parent identifier.
        /// </summary>
        /// <value>The parent identifier.</value>
        public string ParentId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the permalink.
        /// </summary>
        /// <value>The permalink.</value>
        public string Permalink { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        /// <value>The slug.</value>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the tags.
        /// </summary>
        /// <value>The tags.</value>
        public string[] Tags { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets from blog post.
        /// </summary>
        /// <param name="frontMatterKeys">The front matter keys.</param>
        /// <param name="post">The post.</param>
        /// <returns>A <see cref="StaticSiteItemFrontMatter"/>.</returns>
        public static StaticSiteItemFrontMatter GetFromBlogPost(
            StaticSiteConfigFrontMatterKeys frontMatterKeys,
            BlogPost post)
        {
            var frontMatter = new StaticSiteItemFrontMatter(frontMatterKeys);
            frontMatter.LoadFromBlogPost(post);
            return frontMatter;
        }

        /// <summary>
        /// Gets from YAML.
        /// </summary>
        /// <param name="frontMatterKeys">The front matter keys.</param>
        /// <param name="yaml">The YAML.</param>
        /// <returns>A <see cref="StaticSiteItemFrontMatter"/>.</returns>
        public static StaticSiteItemFrontMatter GetFromYaml(
            StaticSiteConfigFrontMatterKeys frontMatterKeys,
            string yaml)
        {
            var frontMatter = new StaticSiteItemFrontMatter(frontMatterKeys);
            frontMatter.Deserialize(yaml);
            return frontMatter;
        }

        /// <summary>
        /// Deserializes the specified YAML.
        /// </summary>
        /// <param name="yaml">The YAML.</param>
        public void Deserialize(string yaml)
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            var root = (YamlMappingNode)stream.Documents[0].RootNode;

            // Load id
            var idNodes = root.Where(kv => kv.Key.ToString() == this.frontMatterKeys.IdKey).ToList();
            if (idNodes.Any())
            {
                this.Id = idNodes.First().Value.ToString();
            }

            // Load title
            var titleNodes = root.Where(kv => kv.Key.ToString() == this.frontMatterKeys.TitleKey).ToList();
            if (titleNodes.Any())
            {
                this.Title = titleNodes.First().Value.ToString();
            }

            // Load date
            var dateNodes = root.Where(kv => kv.Key.ToString() == this.frontMatterKeys.DateKey).ToList();
            if (dateNodes.Any())
            {
                this.Date = dateNodes.First().Value.ToString();
            }

            // Load layout
            var layoutNodes = root.Where(kv => kv.Key.ToString() == this.frontMatterKeys.LayoutKey).ToList();
            if (layoutNodes.Any())
            {
                this.Layout = layoutNodes.First().Value.ToString();
            }

            // Load tags
            var tagNodes = root.Where(kv => kv.Key.ToString() == this.frontMatterKeys.TagsKey).ToList();
            if (tagNodes.Any() && tagNodes.First().Value.NodeType == YamlNodeType.Sequence)
            {
                this.Tags = ((YamlSequenceNode)tagNodes.First().Value).Select(node => node.ToString()).ToArray();
            }

            // Load parent ID
            var parentIdNodes = root.Where(kv => kv.Key.ToString() == this.frontMatterKeys.ParentIdKey).ToList();
            if (parentIdNodes.Any())
            {
                this.ParentId = parentIdNodes.First().Value.ToString();
            }

            // Permalink is never loaded, only saved
        }

        /// <summary>
        /// Loads from blog post.
        /// </summary>
        /// <param name="post">The post.</param>
        public void LoadFromBlogPost(BlogPost post)
        {
            this.Id = post.Id;
            this.Title = post.Title;
            this.Tags = post.Categories.Union(post.NewCategories).Select(cat => cat.Name).ToArray();
            this.Date =
                (post.HasDatePublishedOverride ? post.DatePublishedOverride : post.DatePublished).ToString(
                    "yyyy-MM-dd HH:mm:ss");
            this.Layout = post.IsPage ? "page" : "post";
            if (post.IsPage)
            {
                this.ParentId = post.PageParent.Id;
            }
        }

        /// <summary>
        /// Saves to blog post.
        /// </summary>
        /// <param name="post">The post.</param>
        public void SaveToBlogPost(BlogPost post)
        {
            post.Id = this.Id;
            post.Title = this.Title;
            post.Categories = this.Tags?.Select(t => new BlogPostCategory(t)).ToArray();
            try
            {
                post.DatePublished = post.DatePublishedOverride = DateTime.Parse(this.Date);
            }
            catch
            {
                // ignored
            }

            post.IsPage = this.Layout == "page";
            if (post.IsPage)
            {
                post.PageParent = new PostIdAndNameField(this.ParentId, string.Empty);
            }
        }

        /// <summary>
        /// Converts the front matter to it's YAML representation
        /// </summary>
        /// <returns>YAML representation of post front-matter, lines separated by CRLF</returns>
        public string Serialize()
        {
            var root = new YamlMappingNode();

            if (!string.IsNullOrEmpty(this.Id))
            {
                root.Add(this.frontMatterKeys.IdKey, this.Id);
            }

            if (this.Title != null)
            {
                root.Add(this.frontMatterKeys.TitleKey, this.Title);
            }

            if (this.Date != null)
            {
                root.Add(this.frontMatterKeys.DateKey, this.Date);
            }

            if (this.Layout != null)
            {
                root.Add(this.frontMatterKeys.LayoutKey, this.Layout);
            }

            if (this.Tags != null && this.Tags.Length > 0)
            {
                root.Add(
                    this.frontMatterKeys.TagsKey,
                    new YamlSequenceNode(this.Tags.Select(tag => new YamlScalarNode(tag))));
            }

            if (!string.IsNullOrEmpty(this.ParentId))
            {
                root.Add(this.frontMatterKeys.ParentIdKey, this.ParentId);
            }

            if (!string.IsNullOrEmpty(this.Permalink))
            {
                root.Add(this.frontMatterKeys.PermalinkKey, this.Permalink);
            }

            var stream = new YamlStream(new YamlDocument(root));
            var stringWriter = new StringWriter();
            stream.Save(stringWriter);

            // Trim off end-of-doc
            return new Regex("\\.\\.\\.\r\n$").Replace(stringWriter.ToString(), string.Empty, 1);
        }

        /// <summary>
        /// Converts the front matter to it's YAML representation
        /// </summary>
        /// <returns>YAML representation of post front-matter, lines separated by CRLF</returns>
        public override string ToString() => this.Serialize();
    }
}
