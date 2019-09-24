// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Web;
    using System.Xml;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Extensibility.BlogClient;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// The BloggerCompatibleClient class.
    /// Implements the <see cref="OpenLiveWriter.BlogClient.Clients.XmlRpcBlogClient" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.BlogClient.Clients.XmlRpcBlogClient" />
    public abstract partial class BloggerCompatibleClient : XmlRpcBlogClient
    {
        /// <summary>
        /// The application key
        /// </summary>
        protected const string APP_KEY = "0123456789ABCDEF";

        /// <summary>
        /// Initializes a new instance of the <see cref="BloggerCompatibleClient"/> class.
        /// </summary>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        protected BloggerCompatibleClient(Uri postApiUrl, IBlogCredentialsAccessor credentials)
            : base(postApiUrl, credentials)
        {
        }

        /// <summary>
        /// Deletes the post.
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <param name="postId">The post identifier.</param>
        /// <param name="publish">if set to <c>true</c> [publish].</param>
        public override void DeletePost(string blogId, string postId, bool publish)
        {
            var tc = this.Login();

            this.CallMethod(
                "blogger.deletePost",
                new XmlRpcString(BloggerCompatibleClient.APP_KEY),
                new XmlRpcString(postId),
                new XmlRpcString(tc.Username),
                new XmlRpcString(tc.Password, true),
                new XmlRpcBoolean(publish));
        }

        /// <summary>
        /// Gets the users blogs.
        /// </summary>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        public override BlogInfo[] GetUsersBlogs()
        {
            var tc = this.Login();
            return this.GetUsersBlogs(tc.Username, tc.Password);
        }

        /// <summary>
        /// Nodes to text.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The text.</returns>
        protected virtual string NodeToText(XmlNode node) => node.InnerText;

        /// <inheritdoc />
        protected override void VerifyCredentials(TransientCredentials tc)
        {
            try
            {
                this.GetUsersBlogs(tc.Username, tc.Password);
            }
            catch (BlogClientAuthenticationException)
            {
                throw;
            }
            catch (Exception e)
            {
                if (!BlogClientUIContext.SilentModeForCurrentThread)
                {
                    BloggerCompatibleClient.ShowError(e.Message);
                }

                throw;
            }
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="error">The error.</param>
        private static void ShowError(string error)
        {
            var helper = new ShowErrorHelper(
                BlogClientUIContext.ContextForCurrentThread,
                MessageId.UnexpectedErrorLogin,
                new object[] { error });
            if (BlogClientUIContext.ContextForCurrentThread == null)
            {
                helper.Show();
            }
            else
            {
                BlogClientUIContext.ContextForCurrentThread.Invoke(new ThreadStart(helper.Show), null);
            }
        }

        /// <summary>
        /// Gets the users blogs.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>An <see cref="Array{BlogInfo}"/>.</returns>
        private BlogInfo[] GetUsersBlogs(string username, string password)
        {
            // call method
            var result = this.CallMethod(
                "blogger.getUsersBlogs",
                new XmlRpcString(BloggerCompatibleClient.APP_KEY),
                new XmlRpcString(username),
                new XmlRpcString(password, true));

            try
            {
                // parse results
                var blogNodes = result.SelectNodes("array/data/value/struct")?.Cast<XmlElement>().ToList()
                             ?? new List<XmlElement>();

                // get node values
                // add to our list of blogs
                var blogs = blogNodes
                           .Select(
                                blogNode => new
                                                {
                                                    blogNode,
                                                    idNode = blogNode.SelectSingleNode("member[name='blogid']/value")
                                                }).Select(
                                arg => new
                                           {
                                               nodeAndId = arg,
                                               nameNode = arg.blogNode.SelectSingleNode("member[name='blogName']/value")
                                           }).Select(
                                arg => new
                                           {
                                               nodeAndIdAndName = arg,
                                               urlNode = arg.nodeAndId.blogNode.SelectSingleNode(
                                                   "member[name='url']/value")
                                           }).Select(
                                arg => new BlogInfo(
                                    arg.nodeAndIdAndName.nodeAndId.idNode.InnerText,
                                    HttpUtility.HtmlDecode(this.NodeToText(arg.nodeAndIdAndName.nameNode)),
                                    arg.urlNode.InnerText)).ToList();

                // return list of blogs
                return blogs.ToArray();
            }
            catch (Exception ex)
            {
                var response = result != null ? result.OuterXml : "(empty response)";
                Trace.Fail($"Exception occurred while parsing GetUsersBlogs response: {response}\r\n{ex}");
                throw new BlogClientInvalidServerResponseException("blogger.getUsersBlogs", ex.Message, response);
            }
        }
    }
}
