// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework.Preferences
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.CoreServices.Settings;
    using OpenLiveWriter.Interop.Windows;

    /// <summary>
    /// Preferences base class.
    /// </summary>
    public abstract class Preferences
    {
        /// <summary>
        ///  A value that indicates that the Preferences object has been modified since being
        ///  loaded.
        /// </summary>
        private bool modified;

        /// <summary>
        /// Handle to underlying registry key used by these prefs (we need this in order
        /// to register for change notifications)
        /// </summary>
        private UIntPtr hPrefsKey = UIntPtr.Zero;

        /// <summary>
        /// Win32 event that is signalled whenever our prefs key is changed
        /// </summary>
        private ManualResetEvent settingsChangedEvent = null;

        /// <summary>
        /// State variable that indicates we have disabled change monitoring
        /// (normally because a very unexpected error has occurred during change
        /// monitoring initialization)
        /// </summary>
        private bool changeMonitoringDisabled = false;

        /// <summary>
        /// Occurs when one or more preferences in the Preferences class have been modified.
        /// </summary>
        public event EventHandler PreferencesModified;

        /// <summary>
        /// Occurs when one or more preferences in the Preferences class have changed.
        /// </summary>
        public event EventHandler PreferencesChanged;

        /// <summary>
        /// Initialize preferences
        /// </summary>
        /// <param name="subKey">sub key name</param>
        public Preferences(string subKey) : this(subKey, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the Preferences class (optionally enable change monitoring)
        /// </summary>
        /// <param name="subKey">sub-key name</param>
        /// <param name="monitorChanges">specifies whether the creator intends to monitor
        /// this prefs object for changes by calling the CheckForChanges method</param>
        public Preferences(string subKey, bool monitorChanges)
        {
            //	Instantiate the settings persister helper object.
            this.SettingsPersisterHelper = ApplicationEnvironment.PreferencesSettingsRoot.GetSubSettings(subKey);

            //	Load preferences
            this.LoadPreferences();

            //	Monitor changes, if requested.
            if (monitorChanges)
            {
                this.ConfigureChangeMonitoring(subKey);
            }
        }

        /// <summary>
        /// Check for changes to the preferences
        /// </summary>
        /// <returns>true if there were changes; otherwise, false.</returns>
        public bool CheckForChanges() => this.ReloadPreferencesIfNecessary();

        /// <summary>
        ///	Returns a value that indicates that the Preferences object has been modified since
        ///	being loaded.
        /// </summary>
        public bool IsModified() => this.modified;

        /// <summary>
        /// Saves preferences.
        /// </summary>
        public void Save()
        {
            if (this.modified)
            {
                this.SavePreferences();
                this.modified = false;
            }
        }

        /// <summary>
        /// Loads preferences.  This method is overridden in derived classes to load the
        /// preferences of the class.
        /// </summary>
        protected abstract void LoadPreferences();

        /// <summary>
        /// Saves preferences.  This method is overridden in derived classes to save the
        /// preferences of the class.
        /// </summary>
        protected abstract void SavePreferences();

        /// <summary>
        ///	Sets a value that indicates that the Preferences object has been modified since being
        ///	loaded.
        /// </summary>
        protected void Modified()
        {
            this.modified = true;
            this.OnPreferencesModified(EventArgs.Empty);
        }

        /// <summary>
        /// Gets the SettingsPersisterHelper for this Preferences object.
        /// </summary>
        protected SettingsPersisterHelper SettingsPersisterHelper { get; }

        /// <summary>
        /// Raises the PreferencesModified event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnPreferencesModified(EventArgs e) => PreferencesModified?.Invoke(this, e);

        /// <summary>
        /// Raises the Changed event.
        /// </summary>
        /// <param name="e">An EventArgs that contains the event data.</param>
        protected virtual void OnPreferencesChanged(EventArgs e) => PreferencesChanged?.Invoke(this, e);

        /// <summary>
        /// Configure change monitoring for this prefs object
        /// </summary>
        /// <param name="subKey"></param>
        private void ConfigureChangeMonitoring(string subKey)
        {
            // assert preconditions
            Debug.Assert(this.hPrefsKey == UIntPtr.Zero);
            Debug.Assert(this.settingsChangedEvent == null);

            try
            {
                // open handle to registry key
                var result = Advapi32.RegOpenKeyEx(
                    HKEY.CURRENT_USER,
                    string.Format(
                        CultureInfo.InvariantCulture,
                        @"{0}\{1}\{2}",
                        ApplicationEnvironment.SettingsRootKeyName,
                        ApplicationConstants.PREFERENCES_SUB_KEY,
                        subKey),
                    0,
                    KEY.READ,
                    out this.hPrefsKey);
                if (result != ERROR.SUCCESS)
                {
                    Trace.Fail("Failed to open registry key");
                    this.changeMonitoringDisabled = true;
                    return;
                }

                // create settings changed event
                this.settingsChangedEvent = new ManualResetEvent(false);

                // start monitoring changes
                this.MonitorChanges();
            }
            catch (Exception e)
            {
                // Just being super-paranoid here because this code is likely be called during
                // application initialization -- if ANY type of error occurs then we disable
                // change monitoring for the life of this object
                Trace.WriteLine($"Unexpected error occurred during change monitor configuration: {e.Message}\r\n{e.StackTrace}");
                this.changeMonitoringDisabled = true;
            }
        }

        /// <summary>
        /// Monitor changes in the registry key for this prefs object
        /// </summary>
        private void MonitorChanges()
        {
            // reset the settings changed event so it will not be signaled until
            // a change is made to the specified key
            this.settingsChangedEvent.Reset();

            // request that the event be signaled when the registry key changes
            var result = Advapi32.RegNotifyChangeKeyValue(
                this.hPrefsKey,
                false,
                REG_NOTIFY_CHANGE.LAST_SET,
                this.settingsChangedEvent.SafeWaitHandle,
                true);
            if (result != ERROR.SUCCESS)
            {
                Trace.WriteLine($"Unexpeced failure to monitor reg key (Error code: {result.ToString(CultureInfo.InvariantCulture)}");
                this.changeMonitoringDisabled = true;
            }
        }

        /// <summary>
        /// Load changes to preferences if they have changed since our last check
        /// </summary>
        /// <returns>true if preferences were reloaded; otherwise, false.</returns>
        private bool ReloadPreferencesIfNecessary()
        {
            //	If change monitoring is disabled, then just return.
            if (this.changeMonitoringDisabled)
            {
                return false;
            }

            //	Verify this instance is configured to monitor changes.
            if (this.settingsChangedEvent == null)
            {
                Debug.Fail("Must initialize preferences object with monitorChanges flag set to true in order to call CheckForChanges");
                return false;
            }

            // check to see whether any changes have occurred
            try
            {
                // if the settings changed event is signaled then reload preferences
                if (this.settingsChangedEvent.WaitOne(0, false))
                {
                    //	Reload.
                    this.LoadPreferences();

                    //	Monitor subsequent changes.
                    this.MonitorChanges();

                    //	Raise the PreferencesChanged event.
                    this.OnPreferencesChanged(EventArgs.Empty);

                    //	Changes were loaded.
                    return true;
                }
            }
            catch (Exception e)
            {
                // Just being super-paranoid here because this code is called from a timer
                // in the UI thread -- if ANY type of error occurs during change monitoring
                // then we disable change monitoring for the life of this object
                Trace.WriteLine($"Unexpected error occurred during check for changes: {e.Message}\r\n{e.StackTrace}");
                this.changeMonitoringDisabled = true;
                return false;
            }

            //	Not loaded!
            return false;
        }
    }
}
