// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Windows.Forms;
    using System.Xml;

    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.HtmlParser.Parser;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The BloggerAtomClient class.
    /// Implements the <see cref="OpenLiveWriter.BlogClient.Clients.AtomClient" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.BlogClient.Clients.AtomClient" />
    [BlogClient("BloggerAtom", "Atom")]
    public partial class BloggerAtomClient : AtomClient
    {
        /// <summary>
        /// Used to guard against recursion when attempting delete post recovery.
        /// </summary>
        [ThreadStatic]
        private static bool inDeletePostRecovery;

        /// <summary>
        /// Used to guard against recursion when attempting edit post recovery.
        /// </summary>
        [ThreadStatic]
        private static bool inEditPostRecovery;

        /// <summary>
        /// Initializes a new instance of the <see cref="BloggerAtomClient"/> class.
        /// </summary>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        public BloggerAtomClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(AtomProtocolVersion.V10DraftBlogger, postApiUrl, credentials)
        {
        }

        /// <inheritdoc />
        public override bool IsSecure => true;

        /// <summary>
        /// Gets the category scheme.
        /// </summary>
        /// <value>The category scheme.</value>
        protected override string CategoryScheme => "http://www.blogger.com/atom/ns#";

        /// <summary>
        /// Gets the request filter.
        /// </summary>
        /// <value>The request filter.</value>
        protected override HttpRequestFilter RequestFilter => this.BloggerAuthorizationFilter;

        /// <summary>
        /// Does the after publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        public override void DoAfterPublishUploadWork(IFileUploadContext uploadContext)
        {
        }

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result.</returns>
        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
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

            const string EditMediaLink = "EditMediaLink";
            string srcUrl;
            var editUri = uploadContext.Settings.GetString(EditMediaLink, null);
            if (string.IsNullOrEmpty(editUri))
            {
                this.PostNewImage(albumName, path, out srcUrl, out editUri);
            }
            else
            {
                try
                {
                    this.UpdateImage(editUri, path, out srcUrl, out editUri);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    if (e is WebException webException)
                    {
                        HttpRequestHelper.LogException(webException);
                    }

                    var success = false;
                    srcUrl = null; // compiler complains without this line
                    try
                    {
                        // couldn't update existing image? try posting a new one
                        this.PostNewImage(albumName, path, out srcUrl, out editUri);
                        success = true;
                    }
                    catch
                    {
                        // ignored
                    }

                    if (!success)
                    {
                        throw; // rethrow the exception from the update, not the post
                    }
                }
            }

            uploadContext.Settings.SetString(EditMediaLink, editUri);

            this.PicasaRefererBlockingWorkaround(uploadContext.BlogId, uploadContext.Role, ref srcUrl);

            return srcUrl;
        }

        /// <summary>
        /// Gets the blog images album.
        /// </summary>
        /// <param name="albumName">Name of the album.</param>
        /// <returns>The blog images album.</returns>
        public string GetBlogImagesAlbum(string albumName)
        {
            const string FeedRelUriString = "http://schemas.google.com/g/2005#feed";
            const string GooglePhotosNamespaceUriString = "http://schemas.google.com/photos/2007";

            // TransientCredentials transientCredentials = Credentials.TransientCredentials as TransientCredentials;
            // TODO: HACK: The deprecation-extension flag keeps the deprecated Picasa API alive.
            var picasaUri = new Uri(
                "https://picasaweb.google.com/data/feed/api/user/default?deprecation-extension=true");

            try
            {
                var reqUri = picasaUri;
                var albumListDoc = AtomClient.xmlRestRequestHelper.Get(
                    ref reqUri,
                    this.PicasaAuthorizationFilter,
                    "kind",
                    "album");
                foreach (var entryEl in albumListDoc.SelectNodes(@"/atom:feed/atom:entry", this.NamespaceManager)
                                                   ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
                {
                    if (!(entryEl.SelectSingleNode(@"atom:title", this.NamespaceManager) is XmlElement titleNode))
                    {
                        continue;
                    }

                    var titleText = this.atomVersion.TextNodeToPlaintext(titleNode);
                    if (titleText != albumName)
                    {
                        continue;
                    }

                    var namespaceManager = new XmlNamespaceManager(new NameTable());
                    namespaceManager.AddNamespace("gphoto", "http://schemas.google.com/photos/2007");
                    var numPhotosRemainingNode = entryEl.SelectSingleNode(
                        "gphoto:numphotosremaining/text()",
                        namespaceManager);
                    if (numPhotosRemainingNode != null)
                    {
                        if (int.TryParse(
                            numPhotosRemainingNode.Value,
                            NumberStyles.Integer,
                            CultureInfo.InvariantCulture,
                            out var numPhotosRemaining))
                        {
                            if (numPhotosRemaining < 1)
                            {
                                continue;
                            }
                        }
                    }

                    var selfHref = AtomEntry.GetLink(
                        entryEl,
                        this.NamespaceManager,
                        FeedRelUriString,
                        "application/atom+xml",
                        null,
                        reqUri);
                    if (selfHref.Length > 1)
                    {
                        return selfHref;
                    }
                }
            }
            catch (WebException we)
            {
                if (!(we.Response is HttpWebResponse httpWebResponse))
                {
                    throw;
                }

                HttpRequestHelper.DumpResponse(httpWebResponse);
                if (httpWebResponse.StatusCode != HttpStatusCode.NotFound)
                {
                    throw;
                }

                BlogClientUIContext.ContextForCurrentThread.Invoke(
                    new EventHandler(BloggerAtomClient.ShowPicasaSignupPrompt),
                    null);
                throw new BlogClientOperationCancelledException();
            }

            var newDoc = new XmlDocument();
            var newEntryEl = newDoc.CreateElement("atom", "entry", this.atomVersion.NamespaceUri);
            newDoc.AppendChild(newEntryEl);

            var newTitleEl = newDoc.CreateElement("atom", "title", this.atomVersion.NamespaceUri);
            newTitleEl.SetAttribute("type", "text");
            newTitleEl.InnerText = albumName;
            newEntryEl.AppendChild(newTitleEl);

            var newSummaryEl = newDoc.CreateElement("atom", "summary", this.atomVersion.NamespaceUri);
            newSummaryEl.SetAttribute("type", "text");
            newSummaryEl.InnerText = Res.Get(StringId.BloggerImageAlbumDescription);
            newEntryEl.AppendChild(newSummaryEl);

            var newAccessEl = newDoc.CreateElement("gphoto", "access", GooglePhotosNamespaceUriString);
            newAccessEl.InnerText = "private";
            newEntryEl.AppendChild(newAccessEl);

            var newCategoryEl = newDoc.CreateElement("atom", "category", this.atomVersion.NamespaceUri);
            newCategoryEl.SetAttribute("scheme", "http://schemas.google.com/g/2005#kind");
            newCategoryEl.SetAttribute("term", "http://schemas.google.com/photos/2007#album");
            newEntryEl.AppendChild(newCategoryEl);

            var postUri = picasaUri;
            var newAlbumResult = AtomClient.xmlRestRequestHelper.Post(
                ref postUri,
                this.PicasaAuthorizationFilter,
                "application/atom+xml",
                newDoc,
                null);
            var newAlbumResultEntryEl =
                newAlbumResult.SelectSingleNode("/atom:entry", this.NamespaceManager) as XmlElement;
            Debug.Assert(newAlbumResultEntryEl != null, "New album result entry element cannot be null.");
            return AtomEntry.GetLink(
                newAlbumResultEntryEl,
                this.NamespaceManager,
                FeedRelUriString,
                "application/atom+xml",
                null,
                postUri);
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>A <see cref="Array{BlogPostCategory}"/>.</returns>
        /// <exception cref="BlogClientInvalidServerResponseException">metafeed - The expected blog information was not returned by Blogger. - null</exception>
        public override BlogPostCategory[] GetCategories(string blogId)
        {
            // get metafeed
            this.Login();

            var metafeedUri = new Uri("http://www.blogger.com/feeds/default/blogs");
            var xmlDoc = AtomClient.xmlRestRequestHelper.Get(ref metafeedUri, this.RequestFilter);

            var entryEl = xmlDoc.SelectSingleNode(
                              $@"atom:feed/atom:entry[atom:link[@rel='http://schemas.google.com/g/2005#post' and @href='{blogId}']]",
                              this.NamespaceManager) as XmlElement;
            Res.LOCME("Blogger error message");
            if (entryEl == null)
            {
                throw new BlogClientInvalidServerResponseException(
                    "metafeed",
                    "The expected blog information was not returned by Blogger.",
                    null);
            }

            var categoryList = new ArrayList();
            foreach (var categoryNode in entryEl.SelectNodes(
                                             "atom:category[@scheme='http://www.blogger.com/atom/ns#']",
                                             this.NamespaceManager)?.Cast<XmlNode>().ToList()
                                      ?? new List<XmlNode>())
            {
                var categoryName = ((XmlElement)categoryNode).GetAttribute("term");
                categoryList.Add(new BlogPostCategory(categoryName));
            }

            return (BlogPostCategory[])categoryList.ToArray(typeof(BlogPostCategory));
        }

        /// <inheritdoc />
        public override BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            // By default, New Blogger returns blog posts in last-updated order. We
            // want them in last-published order, so they match up with how it looks
            // on the blog.
            var url = blogId;
            url += $"{(url.IndexOf('?') < 0 ? '?' : '&')}orderby=published";

            return this.GetRecentPostsInternal(url, maxPosts, includeCategories, now);
        }

        /// <summary>
        /// Gets the users blogs.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        public override BlogInfo[] GetUsersBlogs()
        {
            var metafeed = new Uri("http://www.blogger.com/feeds/default/blogs");
            var xmlDoc = AtomClient.xmlRestRequestHelper.Get(ref metafeed, this.RequestFilter);

            var blogInfos = new ArrayList();
            foreach (var entryEl in xmlDoc.SelectNodes(@"atom:feed/atom:entry", this.NamespaceManager)
                                         ?.Cast<XmlElement>().ToList() ?? new List<XmlElement>())
            {
                var feedUrl = string.Empty;
                var title = string.Empty;
                var homepageUrl = string.Empty;

                if (entryEl.SelectSingleNode(
                        @"atom:link[@rel='http://schemas.google.com/g/2005#post' and @type='application/atom+xml']",
                        this.NamespaceManager) is XmlElement feedUrlEl)
                {
                    feedUrl = XmlHelper.GetUrl(feedUrlEl, "@href", metafeed);
                }

                if (entryEl.SelectSingleNode(@"atom:title", this.NamespaceManager) is XmlElement titleEl)
                {
                    title = this.atomVersion.TextNodeToPlaintext(titleEl);
                }

                if (entryEl.SelectSingleNode(
                        @"atom:link[@rel='alternate' and @type='text/html']",
                        this.NamespaceManager) is XmlElement linkEl)
                {
                    homepageUrl = linkEl.GetAttribute("href");
                }

                blogInfos.Add(new BlogInfo(feedUrl, title, homepageUrl));
            }

            return (BlogInfo[])blogInfos.ToArray(typeof(BlogInfo));
        }

        /// <summary>
        /// Parses the specified entry node.
        /// </summary>
        /// <param name="entryNode">The entry node.</param>
        /// <param name="includeCategories">if set to <c>true</c> [include categories].</param>
        /// <param name="documentUri">The document URI.</param>
        /// <returns>A <see cref="BlogPost"/>.</returns>
        public override BlogPost Parse(XmlElement entryNode, bool includeCategories, Uri documentUri)
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

            var content = atomEntry.ContentHtml;
            if (content.Trim() != string.Empty)
            {
                var ex = new HtmlExtractor(content);
                if (this.Options.SupportsExtendedEntries && ex.Seek("<a name=\"more\">").Success)
                {
                    var start = ex.Element.Offset;
                    var length = ex.Element.Length;
                    post.SetContents(
                        content.Substring(0, start),
                        ex.Seek("</a>").Success
                            ? content.Substring(ex.Element.Offset + ex.Element.Length)
                            : content.Substring(start + length));
                }
                else
                {
                    post.Contents = content;
                }
            }

            post.DatePublished = atomEntry.PublishDate;
            if (this.Options.SupportsCategories && includeCategories)
            {
                post.Categories = atomEntry.Categories;
            }

            return post;
        }

        /// <summary>
        /// Attempts the alternate get recent post URL.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="uri">The URI.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected override bool AttemptAlternateGetRecentPostUrl(Exception e, ref string uri)
        {
            var alternateUri = uri;

            if (e is WebException webException)
            {
                if (webException.Response is HttpWebResponse response)
                {
                    switch (response.StatusCode)
                    {
                        /* We have two separate problems to deal with here.
                         *
                         * For New Blogger blogs, passing orderby=published to www.blogger.com
                         * will currently result in a 400 (Bad Request). We need to do the same
                         * request to www2.blogger.com.
                         *
                         * For Old Blogger blogs, passing orderby=published is going to fail no
                         * matter what. However, we don't know in advance whether this blog is
                         * Old Blogger or New Blogger. So we assume we are New Blogger, retry
                         * the request with www2.blogger.com as above, and if we are Old Blogger
                         * then that request will fail with a 401. When that happens we can try
                         * again on www.blogger.com without orderby=published.
                         */
                        case HttpStatusCode.BadRequest
                            when uri.StartsWith("http://www.blogger.com/", StringComparison.OrdinalIgnoreCase)
                              && uri.IndexOf("orderby=published", StringComparison.OrdinalIgnoreCase) >= 0:
                            // www.blogger.com still can't handle orderby=published. Switch to
                            // www2.blogger.com and try the request again.
                            alternateUri = Regex.Replace(
                                uri,
                                "^" + Regex.Escape("http://www.blogger.com/"),
                                "http://www2.blogger.com/",
                                RegexOptions.IgnoreCase);
                            break;
                        case HttpStatusCode.Unauthorized
                            when uri.StartsWith("http://www2.blogger.com/", StringComparison.OrdinalIgnoreCase)
                              && uri.IndexOf("orderby=published", StringComparison.OrdinalIgnoreCase) >= 0:
                            // This is Old Blogger after all. Switch to www.blogger.com and remove the
                            // orderby=published param.

                            // Need to be very careful when removing orderby=published. Blogger freaks
                            // out with a 400 when any little thing is wrong with the query string.
                            // Examples of URLs that cause errors:
                            // http://www2.blogger.com/feeds/7150790056788550577/posts/default?
                            // http://www2.blogger.com/feeds/7150790056788550577/posts/default?&start-index=26
                            // http://www2.blogger.com/feeds/7150790056788550577/posts/default?start-index=26&
                            // http://www2.blogger.com/feeds/7150790056788550577/posts/default?&
                            // http://www2.blogger.com/feeds/7150790056788550577/posts/default&
                            var r1 = new Regex("^" + Regex.Escape("http://www2.blogger.com/"), RegexOptions.IgnoreCase);
                            var r2 = new Regex(@"(\?|&)orderby=published\b(&?)");

                            if (r1.IsMatch(uri) && r2.IsMatch(uri))
                            {
                                alternateUri = r1.Replace(uri, "http://www.blogger.com/");
                                alternateUri = r2.Replace(alternateUri, BloggerAtomClient.OrderByClauseRemover);
                            }

                            break;
                    }
                }
            }

            if (alternateUri == uri)
            {
                return false;
            }

            uri = alternateUri;
            return true;
        }

        /// <summary>
        /// Attempts the delete post recover.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected override bool AttemptDeletePostRecover(Exception e, string blogId, string postId, bool publish)
        {
            // There's a bug with Blogger Beta where their atom feeds are returning
            // edit URIs for entries that don't work for PUT and DELETE.  However, if you do a GET on the
            // edit URI, you can get a different edit URI that DOES work for PUT and DELETE.
            if (BloggerAtomClient.inDeletePostRecovery)
            {
                return false;
            }

            BloggerAtomClient.inDeletePostRecovery = true;
            try
            {
                if (BloggerAtomClient.IsBadRequestError(e))
                {
                    var post = this.GetPost(blogId, postId);
                    if (post.Id != postId)
                    {
                        this.DeletePost(blogId, post.Id, publish);
                        return true;
                    }
                }
            }
            catch (Exception e1)
            {
                Trace.Fail(e1.ToString());
            }
            finally
            {
                BloggerAtomClient.inDeletePostRecovery = false;
            }

            // it didn't work.
            return false;
        }

        /// <summary>
        /// Attempts the edit post recover.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="post">The post.</param>
        /// <param name="newCategoryContext">The new category context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <param name="etag">The e-tag.</param>
        /// <param name="remotePost">The remote post.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        protected override bool AttemptEditPostRecover(
            Exception e,
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish,
            out string etag,
            out XmlDocument remotePost)
        {
            // There's a bug with Blogger Beta where their atom feeds are returning
            // edit URIs for entries that don't work for PUT and DELETE.  However, if you do a GET on the
            // edit URI, you can get a different edit URI that DOES work for PUT and DELETE.
            if (BloggerAtomClient.inEditPostRecovery)
            {
                etag = null;
                remotePost = null;
                return false;
            }

            BloggerAtomClient.inEditPostRecovery = true;
            try
            {
                if (BloggerAtomClient.IsBadRequestError(e))
                {
                    var oldPost = this.GetPost(blogId, post.Id);
                    if (post.Id != oldPost.Id)
                    {
                        // Temporarily change the ID to this alternate Edit URI.  In order to
                        // minimize the chance of unintended side effects, we revert the ID
                        // back to the original value once we're done trying to edit.
                        var originalId = post.Id;
                        try
                        {
                            post.Id = oldPost.Id;
                            if (this.EditPost(blogId, post, newCategoryContext, publish, out etag, out remotePost))
                            {
                                return true;
                            }
                        }
                        finally
                        {
                            post.Id = originalId;
                        }
                    }
                }
            }
            catch (Exception e1)
            {
                Trace.Fail(e1.ToString());
            }
            finally
            {
                BloggerAtomClient.inEditPostRecovery = false;
            }

            etag = null;
            remotePost = null;
            return false;
        }

        /// <summary>
        /// Configures the client options.
        /// </summary>
        /// <param name="clientOptions">The client options.</param>
        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsCustomDate = true;
            clientOptions.SupportsCategories = true;
            clientOptions.SupportsMultipleCategories = true;
            clientOptions.SupportsNewCategories = true;
            clientOptions.SupportsNewCategoriesInline = true;
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsExtendedEntries = true;
        }

        /// <summary>
        /// Gets the category XML.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>The category XML.</returns>
        protected override XmlDocument GetCategoryXml(ref string blogId) => throw new NotImplementedException();

        /// <inheritdoc />
        protected override TransientCredentials Login()
        {
            var tc = base.Login();
            BloggerAtomClient.VerifyAndRefreshCredentials(tc);
            return tc;
        }

        /// <inheritdoc />
        protected override void VerifyCredentials(TransientCredentials tc) =>
            BloggerAtomClient.VerifyAndRefreshCredentials(tc);

        /// <summary>
        /// Determines whether [is bad request error] [the specified e].
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns><c>true</c> if [is bad request error] [the specified e]; otherwise, <c>false</c>.</returns>
        private static bool IsBadRequestError(Exception e) =>
            e is WebException we && we.Response is HttpWebResponse resp && resp.StatusCode == HttpStatusCode.BadRequest;

        /// <summary>
        /// Orders the by clause remover.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns>The result.</returns>
        private static string OrderByClauseRemover(Match match)
        {
            var prefix = match.Groups[1].Value;
            var hasSuffix = match.Groups[2].Success;

            return hasSuffix ? prefix : string.Empty;
        }

        /// <summary>
        /// Shows the picasa signup prompt.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private static void ShowPicasaSignupPrompt(object sender, EventArgs e)
        {
            if (DisplayMessage.Show(MessageId.PicasawebSignup) == DialogResult.Yes)
            {
                ShellHelper.LaunchUrl("http://picasaweb.google.com");
            }
        }

        /// <summary>
        /// Verifies the and refresh credentials.
        /// </summary>
        /// <param name="tc">The transient credentials.</param>
        private static void VerifyAndRefreshCredentials(TransientCredentials tc)
        {
            var gc = GDataCredentials.FromCredentials(tc);

            if (gc.IsValid(tc.Username, tc.Password, GDataCredentials.BloggerServiceName))
            {
            }
            else
            {
                var showUI = !BlogClientUIContext.SilentModeForCurrentThread;
                gc.EnsureLoggedIn(tc.Username, tc.Password, GDataCredentials.BloggerServiceName, showUI);
            }
        }

        /// <summary>
        /// Authorizes the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        private bool AuthorizeRequest(HttpWebRequest request, string serviceName)
        {
            // This line is required to avoid Error 500 from non-beta Blogger blogs.
            // According to Pete Hopkins it is "something with .NET".
            request.Accept = "*/*";

            var transientCredentials = this.Login();
            var cred = GDataCredentials.FromCredentials(transientCredentials);
            return cred.AttachCredentialsIfValid(
                request,
                transientCredentials.Username,
                transientCredentials.Password,
                serviceName);
        }

        /// <summary>
        /// Bloggers the authorization filter.
        /// </summary>
        /// <param name="request">The request.</param>
        private void BloggerAuthorizationFilter(HttpWebRequest request) =>
            this.AuthorizeRequest(request, GDataCredentials.BloggerServiceName);

        /// <summary>
        /// Parses the media entry.
        /// </summary>
        /// <param name="s">The stream.</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="editUri">The edit URI.</param>
        /// <exception cref="ArgumentException">Picasa photo entry was missing content element</exception>
        private void ParseMediaEntry(Stream s, out string srcUrl, out string editUri)
        {
            srcUrl = null;

            // First try <content src>
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(s);
            if (xmlDoc.SelectSingleNode("/atom:entry/atom:content", this.NamespaceManager) is XmlElement contentEl)
            {
                srcUrl = XmlHelper.GetUrl(contentEl, "@src", this.NamespaceManager, null);
            }

            // Then try media RSS
            if (string.IsNullOrEmpty(srcUrl))
            {
                contentEl = xmlDoc.SelectSingleNode(
                                "/atom:entry/media:group/media:content[@medium='image']",
                                this.NamespaceManager) as XmlElement;
                if (contentEl == null)
                {
                    throw new ArgumentException("Picasa photo entry was missing content element");
                }

                srcUrl = XmlHelper.GetUrl(contentEl, "@url", this.NamespaceManager, null);
            }

            editUri = AtomEntry.GetLink(
                xmlDoc.SelectSingleNode("/atom:entry", this.NamespaceManager) as XmlElement,
                this.NamespaceManager,
                "edit-media",
                null,
                null,
                null);
        }

        /// <summary>
        /// Calls the Picasa authorization filter.
        /// </summary>
        /// <param name="request">The request.</param>
        private void PicasaAuthorizationFilter(HttpWebRequest request) =>
            this.AuthorizeRequest(request, GDataCredentials.PicasaWebServiceName);

        /// <summary>
        /// "It looks like the problem with the inline image is due to referrer checking.
        /// The thumbnail image being used is protected for display only on certain domains.
        /// These domains include *.blogspot.com and *.google.com.  This user is using a
        /// feature in Blogger which allows him to display his blog directly on his own
        /// domain, which will not pass the referrer checking.
        /// "The maximum size of a thumbnail image that can be displayed on non-*.blogspot.com
        /// domains is 800px. (blogs don't actually appear at *.google.com).  However, if you
        /// request a 800px thumbnail, and the image is less than 800px for the maximum
        /// dimension, then the original image will be returned without the referrer
        /// restrictions.  That sounds like it will work for you, so feel free to give it a
        /// shot and let me know if you have any further questions or problems."
        /// -- Anonymous Google Employee
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="role">The role.</param>
        /// <param name="srcUrl">The source URL.</param>
        private void PicasaRefererBlockingWorkaround(string blogId, FileUploadRole role, ref string srcUrl)
        {
            if (role == FileUploadRole.LinkedImage && this.Options.UsePicasaS1600h)
            {
                try
                {
                    var lastSlash = srcUrl.LastIndexOf('/');
                    var srcUrl2 = $"{srcUrl.Substring(0, lastSlash)}/s1600-h{srcUrl.Substring(lastSlash)}";
                    var req = HttpRequestHelper.CreateHttpWebRequest(srcUrl2, true);
                    req.Method = "HEAD";
                    req.GetResponse().Close();
                    srcUrl = srcUrl2;
                    return;
                }
                catch (WebException we)
                {
                    Debug.Fail($"Picasa s1600-h hack failed: {we}");
                }
            }

            try
            {
                if (!this.Options.UsePicasaImgMaxAlways)
                {
                    // This class doesn't have access to the homePageUrl, so this is a workaround to
                    // to get the homePageUrl while minimizing the amount of code we have to change (we're at MShip/ZBB)
                    foreach (var id in BlogSettings.GetBlogIds())
                    {
                        using (var settings = BlogSettings.ForBlogId(id))
                        {
                            if (settings.ClientType != "BloggerAtom" || settings.HostBlogId != blogId)
                            {
                                continue;
                            }

                            switch (UrlHelper.GetDomain(settings.HomepageUrl).ToUpperInvariant())
                            {
                                case "BLOGSPOT.COM":
                                case "GOOGLE.COM":
                                    return;
                            }
                        }
                    }
                }

                srcUrl += $"{(srcUrl.IndexOf('?') >= 0 ? "&" : "?")}imgmax=800";
            }
            catch (Exception ex)
            {
                Trace.Fail($"Unexpected error while doing Picasa upload: {ex}");
            }
        }

        /// <summary>
        /// Posts the new image.
        /// </summary>
        /// <param name="albumName">Name of the album.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="editUri">The edit URI.</param>
        private void PostNewImage(string albumName, string filename, out string srcUrl, out string editUri)
        {
            var transientCredentials = this.Credentials.TransientCredentials as TransientCredentials;
            GDataCredentials.FromCredentials(transientCredentials).EnsureLoggedIn(
                transientCredentials?.Username,
                transientCredentials?.Password,
                GDataCredentials.PicasaWebServiceName,
                false);

            var albumUrl = this.GetBlogImagesAlbum(albumName);
            var response = RedirectHelper.GetResponse(
                albumUrl,
                new UploadFileRequestFactory(this, filename, "POST").Create);
            using (var s = response.GetResponseStream())
            {
                this.ParseMediaEntry(s, out srcUrl, out editUri);
            }
        }

        /// <summary>
        /// Updates the image.
        /// </summary>
        /// <param name="editUri">The edit URI.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="srcUrl">The source URL.</param>
        /// <param name="newEditUri">The new edit URI.</param>
        /// <exception cref="ApplicationException">Should never get here</exception>
        private void UpdateImage(string editUri, string filename, out string srcUrl, out string newEditUri)
        {
            for (var retry = 5; retry > 0; retry--)
            {
                HttpWebResponse response;
                var conflict = false;
                try
                {
                    response = RedirectHelper.GetResponse(
                        editUri,
                        new UploadFileRequestFactory(this, filename, "PUT").Create);
                }
                catch (WebException we)
                {
                    if (retry > 1 && we.Response is HttpWebResponse httpWebResponse
                                  && httpWebResponse.StatusCode == HttpStatusCode.Conflict)
                    {
                        response = httpWebResponse;
                        conflict = true;
                    }
                    else
                    {
                        throw;
                    }
                }

                using (var s = response.GetResponseStream())
                {
                    this.ParseMediaEntry(s, out srcUrl, out newEditUri);
                }

                if (!conflict)
                {
                    return; // success!
                }

                editUri = newEditUri;
            }

            Trace.Fail("Should never get here");
            throw new ApplicationException("Should never get here");
        }
    }
}
