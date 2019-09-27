// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Text;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Provides the ability to execute Http requests that utilize the (optional) Web Proxy
    /// settings specified by the user in the Web Proxy Preferences panel. Also enables
    /// reading from and writing to the Internet cache as part of request processing.
    /// </summary>
    public class PluginHttpRequest
    {
        /// <summary>
        /// Is an Internet connection currently available.
        /// </summary>
        public static bool InternetConnectionAvailable => WinInet.InternetGetConnectedState(out var flags, 0);

        /// <summary>
        /// Returns the Web proxy that Writer is configured to use.
        /// </summary>
        public static IWebProxy GetWriterProxy()
        {
            IWebProxy proxy = HttpRequestHelper.GetProxyOverride();

            if (proxy == null)
            {
                // TODO: Some plugins (like Flickr4Writer) cast this to a WebProxy
                // Since the fix for this returns an explicit IWebProxy, we'll need to have
                // the Flickr4Writer plugin fixed, then alter this to use the correct call.
#pragma warning disable 612, 618
                proxy = System.Net.WebProxy.GetDefaultProxy();
            }
#pragma warning restore 612, 618
            return proxy;
        }

        /// <summary>
        /// Initialize a new Http request.
        /// </summary>
        /// <param name="requestUrl">Url for resource to request.</param>
        public PluginHttpRequest(string requestUrl)
            : this(requestUrl, HttpRequestCacheLevel.BypassCache)
        {
        }

        /// <summary>
        /// Initialize a new Http request with the specified cache level.
        /// </summary>
        /// <param name="requestUrl">Url for resource to request.</param>
        /// <param name="cacheLevel">Cache level for request.</param>
        public PluginHttpRequest(string requestUrl, HttpRequestCacheLevel cacheLevel)
        {
            this.requestUrl = requestUrl;
            this.CacheLevel = cacheLevel;
        }

        /// <summary>
        /// Automatically follow host redirects of the request (defaults to true).
        /// </summary>
        public bool AllowAutoRedirect { get; set; } = true;

        /// <summary>
        /// Cache level for Http request (defaults to BypassCache).
        /// </summary>
        public HttpRequestCacheLevel CacheLevel { get; set; } = HttpRequestCacheLevel.BypassCache;

        /// <summary>
        /// Content-type of post data (this value must be specified if post data is included
        /// in the request).
        /// </summary>
        public string ContentType { get; set; } = null;

        /// <summary>
        /// Post data to send along with the request.
        /// </summary>
        public byte[] PostData { get; set; }

        /// <summary>
        /// Retrieve the resource (with no timeout).
        /// </summary>
        /// <returns>A stream representing the requested resource. Can return null
        /// if the CacheLevel is CacheOnly and the resource could not be found
        /// in the cache.</returns>
        public Stream GetResponse() => this.GetResponse(System.Threading.Timeout.Infinite);

        /// <summary>
        /// Retrieve the resource with the specified timeout (in ms).
        /// </summary>
        /// <param name="timeoutMs">Timeout (in ms) for the request.</param>
        /// <returns>A stream representing the requested resource. Can return null
        /// if the CacheLevel is CacheOnly and the resource could not be found
        /// in the cache.</returns>
        public Stream GetResponse(int timeoutMs)
        {
            // always try to get the url from the cache first
            if (this.ReadFromCache)
            {
                if (WinInet.GetUrlCacheEntryInfo(this.requestUrl, out var cacheInfo))
                {
                    if (File.Exists(cacheInfo.lpszLocalFileName))
                    {
                        return new FileStream(cacheInfo.lpszLocalFileName, FileMode.Open, FileAccess.Read);
                    }
                }
            }

            // if that didn't succeed then try to get the file from
            // the web as long as the user has requested we do this
            if (this.MakeRequest)
            {
                var response = HttpRequestHelper.SendRequest(this.requestUrl, delegate (HttpWebRequest request)
                {
                    request.AllowAutoRedirect = this.AllowAutoRedirect;
                    request.Timeout = timeoutMs;
                    request.ContentType = this.ContentType;
                    if (this.PostData != null)
                    {
                        request.Method = "POST";
                        using (var requestStream = request.GetRequestStream())
                        {
                            StreamHelper.Transfer(new MemoryStream(this.PostData), requestStream);
                        }
                    }
                });

                try
                {
                    var responseStream = response.GetResponseStream();
                    return responseStream == null
                        ? null
                        : this.WriteToCache ? this.WriteResponseToCache(responseStream) : StreamHelper.CopyToMemoryStream(responseStream);
                }
                finally
                {
                    response.Close();
                }
            }
            else
            {
                // look only in the cache
                return null;
            }
        }

        private bool ReadFromCache => this.CacheLevel == HttpRequestCacheLevel.CacheIfAvailable ||
                        this.CacheLevel == HttpRequestCacheLevel.CacheOnly;

        private bool MakeRequest => this.CacheLevel != HttpRequestCacheLevel.CacheOnly;

        private bool WriteToCache => this.CacheLevel == HttpRequestCacheLevel.CacheIfAvailable ||
                        this.CacheLevel == HttpRequestCacheLevel.CacheOnly ||
                        this.CacheLevel == HttpRequestCacheLevel.Reload;

        private Stream WriteResponseToCache(Stream responseStream)
        {
            var fileNameBuffer = new StringBuilder(Kernel32.MAX_PATH * 2);
            var created = WinInet.CreateUrlCacheEntry(
                this.requestUrl, 0, UrlHelper.GetExtensionForUrl(this.requestUrl), fileNameBuffer, 0);

            if (created)
            {
                // copy the stream to the file
                var cacheFileName = fileNameBuffer.ToString();
                using (var cacheFile = new FileStream(cacheFileName, FileMode.Create))
                {
                    StreamHelper.Transfer(responseStream, cacheFile);
                }

                // commit the file to the cache

                var zeroFiletime = new System.Runtime.InteropServices.ComTypes.FILETIME();
                var committed = WinInet.CommitUrlCacheEntry(
                    this.requestUrl, cacheFileName, zeroFiletime, zeroFiletime, CACHE_ENTRY.NORMAL, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);
                Trace.Assert(committed);

                // return a stream to the file
                return new FileStream(cacheFileName, FileMode.Open);
            }
            else
            {
                Trace.Fail("Unexpedcted failure to create cache entry for url " + this.requestUrl + ": " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
                return responseStream;
            }
        }

        private readonly string requestUrl;
    }
}
