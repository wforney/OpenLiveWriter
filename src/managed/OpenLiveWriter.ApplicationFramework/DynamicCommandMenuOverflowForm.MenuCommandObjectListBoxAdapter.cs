// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.ApplicationFramework
{
    public partial class DynamicCommandMenuOverflowForm
    {
        /// <summary>
        /// Class MenuCommandObjectListBoxAdapter.
        /// </summary>
        private class MenuCommandObjectListBoxAdapter
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MenuCommandObjectListBoxAdapter"/> class.
            /// </summary>
            /// <param name="menuCommandObject">The menu command object.</param>
            public MenuCommandObjectListBoxAdapter(IMenuCommandObject menuCommandObject) => this.MenuCommandObject = menuCommandObject;

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
            public override string ToString() => this.MenuCommandObject.CaptionNoMnemonic;

            /// <summary>
            /// Gets the menu command object.
            /// </summary>
            /// <value>The menu command object.</value>
            public IMenuCommandObject MenuCommandObject { get; }
        }
    }
}
