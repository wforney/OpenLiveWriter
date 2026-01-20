// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Net;
using System.IO;
using System.Xml;
using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;

namespace OpenLiveWriter.Extensibility.BlogClient
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class BlogClientAttribute : Attribute
    {
        public BlogClientAttribute(string typeName, string protocolName)
        {
            TypeName = typeName;
            ProtocolName = protocolName;
        }

        public string TypeName { get; }

        /// <summary>
        /// The name of the standard protocol that this blog client
        /// adheres to. This is used to let wlwmanifest.xml provide
        /// API-specific options (i.e. Spaces has different maxRecentPosts
        /// values depending on MetaWeblog or Atom API).
        /// </summary>
        public string ProtocolName { get; }
    }

    public interface IBlogClient
    {
        string ProtocolName { get; }

        IBlogClientOptions Options { get; }
        void OverrideOptions(IBlogClientOptions newClientOptions);

        bool VerifyCredentials();

        BlogInfo[] GetUsersBlogs();

        BlogPostCategory[] GetCategories(string blogId);
        BlogPostKeyword[] GetKeywords(string blogId);

        /// <summary>
        /// Returns recent posts
        /// </summary>
        /// <param name="blogId"></param>
        /// <param name="maxPosts"></param>
        /// <param name="includeCategories"></param>
        /// <param name="now">If null, then includes future posts.  If non-null, then only includes posts before the *UTC* 'now' time.</param>
        /// <returns></returns>
        BlogPost[] GetRecentPosts(string blogId, int maxPosts, bool includeCategories, DateTime? now);

        string NewPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost);

        bool EditPost(string blogId, BlogPost post, INewCategoryContext newCategoryContext, bool publish, out string etag, out XmlDocument remotePost);

        /// <summary>
        /// Attempt to get a post with the specified id (note: may return null
        /// if the post could not be found on the remote server)
        /// </summary>
        BlogPost GetPost(string blogId, string postId);

        void DeletePost(string blogId, string postId, bool publish);

        BlogPost GetPage(string blogId, string pageId);

        PageInfo[] GetPageList(string blogId);

        BlogPost[] GetPages(string blogId, int maxPages);

        string NewPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost);

        bool EditPage(string blogId, BlogPost page, bool publish, out string etag, out XmlDocument remotePost);

        void DeletePage(string blogId, string pageId);

        AuthorInfo[] GetAuthors(string blogId);

        bool? DoesFileNeedUpload(IFileUploadContext uploadContext);

        string DoBeforePublishUploadWork(IFileUploadContext uploadContext);

        void DoAfterPublishUploadWork(IFileUploadContext uploadContext);

        string AddCategory(string blogId, BlogPostCategory category);

        BlogPostCategory[] SuggestCategories(string blogId, string partialCategoryName);

        HttpWebResponse SendAuthenticatedHttpRequest(string requestUri, int timeoutMs, HttpRequestFilter filter);

        BlogInfo[] GetImageEndpoints();

        /// <summary>
        /// Returns false if credentials are sent in the clear
        /// </summary>
        bool IsSecure { get; }

        /// <summary>
        /// Returns false if it is not possible to download manifests or templates for detection
        /// eg. for local static sites
        /// </summary>
        bool RemoteDetectionPossible { get; }
    }

    public interface INewCategoryContext
    {
        void NewCategoryAdded(BlogPostCategory category);
    }

    public interface IFileUploadContext
    {
        string BlogId { get; }
        string PostId { get; }
        string PreferredFileName { get; }
        FileUploadRole Role { get; }
        Stream GetContents();
        string GetContentsLocalFilePath();
        IProperties Settings { get; }
        bool ForceDirectImageLink { get; }

        string FormatFileName(string filename);
    }

    public enum FileUploadRole { LinkedImage, InlineImage, File }

    public class PageInfo : ICloneable
    {
        public PageInfo(string id, string title, DateTime datePublished, string parentId)
        {
            Id = id;
            Title = title;
            DatePublished = datePublished;
            ParentId = parentId;
        }

        public string Id { get; }

        public string Title { get; }

        public DateTime DatePublished { get; }

        public string ParentId { get; }

        public object Clone()
        {
            return new PageInfo(Id, Title, DatePublished, ParentId);
        }
    }

    public class AuthorInfo : ICloneable
    {
        public AuthorInfo(string id, string name)
        {
            Id = id;
            Name = name != string.Empty ? name : Id;
        }

        public string Id { get; }

        public string Name { get; }

        public object Clone()
        {
            return new AuthorInfo(Id, Name);
        }
    }
}
