// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Diagnostics;
    using OpenLiveWriter.CoreServices.Threading;

    public partial class SatelliteApplicationForm
    {
        /// <summary>
        /// Reference to command bar control
        /// </summary>
        private class Launcher
        {
            /// <summary>
            /// The form type
            /// </summary>
            private readonly Type formType;

            /// <summary>
            /// The parameters
            /// </summary>
            private readonly object[] parameters;

            /// <summary>
            /// Initializes a new instance of the <see cref="Launcher"/> class.
            /// </summary>
            /// <param name="formType">Type of the form.</param>
            /// <param name="parameters">The parameters.</param>
            public Launcher(Type formType, params object[] parameters)
            {
                this.formType = formType;
                this.parameters = parameters;
            }

            /// <summary>
            /// Opens the form.
            /// </summary>
            public void OpenForm()
            {
                // open a new form on a non-background, STA thread
                using (ProcessKeepalive.Open())
                {
                    // throws exception if we are shutting down
                    // this object will be signalled when the new thread has
                    // finished incrementing
                    var signalIncremented = new object();

                    var formThread = ThreadHelper.NewThread(
                        ThreadStartWithParams.Create(this.ThreadMain, signalIncremented),
                        "FormThread",
                        true,
                        true,
                        false);

                    lock (signalIncremented)
                    {
                        formThread.Start();

                        // Don't continue until refcount has been incremented
                        Monitor.Wait(signalIncremented);
                    }
                }
            }

            /// <summary>
            /// Threads the main.
            /// </summary>
            /// <param name="tmParameters">The parameters.</param>
            [STAThread]
            private void ThreadMain(object[] tmParameters)
            {
                IDisposable splashScreen = null;
                if (tmParameters.Length > 0)
                {
                    splashScreen = tmParameters[tmParameters.Length - 1] as IDisposable;
                }

                ProcessKeepalive pk = null;
                try
                {
                    try
                    {
                        pk = ProcessKeepalive.Open();
                    }
                    finally
                    {
                        var signalIncremented = tmParameters[0];
                        lock (signalIncremented)
                        {
                            Monitor.Pulse(tmParameters[0]);
                        }
                    }

                    // housekeeping initialization
                    Application.OleRequired();
                    UnexpectedErrorDelegate.RegisterWindowsHandler();

                    // Create and run the form
                    var applicationForm =
                        (SatelliteApplicationForm)Activator.CreateInstance(this.formType, this.parameters);
                    Application.Run(applicationForm);
                }
                catch (Exception ex)
                {
                    UnexpectedErrorMessage.Show(ex);
                }
                finally
                {
                    pk?.Dispose();

                    if (splashScreen != null)
                    {
                        Debug.Assert(splashScreen is FormSplashScreen);
                        splashScreen.Dispose();
                    }
                }
            }
        }
    }
}
