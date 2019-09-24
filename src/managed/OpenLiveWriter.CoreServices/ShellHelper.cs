// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.CoreServices
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;

    using Microsoft.Win32;

    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// The shell helper class.
    /// </summary>
    public partial class ShellHelper
    {
        /// <summary>
        /// Extension used for shortcuts
        /// </summary>
        public static string ShortcutExtension = ".lnk";

        /// <inheritdoc />
        private ShellHelper()
        {
        }

        /// <summary>
        /// Gets the path extensions.
        /// </summary>
        /// <value>The path extensions.</value>
        public static string[] PathExtensions
        {
            get
            {
                var pathExtension = Environment.GetEnvironmentVariable("PATHEXT") ?? ".COM;.EXE;.BAT;.CMD";

                var pathExtensions = StringHelper.Split(pathExtension, ";").ToArray();
                for (var i = 0; i < pathExtensions.Length; i++)
                {
                    pathExtensions[i] = pathExtensions[i].ToLower(CultureInfo.CurrentCulture).Trim();
                }

                return pathExtensions;
            }
        }

        /// <summary>
        /// Shell-executes a file with a specific verb, then returns either the exact
        /// process that was launched, or if nothing was launched (i.e. a process was
        /// reused), returns a set of processes that might be handling the file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="verb">The verb.</param>
        /// <returns>An <see cref="ExecuteFileResult"/>.</returns>
        public static ExecuteFileResult ExecuteFile(string filePath, string verb)
        {
            // Execute the document using the shell.
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = filePath,
                Verb = verb
            };

            using (var p = Process.Start(startInfo))
            {
                if (p != null)
                {
                    var processName = ProcessHelper.GetProcessName(p.Handle);
                    if (processName != null)
                    {
                        return new ExecuteFileResult(true, p.Id, processName);
                    }
                }
            }

            var command = ShellHelper.GetExecutablePath(Path.GetExtension(filePath), verb);

            // A process was reused.  Need to find all the possible processes
            // that could have been reused and return them all.
            int[] processIds;
            string[] processNames;

            if (command == null)
            {
                // The extension/verb combination has no registered application.
                // We can't even guess at what process could be editing the file.
                processIds = new int[0];
                processNames = new string[0];
            }
            else
            {
                // A registered app was found.  We assume that a process with the
                // same name is editing the file.
                var imageName = Path.GetFileName(command);
                processIds = ProcessHelper.GetProcessIdsByName(imageName);
                processNames = new string[processIds.Length];
                for (var i = 0; i < processIds.Length; i++)
                {
                    processNames[i] = imageName;
                }
            }

            return new ExecuteFileResult(false, processIds, processNames);
        }

        /// <summary>
        /// Executes the file with executable.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="executable">The executable.</param>
        /// <returns>An <see cref="ExecuteFileResult"/>.</returns>
        public static ExecuteFileResult ExecuteFileWithExecutable(string filePath, string executable)
        {
            var startInfo = new ProcessStartInfo(executable)
            {
                Arguments = $"\"{filePath}\""
            };

            using (var p = Process.Start(startInfo))
            {
                if (p != null)
                {
                    return new ExecuteFileResult(true, p.Id, ProcessHelper.GetProcessName(p.Handle));
                }
            }

            var imageName = Path.GetFileName(executable);
            var processIds = ProcessHelper.GetProcessIdsByName(imageName);
            var processNames = new string[processIds.Length];
            for (var i = 0; i < processIds.Length; i++)
            {
                processNames[i] = imageName;
            }

            return new ExecuteFileResult(false, processIds, processNames);
        }

        /// <summary>
        /// Gets the absolute path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The absolute path.</returns>
        public static string GetAbsolutePath(string path) =>
            Uri.IsWellFormedUriString(path, UriKind.Absolute) ? path : Path.GetFullPath(path);

        /// <summary>
        /// For a file extension (with leading period) and a verb (or null for default
        /// verb), returns the (full?) path to the executable file that is assigned to
        /// that extension/verb.  Returns null if an error occurs.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <param name="verb">The verb.</param>
        /// <returns>The executable path.</returns>
        public static string GetExecutablePath(string extension, string verb)
        {
            var capacity = 270;

            attempt: // we may need to retry with a different (larger) value for "capacity"
            var buffer = new StringBuilder(capacity); // the buffer that will hold the result
            var hresult = Shlwapi.AssocQueryString(
                ASSOCF.NOTRUNCATE,
                ASSOCSTR.EXECUTABLE,
                extension,
                verb,
                buffer,
                ref capacity);

            switch (hresult)
            {
                case HRESULT.S_OK:
                    return buffer.ToString(); // success; return the path

                // failure; buffer was too small
                case HRESULT.E_POINTER:
                case HRESULT.S_FALSE:
                    // the capacity variable now holds the number of chars necessary (AssocQueryString
                    // assigns it).  it should work if we try again.
                    goto attempt;

                // failure.  the default case will catch all, but I'm explicitly
                // calling out the two failure codes I know about in case we need
                // them someday.
                case HRESULT.E_INVALIDARG:
                case HRESULT.E_FAILED:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the large icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        public static IconHandle GetLargeIconForExtension(string extension) =>
            ShellHelper.GetIconForExtension(extension, SHGFI.LARGEICON);

        /// <summary>
        /// Get the large icon for the specified file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        public static IconHandle GetLargeIconForFile(string filePath) =>
            ShellHelper.GetIconForFile(filePath, SHGFI.LARGEICON);

        /// <summary>
        /// Gets the large shortcut icon for extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>An <see cref="IconHandle"/>.</returns>
        public static IconHandle GetLargeShortcutIconForExtension(string extension) =>
            ShellHelper.GetIconForExtension(extension, SHGFI.LARGEICON | SHGFI.LINKOVERLAY);

        /// <summary>
        /// Get the small icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        public static IconHandle GetSmallIconForExtension(string extension) =>
            ShellHelper.GetIconForExtension(extension, SHGFI.SMALLICON);

        /// <summary>
        /// Get the small icon for the specified file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        public static IconHandle GetSmallIconForFile(string filePath) =>
            ShellHelper.GetIconForFile(filePath, SHGFI.SMALLICON);

        /// <summary>
        /// Get the small icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        public static IconHandle GetSmallShortcutIconForExtension(string extension) =>
            ShellHelper.GetIconForExtension(extension, SHGFI.SMALLICON | SHGFI.LINKOVERLAY);

        /// <summary>
        /// Gets the friendly type string for an extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>The type name for the specified extension.</returns>
        public static string GetTypeNameForExtension(string extension)
        {
            extension = extension?.Trim();

            var capacity = 270;

            attempt:
            var builder = new StringBuilder(capacity);
            var hresult = Shlwapi.AssocQueryString(
                ASSOCF.NOTRUNCATE,
                ASSOCSTR.FRIENDLYDOCNAME,
                extension,
                null,
                builder,
                ref capacity);

            switch (hresult)
            {
                case HRESULT.S_OK:
                    return builder.ToString();
                case HRESULT.E_POINTER:
                case HRESULT.S_FALSE:
                    // the capacity variable now holds the number of chars necessary.  try again
                    goto attempt;
                case HRESULT.E_INVALIDARG:
                case HRESULT.E_FAILED:
                default:
                    break;
            }

            return string.IsNullOrEmpty(extension)
                       ? "Unknown"
                       : $"{extension.TrimStart('.').ToUpper(CultureInfo.InvariantCulture)} File";
        }

        /// <summary>
        /// Determine if there is a custom icon handler for the specified file extension
        /// </summary>
        /// <param name="fileExtension">file extension (including ".")</param>
        /// <returns>true if it has a custom icon handler, otherwise false</returns>
        public static bool HasCustomIconHandler(string fileExtension)
        {
            using (var key = Registry.ClassesRoot.OpenSubKey(fileExtension))
            {
                if (key == null)
                {
                    return false;
                }

                using (var classKey = Registry.ClassesRoot.OpenSubKey(
                    key.GetValue(null, string.Empty) + @"\ShellEx\IconHandler"))
                {
                    return classKey != null;
                }
            }
        }

        /// <summary>
        /// Determines whether [is path extension] [the specified extension].
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns><c>true</c> if [is path extension] [the specified extension]; otherwise, <c>false</c>.</returns>
        public static bool IsPathExtension(string extension) =>
            Array.IndexOf(ShellHelper.PathExtensions, extension) >= 0;

        /// <summary>
        /// This should be used as the default URL launching method, instead of Process.Start.
        /// </summary>
        /// <param name="url">The URL.</param>
        public static void LaunchUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch (Win32Exception w32E)
            {
                // Benign but common error due to Firefox and/or Windows stupidity
                // http://kb.mozillazine.org/Windows_error_opening_Internet_shortcut_or_local_HTML_file_-_Firefox
                // The unchecked cast is necessary to make the uint wrap around to the proper int error code.
                if (w32E.ErrorCode == unchecked((int)0x80004005))
                {
                    return;
                }

                throw;
            }
        }

        /// <summary>
        /// Parses the command.
        /// </summary>
        /// <param name="command">The command.</param>
        /// <param name="executable">The executable.</param>
        /// <param name="arguments">The arguments.</param>
        /// <exception cref="ArgumentNullException">command - Command cannot be null</exception>
        /// <exception cref="ArgumentOutOfRangeException">command - Command cannot be the empty string</exception>
        public static void ParseCommand(string command, out string executable, out string arguments)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command), Exceptions.CommandCannotBeNull);
            }

            if (command.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(command), Exceptions.CommandCannotBeTheEmptyString);
            }

            command = command.TrimStart();

            if (command[0] == '"')
            {
                var split = command.IndexOf('"', 1);
                if (split != -1)
                {
                    executable = command.Substring(1, split - 1);
                    arguments = string.Empty;
                    if (command.Length > split + 2)
                    {
                        arguments = command.Substring(split + 2);
                    }
                }
                else
                {
                    executable = command;
                    arguments = string.Empty;
                }
            }
            else
            {
                var split = command.IndexOf(' ');
                if (split != -1)
                {
                    executable = command.Substring(0, split);
                    arguments = string.Empty;
                    if (command.Length > split + 1)
                    {
                        arguments = command.Substring(split + 1);
                    }
                }
                else
                {
                    executable = command;
                    arguments = string.Empty;
                }
            }
        }

        /// <summary>
        /// Parse a "shell" file list (space delimited list with file names that contain spaces
        /// being contained in quotes
        /// </summary>
        /// <param name="fileList">shell file list</param>
        /// <returns>array of file paths in the file list</returns>
        public static string[] ParseShellFileList(string fileList)
        {
            // otherwise check for a "shell format" list
            fileList = fileList.Trim();
            var fileListArray = new ArrayList();
            var currentLoc = 0;

            // scan for file entries
            while (currentLoc < fileList.Length)
            {
                // file entry
                string file = null;

                // skip leading white-space
                while (currentLoc < fileList.Length && char.IsWhiteSpace(fileList[currentLoc]))
                {
                    currentLoc++;
                }

                // account for quoted entries
                if (fileList[currentLoc] == '"')
                {
                    // find next quote
                    var nextQuote = fileList.IndexOf('"', currentLoc + 1);
                    if (nextQuote != -1)
                    {
                        file = fileList.Substring(currentLoc + 1, nextQuote - currentLoc - 1);
                        currentLoc = nextQuote + 1;
                    }
                    else
                    {
                        break; // no end quote!
                    }
                }

                // if we didn't have a quoted entry then find next space delimited entry
                if (file == null)
                {
                    // skip leading white-space
                    while (currentLoc < fileList.Length && char.IsWhiteSpace(fileList[currentLoc]))
                    {
                        currentLoc++;
                    }

                    // if we aren't at the end then get the next entry
                    if (currentLoc < fileList.Length)
                    {
                        // find the end of the entry
                        var endEntry = currentLoc;
                        while (endEntry < fileList.Length)
                        {
                            if (!char.IsWhiteSpace(fileList[endEntry]))
                            {
                                endEntry++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        // get the value for the entry
                        file = fileList.Substring(currentLoc, endEntry - currentLoc);
                        currentLoc = endEntry;
                    }
                    else
                    {
                        break; // at the end
                    }
                }

                // add the file to our list
                fileListArray.Add(file.Trim());
            }

            // return the list
            return (string[])fileListArray.ToArray(typeof(string));
        }

        /// <summary>
        /// Sets the window's application id by its window handle.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="appId">The application id.</param>
        public static void SetWindowAppId(IntPtr hwnd, string appId) =>
            ShellHelper.SetWindowProperty(hwnd, SystemProperties.System.AppUserModel.ID, appId);

        /// <summary>
        /// Gets the window property store.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <returns>IPropertyStore.</returns>
        internal static IPropertyStore GetWindowPropertyStore(IntPtr hwnd)
        {
            var guid = new Guid(Shell32.IPropertyStore);
            var rc = Shell32.SHGetPropertyStoreForWindow(hwnd, ref guid, out var propStore);
            if (rc != 0)
            {
                throw Marshal.GetExceptionForHR(rc);
            }

            return propStore;
        }

        /// <summary>
        /// Sets the window property.
        /// </summary>
        /// <param name="hwnd">The HWND.</param>
        /// <param name="propkey">The propkey.</param>
        /// <param name="value">The value.</param>
        internal static void SetWindowProperty(IntPtr hwnd, PropertyKey propkey, string value)
        {
            // Get the IPropertyStore for the given window handle
            var propStore = ShellHelper.GetWindowPropertyStore(hwnd);

            // Set the value
            var pv = new PropVariant();
            propStore.SetValue(ref propkey, ref pv);

            // Dispose the IPropertyStore and PropVariant
            Marshal.ReleaseComObject(propStore);
            pv.Clear();
        }

        /// <summary>
        /// Get the icon for the specified file extension
        /// </summary>
        /// <param name="extension">file extension (including leading .)</param>
        /// <param name="flags">icon type (small or large)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        private static IconHandle GetIconForExtension(string extension, uint flags)
        {
            // allocate SHFILEINFO for holding results
            var fileInfo = new SHFILEINFO();

            // get icon info for file extension
            var result = Shell32.SHGetFileInfo(
                extension,
                FILE_ATTRIBUTE.NORMAL,
                ref fileInfo,
                (uint)Marshal.SizeOf(fileInfo),
                SHGFI.ICON | flags | SHGFI.USEFILEATTRIBUTES);
            if (result == IntPtr.Zero)
            {
                Debug.Fail($"Error getting icon for file: {Marshal.GetLastWin32Error()}");
                return null;
            }

            // return IconHandle
            return new IconHandle(fileInfo.hIcon);
        }

        /// <summary>
        /// Get the icon for the specified file
        /// </summary>
        /// <param name="filePath">path to file</param>
        /// <param name="iconType">icon type (small or large)</param>
        /// <returns>IconHandle (or null if none found). IconHandle represents
        /// a Win32 HICON. It must be Disposed when the caller is finished with
        /// it to free the underlying resources used by the Icon.</returns>
        private static IconHandle GetIconForFile(string filePath, uint iconType)
        {
            // allocate SHFILEINFO for holding results
            var fileInfo = new SHFILEINFO();

            // get icon info
            var result = Shell32.SHGetFileInfo(
                filePath,
                0,
                ref fileInfo,
                (uint)Marshal.SizeOf(fileInfo),
                SHGFI.ICON | iconType);
            if (result == IntPtr.Zero)
            {
                Debug.Fail($"Error getting icon for file: {Marshal.GetLastWin32Error()}");
                return null;
            }

            // return IconHandle
            return new IconHandle(fileInfo.hIcon);
        }
    }
}
