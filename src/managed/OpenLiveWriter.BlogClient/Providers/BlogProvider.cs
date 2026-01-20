// Copyright (c) .NET Foundation. All rights reserved. Licensed under the MIT license. See LICENSE
// file in the project root for details.

using OpenLiveWriter.Api;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;
using OpenLiveWriter.Localization;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace OpenLiveWriter.BlogClient.Providers
{
    public delegate string OptionReader(string optionName);

    public class BlogClientOptions : IBlogClientOptions
    {
        public const string CATEGORY_SCHEME = "categoryScheme";

        public const string CHARACTER_SET = "characterSet";

        public const string DHTML_IMAGE_VIEWER = "dhtmlImageViewer";

        public const string IMAGE_ENDPOINT = "imageEndpoint";

        public const string POST_BODY_BACKGROUND_COLOR = "postBodyBackgroundColor";

        public const string REQUIRES_XHTML = "requiresXHTML";

        public const string SUPPORTS_CATEGORIES = "supportsCategories";

        public const string SUPPORTS_EMBEDS = "supportsEmbeds";

        public const string SUPPORTS_NEW_CATEGORIES = "supportsNewCategories";

        public const string SUPPORTS_SCRIPTS = "supportsScripts";

        public const string TEMPLATE_IS_RTL = "templateIsRTL";
        private int _maxPostTitleLength;

        // The "none" value for comments is supported by MT and TypePad but not WordPress

        public BlogClientOptions()
        {
            // accept default options (see private data declarations for defaults)
        }

        public string AdminLinkText { get; set; } = string.Empty;

        public string AdminUrl { get; set; } = string.Empty;

        public int AllowPolicyFalseValue { get; set; } = 0;

        public string CategoryScheme { get; set; } = null;

        public string CharacterSet { get; set; } = string.Empty;

        public bool CommentPolicyAsBoolean { get; set; } = false;

        public string ContentFilter { get; set; } = string.Empty;

        public string DefaultView { get; set; } = string.Empty;

        public string DhtmlImageViewer { get; set; }

        public string FileUploadNameFormat { get; set; } = string.Empty;

        public bool FuturePublishDateWarning { get; set; } = false;

        public bool HasRTLFeatures => IsRTLTemplate;

        public string HomepageLinkText { get; set; } = string.Empty;

        public string ImagePostingUrl { get; set; } = string.Empty;

        public string InvalidPostIdFaultCodePattern { get; set; } = string.Empty;

        public string InvalidPostIdFaultStringPattern { get; set; } = string.Empty;

        public bool IsRTLTemplate { get; set; } = false;

        public bool KeywordsAsTags { get; set; } = false;

        public bool LinkToSkyDriveSelfPage { get; set; } = false;

        public int MaxCategoryNameLength { get; set; } = 0;

        public int MaxPostTitleLength
        {
            get => this._maxPostTitleLength > 0 ? this._maxPostTitleLength : int.MaxValue; set => this._maxPostTitleLength = value;
        }

        public int MaxRecentPosts { get; set; } = -1;

        public string PermalinkFormat { get; set; } = string.Empty;

        public int PostBodyBackgroundColor { get; set; } = Color.Transparent.ToArgb();

        public string PostDateFormat { get; set; } = string.Empty;

        public string PostEditingUrl { get; set; } = string.Empty;

        public string PostEditingUrlPostIdPattern { get; set; } = string.Empty;

        public bool RequiresHtmlTitles { get; set; } = true;

        public bool RequiresXHTML { get; set; } = false;

        public SupportsFeature ReturnsHtmlTitlesOnGet { get; set; } = SupportsFeature.Unknown;

        public string ServiceName { get; set; } = string.Empty;

        public bool SupportsAuthor { get; set; } = false;

        public bool SupportsAutoUpdate { get; set; } = true;

        public bool SupportsCategories { get; set; } = false;

        public bool SupportsCategoriesInline { get; set; } = true;

        public bool SupportsCategoryIds { get; set; } = false;

        public bool SupportsCommentPolicy { get; set; } = false;

        public bool SupportsCustomDate { get; set; } = true;

        public bool SupportsCustomDateUpdate { get; set; } = true;

        public SupportsFeature SupportsEmbeds { get; set; } = SupportsFeature.Unknown;

        public bool SupportsEmptyTitles { get; set; } = true;

        public bool SupportsExcerpt { get; set; } = false;

        public bool SupportsExtendedEntries { get; set; } = false;

        public bool SupportsFileUpload { get; set; } = false;

        public bool SupportsGetKeywords { get; set; } = false;

        public bool SupportsHierarchicalCategories { get; set; } = false;

        public bool SupportsHttps { get; set; } = false;

        // @SharedCanvas - not used yet, make sure this makes sense when it used, and the value is
        // read from anywhere it needs to
        public SupportsFeature SupportsImageUpload { get; set; } = SupportsFeature.Yes;

        public bool SupportsKeywords { get; set; } = false;

        public bool SupportsMultipleCategories { get; set; } = false;
        public bool SupportsNewCategories { get; set; } = false;

        public bool SupportsNewCategoriesInline { get; set; } = false;

        public bool SupportsPageOrder { get; set; } = false;

        public bool SupportsPageParent { get; set; } = false;

        public bool SupportsPages { get; set; } = false;

        public bool SupportsPageTrackbacks { get; set; } = false;

        public bool SupportsPassword { get; set; } = false;

        public bool SupportsPingPolicy { get; set; } = false;

        public bool SupportsPostAsDraft { get; set; } = true;

        public bool SupportsPostSynchronization { get; set; } = true;

        public SupportsFeature SupportsScripts { get; set; } = SupportsFeature.Unknown;

        public bool SupportsSlug { get; set; } = false;

        public bool SupportsSuggestCategories { get; set; } = false;
        public bool SupportsTrackbacks { get; set; } = false;
        public TrackbackDelimiter TrackbackDelimiter { get; set; } = TrackbackDelimiter.ArrayElement;
        public bool UseLocalTime { get; set; } = false;

        public bool UsePicasaImgMaxAlways { get; set; } = true;

        public bool UsePicasaS1600h { get; set; } = true;

        public bool UseSpacesCIDMemberName { get; set; } = false;

        public bool UseSpacesTemplateHack { get; set; } = false;

        public static IBlogClientOptions ApplyOptionOverrides(OptionReader optionReader, IBlogClientOptions existingOptions) => ApplyOptionOverrides(optionReader, existingOptions, true);

        public static IBlogClientOptions ApplyOptionOverrides(OptionReader optionReader, IBlogClientOptions existingOptions, bool includeIrregularities)
        {
            BlogClientOptions clientOptions = new BlogClientOptions
            {
                // Protocol capabilities
                SupportsPostAsDraft = ReadBool(optionReader("supportsPostAsDraft"), existingOptions.SupportsPostAsDraft),
                SupportsFileUpload = ReadBool(optionReader("supportsFileUpload"), existingOptions.SupportsFileUpload),
                SupportsExtendedEntries = ReadBool(optionReader("supportsExtendedEntries"), existingOptions.SupportsExtendedEntries),
                SupportsCustomDate = ReadBool(optionReader("supportsCustomDate"), existingOptions.SupportsCustomDate),
                SupportsCustomDateUpdate = ReadBool(optionReader("supportsCustomDateUpdate"), existingOptions.SupportsCustomDateUpdate),
                SupportsHttps = ReadBool(optionReader("supportsHttps"), existingOptions.SupportsHttps),
                SupportsCategories = ReadBool(optionReader(SUPPORTS_CATEGORIES), existingOptions.SupportsCategories),
                SupportsCategoriesInline = ReadBool(optionReader("supportsCategoriesInline"), existingOptions.SupportsCategoriesInline),
                SupportsMultipleCategories = ReadBool(optionReader("supportsMultipleCategories"), existingOptions.SupportsMultipleCategories),
                SupportsHierarchicalCategories = ReadBool(optionReader("supportsHierarchicalCategories"), existingOptions.SupportsHierarchicalCategories),
                SupportsNewCategories = ReadBool(optionReader(SUPPORTS_NEW_CATEGORIES), existingOptions.SupportsNewCategories),
                SupportsNewCategoriesInline = ReadBool(optionReader("supportsNewCategoriesInline"), existingOptions.SupportsNewCategoriesInline),
                SupportsSuggestCategories = ReadBool(optionReader("supportsSuggestCategories"), existingOptions.SupportsSuggestCategories),
                CategoryScheme = ReadText(optionReader(CATEGORY_SCHEME), existingOptions.CategoryScheme),
                SupportsKeywords = ReadBool(optionReader("supportsKeywords"), existingOptions.SupportsKeywords),
                SupportsGetKeywords = ReadBool(optionReader("supportsGetTags"), existingOptions.SupportsGetKeywords),
                SupportsCommentPolicy = ReadBool(optionReader("supportsCommentPolicy"), existingOptions.SupportsCommentPolicy),
                SupportsPingPolicy = ReadBool(optionReader("supportsPingPolicy"), existingOptions.SupportsPingPolicy),
                SupportsAuthor = ReadBool(optionReader("supportsAuthor"), existingOptions.SupportsAuthor),
                SupportsSlug = ReadBool(optionReader("supportsSlug"), existingOptions.SupportsSlug),
                SupportsPassword = ReadBool(optionReader("supportsPassword"), existingOptions.SupportsPassword),
                SupportsExcerpt = ReadBool(optionReader("supportsExcerpt"), existingOptions.SupportsExcerpt),
                SupportsTrackbacks = ReadBool(optionReader("supportsTrackbacks"), existingOptions.SupportsTrackbacks),
                SupportsPages = ReadBool(optionReader("supportsPages"), existingOptions.SupportsPages),
                SupportsPageParent = ReadBool(optionReader("supportsPageParent"), existingOptions.SupportsPageParent),
                SupportsPageOrder = ReadBool(optionReader("supportsPageOrder"), existingOptions.SupportsPageOrder),
                SupportsPageTrackbacks = ReadBool(optionReader("supportsPageTrackbacks"), existingOptions.SupportsPageTrackbacks),

                // Writer capabilities
                LinkToSkyDriveSelfPage = ReadBool(optionReader("linkToSkyDriveSelfPage"), existingOptions.LinkToSkyDriveSelfPage),
                RequiresHtmlTitles = ReadBool(optionReader("requiresHtmlTitles"), existingOptions.RequiresHtmlTitles),
                ReturnsHtmlTitlesOnGet = ReadSupportsFeature(optionReader("returnsHtmlTitlesOnGet"), existingOptions.ReturnsHtmlTitlesOnGet),
                SupportsEmptyTitles = ReadBool(optionReader("supportsEmptyTitles"), existingOptions.SupportsEmptyTitles),
                SupportsScripts = ReadSupportsFeature(optionReader(SUPPORTS_SCRIPTS), existingOptions.SupportsScripts),
                SupportsEmbeds = ReadSupportsFeature(optionReader(SUPPORTS_EMBEDS), existingOptions.SupportsEmbeds),
                SupportsImageUpload = ReadSupportsFeature(optionReader("supportsImageUpload"), existingOptions.SupportsImageUpload),
                DefaultView = ReadText(optionReader("defaultView"), existingOptions.DefaultView),
                CharacterSet = ReadText(optionReader(CHARACTER_SET), existingOptions.CharacterSet),
                RequiresXHTML = ReadBool(optionReader(REQUIRES_XHTML), existingOptions.RequiresXHTML),
                DhtmlImageViewer = ReadText(optionReader(DHTML_IMAGE_VIEWER), existingOptions.DhtmlImageViewer),
                PostBodyBackgroundColor = ReadInt(optionReader(POST_BODY_BACKGROUND_COLOR), existingOptions.PostBodyBackgroundColor),
                MaxCategoryNameLength = ReadInt(optionReader("maxCategoryNameLength"), existingOptions.MaxCategoryNameLength),
                SupportsAutoUpdate = ReadBool(optionReader("supportsAutoUpdate"), existingOptions.SupportsAutoUpdate),
                InvalidPostIdFaultCodePattern = ReadText(optionReader("invalidPostIdFaultCodePattern"), existingOptions.InvalidPostIdFaultCodePattern),
                InvalidPostIdFaultStringPattern = ReadText(optionReader("invalidPostIdFaultStringPattern"), existingOptions.InvalidPostIdFaultStringPattern),
                IsRTLTemplate = ReadBool(optionReader("templateIsRTL"), existingOptions.IsRTLTemplate),
                MaxPostTitleLength = ReadInt(optionReader("maxPostTitleLength"), existingOptions.MaxPostTitleLength),

                // Weblog
                HomepageLinkText = ReadText(optionReader("homepageLinkText"), existingOptions.HomepageLinkText),
                AdminLinkText = ReadText(optionReader("adminLinkText"), existingOptions.AdminLinkText),
                AdminUrl = ReadText(optionReader("adminUrl"), existingOptions.AdminUrl),
                PostEditingUrl = ReadText(optionReader("postEditingUrl"), existingOptions.PostEditingUrl),
                PostEditingUrlPostIdPattern = ReadText(optionReader("postEditingUrlPostIdPattern"), existingOptions.PostEditingUrlPostIdPattern),
                ImagePostingUrl = ReadText(optionReader(IMAGE_ENDPOINT), existingOptions.ImagePostingUrl),
                ServiceName = ReadText(optionReader("serviceName"), existingOptions.ServiceName)
            };

            // Irregularities
            if (includeIrregularities)
            {
                clientOptions.CommentPolicyAsBoolean = ReadBool(optionReader("commentPolicyAsBoolean"), existingOptions.CommentPolicyAsBoolean);
                clientOptions.AllowPolicyFalseValue = ReadInt(optionReader("allowPolicyFalseValue"), existingOptions.AllowPolicyFalseValue);
                clientOptions.MaxRecentPosts = ReadInt(optionReader("maxRecentPosts"), existingOptions.MaxRecentPosts);
                clientOptions.ContentFilter = ReadText(optionReader("contentFilter"), existingOptions.ContentFilter);
                clientOptions.PermalinkFormat = ReadText(optionReader("permalinkFormat"), existingOptions.PermalinkFormat);
                clientOptions.PostDateFormat = ReadText(optionReader("postDateFormat"), existingOptions.PostDateFormat);
                clientOptions.FileUploadNameFormat = ReadText(optionReader("fileUploadNameFormat"), existingOptions.FileUploadNameFormat);
                clientOptions.UseLocalTime = ReadBool(optionReader("useLocalTime"), existingOptions.UseLocalTime);
                clientOptions.SupportsPostSynchronization = ReadBool(optionReader("supportsPostSynchronization"), existingOptions.SupportsPostSynchronization);
                clientOptions.TrackbackDelimiter = ReadTrackbackDelimiter(optionReader("trackbackDelimiter"), existingOptions.TrackbackDelimiter);
                clientOptions.FuturePublishDateWarning = ReadBool(optionReader("futurePublishDateWarning"), existingOptions.FuturePublishDateWarning);
                clientOptions.UsePicasaImgMaxAlways = ReadBool(optionReader("usePicasaImgMaxAlways"), existingOptions.UsePicasaImgMaxAlways);
                clientOptions.UsePicasaS1600h = ReadBool(optionReader("usePicasaS1600h"), existingOptions.UsePicasaS1600h);
                clientOptions.KeywordsAsTags = ReadBool(optionReader("keywordsAsTags"), existingOptions.KeywordsAsTags);
            }
            else
            {
                clientOptions.CommentPolicyAsBoolean = existingOptions.CommentPolicyAsBoolean;
                clientOptions.AllowPolicyFalseValue = existingOptions.AllowPolicyFalseValue;
                clientOptions.MaxRecentPosts = existingOptions.MaxRecentPosts;
                clientOptions.ContentFilter = existingOptions.ContentFilter;
                clientOptions.PermalinkFormat = existingOptions.PermalinkFormat;
                clientOptions.PostDateFormat = existingOptions.PostDateFormat;
                clientOptions.FileUploadNameFormat = existingOptions.FileUploadNameFormat;
                clientOptions.UseLocalTime = existingOptions.UseLocalTime;
                clientOptions.SupportsPostSynchronization = existingOptions.SupportsPostSynchronization;
                clientOptions.TrackbackDelimiter = existingOptions.TrackbackDelimiter;
                clientOptions.FuturePublishDateWarning = existingOptions.FuturePublishDateWarning;
                clientOptions.UsePicasaImgMaxAlways = existingOptions.UsePicasaImgMaxAlways;
                clientOptions.UsePicasaS1600h = existingOptions.UsePicasaS1600h;
                clientOptions.KeywordsAsTags = existingOptions.KeywordsAsTags;
            }

            // return options
            return clientOptions;
        }

        public static void ShowInNotepad(IBlogClientOptions options)
        {
            string clientOptionsFile = TempFileManager.Instance.CreateTempFile("BlogClientOptions.txt");
            using (StreamWriter stream = new StreamWriter(clientOptionsFile))
            {
                OptionStreamWriter writer = new OptionStreamWriter(stream);
                writer.Write(options);
            }

            _ = Process.Start(clientOptionsFile);
        }

        internal static bool ReadBool(string boolValue, bool defaultValue) => StringHelper.ToBool(boolValue, defaultValue);

        private static int ReadInt(string intValue, int defaultValue)
        {
            if (intValue != null)
            {
                try
                {
                    return int.Parse(intValue, CultureInfo.InvariantCulture);
                }
                catch (Exception e)
                {
                    Trace.Fail("Error parsing int string \"" + intValue + ": " + e.ToString());
                }
            }

            return defaultValue;
        }

        private static SupportsFeature ReadSupportsFeature(string supportsFeatureValue, SupportsFeature defaultValue)
        {
            if (supportsFeatureValue != null)
            {
                switch (supportsFeatureValue.Trim().ToUpperInvariant())
                {
                    case "UNKNOWN":
                        return SupportsFeature.Unknown;

                    case "YES":
                        return SupportsFeature.Yes;

                    case "NO":
                        return SupportsFeature.No;

                    default:
                        throw new ArgumentException("Invalid value for SupportsFeature: " + supportsFeatureValue);
                }
            }
            else
            {
                return defaultValue;
            }
        }

        private static string ReadText(string textValue, string defaultValue) => textValue != null ? textValue.Trim() : defaultValue;

        private static TrackbackDelimiter ReadTrackbackDelimiter(string delimiterValue, TrackbackDelimiter defaultValue)
        {
            if (delimiterValue != null)
            {
                switch (delimiterValue.Trim().ToUpperInvariant())
                {
                    case "ARRAYELEMENT":
                        return TrackbackDelimiter.ArrayElement;

                    case "SPACE":
                        return TrackbackDelimiter.Space;

                    case "COMMA":
                        return TrackbackDelimiter.Comma;
                }
            }

            return defaultValue;
        }

        private class OptionStreamWriter : DisplayableBlogClientOptionWriter
        {
            private readonly StreamWriter _output;

            public OptionStreamWriter(StreamWriter output) => this._output = output;

            protected override void WriteOption(string name, string value) => this._output.WriteLine("{0,-30}     {1}", name, value);
        }
    }

    public class BlogProvider : BlogProviderDescription, IBlogProvider
    {
        private static readonly SettingsPersisterHelper _postEditorSettings = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");

        private static readonly bool WP_ENABLED = _postEditorSettings.GetBoolean("M1Enabled", false);

        private string _homepageContentPattern;

        private Regex _homepageContentRegex;

        private string _homepageUrlPattern;

        private Regex _homepageUrlRegex;

        private ProviderFault[] _providerFaults;

        private RsdClientTypeMapping[] _rsdClientTypeMappings;

        private string _rsdEngineNamePattern;

        private Regex _rsdEngineNameRegex;

        private string _rsdHomepageLinkPattern;

        private Regex _rsdHomepageLinkRegex;

        protected BlogProvider()
        {
        }

        public bool Visible { get; private set; }

        private Regex HomepageContentRegex
        {
            get
            {
                if (this._homepageContentRegex == null && this._homepageContentPattern != string.Empty)
                {
                    try
                    {
                        this._homepageContentRegex = new Regex(this._homepageContentPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + this._homepageContentPattern);
                    }
                }

                return this._homepageContentRegex;
            }
        }

        private Regex HompepageUrlRegex
        {
            get
            {
                if (this._homepageUrlRegex == null && this._homepageUrlPattern != string.Empty)
                {
                    try
                    {
                        this._homepageUrlRegex = new Regex(this._homepageUrlPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + this._homepageUrlPattern);
                    }
                }

                return this._homepageUrlRegex;
            }
        }

        private Regex RsdEngineNameRegex
        {
            get
            {
                if (this._rsdEngineNameRegex == null && this._rsdEngineNamePattern != string.Empty)
                {
                    try
                    {
                        this._rsdEngineNameRegex = new Regex(this._rsdEngineNamePattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + this._rsdEngineNamePattern);
                    }
                }

                return this._rsdEngineNameRegex;
            }
        }

        private Regex RsdHomepageLinkRegex
        {
            get
            {
                if (this._rsdHomepageLinkRegex == null && this._rsdHomepageLinkPattern != string.Empty)
                {
                    try
                    {
                        this._rsdHomepageLinkRegex = new Regex(this._rsdHomepageLinkPattern, RegexOptions.IgnoreCase);
                    }
                    catch (ArgumentException)
                    {
                        Trace.Fail("Invalid regular expression: " + this._rsdHomepageLinkPattern);
                    }
                }

                return this._rsdHomepageLinkRegex;
            }
        }

        public virtual IBlogClientOptions ConstructBlogOptions(IBlogClientOptions defaultOptions) => defaultOptions;

        public virtual BlogAccount DetectAccountFromRsdEngineName(RsdServiceDescription rsdServiceDescription) => IsMatch(RsdEngineNameRegex, rsdServiceDescription.EngineName) ? ScanForSupportedRsdApi(rsdServiceDescription) : null;

        public virtual BlogAccount DetectAccountFromRsdHomepageLink(RsdServiceDescription rsdServiceDescription) => IsMatch(RsdHomepageLinkRegex, rsdServiceDescription.HomepageLink) ? ScanForSupportedRsdApi(rsdServiceDescription) : null;

        public virtual MessageId DisplayMessageForProviderError(string faultCode, string faultString)
        {
            foreach (ProviderFault providerFault in this._providerFaults)
            {
                if (providerFault.IsMatch(faultCode, faultString))
                {
                    return providerFault.MessageId;
                }
            }

            return MessageId.None;
        }

        public virtual bool IsProviderForHomepageContent(string homepageContent) => IsMatch(HomepageContentRegex, homepageContent);

        public virtual bool IsProviderForHomepageUrl(string homepageUrl) => IsMatch(HompepageUrlRegex, homepageUrl);

        protected void Init(
                                                                                                    string id, string name, string description, string link, string clientType, string postApiUrl,
            StringId postApiUrlLabel,
            string appid,
            string homepageUrlPattern, string homepageContentPattern,
            RsdClientTypeMapping[] rsdClientTypeMappings, string rsdEngineNamePattern, string rsdHomepageLinkPattern,
            ProviderFault[] providerFaults,
            bool visible)
        {
            base.Init(id, name, description, link, clientType, postApiUrl, postApiUrlLabel, appid);
            this._homepageUrlPattern = homepageUrlPattern;
            this._homepageContentPattern = homepageContentPattern;
            this._rsdClientTypeMappings = rsdClientTypeMappings.Clone() as RsdClientTypeMapping[];
            this._rsdEngineNamePattern = rsdEngineNamePattern;
            this._rsdHomepageLinkPattern = rsdHomepageLinkPattern;
            this._providerFaults = providerFaults;
            Visible = visible;
        }

        private bool IsMatch(Regex regex, string inputText) => regex != null && regex.IsMatch(inputText);

        private BlogAccount ScanForSupportedRsdApi(RsdServiceDescription rsdServiceDescription)
        {
            // initialize client type mappings (including mapping "implied" from ClientType itself
            ArrayList rsdClientTypeMappings = new ArrayList(this._rsdClientTypeMappings)
            {
                new RsdClientTypeMapping(ClientType, ClientType)
            };

            // scan for a match
            foreach (RsdClientTypeMapping mapping in rsdClientTypeMappings)
            {
                RsdApi rsdApi = rsdServiceDescription.ScanForApi(mapping.RsdClientType);
                if (rsdApi != null)
                {
                    // HACK: the internal spaces.msn-int site has a bug that causes it to return the
                    // production API URL, so force storage.msn.com to storage.msn-int.com
                    string postApiUrl = rsdApi.ApiLink;
                    if (rsdServiceDescription.HomepageLink.IndexOf("msn-int", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        postApiUrl = postApiUrl.Replace("storage.msn.com", "storage.msn-int.com");
                    }

                    return new BlogAccount(Name, mapping.ClientType, postApiUrl, rsdApi.BlogId);
                }
            }

            // no match
            return null;
        }

        protected readonly struct RsdClientTypeMapping
        {
            public readonly string ClientType;

            public readonly string RsdClientType;

            public RsdClientTypeMapping(string rsdClientType, string clientType)
            {
                this.RsdClientType = rsdClientType;
                this.ClientType = clientType;
            }
        }

        protected class ProviderFault
        {
            private readonly string _faultCodePattern;

            private readonly string _faultStringPattern;

            private readonly string _messageId;

            public ProviderFault(string faultCodePattern, string faultStringPattern, string messageId)
            {
                this._faultCodePattern = faultCodePattern;
                this._faultStringPattern = faultStringPattern;
                this._messageId = messageId;
            }

            public MessageId MessageId
            {
                get
                {
                    try
                    {
                        return (MessageId)MessageId.Parse(typeof(MessageId), this._messageId, false);
                    }
                    catch (ArgumentException)
                    {
                        return MessageId.None;
                    }
                }
            }

            public bool IsMatch(string faultCode, string faultString)
            {
                return this._faultCodePattern != string.Empty &&
                    this._faultStringPattern != string.Empty
                    ? FaultCodeMatches(faultCode) && FaultStringMatches(faultString)
                    : this._faultCodePattern != string.Empty
                        ? FaultCodeMatches(faultCode)
                        : this._faultStringPattern != string.Empty && FaultStringMatches(faultString);
            }

            private bool FaultCodeMatches(string faultCode)
            {
                try // defend against invalid regex in provider or manifest file
                {
                    Regex regex = new Regex(this._faultCodePattern, RegexOptions.IgnoreCase);
                    return regex.IsMatch(faultCode);
                }
                catch (ArgumentException e)
                {
                    Trace.Fail("Error processing regular expression: " + e.ToString());
                    return false;
                }
            }

            private bool FaultStringMatches(string faultString)
            {
                try // defend against invalid regex in provider or manifest file
                {
                    Regex regex = new Regex(this._faultStringPattern, RegexOptions.IgnoreCase);
                    return regex.IsMatch(faultString);
                }
                catch (ArgumentException e)
                {
                    Trace.Fail("Error processing regular expression: " + e.ToString());
                    return false;
                }
            }
        }
    }

    public class BlogProviderDescription : IBlogProviderDescription
    {
        public BlogProviderDescription(
            string id,
            string name,
            string description,
            string link,
            string clientType,
            string postApiUrl,
            StringId postApiUrlDescription,
            string appid) =>
            Init(id, name, description, link, clientType, postApiUrl, postApiUrlDescription, appid);

        protected BlogProviderDescription()
        {
        }

        public string AppId { get; private set; }

        public string ClientType { get; private set; }

        public string Description { get; private set; }

        public string Id { get; private set; }

        public string Link { get; private set; }

        public string Name { get; private set; }

        public string PostApiUrl { get; private set; }

        public StringId PostApiUrlLabel { get; private set; }

        protected void Init(
                                                                            string id, string name, string description, string link, string clientType, string postApiUrl, StringId postApiUrlLabel, string appid)
        {
            Id = id;
            Name = name;
            Description = description;
            Link = link;
            ClientType = clientType;
            PostApiUrl = postApiUrl;
            PostApiUrlLabel = postApiUrlLabel;
            AppId = appid;
        }
    }

    public abstract class DisplayableBlogClientOptionWriter
    {
        public void Write(IBlogClientOptions clientOptions)
        {
            WriteOption(Res.Get(StringId.CapabilityPostDraftToServer), clientOptions.SupportsPostAsDraft);
            WriteOption(Res.Get(StringId.CapabilityFileUpload), clientOptions.SupportsFileUpload);
            WriteOption(Res.Get(StringId.CapabilityExtendedEntries), clientOptions.SupportsExtendedEntries);
            WriteOption(Res.Get(StringId.CapabilityCustomPublishDate), clientOptions.SupportsCustomDate);
            WriteOption(Res.Get(StringId.CapabilityCustomPublishDateUpdate), clientOptions.SupportsCustomDateUpdate);
            WriteOption(Res.Get(StringId.CapabilityCategories), clientOptions.SupportsCategories);
            WriteOption(Res.Get(StringId.CapabilityCategoriesInline), clientOptions.SupportsCategoriesInline);
            WriteOption(Res.Get(StringId.CapabilityMultipleCategories), clientOptions.SupportsMultipleCategories);
            WriteOption(Res.Get(StringId.CapabilityHierarchicalCategories), clientOptions.SupportsHierarchicalCategories);
            WriteOption(Res.Get(StringId.CapabilityNewCategories), clientOptions.SupportsNewCategories);
            WriteOption(Res.Get(StringId.CapabilityNewCategoriesInline), clientOptions.SupportsNewCategoriesInline);
            WriteOption(Res.Get(StringId.CapabilityCategoryScheme), clientOptions.CategoryScheme);
            WriteOption(Res.Get(StringId.CapabilityKeywords), clientOptions.SupportsKeywords);
            WriteOption(Res.Get(StringId.CapabilityKeywordRetrieval), clientOptions.SupportsGetKeywords);
            WriteOption(Res.Get(StringId.CapabilityCommentPolicy), clientOptions.SupportsCommentPolicy);
            WriteOption(Res.Get(StringId.CapabilityTrackbackPolicy), clientOptions.SupportsPingPolicy);
            WriteOption(Res.Get(StringId.CapabilityAuthor), clientOptions.SupportsAuthor);
            WriteOption(Res.Get(StringId.CapabilitySlug), clientOptions.SupportsSlug);
            WriteOption(Res.Get(StringId.CapabilityPassword), clientOptions.SupportsPassword);
            WriteOption(Res.Get(StringId.CapabilityExcerpt), clientOptions.SupportsExcerpt);
            WriteOption(Res.Get(StringId.CapabilitySendTrackbacks), clientOptions.SupportsTrackbacks);
            WriteOption(Res.Get(StringId.CapabilityPages), clientOptions.SupportsPages);
            WriteOption(Res.Get(StringId.CapabilityPageParent), clientOptions.SupportsPageParent);
            WriteOption(Res.Get(StringId.CapabilityPageOrder), clientOptions.SupportsPageOrder);
            WriteOption(Res.Get(StringId.CapabilityHtmlTitles), clientOptions.RequiresHtmlTitles);
            WriteOption(Res.Get(StringId.CapabilityEmptyTitles), clientOptions.SupportsEmptyTitles);
            WriteOption(Res.Get(StringId.CapabilityScripts), clientOptions.SupportsScripts);
            WriteOption(Res.Get(StringId.CapabilityEmbeds), clientOptions.SupportsEmbeds);

            string defaultView;
            switch (clientOptions.DefaultView)
            {
                case "WebLayout":
                    defaultView = Res.Get(StringId.CapabilityValueWebLayout);
                    break;

                case "WebPreview":
                    defaultView = Res.Get(StringId.CapabilityValueWebPreview);
                    break;

                case "Normal":
                case "":
                case null:
                    defaultView = Res.Get(StringId.CapabilityValueNormal);
                    break;

                default:
                    defaultView = clientOptions.DefaultView;
                    break;
            }

            WriteOption(Res.Get(StringId.CapabilityDefaultView), defaultView);

            WriteOption(Res.Get(StringId.CapabilityCharacterSet), clientOptions.CharacterSet != string.Empty ? clientOptions.CharacterSet : "UTF-8");
            WriteOption(Res.Get(StringId.CapabilityRequiresXHTML), clientOptions.RequiresXHTML);
            WriteOption(Res.Get(StringId.CapabilityTemplateIsRTL), clientOptions.IsRTLTemplate);
            WriteOption(Res.Get(StringId.CapabilityCategoryNameLimit), clientOptions.MaxCategoryNameLength != 0 ? clientOptions.MaxCategoryNameLength.ToString(CultureInfo.CurrentCulture) : Res.Get(StringId.CapabilityValueNoLimit));
            WriteOption(Res.Get(StringId.CapabilityAutoUpdate), clientOptions.SupportsAutoUpdate);
            WriteOption(Res.Get(StringId.CapabilityPostTitleLengthLimit), clientOptions.MaxPostTitleLength != int.MaxValue ? clientOptions.MaxPostTitleLength.ToString(CultureInfo.CurrentCulture) : Res.Get(StringId.Unknown));
        }

        protected abstract void WriteOption(string name, string value);

        private void WriteOption(string name, bool value) => WriteOption(name, value ? Res.Get(StringId.Yes) : Res.Get(StringId.No));

        private void WriteOption(string name, SupportsFeature value)
        {
            switch (value)
            {
                case SupportsFeature.Yes:
                    WriteOption(name, Res.Get(StringId.Yes));
                    break;

                case SupportsFeature.No:
                    WriteOption(name, Res.Get(StringId.No));
                    break;

                case SupportsFeature.Unknown:
                    WriteOption(name, Res.Get(StringId.Unknown));
                    break;
            }
        }
    }
}