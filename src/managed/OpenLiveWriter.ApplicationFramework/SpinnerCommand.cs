// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Interface IRepresentativeString
    /// </summary>
    public interface IRepresentativeString
    {
        /// <summary>
        /// Gets the representative string.
        /// </summary>
        /// <value>The representative string.</value>
        string RepresentativeString { get; }
    }

    /// <summary>
    /// Class SpinnerCommand.
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.Command" />
    /// Implements the <see cref="OpenLiveWriter.ApplicationFramework.IRepresentativeString" />
    /// </summary>
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.Command" />
    /// <seealso cref="OpenLiveWriter.ApplicationFramework.IRepresentativeString" />
    public class SpinnerCommand : Command, IRepresentativeString
    {
        /// <summary>
        /// The value
        /// </summary>
        private decimal value;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpinnerCommand"/> class.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="increment">The increment.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <param name="representativeString">The representative string.</param>
        /// <param name="formatString">The format string.</param>
        public SpinnerCommand(
            CommandId commandId,
            decimal minValue,
            decimal maxValue,
            decimal initialValue,
            decimal increment,
            uint decimalPlaces,
            string representativeString,
            string formatString)
            : base(commandId)
        {
            Debug.Assert(
                initialValue >= minValue && initialValue <= maxValue,
                "Initial value is outside of allowed range.");
            this.MinValue = minValue;
            this.MaxValue = maxValue;
            this.value = initialValue;
            this.Increment = increment;
            this.DecimalPlaces = decimalPlaces;
            this.RepresentativeString = representativeString;
            this.FormatString = formatString;

            this.UpdateInvalidationState(PropertyKeys.MinValue, InvalidationState.Pending);
            this.UpdateInvalidationState(PropertyKeys.MaxValue, InvalidationState.Pending);
            this.UpdateInvalidationState(PropertyKeys.DecimalValue, InvalidationState.Pending);
            this.UpdateInvalidationState(PropertyKeys.Increment, InvalidationState.Pending);
            this.UpdateInvalidationState(PropertyKeys.DecimalPlaces, InvalidationState.Pending);
            this.UpdateInvalidationState(PropertyKeys.RepresentativeString, InvalidationState.Pending);
            this.UpdateInvalidationState(PropertyKeys.FormatString, InvalidationState.Pending);
        }

        /// <summary>
        /// Gets the decimal places.
        /// </summary>
        /// <value>The decimal places.</value>
        public uint DecimalPlaces { get; }

        /// <summary>
        /// Gets the format string.
        /// </summary>
        /// <value>The format string.</value>
        public string FormatString { get; }

        /// <summary>
        /// Gets the increment.
        /// </summary>
        /// <value>The increment.</value>
        public decimal Increment { get; }

        /// <summary>
        /// Gets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        public decimal MaxValue { get; }

        /// <summary>
        /// Gets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        public decimal MinValue { get; }

        /// <summary>
        /// Gets the representative string.
        /// </summary>
        /// <value>The representative string.</value>
        public string RepresentativeString { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public decimal Value
        {
            get => this.value;
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.UpdateInvalidationState(PropertyKeys.DecimalValue, InvalidationState.Pending);
                    this.OnStateChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets the property variant.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="value">The value.</param>
        public override void GetPropVariant(PropertyKey key, PropVariantRef currentValue, ref PropVariant value)
        {
            if (key == PropertyKeys.DecimalValue)
            {
                value.SetDecimal(this.Value);
            }
            else if (key == PropertyKeys.MinValue)
            {
                value.SetDecimal(this.MinValue);
            }
            else if (key == PropertyKeys.MaxValue)
            {
                value.SetDecimal(this.MaxValue);
            }
            else if (key == PropertyKeys.Increment)
            {
                value.SetDecimal(this.Increment);
            }
            else if (key == PropertyKeys.DecimalPlaces)
            {
                value.SetUInt(this.DecimalPlaces);
            }
            else if (key == PropertyKeys.FormatString)
            {
                value.SetString(this.FormatString);
            }
            else
            {
                base.GetPropVariant(key, currentValue, ref value);
            }
        }

        /// <summary>
        /// Performs the execute.
        /// </summary>
        /// <param name="verb">The verb.</param>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="commandExecutionProperties">The command execution properties.</param>
        /// <returns>System.Int32.</returns>
        public override int PerformExecute(
            CommandExecutionVerb verb,
            PropertyKeyRef key,
            PropVariantRef currentValue,
            IUISimplePropertySet commandExecutionProperties)
        {
            var spinnerValue = Convert.ToDecimal(currentValue.PropVariant.Value, CultureInfo.InvariantCulture);
            this.PerformExecuteWithArgs(verb, new ExecuteEventHandlerArgs(this.CommandId.ToString(), spinnerValue));
            return HRESULT.S_OK;
        }
    }
}