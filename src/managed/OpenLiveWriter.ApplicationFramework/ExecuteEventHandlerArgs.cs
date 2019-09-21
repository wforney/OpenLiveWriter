// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.Drawing;

    /// <summary>
    /// Class ExecuteEventHandlerArgs.
    /// Implements the <see cref="System.EventArgs" />
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class ExecuteEventHandlerArgs : EventArgs
    {
        /// <summary>
        /// The arguments
        /// </summary>
        private readonly Hashtable args = new Hashtable();

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ExecuteEventHandlerArgs"/> is cancelled.
        /// </summary>
        /// <value><c>true</c> if cancelled; otherwise, <c>false</c>.</value>
        public bool Cancelled { get; set; }

        /// <summary>
        /// Determines whether the specified argument name has argument.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <returns><c>true</c> if the specified argument name has argument; otherwise, <c>false</c>.</returns>
        public bool HasArg(string argName) => this.args.Contains(argName);

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <returns>System.String.</returns>
        public string GetString(string argName) => (string)this.args[argName];

        /// <summary>
        /// Gets the int.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <returns>System.Int32.</returns>
        public int GetInt(string argName) => (int)this.args[argName];

        /// <summary>
        /// Gets the decimal.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <returns>System.Decimal.</returns>
        public decimal GetDecimal(string argName) => (decimal)this.args[argName];

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <returns>Color.</returns>
        public Color GetColor(string argName) => (Color)this.args[argName];

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteEventHandlerArgs"/> class.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="arg">The argument.</param>
        public ExecuteEventHandlerArgs(string argName, string arg) => this.args.Add(argName, arg);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteEventHandlerArgs"/> class.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="arg">The argument.</param>
        public ExecuteEventHandlerArgs(string argName, int arg) => this.args.Add(argName, arg);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteEventHandlerArgs"/> class.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="arg">The argument.</param>
        public ExecuteEventHandlerArgs(string argName, decimal arg) => this.args.Add(argName, arg);

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteEventHandlerArgs"/> class.
        /// </summary>
        public ExecuteEventHandlerArgs()
        {
        }

        /// <summary>
        /// Adds the specified argument name.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="arg">The argument.</param>
        public void Add(string argName, int arg) => this.args.Add(argName, arg);

        /// <summary>
        /// Adds the specified argument name.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="arg">if set to <c>true</c> [argument].</param>
        public void Add(string argName, bool arg) => this.args.Add(argName, arg);

        /// <summary>
        /// Adds the specified argument name.
        /// </summary>
        /// <param name="argName">Name of the argument.</param>
        /// <param name="color">The color.</param>
        public void Add(string argName, Color color) => this.args.Add(argName, color);
    }
}
