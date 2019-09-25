// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using ApplicationFramework.Preferences;

    /// <summary>
    /// Interface used for specifying spelling options
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.Preferences.Preferences" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.Preferences.Preferences" />
    public class SpellingPreferences : Preferences
    {
        /// <summary>
        /// The FlagsPreferences sub-key.
        /// </summary>
        private const string PreferencesSubKey = "Spelling";

        /// <summary>
        /// The check spelling before publish
        /// </summary>
        private bool checkSpellingBeforePublish;

        /// <summary>
        /// The enable automatic correct
        /// </summary>
        private bool enableAutoCorrect;

        /// <summary>
        /// The language
        /// </summary>
        private string language;

        /// <summary>
        /// The real time spell checking
        /// </summary>
        private bool realTimeSpellChecking;

        /// <summary>
        /// Initializes a new instance of the SpellingPreferences class.
        /// </summary>
        public SpellingPreferences()
            : base(SpellingPreferences.PreferencesSubKey)
        {
        }

        /// <summary>
        /// Run the spelling form before publishing
        /// </summary>
        /// <value><c>true</c> if [check spelling before publish]; otherwise, <c>false</c>.</value>
        public bool CheckSpellingBeforePublish
        {
            get => this.checkSpellingBeforePublish;
            set
            {
                this.checkSpellingBeforePublish = value;
                this.Modified();
            }
        }

        /// <summary>
        /// Run real time spell checking
        /// </summary>
        /// <value><c>true</c> if [real time spell checking]; otherwise, <c>false</c>.</value>
        public bool RealTimeSpellChecking
        {
            get => this.realTimeSpellChecking;
            set
            {
                this.realTimeSpellChecking = value;
                this.Modified();
            }
        }

        /// <summary>
        /// Main language for spell checking
        /// </summary>
        /// <value>The language.</value>
        public string Language
        {
            get => this.language;
            set
            {
                this.language = value;
                this.Modified();
            }
        }

        /// <summary>
        /// Enables/Disables AutoCorrect
        /// </summary>
        /// <value><c>true</c> if [enable automatic correct]; otherwise, <c>false</c>.</value>
        public bool EnableAutoCorrect
        {
            get => this.enableAutoCorrect;
            set
            {
                this.enableAutoCorrect = value;
                this.Modified();
            }
        }

        /// <summary>
        /// Load preference values
        /// </summary>
        protected override void LoadPreferences()
        {
            this.realTimeSpellChecking = SpellingSettings.RealTimeSpellChecking;
            this.checkSpellingBeforePublish = SpellingSettings.CheckSpellingBeforePublish;
            this.enableAutoCorrect = SpellingSettings.EnableAutoCorrect;
            this.language = SpellingSettings.Language;
        }

        /// <summary>
        /// Save preference values
        /// </summary>
        protected override void SavePreferences()
        {
            SpellingSettings.RealTimeSpellChecking = this.realTimeSpellChecking;
            SpellingSettings.CheckSpellingBeforePublish = this.checkSpellingBeforePublish;
            SpellingSettings.EnableAutoCorrect = this.enableAutoCorrect;
            SpellingSettings.Language = this.language;
            SpellingSettings.FireChangedEvent();
        }
    }
}
