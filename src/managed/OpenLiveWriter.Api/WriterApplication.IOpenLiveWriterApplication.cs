// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.Api
{
    using System;
    using System.Runtime.InteropServices;

    public sealed partial class WriterApplication
    {
        /// <summary>
        /// Interface IOpenLiveWriterApplication
        /// </summary>
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsDual)]
        [Guid("D7E5B1EC-FEEA-476C-9CE1-BC5C3E2DB662")]
        private interface IOpenLiveWriterApplication
        {
            /// <summary>
            /// Creates new post.
            /// </summary>
            void NewPost();

            /// <summary>
            /// Opens the post.
            /// </summary>
            void OpenPost();

            /// <summary>
            /// Shows the options.
            /// </summary>
            /// <param name="optionsPage">The options page.</param>
            void ShowOptions(string optionsPage);
        }
    }
}
