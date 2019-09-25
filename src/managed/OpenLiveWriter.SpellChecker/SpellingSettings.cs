// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

/*
TODO:
Should we pass the language code?
Be robust against the selected or default language not being present in the list of dictionaries
Include tech.tlx for everyone?
*/

namespace OpenLiveWriter.SpellChecker
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using CoreServices;
    using CoreServices.Settings;

    /// <summary>
    /// The SpellingSettings class.
    /// </summary>
    public class SpellingSettings
    {
        /// <summary>
        /// The real time spell checking
        /// </summary>
        private const string REAL_TIME_SPELL_CHECKING = "RealTimeSpellChecking";

        /// <summary>
        /// The check spelling before publish
        /// </summary>
        private const string CHECK_SPELLING_BEFORE_PUBLISH = "CheckSpellingBeforePublish";

        /// <summary>
        /// The check spelling before publish default
        /// </summary>
        private const bool CheckSpellingBeforePublishDefault = false;

        /// <summary>
        /// The ignore uppercase
        /// </summary>
        private const string IGNORE_UPPERCASE = "IgnoreUppercase";

        /// <summary>
        /// The ignore uppercase default
        /// </summary>
        private const bool IgnoreUppercaseDefault = true;

        /// <summary>
        /// The ignore numbers
        /// </summary>
        private const string IgnoreNumbers = "IgnoreNumbers";

        /// <summary>
        /// The ignore numbers default
        /// </summary>
        private const bool IgnoreNumbersDefault = true;

        /// <summary>
        /// The autocorrect
        /// </summary>
        private const string Autocorrect = "AutoCorrectEnabled";

        /// <summary>
        /// The autocorrect default
        /// </summary>
        private const bool AutocorrectDefault = true;

        /// <summary>
        /// The language
        /// </summary>
        private const string LANGUAGE = "SpellingLanguage";

        /// <summary>
        /// The settings key
        /// </summary>
        internal static SettingsPersisterHelper SettingsKey =
            ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings("PostEditor");

        /// <summary>
        /// The spelling key
        /// </summary>
        public static SettingsPersisterHelper SpellingKey = SpellingSettings.SettingsKey.GetSubSettings("Spelling");

        /// <summary>
        /// Run real time spell checker (squiggles)
        /// </summary>
        /// <value><c>true</c> if [real time spell checking]; otherwise, <c>false</c>.</value>
        public static bool RealTimeSpellChecking
        {
            get
            {
                // Just doing this GetString to see if the value is present.
                // There isn't a SpellingKey.HasValue().
                if (SpellingSettings.SpellingKey.HasValue(SpellingSettings.REAL_TIME_SPELL_CHECKING))
                {
                    return SpellingSettings.SpellingKey.GetBoolean(SpellingSettings.REAL_TIME_SPELL_CHECKING, true);
                }

                // This is GetBoolean rather than just "return RealTimeSpellCheckingDefault"
                // to ensure that the default gets written to the registry.
                return SpellingSettings.SpellingKey.GetBoolean(SpellingSettings.REAL_TIME_SPELL_CHECKING, true);
            }
            set => SpellingSettings.SpellingKey.SetBoolean(SpellingSettings.REAL_TIME_SPELL_CHECKING, value);
        }

        /// <summary>
        /// Run spelling form before publishing
        /// </summary>
        /// <value><c>true</c> if [check spelling before publish]; otherwise, <c>false</c>.</value>
        public static bool CheckSpellingBeforePublish
        {
            get => SpellingSettings.SpellingKey.GetBoolean(
                SpellingSettings.CHECK_SPELLING_BEFORE_PUBLISH,
                SpellingSettings.CheckSpellingBeforePublishDefault);
            set => SpellingSettings.SpellingKey.SetBoolean(
                SpellingSettings.CHECK_SPELLING_BEFORE_PUBLISH,
                value);
        }

        /// <summary>
        /// Ignore words in upper-case
        /// </summary>
        /// <value><c>true</c> if [ignore uppercase]; otherwise, <c>false</c>.</value>
        public static bool IgnoreUppercase
        {
            get => SpellingSettings.SpellingKey.GetBoolean(SpellingSettings.IGNORE_UPPERCASE,
                                                           SpellingSettings.IgnoreUppercaseDefault);
            set => SpellingSettings.SpellingKey.SetBoolean(SpellingSettings.IGNORE_UPPERCASE, value);
        }

        /// <summary>
        /// Ignore words with numbers
        /// </summary>
        /// <value><c>true</c> if [ignore words with numbers]; otherwise, <c>false</c>.</value>
        public static bool IgnoreWordsWithNumbers
        {
            get => SpellingSettings.SpellingKey.GetBoolean(SpellingSettings.IgnoreNumbers,
                                                           SpellingSettings.IgnoreNumbersDefault);
            set => SpellingSettings.SpellingKey.SetBoolean(SpellingSettings.IgnoreNumbers, value);
        }

        /// <summary>
        /// Enable AutoCorrect
        /// </summary>
        /// <value><c>true</c> if [enable automatic correct]; otherwise, <c>false</c>.</value>
        public static bool EnableAutoCorrect
        {
            get => SpellingSettings.SpellingKey.GetBoolean(SpellingSettings.Autocorrect,
                                                           SpellingSettings.AutocorrectDefault);
            set => SpellingSettings.SpellingKey.SetBoolean(SpellingSettings.Autocorrect, value);
        }

        /// <summary>
        /// Main language for spell checking
        /// </summary>
        /// <value>The language.</value>
        public static string Language
        {
            get
            {
                // Check if the language specified in the registry has dictionaries installed.
                // If so, use it. If not, check if the default language has dictionaries
                // installed. If so, use it. If not, use the English-US.

                // CurrentUICulture is currently forced into en-US which is unlikely to be the preferred dictionary
                // CurrentCulture is not modified so is more likely to be correct
                var defaultLanguage = CultureInfo.CurrentCulture.Name;
                string preferredLanguage;
                try
                {
                    preferredLanguage =
                        SpellingSettings.SpellingKey.GetString(SpellingSettings.LANGUAGE, defaultLanguage);
                }
                catch (Exception e)
                {
                    Trace.Fail(e.ToString());
                    preferredLanguage = defaultLanguage;
                }

                if (string.IsNullOrEmpty(preferredLanguage))
                {
                    return string.Empty;
                }

                if (WinSpellingChecker.IsLanguageSupported(preferredLanguage))
                {
                    return preferredLanguage;
                }

                // Language in registry is not installed!
                Trace.WriteLine("Dictionary language specified in registry is not installed. Using fallback");

                if (WinSpellingChecker.IsLanguageSupported(defaultLanguage))
                {
                    SpellingSettings.Language = defaultLanguage;
                    return defaultLanguage;
                }

                if (WinSpellingChecker.IsLanguageSupported("en-US"))
                {
                    SpellingSettings.Language = "en-US";
                    return "en-US";
                }

                return string.Empty;
            }

            set => SpellingSettings.SpellingKey.SetString(SpellingSettings.LANGUAGE, value);
        }

        /// <summary>
        /// Occurs when [spelling settings changed].
        /// </summary>
        public static event EventHandler SpellingSettingsChanged;

        /// <summary>
        /// Fires the changed event.
        /// </summary>
        public static void FireChangedEvent() => SpellingSettings.SpellingSettingsChanged?.Invoke(null, new EventArgs());

        /// <summary>
        /// Languages supported by the sentry spelling checker
        /// </summary>
        /// <returns>SpellingLanguageEntry[].</returns>
        public static SpellingLanguageEntry[] GetInstalledLanguages()
        {
            var list = new List<SpellingLanguageEntry>();

            foreach (var entry in WinSpellingChecker.GetInstalledLanguages())
            {
                try
                {
                    var entryCulture = CultureInfo.GetCultureInfo(entry);

                    list.Add(new SpellingLanguageEntry(entry, entryCulture.DisplayName));
                }
                catch
                {
                    list.Add(new SpellingLanguageEntry(entry, entry));
                }
            }

            return list.ToArray();
        }
    }
}
