// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using mshtml;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Diagnostics;
using OpenLiveWriter.HtmlEditor.Linking;
using OpenLiveWriter.Mshtml;
using OpenLiveWriter.PostEditor.Autoreplace;
using OpenLiveWriter.PostEditor.Emoticons;
using OpenLiveWriter.PostEditor.PostHtmlEditing;

namespace OpenLiveWriter.PostEditor
{

    /// <summary>
    /// This class / thread static event is here to allow the typographic handling to execute even
    /// when behaviors or others actually handle and suppress keys (for example,
    /// the table editing behavior
    /// handles enter and tab). Classes should call OnBeforeKeyHandled when they are going to use e.Cancel() to
    /// cancel the key event and prevent its propagation.
    /// </summary>
    class LastChanceKeyboardHook
    {
        public static void OnBeforeKeyHandled(object sender, KeyEventArgs e)
        {
            BeforeKeyHandled(sender, e);
        }

        [ThreadStatic]
        public static KeyEventHandler BeforeKeyHandled;
    }

    class PostEditorKeyboardHandler : IDisposable
    {
        private BlogPostHtmlEditorControl _blogPostHtmlEditorControl;
        private IBlogPostImageEditingContext _imageEditingContext;
        private IEditingMode _editingModeContext;

        public PostEditorKeyboardHandler(BlogPostHtmlEditorControl blogPostHtmlEditorControl, IBlogPostImageEditingContext imageEditingContext, IEditingMode editingModeContext)
        {
            _blogPostHtmlEditorControl = blogPostHtmlEditorControl;
            _blogPostHtmlEditorControl.PostEditorEvent += _blogPostHtmlEditorControl_PostEditorEvent;
            _blogPostHtmlEditorControl.KeyPress += _blogPostHtmlEditorControl_KeyPress;
            _blogPostHtmlEditorControl.KeyDown += _blogPostHtmlEditorControl_KeyDown;
            _blogPostHtmlEditorControl.SelectionChanged += _blogPostHtmlEditorControl_SelectionChanged;

            _imageEditingContext = imageEditingContext;
            _editingModeContext = editingModeContext;

            _autoreplaceManager = new AutoreplaceManager();
            AutoreplaceSettings.SettingsChanged += AutoreplaceSettings_SettingsChanged;

            LastChanceKeyboardHook.BeforeKeyHandled += _keyHandled;
        }

        public void SetAutoCorrectFile(string path)
        {
            _autoreplaceManager.SetAutoCorrectFile(path);
        }

        private void _blogPostHtmlEditorControl_PostEditorEvent(object sender, MshtmlEditor.EditDesignerEventArgs e)
        {
            if (e.EventDispId != DISPID_HTMLELEMENTEVENTS2.ONKEYPRESS)
                return;
            char c = Convert.ToChar(e.EventObj.keyCode);

            try
            {
                MaybeExecuteDelayedAutoReplaceOperation(c);
            }
            catch (Exception ex)
            {
                Trace.Fail("Error while handling posteditorevent, suppressing autocomplete " + ex);
                _delayedAutoReplaceAction = null;
                _haltAutoReplace = true;
                throw;
            }
        }

        private void MaybeExecuteDelayedAutoReplaceOperation(char c)
        {
            // The TCH's gravity must be set to right/left initially, to capture
            // the content generated by MSHTML's keystroke handling. But now it
            // must be snapped right, so that the _delayedAutoReplaceAction
            // replacement contents stay to the left of the TCH's range.
            if (_typographicCharacterHandler != null)
                _typographicCharacterHandler.SetGravityRight();

            if (_delayedAutoReplaceAction != null)
            {
                AutoReplaceAction temp = _delayedAutoReplaceAction;
                _delayedAutoReplaceAction = null;
                temp.Execute(c);
            }

            if (_typographicCharacterHandler != null)
            {
                TypographicCharacterHandler tHandler = _typographicCharacterHandler;
                _typographicCharacterHandler = null;
                if (tHandler.HandleTypographicReplace())
                    _lastActionWasReplace++;
            }
        }

