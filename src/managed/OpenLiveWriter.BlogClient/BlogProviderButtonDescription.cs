// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using System.Xml;
using System.IO;
using System.Drawing;
using OpenLiveWriter.CoreServices;
using OpenLiveWriter.CoreServices.Settings;
using OpenLiveWriter.Extensibility.BlogClient;

namespace OpenLiveWriter.BlogClient
{

    public class BlogProviderButtonDescription : IBlogProviderButtonDescription, ICloneable
    {
        public BlogProviderButtonDescription(string id, string imageUrl, Bitmap image, string description, string clickUrl, string contentUrl, Size contentDisplaySize, string notificationUrl)
        {
            Init(id, imageUrl, image, description, clickUrl, contentUrl, contentDisplaySize, notificationUrl);
        }

        public BlogProviderButtonDescription(IBlogProviderButtonDescription button)
        {
            Init(button.Id, button.ImageUrl, button.Image, button.Description, button.ClickUrl, button.ContentUrl, button.ContentDisplaySize, button.NotificationUrl);
        }

        protected BlogProviderButtonDescription()
        {
        }

        protected void Init(string id, string imageUrl, Bitmap image, string description, string clickUrl, string contentUrl, Size contentDisplaySize, string notificationUrl)
        {
            Id = id;
            ImageUrl = imageUrl;
            Image = image;
            Description = description;
            ClickUrl = clickUrl;
            ContentUrl = contentUrl;
            _contentDisplaySize = contentDisplaySize;
            NotificationUrl = notificationUrl;
        }

        public string Id { get; private set; }

        public string ImageUrl { get; private set; }

        public Bitmap Image { get; private set; }

        public string Description { get; private set; }

        public string ClickUrl { get; private set; }

        public string ContentUrl { get; private set; }

        public Size ContentDisplaySize
        {
            get
            {
                return _contentDisplaySize;
            }
        }
        private Size _contentDisplaySize;

        public string NotificationUrl { get; private set; }

        public bool SupportsClick
        {
            get
            {
                return ClickUrl != null && ClickUrl != String.Empty;
            }
        }

        public bool SupportsContent
        {
            get
            {
                return ContentUrl != null && ContentUrl != String.Empty;
            }
        }

        public bool SupportsNotification
        {
            get
            {
                return NotificationUrl != null && NotificationUrl != String.Empty;
            }
        }

        public override bool Equals(object obj)
        {
            return Id.Equals((obj as BlogProviderButtonDescription).Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public object Clone()
        {
            return new BlogProviderButtonDescription(Id, ImageUrl, (Bitmap)Image.Clone(), Description, ClickUrl, ContentUrl, ContentDisplaySize, NotificationUrl);
        }

        protected const string ID = "Id";
        protected const string IMAGE_URL = "ImageUrl";
        protected const string IMAGE = "Image";
        protected const string DESCRIPTION = "Description";
        protected const string CLICK_URL = "ClickUrl";
        protected const string CONTENT_URL = "ContentUrl";
        protected const string CONTENT_DISPLAY_SIZE = "ContentDisplaySize";
        protected const string NOTIFICATION_URL = "NotificationUrl";
    }

    public class BlogProviderButtonDescriptionFromSettings : BlogProviderButtonDescription
    {
        public BlogProviderButtonDescriptionFromSettings(SettingsPersisterHelper settingsKey)
        {
            // id
            string id = settingsKey.GetString(ID, String.Empty);

            // image (required)
            string imageUrl = settingsKey.GetString(IMAGE_URL, String.Empty);
            byte[] imageData = settingsKey.GetByteArray(IMAGE, null);
            Bitmap image = null;
            if (imageData != null)
            {
                try
                {
                    image = new Bitmap(new MemoryStream(imageData));
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.ToString());
                }
            }

            // tool-tip text
            string description = settingsKey.GetString(DESCRIPTION, String.Empty);

            // click-url
            string clickUrl = settingsKey.GetString(CLICK_URL, String.Empty);

            // has content
            string contentUrl = settingsKey.GetString(CONTENT_URL, String.Empty);
            Size contentDisplaySize = settingsKey.GetSize(CONTENT_DISPLAY_SIZE, Size.Empty);

            // has notification image
            string notificationUrl = settingsKey.GetString(NOTIFICATION_URL, String.Empty);

            // initialize
            Init(id, imageUrl, image, description, clickUrl, contentUrl, contentDisplaySize, notificationUrl);
        }

        public static void SaveFrameButtonDescriptionToSettings(IBlogProviderButtonDescription button, SettingsPersisterHelper settingsKey)
        {
            settingsKey.SetString(ID, button.Id);
            settingsKey.SetString(IMAGE_URL, button.ImageUrl);
            settingsKey.SetByteArray(IMAGE, ImageHelper.GetBitmapBytes(button.Image, new Size(24, 24)));
            settingsKey.SetString(DESCRIPTION, button.Description);
            settingsKey.SetString(CLICK_URL, button.ClickUrl);
            settingsKey.SetString(CONTENT_URL, button.ContentUrl);
            settingsKey.SetSize(CONTENT_DISPLAY_SIZE, button.ContentDisplaySize);
            settingsKey.SetString(NOTIFICATION_URL, button.NotificationUrl);
        }
    }

    public class BlogProviderButtonNotification : IBlogProviderButtonNotification
    {
        public BlogProviderButtonNotification(TimeSpan pollingInterval, string notificationText, Bitmap notificationImage, bool clearNotificationOnClick)
        {
            Init(pollingInterval, notificationText, notificationImage, clearNotificationOnClick);
        }

        protected BlogProviderButtonNotification()
        {
        }

        protected void Init(TimeSpan pollingInterval, string notificationText, Bitmap notificationImage, bool clearNotificationOnClick)
        {
            PollingInterval = pollingInterval;
            NotificationText = notificationText;
            NotificationImage = notificationImage;
            ClearNotificationOnClick = clearNotificationOnClick;
        }

        // interval until next notification check
        public TimeSpan PollingInterval { get; private set; }

        // url for custom button image
        public Bitmap NotificationImage { get; private set; }

        // text for notification
        public string NotificationText { get; private set; }

        // clear notification image on click?
        public bool ClearNotificationOnClick { get; private set; }
    }
}
