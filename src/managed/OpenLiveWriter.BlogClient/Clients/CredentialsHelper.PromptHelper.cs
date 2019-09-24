// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Windows.Forms;

    public partial class CredentialsHelper
    {
        /// <summary>
        /// The PromptHelper class.
        /// </summary>
        private class PromptHelper
        {
            /// <summary>
            /// The domain
            /// </summary>
            private readonly ICredentialsDomain domain;

            /// <summary>
            /// The owner
            /// </summary>
            private readonly IWin32Window owner;

            /// <summary>
            /// Initializes a new instance of the <see cref="PromptHelper"/> class.
            /// </summary>
            /// <param name="owner">The owner.</param>
            /// <param name="username">The username.</param>
            /// <param name="password">The password.</param>
            /// <param name="domain">The domain.</param>
            public PromptHelper(IWin32Window owner, string username, string password, ICredentialsDomain domain)
            {
                this.owner = owner;
                this.Username = username;
                this.Password = password;
                this.domain = domain;
            }

            /// <summary>
            /// Gets the password.
            /// </summary>
            /// <value>The password.</value>
            public string Password { get; private set; }

            /// <summary>
            /// Gets the result.
            /// </summary>
            /// <value>The result.</value>
            public CredentialsPromptResult Result { get; private set; }

            /// <summary>
            /// Gets the username.
            /// </summary>
            /// <value>The username.</value>
            public string Username { get; private set; }

            /// <summary>
            /// Shows the prompt.
            /// </summary>
            public void ShowPrompt()
            {
                using (var form = new BlogClientLoginDialog())
                {
                    if (this.Username != null)
                    {
                        form.UserName = this.Username;
                    }

                    if (this.Password != null)
                    {
                        form.Password = this.Password;
                    }

                    if (this.domain != null)
                    {
                        form.Domain = this.domain;
                        form.Text = $"{form.Text} - {this.domain.Name}";
                    }

                    var dialogResult = form.ShowDialog(this.owner);
                    if (dialogResult == DialogResult.OK)
                    {
                        this.Username = form.UserName;
                        this.Password = form.Password;
                        this.Result = form.SavePassword
                                           ? CredentialsPromptResult.SaveUsernameAndPassword
                                           : CredentialsPromptResult.SaveUsername;
                    }
                    else
                    {
                        this.Result = CredentialsPromptResult.Cancel;
                    }
                }
            }
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
