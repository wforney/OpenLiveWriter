// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Forms;
    using ApplicationFramework.Preferences;
    using Autoreplace;
    using Configuration.Accounts;
    using CoreServices;
    using CoreServices.Diagnostics;
    using HtmlEditor.Linking;
    using LiveClipboard;
    using Localization;
    using SpellChecker;

    /// <summary>
    /// Class for handling preferences
    /// </summary>
    public class PreferencesHandler
    {
        /// <summary>
        /// The array of preferences panel types.
        /// </summary>
        private static Type[] preferencesPanelTypes;

        /// <summary>
        /// The name to preferences panel type table.
        /// </summary>
        private static Hashtable preferencesPanelTypeTable;

        /// <summary>
        /// The instance
        /// </summary>
        private static PreferencesHandler instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public static PreferencesHandler Instance => PreferencesHandler.instance ?? (PreferencesHandler.instance = new PreferencesHandler());

        /// <summary>
        /// A version of show preferences that allows the caller to specify which
        /// panel should be selected when the dialog opens
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="selectedPanelName">The selected panel name.</param>
        public void ShowPreferences(IWin32Window owner, string selectedPanelName)
        {
            PreferencesHandler.LoadPreferencesPanels();
            Type type;
            if (string.IsNullOrEmpty(selectedPanelName))
            {
                type = null;
            }
            else
            {
                type = (Type) PreferencesHandler.preferencesPanelTypeTable[
                    selectedPanelName.ToLower(CultureInfo.InvariantCulture)];
            }

            this.ShowPreferences(owner, null, type);
        }

        /// <summary>
        /// A version of show preferences that allows the caller to specify which
        /// panel should be selected when the dialog opens
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="editingSite">The editing site.</param>
        /// <param name="selectedPanelType">Type of the selected panel.</param>
        public void ShowPreferences(IWin32Window owner, IBlogPostEditingSite editingSite, Type selectedPanelType)
        {
            //	Load preferences panels.
            PreferencesHandler.LoadPreferencesPanels();

            //	Show the preferences form.
            using (new WaitCursor())
            using (var preferencesForm = new PreferencesForm())
            {
                //	Set the PreferencesPanel entries.
                for (var i = 0; i < PreferencesHandler.preferencesPanelTypes.Length; i++)
                {
                    //	Add the entry.
                    var type = PreferencesHandler.preferencesPanelTypes[i];
                    var panel = Activator.CreateInstance(type) as PreferencesPanel;
                    if (editingSite != null && panel is IBlogPostEditingSitePreferences blogPostEditingSitePreferences)
                    {
                        blogPostEditingSitePreferences.EditingSite = editingSite;
                    }

                    preferencesForm.SetEntry(i, panel);

                    //	Select it, if requested.
                    if (type == selectedPanelType)
                    {
                        preferencesForm.SelectedIndex = i;
                    }
                }

                //	Provide a default selected index if none was specified.
                if (preferencesForm.SelectedIndex == -1)
                {
                    preferencesForm.SelectedIndex = 0;
                }

                //	Show the form.
                preferencesForm.Win32Owner = owner;
                preferencesForm.ShowDialog(owner);

                // if we have an editing site then let it know that the account
                // list may have been edited (allows it to adapt to the currently
                // active weblog being deleted)
                editingSite?.NotifyWeblogAccountListEdited();
            }
        }

        /// <summary>
        /// Shows the web proxy preferences.
        /// </summary>
        /// <param name="owner">The owner.</param>
        public void ShowWebProxyPreferences(IWin32Window owner)
        {
            //	Show the preferences form.
            using (new WaitCursor())
            using (var preferencesForm = new PreferencesForm())
            {
                preferencesForm.Text = Res.Get(StringId.ProxyPrefTitle);
                preferencesForm.SetEntry(0, new WebProxyPreferencesPanel());
                preferencesForm.SelectedIndex = 0;
                preferencesForm.Win32Owner = owner;
                preferencesForm.ShowDialog(owner);
            }
        }

        /// <summary>
        /// Helper to load the preferences panels.
        /// </summary>
        private static void LoadPreferencesPanels()
        {
            //	If preferences panels have been loaded already, return.
            if (PreferencesHandler.preferencesPanelTypes != null)
            {
                return;
            }

            var types = new List<Type>();
            PreferencesHandler.preferencesPanelTypeTable = new Hashtable();

            //	Writer Preferences
            var type = typeof(PostEditorPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["preferences"] = type;
            types.Add(type);

            type = typeof(EditingPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["editing"] = type;
            types.Add(type);

            type = typeof(WeblogAccountPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["accounts"] = type;
            types.Add(type);

            // Spelling preferences.
            type = typeof(SpellingPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["spelling"] = type;
            types.Add(type);

            //glossary management
            type = typeof(GlossaryPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["glossary"] = type;
            types.Add(type);

            type = typeof(AutoreplacePreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["autoreplace"] = type;

            //types.Add(type);

            if (ApplicationDiagnostics.TestMode || LiveClipboardManager.LiveClipboardFormatHandlers.Length > 0)
            {
                type = typeof(LiveClipboardPreferencesPanel);
                PreferencesHandler.preferencesPanelTypeTable["liveclipboard"] = type;
                types.Add(type);
            }

            // Plugin preferences
            type = typeof(PluginsPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["plugins"] = type;
            types.Add(type);

            //	WebProxy preferences
            type = typeof(WebProxyPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["webproxy"] = type;
            types.Add(type);

            type = typeof(PingPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["pings"] = type;
            types.Add(type);

            type = typeof(PrivacyPreferencesPanel);
            PreferencesHandler.preferencesPanelTypeTable["privacy"] = type;
            types.Add(type);

            //	Set the preferences panels type array.
            PreferencesHandler.preferencesPanelTypes = types.ToArray();
        }
    }
}
