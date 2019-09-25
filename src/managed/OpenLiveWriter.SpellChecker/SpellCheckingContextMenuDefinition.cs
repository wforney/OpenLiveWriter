// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.SpellChecker
{
    using ApplicationFramework;
    using Localization;

    /// <summary>
    /// Summary description for SpellCheckingContextMenuDefinition.
    /// 1. list of word suggestions
    /// 2. static menu with ignore all, add to dictionary (which take word as argument)
    /// 3. launch spelling dialog ??
    /// 4. static menu with cut/copy/paste
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.CommandContextMenuDefinition" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.CommandContextMenuDefinition" />
    public class SpellCheckingContextMenuDefinition : CommandContextMenuDefinition
    {
        /// <summary>
        /// Default maximum suggestions to return
        /// </summary>
        private const short DefaultMaxSuggestions = 10;

        /// <summary>
        /// If we detect a gap between scores of this value or greater then
        /// we drop the score and all remaining
        /// </summary>
        private const short ScoreGapFilter = 20;

        /// <summary>
        /// Suggestion depth for searching (100 is the maximum)
        /// </summary>
        private const short SuggestionDepth = 80;

        /// <summary>
        /// The current word
        /// </summary>
        private readonly string currentWord;

        /// <summary>
        /// The spelling manager
        /// </summary>
        private readonly SpellingManager spellingManager;

        /// <inheritdoc />
        public SpellCheckingContextMenuDefinition() : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellCheckingContextMenuDefinition"/> class.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="manager">The manager.</param>
        public SpellCheckingContextMenuDefinition(string word, SpellingManager manager)
        {
            this.currentWord = word;
            this.spellingManager = manager;
            this.Entries.AddRange(this.GetSpellingSuggestions());
            this.Entries.Add(CommandId.IgnoreOnce, true, false);
            this.Entries.Add(CommandId.IgnoreAll, false, false);
            this.Entries.Add(CommandId.AddToDictionary, false, false);
            this.Entries.Add(CommandId.OpenSpellingForm, false, false);
        }

        /// <summary>
        /// Gets the spelling suggestions.
        /// </summary>
        /// <returns>MenuDefinitionEntryCollection.</returns>
        private MenuDefinitionEntryCollection GetSpellingSuggestions()
        {
            var commandManager = this.spellingManager.CommandManager;
            var listOfSuggestions = new MenuDefinitionEntryCollection();
            commandManager.SuppressEvents = true;
            commandManager.BeginUpdate();
            try
            {
                // provide suggestions
                var suggestions = this.spellingManager.SpellingChecker.Suggest(
                    this.currentWord,
                    SpellCheckingContextMenuDefinition.DefaultMaxSuggestions,
                    SpellCheckingContextMenuDefinition.SuggestionDepth);
                var foundSuggestion = false;
                if (suggestions.Length > 0)
                {
                    // add suggestions to list (stop adding when the quality of scores
                    // declines precipitously)
                    var lastScore = suggestions[0].Score;
                    for (var i = 0; i < suggestions.Length; i++)
                    {
                        var suggestion = suggestions[i];

                        //note: in some weird cases, like 's, a suggestion is returned but lacks a suggested replacement, so need to check that case
                        if (lastScore - suggestion.Score < SpellCheckingContextMenuDefinition.ScoreGapFilter &&
                            suggestion.Suggestion != null)
                        {
                            var fixSpellingCommand = new Command(CommandId.FixWordSpelling);
                            fixSpellingCommand.Identifier += suggestion.Suggestion;
                            fixSpellingCommand.Text = suggestion.Suggestion;
                            fixSpellingCommand.MenuText = suggestion.Suggestion;
                            fixSpellingCommand.Execute += this.spellingManager.fixSpellingApplyCommand_Execute;
                            fixSpellingCommand.Tag = suggestion.Suggestion;
                            commandManager.Add(fixSpellingCommand);

                            listOfSuggestions.Add(fixSpellingCommand.Identifier, false, i == suggestions.Length - 1);
                            foundSuggestion = true;
                        }
                        else
                        {
                            break;
                        }

                        // update last score
                        lastScore = suggestion.Score;
                    }
                }

                if (!foundSuggestion)
                {
                    var fixSpellingCommand = new Command(CommandId.FixWordSpelling);
                    fixSpellingCommand.Enabled = false;

                    commandManager.Add(fixSpellingCommand);
                    listOfSuggestions.Add(CommandId.FixWordSpelling, false, true);
                }
            }
            finally
            {
                commandManager.EndUpdate();
                commandManager.SuppressEvents = false;
            }

            return listOfSuggestions;
        }
    }
}
