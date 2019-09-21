// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Resources;
    using System.Text;
    using OpenLiveWriter.Localization;

    public partial class CommandResourceLoader
    {
        /// <summary>
        /// Parses the structure of the main menu into a hashtable whose keys are
        /// command identifier strings and values are localized MainMenuPath strings.
        /// The structure is stored as a string in PropertiesNonLoc.resx under
        /// the name "MainMenuStructure". Here's an example of the format:
        /// (File@0 NewPost@0 NewPage@1 OpenPost@2 -SavePost@3 PostAsDraft@4 PostAsDraftAndEditOnline@5 -DeleteDraft@6 -PostAndPublish@7 -Close@8) (Edit@1 Undo@0 Redo@1 -Cut@2 Copy@3 Paste@4 PasteSpecial@5 -Clear@6 -SelectAll@7 Find@8) (View@2 ViewNormal@0 ViewWebLayout@1 ViewPreview@2 ViewCode@3 -UpdateWeblogStyle@4 -ShowMenu@5 ViewSidebar@6 PostProperties@7) (Insert@3 InsertLink@0 InsertPicture@1 -InsertTable2@2) (Format@4 Font@0 FontColor@1 (-Align@2 AlignLeft@0 AlignCenter@1 AlignRight@2) -Numbers@3 Bullets@4 Blockquote@5 -InsertExtendedEntry@6) (Table@5 InsertTable@0 -TableProperties@1 RowProperties@2 ColumnProperties@3 CellProperties@4 -InsertRowAbove@5 InsertRowBelow@6 MoveRowUp@7 MoveRowDown@8 -InsertColumnLeft@9 InsertColumnRight@10 MoveColumnLeft@11 MoveColumnRight@12 -DeleteTable@13 DeleteRow@14 DeleteColumn@15 -ClearCell@16) (Tools@6 CheckSpelling@0 -Preferences@1 -DiagnosticsConsole@2 ForceWatson@3 BlogClientOptions@4 ShowDisplayMessageTestForm@5) (Weblog@7 ViewWeblog@0 ViewWeblogAdmin@1 -ConfigureWeblog@2 -AddWeblog@9999) (Help@8 Help@0 -SendFeedback@1 -About@2)
        /// Each menu or sub-menu is surrounded by parentheses. The first
        /// item after the open-parenthesis is the title of that menu;
        /// the remaining items are the commands or (sub-menus) of the menu.
        /// The localized text for each menu/sub-menu title is in Properties.resx
        /// under the name "MainMenu.[menu]", for example "MainMenu.File". The localized
        /// text for each command is under the name "Command.[command].MenuText", for
        /// example "Command.NewPost.MenuText".
        /// </summary>
        private class MainMenuParser
        {
            /// <summary>
            /// The loc
            /// </summary>
            private readonly ResourceManager loc;

            /// <summary>
            /// The non loc
            /// </summary>
            private readonly ResourceManager nonLoc;

            /// <summary>
            /// Initializes a new instance of the <see cref="MainMenuParser"/> class.
            /// </summary>
            /// <param name="loc">The loc.</param>
            /// <param name="nonLoc">The non loc.</param>
            public MainMenuParser(ResourceManager loc, ResourceManager nonLoc)
            {
                this.loc = loc;
                this.nonLoc = nonLoc;
            }

            /// <summary>
            /// Returns a hashtable whose keys are command ID strings and
            /// whose values are localized MainMenuPath strings.
            /// </summary>
            /// <returns>Hashtable.</returns>
            public Hashtable Parse()
            {
                var data = this.nonLoc.GetString("MainMenuStructure");
                if (data == null)
                {
                    data = string.Empty;
                }

                var results = new Hashtable();
                this.Parse(new StringReader(data), results, string.Empty);
                return results;
            }

            /// <summary>
            /// Parse the body of a menu
            /// </summary>
            /// <param name="data">The data.</param>
            /// <param name="results">The results.</param>
            /// <param name="prefix">The prefix.</param>
            private void Parse(StringReader data, Hashtable results, string prefix)
            {
                int c;
                while (-1 != (c = data.Peek()))
                {
                    if (c == '(')
                    {
                        // sub-menu begins
                        data.Read(); // eat the (

                        var menuPath = this.ReadNextEntry(true, prefix, data, out _);
                        this.Parse(data, results, menuPath);
                    }
                    else if (c == ')')
                    {
                        // this menu ends
                        data.Read();
                        if (data.Peek() == ' ')
                        {
                            data.Read();
                        }

                        return;
                    }
                    else
                    {
                        var commandMenuPath = this.ReadNextEntry(false, prefix, data, out var commandId);
                        results.Add(commandId, commandMenuPath);
                    }
                }
            }

            /// <summary>
            /// Reads the next entry.
            /// </summary>
            /// <param name="isMenu">if set to <c>true</c> [is menu].</param>
            /// <param name="prefix">The prefix.</param>
            /// <param name="data">The data.</param>
            /// <param name="id">The identifier.</param>
            /// <returns>System.String.</returns>
            /// <exception cref="System.ArgumentException">
            /// Malformed menu structure
            /// or
            /// No loc string for " + id
            /// </exception>
            private string ReadNextEntry(bool isMenu, string prefix, StringReader data, out string id)
            {
                var chunk = new StringBuilder();
                var hasSeparator = false;
                id = null;
                int c;
                while (-1 != (c = data.Peek()))
                {
                    if (c == ')')
                    {
                        break;
                    }

                    data.Read();

                    if (c == ' ')
                    {
                        break;
                    }

                    if (c == '@')
                    {
                        id = chunk.ToString();
                        chunk = new StringBuilder();
                        continue;
                    }

                    if (c == '-' && id == null && chunk.Length == 0)
                    {
                        hasSeparator = true;
                        continue;
                    }

                    chunk.Append((char)c);
                }

                if (id == null)
                {
                    throw new ArgumentException("Malformed menu structure");
                }

                var position = int.Parse(chunk.ToString(), CultureInfo.InvariantCulture);

                var locString = this.loc.GetString(string.Format(CultureInfo.InvariantCulture, isMenu ? "MainMenu.{0}" : "Command.{0}.MenuText", id));

                if (Res.DebugMode)
                {
                    locString = string.Format(CultureInfo.InvariantCulture, isMenu ? "MainMenu.{0}" : "Command.{0}.MenuText", id);
                }

                if (locString == null)
                {
                    throw new ArgumentException($"No loc string for {id}");
                }

                return
                    (prefix.Length == 0 ? string.Empty : prefix + "/")
                    + (hasSeparator ? "-" : string.Empty)
                    + locString
                    + "@"
                    + position.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
