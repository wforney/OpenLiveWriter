// <copyright file="GoogleBloggerv3Client.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Xml;

    using Google.Apis.Auth.OAuth2;
    using Google.Apis.Auth.OAuth2.Flows;
    using Google.Apis.Auth.OAuth2.Responses;
    using Google.Apis.Blogger.v3;
    using Google.Apis.Blogger.v3.Data;
    using Google.Apis.Drive.v3;
    using Google.Apis.Services;
    using Google.Apis.Upload;
    using Google.Apis.Util;
    using Google.Apis.Util.Store;

    using Newtonsoft.Json;

    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    using GoogleDriveData = Google.Apis.Drive.v3.Data;

    /// <summary>
    /// The GoogleBloggerv3Client class.
    /// Implements the <see cref="BlogClientBase" />
    /// Implements the <see cref="IBlogClient" />
    /// Implements the <see cref="OpenLiveWriter.BlogClient.BlogClientBase" />
    /// Implements the <see cref="OpenLiveWriter.Extensibility.BlogClient.IBlogClient" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.BlogClient.BlogClientBase" />
    /// <seealso cref="OpenLiveWriter.Extensibility.BlogClient.IBlogClient" />
    /// <seealso cref="BlogClientBase" />
    /// <seealso cref="IBlogClient" />
    [BlogClient("GoogleBloggerv3", "GoogleBloggerv3")]
    public partial class GoogleBloggerv3Client : BlogClientBase, IBlogClient
    {
        /// <summary>
        /// The categories end point
        /// </summary>
        private const string CategoriesEndPoint = "/feeds/posts/summary?alt=json&max-results=0";

        /// <summary>
        /// The entry content type
        /// </summary>
        private const string EntryContentType = "application/atom+xml;type=entry";

        /// <summary>
        /// The features namespace URI string
        /// </summary>
        private const string FeaturesNamespaceUriString = "http://purl.org/atompub/features/1.0";

        /// <summary>
        /// The google photo namespace URI string
        /// </summary>
        private const string GooglePhotoNamespaceUriString = "http://schemas.google.com/photos/2007";

        /// <summary>
        /// The live namespace URI string
        /// </summary>
        private const string LiveNamespaceUriString = "http://api.live.com/schemas";

        /// <summary>
        /// The maximum retries
        /// </summary>
        private const int MaxRetries = 5;

        /// <summary>
        /// The media namespace URI string
        /// </summary>
        private const string MediaNamespaceUriString = "http://search.yahoo.com/mrss/";

        /// <summary>
        /// The XHTML namespace URI string
        /// </summary>
        private const string XhtmlNamespaceUriString = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// The Google API scopes
        /// </summary>
        public static readonly string[] GoogleApiScopes =
            {
                DriveService.Scope.DriveFile, BloggerService.Scope.Blogger
            };

        /// <summary>
        /// The label delimiter
        /// </summary>
        public static char LabelDelimiter = ',';

        /// <summary>
        /// Maximum number of results the Google Blogger v3 API will return in one request.
        /// </summary>
        public static int MaxResultsPerRequest = 500;

        /// <summary>
        /// The atom namespace
        /// </summary>
        private static readonly Namespace AtomNamespace = new Namespace(
            AtomProtocolVersion.V10DraftBlogger.NamespaceUri,
            "atom");

        /// <summary>
        /// The pub namespace
        /// </summary>
        private static readonly Namespace PubNamespace = new Namespace(
            AtomProtocolVersion.V10DraftBlogger.PubNamespaceUri,
            "app");

        /// <summary>
        /// The photo namespace
        /// </summary>
        private static readonly Namespace PhotoNamespace = new Namespace(
            GoogleBloggerv3Client.GooglePhotoNamespaceUriString,
            "gphoto");

        /// <summary>
        /// The namespace manager
        /// </summary>
        private readonly XmlNamespaceManager namespaceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleBloggerv3Client" /> class.
        /// </summary>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        public GoogleBloggerv3Client(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            // configure client options
            var clientOptions = new BlogClientOptions
                                    {
                                        SupportsCategories = true,
                                        SupportsMultipleCategories = true,
                                        SupportsNewCategories = true,
                                        SupportsCustomDate = true,
                                        SupportsExcerpt = false,
                                        SupportsSlug = false,
                                        SupportsFileUpload = true,
                                        SupportsKeywords = false,
                                        SupportsGetKeywords = false,
                                        SupportsPages = true,
                                        SupportsExtendedEntries = true,
                                        UseLocalTime = true
                                    };
            this.Options = clientOptions;

            this.namespaceManager = new XmlNamespaceManager(new NameTable());
            this.namespaceManager.AddNamespace(
                GoogleBloggerv3Client.AtomNamespace.Prefix,
                GoogleBloggerv3Client.AtomNamespace.Uri);
            this.namespaceManager.AddNamespace(
                GoogleBloggerv3Client.PubNamespace.Prefix,
                GoogleBloggerv3Client.PubNamespace.Uri);
            this.namespaceManager.AddNamespace(
                GoogleBloggerv3Client.PhotoNamespace.Prefix,
                GoogleBloggerv3Client.PhotoNamespace.Uri);
            this.namespaceManager.AddNamespace(AtomClient.xhtmlNamespace.Prefix, AtomClient.xhtmlNamespace.Uri);
            this.namespaceManager.AddNamespace(AtomClient.featuresNamespace.Prefix, AtomClient.featuresNamespace.Uri);
            this.namespaceManager.AddNamespace(AtomClient.mediaNamespace.Prefix, AtomClient.mediaNamespace.Uri);
            this.namespaceManager.AddNamespace(AtomClient.liveNamespace.Prefix, AtomClient.liveNamespace.Uri);
        }

        /// <inheritdoc />
        public bool IsSecure => true;

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public IBlogClientOptions Options { get; private set; }

        /// <summary>
        /// Gets the client secrets stream.
        /// </summary>
        /// <value>The client secrets stream.</value>
        private static Stream ClientSecretsStream =>
            ResourceHelper.LoadAssemblyResourceStream("Clients.GoogleBloggerv3Secrets.json");

        /// <summary>
        /// Gets the OAuth2 authorization asynchronous.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="taskCancellationToken">The task cancellation token.</param>
        /// <returns>The user credential.</returns>
        public static Task<UserCredential> GetOAuth2AuthorizationAsync(string blogId, CancellationToken taskCancellationToken) =>
            GoogleWebAuthorizationBroker.AuthorizeAsync(
                GoogleClientSecrets.Load(GoogleBloggerv3Client.ClientSecretsStream).Secrets,
                GoogleBloggerv3Client.GoogleApiScopes,
                blogId,
                taskCancellationToken,
                GoogleBloggerv3Client.GetCredentialsDataStoreForBlog(blogId));

        /// <summary>
        /// Adds the category.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="category">The category.</param>
        /// <returns>The result.</returns>
        public string AddCategory(string blogId, BlogPostCategory category) =>
            throw new BlogClientMethodUnsupportedException("AddCategory");

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="pageId">The page identifier.</param>
        public void DeletePage(string blogId, string pageId)
        {
            var deletePostRequest = this.GetService().Pages.Delete(blogId, pageId);
            deletePostRequest.Execute();
        }

        /// <summary>
        /// Deletes the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        public void DeletePost(string blogId, string postId, bool publish)
        {
            var deletePostRequest = this.GetService().Posts.Delete(blogId, postId);
            deletePostRequest.Execute();
        }

        /// <summary>
        /// Does the after publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        public void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
            // Nothing to do.
        }

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result.</returns>
        public string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            var albumName = ApplicationEnvironment.ProductName;

            var path = uploadContext.GetContentsLocalFilePath();

            if (!string.IsNullOrEmpty(this.Options.FileUploadNameFormat))
            {
                var formattedFileName = uploadContext.FormatFileName(uploadContext.PreferredFileName);
                var chunks = StringHelper.Reverse(formattedFileName).Split(new[] { '/' }, 2);
                if (chunks.Length == 2)
                {
                    albumName = StringHelper.Reverse(chunks[1]);
                }
            }

            return this.PostNewImage(albumName, path);
        }

        /// <summary>
        /// Determines if the file needs to be uploaded.
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
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPage = GoogleBloggerv3Client.ConvertToGoogleBloggerPage(page, this.Options);
            var updatePostRequest = this.GetService().Pages.Update(bloggerPage, blogId, page.Id);
            updatePostRequest.Publish = publish;

            var updatedPage = updatePostRequest.Execute();
            etag = updatedPage.ETag;
            return true;
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
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPost = GoogleBloggerv3Client.ConvertToGoogleBloggerPost(post, this.Options);
            var updatePostRequest = this.GetService().Posts.Update(bloggerPost, blogId, post.Id);
            updatePostRequest.Publish = publish;

            var updatedPost = updatePostRequest.Execute();
            etag = updatedPost.ETag;
            return true;
        }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>The <see cref="Array{AuthorInfo}" />.</returns>
        public AuthorInfo[] GetAuthors(string blogId) => throw new NotImplementedException();

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>The <see cref="Array{BlogPostCategory}" />.</returns>
        public BlogPostCategory[] GetCategories(string blogId)
        {
            var categories = new BlogPostCategory[0];
            var blog = this.GetService().Blogs.Get(blogId).Execute();

            if (blog == null)
            {
                return categories;
            }

            var categoriesUrl = string.Concat(blog.Url, GoogleBloggerv3Client.CategoriesEndPoint);

            var response = this.SendAuthenticatedHttpRequest(categoriesUrl, 30, this.CreateAuthorizationFilter());
            if (response == null)
            {
                return categories;
            }

            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var json = reader.ReadToEnd();
                var item = JsonConvert.DeserializeObject<CategoryResponse>(json);
                var cats = item?.Feed?.CategoryArray?.Select(x => new BlogPostCategory(x.Term));
                categories = cats?.ToArray() ?? new BlogPostCategory[0];
            }

            return categories;
        }

        /// <summary>
        /// Gets the image endpoints.
        /// </summary>
        /// <returns>The <see cref="Array{BlogInfo}" />.</returns>
        public BlogInfo[] GetImageEndpoints() => throw new NotImplementedException();

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>The <see cref="Array{BlogPostKeyword}" />.</returns>
        /// <remarks>Google Blogger does not support get labels</remarks>
        public BlogPostKeyword[] GetKeywords(string blogId) => Array.Empty<BlogPostKeyword>();

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns>The <see cref="BlogPost" />.</returns>
        public BlogPost GetPage(string blogId, string pageId)
        {
            var getPageRequest = this.GetService().Pages.Get(blogId, pageId);
            getPageRequest.View = PagesResource.GetRequest.ViewEnum.AUTHOR;
            return GoogleBloggerv3Client.ConvertToBlogPost(getPageRequest.Execute());
        }

        /// <summary>
        /// Gets the page list.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{PageInfo}" />.</returns>
        public PageInfo[] GetPageList(string blogId) =>
            this.ListAllPages(blogId, null).OrderByDescending(p => p.Published)
                .Select(GoogleBloggerv3Client.ConvertToPageInfo).ToArray();

        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPages">The maximum pages.</param>
        /// <returns>An <see cref="Array{BlogPost}" />.</returns>
        public BlogPost[] GetPages(string blogId, int maxPages) =>
            this.ListAllPages(blogId, maxPages).OrderByDescending(p => p.Published)
                .Select(GoogleBloggerv3Client.ConvertToBlogPost).Take(maxPages).ToArray();

        /// <inheritdoc />
        public BlogPost GetPost(string blogId, string postId)
        {
            var getPostRequest = this.GetService().Posts.Get(blogId, postId);
            getPostRequest.View = PostsResource.GetRequest.ViewEnum.AUTHOR;
            return GoogleBloggerv3Client.ConvertToBlogPost(getPostRequest.Execute());
        }

        /// <inheritdoc />
        public BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            var allPosts = new List<Post>();

            // We keep around the PostList returned by each request to support pagination.
            PostList draftRecentPostsList = null;
            PostList liveRecentPostsList = null;
            PostList scheduledRecentPostsList = null;

            // Google has a per-request results limit on their API.
            var maxResultsPerRequest = Math.Min(maxPosts, GoogleBloggerv3Client.MaxResultsPerRequest);

            // Blogger requires separate API calls to get drafts vs. live vs. scheduled posts. We aggregate each
            // type of post separately.
            IList<Post> draftRecentPosts;
            IList<Post> liveRecentPosts;
            IList<Post> scheduledRecentPosts;

            // We break out of the following loop depending on which one of these two cases we hit:
            // (a) the number of all blog posts ever posted to this blog is greater than maxPosts, so eventually
            // allPosts.count() will exceed maxPosts and we can stop making requests.
            // (b) the number of all blog posts ever posted to this blog is less than maxPosts, so eventually our
            // calls to ListRecentPosts() will return 0 results and we need to stop making requests.
            do
            {
                draftRecentPostsList = this.ListRecentPosts(
                    blogId,
                    maxResultsPerRequest,
                    now,
                    PostsResource.ListRequest.StatusEnum.Draft,
                    draftRecentPostsList);
                liveRecentPostsList = this.ListRecentPosts(
                    blogId,
                    maxResultsPerRequest,
                    now,
                    PostsResource.ListRequest.StatusEnum.Live,
                    liveRecentPostsList);
                scheduledRecentPostsList = this.ListRecentPosts(
                    blogId,
                    maxResultsPerRequest,
                    now,
                    PostsResource.ListRequest.StatusEnum.Scheduled,
                    scheduledRecentPostsList);

                draftRecentPosts = draftRecentPostsList?.Items ?? new List<Post>();
                liveRecentPosts = liveRecentPostsList?.Items ?? new List<Post>();
                scheduledRecentPosts = scheduledRecentPostsList?.Items ?? new List<Post>();
                allPosts = allPosts.Concat(draftRecentPosts).Concat(liveRecentPosts).Concat(scheduledRecentPosts)
                                   .ToList();
            }
            while (allPosts.Count < maxPosts
                && (draftRecentPosts.Count > 0 || liveRecentPosts.Count > 0 || scheduledRecentPosts.Count > 0));

            return allPosts.OrderByDescending(p => p.Published).Take(maxPosts)
                           .Select(GoogleBloggerv3Client.ConvertToBlogPost).ToArray();
        }

        /// <summary>
        /// Gets the users blogs.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}" />.</returns>
        public BlogInfo[] GetUsersBlogs()
        {
            var blogList = this.GetService().Blogs.ListByUser("self").Execute();
            return blogList.Items?.Select(b => new BlogInfo(b.Id, b.Name, b.Url)).ToArray() ?? new BlogInfo[0];
        }

        /// <summary>
        /// Creates new page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="page">The page.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns>The result.</returns>
        public string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost)
        {
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPage = GoogleBloggerv3Client.ConvertToGoogleBloggerPage(page, this.Options);
            var newPageRequest = this.GetService().Pages.Insert(bloggerPage, blogId);
            newPageRequest.IsDraft = !publish;

            var newPage = newPageRequest.Execute();
            etag = newPage.ETag;
            return newPage.Id;
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
        /// <returns>The result.</returns>
        public string NewPost(
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish,
            out string etag,
            out XmlDocument remotePost)
        {
            // The remote post is only meant to be used for blogs that use the Atom protocol.
            remotePost = null;

            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            var bloggerPost = GoogleBloggerv3Client.ConvertToGoogleBloggerPost(post, this.Options);
            var newPostRequest = this.GetService().Posts.Insert(bloggerPost, blogId);
            newPostRequest.IsDraft = !publish;

            var newPost = newPostRequest.Execute();
            etag = newPost.ETag;
            return newPost.Id;
        }

        /// <summary>
        /// Overrides the options.
        /// </summary>
        /// <param name="newClientOptions">The new client options.</param>
        public void OverrideOptions(IBlogClientOptions newClientOptions) => this.Options = newClientOptions;

        /// <summary>
        /// Sends the authenticated HTTP request.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="timeoutMs">The timeout milliseconds.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>An <see cref="HttpWebResponse" />.</returns>
        public HttpWebResponse
            SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter) =>
            BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, this.CreateAuthorizationFilter());

        /// <summary>
        /// Suggests the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="partialCategoryName">Partial name of the category.</param>
        /// <returns>An <see cref="Array{BlogPostCategory}" />.</returns>
        public BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName) =>
            throw new BlogClientMethodUnsupportedException("SuggestCategories");

        /// <inheritdoc />
        protected override TransientCredentials Login()
        {
            var transientCredentials = this.Credentials.TransientCredentials as TransientCredentials
                                    ?? new TransientCredentials(
                                           this.Credentials.Username,
                                           this.Credentials.Password,
                                           null);
            GoogleBloggerv3Client.VerifyAndRefreshCredentials(transientCredentials);
            this.Credentials.TransientCredentials = transientCredentials;
            return transientCredentials;
        }

        /// <inheritdoc />
        protected override void VerifyCredentials(TransientCredentials tc) =>
            GoogleBloggerv3Client.VerifyAndRefreshCredentials(tc);

        /// <summary>
        /// Converts to blog post.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>A <see cref="BlogPost" />.</returns>
        private static BlogPost ConvertToBlogPost(Page page) =>
            new BlogPost
                {
                    Title = page.Title,
                    Id = page.Id,
                    Permalink = page.Url,
                    Contents = page.Content,
                    DatePublished = page.Published.GetValueOrDefault()
                };

        /// <summary>
        /// Converts to blog post.
        /// </summary>
        /// <param name="post">The post.</param>
        /// <returns>A <see cref="BlogPost" />.</returns>
        private static BlogPost ConvertToBlogPost(Post post) =>
            new BlogPost
                {
                    Title = post.Title,
                    Id = post.Id,
                    Permalink = post.Url,
                    Contents = post.Content,
                    DatePublished = post.Published.GetValueOrDefault(),
                    Categories = post.Labels?.Select(x => new BlogPostCategory(x)).ToArray()
                              ?? new BlogPostCategory[0]
                };

        /// <summary>
        /// Converts to google blogger page.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="clientOptions">The client options.</param>
        /// <returns>The Google Blogger <see cref="Page" />.</returns>
        private static Page ConvertToGoogleBloggerPage(BlogPost page, IBlogClientOptions clientOptions) =>
            new Page
                {
                    Content = page.Contents,
                    Published = GoogleBloggerv3Client.GetDatePublishedOverride(page, clientOptions),
                    Title = page.Title
                };

        /// <summary>
        /// Converts to google blogger post.
        /// </summary>
        /// <param name="post">The post.</param>
        /// <param name="clientOptions">The client options.</param>
        /// <returns>The <see cref="Post" />.</returns>
        private static Post ConvertToGoogleBloggerPost(BlogPost post, IBlogClientOptions clientOptions)
        {
            var labels = post.Categories?.Select(x => x.Name).ToList();
            labels?.AddRange(post.NewCategories?.Select(x => x.Name) ?? new List<string>());

            return new Post
                       {
                           Content = post.Contents,
                           Labels = labels ?? new List<string>(),
                           Published = GoogleBloggerv3Client.GetDatePublishedOverride(post, clientOptions),
                           Title = post.Title
                       };
        }

        /// <summary>
        /// Converts to page information.
        /// </summary>
        /// <param name="page">The page.</param>
        /// <returns>A <see cref="PageInfo" />.</returns>
        private static PageInfo ConvertToPageInfo(Page page) =>
            new PageInfo(page.Id, page.Title, page.Published.GetValueOrDefault(DateTime.Now), string.Empty);

        /// <summary>
        /// Gets all folders.
        /// </summary>
        /// <param name="drive">The drive.</param>
        /// <returns>A <see cref="GoogleDriveData.File" />.</returns>
        private static IEnumerable<GoogleDriveData.File> GetAllFolders(DriveService drive)
        {
            // Navigate GDrive pagination and return a list of all the user's top level folders
            var folders = new List<GoogleDriveData.File>();
            string pageToken;
            do
            {
                var listRequest = drive.Files.List();
                listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
                var fileList = listRequest.Execute();

                if (fileList.Files != null)
                {
                    folders.AddRange(fileList.Files);
                }

                pageToken = fileList.NextPageToken;
            }
            while (pageToken != null);

            return folders;
        }

        /// <summary>
        /// Gets the blog images folder.
        /// </summary>
        /// <param name="drive">The drive.</param>
        /// <param name="folderName">Name of the folder.</param>
        /// <returns>A <see cref="GoogleDriveData.File" />.</returns>
        private static GoogleDriveData.File GetBlogImagesFolder(DriveService drive, string folderName)
        {
            // Get the ID of the Google Drive 'Open Live Writer' folder, creating it if it doesn't exist
            var matchingFolders = GoogleBloggerv3Client.GetAllFolders(drive).Where(folder => folder.Name == folderName)
                                                       .ToList();
            if (matchingFolders.Any())
            {
                return matchingFolders.First();
            }

            // Attempt to create and return the folder as it does not exist
            return drive.Files.Create(
                new GoogleDriveData.File
                    {
                        Name = folderName, MimeType = "application/vnd.google-apps.folder"
                    }).Execute();
        }

        /// <summary>
        /// Gets the credentials data store for blog.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="IDataStore" />.</returns>
        private static IDataStore GetCredentialsDataStoreForBlog(string blogId)
        {
            // The Google APIs will automatically store the OAuth2 tokens in the given path.
            var folderPath = Path.Combine(ApplicationEnvironment.ApplicationDataDirectory, "GoogleBloggerv3");
            return new FileDataStore(folderPath, true);
        }

        /// <summary>
        /// Gets the date published override.
        /// </summary>
        /// <param name="post">The post.</param>
        /// <param name="clientOptions">The client options.</param>
        /// <returns>The date published override.</returns>
        private static DateTime? GetDatePublishedOverride(BlogPost post, IBlogClientOptions clientOptions)
        {
            var datePublishedOverride = post.HasDatePublishedOverride ? post?.DatePublishedOverride : null;
            if (datePublishedOverride.HasValue && clientOptions.UseLocalTime)
            {
                datePublishedOverride = DateTimeHelper.UtcToLocal(datePublishedOverride.Value);
            }

            return datePublishedOverride;
        }

        /// <summary>
        /// Determines whether [is valid token] [the specified token].
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns><c>true</c> if [is valid token] [the specified token]; otherwise, <c>false</c>.</returns>
        private static bool IsValidToken(TokenResponse token) =>
            token != null && (!token.IsExpired(SystemClock.Default) || token.RefreshToken != null);

        /// <summary>
        /// Verifies the and refresh credentials.
        /// </summary>
        /// <param name="tc">The transient credentials.</param>
        private static void VerifyAndRefreshCredentials(TransientCredentials tc)
        {
            var userCredential = tc.Token as UserCredential;
            var token = userCredential?.Token;

            if (GoogleBloggerv3Client.IsValidToken(token))
            {
                // We already have a valid OAuth token.
                return;
            }

            if (userCredential == null)
            {
                // Attempt to load a cached OAuth token.
                var flow = new GoogleAuthorizationCodeFlow(
                    new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecretsStream = GoogleBloggerv3Client.ClientSecretsStream,
                            DataStore = GoogleBloggerv3Client.GetCredentialsDataStoreForBlog(tc.Username),
                            Scopes = GoogleBloggerv3Client.GoogleApiScopes
                        });

                var loadTokenTask = flow.LoadTokenAsync(tc.Username, CancellationToken.None);
                loadTokenTask.Wait();
                if (loadTokenTask.IsCompleted)
                {
                    // We were able re-create the user credentials from the cache.
                    userCredential = new UserCredential(flow, tc.Username, loadTokenTask.Result);
                    token = loadTokenTask.Result;
                }
            }

            if (!GoogleBloggerv3Client.IsValidToken(token))
            {
                // The token is invalid, so we need to login again. This likely includes popping out a new browser window.
                if (BlogClientUIContext.SilentModeForCurrentThread)
                {
                    // If we're in silent mode where prompting isn't allowed, throw the verification exception
                    throw new BlogClientAuthenticationException(string.Empty, string.Empty);
                }

                // Start an OAuth flow to renew the credentials.
                var authorizationTask =
                    GoogleBloggerv3Client.GetOAuth2AuthorizationAsync(tc.Username, CancellationToken.None);
                authorizationTask.Wait();
                if (authorizationTask.IsCompleted)
                {
                    userCredential = authorizationTask.Result;
                    token = userCredential?.Token;
                }
            }

            if (!GoogleBloggerv3Client.IsValidToken(token))
            {
                // The token is still invalid after all of our attempts to refresh it. The user did not complete the
                // authorization flow, so we interpret that as a cancellation.
                throw new BlogClientOperationCancelledException();
            }

            // Stash the valid user credentials.
            tc.Token = userCredential;
        }

        /// <summary>
        /// Creates the authorization filter.
        /// </summary>
        /// <returns>An <see cref="HttpRequestFilter" />.</returns>
        private HttpRequestFilter CreateAuthorizationFilter()
        {
            var transientCredentials = this.Login();
            var userCredential = (UserCredential)transientCredentials.Token;
            var accessToken = userCredential.Token.AccessToken;

            return request =>
                {
                    // OAuth uses a Bearer token in the HTTP Authorization header.
                    request.Headers.Add(
                        HttpRequestHeader.Authorization,
                        string.Format(CultureInfo.InvariantCulture, "Bearer {0}", accessToken));
                };
        }

        /// <summary>
        /// Gets the drive service.
        /// </summary>
        /// <returns>The <see cref="DriveService" />.</returns>
        private DriveService GetDriveService()
        {
            var transientCredentials = this.Login();
            return new DriveService(
                new BaseClientService.Initializer
                    {
                        HttpClientInitializer = (UserCredential)transientCredentials.Token,
                        ApplicationName = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1}",
                            ApplicationEnvironment.ProductName,
                            ApplicationEnvironment.ProductVersion)
                    });
        }

        /// <summary>
        /// Gets the service.
        /// </summary>
        /// <returns>The <see cref="BloggerService" />.</returns>
        private BloggerService GetService()
        {
            var transientCredentials = this.Login();
            return new BloggerService(
                new BaseClientService.Initializer
                    {
                        HttpClientInitializer = (UserCredential)transientCredentials.Token,
                        ApplicationName = string.Format(
                            CultureInfo.InvariantCulture,
                            "{0} {1}",
                            ApplicationEnvironment.ProductName,
                            ApplicationEnvironment.ProductVersion)
                    });
        }

        /// <summary>
        /// Lists all pages.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPages">The maximum pages.</param>
        /// <returns>The pages.</returns>
        private IEnumerable<Page> ListAllPages(string blogId, int? maxPages)
        {
            var allPages = new List<Page>();

            // We keep around the PageList returned by each request to support pagination.
            PageList draftPagesList = null;
            PageList livePagesList = null;

            // Blogger requires separate API calls to get drafts vs. live vs. scheduled posts. We aggregate each
            // type of post separately.
            IList<Page> draftPages;
            IList<Page> livePages;

            // We break out of the following loop depending on which one of these two cases we hit:
            // (a) the number of all blog pages ever posted to this blog is greater than maxPages, so eventually
            // allPages.count() will exceed maxPages and we can stop making requests.
            // (b) the number of all blog pages ever posted to this blog is less than maxPages, so eventually our
            // calls to ListPages() will return 0 results and we need to stop making requests.
            do
            {
                draftPagesList = this.ListPages(
                    blogId,
                    maxPages,
                    PagesResource.ListRequest.StatusEnum.Draft,
                    draftPagesList);
                livePagesList = this.ListPages(
                    blogId,
                    maxPages,
                    PagesResource.ListRequest.StatusEnum.Live,
                    livePagesList);

                draftPages = draftPagesList?.Items ?? new List<Page>();
                livePages = livePagesList?.Items ?? new List<Page>();
                allPages = allPages.Concat(draftPages).Concat(livePages).ToList();
            }
            while (allPages.Count < maxPages && (draftPages.Count > 0 || livePages.Count > 0));

            return allPages;
        }

        /// <summary>
        /// Lists the pages.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPages">The maximum pages.</param>
        /// <param name="status">The status.</param>
        /// <param name="previousPage">The previous page.</param>
        /// <returns>The <see cref="PageList" />.</returns>
        private PageList ListPages(
            string blogId,
            int? maxPages,
            PagesResource.ListRequest.StatusEnum status,
            PageList previousPage)
        {
            if (previousPage != null && string.IsNullOrWhiteSpace(previousPage.NextPageToken))
            {
                // The previous page was also the last page, so do nothing and return an empty list.
                return new PageList();
            }

            var getPagesRequest = this.GetService().Pages.List(blogId);
            if (maxPages.HasValue)
            {
                // Google has a per-request results limit on their API.
                getPagesRequest.MaxResults = Math.Min(maxPages.Value, GoogleBloggerv3Client.MaxResultsPerRequest);
            }

            getPagesRequest.Status = status;
            return getPagesRequest.Execute();
        }

        /// <summary>
        /// Lists the recent posts.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPosts">The maximum posts.</param>
        /// <param name="now">The now.</param>
        /// <param name="status">The status.</param>
        /// <param name="previousPage">The previous page.</param>
        /// <returns>The <see cref="PostList" />.</returns>
        private PostList ListRecentPosts(
            string blogId,
            int maxPosts,
            DateTime? now,
            PostsResource.ListRequest.StatusEnum status,
            PostList previousPage)
        {
            if (previousPage != null && string.IsNullOrWhiteSpace(previousPage.NextPageToken))
            {
                // The previous page was also the last page, so do nothing and return an empty list.
                return new PostList();
            }

            var recentPostsRequest = this.GetService().Posts.List(blogId);
            if (now.HasValue)
            {
                recentPostsRequest.EndDate = now.Value;
            }

            recentPostsRequest.FetchImages = false;
            recentPostsRequest.MaxResults = maxPosts;
            recentPostsRequest.OrderBy = PostsResource.ListRequest.OrderByEnum.Published;
            recentPostsRequest.Status = status;
            recentPostsRequest.PageToken = previousPage?.NextPageToken;

            return recentPostsRequest.Execute();
        }

        /// <summary>
        /// Posts the new image.
        /// </summary>
        /// <param name="imagesFolderName">Name of the images folder.</param>
        /// <param name="filename">The filename.</param>
        /// <returns>The result.</returns>
        /// <exception cref="BlogClientFileTransferException">BloggerDriveError - Google Drive image upload for {Path.GetFileName(filename)} failed.\nDetails: {uploadRes.Exception}</exception>
        private string PostNewImage(string imagesFolderName, string filename)
        {
            using (var drive = this.GetDriveService())
            {
                var imagesFolder = GoogleBloggerv3Client.GetBlogImagesFolder(drive, imagesFolderName);
                FilesResource.CreateMediaUpload uploadReq;

                // Create a FileStream for the image to upload
                using (var imageFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    // Detect mime type for file based on extension
                    var imageMime = MimeMapping.GetMimeMapping(filename);

                    // Upload the image to the images folder, naming it with a GUID to prevent clashes
                    uploadReq = drive.Files.Create(
                        new GoogleDriveData.File
                            {
                                Name = Guid.NewGuid().ToString(),
                                Parents = new[] { imagesFolder.Id },
                                OriginalFilename = Path.GetFileName(filename)
                            },
                        imageFileStream,
                        imageMime);
                    uploadReq.Fields = "id,webContentLink"; // Retrieve Id and WebContentLink fields
                    var uploadRes = uploadReq.Upload();
                    if (uploadRes.Status != UploadStatus.Completed)
                    {
                        throw new BlogClientFileTransferException(
                            string.Format(
                                Res.Get(StringId.BCEFileTransferTransferringFile),
                                Path.GetFileName(filename)),
                            "BloggerDriveError",
                            $"Google Drive image upload for {Path.GetFileName(filename)} failed.\nDetails: {uploadRes.Exception}");
                    }
                }

                // Make the uploaded file public
                var imageFile = uploadReq.ResponseBody;
                drive.Permissions.Create(
                    new GoogleDriveData.Permission { Type = "anyone", Role = "reader" },
                    imageFile.Id).Execute();

                // Retrieve the appropriate URL for inlining the image, splitting off the download parameter
                return imageFile.WebContentLink.Split('&').First();
            }
        }

        /// <summary>
        /// Refreshes the access token.
        /// </summary>
        /// <param name="transientCredentials">The transient credentials.</param>
        private void RefreshAccessToken(TransientCredentials transientCredentials)
        {
            // Using the BloggerService automatically refreshes the access token, but we call the Picasa endpoint
            // directly and therefore need to force refresh the access token on occasion.
            var userCredential = transientCredentials.Token as UserCredential;
            userCredential?.RefreshTokenAsync(CancellationToken.None).Wait();
        }
    }
}
