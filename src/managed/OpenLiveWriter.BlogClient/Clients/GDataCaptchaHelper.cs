// <copyright file="GDataCaptchaForm.cs" company=".NET Foundation">
// Copyright (c) .NET Foundation. All rights reserved.
// </copyright>
// <summary>
// Licensed under the MIT license. See LICENSE file in the project root for details.
// </summary>

namespace OpenLiveWriter.BlogClient.Clients
{
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    using OpenLiveWriter.CoreServices;

    /// <summary>
    /// The GDataCaptchaHelper class.
    /// </summary>
    internal class GDataCaptchaHelper
    {
        /// <summary>
        /// The image URL
        /// </summary>
        private readonly string imageUrl;

        /// <summary>
        /// The owner
        /// </summary>
        private readonly IWin32Window owner;

        /// <summary>
        /// Initializes a new instance of the <see cref="GDataCaptchaHelper"/> class.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="imageUrl">The image URL.</param>
        public GDataCaptchaHelper(IWin32Window owner, string imageUrl)
        {
            this.owner = owner;
            this.imageUrl = UrlHelper.UrlCombineIfRelative("http://www.google.com/accounts/", imageUrl);
        }

        /// <summary>
        /// Gets the dialog result.
        /// </summary>
        /// <value>The dialog result.</value>
        public DialogResult DialogResult { get; private set; }

        /// <summary>
        /// Gets the reply.
        /// </summary>
        /// <value>The reply.</value>
        public string Reply { get; private set; }

        /// <summary>
        /// Shows the captcha.
        /// </summary>
        public void ShowCaptcha()
        {
            var response = HttpRequestHelper.SendRequest(this.imageUrl);
            Image image;
            using (var s = response.GetResponseStream())
            {
                image = Image.FromStream(new MemoryStream(StreamHelper.AsBytes(s)));
            }

            using (image)
            {
                using (var form = new GDataCaptchaForm())
                {
                    form.SetImage(image);
                    this.DialogResult = form.ShowDialog(this.owner);
                    if (this.DialogResult == DialogResult.OK)
                    {
                        this.Reply = form.Reply;
                    }
                }
            }
        }
    }
}
