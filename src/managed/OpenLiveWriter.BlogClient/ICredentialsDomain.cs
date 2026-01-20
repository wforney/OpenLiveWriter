// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.BlogClient
{
    public interface ICredentialsDomain
    {
        string Name { get; }
        string Description { get; }
        byte[] Icon { get; }
        byte[] Image { get; }
        bool AllowsSavePassword { get; }
    }

    public class CredentialsDomain : ICredentialsDomain
    {
        public CredentialsDomain(string name, string description, byte[] icon, byte[] image)
        {
            CredentialsDomainInternal(name, description, icon, image, true);
        }

        public CredentialsDomain(string name, string description, byte[] icon, byte[] image, bool allowSavePassword)
        {
            CredentialsDomainInternal(name, description, icon, image, allowSavePassword);
        }

        private void CredentialsDomainInternal(string name, string description, byte[] icon, byte[] image, bool allowSavePassword)
        {
            Name = name;
            Description = description;
            Icon = icon;
            Image = image;
            AllowsSavePassword = allowSavePassword;
        }

        #region ICredentialsDomain Members

        public string Name { get; private set; }

        public string Description { get; private set; }

        public byte[] Icon { get; private set; }

        public byte[] Image { get; private set; }

        public bool AllowsSavePassword { get; private set; }

        #endregion
    }
}
