// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;

    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.HtmlParser.Parser;

    /// <summary>
    /// The LiveJournalClient class.
    /// Implements the <see cref="BloggerCompatibleClient" />
    /// </summary>
    /// <seealso cref="BloggerCompatibleClient" />
    [BlogClient("LiveJournal", "LiveJournal")]
    public partial class LiveJournalClient : BloggerCompatibleClient
    {
        /// <summary>
        /// The UTF8 encoding no byte order mark
        /// </summary>
        private readonly Encoding utf8EncodingNoBom = new UTF8Encoding(false);

        /// <summary>
        /// Initializes a new instance of the <see cref="LiveJournalClient"/> class.
        /// </summary>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        public LiveJournalClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
        }

        /// <summary>
        /// Does the before publish upload work.
        /// </summary>
        /// <param name="uploadContext">The upload context.</param>
        /// <returns>The result..</returns>
        /// <exception cref="BlogClientInvalidServerResponseException">LiveJournal.UploadPic - No URL returned from server</exception>
        public override string DoBeforePublishUploadWork(IFileUploadContext uploadContext)
        {
            const int RequestCount = 2;

            // get as many challenge tokens as we'll need (one for each authenticated request)
            var frm = new FotobilderRequestManager(this.Username, this.Password);
            var doc = frm.PerformGet(
                "GetChallenges",
                null,
                "GetChallenges.Qty",
                RequestCount.ToString(CultureInfo.InvariantCulture));
            var challengeNodes = doc.SelectNodes(@"/FBResponse/GetChallengesResponse/Challenge");
            Trace.Assert(challengeNodes?.Count == RequestCount);
            var challenges = new Stack(challengeNodes.Count);
            foreach (XmlNode node in challengeNodes)
            {
                challenges.Push(node.InnerText);
            }

            // login
            ////var bytesAvailable = long.MaxValue;
            doc = frm.PerformGet(
                "Login",
                (string)challenges.Pop(),
                "Login.ClientVersion",
                ApplicationEnvironment.UserAgent);
            var remainingQuotaNode = doc.SelectSingleNode("/FBResponse/LoginResponse/Quota/Remaining");
            if (remainingQuotaNode != null)
            {
                ////bytesAvailable = long.Parse(remainingQuotaNode.InnerText, CultureInfo.InvariantCulture);
            }

            // upload picture
            using (var fileContents = uploadContext.GetContents())
            {
                doc = frm.PerformPut(
                    "UploadPic",
                    (string)challenges.Pop(),
                    fileContents,
                    "UploadPic.PicSec",
                    "255",
                    "UploadPic.Meta.Filename",
                    uploadContext.FormatFileName(uploadContext.PreferredFileName),
                    "UploadPic.Gallery._size",
                    "1",
                    "UploadPic.Gallery.0.GalName",
                    ApplicationEnvironment.ProductName,
                    "UploadPic.Gallery.0.GalSec",
                    "255");
            }

            var picUrlNode = doc.SelectSingleNode("/FBResponse/UploadPicResponse/URL");
            if (picUrlNode != null)
            {
                return picUrlNode.InnerText;
            }

            throw new BlogClientInvalidServerResponseException(
                "LiveJournal.UploadPic",
                "No URL returned from server",
                doc.OuterXml);
        }

        /// <summary>
        /// Edits the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="post">The post.</param>
        /// <param name="newCategoryContext">The new category context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <returns><c>true</c> if successful, <c>false</c> otherwise.</returns>
        public override bool EditPost(
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish)
        {
            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            // call the method
            var result = this.CallMethod(
                "blogger.editPost",
                new XmlRpcString(BloggerCompatibleClient.APP_KEY),
                new XmlRpcString(post.Id),
                new XmlRpcString(this.Username),
                new XmlRpcString(this.Password, true),
                this.FormatBlogPost(post),
                new XmlRpcBoolean(publish));

            return result.InnerText == "1";
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{BlogPostCategory}"/>.</returns>
        /// <remarks>
        /// LiveJournal does not support client posting of categories
        /// </remarks>
        public override BlogPostCategory[] GetCategories(string blogId) => Array.Empty<BlogPostCategory>();

        /// <summary>
        /// Gets the keywords.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns>An <see cref="Array{BlogPostKeyword}"/>.</returns>
        public override BlogPostKeyword[] GetKeywords(string blogId)
        {
            Trace.Fail("LiveJournal does not support GetKeywords!");
            return Array.Empty<BlogPostKeyword>();
        }

        /// <summary>
        /// Gets the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <returns>A <see cref="BlogPost"/>.</returns>
        public override BlogPost GetPost(string blogId, string postId)
        {
            // query for post
            var postResult = this.CallMethod(
                "blogger.getPost",
                new XmlRpcString(BloggerCompatibleClient.APP_KEY),
                new XmlRpcString(postId),
                new XmlRpcString(this.Username),
                new XmlRpcString(this.Password, true));

            // parse results
            try
            {
                // get the post struct
                var postNode = postResult.SelectSingleNode("struct");

                // create a post to return
                var blogPost = new BlogPost();

                // extract content
                this.ExtractStandardPostFields(postNode, blogPost);

                // return the post
                return blogPost;
            }
            catch (Exception ex)
            {
                var response = postResult == null ? "(empty response)" : postResult.OuterXml;
                Trace.Fail($"Exception occurred while parsing blogger.getPost response: {response}\r\n{ex}");
                throw new BlogClientInvalidServerResponseException("blogger.getPost", ex.Message, response);
            }
        }

        /// <summary>
        /// Gets the recent posts.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="maxPosts">The maximum posts.</param>
        /// <param name="includeCategories">if set to <c>true</c> [include categories].</param>
        /// <param name="now">The now.</param>
        /// <returns>A <see cref="Array{BlogPost}"/>.</returns>
        public override BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now)
        {
            // posts to return
            var posts = new List<BlogPost>();

            // call the method
            var result = this.CallMethod(
                "blogger.getRecentPosts",
                new XmlRpcString(BloggerCompatibleClient.APP_KEY),
                new XmlRpcString(blogId),
                new XmlRpcString(this.Username),
                new XmlRpcString(this.Password, true),
                new XmlRpcInt(maxPosts));

            // parse results
            try
            {
                var postNodes = result.SelectNodes("array/data/value/struct")?.Cast<XmlNode>().ToList()
                             ?? new List<XmlNode>();

                foreach (var postNode in postNodes)
                {
                    // create blog post
                    var blogPost = new BlogPost();

                    this.ExtractStandardPostFields(postNode, blogPost);

                    // add to our list of posts
                    if (!now.HasValue || blogPost.DatePublished.CompareTo(now.Value) < 0)
                    {
                        posts.Add(blogPost);
                    }
                }
            }
            catch (Exception ex)
            {
                var response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail($"Exception occurred while parsing GetRecentPosts response: {response}\r\n{ex}");
                throw new BlogClientInvalidServerResponseException("blogger.getRecentPosts", ex.Message, response);
            }

            // return list of posts
            return posts.ToArray();
        }

        /// <summary>
        /// Creates new post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="post">The post.</param>
        /// <param name="newCategoryContext">The new category context.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        /// <returns>The result.</returns>
        public override string NewPost(
            string blogId,
            BlogPost post,
            INewCategoryContext newCategoryContext,
            bool publish)
        {
            if (!publish && !this.Options.SupportsPostAsDraft)
            {
                Trace.Fail("Post to draft not supported on this provider");
                throw new BlogClientPostAsDraftUnsupportedException();
            }

            // call the method
            var result = this.CallMethod(
                "blogger.newPost",
                new XmlRpcString(BloggerCompatibleClient.APP_KEY),
                new XmlRpcString(blogId),
                new XmlRpcString(this.Username),
                new XmlRpcString(this.Password, true),
                this.FormatBlogPost(post),
                new XmlRpcBoolean(publish));

            // return the blog-id
            return result.InnerText;
        }

        /// <inheritdoc />
        protected override void ConfigureClientOptions(BlogClientOptions clientOptions)
        {
            clientOptions.SupportsFileUpload = true;
            clientOptions.SupportsCustomDate = false;
            clientOptions.SupportsExtendedEntries = true;
        }

        /// <summary>
        /// Exceptions for fault.
        /// </summary>
        /// <param name="faultCode">The fault code.</param>
        /// <param name="faultString">The fault string.</param>
        /// <returns>A <see cref="BlogClientProviderException"/>.</returns>
        protected override BlogClientProviderException ExceptionForFault(string faultCode, string faultString) =>
            faultCode == "100"
         || faultCode == "101"
         || faultCode.ToUpperInvariant() == "SERVER"
         && faultString.StartsWith("invalid login", StringComparison.OrdinalIgnoreCase)
                ? new BlogClientAuthenticationException(faultCode, faultString)
                : null;

        /// <summary>
        /// Nodes to text.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The text.</returns>
        protected override string NodeToText(XmlNode node)
        {
            if (node.FirstChild is XmlElement childNode && childNode.LocalName == "base64")
            {
                try
                {
                    return Encoding.UTF8.GetString(Convert.FromBase64String(childNode.InnerText));
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                }
            }

            return node.InnerText;
        }

        /// <summary>
        /// Extracts the standard post fields.
        /// </summary>
        /// <param name="postNode">The post node.</param>
        /// <param name="blogPost">The blog post.</param>
        private void ExtractStandardPostFields(XmlNode postNode, BlogPost blogPost)
        {
            // post id
            blogPost.Id = XmlRpcBlogClient.NodeText(postNode.SelectSingleNode("member[name='postId']/value"));

            // contents and title
            this.ParsePostContent(postNode.SelectSingleNode("member[name='content']/value"), blogPost);

            // date published
            blogPost.DatePublished = this.ParseBlogDate(postNode.SelectSingleNode("member[name='dateCreated']/value"));
        }

        /// <summary>
        /// Formats the blog post.
        /// </summary>
        /// <param name="post">The post.</param>
        /// <returns>An <see cref="XmlRpcValue"/>.</returns>
        private XmlRpcValue FormatBlogPost(BlogPost post)
        {
            var content = post.MainContents;
            if (!string.IsNullOrEmpty(post.ExtendedContents))
            {
                content += $"<lj-cut>{post.ExtendedContents}";
            }

            var blogPostBody = string.Format(
                CultureInfo.InvariantCulture,
                "<title>{0}</title>{1}",
                this.GetPostTitleForXmlValue(post),
                content);
            return new XmlRpcBase64(this.utf8EncodingNoBom.GetBytes(blogPostBody));
        }

        /// <summary>
        /// Parses the content of the post.
        /// </summary>
        /// <param name="xmlNode">The XML node.</param>
        /// <param name="blogPost">The blog post.</param>
        private void ParsePostContent(XmlNode xmlNode, BlogPost blogPost)
        {
            // get raw content (decode base64 if necessary)
            string content;
            var base64Node = xmlNode.SelectSingleNode("base64");
            if (base64Node != null)
            {
                var contentBytes = Convert.FromBase64String(base64Node.InnerText);
                content = this.utf8EncodingNoBom.GetString(contentBytes);
            }
            else
            {
                // no base64 encoding, just read text
                content = xmlNode.InnerText;
            }

            // parse out the title and contents of the post
            var ex = new HtmlExtractor(content);
            if (ex.Seek("<title>").Success)
            {
                this.SetPostTitleFromXmlValue(blogPost, ex.CollectTextUntil("title"));
                content = content.Substring(ex.Parser.Position).TrimStart('\r', '\n');
            }

            if (content.Trim() == string.Empty)
            {
                return;
            }

            var ex2 = new HtmlExtractor(content);
            if (this.Options.SupportsExtendedEntries && ex2.Seek("<lj-cut>").Success)
            {
                blogPost.SetContents(
                    content.Substring(0, ex2.Element.Offset),
                    content.Substring(ex2.Element.Offset + ex2.Element.Length));
            }
            else
            {
                blogPost.Contents = content;
            }
        }
    }
}
