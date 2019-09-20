// <copyright file="Log.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunner.Core
{
    using System;

    /// <summary>
    /// The log class.
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// The indent level.
        /// </summary>
        [ThreadStatic]
        private static int indentLevel;

        /// <summary>
        /// The action delegate.
        /// </summary>
        public delegate void Action();

        /// <summary>
        /// Gets the indent.
        /// </summary>
        /// <value>The indent.</value>
        private static string Indent => new string(' ', indentLevel * 2);

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void WriteLine(string message)
        {
            if (indentLevel > 0)
            {
                message = Indent + message?.Replace("\n", Indent);
            }

            Console.WriteLine(message);
        }

        /// <summary>
        /// Sections the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The action.</param>
        public static void Section(string name, Action action)
        {
            WriteLine("/== " + name + " ====");
            indentLevel++;
            try
            {
                action?.Invoke();
            }
            finally
            {
                indentLevel--;
            }

            WriteLine(@"\== " + name + " ====");
        }
    }
}
