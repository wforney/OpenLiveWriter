// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;

    using BlogClient;

    using CoreServices;
    using CoreServices.HTML;

    using Extensibility.BlogClient;

    using Localization;

    /// <summary>
    /// The RecentPostSynchronizer class.
    /// </summary>
    public partial class RecentPostSynchronizer
    {
        /// <summary>
        /// Synchronize the local and remote copies of the recent post to create an
        /// edit context that combines the latest HTML content, etc. from the web
        /// with the local image editing context
        /// </summary>
        /// <param name="mainFrameWindow">The main frame window.</param>
        /// <param name="editingContext">The editing context.</param>
        /// <returns>IBlogPostEditingContext.</returns>
        public static IBlogPostEditingContext Synchronize(IWin32Window mainFrameWindow,
                                                          IBlogPostEditingContext editingContext)
        {
            // reloading a local draft does not require synchronization
            if (editingContext.LocalFile.IsDraft && editingContext.LocalFile.IsSaved)
            {
                return editingContext;
            }

            if (editingContext.LocalFile.IsRecentPost)
            {
                // search for a draft of this post which has already been initialized for offline editing of the post
                // (we don't want to allow opening multiple local "drafts" of edits to the same remote post
                var postEditorFile =
                    PostEditorFile.FindPost(PostEditorFile.DraftsFolder, editingContext.BlogId,
                                            editingContext.BlogPost.Id);
                if (postEditorFile != null)
                {
                    // return the draft
                    return postEditorFile.Load();
                }

                //verify synchronization is supported for this blog service
                if (!RecentPostSynchronizer.SynchronizationSupportedForBlog(editingContext.BlogId))
                {
                    Debug.WriteLine("Post synchronization is not supported");
                    return editingContext;
                }

                // opening local copy, try to marry with up to date post content on the server
                // (will return the existing post if an error occurs or the user cancels)
                var serverBlogPost =
                    RecentPostSynchronizer.SafeGetPostFromServer(mainFrameWindow, editingContext.BlogId,
                                                                 editingContext.BlogPost);
                if (serverBlogPost != null)
                {
                    // if the server didn't return a post-id then replace it with the
                    // known post id
                    if (serverBlogPost.Id == string.Empty)
                    {
                        serverBlogPost.Id = editingContext.BlogPost.Id;
                    }

                    // merge trackbacks
                    RecentPostSynchronizer.MergeTrackbacksFromClient(serverBlogPost, editingContext.BlogPost);

                    // create new init params
                    IBlogPostEditingContext newEditingContext = new BlogPostEditingContext(
                        editingContext.BlogId,
                        serverBlogPost, // swap-in blog post from server
                        editingContext.LocalFile,
                        null,
                        editingContext.ServerSupportingFileDirectory,
                        editingContext.SupportingFileStorage,
                        editingContext.ImageDataList,
                        editingContext.ExtensionDataList,
                        editingContext.SupportingFileService);

                    RecentPostSynchronizer.SynchronizeLocalContentsWithEditingContext(editingContext.BlogPost.Contents,
                                                                                      editingContext
                                                                                         .BlogPost
                                                                                         .ContentsVersionSignature,
                                                                                      newEditingContext);

                    // return new init params
                    return newEditingContext;
                }

                return editingContext;
            }

            if (editingContext.LocalFile.IsSaved)
            {
                // Opening draft from somewhere other than the official drafts directory
                return editingContext;
            }

            {
                // opening from the server, first see if the user already has a draft
                // "checked out" for this post
                var postEditorFile =
                    PostEditorFile.FindPost(PostEditorFile.DraftsFolder, editingContext.BlogId,
                                            editingContext.BlogPost.Id);
                if (postEditorFile != null)
                {
                    return postEditorFile.Load();
                }

                // no draft, try to marry with local copy of recent post
                var recentPost = PostEditorFile.FindPost(
                    PostEditorFile.RecentPostsFolder,
                    editingContext.BlogId,
                    editingContext.BlogPost.Id);

                if (recentPost != null)
                {
                    // load the recent post
                    var newEditingContext = recentPost.Load();

                    var localContents = newEditingContext.BlogPost.Contents;
                    var localContentsSignature = newEditingContext.BlogPost.ContentsVersionSignature;

                    // merge trackbacks from client
                    RecentPostSynchronizer.MergeTrackbacksFromClient(editingContext.BlogPost,
                                                                     newEditingContext.BlogPost);

                    // copy the BlogPost properties from the server (including merged trackbacks)
                    newEditingContext.BlogPost.CopyFrom(editingContext.BlogPost);

                    RecentPostSynchronizer.SynchronizeLocalContentsWithEditingContext(
                        localContents, localContentsSignature, newEditingContext);

                    // return the init params
                    return newEditingContext;
                }

                return editingContext;
            }
        }

        /// <summary>
        /// Merges the trackbacks from client.
        /// </summary>
        /// <param name="serverBlogPost">The server blog post.</param>
        /// <param name="clientBlogPost">The client blog post.</param>
        private static void MergeTrackbacksFromClient(BlogPost serverBlogPost, BlogPost clientBlogPost)
        {
            // Merge trackbacks from client. Servers don't seem to ever return trackbacks
            // (haven't found one that does). Our protocol code assumes that any trackbacks
            // returned by the server will be considered "already sent" and placed in the
            // PingUrlsSent bucket. So the correct list of trackbacks is whatever we are
            // storing on the client as PingUrlsPending plus the union of the client
            // and server PingUrlsSent fields.

            // ping urls already sent is the union of the client record of pings sent plus the
            // server record (if any) of pings sent
            serverBlogPost.PingUrlsSent = ArrayHelper.Union(clientBlogPost.PingUrlsSent, serverBlogPost.PingUrlsSent);

            // pending ping urls are any pings that the client has pending that are not contained
            // in our list of already sent ping urls
            Debug.Assert(serverBlogPost.PingUrlsPending.Length ==
                         0); // we never read into PingUrls at the protocol layer
            var pingUrlsSent = new List<string>(serverBlogPost.PingUrlsSent);
            var pingUrlsPending = clientBlogPost.PingUrlsPending.Where(pingUrl => !pingUrlsSent.Contains(pingUrl)).ToArray();

            serverBlogPost.PingUrlsPending = pingUrlsPending;
        }

        /// <summary>
        /// Does this blog support post-sync? (default to true if we can't figure this out)
        /// </summary>
        /// <param name="blogId">The blog identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool SynchronizationSupportedForBlog(string blogId)
        {
            try
            {
                if (!BlogSettings.BlogIdIsValid(blogId))
                {
                    return false;
                }

                // verify synchronization is supported for this blog service
                using (var blog = new Blog(blogId))
                {
                    return blog.ClientOptions.SupportsPostSynchronization;
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception getting post sync options: " + ex);
                return false;
            }
        }

        /// <summary>
        /// Synchronizes the editing context contents with the local contents (if the version signatures are equivalent).
        /// </summary>
        /// <param name="localContents">The local contents.</param>
        /// <param name="localContentsSignature">The local contents signature.</param>
        /// <param name="editingContext">The editing context.</param>
        private static void SynchronizeLocalContentsWithEditingContext(string localContents,
                                                                       string localContentsSignature,
                                                                       IBlogPostEditingContext editingContext)
        {
            var contentIsUpToDate = localContentsSignature == editingContext.BlogPost.ContentsVersionSignature;
            if (contentIsUpToDate)
            {
                Debug.WriteLine("RecentPostSynchronizer: Use local contents");
                editingContext.BlogPost.Contents = localContents;
            }
            else
            {
                Debug.WriteLine("RecentPostSynchronizer: Using remote contents");

                // convert image references
                RecentPostSynchronizer.ConvertImageReferencesToLocal(editingContext);
            }
        }

        /// <summary>
        /// Safes the get post from server.
        /// </summary>
        /// <param name="mainFrameWindow">The main frame window.</param>
        /// <param name="destinationBlogId">The destination blog identifier.</param>
        /// <param name="blogPost">The blog post.</param>
        /// <returns>BlogPost.</returns>
        private static BlogPost SafeGetPostFromServer(IWin32Window mainFrameWindow, string destinationBlogId,
                                                      BlogPost blogPost)
        {
            try
            {
                if (BlogSettings.BlogIdIsValid(destinationBlogId))
                {
                    var entityName = blogPost.IsPage ? Res.Get(StringId.Page) : Res.Get(StringId.Post);

                    if (!RecentPostSynchronizer.VerifyBlogCredentials(destinationBlogId))
                    {
                        // warn the user we couldn't synchronize
                        RecentPostSynchronizer.ShowRecentPostNotSynchronizedWarning(mainFrameWindow, entityName);
                        return null;
                    }

                    // get the recent post (with progress if it takes more than a predefined interval)
                    GetRecentPostOperation getRecentPostOperation;
                    using (var progressForm = new RecentPostProgressForm(entityName))
                    {
                        progressForm.CreateControl();
                        getRecentPostOperation =
                            new GetRecentPostOperation(new BlogClientUIContextImpl(progressForm), destinationBlogId,
                                                       blogPost);
                        getRecentPostOperation.Start();
                        progressForm.ShowDialogWithDelay(mainFrameWindow, getRecentPostOperation, 3000);
                    }

                    // if we got the post then return it
                    if (!getRecentPostOperation.WasCancelled && getRecentPostOperation.ServerBlogPost != null)
                    {
                        return getRecentPostOperation.ServerBlogPost;
                    }

                    // remote server didn't have a copy of the post

                    if (getRecentPostOperation.NoPostAvailable)
                    {
                        return null;
                    }

                    // some type of error occurred (including the user cancelled)

                    // warn the user we couldn't synchronize
                    RecentPostSynchronizer.ShowRecentPostNotSynchronizedWarning(mainFrameWindow, entityName);
                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected error attempting to fetch blog-post from server: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Shows the recent post not synchronized warning.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="entityName">Name of the entity.</param>
        private static void ShowRecentPostNotSynchronizedWarning(IWin32Window owner, string entityName)
        {
            using (var warningForm = new RecentPostNotSynchronizedWarningForm(entityName))
            {
                warningForm.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Verifies the blog credentials.
        /// </summary>
        /// <param name="destinationBlogId">The destination blog identifier.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool VerifyBlogCredentials(string destinationBlogId)
        {
            using (var blog = new Blog(destinationBlogId))
            {
                return blog.VerifyCredentials();
            }
        }

        /// <summary>
        /// Converts the image references to local.
        /// </summary>
        /// <param name="editingContext">The editing context.</param>
        private static void ConvertImageReferencesToLocal(IBlogPostEditingContext editingContext)
        {
            var refFixer = new ImageReferenceFixer(editingContext.ImageDataList, editingContext.BlogId);

            // Create a text writer that the new html will be written to
            TextWriter htmlWriter = new StringWriter(CultureInfo.InvariantCulture);

            // Check an html image fixer that will find references and rewrite them to new paths
            var referenceFixer = new HtmlReferenceFixer(editingContext.BlogPost.Contents);

            // We need to update the editing context when we change an image
            var contextFixer = new ContextImageReferenceFixer(editingContext);

            // Do the fixing
            referenceFixer.FixReferences(htmlWriter, refFixer.FixImageReferences, contextFixer.ReferenceFixedCallback);

            // Write back the new html
            editingContext.BlogPost.Contents = htmlWriter.ToString();
        }
    }
}
