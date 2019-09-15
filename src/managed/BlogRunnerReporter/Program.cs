// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunnerReporter
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.Mail;
    using System.Threading;

    /// <summary>
    /// The program class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        public static int Main(string[] args)
        {
            try
            {
                var input = Path.GetFullPath(args[0]);
                var output = Path.GetFullPath(args[1]);
                var errors = Path.GetFullPath(args[2]);
                var report = Path.GetFullPath(args[3]);

                CheckPathExists("input", input);
                CheckPathExists("output", output);
                CheckPathExists("errors", errors);

                var hasErrors = new FileInfo(errors).Length > 0;
                var filesDiffer = !Compare(input, output);

                if (!hasErrors && !filesDiffer)
                {
                    return 0;
                }

                if (filesDiffer)
                {
                    var diffCommand = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                        @"Beyond Compare 2\bc2.exe");

                    if (!File.Exists(diffCommand))
                    {
                        Console.Error.WriteLine("Warning: Beyond Compare 2 is not installed; diff report generation will be skipped");
                    }
                    else
                    {
                        var diffCommandArgs = string.Format(
                            CultureInfo.InvariantCulture,
                            @"/silent @bcscript.txt ""{0}"" ""{1}"" ""{2}""",
                            input,
                            output,
                            report);

                        var p = Process.Start(diffCommand, diffCommandArgs);
                        p.WaitForExit();
                        Thread.Sleep(3000);
                    }
                }

                var notificationType =
                    (hasErrors && filesDiffer) ? "changes and errors" :
                    hasErrors ? "errors" :
                    "changes";

                using (Stream outputStream = File.OpenRead(output))
                using (Stream reportStream = File.Exists(report) ? File.OpenRead(report) : null)
                using (Stream errorsStream = hasErrors ? File.OpenRead(errors) : null)
                {
                    var msg = new MailMessage("wlwbuild@microsoft.com", "wlwbuild@microsoft.com");
                    if (filesDiffer)
                    {
                        msg.Attachments.Add(new Attachment(outputStream, "BlogProvidersB5.xml", "text/xml"));
                        if (reportStream != null)
                        {
                            msg.Attachments.Add(new Attachment(reportStream, "diff.htm", "text/html"));
                        }
                    }

                    if (errorsStream != null)
                    {
                        msg.Attachments.Add(new Attachment(errorsStream, "errors.txt", "text/plain"));
                    }

                    msg.Subject = "Blog provider " + notificationType + " detected";
                    msg.Body = notificationType + " detected while running blog provider tests. Please see attached.";

                    using (var client = new SmtpClient
                    {
                        DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis,
                    })
                    {
                        client.Send(msg);
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                return 2;
            }
        }

        /// <summary>
        /// Compares the specified files.
        /// </summary>
        /// <param name="file1">The first file.</param>
        /// <param name="file2">The second file.</param>
        /// <returns><c>true</c> if the file contents are the same, <c>false</c> otherwise.</returns>
        private static bool Compare(string file1, string file2)
        {
            var f1 = new FileInfo(file1);
            var f2 = new FileInfo(file2);

            if (f1.Length != f2.Length)
            {
                return false;
            }

            using (Stream s1 = f1.OpenRead())
            using (Stream s2 = f2.OpenRead())
            {
                for (var i = 0; i < s1.Length; i++)
                {
                    if (s1.ReadByte() != s2.ReadByte())
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks the path exists.
        /// </summary>
        /// <param name="label">The label.</param>
        /// <param name="path">The path.</param>
        /// <exception cref="System.ArgumentException">The file does not exist.</exception>
        private static void CheckPathExists(string label, string path)
        {
            if (!File.Exists(path))
            {
                throw new ArgumentException($"{label} file does not exist: {path}");
            }
        }
    }
}
