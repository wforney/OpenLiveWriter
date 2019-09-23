// <copyright file="StaticSiteConfigFrontMatterKeys.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    /// <summary>
    /// Represents the YAML keys used for each of these properties in the front matter
    /// </summary>
    public class StaticSiteConfigFrontMatterKeys
    {
        /// <summary>
        /// The configuration date key
        /// </summary>
        private const string ConfigDateKey = "FrontMatterKey.Date";

        /// <summary>
        /// The configuration identifier key
        /// </summary>
        private const string ConfigIdKey = "FrontMatterKey.Id";

        /// <summary>
        /// The configuration layout key
        /// </summary>
        private const string ConfigLayoutKey = "FrontMatterKey.Layout";

        /// <summary>
        /// The configuration parent identifier key
        /// </summary>
        private const string ConfigParentIdKey = "FrontMatterKey.ParentId";

        /// <summary>
        /// The configuration permalink key
        /// </summary>
        private const string ConfigPermalinkKey = "FrontMatterKey.Permalink";

        /// <summary>
        /// The configuration tags key
        /// </summary>
        private const string ConfigTagsKey = "FrontMatterKey.Tags";

        /// <summary>
        /// The configuration title key
        /// </summary>
        private const string ConfigTitleKey = "FrontMatterKey.Title";

        /// <summary>
        /// The KeyIdentifier enumeration.
        /// </summary>
        public enum KeyIdentifier
        {
            /// <summary>
            /// The identifier
            /// </summary>
            Id,

            /// <summary>
            /// The title
            /// </summary>
            Title,

            /// <summary>
            /// The date
            /// </summary>
            Date,

            /// <summary>
            /// The layout
            /// </summary>
            Layout,

            /// <summary>
            /// The tags
            /// </summary>
            Tags,

            /// <summary>
            /// The parent identifier
            /// </summary>
            ParentId,

            /// <summary>
            /// The permalink
            /// </summary>
            Permalink
        }

        /// <summary>
        /// Gets or sets the date key.
        /// </summary>
        /// <value>The date key.</value>
        public string DateKey { get; set; } = "date";

        /// <summary>
        /// Gets or sets the identifier key.
        /// </summary>
        /// <value>The identifier key.</value>
        public string IdKey { get; set; } = "id";

        /// <summary>
        /// Gets or sets the layout key.
        /// </summary>
        /// <value>The layout key.</value>
        public string LayoutKey { get; set; } = "layout";

        /// <summary>
        /// Gets or sets the parent identifier key.
        /// </summary>
        /// <value>The parent identifier key.</value>
        public string ParentIdKey { get; set; } = "parent_id";

        /// <summary>
        /// Gets or sets the permalink key.
        /// </summary>
        /// <value>The permalink key.</value>
        public string PermalinkKey { get; set; } = "permalink";

        /// <summary>
        /// Gets or sets the tags key.
        /// </summary>
        /// <value>The tags key.</value>
        public string TagsKey { get; set; } = "tags";

        /// <summary>
        /// Gets or sets the title key.
        /// </summary>
        /// <value>The title key.</value>
        public string TitleKey { get; set; } = "title";

        /// <summary>
        /// Create a new StaticSiteConfigFrontMatterKeys instance and load configuration from blog credentials
        /// </summary>
        /// <param name="blogCredentials">An <see cref="IBlogCredentialsAccessor" />.</param>
        /// <returns>A <see cref="StaticSiteConfigFrontMatterKeys" />.</returns>
        public static StaticSiteConfigFrontMatterKeys LoadKeysFromCredentials(IBlogCredentialsAccessor blogCredentials)
        {
            var frontMatterKeys = new StaticSiteConfigFrontMatterKeys();
            frontMatterKeys.LoadFromCredentials(blogCredentials);
            return frontMatterKeys;
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigFrontMatterKeys" />.</returns>
        public StaticSiteConfigFrontMatterKeys Clone() =>
            new StaticSiteConfigFrontMatterKeys
                {
                    IdKey = this.IdKey,
                    TitleKey = this.TitleKey,
                    DateKey = this.DateKey,
                    LayoutKey = this.LayoutKey,
                    TagsKey = this.TagsKey,
                    ParentIdKey = this.ParentIdKey,
                    PermalinkKey = this.PermalinkKey
                };

        /// <summary>
        /// Load front matter keys configuration from blog credentials
        /// </summary>
        /// <param name="credentials">An <see cref="IBlogCredentialsAccessor" />.</param>
        public void LoadFromCredentials(IBlogCredentialsAccessor credentials)
        {
            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigIdKey) != string.Empty)
            {
                this.IdKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigIdKey);
            }

            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigTitleKey) != string.Empty)
            {
                this.TitleKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigTitleKey);
            }

            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigDateKey) != string.Empty)
            {
                this.DateKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigDateKey);
            }

            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigLayoutKey) != string.Empty)
            {
                this.LayoutKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigLayoutKey);
            }

            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigTagsKey) != string.Empty)
            {
                this.TagsKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigTagsKey);
            }

            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigParentIdKey) != string.Empty)
            {
                this.ParentIdKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigParentIdKey);
            }

            if (credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigPermalinkKey) != string.Empty)
            {
                this.PermalinkKey = credentials.GetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigPermalinkKey);
            }
        }

        /// <summary>
        /// Save front matter keys configuration to blog credentials
        /// </summary>
        /// <param name="credentials">An <see cref="IBlogCredentialsAccessor" />.</param>
        public void SaveToCredentials(IBlogCredentialsAccessor credentials)
        {
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigIdKey, this.IdKey);
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigTitleKey, this.TitleKey);
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigDateKey, this.DateKey);
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigLayoutKey, this.LayoutKey);
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigTagsKey, this.TagsKey);
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigParentIdKey, this.ParentIdKey);
            credentials.SetCustomValue(StaticSiteConfigFrontMatterKeys.ConfigPermalinkKey, this.PermalinkKey);
        }
    }
}
