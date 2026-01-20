// Copyright (c) .NET Foundation. All rights reserved. Licensed under the MIT license. See LICENSE
// file in the project root for details.

using OpenLiveWriter.Api;
using OpenLiveWriter.BlogClient.Clients;
using OpenLiveWriter.BlogClient.Providers;
using OpenLiveWriter.Controls;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace OpenLiveWriter.BlogClient
{
    public struct PostResult
    {
        public XmlDocument AtomRemotePost;
        public DateTime DatePublished;
        public string ETag;
        public string PostId;
    }

    /// <summary>
    /// Facade class for programmatically interacting with a blog
    /// </summary>
    public class Blog : IEditorAccount
    {
        private readonly IBlogSettingsAccessor _settings;

        private IBlogClient _blogClient;

        public Blog(string blogId) => this._settings = BlogSettings.ForBlogId(blogId);

        public Blog(IBlogSettingsAccessor accessor) => this._settings = accessor;

        ~Blog()
        {
            Trace.Fail("Failed to dispose Blog object");
        }

        private enum ContentFilterMode
        {
            Open,
            Publish
        };

        public string AdminUrl => FormatUrl(ClientOptions.AdminUrl);

        public AuthorInfo[] Authors
        {
            get
            {
                AuthorInfo[] authors = this._settings.Authors;
                if (authors != null)
                {
                    Array.Sort(authors, new Comparison<AuthorInfo>(delegate (AuthorInfo a, AuthorInfo b)
                                            {
                                                return a == null ^ b == null
                                                    ? (a == null) ? -1 : 1
                                                    : a == null ? 0 : string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
                                            }));
                }

                return authors;
            }
        }

        public IBlogProviderButtonDescription[] ButtonDescriptions => this._settings.ButtonDescriptions;

        public BlogPostCategory[] Categories => this._settings.Categories;

        public IBlogClientOptions ClientOptions => BlogClient.Options;

        public string DefaultView => ClientOptions.DefaultView;

        IEditorOptions IEditorAccount.EditorOptions => BlogClient.Options;

        public Icon FavIcon
        {
            get
            {
                try
                {
                    // HACK: overcome WordPress icon transparency issues
                    return this._settings.ProviderId == "556A165F-DA11-463c-BB4A-C77CC9047F22" ||
                        this._settings.ProviderId == "82E6C828-8764-4af1-B289-647FC84E7093"
                        ? ResourceHelper.LoadAssemblyResourceIcon("Images.WordPressFav.ico")
                        : this._settings.FavIcon != null ? new Icon(new MemoryStream(this._settings.FavIcon), 16, 16) : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public IBlogFileUploadSettings FileUploadSettings => this._settings.FileUploadSettings;

        public FileUploadSupport FileUploadSupport => this._settings.FileUploadSupport;

        public string HomepageBaseUrl
        {
            get
            {
                string baseUrl = HomepageUrl;
                Uri uri = new Uri(HomepageUrl);
                string path = uri.PathAndQuery;
                int queryIndex = path.IndexOf("?", StringComparison.OrdinalIgnoreCase);
                if (queryIndex != -1)
                {
                    path = path.Substring(0, queryIndex);
                }

                if (!path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    int lastPathIndex = path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                    string lastPathPart = path.Substring(lastPathIndex + 1);
                    if (lastPathPart.IndexOf(".", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        path = path.Substring(0, lastPathIndex);
                        string hostUrl = uri.GetLeftPart(UriPartial.Authority);
                        baseUrl = UrlHelper.UrlCombine(hostUrl, path);
                    }
                }

                return baseUrl;
            }
        }

        public string HomepageUrl => this._settings.HomepageUrl;

        public string HostBlogId => this._settings.HostBlogId;

        public Icon Icon
        {
            get
            {
                try
                {
                    Icon favIcon = FavIcon;
                    return favIcon ?? ApplicationEnvironment.ProductIconSmall;
                }
                catch
                {
                    return ApplicationEnvironment.ProductIconSmall;
                }
            }
        }

        public string Id => this._settings.Id;

        public Image Image
        {
            get
            {
                try
                {
                    return this._settings.Image != null ? new Bitmap(new MemoryStream(this._settings.Image)) : (Image)null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public bool IsSpacesBlog => this._settings.IsSpacesBlog;

        public BlogPostKeyword[] Keywords
        {
            get => this._settings.Keywords; set => this._settings.Keywords = value;
        }

        public string Name => this._settings.BlogName;

        public PageInfo[] PageList => this._settings.Pages;

        public string PostApiUrl => this._settings.PostApiUrl;

        public string ProviderId => this._settings.ProviderId;

        public string ServiceDisplayName =>
                // look for an option override
                BlogClient.Options.ServiceName != string.Empty ? BlogClient.Options.ServiceName : this._settings.ServiceName;

        public string ServiceName => this._settings.ServiceName;

        public SupportsFeature SupportsImageUpload => this._settings.FileUploadSupport == FileUploadSupport.FTP || ClientOptions.SupportsFileUpload
                    ? SupportsFeature.Yes
                    : SupportsFeature.No;

        public Image WatermarkImage
        {
            get
            {
                try
                {
                    return this._settings.WatermarkImage != null ? new Bitmap(new MemoryStream(this._settings.WatermarkImage)) : (Image)null;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Weblog client
        /// </summary>
        private IBlogClient BlogClient
        {
            get
            {
                if (this._blogClient == null)
                {
                    this._blogClient = BlogClientManager.CreateClient(this._settings);
                }

                return this._blogClient;
            }
        }

        public void DeletePost(string postId, bool isPage, bool publish)
        {
            if (isPage)
            {
                BlogClient.DeletePage(this._settings.HostBlogId, postId);
            }
            else
            {
                BlogClient.DeletePost(this._settings.HostBlogId, postId, publish);
            }
        }

        public void DisplayException(IWin32Window owner, Exception ex)
        {
            // display a custom display message for exceptions that have one registered, otherwise
            // display the generic error form
            if (ex is BlogClientProviderException)
            {
                IBlogProvider provider = BlogProviderManager.FindProvider(this._settings.ProviderId);
                if (provider != null)
                {
                    BlogClientProviderException pe = ex as BlogClientProviderException;
                    MessageId messageId = provider.DisplayMessageForProviderError(pe.ErrorCode, pe.ErrorString);
                    if (messageId != MessageId.None)
                    {
                        _ = DisplayMessage.Show(messageId, owner);
                        return;
                    }
                }
            }
            else if (ex is WebException we)
            {
                if (we.Response is HttpWebResponse resp)
                {
                    string friendlyError = HttpRequestHelper.GetFriendlyErrorMessage(we);
                    Trace.WriteLine("Server response body:\r\n" + friendlyError);
                    ex = new BlogClientHttpErrorException(
                        UrlHelper.SafeToAbsoluteUri(resp.ResponseUri),
                        friendlyError,
                        we);
                }
                else
                {
                    DisplayMessage msg = new DisplayMessage(MessageId.ErrorConnecting);
                    ex = new BlogClientException(msg.Title, msg.Text);
                }

                HttpRequestHelper.LogException(we);
            }

            // no custom message, use default UI
            DisplayableExceptionDisplayForm.Show(owner, ex);
        }

        public void Dispose()
        {
            this._settings?.Dispose();

            GC.SuppressFinalize(this);
        }

        public PostResult EditPost(BlogPost post, INewCategoryContext newCategoryContext, bool publish)
        {
            // initialize result (for edits the id never changes)
            PostResult result = new PostResult
            {
                PostId = post.Id
            };
            try
            {
                //apply any publishing filters and make the post
                using (new ContentFilterApplier(post, ClientOptions, ContentFilterMode.Publish))
                {
                    // make the post
                    _ = post.IsPage
                        ? BlogClient.EditPage(this._settings.HostBlogId, post, publish, out result.ETag, out result.AtomRemotePost)
                        : BlogClient.EditPost(this._settings.HostBlogId, post, newCategoryContext, publish, out result.ETag, out result.AtomRemotePost);
                }
                // note success
                this._settings.LastPublishFailed = false;
            }
            catch (BlogClientProviderException ex)
            {
                if (ErrorIsInvalidPostId(ex))
                {
                    return NewPost(post, newCategoryContext, publish);
                }
                else
                {
                    throw;
                }
            }
            catch
            {
                this._settings.LastPublishFailed = true;
                throw;
            }

            // determine the date-published based on whether there was an override
            result.DatePublished = post.HasDatePublishedOverride ? post.DatePublishedOverride : DateTimeHelper.UtcNow;

            // return result
            return result;
        }

        public BlogPost[] GetPages(int maxPages)
        {
            // get the pages
            BlogPost[] pages = BlogClient.GetPages(this._settings.HostBlogId, maxPages);

            // ensure they are marked with IsPage = true
            foreach (BlogPost page in pages)
            {
                page.IsPage = true;
            }

            // narrow the array to the "max" if necessary
            ArrayList pageList = new ArrayList();
            for (int i = 0; i < Math.Min(pages.Length, maxPages); i++)
            {
                _ = pageList.Add(pages[i]);
            }

            // return pages
            return pageList.ToArray(typeof(BlogPost)) as BlogPost[];
        }

        /// <summary>
        /// Get the version of the post currently residing on the server
        /// </summary>
        /// <param name="blogPost"></param>
        /// <returns></returns>
        public BlogPost GetPost(string postId, bool isPage)
        {
            BlogPost blogPost;
            if (isPage)
            {
                // get the page
                blogPost = BlogClient.GetPage(this._settings.HostBlogId, postId);

                // ensure it is marked as a page
                blogPost.IsPage = true;
            }
            else
            {
                blogPost = BlogClient.GetPost(this._settings.HostBlogId, postId);

                // if there is no permalink then attempt to construct one
                EnsurePermalink(blogPost);
            }

            // apply content filters
            blogPost.Contents = ContentFilterApplier.ApplyContentFilters(ClientOptions.ContentFilter, blogPost.Contents, ContentFilterMode.Open);

            // return the blog post
            return blogPost;
        }

        public string GetPostEditingUrl(string postId)
        {
            string pattern = ClientOptions.PostEditingUrlPostIdPattern;
            if (!string.IsNullOrEmpty(pattern))
            {
                Match m = Regex.Match(postId, pattern, RegexOptions.IgnoreCase);
                if (m.Success && m.Groups[1].Success)
                {
                    postId = m.Groups[1].Value;
                }
                else
                {
                    Trace.Fail("Parsing failed: " + postId);
                }
            }

            return FormatUrl(ClientOptions.PostEditingUrl, postId);
        }

        public BlogPost[] GetRecentPosts(int maxPosts, bool includeCategories)
        {
            BlogPost[] recentPosts = BlogClient.GetRecentPosts(this._settings.HostBlogId, maxPosts, includeCategories, null);
            foreach (BlogPost blogPost in recentPosts)
            {
                // apply content filters
                blogPost.Contents = ContentFilterApplier.ApplyContentFilters(ClientOptions.ContentFilter, blogPost.Contents, ContentFilterMode.Open);

                // if there is no permalink then attempt to construct one
                EnsurePermalink(blogPost);
            }

            return recentPosts;
        }

        /// <summary>
        /// Force a refresh of ClientOptions by forcing the re-creation of the _blogClient
        /// </summary>
        public void InvalidateClient() => this._blogClient = null;

        public PostResult NewPost(BlogPost post, INewCategoryContext newCategoryContext, bool publish)
        {
            // initialize result
            PostResult result = new PostResult();

            try
            {
                using (new ContentFilterApplier(post, ClientOptions, ContentFilterMode.Publish))
                {
                    // make the post
                    result.PostId = post.IsPage
                        ? BlogClient.NewPage(this._settings.HostBlogId, post, publish, out result.ETag, out result.AtomRemotePost)
                        : BlogClient.NewPost(this._settings.HostBlogId, post, newCategoryContext, publish, out result.ETag, out result.AtomRemotePost);
                }

                // note success
                this._settings.LastPublishFailed = false;
            }
            catch
            {
                this._settings.LastPublishFailed = true;
                throw;
            }

            // determine the date-published based on whether there was an override
            result.DatePublished = post.HasDatePublishedOverride ? post.DatePublishedOverride : DateTimeHelper.UtcNow;

            // return result
            return result;
        }

        public void RefreshAuthors() => this._settings.Authors = BlogClient.GetAuthors(this._settings.HostBlogId);

        public void RefreshCategories()
        {
            try
            {
                this._settings.Categories = BlogClient.GetCategories(this._settings.HostBlogId);
            }
            catch (BlogClientOperationCancelledException)
            {
            }
        }

        public void RefreshKeywords()
        {
            try
            {
                this._settings.Keywords = BlogClient.GetKeywords(this._settings.HostBlogId);
            }
            catch (BlogClientOperationCancelledException)
            {
            }
        }

        public void RefreshPageList() => this._settings.Pages = BlogClient.GetPageList(this._settings.HostBlogId);

        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs) => BlogClient.SendAuthenticatedHttpRequest(requestUri, timeoutMs, null);

        public HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter) => BlogClient.SendAuthenticatedHttpRequest(requestUri, timeoutMs, filter);

        public override string ToString() => this._settings.BlogName;

        public bool VerifyCredentials() => BlogClient.VerifyCredentials();

        private void EnsurePermalink(BlogPost blogPost)
        {
            if (blogPost.Permalink == string.Empty)
            {
                if (ClientOptions.PermalinkFormat != string.Empty)
                {
                    // construct the permalink from a pre-provided pattern
                    blogPost.Permalink = FormatUrl(ClientOptions.PermalinkFormat, blogPost.Id);
                }
            }
            else if (!UrlHelper.IsUrl(blogPost.Permalink))
            {
                // if it is not a URL, then we may need to combine it with the homepage url
                try
                {
                    string permalink = UrlHelper.UrlCombine(this._settings.HomepageUrl, blogPost.Permalink);
                    if (UrlHelper.IsUrl(permalink))
                    {
                        blogPost.Permalink = permalink;
                    }
                }
                catch
                {
                    // url combine can throw exceptions, ignore these
                }
            }
        }

        private bool ErrorIsInvalidPostId(BlogClientProviderException ex)
        {
            string faultCodePattern = BlogClient.Options.InvalidPostIdFaultCodePattern;
            string faultStringPattern = BlogClient.Options.InvalidPostIdFaultStringPattern;

            return faultCodePattern != string.Empty && faultStringPattern != string.Empty
                ? FaultCodeMatchesInvalidPostId(ex.ErrorCode, faultCodePattern) &&
                    FaultStringMatchesInvalidPostId(ex.ErrorString, faultStringPattern)
                : faultCodePattern == string.Empty
                    ? faultStringPattern != string.Empty && FaultStringMatchesInvalidPostId(ex.ErrorString, faultStringPattern)
                    : FaultCodeMatchesInvalidPostId(ex.ErrorCode, faultCodePattern);
        }

        private bool FaultCodeMatchesInvalidPostId(string faultCode, string pattern)
        {
            try // defend against invalid regex in provider or manifest file
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return regex.IsMatch(faultCode);
            }
            catch (ArgumentException e)
            {
                Trace.Fail("Error processing regular expression: " + e.ToString());
                return false;
            }
        }

        private bool FaultStringMatchesInvalidPostId(string faultString, string pattern)
        {
            try // defend against invalid regex in provider or manifest file
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return regex.IsMatch(faultString);
            }
            catch (ArgumentException e)
            {
                Trace.Fail("Error processing regular expression: " + e.ToString());
                return false;
            }
        }

        private string FormatUrl(string url) => FormatUrl(url, null);

        private string FormatUrl(string url, string postId) => BlogClientHelper.FormatUrl(url, this._settings.HomepageUrl, this._settings.PostApiUrl, this._settings.HostBlogId, postId);

        private class ContentFilterApplier : IDisposable
        {
            private readonly BlogPost _blogPost;
            private readonly string _originalContents;

            public ContentFilterApplier(BlogPost blogPost, IBlogClientOptions clientOptions, ContentFilterMode filterMode)
            {
                this._blogPost = blogPost;
                this._originalContents = this._blogPost.Contents;
                if (this._originalContents != null)
                {
                    this._blogPost.Contents = ApplyContentFilters(clientOptions.ContentFilter, this._originalContents, filterMode);
                }
            }

            public void Dispose() => this._blogPost.Contents = this._originalContents;

            internal static string ApplyContentFilters(string filters, string content, ContentFilterMode filterMode)
            {
                string[] contentFilters = filters.Split(',');
                foreach (string filterString in contentFilters)
                {
                    string contentFilter = filterString.Trim();
                    if (contentFilter != string.Empty)
                    {
                        IBlogPostContentFilter bpContentFilter = BlogPostContentFilters.CreateContentFilter(contentFilter);
                        content = filterMode == ContentFilterMode.Open ? bpContentFilter.OpenFilter(content) : bpContentFilter.PublishFilter(content);
                    }
                }

                return content;
            }
        }
    }
}