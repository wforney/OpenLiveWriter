// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using System.IO;
using OpenLiveWriter.CoreServices.Progress;

namespace OpenLiveWriter.CoreServices
{
    /// <summary>
    /// Service Locator for IBrowserBasedWebRequest.
    /// </summary>
    public class BrowserBasedWebRequestLocator
    {
        private static IBrowserBasedWebRequest service;

        public static IBrowserBasedWebRequest Instance
        {
            get
            {
                EnsureInitialized();
                return service;
            }
        }

        public static void Initialize(IBrowserBasedWebRequest instance)
        {
            if (service != null)
                Trace.Assert(object.ReferenceEquals(service, instance), "BrowserBasedWebRequestLocator was initialized twice with different values");
            service = instance;
        }

        private static void EnsureInitialized()
        {
            if (service == null)
            {
                service = new UrlDownloadToFileImpl();
            }
        }

        private class UrlDownloadToFileImpl : IBrowserBasedWebRequest
        {

            public Stream HttpGet(string url)
            {
                UrlDownloadToFile urlDownloadToFile = new UrlDownloadToFile
                {
                    Url = url,
                    DownloadAction = UrlDownloadToFile.DownloadActions.GET,
                    ShowSecurityUI = true
                };
                return DoDownload(urlDownloadToFile);
            }

            public Stream HttpPost(string url, Stream postData)
            {
                UrlDownloadToFile urlDownloadToFile = new UrlDownloadToFile
                {
                    Url = url,
                    DownloadAction = UrlDownloadToFile.DownloadActions.POST,
                    ShowSecurityUI = true,
                    PostData = StreamHelper.AsBytes(postData)
                };
                return DoDownload(urlDownloadToFile);
            }

            private static Stream DoDownload(UrlDownloadToFile urlDownloadToFile)
            {
                string path = TempFileManager.Instance.CreateTempFile("urldownload");
                urlDownloadToFile.FilePath = path;
                urlDownloadToFile.Download(SilentProgressHost.Instance);
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
        }
    }
}
