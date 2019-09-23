// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

#define APIHACK

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Windows.Forms;
    using System.Xml;

    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The AtomClient class.
    /// Implements the <see cref="BlogClientBase" />
    /// Implements the <see cref="IBlogClient" />
    /// </summary>
    /// <seealso cref="BlogClientBase" />
    /// <seealso cref="IBlogClient" />
    public abstract partial class AtomClient : BlogClientBase, IBlogClient
    {
        /// <summary>
        /// The entry content type
        /// </summary>
        public const string EntryContentType = "application/atom+xml;type=entry";

        /// <summary>
        /// The SkyDrive entry content type
        /// </summary>
        public const string SkydriveEntryContentType = AtomClient.EntryContentType; // "application/atom+xml";

        /// <summary>
        /// The features namespace
        /// </summary>
        private const string FeaturesNamespaceString = "http://purl.org/atompub/features/1.0";

        /// <summary>
        /// The live namespace.
        /// </summary>
        private const string LiveNamespaceString = "http://api.live.com/schemas";

        /// <summary>
        /// The media namespace.
        /// </summary>
        private const string MediaNamespaceString = "http://search.yahoo.com/mrss/";

        /// <summary>
        /// The XHTML namespace.
        /// </summary>
        private const string XHtmlNamespaceString = "http://www.w3.org/1999/xhtml";

        /// <summary>
        /// The XHTML namespace.
        /// </summary>
        internal static readonly Namespace xhtmlNamespace = new Namespace(AtomClient.XHtmlNamespaceString, "xhtml");

        /// <summary>
        /// The features namespace.
        /// </summary>
        internal static readonly Namespace featuresNamespace = new Namespace(AtomClient.FeaturesNamespaceString, "f");

        /// <summary>
        /// The media namespace.
        /// </summary>
        internal static readonly Namespace mediaNamespace = new Namespace(AtomClient.MediaNamespaceString, "media");

        /// <summary>
        /// The live namespace.
        /// </summary>
        internal static readonly Namespace liveNamespace = new Namespace(AtomClient.LiveNamespaceString, "live");

        /// <summary>
        /// The XML rest request helper.
        /// </summary>
        protected internal static XmlRestRequestHelper xmlRestRequestHelper = new XmlRestRequestHelper();

        /// <summary>
        /// The atom namespace.
        /// </summary>
        internal readonly Namespace atomNamespace;

        /// <summary>
        /// The pub namespace.
        /// </summary>
        internal readonly Namespace pubNamespace;

        /// <summary>
        /// The atom version.
        /// </summary>
        protected internal AtomProtocolVersion atomVersion;

        /// <summary>
        /// The namespace manager.
        /// </summary>
        protected internal XmlNamespaceManager NamespaceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomClient"/> class.
        /// </summary>
        /// <param name="atomVersion">The atom version.</param>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        protected AtomClient(AtomProtocolVersion atomVersion, Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(credentials)
        {
            this.FeedServiceUrl = postApiUrl;

            // configure client options
            var clientOptions = new BlogClientOptions();
            this.ConfigureClientOptions(clientOptions);
            this.Options = clientOptions;

            this.atomVersion = atomVersion;
            this.atomNamespace = new Namespace(atomVersion.NamespaceUri, "atom");
            this.pubNamespace = new Namespace(atomVersion.PubNamespaceUri, "app");
            this.NamespaceManager = new XmlNamespaceManager(new NameTable());
            this.NamespaceManager.AddNamespace(this.atomNamespace.Prefix, this.atomNamespace.Uri);
            this.NamespaceManager.AddNamespace(this.pubNamespace.Prefix, this.pubNamespace.Uri);
            this.NamespaceManager.AddNamespace(AtomClient.xhtmlNamespace.Prefix, AtomClient.xhtmlNamespace.Uri);
            this.NamespaceManager.AddNamespace(AtomClient.featuresNamespace.Prefix, AtomClient.featuresNamespace.Uri);
            this.NamespaceManager.AddNamespace(AtomClient.mediaNamespace.Prefix, AtomClient.mediaNamespace.Uri);
            this.NamespaceManager.AddNamespace(AtomClient.liveNamespace.Prefix, AtomClient.liveNamespace.Uri);
        }

        /// <inheritdoc />
        public virtual bool IsSecure
        {
            get
            {
                try
                {
                    return UrlHelper.SafeToAbsoluteUri(this.FeedServiceUrl).StartsWith(
                        "https://",
                        StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        public IBlogClientOptions Options { get; private set; }

        /// <summary>
        /// Gets the category scheme.
        /// </summary>
        /// <value>The category scheme.</value>
        protected virtual string CategoryScheme => string.Empty;

        /// <summary>
        /// Gets the feed service URL.
        /// </summary>
        /// <value>The feed service URL.</value>
        protected virtual Uri FeedServiceUrl { get; }

        /// <summary>
        /// Gets the request filter.
        /// </summary>
        /// <value>The request filter.</value>
        protected virtual HttpRequestFilter RequestFilter => this.AuthorizationFilter;

        /// <summary>
        /// Confirms the overwrite.
        /// </summary>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public static bool ConfirmOverwrite() =>
            DialogResult.Yes == BlogClientUIContext.ShowDisplayMessageOnUIThread(MessageId.ConfirmOverwrite);

        /// <summary>
        /// Gets the e-tag.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="requestFilter">The request filter.</param>
        /// <returns>The e-tag.</returns>
        public static string GetEtag(string uri, HttpRequestFilter requestFilter) =>
            AtomClient.GetEtagImpl(uri, requestFilter, "HEAD", "GET");

        /// <summary>
        /// Adds the category.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="category">The category.</param>
        /// <returns>The result.</returns>
        public virtual string AddCategory(string blogId, BlogPostCategory category) =>
            throw new BlogClientMethodUnsupportedException("AddCategory");

        /// <summary>
        /// Deletes the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="pageId">The page identifier.</param>
        public virtual void DeletePage(string blogId, string pageId) =>
            throw new BlogClientMethodUnsupportedException("DeletePage");

        /// <summary>
        /// Deletes the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        public void DeletePost(string blogId, string postId, bool publish)
        {
            this.Login();

            this.FixupBlogId(ref blogId);
            var editUri = this.PostIdToPostUri(postId);

            try
            {
                var sr = new RedirectHelper.SimpleRequest("DELETE", this.DeleteRequestFilter);
                var response = RedirectHelper.GetResponse(UrlHelper.SafeToAbsoluteUri(editUri), sr.Create);
                response.Close();
            }
            catch (Exception e)
            {
                if (e is WebException we && we.Response is HttpWebResponse httpWebResponse)
                {
                    switch (httpWebResponse.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.Gone:
                            return; // these are acceptable responses to a DELETE
                    }
                }

                if (!this.AttemptDeletePostRecover(e, blogId, UrlHelper.SafeToAbsoluteUri(editUri), publish))
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Does the after publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        public virtual void DoAfterPublishUploadWork(IFileUploadContext uploadContext) =>
            throw new BlogClientMethodUnsupportedException("UploadFileAfterPublish");

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result.</returns>
        public virtual string DoBeforePublishUploadWork(IFileUploadContext uploadContext) =>
            throw new BlogClientMethodUnsupportedException("UploadFileBeforePublish");

        /// <summary>
        /// Determines whether the file needs to be uploaded.
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
        public virtual bool EditPage(
            string blogId,
            BlogPost page,
            bool publish,
            out string etag,
            out XmlDocument remotePost) =>
            throw new BlogClientMethodUnsupportedException("EditPage");

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
        public virtual bool EditPost(
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish,
            out string etag,
            out XmlDocument remotePost)
        {
            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            this.Login();

            this.FixupBlogId(ref blogId);

            var doc = post.AtomRemotePost;
            var entryNode = doc.SelectSingleNode("/atom:entry", this.NamespaceManager) as XmlElement;

            // No documentUri is needed because we ensure xml:base is set on the root
            // when we retrieve from XmlRestRequestHelper
            this.Populate(post, null, entryNode, publish);
            var etagToMatch = AtomClient.FilterWeakEtag(post.ETag);

            try
            {
                retry:
                try
                {
                    var uri = this.PostIdToPostUri(post.Id);
                    AtomClient.xmlRestRequestHelper.Put(
                        ref uri,
                        etagToMatch,
                        this.RequestFilter,
                        AtomClient.EntryContentType,
                        doc,
                        this.Options.CharacterSet,
                        true,
                        out _);
                }
                catch (WebException we)
                {
                    if (we.Status != WebExceptionStatus.ProtocolError)
                    {
                        throw;
                    }

                    if (((HttpWebResponse)we.Response).StatusCode != HttpStatusCode.PreconditionFailed)
                    {
                        throw;
                    }

                    if (string.IsNullOrEmpty(etagToMatch))
                    {
                        throw;
                    }

                    HttpRequestHelper.LogException(we);

                    var currentEtag = this.GetEtag(UrlHelper.SafeToAbsoluteUri(this.PostIdToPostUri(post.Id)));

                    if (string.IsNullOrEmpty(currentEtag) || currentEtag == etagToMatch)
                    {
                        throw;
                    }

                    if (!AtomClient.ConfirmOverwrite())
                    {
                        throw new BlogClientOperationCancelledException();
                    }

                    etagToMatch = currentEtag;
                    goto retry;
                }
            }
            catch (Exception e)
            {
                if (!this.AttemptEditPostRecover(
                        e,
                        blogId,
                        post,
                        newCategoryContext,
                        publish,
                        out etag,
                        out remotePost))
                {
                    // convert to a provider exception if this is a 404 (allow us to
                    // catch this case explicitly and attempt a new post to recover)
                    if (!(e is WebException))
                    {
                        // no special handling, just re-throw
                        throw;
                    }

                    var webEx = (WebException)e;
                    if (webEx.Response is HttpWebResponse response && response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new BlogClientProviderException("404", e.Message);
                    }

                    // no special handling, just re-throw
                    throw;
                }
            }

            var getUri = this.PostIdToPostUri(post.Id);
            remotePost = AtomClient.xmlRestRequestHelper.Get(ref getUri, this.RequestFilter, out var responseHeaders);
            etag = AtomClient.FilterWeakEtag(responseHeaders?["ETag"]);
            Trace.Assert(remotePost != null, "After successful PUT, remote post could not be retrieved");

            if (this.Options.SupportsNewCategories)
            {
                foreach (var category in post.NewCategories)
                {
                    newCategoryContext.NewCategoryAdded(category);
                }
            }

            return true;
        }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{AuthorInfo}"/>.</returns>
        public virtual AuthorInfo[] GetAuthors(string blogId) =>
            throw new BlogClientMethodUnsupportedException("GetAuthors");

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>A <see cref="Array{BlogPostCategory}"/>.</returns>
        public virtual BlogPostCategory[] GetCategories(string blogId)
        {
            var categoryList = new ArrayList();

            var xmlDoc = this.GetCategoryXml(ref blogId);
            foreach (var categoriesNode in xmlDoc.DocumentElement?.SelectNodes("app:categories", this.NamespaceManager)
                                                ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                var categoriesScheme = categoriesNode.GetAttribute("scheme");
                foreach (var categoryNode in categoriesNode
                                            .SelectNodes("atom:category", this.NamespaceManager)?.Cast<XmlElement>()
                                            .ToList() ?? new List<XmlElement>())
                {
                    var categoryScheme = categoryNode.GetAttribute("scheme");
                    if (categoryScheme == string.Empty)
                    {
                        categoryScheme = categoriesScheme;
                    }

                    if (this.CategoryScheme == categoryScheme)
                    {
                        var categoryName = categoryNode.GetAttribute("term");
                        var categoryLabel = categoryNode.GetAttribute("label");
                        if (categoryLabel == string.Empty)
                        {
                            categoryLabel = categoryName;
                        }

                        categoryList.Add(new BlogPostCategory(categoryName, categoryLabel));
                    }
                }
            }

            return (BlogPostCategory[])categoryList.ToArray(typeof(BlogPostCategory));
        }

        /// <summary>
        /// Gets the e-tag.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns>The e-tag.</returns>
        public string GetEtag(string uri) => AtomClient.GetEtag(uri, this.RequestFilter);

        /// <summary>
        /// Gets the image endpoints.
        /// </summary>
        /// <returns>A <see cref="Array{BlogInfo}"/>.</returns>
        public virtual BlogInfo[] GetImageEndpoints() => throw new NotImplementedException();

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{BlogPostKeyword}"/>.</returns>
        public virtual BlogPostKeyword[] GetKeywords(string blogId)
        {
            Trace.Fail("AtomClient does not support GetKeywords!");
            return new BlogPostKeyword[] { };
        }

        /// <summary>
        /// Gets the page.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="pageId">The page identifier.</param>
        /// <returns>A <see cref="BlogPost"/>.</returns>
        public virtual BlogPost GetPage(string blogId, string pageId) =>
            throw new BlogClientMethodUnsupportedException("GetPage");

        /// <summary>
        /// Gets the page list.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{PageInfo}"/>.</returns>
        public virtual PageInfo[] GetPageList(string blogId) =>
            throw new BlogClientMethodUnsupportedException("GetPageList");

        /// <summary>
        /// Gets the pages.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPages">The maximum pages.</param>
        /// <returns>An <see cref="Array{BlogPost}"/>.</returns>
        public virtual BlogPost[] GetPages(string blogId, int maxPages) =>
            throw new BlogClientMethodUnsupportedException("GetPages");

        /// <inheritdoc />
        public BlogPost GetPost(string blogId, string postId)
        {
            this.Login();

            this.FixupBlogId(ref blogId);

            var uri = this.PostIdToPostUri(postId);
            var doc = AtomClient.xmlRestRequestHelper.Get(ref uri, this.RequestFilter, out var responseHeaders);
            var remotePost = (XmlDocument)doc.Clone();
            if (!(doc.SelectSingleNode("/atom:entry", this.NamespaceManager) is XmlElement entryNode))
            {
                throw new BlogClientInvalidServerResponseException(
                    "GetPost",
                    "No post entry returned from server",
                    doc.OuterXml);
            }

            var post = this.Parse(entryNode, true, uri);
            post.Id = postId;
            post.ETag = AtomClient.FilterWeakEtag(responseHeaders?["ETag"]);
            post.AtomRemotePost = remotePost;
            return post;
        }

        /// <inheritdoc />
        public virtual BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now) =>
            this.GetRecentPostsInternal(blogId, maxPosts, includeCategories, now);

        /// <summary>
        /// Gets the users blogs.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        public virtual BlogInfo[] GetUsersBlogs()
        {
            this.Login();
            return this.GetUsersBlogsInternal();
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
        public virtual string NewPage(
            string blogId,
            BlogPost page,
            bool publish,
            out string etag,
            out XmlDocument remotePost) =>
            throw new BlogClientMethodUnsupportedException("NewPage");

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
        /// <exception cref="BlogClientInvalidServerResponseException">POST - The HTTP response was missing the required Location header.</exception>
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
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            this.Login();

            this.FixupBlogId(ref blogId);

            var doc = new XmlDocument();
            var entryNode = doc.CreateElement(this.atomNamespace.Prefix, "entry", this.atomNamespace.Uri);
            doc.AppendChild(entryNode);
            this.Populate(post, null, entryNode, publish);

            string slug = null;
            if (this.Options.SupportsSlug)
            {
                slug = post.Slug;
            }

            var uri = new Uri(blogId);
            var result = AtomClient.xmlRestRequestHelper.Post(
                ref uri,
                new NewPostRequest(this, slug).RequestFilter,
                AtomClient.EntryContentType,
                doc,
                this.Options.CharacterSet,
                out var responseHeaders);

            etag = AtomClient.FilterWeakEtag(responseHeaders["ETag"]);
            var location = responseHeaders["Location"];
            if (string.IsNullOrEmpty(location))
            {
                throw new BlogClientInvalidServerResponseException(
                    "POST",
                    "The HTTP response was missing the required Location header.",
                    string.Empty);
            }

            if (location != responseHeaders["Content-Location"] || result == null)
            {
                var locationUri = new Uri(location);
                result = AtomClient.xmlRestRequestHelper.Get(
                    ref locationUri,
                    this.RequestFilter,
                    out var getResponseHeaders);
                etag = AtomClient.FilterWeakEtag(getResponseHeaders["ETag"]);
            }

            remotePost = (XmlDocument)result.Clone();
            this.Parse(result.DocumentElement, true, uri);

            if (this.Options.SupportsNewCategories)
            {
                foreach (var category in post.NewCategories)
                {
                    newCategoryContext.NewCategoryAdded(category);
                }
            }

            return this.PostUriToPostId(location);
        }

        /// <summary>
        /// Enable external users of the class to completely replace the client options.
        /// </summary>
        /// <param name="newClientOptions">The new client options.</param>
        public void OverrideOptions(IBlogClientOptions newClientOptions) => this.Options = newClientOptions;

        /// <summary>
        /// Parses the specified entry node.
        /// </summary>
        /// <param name="entryNode">The entry node.</param>
        /// <param name="includeCategories">if set to <c>true</c> [include categories].</param>
        /// <param name="documentUri">The document URI.</param>
        /// <returns>A <see cref="BlogPost"/>.</returns>
        public virtual BlogPost Parse(XmlElement entryNode, bool includeCategories, Uri documentUri)
        {
            var post = new BlogPost();
            var atomEntry = new AtomEntry(
                this.atomVersion,
                this.atomNamespace,
                this.CategoryScheme,
                this.NamespaceManager,
                documentUri,
                entryNode);

            post.Title = atomEntry.Title;
            post.Excerpt = atomEntry.Excerpt;
            post.Id = this.PostUriToPostId(atomEntry.EditUri);
            post.Permalink = atomEntry.Permalink;
            post.Contents = atomEntry.ContentHtml;
            post.DatePublished = atomEntry.PublishDate;
            if (this.Options.SupportsCategories && includeCategories)
            {
                post.Categories = atomEntry.Categories;
            }

            return post;
        }

        /// <summary>
        /// Sends the authenticated HTTP request.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="timeoutMs">The timeout milliseconds.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>An <see cref="HttpWebResponse"/>.</returns>
        public virtual HttpWebResponse SendAuthenticatedHttpRequest(
            string requestUri,
            int timeoutMs,
            HttpRequestFilter filter) =>
            BlogClientHelper.SendAuthenticatedHttpRequest(requestUri, filter, this.CreateCredentialsFilter(requestUri));

        /// <summary>
        /// Suggests the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="partialCategoryName">Partial name of the category.</param>
        /// <returns>A <see cref="Array{BlogPostCategory}"/>.</returns>
        public virtual BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName) =>
            throw new BlogClientMethodUnsupportedException("SuggestCategories");

        /// <summary>
        /// Subclasses should override this if there are particular exception conditions
        /// that can be repaired by modifying the URI. Return true if the request should
        /// be retried using the (possibly modified) URI, or false if the exception should
        /// be thrown by the caller.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="uri">The URI.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected virtual bool AttemptAlternateGetRecentPostUrl(Exception e, ref string uri) => false;

        /// <summary>
        /// Attempts the delete post recover.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected virtual bool AttemptDeletePostRecover(Exception e, string blogId, string postId, bool publish) =>
            false;

        /// <summary>
        /// Attempts the edit post recover.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="post">The post.</param>
        /// <param name="newCategoryContext">The new category context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected virtual bool AttemptEditPostRecover(
            Exception e,
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish,
            out string etag,
            out XmlDocument remotePost)
        {
            etag = null;
            remotePost = null;
            return false;
        }

        /// <summary>
        /// Enable subclasses to change the default client options
        /// </summary>
        /// <param name="clientOptions">The client options.</param>
        protected virtual void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
        }

        /// <summary>
        /// Creates the credentials filter.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <returns>An <see cref="HttpRequestFilter"/>.</returns>
        protected virtual HttpRequestFilter CreateCredentialsFilter(string requestUri)
        {
            var tc = this.Login();
            return HttpRequestCredentialsFilter.Create(tc.Username, tc.Password, requestUri, true);
        }

        /// <summary>
        /// Fixes up the blog identifier.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        protected virtual void FixupBlogId(ref string blogId)
        {
        }

        /// <summary>
        /// Gets the category XML.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="XmlDocument"/>.</returns>
        protected virtual XmlDocument GetCategoryXml(ref string blogId)
        {
            // Get the service document
            this.Login();

            this.FixupBlogId(ref blogId);

            var uri = this.FeedServiceUrl;
            var xmlDoc = AtomClient.xmlRestRequestHelper.Get(ref uri, this.RequestFilter);

            foreach (var entryEl in xmlDoc.SelectNodes(
                                        "app:service/app:workspace/app:collection",
                                        this.NamespaceManager)?.Cast<XmlElement>().ToList()
                                 ?? new List<XmlElement>())
            {
                var href = XmlHelper.GetUrl(entryEl, "@href", uri);
                if (blogId != href)
                {
                    continue;
                }

                var results = new XmlDocument();
                var rootElement = results.CreateElement("categoryInfo");
                results.AppendChild(rootElement);
                foreach (var categoriesNode in entryEl.SelectNodes("app:categories", this.NamespaceManager)
                                                     ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
                {
                    this.AddCategoriesXml(categoriesNode, rootElement, uri);
                }

                return results;
            }

            Trace.Fail($"Couldn't find collection in service document:\r\n{xmlDoc.OuterXml}");
            return new XmlDocument();
        }

        /// <summary>
        /// Gets the recent posts internal.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPosts">The maximum posts.</param>
        /// <param name="includeCategories">if set to <c>true</c> [include categories].</param>
        /// <param name="now">The now.</param>
        /// <returns>An <see cref="Array{BlogPost}"/>.</returns>
        protected BlogPost[] GetRecentPostsInternal(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            this.Login();

            this.FixupBlogId(ref blogId);

            var seenIds = new HashSet();

            var blogPosts = new ArrayList();
            try
            {
                while (true)
                {
                    XmlDocument doc;
                    var thisUri = new Uri(blogId);

                    // This while-loop nonsense is necessary because New Blogger has a bug
                    // where the official URL for getting recent posts doesn't work when
                    // the orderby=published flag is set, but there's an un-official URL
                    // that will work correctly. Therefore, subclasses need the ability
                    // to inspect exceptions that occur, along with the URI that was used
                    // to make the request, and determine whether an alternate URI should
                    // be used.
                    while (true)
                    {
                        try
                        {
                            doc = AtomClient.xmlRestRequestHelper.Get(ref thisUri, this.RequestFilter);
                            break;
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.ToString());
                            if (this.AttemptAlternateGetRecentPostUrl(e, ref blogId))
                            {
                            }
                            else
                            {
                                throw;
                            }
                        }
                    }

                    var nodeList = doc.SelectNodes("/atom:feed/atom:entry", this.NamespaceManager);
                    if (nodeList == null || nodeList.Count == 0)
                    {
                        break;
                    }

                    foreach (XmlElement node in nodeList)
                    {
                        var blogPost = this.Parse(node, includeCategories, thisUri);
                        if (blogPost != null)
                        {
                            if (seenIds.Contains(blogPost.Id))
                            {
                                throw new DuplicateEntryIdException();
                            }

                            seenIds.Add(blogPost.Id);

                            if (!now.HasValue || blogPost.DatePublished.CompareTo(now.Value) < 0)
                            {
                                blogPosts.Add(blogPost);
                            }
                        }

                        if (blogPosts.Count >= maxPosts)
                        {
                            break;
                        }
                    }

                    if (blogPosts.Count >= maxPosts)
                    {
                        break;
                    }

                    if (!(doc.SelectSingleNode(
                              "/atom:feed/atom:link[@rel='next']",
                              this.NamespaceManager) is XmlElement nextNode))
                    {
                        break;
                    }

                    blogId = XmlHelper.GetUrl(nextNode, "@href", thisUri);
                    if (blogId.Length == 0)
                    {
                        break;
                    }
                }
            }
            catch (DuplicateEntryIdException)
            {
                if (ApplicationDiagnostics.AutomationMode)
                {
                    Trace.WriteLine("Duplicate IDs detected in feed");
                }
                else
                {
                    Trace.Fail("Duplicate IDs detected in feed");
                }
            }

            return (BlogPost[])blogPosts.ToArray(typeof(BlogPost));
        }

        /// <summary>
        /// Gets the users blogs internal.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        protected BlogInfo[] GetUsersBlogsInternal()
        {
            var serviceUri = this.FeedServiceUrl;
            var xmlDoc = AtomClient.xmlRestRequestHelper.Get(ref serviceUri, this.RequestFilter);

            // Either the FeedServiceUrl points to a service document OR a feed.
            if (xmlDoc.SelectSingleNode("/app:service", this.NamespaceManager) != null)
            {
                var blogInfos = new ArrayList();
                foreach (var coll in xmlDoc.SelectNodes(
                                         "/app:service/app:workspace/app:collection",
                                         this.NamespaceManager)?.Cast<XmlElement>().ToList()
                                  ?? new List<XmlElement>())
                {
                    var promote = this.ShouldPromote(coll);

                    // does this collection accept entries?
                    var acceptNodes = coll.SelectNodes("app:accept", this.NamespaceManager);
                    var acceptsEntries = false;
                    if (acceptNodes != null && acceptNodes.Count == 0)
                    {
                        acceptsEntries = true;
                    }
                    else
                    {
                        if (acceptNodes != null && acceptNodes
                                                  .Cast<XmlElement>().Any(
                                                       acceptNode => AtomClient.AcceptsEntry(acceptNode.InnerText)))
                        {
                            acceptsEntries = true;
                        }
                    }

                    if (acceptsEntries)
                    {
                        var feedUrl = XmlHelper.GetUrl(coll, "@href", serviceUri);
                        if (string.IsNullOrEmpty(feedUrl))
                        {
                            continue;
                        }

                        // form title
                        var titleBuilder = new StringBuilder();
                        foreach (var titleContainerNode in new[] { coll.ParentNode as XmlElement, coll })
                        {
                            if (!(titleContainerNode.SelectSingleNode("atom:title", this.NamespaceManager) is XmlElement
                                      titleNode))
                            {
                                continue;
                            }

                            var titlePart = this.atomVersion.TextNodeToPlaintext(titleNode);
                            if (titlePart.Length == 0)
                            {
                                continue;
                            }

                            Res.LOCME("loc the separator between parts of the blog name");
                            if (titleBuilder.Length != 0)
                            {
                                titleBuilder.Append(" - ");
                            }

                            titleBuilder.Append(titlePart);
                        }

                        // get homepage URL
                        var homepageUrl = string.Empty;
                        var dummy = string.Empty;
                        var tempFeedUrl = new Uri(feedUrl);
                        var feedDoc = AtomClient.xmlRestRequestHelper.Get(ref tempFeedUrl, this.RequestFilter);
                        this.ParseFeedDoc(feedDoc, tempFeedUrl, false, ref homepageUrl, ref dummy);

                        // TODO: Sniff out the homepage URL
                        var blogInfo = new BlogInfo(feedUrl, titleBuilder.ToString().Trim(), homepageUrl);
                        if (promote)
                        {
                            blogInfos.Insert(0, blogInfo);
                        }
                        else
                        {
                            blogInfos.Add(blogInfo);
                        }
                    }
                }

                return (BlogInfo[])blogInfos.ToArray(typeof(BlogInfo));
            }
            {
                var title = string.Empty;
                var homepageUrl = string.Empty;

                this.ParseFeedDoc(xmlDoc, serviceUri, true, ref homepageUrl, ref title);

                return new[] { new BlogInfo(UrlHelper.SafeToAbsoluteUri(this.FeedServiceUrl), title, homepageUrl) };
            }
        }

        /// <summary>
        /// Take the blog post data and put it into the XML node.
        /// </summary>
        /// <param name="post">The post.</param>
        /// <param name="documentUri">The document URI.</param>
        /// <param name="node">The node.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        protected virtual void Populate(BlogPost post, Uri documentUri, XmlElement node, bool publish)
        {
            var atomEntry = new AtomEntry(
                this.atomVersion,
                this.atomNamespace,
                this.CategoryScheme,
                this.NamespaceManager,
                documentUri,
                node);

            if (post.IsNew)
            {
                atomEntry.GenerateId();
            }

            atomEntry.Title = post.Title;
            if (this.Options.SupportsExcerpt && !string.IsNullOrEmpty(post.Excerpt))
            {
                atomEntry.Excerpt = post.Excerpt;
            }

            // extra space is to work around AOL Journals XML parsing bug
            atomEntry.ContentHtml = post.Contents + " ";
            if (this.Options.SupportsCustomDate && post.HasDatePublishedOverride)
            {
                atomEntry.PublishDate = post.DatePublishedOverride;
            }

            if (this.Options.SupportsCategories)
            {
                atomEntry.ClearCategories();

                foreach (var cat in post.Categories)
                {
                    if (!BlogPostCategoryNone.IsCategoryNone(cat))
                    {
                        atomEntry.AddCategory(cat);
                    }
                }

                if (this.Options.SupportsNewCategories)
                {
                    foreach (var cat in post.NewCategories)
                    {
                        if (!BlogPostCategoryNone.IsCategoryNone(cat))
                        {
                            atomEntry.AddCategory(cat);
                        }
                    }
                }
            }

            if (this.Options.SupportsPostAsDraft)
            {
                // remove existing draft nodes
                while (true)
                {
                    var draftNode = node.SelectSingleNode(@"app:control/app:draft", this.NamespaceManager);
                    if (draftNode == null)
                    {
                        break;
                    }

                    draftNode.ParentNode?.RemoveChild(draftNode);
                }

                if (!publish)
                {
                    // ensure control node exists
                    var controlNode = node.SelectSingleNode(@"app:control", this.NamespaceManager);
                    if (controlNode == null)
                    {
                        controlNode = node.OwnerDocument?.CreateElement(
                            this.pubNamespace.Prefix,
                            "control",
                            this.pubNamespace.Uri);
                        if (controlNode != null)
                        {
                            node.AppendChild(controlNode);
                        }
                    }

                    // create new draft node
                    var newDraftNode = node.OwnerDocument?.CreateElement(
                        this.pubNamespace.Prefix,
                        "draft",
                        this.pubNamespace.Uri);
                    if (newDraftNode != null)
                    {
                        newDraftNode.InnerText = "yes";
                        controlNode?.AppendChild(newDraftNode);
                    }
                }
            }

            //// post.Categories;
            //// post.CommentPolicy;
            //// post.CopyFrom;
            //// post.Excerpt;
            //// post.HasDatePublishedOverride;
            //// post.Id;
            //// post.IsNew;
            //// post.IsTemporary;
            //// post.Keywords;
            //// post.Link;
            //// post.Permalink;
            //// post.PingUrls;
            //// post.ResetToNewPost;
            //// post.TrackbackPolicy;
        }

        /// <summary>
        /// Posts the identifier to post URI.
        /// </summary>
        /// <param name="postId">The post identifier.</param>
        /// <returns>The <see cref="Uri"/>.</returns>
        protected virtual Uri PostIdToPostUri(string postId) => new Uri(postId);

        /// <summary>
        /// Posts the URI to post identifier.
        /// </summary>
        /// <param name="postUri">The post URI.</param>
        /// <returns>The result.</returns>
        protected virtual string PostUriToPostId(string postUri) => postUri;

        /// <summary>
        /// Determines whether the element should promote.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns><c>true</c> if the element should promote, <c>false</c> otherwise.</returns>
        protected virtual bool ShouldPromote(XmlElement collection) => false;

        /// <summary>
        /// Accepts the entry.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        /// <returns><c>true</c> if accepted, <c>false</c> otherwise.</returns>
        private static bool AcceptsEntry(string contentType)
        {
            var values = MimeHelper.ParseContentType(contentType, true);
            var mainType = values[string.Empty] as string;

            switch (mainType)
            {
                case "entry":
                case "*/*":
                case "application/*":
                    return true;
                case "application/atom+xml":
                    var subType = values["type"] as string;
                    subType = subType?.Trim().ToUpperInvariant();

                    return subType == "ENTRY";

                default:
                    return false;
            }
        }

        /// <summary>
        /// Filters the weak e-tag.
        /// </summary>
        /// <param name="etag">The e-tag.</param>
        /// <returns>The resulting e-tag.</returns>
        private static string FilterWeakEtag(string etag) =>
            etag == null || !etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase) ? etag : null;

        /// <summary>
        /// Gets the e-tag implementation.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="requestFilter">The request filter.</param>
        /// <param name="methods">An array of HTTP methods that should be tried until one of them does not return 405.</param>
        /// <returns>The e-tag.</returns>
        private static string GetEtagImpl(string uri, HttpRequestFilter requestFilter, params string[] methods)
        {
            try
            {
                var response = RedirectHelper.GetResponse(
                    uri,
                    new RedirectHelper.SimpleRequest(methods[0], requestFilter).Create);
                try
                {
                    return AtomClient.FilterWeakEtag(response.Headers["ETag"]);
                }
                finally
                {
                    response.Close();
                }
            }
            catch (WebException we)
            {
                if (methods.Length <= 1 || we.Status != WebExceptionStatus.ProtocolError
                                        || !(we.Response is HttpWebResponse resp)
                                        || (resp.StatusCode != HttpStatusCode.MethodNotAllowed
                                        && resp.StatusCode != HttpStatusCode.NotImplemented))
                {
                    throw;
                }

                var newMethods = new string[methods.Length - 1];
                Array.Copy(methods, 1, newMethods, 0, newMethods.Length);
                return AtomClient.GetEtagImpl(uri, requestFilter, newMethods);
            }
        }

        /// <summary>
        /// Adds the categories XML.
        /// </summary>
        /// <param name="categoriesNode">The categories node.</param>
        /// <param name="containerNode">The container node.</param>
        /// <param name="baseUri">The base URI.</param>
        private void AddCategoriesXml(XmlElement categoriesNode, XmlElement containerNode, Uri baseUri)
        {
            while (true)
            {
                if (categoriesNode.HasAttribute("href"))
                {
                    var href = XmlHelper.GetUrl(categoriesNode, "@href", baseUri);
                    if (string.IsNullOrEmpty(href))
                    {
                        return;
                    }

                    var uri = new Uri(href);
                    if (baseUri != null && uri.Equals(baseUri))
                    {
                        return;
                    }

                    // detect simple cycles
                    var doc = AtomClient.xmlRestRequestHelper.Get(ref uri, this.RequestFilter);
                    var categories = (XmlElement)doc.SelectSingleNode(@"app:categories", this.NamespaceManager);
                    if (categories != null)
                    {
                        categoriesNode = categories;
                        baseUri = uri;
                        continue;
                    }
                }
                else
                {
                    if (containerNode.OwnerDocument != null)
                    {
                        containerNode.AppendChild(containerNode.OwnerDocument.ImportNode(categoriesNode, true));
                    }
                }

                break;
            }
        }

        /// <summary>
        /// Authorizations the filter.
        /// </summary>
        /// <param name="request">The request.</param>
        private void AuthorizationFilter(HttpWebRequest request) =>

            ////request.KeepAlive = true;
            ////request.ProtocolVersion = HttpVersion.Version11;
            request.Credentials = new NetworkCredential(this.Credentials.Username, this.Credentials.Password);

        /// <summary>
        /// Deletes the request filter.
        /// </summary>
        /// <param name="request">The request.</param>
        private void DeleteRequestFilter(HttpWebRequest request)
        {
            request.Headers["If-match"] = "*";
            this.RequestFilter(request);
        }

        /// <summary>
        /// Parses the feed document.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="baseUri">The base URI.</param>
        /// <param name="includeTitle">if set to <c>true</c> [include title].</param>
        /// <param name="homepageUrl">The homepage URL.</param>
        /// <param name="title">The title.</param>
        private void ParseFeedDoc(
            XmlDocument xmlDoc,
            Uri baseUri,
            bool includeTitle,
            ref string homepageUrl,
            ref string title)
        {
            if (includeTitle)
            {
                if (xmlDoc.SelectSingleNode(@"atom:feed/atom:title", this.NamespaceManager) is XmlElement titleElement)
                {
                    title = this.atomVersion.TextNodeToPlaintext(titleElement);
                }
            }

            foreach (var linkEl in xmlDoc.SelectNodes(@"atom:feed/atom:link[@rel='alternate']", this.NamespaceManager)
                                        ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                var contentTypeInfo = MimeHelper.ParseContentType(linkEl.GetAttribute("type"), true);
                switch (contentTypeInfo[string.Empty] as string)
                {
                    case "text/html":
                    case "application/xhtml+xml":
                        homepageUrl = XmlHelper.GetUrl(linkEl, "@href", baseUri);
                        return;
                }
            }
        }
    }
}
