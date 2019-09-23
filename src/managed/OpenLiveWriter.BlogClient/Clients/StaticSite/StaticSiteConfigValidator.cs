// <copyright file="StaticSiteConfigValidator.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System.IO;

    using OpenLiveWriter.Localization;

    /// <summary>
    /// The StaticSiteConfigValidator class.
    /// </summary>
    public class StaticSiteConfigValidator
    {
        /// <summary>
        /// The configuration
        /// </summary>
        private readonly StaticSiteConfig config;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteConfigValidator"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        public StaticSiteConfigValidator(StaticSiteConfig config) => this.config = config;

        /// <summary>
        /// Validates all.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidateAll() =>
            this.ValidateLocalSitePath().ValidatePostsPath().ValidatePagesPath().ValidateDraftsPath()
                .ValidateImagesPath().ValidateOutputPath().ValidateBuildCommand().ValidatePublishCommand();

        /// <summary>
        /// Validates the build command.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidateBuildCommand()
        {
            if (!this.config.BuildingEnabled)
            {
                return this; // Don't validate if building isn't enabled
            }

            if (this.config.BuildCommand.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorBuildCommandEmptyTitle),
                    Res.Get(StringId.SSGErrorBuildCommandEmptyText));
            }

            return this;
        }

        /// <summary>
        /// Validates the drafts path.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidateDraftsPath()
        {
            if (!this.config.DraftsEnabled)
            {
                return this; // Don't validate if drafts aren't enabled
            }

            var draftsPathFull = $"{this.config.LocalSitePath}\\{this.config.DraftsPath}";

            // If the Drafts path is empty, display an error
            if (this.config.DraftsPath.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathDraftsEmpty));
            }

            // If the path doesn't exist, display an error
            if (!Directory.Exists(draftsPathFull))
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathDraftsNotFound),
                    draftsPathFull);
            }

            return this;
        }

        /// <summary>
        /// Validates the images path.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidateImagesPath()
        {
            if (!this.config.ImagesEnabled)
            {
                return this; // Don't validate if images aren't enabled
            }

            var imagesPathFull = $"{this.config.LocalSitePath}\\{this.config.ImagesPath}";

            // If the Images path is empty, display an error
            if (this.config.ImagesPath.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathImagesEmpty));
            }

            // If the path doesn't exist, display an error
            if (!Directory.Exists(imagesPathFull))
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathImagesNotFound),
                    imagesPathFull);
            }

            return this;
        }

        /// <summary>
        /// Validates the local site path.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidateLocalSitePath()
        {
            if (!Directory.Exists(this.config.LocalSitePath))
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathLocalSitePathNotFound),
                    this.config.LocalSitePath);
            }

            return this;
        }

        /// <summary>
        /// Validates the output path.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidateOutputPath()
        {
            if (!this.config.BuildingEnabled)
            {
                return this; // Don't validate if building isn't enabled
            }

            var outputPathFull = $"{this.config.LocalSitePath}\\{this.config.OutputPath}";

            // If the Output path is empty, display an error
            if (this.config.OutputPath.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathOutputEmpty));
            }

            // If the path doesn't exist, display an error
            if (!Directory.Exists(outputPathFull))
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathOutputNotFound),
                    outputPathFull);
            }

            return this;
        }

        /// <summary>
        /// Validates the pages path.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidatePagesPath()
        {
            if (!this.config.PagesEnabled)
            {
                return this; // Don't validate if pages aren't enabled
            }

            var pagesPathFull = $"{this.config.LocalSitePath}\\{this.config.PagesPath}";

            // If the Pages path is empty, display an error
            if (this.config.PagesPath.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPagesEmpty));
            }

            // If the path doesn't exist, display an error
            if (!Directory.Exists(pagesPathFull))
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPagesNotFound),
                    pagesPathFull);
            }

            return this;
        }

        /// <summary>
        /// Validates the posts path.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidatePostsPath()
        {
            var postsPathFull = $"{this.config.LocalSitePath}\\{this.config.PostsPath}";

            // If the Posts path is empty, display an error
            if (this.config.PostsPath.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPostsEmpty));
            }

            // If the Posts path doesn't exist, display an error
            if (!Directory.Exists(postsPathFull))
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPathFolderNotFound),
                    Res.Get(StringId.SSGErrorPathPostsNotFound),
                    postsPathFull);
            }

            return this;
        }

        /// <summary>
        /// Validates the publish command.
        /// </summary>
        /// <returns>A <see cref="StaticSiteConfigValidator"/>.</returns>
        public StaticSiteConfigValidator ValidatePublishCommand()
        {
            if (this.config.PublishCommand.Trim() == string.Empty)
            {
                throw new StaticSiteConfigValidationException(
                    Res.Get(StringId.SSGErrorPublishCommandEmptyTitle),
                    Res.Get(StringId.SSGErrorPublishCommandEmptyText));
            }

            return this;
        }
    }
}
