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

    using OpenLiveWriter.BlogClient.Clients.StaticSite;
    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.Extensibility.BlogClient;

    /// <summary>
    /// The BlogClientManager class. This class cannot be inherited.
    /// </summary>
    public sealed partial class BlogClientManager
    {
        /// <summary>
        /// The class lock
        /// </summary>
        private static readonly object ClassLock = new object();

        /// <summary>
        /// The client types
        /// </summary>
        private static IList clientTypes;

        /// <summary>
        /// Gets the client types.
        /// </summary>
        /// <value>The client types.</value>
        private static IList ClientTypes
        {
            get
            {
                lock (BlogClientManager.ClassLock)
                {
                    if (BlogClientManager.clientTypes == null)
                    {
                        BlogClientManager.clientTypes = new ArrayList();
                        BlogClientManager.AddClientType(typeof(LiveJournalClient));
                        BlogClientManager.AddClientType(typeof(MetaweblogClient));
                        BlogClientManager.AddClientType(typeof(MovableTypeClient));
                        BlogClientManager.AddClientType(typeof(GenericAtomClient));
                        BlogClientManager.AddClientType(typeof(GoogleBloggerv3Client));
                        BlogClientManager.AddClientType(typeof(BloggerAtomClient));
                        BlogClientManager.AddClientType(typeof(SharePointClient));
                        BlogClientManager.AddClientType(typeof(WordPressClient));
                        BlogClientManager.AddClientType(typeof(TistoryBlogClient));
                        BlogClientManager.AddClientType(typeof(StaticSiteClient));
                    }

                    return BlogClientManager.clientTypes;
                }
            }
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="blogAccount">The blog account.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>An <see cref="IBlogClient"/>.</returns>
        public static IBlogClient CreateClient(BlogAccount blogAccount, IBlogCredentialsAccessor credentials) =>
            BlogClientManager.CreateClient(blogAccount.ClientType, blogAccount.PostApiUrl, credentials);

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        /// <returns>An <see cref="IBlogClient"/>.</returns>
        public static IBlogClient CreateClient(
            string clientType,
            string postApiUrl,
            IBlogCredentialsAccessor credentials)
        {
            Debug.Assert(clientType != "WindowsLiveSpaces", "Use of WindowsLiveSpaces client is deprecated");

            // scan for a client type with a matching name
            var clientTypeUpper = clientType.ToUpperInvariant();
            foreach (ClientTypeDefinition clientTypeDefinition in BlogClientManager.ClientTypes)
            {
                if (clientTypeDefinition.Name.ToUpperInvariant() == clientTypeUpper)
                {
                    return (IBlogClient)clientTypeDefinition.Constructor.Invoke(
                        new object[] { new Uri(postApiUrl), credentials });
                }
            }

            // didn't find a match!
            throw new ArgumentException(
                string.Format(CultureInfo.CurrentCulture, "Client type {0} not found.", clientType));
        }

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>An <see cref="IBlogClient"/>.</returns>
        public static IBlogClient CreateClient(IBlogSettingsAccessor settings) =>
            BlogClientManager.CreateClient(
                settings.ClientType,
                settings.PostApiUrl,
                settings.Credentials,
                settings.ProviderId,
                settings.OptionOverrides,
                settings.UserOptionOverrides,
                settings.HomePageOverrides);

        /// <summary>
        /// Creates the client.
        /// </summary>
        /// <param name="clientType">Type of the client.</param>
        /// <param name="postApiUrl">The post API URL.</param>
        /// <param name="credentials">The credentials.</param>
        /// <param name="providerId">The provider identifier.</param>
        /// <param name="optionOverrides">The option overrides.</param>
        /// <param name="userOptionOverrides">The user option overrides.</param>
        /// <param name="homepageOptionOverrides">The homepage option overrides.</param>
        /// <returns>An <see cref="IBlogClient"/>.</returns>
        public static IBlogClient CreateClient(
            string clientType,
            string postApiUrl,
            IBlogCredentialsAccessor credentials,
            string providerId,
            IDictionary<string, string> optionOverrides,
            IDictionary<string, string> userOptionOverrides,
            IDictionary<string, string> homepageOptionOverrides)
        {
            // create blog client reflecting the settings
            var blogClient = BlogClientManager.CreateClient(clientType, postApiUrl, credentials);

            // if there is a provider associated with the client then use it to override options
            // as necessary for this provider
            var provider = BlogProviderManager.FindProvider(providerId);
            if (provider != null)
            {
                var providerOptions = provider.ConstructBlogOptions(blogClient.Options);
                blogClient.OverrideOptions(providerOptions);
            }

            if (homepageOptionOverrides != null)
            {
                var homepageOptionsReader = new OptionOverrideReader(homepageOptionOverrides);
                var homepageOptions = BlogClientOptions.ApplyOptionOverrides(
                    homepageOptionsReader.Read,
                    blogClient.Options,
                    true);
                blogClient.OverrideOptions(homepageOptions);
            }

            // if there are manifest overrides then apply them
            if (optionOverrides != null)
            {
                var manifestOptionsReader = new OptionOverrideReader(optionOverrides);
                var manifestOptions = BlogClientOptions.ApplyOptionOverrides(
                    manifestOptionsReader.Read,
                    blogClient.Options,
                    true);
                blogClient.OverrideOptions(manifestOptions);
            }

            // if there are user overrides then apply them
            if (userOptionOverrides != null)
            {
                var userOptionsReader = new OptionOverrideReader(userOptionOverrides);
                var userOptions = BlogClientOptions.ApplyOptionOverrides(
                    userOptionsReader.Read,
                    blogClient.Options,
                    true);
                blogClient.OverrideOptions(userOptions);
            }

            // return the blog client
            return blogClient;
        }

        /// <summary>
        /// Determines whether [is valid client type] [the specified type name].
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <returns><c>true</c> if [is valid client type] [the specified type name]; otherwise, <c>false</c>.</returns>
        public static bool IsValidClientType(string typeName)
        {
            // scan for a client type with a matching name
            var typeNameUpper = typeName.ToUpperInvariant();
            return BlogClientManager.ClientTypes.Cast<ClientTypeDefinition>().Any(
                clientTypeDefinition =>
                    clientTypeDefinition.Name.ToUpperInvariant() == typeNameUpper);
        }

        /// <summary>
        /// Adds the type of the client.
        /// </summary>
        /// <param name="clientType">Type of the client.</param>
        private static void AddClientType(Type clientType)
        {
            try
            {
                BlogClientManager.clientTypes.Add(new ClientTypeDefinition(clientType));
            }
            catch (Exception ex)
            {
                Trace.Fail($"Unexpected exception adding Blog ClientType: {ex}");
            }
        }
    }
}
