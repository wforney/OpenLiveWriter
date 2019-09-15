// <copyright file="Form1.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace BlogRunnerGui
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Windows.Forms;
    using System.Xml;
    using BlogRunner.Core.Config;

    /// <summary>
    /// The form class.
    /// Implements the <see cref="System.Windows.Forms.Form" />
    /// </summary>
    /// <seealso cref="System.Windows.Forms.Form" />
    public partial class Form1 : Form
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Form1"/> class.
        /// </summary>
        public Form1() => this.InitializeComponent();

        /// <summary>
        /// Gets the selected providers.
        /// </summary>
        /// <value>The selected providers.</value>
        private BlogProviderItem[] SelectedProviders
        {
            get
            {
                var checkedItems = new List<BlogProviderItem>();
                foreach (BlogProviderItem item in this.listProviders.CheckedItems)
                {
                    checkedItems.Add(item);
                }

                return checkedItems.ToArray();
            }
        }

        /// <summary>
        /// Handles the PathChanged event of the fileBlogProviders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fileBlogProviders_PathChanged(object sender, EventArgs e) => this.UpdateCommand();

        /// <summary>
        /// Handles the PathChanged event of the fileConfig control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fileConfig_PathChanged(object sender, EventArgs e)
        {
            this.listProviders.Items.Clear();

            var configPath = this.fileConfig.Path;

            this.UpdateCommand();

            try
            {
                if (configPath.Length == 0 || !File.Exists(configPath))
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(configPath);
            foreach (XmlElement providerEl in xmlDoc.SelectNodes("/config/providers/provider/blog/.."))
            {
                var id = providerEl.SelectSingleNode("id/text()").Value;
                var name = providerEl.SelectSingleNode("name/text()").Value;
                var item = new BlogProviderItem(id, name);
                this.listProviders.Items.Add(item);
            }

            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the PathChanged event of the fileOutput control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void fileOutput_PathChanged(object sender, EventArgs e) => this.UpdateCommand();

        /// <summary>
        /// Handles the Click event of the btnSelectAll control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < this.listProviders.Items.Count; i++)
            {
                this.listProviders.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectNone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnSelectNone_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < this.listProviders.Items.Count; i++)
            {
                this.listProviders.SetItemChecked(i, false);
            }
        }

        /// <summary>
        /// Handles the ItemCheck event of the listProviders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ItemCheckEventArgs"/> instance containing the event data.</param>
        private void listProviders_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            var checkedItems = new List<BlogProviderItem>(this.SelectedProviders);

            var currentItem = (BlogProviderItem)this.listProviders.Items[e.Index];
            if (e.NewValue == CheckState.Checked)
            {
                checkedItems.Add(currentItem);
            }
            else
            {
                checkedItems.Remove(currentItem);
            }

            this.UpdateCommand(checkedItems);
        }

        /// <summary>
        /// Handles the CheckedChanged event of the chkVerbose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void chkVerbose_CheckedChanged(object sender, EventArgs e) => this.UpdateCommand();

        /// <summary>
        /// Updates the command.
        /// </summary>
        private void UpdateCommand() => this.UpdateCommand(this.SelectedProviders);

        /// <summary>
        /// Sets the selected provider ids.
        /// </summary>
        /// <param name="providerIds">The provider ids.</param>
        public void SetSelectedProviderIds(string[] providerIds)
        {
            for (var i = 0; i < this.listProviders.Items.Count; i++)
            {
                if (Array.IndexOf(providerIds, ((BlogProviderItem)this.listProviders.Items[i]).Id) >= 0)
                {
                    this.listProviders.SetItemChecked(i, true);
                }
            }
        }

        /// <summary>
        /// Updates the command.
        /// </summary>
        /// <param name="providers">The providers.</param>
        private void UpdateCommand(IEnumerable<BlogProviderItem> providers)
        {
            var ids = new List<BlogProviderItem>(providers).ConvertAll((BlogProviderItem item) => item.Id);
            if (ids.Count == this.listProviders.Items.Count)
            {
                ids.Clear();
            }

            var args = new List<string>
            {
                "BlogRunner.exe",
                $"/{BlogRunnerCommandLineOptions.OptionProviders}:{this.fileBlogProviders.Path}",
                $"/{BlogRunnerCommandLineOptions.OptionConfig}:{this.fileConfig.Path}",
            };

            if (this.fileOutput.Path.Length > 0)
            {
                args.Add($"/{BlogRunnerCommandLineOptions.OptionOutput}:{this.fileOutput.Path}");
            }

            if (this.chkVerbose.Checked)
            {
                args.Add($"/{BlogRunnerCommandLineOptions.OptionVerbose}");
            }

            args.Add($"/{BlogRunnerCommandLineOptions.OptionPause}");

            args.AddRange(ids);
            args = args.ConvertAll((string str) => this.MaybeQuote(str));
            this.textBox1.Text = string.Join(" ", args.ToArray());
        }

        /// <summary>
        /// Maybes the quote.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>System.String.</returns>
        private string MaybeQuote(string str)
        {
            if (str.Contains(" "))
            {
                return $"\"{str}\"";
            }

            return str;
        }

        /// <summary>
        /// Handles the Load event of the Form1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Form1_Load(object sender, EventArgs e)
        {
            this.LoadSettings();
            this.UpdateCommand();
        }

        /// <summary>
        /// Handles the Click event of the btnRun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            var cmdLine = this.textBox1.Text;
            if (cmdLine.Length == 0)
            {
                MessageBox.Show("Nothing to do!");
                return;
            }

            var chunks = cmdLine.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (chunks.Length == 1)
            {
                Process.Start(chunks[0]);
            }
            else
            {
                Process.Start(chunks[0], chunks[1]);
            }
        }

        /// <summary>
        /// Loads the settings.
        /// </summary>
        private void LoadSettings()
        {
            var options = new BlogRunnerCommandLineOptions();
            options.Parse(Environment.GetCommandLineArgs(), false);
            this.fileBlogProviders.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OptionProviders, string.Empty);
            this.fileConfig.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OptionConfig, string.Empty);
            this.fileOutput.Path = (string)options.GetValue(BlogRunnerCommandLineOptions.OptionOutput, string.Empty);
            this.chkVerbose.Checked = options.GetFlagValue(BlogRunnerCommandLineOptions.OptionVerbose, false);
            this.SetSelectedProviderIds(options.UnnamedArgs);
        }

        private const string SETTING_PROVIDERS = "providers";
        private const string SETTING_CONFIG = "config";
        private const string SETTING_OUTPUT = "output";
        private const string SETTING_PROVIDER = "provider";

        /// <summary>
        /// The blog provider item class.
        /// </summary>
        public class BlogProviderItem
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="BlogProviderItem"/> class.
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <param name="name">The name.</param>
            public BlogProviderItem(string id, string name)
            {
                this.Id = id;
                this.Name = name;
            }

            /// <summary>
            /// Gets the identifier.
            /// </summary>
            /// <value>The identifier.</value>
            public string Id { get; private set; }

            /// <summary>
            /// Gets the name.
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; private set; }

            /// <summary>
            /// Returns a <see cref="string" /> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="string" /> that represents this instance.</returns>
            public override string ToString() => this.Name;

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
            public override int GetHashCode() => this.Id.GetHashCode();

            /// <summary>
            /// Determines whether the specified <see cref="object" /> is equal to this instance.
            /// </summary>
            /// <param name="obj">The object to compare with the current object.</param>
            /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
            public override bool Equals(object obj) =>
                obj is BlogProviderItem other ? string.Equals(this.Id, other.Id, StringComparison.Ordinal) : false;
        }

        /// <summary>
        /// Handles the Click event of the btnClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void btnClose_Click(object sender, EventArgs e) => this.Close();
    }
}