        private bool _ignoreNextSelectionChange = false;
        void _blogPostHtmlEditorControl_SelectionChanged(object sender, EventArgs e)
        {
            // After an autoreplace operation occurs, if you immediately hit backspace,
            // that will cause an undo of the autoreplace operation. This causes a selection
            // changed event. In that particular case, we don't want to reset the _lastActionWasReplace
            // value. This is because it's possible for a single keystroke to cause both
            // an autoreplace operation and a typographic character replacement to occur.
            // For example, try typing "teh'" (minus the double quotes). The apostrophe
            // keystroke will cause both teh=>the and '=>’ at once, and hitting backspace
            // twice should cause first one then the other to be undone.

            if (_ignoreNextSelectionChange)
                _ignoreNextSelectionChange = false;
            else
                _lastActionWasReplace = 0;
        }

        void _keyHandled(object sender, KeyEventArgs e)
        {
            // This is special handling so that when the user tabs or presses enter in the title, we'll still try to
            // handle replaces
            if (e.KeyCode == Keys.Enter)
                MaybeHandleKey('\r');
            else if (e.KeyCode == Keys.Tab)
                MaybeHandleKey('\t');
        }

        private bool AutoLinkEnabled
        {
            get
            {
                return GlobalEditorOptions.SupportsFeature(ContentEditorFeature.AutoLinking) &&
                       GlossarySettings.AutoLinkEnabled;
            }
        }

        private bool AutoReplaceEnabled
        {
            get
            {
                return true;
            }
        }

        private bool TypographicReplacementEnabled
        {
            get
            {
                return _editingModeContext.CurrentEditingMode == EditingMode.Wysiwyg &&
                    AutoreplaceSettings.AnyReplaceEnabled;
            }
        }

