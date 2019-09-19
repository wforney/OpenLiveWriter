// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;

    using OpenLiveWriter.BlogClient.Providers;
    using OpenLiveWriter.Extensibility.BlogClient;

    public sealed class BlogClientManager
    {
        public static bool IsValidClientType(string typeName)
        {
            // scan for a client type with a matching name
            var typeNameUpper = typeName.ToUpperInvariant();
            foreach (ClientTypeDefinition clientTypeDefinition in ClientTypes)
            {
                if (clientTypeDefinition.Name.ToUpperInvariant() == typeNameUpper)
                {
                    return true;
                }
            }

            // none found
            return false;
        }

        public static IBlogClient CreateClient(BlogAccount blogAccount, IBlogCredentialsAccessor credentials) =>
            CreateClient(blogAccount.ClientType, blogAccount.PostApiUrl, credentials);

        public static IBlogClient CreateClient(string clientType, string postApiUrl, IBlogCredentialsAccessor credentials)
        {
            Debug.Assert(clientType != "WindowsLiveSpaces", "Use of WindowsLiveSpaces client is deprecated");

            // scan for a client type with a matching name
            var clientTypeUpper = clientType.ToUpperInvariant();
            foreach (ClientTypeDefinition clientTypeDefinition in ClientTypes)
            {
                if (clientTypeDefinition.Name.ToUpperInvariant() == clientTypeUpper)
                {
                    return (IBlogClient)clientTypeDefinition.Constructor.Invoke(new object[] {
                        new Uri(postApiUrl), credentials  });
                }
            }

            // didn't find a match!
            throw new ArgumentException(
                String.Format(CultureInfo.CurrentCulture, "Client type {0} not found.", clientType));
        }

        public static IBlogClient CreateClient(IBlogSettingsAccessor settings) =>
            CreateClient(
                settings.ClientType,
                settings.PostApiUrl,
                settings.Credentials,
                settings.ProviderId,
                settings.OptionOverrides,
                settings.UserOptionOverrides,
                settings.HomePageOverrides);

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
            var blogClient = CreateClient(clientType, postApiUrl, credentials);

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
                var homepageOptions = BlogClientOptions.ApplyOptionOverrides(new OptionReader(homepageOptionsReader.Read), blogClient.Options, true);
                blogClient.OverrideOptions(homepageOptions);
            }

            // if there are manifest overrides then apply them
            if (optionOverrides != null)
            {
                var manifestOptionsReader = new OptionOverrideReader(optionOverrides);
                var manifestOptions = BlogClientOptions.ApplyOptionOverrides(new OptionReader(manifestOptionsReader.Read), blogClient.Options, true);
                blogClient.OverrideOptions(manifestOptions);
            }

            // if there are user overrides then apply them
            if (userOptionOverrides != null)
            {
                var userOptionsReader = new OptionOverrideReader(userOptionOverrides);
                var userOptions = BlogClientOptions.ApplyOptionOverrides(new OptionReader(userOptionsReader.Read), blogClient.Options, true);
                blogClient.OverrideOptions(userOptions);
            }

            // return the blog client
            return blogClient;
        }

        private class OptionOverrideReader
        {
            public OptionOverrideReader(IDictionary<string, string> optionOverrides) => this.optionOverrides = optionOverrides;

            public string Read(string optionName) => optionOverrides[optionName] as string;

            private readonly IDictionary<string, string> optionOverrides;
        }

        private static IList ClientTypes
        {
            get
            {
                lock (classLock)
                {
                    if (clientTypes == null)
                    {
                        clientTypes = new ArrayList();
                        AddClientType(typeof(LiveJournalClient));
                        AddClientType(typeof(MetaweblogClient));
                        AddClientType(typeof(MovableTypeClient));
                        AddClientType(typeof(GenericAtomClient));
                        AddClientType(typeof(GoogleBloggerv3Client));
                        AddClientType(typeof(BloggerAtomClient));
                        AddClientType(typeof(SharePointClient));
                        AddClientType(typeof(WordPressClient));
                        AddClientType(typeof(TistoryBlogClient));
                        AddClientType(typeof(StaticSite.StaticSiteClient));
                    }

                    return clientTypes;
                }
            }
        }
        private static IList clientTypes;
        private static readonly object classLock = new object();

        private static void AddClientType(Type clientType)
        {
            try
            {
                clientTypes.Add(new ClientTypeDefinition(clientType));
            }
            catch (Exception ex)
            {
                Trace.Fail("Unexpected exception adding Blog ClientType: " + ex.ToString());
            }
        }

        private class ClientTypeDefinition
        {
            public ClientTypeDefinition(Type clientType)
            {
                // determine the name from the custom attribute
                var blogClientAttributes = clientType.GetCustomAttributes(typeof(BlogClientAttribute), false) as BlogClientAttribute[];
                if (blogClientAttributes.Length != 1)
                    throw new ArgumentException("You must provide a single BlogClientAttribute for all registered blog client types.");
                _name = blogClientAttributes[0].TypeName;

                // get the constructor used for creation
                _constructor = clientType.GetConstructor(new Type[] { typeof(Uri), typeof(IBlogCredentialsAccessor) });
                if (_constructor == null)
                    throw new ArgumentException("You must implement a public constructor with the signature (Uri,IBlogCredentials,PostFormatOptions) for all registered blog client types.");

                // record the type
                _type = clientType;
            }

            public string Name
            {
                get { return _name; }
            }
            private string _name;

            public ConstructorInfo Constructor
            {
                get { return _constructor; }
            }
            private ConstructorInfo _constructor;

            public Type Type
            {
                get { return _type; }
            }
            private Type _type;
        }
    }

}
