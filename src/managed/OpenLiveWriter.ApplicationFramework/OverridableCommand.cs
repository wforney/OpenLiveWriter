// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Localization;

    /// <summary>
    ///     Class OverridableCommand.
    ///     Implements the <see cref="OpenLiveWriter.ApplicationFramework.Command" />
    ///     Implements the <see cref="OpenLiveWriter.ApplicationFramework.IOverridableCommand" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.Command" />
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.IOverridableCommand" />
    public class OverridableCommand : Command, IOverridableCommand
    {
        /// <summary>
        ///     The overrides
        /// </summary>
        protected Dictionary<PropertyKey, PropVariant> overrides = new Dictionary<PropertyKey, PropVariant>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverridableCommand" /> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        public OverridableCommand(CommandId commandId)
            : base(commandId)
        {
        }

        /// <summary>
        ///     Delegate IComparableDelegate
        /// </summary>
        /// <returns>A <see cref="IComparable"/>.</returns>
        protected delegate IComparable IComparableDelegate();

        /// <summary>
        ///     Gets or sets the context availability.
        /// </summary>
        /// <value>The context availability.</value>
        public virtual ContextAvailability ContextAvailability
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the Command is enabled or not.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public override bool Enabled
        {
            get =>
                Convert.ToBoolean(
                    this.GetOverride(ref PropertyKeys.Enabled, this.enabled),
                    CultureInfo.InvariantCulture);

            set
            {
                if (this.enabled == value)
                {
                    return;
                }

                var currentValue = this.Enabled;
                this.enabled = value;
                this.UpdateInvalidationStateAndNotifyIfDifferent(
                    ref PropertyKeys.Enabled,
                    this.Enabled,
                    () => currentValue);

                // If changing in High Contrast Black mode, force a refresh of the image
                if (ApplicationEnvironment.IsHighContrastBlack)
                {
                    this.Invalidate(
                        new[] { PropertyKeys.SmallHighContrastImage, PropertyKeys.LargeHighContrastImage });
                }
            }
        }

        /// <summary>
        ///     Cancels the override.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The result.</returns>
        public virtual int CancelOverride(ref PropertyKey key) =>
            key == PropertyKeys.Enabled
                ? this.RemoveOverride(ref key, () => this.Enabled)
                :
                key == PropertyKeys.ContextAvailable
                    ?
                    this.RemoveOverride(ref key, () => (uint)this.ContextAvailability)
                    : HRESULT.E_INVALIDARG;

        /// <summary>
        ///     Overrides the property.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="overrideValue">The override value.</param>
        /// <returns>The result.</returns>
        public virtual int OverrideProperty(ref PropertyKey key, PropVariantRef overrideValue)
        {
            if (key == PropertyKeys.Enabled)
            {
                var currentValue = this.Enabled;
                this.overrides[key] = overrideValue.PropVariant;
                this.UpdateInvalidationStateAndNotifyIfDifferent(
                    ref key,
                    Convert.ToBoolean(overrideValue.PropVariant.Value, CultureInfo.InvariantCulture),
                    () => currentValue);
                return HRESULT.S_OK;
            }

            if (key == PropertyKeys.ContextAvailable)
            {
                var currentValue = (uint)this.ContextAvailability;
                this.overrides[key] = overrideValue.PropVariant;
                this.UpdateInvalidationStateAndNotifyIfDifferent(
                    ref key,
                    Convert.ToUInt32(overrideValue.PropVariant.Value, CultureInfo.InvariantCulture),
                    () => currentValue);
                return HRESULT.S_OK;
            }

            return HRESULT.E_INVALIDARG;
        }

        /// <summary>
        ///     Gets the override.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>An <see cref="object"/>.</returns>
        protected object GetOverride(ref PropertyKey key, object defaultValue) =>
            this.overrides.TryGetValue(key, out var propVariant) ? propVariant.Value : defaultValue;

        /// <summary>
        ///     Removes the override.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="currentValueDelegate">The current value delegate.</param>
        /// <returns>The result.</returns>
        protected int RemoveOverride(ref PropertyKey key, IComparableDelegate currentValueDelegate)
        {
            if (!this.overrides.TryGetValue(key, out var overrideValue))
            {
                return HRESULT.S_FALSE;
            }

            this.overrides.Remove(key);
            this.UpdateInvalidationStateAndNotifyIfDifferent(
                ref key,
                (IComparable)overrideValue.Value,
                currentValueDelegate);
            return HRESULT.S_OK;
        }

        /// <summary>
        ///     Updates the invalidation state and notify if different.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="overrideValue">The override value.</param>
        /// <param name="currentValueDelegate">The current value delegate.</param>
        private void UpdateInvalidationStateAndNotifyIfDifferent(
            ref PropertyKey key,
            IComparable overrideValue,
            IComparableDelegate currentValueDelegate)
        {
            // where T : IComparable
            // Only invalidate if we're actually making a change.
            // Unnecessary invalidations hurt perf as well as cause ribbon bugs.
            if (overrideValue.CompareTo(currentValueDelegate()) == 0)
            {
                return;
            }

            this.UpdateInvalidationState(key, InvalidationState.Pending);
            this.OnStateChanged(EventArgs.Empty);
        }
    }
}
