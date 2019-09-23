// <copyright file="StaticSiteClient.cs" company=".NET Foundation">
//     Copyright © .NET Foundation. All rights reserved.
// </copyright>

namespace OpenLiveWriter.BlogClient.Clients.StaticSite
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Xml;

    using OpenLiveWriter.Api;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The StaticSiteClient class.
    /// Implements the <see cref="BlogClientBase" />
    /// Implements the <see cref="IBlogClient" />
    /// </summary>
    /// <seealso cref="BlogClientBase" />
    /// <seealso cref="IBlogClient" />
    [BlogClient(StaticSiteClient.ClientType, StaticSiteClient.ClientType)]
    public class StaticSiteClient : BlogClientBase, IBlogClient
    {
        // The 'provider' concept doesn't really apply to local static sites
        // Store these required constants here so they're in one place

        /// <summary>
        /// The client type
        /// </summary>
        public const string ClientType = "StaticSite";

        /// <summary>
        /// The post API URL
        /// </summary>
        /// <remarks>
        /// A valid URI is required for BlogClientManager to instantiate a URI object on.
        /// </remarks>
        public const string PostApiUrl = "http://localhost/";

        /// <summary>
        /// The provider identifier
        /// </summary>
        public const string ProviderId = "D0E0062F-7540-4462-94FD-DC55004D95E6";

        /// <summary>
        /// The service name
        /// </summary>
        public const string ServiceName = "Static Site Generator";

        /// <summary>
        /// The configuration
        /// </summary>
        private readonly StaticSiteConfig config;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSiteClient"/> class.
        /// </summary>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        public StaticSiteClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            this.config = StaticSiteConfig.LoadConfigFromCredentials(credentials);

            // Set the client options
            var options = new BlogClientOptions();
            this.ConfigureClientOptions(options);
            this.Options = options;
        }

        /// <summary>
        /// Gets the web unsafe chars
        /// </summary>
        public static Regex WebUnsafeChars { get; } = new Regex("[^A-Za-z0-9- ]*");

        /// <summary>
        /// Returns if this StaticSiteGeneratorClient is secure
        /// Returns true for now as we trust the user publish script
        /// </summary>
        /// <value><c>true</c> if this instance is secure; otherwise, <c>false</c>.</value>
        public bool IsSecure => true;

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public IBlogClientOptions Options { get; private set; }

        /// <summary>
        /// Remote detection is now possible as SendAuthenticatedHttpRequest has been implemented.
        /// </summary>
        /// <value><c>true</c> if [remote detection possible]; otherwise, <c>false</c>.</value>
        public override bool RemoteDetectionPossible => true;

        // Authentication is handled by publish script at the moment

        /// <inheritdoc />
        protected override bool RequiresPassword => false;

        /// <summary>
        /// Adds the category.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="category">The category.</param>
        /// <returns>The category result.</returns>
        public string AddCategory(string blogId, BlogPostCategory category) =>
            throw new BlogClientMethodUnsupportedException("AddCategory");

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="pageId">The page identifier.</param>
        public void DeletePage(string blogId, string pageId)
        {
            var page = StaticSitePage.GetPageById(this.config, pageId);
            if (page == null)
            {
                throw new BlogClientException(
                    Res.Get(StringId.SSGErrorPageDoesNotExistTitle),
                    Res.Get(StringId.SSGErrorPageDoesNotExistText));
            }

            this.DoDeleteItem(page);
        }

        /// <summary>
        /// Deletes the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        public void DeletePost(string blogId, string postId, bool publish)
        {
            var post = StaticSitePost.GetPostById(this.config, postId);
            if (post == null)
            {
                throw new BlogClientException(
                    Res.Get(StringId.SSGErrorPostDoesNotExistTitle),
                    Res.Get(StringId.SSGErrorPostDoesNotExistText));
            }

            this.DoDeleteItem(post);
        }

        /// <summary>
        /// Does the after publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result.</returns>
        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            var path = uploadContext.GetContentsLocalFilePath();
            return this.DoPostImage(path);
        }

        /// <summary>
        /// Determines whether the the file needs to be uploaded.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns><c>true</c> if the file needs to be uploaded, <c>false</c> otherwise.</returns>
        public bool? DoesFileNeedUpload(IFileUploadContext uploadContext) => null;

        /// <summary>
        /// Edits the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="page">The page.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish)
            {
                Trace.Fail("Posting pages as drafts not yet implemented.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            // if (!publish && !Options.SupportsPostAsDraft)
            // {
            // Trace.Fail("Static site does not support drafts, cannot post.");
            // throw new BlogClientPostAsDraftUnsupportedException();
            // }
            remotePost = null;
            etag = string.Empty;

            // Create a StaticSitePage on the provided page
            var ssgPage = new StaticSitePage(this.config, page);

            if (ssgPage.FilePathById == null)
            {
                // Existing page could not be found to edit, call NewPage instead;
                this.NewPage(blogId, page, true, out etag, out remotePost);
                return true;
            }

            // Set slug to existing slug on page
            ssgPage.Slug = page.Slug;

            return this.DoEditItem(ssgPage);
        }

        /// <summary>
        /// Edits the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="post">The post.</param>
        /// <param name="newCategoryContext">The new category context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public bool EditPost(
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish,
            out string etag,
            out XmlDocument remotePost)
        {
            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Static site does not support drafts, cannot post.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            remotePost = null;
            etag = string.Empty;

            // Create a StaticSitePost on the provided post
            var ssgPost = new StaticSitePost(this.config, post, !publish);

            if (ssgPost.FilePathById == null)
            {
                // If we are publishing and there exists a draft with this ID, delete it.
                if (publish)
                {
                    var filePath = new StaticSitePost(this.config, post, true).FilePathById;
                    if (filePath != null)
                    {
                        File.Delete(filePath);
                    }
                }

                // Existing post could not be found to edit, call NewPost instead;
                this.NewPost(blogId, post, newCategoryContext, publish, out etag, out remotePost);
                return true;
            }

            // Set slug to existing slug on post
            ssgPost.Slug = post.Slug;

            return this.DoEditItem(ssgPost);
        }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{AuthorInfo}"/>.</returns>
        public AuthorInfo[] GetAuthors(string blogId) => throw new NotImplementedException();

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{BlogPostCategory}"/>.</returns>
        public BlogPostCategory[] GetCategories(string blogId) =>
            StaticSitePost.GetAllPosts(this.config, false)
                          .SelectMany(post => post.BlogPost.Categories.Select(cat => cat.Name)).Distinct()
                          .Select(cat => new BlogPostCategory(cat)).ToArray();

        /// <summary>
        /// Gets the image endpoints.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        public BlogInfo[] GetImageEndpoints() => throw new NotImplementedException();

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{BlogPostKeyword}"/>.</returns>
        public BlogPostKeyword[] GetKeywords(string blogId) => new BlogPostKeyword[0];

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns>A <see cref="BlogPost"/>.</returns>
        public BlogPost GetPage(string blogId, string pageId)
        {
            var page = StaticSitePage.GetPageById(this.config, pageId);
            page.ResolveParent();
            return page.BlogPost;
        }

        /// <summary>
        /// Gets the page list.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{PageInfo}"/>.</returns>
        public PageInfo[] GetPageList(string blogId) =>
            StaticSitePage.GetAllPages(this.config).Select(page => page.PageInfo).ToArray();

        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPages">The maximum pages.</param>
        /// <returns>An <see cref="Array{BlogPost}"/>.</returns>
        public BlogPost[] GetPages(string blogId, int maxPages) =>
            StaticSitePage.GetAllPages(this.config).Select(page => page.BlogPost)
                          .OrderByDescending(page => page.DatePublished).Take(maxPages).ToArray();

        /// <summary>
        /// Attempt to get a post with the specified id (note: may return null
        /// if the post could not be found on the remote server)
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <returns>A <see cref="BlogPost"/>.</returns>
        public BlogPost GetPost(string blogId, string postId) =>
            StaticSitePost.GetPostById(this.config, postId).BlogPost;

        /// <summary>
        /// Returns recent posts
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPosts">The maximum posts.</param>
        /// <param name="includeCategories">if set to <c>true</c> [include categories].</param>
        /// <param name="now">If null, then includes future posts.  If non-null, then only includes posts before the *UTC* 'now' time.</param>
        /// <returns>An <see cref="Array{BlogPost}"/>.</returns>
        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now) =>
            StaticSitePost.GetAllPosts(this.config, true).Select(post => post.BlogPost)
                          .Where(post => post != null && (now == null || post.DatePublished < now))
                          .OrderByDescending(post => post.DatePublished).Take(maxPosts).ToArray();

        /// <summary>
        /// Gets the users blogs.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        public BlogInfo[] GetUsersBlogs() => new BlogInfo[0];

        /// <summary>
        /// Creates new page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="page">The page.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns>The new page.</returns>
        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            if (!publish)
            {
                Trace.Fail("Posting pages as drafts not yet implemented.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            // if (!publish && !Options.SupportsPostAsDraft)
            // {
            // Trace.Fail("Static site does not support drafts, cannot post.");
            // throw new BlogClientPostAsDraftUnsupportedException();
            // }
            remotePost = null;
            etag = string.Empty;

            // Create a StaticSitePost on the provided page
            return this.DoNewItem(new StaticSitePage(this.config, page));
        }

        /// <summary>
        /// Creates new post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="post">The post.</param>
        /// <param name="newCategoryContext">The new category context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns>The new post.</returns>
        public string NewPost(
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish,
            out string etag,
            out XmlDocument remotePost)
        {
            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Static site does not support drafts, cannot post.");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            remotePost = null;
            etag = string.Empty;

            // Create a StaticSitePost on the provided post
            return this.DoNewItem(new StaticSitePost(this.config, post, !publish));
        }

        /// <summary>
        /// Overrides the options.
        /// </summary>
        /// <param name="newClientOptions">The new client options.</param>
        public void OverrideOptions(IBlogClientOptions newClientOptions) => this.Options = newClientOptions;

        /// <summary>
        /// Currently sends an UNAUTHENTICATED HTTP request.
        /// If a static site requires authentication, this may be implemented here later.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="timeoutMs">The timeout milliseconds.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>The <see cref="HttpWebResponse"/>.</returns>
        public HttpWebResponse
            SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter) =>
            BlogClientHelper.SendAuthenticatedHttpRequest(
                requestUri,
                filter,
                request =>
                    {
                    });

        /// <summary>
        /// Suggests the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="partialCategoryName">Partial name of the category.</param>
        /// <returns>A <see cref="Array{BlogPostCategory}"/>.</returns>
        /// <exception cref="BlogClientMethodUnsupportedException">SuggestCategories</exception>
        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName) =>
            throw new BlogClientMethodUnsupportedException("SuggestCategories");

        /// <summary>
        /// Verifies the credentials.
        /// </summary>
        /// <param name="transientCredentials">The transient credentials.</param>
        protected override void VerifyCredentials(TransientCredentials transientCredentials)
        {
        }

        /// <summary>
        /// Sets the relevant BlogClientOptions for this client based on values from the StaticSiteConfig
        /// </summary>
        /// <param name="clientOptions">A BlogClientOptions instance</param>
        private void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsPages = clientOptions.SupportsPageParent = this.config.PagesEnabled;
            clientOptions.SupportsPostAsDraft = this.config.DraftsEnabled;
            clientOptions.SupportsFileUpload = this.config.ImagesEnabled;
            clientOptions.SupportsImageUpload = this.config.ImagesEnabled ? SupportsFeature.Yes : SupportsFeature.No;
            clientOptions.SupportsScripts = clientOptions.SupportsEmbeds = SupportsFeature.Yes;
            clientOptions.SupportsExtendedEntries = true;

            // Blog template is downloaded from publishing a test post
            clientOptions.SupportsAutoUpdate = true;

            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsKeywords = false;

            clientOptions.FuturePublishDateWarning = true;
            clientOptions.SupportsCustomDate = clientOptions.SupportsCustomDateUpdate = true;
            clientOptions.SupportsSlug = true;
            clientOptions.SupportsAuthor = false;
        }

        /// <summary>
        /// Delete a StaticSiteItem from disk, and publish the changes
        /// </summary>
        /// <param name="item">a StaticSiteItem</param>
        private void DoDeleteItem(StaticSiteItem item)
        {
            var backupFileName = Path.GetTempFileName();
            File.Copy(item.FilePathById, backupFileName, true);

            try
            {
                File.Delete(item.FilePathById);

                // Build the site, if required
                if (this.config.BuildCommand != string.Empty)
                {
                    this.DoSiteBuild();
                }

                // Publish the site
                this.DoSitePublish();
            }
            catch
            {
                File.Copy(backupFileName, item.FilePathById, true);
                File.Delete(backupFileName);

                // Throw the exception up
                throw;
            }
        }

        /// <summary>
        /// Generic method to edit an already-published StaticSiteItem derived instance
        /// </summary>
        /// <param name="item">an existing StaticSiteItem derived instance</param>
        /// <returns>True if successful</returns>
        private bool DoEditItem(StaticSiteItem item)
        {
            // Copy the existing post to a temporary file
            var backupFileName = Path.GetTempFileName();
            File.Copy(item.FilePathById, backupFileName, true);

            var renameOccurred = false;

            // Store the old file path and slug
            var oldPath = item.FilePathById;

            // string oldSlug = item.DiskSlugFromFilePathById;
            try
            {
                // Determine if the post file needs renaming (slug, date or parent change)
                if (item.FilePathById != item.FilePathBySlug)
                {
                    renameOccurred = true;

                    // Find a new safe slug for the post
                    item.Slug = item.FindNewSlug(item.Slug, true);

                    // Remove the old file
                    File.Delete(oldPath);

                    // Save to the new file
                    item.SaveToFile(item.FilePathBySlug);
                }
                else
                {
                    // Save the post to disk based on it's existing id
                    item.SaveToFile(item.FilePathById);
                }

                // Build the site, if required
                if (this.config.BuildCommand != string.Empty)
                {
                    this.DoSiteBuild();
                }

                // Publish the site
                this.DoSitePublish();

                return true;
            }
            catch
            {
                // Clean up the failed output
                File.Delete(
                    renameOccurred

                        // Delete the rename target
                        ? item.FilePathBySlug

                        // Delete the original file
                        : item.FilePathById);

                // Copy the backup to the old location
                File.Copy(backupFileName, oldPath, true);

                // Delete the backup
                File.Delete(backupFileName);

                // Throw the exception up
                throw;
            }
        }

        /// <summary>
        /// Generic method to prepare and publish a new StaticSiteItem derived instance
        /// </summary>
        /// <param name="item">a new StaticSiteItem derived instance</param>
        /// <returns>the new StaticSitePost ID</returns>
        private string DoNewItem(StaticSiteItem item)
        {
            // Ensure the post has an ID
            var newPostId = item.EnsureId();

            // Ensure the post has a date
            item.EnsureDatePublished();

            // Ensure the post has a safe slug
            item.EnsureSafeSlug();

            // Save the post to disk under it's new slug-based path
            item.SaveToFile(item.FilePathBySlug);

            try
            {
                // Build the site, if required
                if (this.config.BuildCommand != string.Empty)
                {
                    this.DoSiteBuild();
                }

                // Publish the site
                this.DoSitePublish();

                return newPostId;
            }
            catch
            {
                // Clean up our output file
                File.Delete(item.FilePathBySlug);

                // Throw the exception up
                throw;
            }
        }

        /// <summary>
        /// Copy image to images directory, returning the URL on site (e.g. http://example.com/images/test.jpg)
        /// This method does not upload the image, it is assumed this will be done later on.
        /// </summary>
        /// <param name="filePath">Path to image on disk</param>
        /// <returns>URL to image on site</returns>
        private string DoPostImage(string filePath)
        {
            // Generate a unique file name
            var fileExt = Path.GetExtension(filePath);
            var uniqueName = string.Empty;

            for (var i = 0; i <= 1000; i++)
            {
                uniqueName = Path.GetFileNameWithoutExtension(filePath)?.Replace(" ", string.Empty) ?? string.Empty;

                if (i == 1000)
                {
                    // Failed to find a unique file name, return a GUID
                    uniqueName = Guid.NewGuid().ToString();
                    break;
                }

                if (i > 0)
                {
                    uniqueName += $"-{i}";
                }

                if (!File.Exists(Path.Combine(this.config.LocalSitePath, this.config.ImagesPath, uniqueName + fileExt)))
                {
                    break;
                }
            }

            // Copy the image to the images path
            File.Copy(filePath, Path.Combine(this.config.LocalSitePath, this.config.ImagesPath, uniqueName + fileExt));

            // I attempted to return an absolute server path here, however other parts of OLW expect a fully formed URI
            // This may cause issue for users who decide to relocate their site to a different URL.
            // I also attempted to strip the protocol here, however C# does not think protocol-less URIs are valid
            return Path.Combine(this.config.SiteUrl, this.config.ImagesPath, uniqueName + fileExt).Replace("\\", "/");
        }

        /// <summary>
        /// Build the static site
        /// </summary>
        private void DoSiteBuild()
        {
            var proc = this.RunSiteCommand(this.config.BuildCommand, out var stdout, out var stderr);
            if (proc.ExitCode != 0)
            {
                throw new BlogClientException(
                    StringId.SSGBuildErrorTitle,
                    StringId.SSGBuildErrorText,
                    Res.Get(StringId.ProductNameVersioned),
                    proc.ExitCode.ToString(),
                    this.config.ShowCmdWindows ? "N/A" : stdout,
                    this.config.ShowCmdWindows ? "N/A" : stderr);
            }
        }

        /// <summary>
        /// Publish the static site
        /// </summary>
        private void DoSitePublish()
        {
            var proc = this.RunSiteCommand(this.config.PublishCommand, out var stdout, out var stderr);
            if (proc.ExitCode != 0)
            {
                throw new BlogClientException(
                    StringId.SSGPublishErrorTitle,
                    StringId.SSGPublishErrorText,
                    Res.Get(StringId.ProductNameVersioned),
                    proc.ExitCode.ToString(),
                    this.config.ShowCmdWindows ? "N/A" : stdout,
                    this.config.ShowCmdWindows ? "N/A" : stderr);
            }
        }

        /// <summary>
        /// Run a command from the site directory
        /// </summary>
        /// <param name="localCommand">Command to run, relative to site directory</param>
        /// <param name="outStdout">The standard output.</param>
        /// <param name="outStderr">The standard error.</param>
        /// <returns>The <see cref="Process"/>.</returns>
        private Process RunSiteCommand(string localCommand, out string outStdout, out string outStderr)
        {
            var proc = new Process();
            var stdout = string.Empty;
            var stderr = string.Empty;

            // If a 32-bit process on a 64-bit system, call the 64-bit cmd
            proc.StartInfo.FileName = !Environment.Is64BitProcess && Environment.Is64BitOperatingSystem
                                          ? $"{Environment.GetEnvironmentVariable("windir")}\\Sysnative\\cmd.exe"
                                          : // 32-on-64, launch sys native cmd
                                          "cmd.exe"; // Launch regular cmd

            // Set working directory to local site path
            proc.StartInfo.WorkingDirectory = this.config.LocalSitePath;

            proc.StartInfo.RedirectStandardInput = !this.config.ShowCmdWindows;
            proc.StartInfo.RedirectStandardError = !this.config.ShowCmdWindows;
            proc.StartInfo.RedirectStandardOutput = !this.config.ShowCmdWindows;
            proc.StartInfo.CreateNoWindow = !this.config.ShowCmdWindows;
            proc.StartInfo.UseShellExecute = false;

            proc.StartInfo.Arguments = $"/C {localCommand}";

            if (!this.config.ShowCmdWindows)
            {
                proc.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            stdout += e.Data;
                            Trace.WriteLine($"StaticSiteClient stdout: {e.Data}");
                        }
                    };

                proc.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            stderr += e.Data;
                            Trace.WriteLine($"StaticSiteClient stderr: {e.Data}");
                        }
                    };
            }

            proc.Start();
            if (!this.config.ShowCmdWindows)
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }

            if (this.config.CmdTimeoutMs < 0)
            {
                // If timeout is negative, timeout is disabled.
                proc.WaitForExit();
            }
            else
            {
                if (!proc.WaitForExit(this.config.CmdTimeoutMs))
                {
                    // Timeout reached
                    try
                    {
                        proc.Kill();
                    }
                    catch
                    {
                        // ignored
                    }

                    // Attempt to kill the process
                    throw new BlogClientException(
                        Res.Get(StringId.SSGErrorCommandTimeoutTitle),
                        Res.Get(StringId.SSGErrorCommandTimeoutText));
                }
            }

            // The caller will have all output waiting in outStdout and outStderr
            outStdout = stdout;
            outStderr = stderr;
            return proc;
        }
    }
}
