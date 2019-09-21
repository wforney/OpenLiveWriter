// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;

    /// <summary>
    ///     Interface IOverridableCommand
    /// </summary>
    public interface IOverridableCommand
    {
        /// <summary>
        ///     Gets or sets the context availability.
        /// </summary>
        /// <value>The context availability.</value>
        ContextAvailability ContextAvailability { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="IOverridableCommand" /> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        bool Enabled { get; set; }

        /// <summary>
        ///     Cancels the override.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The result.</returns>
        int CancelOverride(ref PropertyKey key);

        /// <summary>
        ///     Overrides the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="overrideValue">The override value.</param>
        /// <returns>The result.</returns>
        int OverrideProperty(ref PropertyKey key, PropVariantRef overrideValue);
    }
}
