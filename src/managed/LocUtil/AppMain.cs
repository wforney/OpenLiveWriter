// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace LocUtil
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.Xsl;

    using OpenLiveWriter.ApplicationFramework;
    using OpenLiveWriter.Controls;
    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class AppMain.
    /// </summary>
    public class AppMain
    {
        /// <summary>
        /// The comment namespace
        /// </summary>
        private const string CommentNamespace = "http://OpenLiveWriter.spaces.live.com/#comment";

        /////// <summary>
        /////// The comment attribute
        /////// </summary>
        ////private const string COMMENT_ATTRIBUTE = "Comment";

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ConfigurationErrorsException">File not found: " + inputFile</exception>
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length == 1 && args[0].ToLowerInvariant() == "/validateall")
            {
                return ValidateAll() ? 0 : 1;
            }

            var clo = new CommandLineOptions(
                new ArgSpec("c", ArgSpec.Options.Multiple, "Command XML input file(s)"),
                new ArgSpec("r", ArgSpec.Options.Multiple, "Ribbon XML input file"),
                new ArgSpec("d", ArgSpec.Options.Multiple, "DisplayMessage XML input file(s)"),
                new ArgSpec("s", ArgSpec.Options.Multiple, "Strings CSV input file(s)"),
                new ArgSpec("cenum", ArgSpec.Options.Default, "Path to CommandId.cs enum output file"),
                new ArgSpec("denum", ArgSpec.Options.Default, "Path to MessageId.cs enum output file"),
                new ArgSpec("senum", ArgSpec.Options.Default, "Path to StringId.cs enum output file"),
                new ArgSpec("props", ArgSpec.Options.Required, "Path to Properties.resx output file"),
                new ArgSpec("propsnonloc", ArgSpec.Options.Required, "Path to PropertiesNonLoc.resx output file"),
                new ArgSpec("strings", ArgSpec.Options.Required, "Path to Strings.resx output file")
                );

            if (!clo.Parse(args, true))
            {
                Console.Error.WriteLine(clo.ErrorMessage);
                return 1;
            }

            var pairsLoc = new Hashtable();
            var pairsNonLoc = new Hashtable();

            var commandFiles = (string[])ArrayHelper.Narrow(clo.GetValues("c"), typeof(string));
            var dialogFiles = (string[])ArrayHelper.Narrow(clo.GetValues("d"), typeof(string));
            var ribbonFiles = (string[])ArrayHelper.Narrow(clo.GetValues("r"), typeof(string));
            var stringFiles = (string[])ArrayHelper.Narrow(clo.GetValues("s"), typeof(string));

            if (commandFiles.Length + dialogFiles.Length + stringFiles.Length == 0)
            {
                Console.Error.WriteLine("No input files were specified.");
                return 1;
            }

            if (commandFiles.Length + dialogFiles.Length > 0)
            {
                Console.WriteLine($"Parsing commands from {StringHelper.Join(commandFiles, ";")}");
                if (!ParseRibbonXml(ribbonFiles, pairsLoc, pairsNonLoc, typeof(Command), "//ribbon:Command", "Command.{0}.{1}", out var ribbonIds, out var ribbonValues))
                {
                    return 1;
                }

                Console.WriteLine($"Parsing commands from {StringHelper.Join(commandFiles, ";")}");

                var transformedCommandFiles = commandFiles;
                try
                {
                    // Transform the files
                    var xslTransform = new XslCompiledTransform(true);
                    var xslFile = Path.GetFullPath("Commands.xsl");

                    for (var i = 0; i < commandFiles.Length; i++)//string filename in commandFiles)
                    {
                        var inputFile = Path.GetFullPath(commandFiles[i]);
                        if (!File.Exists(inputFile))
                        {
                            throw new ConfigurationErrorsException($"File not found: {inputFile}");
                        }

                        xslTransform.Load(xslFile);

                        var transformedFile = inputFile.Replace(".xml", ".transformed.xml");
                        xslTransform.Transform(inputFile, transformedFile);
                        transformedCommandFiles[i] = transformedFile;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to transform file: {ex}");
                    return 1;
                }

                if (!ParseCommandXml(transformedCommandFiles, pairsLoc, pairsNonLoc, typeof(Command), "/Commands/Command", "Command.{0}.{1}", out var commandIds))
                {
                    return 1;
                }

                Console.WriteLine($"Parsing messages from {StringHelper.Join(dialogFiles, ";")}");
                if (!ParseCommandXml(dialogFiles, pairsLoc, pairsNonLoc, typeof(DisplayMessage), "/Messages/Message", "DisplayMessage.{0}.{1}", out var dialogIds))
                {
                    return 1;
                }

                var propsFile = (string)clo.GetValue("props", null);
                Console.WriteLine($"Writing localizable resources to {propsFile}");
                WritePairs(pairsLoc, propsFile, true);

                var propsNonLocFile = (string)clo.GetValue("propsnonloc", null);
                Console.WriteLine($"Writing non-localizable resources to {propsNonLocFile}");
                WritePairs(pairsNonLoc, propsNonLocFile, false);

                if (clo.IsArgPresent("cenum"))
                {
                    var cenum = (string)clo.GetValue("cenum", null);
                    Console.WriteLine($"Generating CommandId enum file {cenum}");

                    // commandId:    command name
                    // ribbonValues: command name --> resource id
                    commandIds.AddAll(ribbonIds);
                    if (!GenerateEnum(commandIds, "CommandId", cenum, null, ribbonValues))
                    {
                        return 1;
                    }
                }

                if (clo.IsArgPresent("denum"))
                {
                    var denum = (string)clo.GetValue("denum", null);
                    Console.WriteLine($"Generating MessageId enum file {denum}");
                    if (!GenerateEnum(dialogIds, "MessageId", denum, null, null))
                    {
                        return 1;
                    }
                }
            }

            if (clo.IsArgPresent("s"))
            {
                var pairs = new Hashtable();
                Console.WriteLine("Reading strings");
                foreach (string sPath in clo.GetValues("s"))
                {
                    using (var csvParser = new CsvParser(
                        new StreamReader(
                            new FileStream(
                                Path.GetFullPath(sPath),
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.ReadWrite),
                            Encoding.Default),
                        true))
                    {
                        foreach (var line in csvParser)
                        {
                            var value = line[1];
                            value = value.Replace($"{(char)8230}", "..."); // undo ellipses
                            var comment = (line.Length > 2) ? line[2] : string.Empty;
                            pairs.Add(line[0], new Values(value, comment));
                        }
                    }
                }

                if (clo.IsArgPresent("senum"))
                {
                    var senum = (string)clo.GetValue("senum", null);
                    Console.WriteLine($"Writing StringId enum file {senum}");
                    if (!GenerateEnum(new HashSet(pairs), "StringId", senum, pairs, null))
                    {
                        return 1;
                    }
                }

                if (clo.IsArgPresent("strings"))
                {
                    var stringsResx = (string)clo.GetValue("strings", null);
                    Console.WriteLine($"Writing {pairs.Count} localizable strings to {stringsResx}");
                    WritePairs(pairs, stringsResx, false);
                }
            }

            return 0;
        }

        /// <summary>
        /// Validates all.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private static bool ValidateAll()
        {
            var success = true;

            var basePath = Path.Combine(Environment.GetEnvironmentVariable("INETROOT"), @"client\writer\intl\lba\");
            foreach (var dir in Directory.GetDirectories(basePath))
            {
                var name = Path.GetFileName(dir);
                if (name != "default")
                {
                    if (name == "zh-chs")
                    {
                        name = "zh-cn";
                    }
                    else if (name == "zh-cht")
                    {
                        name = "zh-tw";
                    }

                    try
                    {
                        Thread.CurrentThread.CurrentUICulture = CultureHelper.GetBestCulture(name);
                        var errors = Res.Validate();
                        if (errors.Length > 0)
                        {
                            success = false;

                            Console.Error.WriteLine(name);
                            foreach (var err in errors)
                            {
                                Console.Error.WriteLine("\t" + err);
                            }

                            Console.Error.WriteLine();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.Error.WriteLine($"ERROR [{name}]: {e}");
                        Console.Error.WriteLine();
                        success = false;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Writes the pairs.
        /// </summary>
        /// <param name="pairs">The pairs.</param>
        /// <param name="path">The path.</param>
        /// <param name="includeFonts">if set to <c>true</c> [include fonts].</param>
        private static void WritePairs(Hashtable pairs, string path, bool includeFonts)
        {
            if (includeFonts)
            {
                pairs = (Hashtable)pairs.Clone();

                // HACK: hard-code invariant font resource values
                pairs.Add("Font", new Values("Segoe UI", "The font to be used to render all text in the product in Windows Vista and later. DO NOT specify more than one font!"));
                pairs.Add("Font.Size.Normal", new Values("9", "The default font size used throughout the product in Windows Vista and later"));
                pairs.Add("Font.Size.Large", new Values("10", "The size of titles in some error dialogs in Windows Vista and later"));
                pairs.Add("Font.Size.XLarge", new Values("11", "The size of titles in some error dialogs in Windows Vista and later.  Also used for the text that shows an error on the video publish place holder."));
                pairs.Add("Font.Size.XXLarge", new Values("12", "The size of panel titles in the Preferences dialog in Windows Vista and later.  Also the size of the text used to show the status on the video before publish."));
                pairs.Add("Font.Size.Heading", new Values("12", "The size of the header text in the Add Weblog wizard and Welcome wizard in Windows Vista and later"));
                pairs.Add("Font.Size.GiantHeading", new Values("15.75", "The size of the header text in the Add Weblog wizard and Welcome wizard in Windows Vista and later"));
                pairs.Add("Font.Size.ToolbarFormatButton", new Values("15", "The size of the font used to draw the edit toolbar's B(old), I(talic), U(nderline), and S(trikethrough) in Windows Vista and later. THIS SIZE IS IN PIXELS, NOT POINTS! Example: 14.75"));
                pairs.Add("Font.Size.PostSplitCaption", new Values("7", "The size of the font used to draw the 'More...' divider when using the Format | Split Post feature in Windows Vista and later."));
                pairs.Add("Font.Size.Small", new Values("7", "The size used for small messages, such as please respect copyright, in Windows Vista and later.  "));

                // HACK: hard-code default sidebar size
                pairs.Add("Sidebar.WidthInPixels", new Values("200", "The width of the sidebar, in pixels."));

                // HACK: hard-code wizard height
                pairs.Add("ConfigurationWizard.Height", new Values("380", "The height of the configuration wizard, in pixels."));
            }

            pairs.Add("Culture.UseItalics", new Values("True", "Whether or not the language uses italics"));

            var keys = new ArrayList(pairs.Keys);
            keys.Sort(new CaseInsensitiveComparer(CultureInfo.InvariantCulture));

            //using (TextWriter tw = new StreamWriter(path, false))
            var xmlBuffer = new StringBuilder();
            using (TextWriter tw = new StringWriter(xmlBuffer))
            {
                var writer = new ResXResourceWriter(tw);
                foreach (string key in keys)
                {
                    writer.AddResource(key, ((Values)pairs[key]).Val);
                }

                writer.Close();
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlBuffer.ToString());

            // Add auto-generation note
            xmlDoc.GetElementsByTagName("root")[0].PrependChild(xmlDoc.CreateComment(@"
    This file is automatically generated. DO NOT edit it manually.
    Edit the relevant XML or CSV files, and run LocUtil.
    A Batch file is provided in the repository root for easy regeneration of the strings tables."));

            foreach (XmlElement dataNode in xmlDoc.SelectNodes("/root/data"))
            {
                var name = dataNode.GetAttribute("name");
                if (pairs.ContainsKey(name))
                {
                    var comment = ((Values)pairs[name]).Comment;
                    if (comment != null && comment.Length > 0)
                    {
                        var commentEl = xmlDoc.CreateElement("comment");
                        var text = xmlDoc.CreateTextNode(comment);
                        commentEl.AppendChild(text);
                        dataNode.AppendChild(commentEl);
                    }
                }
            }

            // Correct the formatting as to not create needlessly large diffs
            var sb = new StringBuilder();
            var stringWriter = new Utf8StringWriter(sb);
            xmlDoc.Save(stringWriter);
            File.WriteAllText(path,
                sb.ToString()
                .Replace("  <comment>", "    <comment>") // Fix comment tag indent
                .Replace("</comment></data>", "</comment>\r\n  </data>"), // Move data close following comment close onto own line
                Encoding.UTF8);
        }

        // @RIBBON TODO: For now the union of the command in Commands.xml and Ribbon.xml will go into the CommandId enum.
        /// <summary>
        /// Generates the enum.
        /// </summary>
        /// <param name="commandIds">The command ids.</param>
        /// <param name="enumName">Name of the enum.</param>
        /// <param name="enumPath">The enum path.</param>
        /// <param name="descriptions">The descriptions.</param>
        /// <param name="values">The values.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="System.NotImplementedException">Inserting values and descriptions not supported presently</exception>
        private static bool GenerateEnum(HashSet commandIds, string enumName, string enumPath, Hashtable descriptions, Hashtable values)
        {
            const string TEMPLATE = @"// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//
// This file is automatically generated. DO NOT edit it manually.
// Edit the relevant XML or CSV files, and run LocUtil.
// A Batch file is provided in the repository root for easy regeneration of the strings tables.

namespace OpenLiveWriter.Localization
{{
    public enum {0}
    {{
        None,
        {1}
    }}
}}
";

            var commandList = commandIds.ToArrayList();
            commandList.Sort(new CaseInsensitiveComparer(CultureInfo.InvariantCulture));
            using (var sw = new StreamWriter(Path.GetFullPath(enumPath)))
            {
                if (descriptions == null && values == null)
                {
                    sw.Write(string.Format(CultureInfo.InvariantCulture, TEMPLATE, enumName, StringHelper.Join(commandList.ToArray(), ",\r\n\t\t")));
                }
                else if (descriptions == null)
                {
                    // insert values
                    const string VALUE_TEMPLATE = "{0} = {1}";
                    const string VALUELESS_TEMPLATE = "{0}";
                    var pairs = new ArrayList();
                    var unmappedCommands = new ArrayList();
                    foreach (string command in commandList.ToArray())
                    {
                        if (values[command] is string value)
                        {
                            pairs.Add(string.Format(CultureInfo.InvariantCulture, VALUE_TEMPLATE, command, value));
                        }
                        else
                        {
                            var color = Console.ForegroundColor;
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Error.WriteLine($"ERROR: Command {command} is missing an Id (required for instrumentation)");
                            Console.Beep();
                            Console.ForegroundColor = color;
                            Console.Error.Flush();

                            // This command is not mapped to a ribbon command
                            // We'll keep track of these and put them at the end
                            unmappedCommands.Add(command);
                        }
                    }

                    if (unmappedCommands.Count > 0)
                    {
                        return false;
                    }

                    // Now add the commands that were not mapped to a value
                    var index = 1;
                    foreach (string command in unmappedCommands.ToArray())
                    {
                        if (index == 1)
                        {
                            pairs.Add(string.Format(CultureInfo.InvariantCulture, VALUE_TEMPLATE, command, index));
                        }
                        else
                        {
                            pairs.Add(string.Format(CultureInfo.InvariantCulture, VALUELESS_TEMPLATE, command));
                        }

                        index++;
                    }

                    sw.Write(string.Format(CultureInfo.InvariantCulture, TEMPLATE, enumName, StringHelper.Join(pairs.ToArray(), ",\r\n        ")));
                }
                else if (values == null)
                {
                    const string DESC_TEMPLATE = "/// <summary>\n        /// {0}\n        /// </summary>\n        {1}";
                    var descs = new ArrayList();
                    foreach (string command in commandList.ToArray())
                    {
                        var description = ((Values)descriptions[command]).Val as string;
                        description = description.Replace("\n", "\n        /// ").Replace("/// \r\n", "///\r\n");
                        descs.Add(string.Format(CultureInfo.InvariantCulture, DESC_TEMPLATE, description, command));
                    }

                    sw.Write(string.Format(CultureInfo.InvariantCulture, TEMPLATE, enumName, StringHelper.Join(descs.ToArray(), ",\r\n        ")));
                }
                else
                {
                    // insert values and descriptions
                    throw new NotImplementedException("Inserting values and descriptions not supported presently");
                }
            }

            return true;
        }

        /// <summary>
        /// Parses the ribbon XML.
        /// </summary>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="pairs">The pairs.</param>
        /// <param name="pairsNonLoc">The pairs non loc.</param>
        /// <param name="t">The t.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="KEY_FORMAT">The key format.</param>
        /// <param name="ids">The ids.</param>
        /// <param name="values">The values.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="ConfigurationErrorsException">
        /// File not found
        /// or
        /// or
        /// Duplicate command: " + symbol + " with id " + id
        /// or
        /// or
        /// Unexpected attribute: " + attr.Name
        /// </exception>
        private static bool ParseRibbonXml(
            string[] inputFiles,
            Hashtable pairs,
            Hashtable pairsNonLoc,
            Type t,
            string xpath,
            string KEY_FORMAT,
            out HashSet ids,
            out Hashtable values)
        {
            // Add to the proptable
            var propTable = new Hashtable();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propTable.Add(prop.Name, prop);
            }

            ids = new HashSet();
            values = new Hashtable();

            foreach (var relativeInputFile in inputFiles)
            {
                var inputFile = Path.GetFullPath(relativeInputFile);
                try
                {
                    if (!File.Exists(inputFile))
                    {
                        throw new ConfigurationErrorsException("File not found");
                    }

                    var nsm = new XmlNamespaceManager(new NameTable());
                    nsm.AddNamespace(RibbonMarkup.XPathPrefix, RibbonMarkup.NamespaceUri);

                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(inputFile);

                    foreach (XmlElement el in xmlDoc.SelectNodes(xpath, nsm))
                    {

                        var id = el.GetAttribute("Id");
                        var symbol = el.GetAttribute("Symbol");
                        if (id == "")
                        {
                            throw new ConfigurationErrorsException(
                                string.Format(CultureInfo.CurrentCulture, "The following command is missing an identifier:\r\n{0}", el.OuterXml));
                        }

                        // RibbonDUI.js requires that command names begin with the prefix "cmd".
                        // We will strip that prefix when generating the CommandId enum.
                        var cmdIndex = symbol.IndexOf("cmd");
                        if (cmdIndex >= 0)
                        {
                            symbol = symbol.Substring(cmdIndex + 3);
                        }

                        if (!ids.Add(symbol))
                        {
                            throw new ConfigurationErrorsException($"Duplicate command: {symbol} with id {id}");
                        }

                        values.Add(symbol, id);

                        foreach (XmlAttribute attr in el.Attributes)
                        {
                            if (attr.NamespaceURI.Length != 0)
                            {
                                continue;
                            }

                            var name = attr.Name;

                            object val;
                            var thisProp = propTable[name] as PropertyInfo;
                            if (thisProp == null)
                            {
                                continue; // This attribute does not have a corresponding property in the type.
                            }

                            if (thisProp.PropertyType.IsPrimitive)
                            {
                                try
                                {
                                    val = Convert.ChangeType(attr.Value, thisProp.PropertyType).ToString();
                                }
                                catch (ArgumentException)
                                {
                                    throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Invalid attribute value: {0}=\"{1}\"", attr.Name, attr.Value));
                                }
                            }
                            else if (thisProp.PropertyType == typeof(string))
                            {
                                val = attr.Value;
                            }
                            else
                            {
                                throw new ConfigurationErrorsException($"Unexpected attribute: {attr.Name}");
                            }

                            var comment = GetComment(el, name);

                            var locAttr = thisProp.GetCustomAttributes(typeof(LocalizableAttribute), true);
                            var isNonLoc = locAttr.Length == 0 || !((LocalizableAttribute)locAttr[0]).IsLocalizable;
                            if (isNonLoc)
                            {
                                pairsNonLoc.Add(string.Format(CultureInfo.InvariantCulture, KEY_FORMAT, symbol, name), new Values(val, comment));
                            }
                            else
                            {
                                pairs.Add(string.Format(CultureInfo.InvariantCulture, KEY_FORMAT, symbol, name), new Values(val, comment));
                            }
                        }
                    }
                }
                catch (ConfigurationErrorsException ce)
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error in file {0}: {1}", inputFile, ce.Message));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Parses the command XML.
        /// </summary>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="pairs">The pairs.</param>
        /// <param name="pairsNonLoc">The pairs non loc.</param>
        /// <param name="t">The t.</param>
        /// <param name="xpath">The xpath.</param>
        /// <param name="KEY_FORMAT">The key format.</param>
        /// <param name="ids">The ids.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        /// <exception cref="ConfigurationErrorsException">
        /// File not found
        /// or
        /// or
        /// Duplicate command identifier: " + id
        /// or
        /// or
        /// or
        /// or
        /// Unexpected attribute: " + attr.Name
        /// or
        /// Duplicate main menu definition detected
        /// </exception>
        private static bool ParseCommandXml(
            string[] inputFiles,
            Hashtable pairs,
            Hashtable pairsNonLoc,
            Type t,
            string xpath,
            string KEY_FORMAT,
            out HashSet ids)
        {
            var seenMenu = false;

            var propTable = new Hashtable();
            foreach (var prop in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                propTable.Add(prop.Name, prop);
            }

            ids = new HashSet();

            foreach (var relativeInputFile in inputFiles)
            {
                var inputFile = Path.GetFullPath(relativeInputFile);
                try
                {
                    if (!File.Exists(inputFile))
                    {
                        throw new ConfigurationErrorsException("File not found");
                    }

                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(inputFile);
                    foreach (XmlElement el in xmlDoc.SelectNodes(xpath))
                    {
                        var id = el.GetAttribute("Identifier");
                        if (id == "")
                        {
                            throw new ConfigurationErrorsException(
                                string.Format(CultureInfo.CurrentCulture, "The following command is missing an identifier:\r\n{0}", el.OuterXml));
                        }

                        if (!ids.Add(id))
                        {
                            throw new ConfigurationErrorsException($"Duplicate command identifier: {id}");
                        }

                        foreach (XmlAttribute attr in el.Attributes)
                        {
                            if (attr.NamespaceURI.Length != 0)
                            {
                                continue;
                            }

                            var name = attr.Name;

                            if (name == "DebugOnly" || name == "Identifier")
                            {
                                continue;
                            }

                            object val;
                            var thisProp = propTable[name] as PropertyInfo;
                            if (thisProp == null)
                            {
                                throw new ConfigurationErrorsException(string.Format(CultureInfo.CurrentCulture, "Attribute {0} does not have a corresponding property", name));
                            }

                            if (thisProp.PropertyType.IsEnum)
                            {
                                try
                                {
                                    val = Enum.Parse(thisProp.PropertyType, attr.Value, false).ToString();
                                }
                                catch (ArgumentException)
                                {
                                    throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Invalid attribute value: {0}=\"{1}\"", attr.Name, attr.Value));
                                }
                            }
                            else if (thisProp.PropertyType.IsPrimitive)
                            {
                                try
                                {
                                    val = Convert.ChangeType(attr.Value, thisProp.PropertyType).ToString();
                                }
                                catch (ArgumentException)
                                {
                                    throw new ConfigurationErrorsException(string.Format(CultureInfo.InvariantCulture, "Invalid attribute value: {0}=\"{1}\"", attr.Name, attr.Value));
                                }
                            }
                            else if (thisProp.PropertyType == typeof(string))
                            {
                                val = attr.Value;
                            }
                            else
                            {
                                throw new ConfigurationErrorsException($"Unexpected attribute: {attr.Name}");
                            }

                            var comment = GetComment(el, name);

                            var locAttr = thisProp.GetCustomAttributes(typeof(LocalizableAttribute), true);
                            var isNonLoc = locAttr.Length == 0 || !((LocalizableAttribute)locAttr[0]).IsLocalizable;
                            if (isNonLoc)
                            {
                                pairsNonLoc.Add(string.Format(CultureInfo.InvariantCulture, KEY_FORMAT, id, name), new Values(val, comment));
                            }
                            else
                            {
                                pairs.Add(string.Format(CultureInfo.InvariantCulture, KEY_FORMAT, id, name), new Values(val, comment));
                            }
                        }
                    }

                    foreach (XmlElement mainMenuEl in xmlDoc.SelectNodes("/Commands/MainMenu"))
                    {
                        if (seenMenu)
                        {
                            throw new ConfigurationErrorsException("Duplicate main menu definition detected");
                        }

                        seenMenu = true;

                        Console.WriteLine("Parsing main menu definition");
                        var menuStructure = new StringBuilder();
                        BuildMenuString(menuStructure, mainMenuEl, ids, pairs);
                        pairsNonLoc.Add("MainMenuStructure", new Values(menuStructure.ToString(), ""));
                    }
                }
                catch (ConfigurationErrorsException ce)
                {
                    Console.Error.WriteLine(string.Format(CultureInfo.InvariantCulture, "Error in file {0}: {1}", inputFile, ce.Message));
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a comment for the specified property.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="property">The property.</param>
        /// <returns>System.String.</returns>
        private static string GetComment(XmlElement element, string property)
        {
            // Legacy XML files use a special namespace for comments (e.g. <Command MenuText="..." comment:MenuText="..." />)
            var comment = element.GetAttribute(property, CommentNamespace);

            // Ribbon XML files use a Comment attribute (e.g. <Command Comment=".." />)
            if (string.IsNullOrEmpty(comment) && element.HasAttribute("Comment"))
            {
                comment = element.GetAttribute("Comment");
            }

            return comment;
        }

        /// <summary>
        /// Builds the menu string.
        /// </summary>
        /// <param name="structure">The structure.</param>
        /// <param name="el">The el.</param>
        /// <param name="commandIds">The command ids.</param>
        /// <param name="pairs">The pairs.</param>
        /// <exception cref="ConfigurationErrorsException">
        /// Main menu definition uses unknown command id: " + id
        /// or
        /// Menu with id '" + id + "' was missing the required Text attribute
        /// or
        /// Unexpected element " + childEl.Name
        /// </exception>
        private static void BuildMenuString(StringBuilder structure, XmlElement el, HashSet commandIds, Hashtable pairs)
        {
            ////var startLen = structure.Length;
            var pos = 0;
            var lastWasSeparator = false;
            foreach (XmlNode childNode in el.ChildNodes)
            {
                if (!(childNode is XmlElement childEl))
                {
                    continue;
                }

                if (childEl.HasAttribute("Position"))
                {
                    pos = int.Parse(childEl.GetAttribute("Position"), CultureInfo.InvariantCulture);
                }

                var separator = string.Empty;
                if (childEl.Name == "Separator")
                {
                    lastWasSeparator = true;
                    continue;
                }
                else if (lastWasSeparator)
                {
                    separator = "-";
                    lastWasSeparator = false;
                }

                if (childEl.Name == "Menu" || childEl.Name == "Command")
                {
                    if (!childEl.HasAttribute("Identifier"))
                    {
                        throw new ConfigurationErrorsException($"{childEl.Name} element was missing required attribute 'Identifier'");
                    }

                    var id = childEl.GetAttribute("Identifier");

                    if (childEl.Name == "Command")
                    {
                        if (!commandIds.Contains(id))
                        {
                            throw new ConfigurationErrorsException($"Main menu definition uses unknown command id: {id}");
                        }
                    }

                    var idWithOrder = string.Format(CultureInfo.InvariantCulture, "{0}{1}@{2}", separator, id, (pos++ * 10));

                    if (structure.Length != 0)
                    {
                        structure.Append(" ");
                    }

                    switch (childEl.Name)
                    {
                        case "Menu":
                            if (!childEl.HasAttribute("Text"))
                            {
                                throw new ConfigurationErrorsException($"Menu with id '{id}' was missing the required Text attribute");
                            }

                            var menuText = childEl.GetAttribute("Text");
                            pairs.Add($"MainMenu.{id}", new Values(menuText, string.Empty));

                            structure.Append($"({idWithOrder}");
                            BuildMenuString(structure, childEl, commandIds, pairs);
                            structure.Append(")");
                            break;
                        case "Command":
                            structure.Append(idWithOrder);
                            break;
                    }
                }
                else
                {
                    throw new ConfigurationErrorsException($"Unexpected element {childEl.Name}");
                }
            }
        }
    }
}
