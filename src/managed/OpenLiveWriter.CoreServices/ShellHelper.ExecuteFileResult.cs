// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    public partial class ShellHelper
    {
        /// <summary>
        /// The ExecuteFileResult structure.
        /// </summary>
        public struct ExecuteFileResult
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ExecuteFileResult"/> struct.
            /// </summary>
            /// <param name="newProcessCreated">if set to <c>true</c> [new process created].</param>
            /// <param name="processId">The process identifier.</param>
            /// <param name="processName">Name of the process.</param>
            public ExecuteFileResult(bool newProcessCreated, int processId, string processName)
                : this(newProcessCreated, new[] { processId }, new[] { processName })
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecuteFileResult"/> struct.
            /// </summary>
            /// <param name="newProcessCreated">if set to <c>true</c> [new process created].</param>
            /// <param name="processIdList">The process identifier list.</param>
            /// <param name="processNameList">The process name list.</param>
            public ExecuteFileResult(bool newProcessCreated, int[] processIdList, string[] processNameList)
            {
                this.NewProcessCreated = newProcessCreated;
                this.ProcessIdList = processIdList;
                this.ProcessNameList = processNameList;
            }

            /// <summary>
            /// A value indicating whether a new process was created.
            /// </summary>
            public bool NewProcessCreated;

            /// <summary>
            /// The process identifier list
            /// </summary>
            public int[] ProcessIdList;

            /// <summary>
            /// The process name list
            /// </summary>
            public string[] ProcessNameList;
        }
    }
}