        void _blogPostHtmlEditorControl_KeyDown(object o, HtmlEventArgs e)
        {
            try
            {
                using (ApplicationPerformance.LogEvent("PostEditorKeyDown"))
                {
                    if (HandleKey && Convert.ToChar(e.htmlEvt.keyCode) == 8)
                    {
                        _linkIgnoreWord = null;
                        if (AutoLinkEnabled)
                        {
                            MarkupPointer blockBoundary;
                            string htmlText = GetHtmlText(out blockBoundary);

                            if (blockBoundary == null)
                                return;

                            MatchUrl(htmlText, IgnoreSuggestedUrl);
                        }

                        if (_lastActionWasReplace > 0)
                        {
                            _lastActionWasReplace--;
                            _ignoreNextSelectionChange = true;
                            _blogPostHtmlEditorControl.Undo();
                            e.Cancel();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Error while handling backspace key, suppressing autocomplete " + ex);
                _haltAutoReplace = true;
                throw;
            }
        }

        void _blogPostHtmlEditorControl_KeyPress(object o, HtmlEventArgs e)
        {
            using (ApplicationPerformance.LogEvent("PostEditorKeyPress"))
            {
                // These are important for helping to find bugs with autoreplace, but cause to much
                // noise with tests and testers even in debug mode.  They can have many 'false positives' hits
                if (Debugger.IsAttached)
                {
                    Debug.Assert(_delayedAutoReplaceAction == null, "Delayed autoreplace operation wasn't null!");
                    Debug.Assert(_typographicCharacterHandler == null, "Delayed typographic character operation wasn't null!");
                }
                _delayedAutoReplaceAction = null;
                _typographicCharacterHandler = null;
                _lastActionWasReplace = 0;
                _ignoreNextSelectionChange = false;
                if (MaybeHandleKey(Convert.ToChar(e.htmlEvt.keyCode)))
                    e.Cancel();
            }
        }

        void AutoreplaceSettings_SettingsChanged(object sender, EventArgs e)
        {
            _autoreplaceManager.ReloadPhraseSettings();
        }

        private int _lastActionWasReplace = 0;

        public void Dispose()
        {
            if (_blogPostHtmlEditorControl != null)
            {
                _blogPostHtmlEditorControl.PostEditorEvent -= _blogPostHtmlEditorControl_PostEditorEvent;
                _blogPostHtmlEditorControl.KeyPress -= _blogPostHtmlEditorControl_KeyPress;
                _blogPostHtmlEditorControl.KeyDown -= _blogPostHtmlEditorControl_KeyDown;
                _blogPostHtmlEditorControl.SelectionChanged -= _blogPostHtmlEditorControl_SelectionChanged;
                _blogPostHtmlEditorControl = null;
            }
            LastChanceKeyboardHook.BeforeKeyHandled -= _keyHandled;

            AutoreplaceSettings.SettingsChanged -= AutoreplaceSettings_SettingsChanged;
        }

        public bool MaybeHandleInsert(char c, ThreadStart action)
        {
            bool handled = MaybeHandleKey(c);
            action();
            MaybeExecuteDelayedAutoReplaceOperation(c);
            return handled;
        }

        private bool MaybeHandleKey(char c)
        {
            if (_blogPostHtmlEditorControl.SelectionIsInvalid)
                return false;

            _delayedAutoReplaceAction = null;
            _typographicCharacterHandler = null;

            bool handled = false;
            try
            {
                bool whiteSpaceOrPunctuation = char.IsWhiteSpace(c) || char.IsPunctuation(c);
                if (HandleKey && (whiteSpaceOrPunctuation || EmoticonsManager.IsAutoReplaceEndingCharacter(c)))
                {
                    handled = DoHandleKey(c, whiteSpaceOrPunctuation);
                    if (handled && _delayedAutoReplaceAction != null)
                        MaybeExecuteDelayedAutoReplaceOperation(c);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail("Error while handling key, suppressing autocomplete " + ex);
                _delayedAutoReplaceAction = null;
                _typographicCharacterHandler = null;
                _haltAutoReplace = true;
                throw;
            }
            return handled;
        }

        private TypographicCharacterHandler _typographicCharacterHandler;

        private bool DoHandleKey(char c, bool whiteSpaceOrPunctuation)
        {
            MarkupPointer blockBoundary;
            string htmlText = GetHtmlText(out blockBoundary);

            if (blockBoundary == null)
                return false;

            // only do autoreplace after whitespace or punctuation
            if (whiteSpaceOrPunctuation && AutoReplaceEnabled)
                MaybeScheduleDelayedAutoreplace(htmlText, c);

            // handle typographic characters after any character
            if (TypographicReplacementEnabled)
                _typographicCharacterHandler = new TypographicCharacterHandler(_blogPostHtmlEditorControl.SelectedMarkupRange, _blogPostHtmlEditorControl.InsertHtml, _imageEditingContext, _blogPostHtmlEditorControl.PostBodyElement, c, htmlText, blockBoundary);

            // only do glossary replacements after whitespace or punctuation
            if (whiteSpaceOrPunctuation && AutoLinkEnabled)
                MatchUrl(htmlText, InsertSuggestedUrl);

            _linkIgnoreWord = null;
            return false;
        }

        private string _linkIgnoreWord = null;

        private bool HandleKey
        {
            get
            {
                return (!_haltAutoReplace &&
                    (AutoReplaceEnabled || AutoLinkEnabled || TypographicReplacementEnabled));
            }
        }

        private bool MaybeScheduleDelayedAutoreplace(string htmlText, char key)
        {
            if (_autoreplaceManager == null)
                return false;

            bool handled = false;
            int length;
            string replaceHtml = _autoreplaceManager.FindMatch(htmlText, out length);
            if (replaceHtml != null && length > 0 && htmlText != replaceHtml)
            {
                if (!ShouldAutoCorrect(htmlText, key, replaceHtml))
                    return false;

                MarkupRange insertMarkupRange = _blogPostHtmlEditorControl.SelectedMarkupRange.Clone();
                for (int i = 0; i < length; i++)
                    insertMarkupRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);

                insertMarkupRange.Start.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Right;
                insertMarkupRange.End.Gravity = _POINTER_GRAVITY.POINTER_GRAVITY_Left;
                _delayedAutoReplaceAction = new AutoReplaceAction(_blogPostHtmlEditorControl, key, replaceHtml, insertMarkupRange);

                //_blogPostHtmlEditorControl.InsertHtml(insertMarkupRange.Start, insertMarkupRange.End, replaceHtml);
                handled = true;
                _lastActionWasReplace++;

            }
            return handled;
        }

        /// <summary>
        /// WinLive 27883: There a few cases of autocorrect that we don't want to do, mainly involving punctuation
        /// typed after a single "i".
        /// </summary>
        private bool ShouldAutoCorrect(string htmlText, char key, string replaceHtml)
        {
            // WinLive 259680: Auto-replace should not work within URLs
            htmlText = StringHelper.GetLastWord(htmlText);
            if (UrlHelper.StartsWithKnownScheme(htmlText) ||
                htmlText.StartsWith("www.") ||
                htmlText.StartsWith(@"\\") ||
                htmlText.StartsWith("skype:"))
                return false;

            if (replaceHtml == "I")
            {
                // These are the few cases we DO want to autocorrect.
                if (key == '\'' || key == ',' || key == '"' || key == ';' || key == '\t' || key == ' ')
                    return true;
                else
                    return false;
            }

            return true;
        }

        private readonly AutoreplaceManager _autoreplaceManager;

        /// <summary>
        /// We want autoreplace operations to happen after the character that was just pressed is
        /// processed. That's because we want the Undo unit for the autoreplace to be after the
        /// character, so hitting Ctrl+Z after autoreplace causes just the autoreplace to be undone.
        /// In Wave 3 and earlier, in contrast, Ctrl+Z would undo the keypress first; this behavior
        /// is different than both Word and Windows Live Mail.
        ///
        /// In the common case, MSHTML handles the keypress, and we do the autoreplace in PostEditorEvent
        /// (in this case, "Post" meaning "after"). It's hard to execute all of the autoreplace logic
        /// here, though, in the case that the user hits Enter--it's annoying to try to figure out
        /// where the boundary is for doing autoreplace matching. So instead, in the pre event handler
        /// we figure out what autoreplace work should be done and where we should do it, and in the
        /// post event handler we actually do the work, thus giving it the proper position on the undo
        /// stack.
        ///
        /// The only time this doesn't work is when our pre event handler stops MSHTML from handling
        /// the keypress, i.e. typographic substitution happens. In this case we need to make sure we
        /// still do the autoreplace operation even though the PostEditorEvent will not get fired.
        /// </summary>
        private AutoReplaceAction _delayedAutoReplaceAction;

        /// <summary>
        /// Stores all the info we need to perform an autoreplace operation later.
        /// </summary>
        class AutoReplaceAction
        {
            private readonly BlogPostHtmlEditorControl editor;
            private readonly char key;
            private readonly string replacement;
            private readonly MarkupRange range;

            public AutoReplaceAction(BlogPostHtmlEditorControl editor, char key, string replacement, MarkupRange range)
            {
                this.editor = editor;
                this.key = key;
                this.replacement = replacement;
                this.range = range;
            }

            public bool Execute(char c)
            {
                if (key != c)
                {
                    Debug.Fail("Incorrect delayed autoreplace operation was found!");
                }
                else if (!range.Positioned)
                {
                    Debug.Fail("Delayed autoreplace operation wasn't positioned");
                }
                else
                {
                    editor.InsertHtml(range.Start, range.End, replacement);
                    return true;
                }
                return false;
            }
        }

        private void MatchUrl(string htmlText, MatchAction action)
        {
            int length;
            GlossaryLinkItem linkItem = GlossaryManager.Instance.SuggestLink(htmlText, out length);
            if (linkItem != null && length > 0)
            {
                string matchText = htmlText.Substring(htmlText.Length - length, length);

                MarkupRange insertMarkupRange = _blogPostHtmlEditorControl.SelectedMarkupRange.Clone();
                for (int i = 0; i < length; i++)
                    insertMarkupRange.Start.MoveUnit(_MOVEUNIT_ACTION.MOVEUNIT_PREVCHAR);

                action(matchText, linkItem, insertMarkupRange);
            }
        }
        private delegate void MatchAction(string matchText, GlossaryLinkItem linkItem, MarkupRange markupRange);

        private bool TermAlreadyLinked(string term, string url)
        {
            IHTMLElement2 bodyElement = (IHTMLElement2)_blogPostHtmlEditorControl.PostBodyElement;
            IHTMLElementCollection aElements = bodyElement.getElementsByTagName("a");
            foreach (IHTMLElement aElement in aElements)
            {
                try
                {
                    IHTMLAnchorElement anchor = aElement as IHTMLAnchorElement;
                    if (anchor != null && aElement.innerText != null &&
                        aElement.innerText.ToLower(CultureInfo.CurrentCulture) ==
                        term.ToLower(CultureInfo.CurrentCulture) &&
                        anchor.href != null && anchor.href.TrimEnd('/') == url.TrimEnd('/'))
                        return true;
                }
                catch (COMException ex)
                {
                    // Bug 624250: Swallow operation failed exception
                    if (ex.ErrorCode != unchecked((int)0x80004005))
                        throw;
                    else
                        Trace.WriteLine(ex.ToString());
                }
            }
            return false;

        }

        private void InsertSuggestedUrl(string matchText, GlossaryLinkItem linkItem, MarkupRange markupRange)
        {
            if (GlossarySettings.AutoLinkTermsOnlyOnce && TermAlreadyLinked(matchText, linkItem.Url))
                return;

            // Make sure we're not in the title and not in a hyperlink already
            IHTMLElement parentElement = _blogPostHtmlEditorControl.PostTitleElement;
            IHTMLElement currentElement = markupRange.ParentElement();
            while (currentElement != null)
            {
                if (parentElement == currentElement)
                    return; // in the title

                if (currentElement.tagName.ToLower(CultureInfo.InvariantCulture) == "a")
                    return; // in an anchor

                currentElement = currentElement.parentElement;
            }

            if (_linkIgnoreWord != null && matchText.ToLower(CultureInfo.CurrentCulture) == _linkIgnoreWord.ToLower(CultureInfo.CurrentCulture))
                return;

            _blogPostHtmlEditorControl.InsertLink(linkItem.Url, matchText, linkItem.Title, linkItem.Rel, linkItem.OpenInNewWindow, markupRange);
            _blogPostHtmlEditorControl.SelectedMarkupRange.Collapse(false);
        }

        private void IgnoreSuggestedUrl(string matchText, GlossaryLinkItem linkItem, MarkupRange markupRange)
        {
            IHTMLElement parent = markupRange.ParentElement();
            while (parent != null && !(parent is IHTMLAnchorElement))
            {
                parent = parent.parentElement;
            }
            if (parent != null && matchText.ToLower(CultureInfo.CurrentCulture) == parent.innerText.ToLower(CultureInfo.CurrentCulture))
                _linkIgnoreWord = matchText;
        }

        private string GetHtmlText(out MarkupPointer blockBoundary)
        {
            MarkupRange blockRange = _blogPostHtmlEditorControl.SelectedMarkupRange.Clone();
            MarkupRange textRange = blockRange.Clone();
            IHTMLElement ele = blockRange.ParentElement(ElementFilters.IsBlockOrTableCellOrBodyElement);

            if (ele == null)
            {
                blockBoundary = null;
                return string.Empty;
            }

            blockRange.MoveToElement(ele, false);
            blockBoundary = blockRange.Start;

            //   Fix Bug 616152 - We want the start and end pointer to match so
            //  we can look back from the start of the insertion point (not the end,
            //  which would include the selection that is going to be overwritten)
            textRange.Collapse(true);

            // Fix bug WinLive 59172: Specific characters of mixed characters are broken in mail body after press the 'Enter' key
            // Caused by using MOVEUNIT_PREVCHAR to navigate into Unicode surrogate (32-bit) characters. We work around this by moving
            // only at word or block boundaries.
            string html = null;
            do
            {
                int startPos = textRange.Start.MarkupPosition;
                textRange.Start.MoveUnitBounded(_MOVEUNIT_ACTION.MOVEUNIT_PREVWORDBEGIN, blockBoundary);
                if (textRange.Start.MarkupPosition == startPos)
                {
                    // PREVWORDBEGIN didn't actually move us, due to no word being available.
                    // To avoid an infinite loop, just move the text range to include the whole
                    // block and move on.
                    textRange.Start.MoveToPointer(blockRange.Start);
                    break;
                }

                if (textRange.Positioned && textRange.Start.IsLeftOfOrEqualTo(textRange.End))
                {
                    html = MarkupHelpers.GetRangeTextFast(textRange);
                }

                html = html ?? string.Empty;

            } while (html.Length < MinLookBack && textRange.Start.IsRightOf(blockRange.Start));

            if (html == null && textRange.Positioned && textRange.Start.IsLeftOfOrEqualTo(textRange.End))
            {
                html = MarkupHelpers.GetRangeTextFast(textRange);
            }

            return html ?? string.Empty;
        }

        private static int MinLookBack
        {
            get
            {
                return Math.Max(GlossaryManager.Instance.MaxLengthHint, TypographicCharacterHandler.MaxLengthHint);
            }
        }

        private bool _haltAutoReplace = false;
    }

}
