// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    using OpenLiveWriter.CoreServices;
    using OpenLiveWriter.Interop.Com;
    using OpenLiveWriter.Interop.Com.Ribbon;
    using OpenLiveWriter.Localization;

    /// <summary>
    /// Class GenericCommandHandler.
    /// Implements the <see cref="IUICommandHandler" />
    /// </summary>
    /// <seealso cref="IUICommandHandler" />
    public class GenericCommandHandler : IUICommandHandler
    {
        /// <summary>
        /// The parent command manager
        /// </summary>
        private readonly CommandManager ParentCommandManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericCommandHandler"/> class.
        /// </summary>
        /// <param name="commandManager">The command manager.</param>
        public GenericCommandHandler(CommandManager commandManager) => this.ParentCommandManager = commandManager;

        /// <summary>
        /// Gets the placeholder image.
        /// </summary>
        /// <returns>IUIImage.</returns>
        protected IUIImage GetPlaceholderImage()
        {
            var bitmap = Images.Missing_LargeImage;

            return bitmap == null ? null : RibbonHelper.CreateImage(bitmap.GetHbitmap(), ImageCreationOptions.Transfer);
        }

        /// <summary>
        /// Executes the specified command identifier.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="verb">The verb.</param>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="commandExecutionProperties">The command execution properties.</param>
        /// <returns>System.Int32.</returns>
        public virtual int Execute(
            uint commandId,
            CommandExecutionVerb verb,
            PropertyKeyRef key,
            PropVariantRef currentValue,
            IUISimplePropertySet commandExecutionProperties)
        {
            switch (verb)
            {
                case CommandExecutionVerb.Execute:
                    this.ParentCommandManager.Execute((CommandId)commandId);
                    return HRESULT.S_OK;
                case CommandExecutionVerb.Preview:
                    break;
                case CommandExecutionVerb.CancelPreview:
                    break;
            }

            return HRESULT.S_OK;
        }

        /// <summary>
        /// Updates the property.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>System.Int32.</returns>
        public virtual int UpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            var command = this.ParentCommandManager.Get((CommandId)commandId);
            if (command == null)
            {
                return this.NullCommandUpdateProperty(commandId, ref key, currentValue, out newValue);
            }

            try
            {
                newValue = new PropVariant();
                command.GetPropVariant(key, currentValue, ref newValue);

                if (newValue.IsNull())
                {
                    Trace.Fail($"Didn't property update property for {PropertyKeys.GetName(key)} on command {((CommandId)commandId).ToString()}");

                    newValue = PropVariant.FromObject(currentValue.PropVariant.Value);
                }
            }
            catch (Exception ex)
            {
                Trace.Fail($"Exception in UpdateProperty for {PropertyKeys.GetName(key)} on command {commandId}: {ex}");
                newValue = PropVariant.FromObject(currentValue.PropVariant.Value);
            }

            return HRESULT.S_OK;
        }

        /// <summary>
        /// Nulls the command update property.
        /// </summary>
        /// <param name="commandId">The command identifier.</param>
        /// <param name="key">The key.</param>
        /// <param name="currentValue">The current value.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns>System.Int32.</returns>
        public int NullCommandUpdateProperty(uint commandId, ref PropertyKey key, PropVariantRef currentValue, out PropVariant newValue)
        {
            try
            {
                newValue = new PropVariant();
                if (key == PropertyKeys.Enabled)
                {
                    newValue.SetBool(false);
                }
                else if (key == PropertyKeys.SmallImage)
                {
                    var bitmap = CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "SmallImage");
                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.SmallHighContrastImage)
                {
                    var bitmap =
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(),
                                                                "SmallHighContrastImage") ??
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "SmallImage");

                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.LargeImage)
                {
                    var bitmap = CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "LargeImage");
                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.LargeHighContrastImage)
                {
                    var bitmap =
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(),
                                                                "LargeHighContrastImage") ??
                        CommandResourceLoader.LoadCommandBitmap(((CommandId)commandId).ToString(), "LargeImage");

                    RibbonHelper.CreateImagePropVariant(bitmap, out newValue);
                }
                else if (key == PropertyKeys.Label)
                {
                    var str = $"Command.{((CommandId)commandId).ToString()}.LabelTitle";
                    newValue = new PropVariant(TextHelper.UnescapeNewlines(Res.GetProp(str)) ?? string.Empty);
                }
                else if (key == PropertyKeys.LabelDescription)
                {
                    var str = $"Command.{((CommandId)commandId).ToString()}.LabelDescription";
                    newValue = new PropVariant(Res.GetProp(str) ?? string.Empty);
                }
                else if (key == PropertyKeys.TooltipTitle)
                {
                    var commandName = ((CommandId)commandId).ToString();
                    var str = $"Command.{commandName}.TooltipTitle";
                    newValue = new PropVariant(Res.GetProp(str) ?? (Res.GetProp($"Command.{commandName}.LabelTitle") ?? string.Empty));
                }
                else if (key == PropertyKeys.TooltipDescription)
                {
                    var str = $"Command.{((CommandId)commandId).ToString()}.TooltipDescription";
                    newValue = new PropVariant(Res.GetProp(str) ?? string.Empty);
                }
                else if (key == PropertyKeys.Keytip)
                {
                    newValue = new PropVariant("XXX");
                }
                else if (key == PropertyKeys.ContextAvailable)
                {
                    newValue.SetUInt((uint)ContextAvailability.NotAvailable);
                }
                else if (key == PropertyKeys.Categories)
                {
                    newValue = new PropVariant();
                    newValue.SetIUnknown(currentValue);
                }
                else if (key == PropertyKeys.RecentItems)
                {
                    var currColl = (object[])currentValue.PropVariant.Value;
                    newValue = new PropVariant();
                    newValue.SetSafeArray(currColl);
                    return HRESULT.S_OK;
                }
                else if (key == PropertyKeys.ItemsSource)
                {
                    // This should only be necessary if you have created a gallery in the ribbon markup that you have not yet put into the command manager.
                    var list = new List<IUISimplePropertySet>();

                    Interop.Com.Ribbon.IEnumUnknown enumUnk = new BasicCollection(list);
                    newValue = new PropVariant();
                    newValue.SetIUnknown(enumUnk);
                    return HRESULT.S_OK;
                }
                else if (key == PropertyKeys.StringValue)
                {
                    newValue = new PropVariant(string.Empty);
                }
                else if (key == PropertyKeys.SelectedItem)
                {
                    newValue = new PropVariant(0);
                }
                else if (key == PropertyKeys.DecimalValue)
                {
                    newValue.SetDecimal(new decimal(0));
                }
                else if (key == PropertyKeys.MinValue)
                {
                    newValue.SetDecimal(new decimal(0));
                }
                else if (key == PropertyKeys.MaxValue)
                {
                    newValue.SetDecimal(new decimal(100));
                }
                else if (key == PropertyKeys.Increment)
                {
                    newValue.SetDecimal(new decimal(1));
                }
                else if (key == PropertyKeys.DecimalPlaces)
                {
                    newValue.SetDecimal(new decimal(0));
                }
                else if (key == PropertyKeys.RepresentativeString)
                {
                    newValue.SetString("9999");
                }
                else if (key == PropertyKeys.FormatString)
                {
                    newValue.SetString(string.Empty);
                }
                else if (key == PropertyKeys.StandardColors)
                {
                    newValue = new PropVariant();
                    newValue.SetUIntVector(new uint[] { });
                }
                else if (key == PropertyKeys.StandardColorsTooltips)
                {
                    newValue = new PropVariant();
                    newValue.SetStringVector(new string[] { });
                }
                else if (key == PropertyKeys.BooleanValue)
                {
                    newValue = new PropVariant();
                    newValue.SetBool(false);
                }
                else
                {
                    Trace.Fail($"Didn't properly update property for {PropertyKeys.GetName(key)} on command {((CommandId)commandId)}");
                    newValue = new PropVariant();
                }
            }
            catch (Exception ex)
            {
                Trace.Fail($"Exception in UpdateProperty for {PropertyKeys.GetName(key)} on command {commandId}: {ex}");
                newValue = new PropVariant();
            }

            return HRESULT.S_OK;
        }
    }
}
