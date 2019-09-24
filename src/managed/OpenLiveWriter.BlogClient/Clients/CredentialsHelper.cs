// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// The CredentialsHelper class.
    /// </summary>
    public partial class CredentialsHelper
    {
        /// <summary>
        /// Prompts for credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain.</param>
        /// <returns>A <see cref="CredentialsPromptResult"/>.</returns>
        public static CredentialsPromptResult PromptForCredentials(
            ref string username,
            ref string password,
            ICredentialsDomain domain)
        {
            CredentialsPromptResult result;

            if (BlogClientUIContext.SilentModeForCurrentThread)
            {
                return CredentialsPromptResult.Abort;
            }

            var uiContext = BlogClientUIContext.ContextForCurrentThread;
            if (uiContext != null)
            {
                var promptHelper = new PromptHelper(uiContext, username, password, domain);
                if (uiContext.InvokeRequired)
                {
                    uiContext.Invoke(new InvokeInUIThreadDelegate(promptHelper.ShowPrompt), new object[0]);
                }
                else
                {
                    promptHelper.ShowPrompt();

                    // force a UI loop so that the dialog closes without hanging while post-dialog logic executes
                    Application.DoEvents();
                }

                result = promptHelper.Result;
                if (result != CredentialsPromptResult.Cancel)
                {
                    username = promptHelper.Username;
                    password = promptHelper.Password;
                }
            }
            else
            {
                result = CredentialsPromptResult.Abort;
            }

            return result;
        }

        /// <summary>
        /// Shows the wait cursor.
        /// </summary>
        /// <returns>An <see cref="IDisposable"/>.</returns>
        public static IDisposable ShowWaitCursor()
        {
            var uiContext = BlogClientUIContext.ContextForCurrentThread;
            return uiContext == null || uiContext.InvokeRequired ? null : new WaitCursor();
        }
    }

#if false

    /// <summary>
    /// Summary description for CredentialsHelper.
    /// </summary>
    public class CredentialsHelper
    {
        private CredentialsHelper()
        {
        }

        public static BlogCredentialsRefreshCallback GetRefreshCallback()
        {
            return new BlogCredentialsRefreshCallback(RefreshCredentials);
        }

        private static BlogCredentialsRefreshResult RefreshCredentials(IBlogClientUIContext owner, ref string username, ref string password, ref object authToken)
        {
            BlogCredentialsRefreshResult refreshResult;
            if(authToken != null ||
                (username != String.Empty && username != null && password != String.Empty && password != null))
            {
                refreshResult = BlogCredentialsRefreshResult.OK;
            }
            else
            {
                // Some of the required credential values are  missing, so prompt for them if there is
                // a UI context available
                if(owner == null)
                    refreshResult = BlogCredentialsRefreshResult.Abort;
                else
                {
                    PromptHelper promptHelper = new PromptHelper(owner, ref username, ref password);
                    if(owner.InvokeRequired)
                        owner.Invoke(new InvokeInUIThreadDelegate(promptHelper.ShowPrompt), new object[0]);
                    else
                        promptHelper.ShowPrompt();

                    username = promptHelper.Username;
                    password = promptHelper.Password;
                    refreshResult = promptHelper.Result;
                }
            }

            if(refreshResult != BlogCredentialsRefreshResult.Cancel && refreshResult != BlogCredentialsRefreshResult.Abort)
            {
                // save the login time in the authtoken
                authToken = DateTime.Now;
            }
            return refreshResult;
        }

        public class PromptHelper
        {
            private IWin32Window owner;
            private string _username;
            private string _password;
            private BlogCredentialsRefreshResult _result;
            public PromptHelper(IWin32Window owner, ref string username, ref string password)
            {
                this.owner = owner;
                _username = username;
                _password = password;
            }

            public void ShowPrompt()
            {
                BlogCredentialsRefreshResult refreshResult;
                using (BlogClientLoginDialog form = new BlogClientLoginDialog())
                {
                    if(_username != null)
                        form.UserName = _username;
                    if(_password != null)
                        form.Password = _password;

                    DialogResult dialogResult = form.ShowDialog(owner) ;
                    if (dialogResult == DialogResult.OK)
                    {
                        _username = form.UserName;
                        _password = form.Password;
                        refreshResult = form.SavePassword
                            ? BlogCredentialsRefreshResult.SaveUsernameAndPassword
                            : BlogCredentialsRefreshResult.SaveUsername;
                    }
                    else
                        refreshResult = BlogCredentialsRefreshResult.Cancel;
                }
                _result = refreshResult;
            }

            public string Username
            {
                get { return _username; }
            }

            public string Password
            {
                get { return _password; }
            }

            public BlogCredentialsRefreshResult Result
            {
                get { return _result; }
            }
        }
    }
#endif
}
