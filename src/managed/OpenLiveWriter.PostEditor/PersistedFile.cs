// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    /// <summary>
    /// The PersistedFile class.
    /// </summary>
    internal class PersistedFile
    {
        /// <summary>
        /// The path
        /// </summary>
        public readonly string Path;

        /// <summary>
        /// If true, save the file to the smart content and
        /// recreate when necessary.
        /// </summary>
        public readonly bool PersistToPostFile;

        /// <summary>
        /// The smart content name
        /// </summary>
        internal string SmartContentName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedFile"/> class.
        /// </summary>
        /// <param name="shouldPersistToPostFile">if set to <c>true</c> [should persist to post file].</param>
        /// <param name="path">The path.</param>
        public PersistedFile(bool shouldPersistToPostFile, string path)
        {
            this.PersistToPostFile = shouldPersistToPostFile;
            this.Path = path;
        }
    }
}
