// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

// @RIBBON TODO: Cleanly remove those aspects of the command class that are obviated by the ribbon.

namespace OpenLiveWriter.ApplicationFramework
{
    using System;
    using System.Collections;
    using System.Drawing;

    public class ExecuteEventHandlerArgs : EventArgs
    {
        private Hashtable args = new Hashtable();

        public bool Cancelled { get; set; }

        public bool HasArg(string argName) => this.args.Contains(argName);

        public string GetString(string argName) => (string)this.args[argName];

        public int GetInt(string argName) => (int)this.args[argName];

        public decimal GetDecimal(string argName) => (decimal)this.args[argName];

        public Color GetColor(string argName) => (Color)this.args[argName];

        public ExecuteEventHandlerArgs(string argName, string arg)
        {
            this.args.Add(argName, arg);
        }

        public ExecuteEventHandlerArgs(string argName, int arg)
        {
            this.args.Add(argName, arg);
        }

        public ExecuteEventHandlerArgs(string argName, decimal arg)
        {
            this.args.Add(argName, arg);
        }

        public ExecuteEventHandlerArgs()
        {
        }

        public void Add(string argName, int arg) => this.args.Add(argName, arg);

        public void Add(string argName, bool arg) => this.args.Add(argName, arg);

        public void Add(string argName, Color color) => this.args.Add(argName, color);
    }
}
