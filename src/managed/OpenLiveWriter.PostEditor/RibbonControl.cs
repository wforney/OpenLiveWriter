// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

namespace OpenLiveWriter.PostEditor
{
    using ApplicationFramework;
    using Commands;
    using HtmlEditor;
    using Localization;

    /// <summary>
    /// The RibbonControl class.
    /// </summary>
    public class RibbonControl
    {
        /// <summary>
        /// The command source
        /// </summary>
        private readonly IHtmlEditorCommandSource commandSource;

        /// <summary>
        /// The component context
        /// </summary>
        private readonly IHtmlEditorComponentContext componentContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RibbonControl"/> class.
        /// </summary>
        /// <param name="componentContext">The component context.</param>
        /// <param name="commandSource">The command source.</param>
        public RibbonControl(IHtmlEditorComponentContext componentContext, IHtmlEditorCommandSource commandSource)
        {
            // Note that this code is *not* called within Mail.
            // Shared canvas commands/code need to go in ContentEditor.

            this.componentContext = componentContext;
            this.commandSource = commandSource;

            componentContext.CommandManager.BeginUpdate();

            componentContext.CommandManager.Add(CommandId.FileMenu, null);

            componentContext.CommandManager.Add(CommandId.HomeTab, null);

            componentContext.CommandManager.Add(
                new GroupCommand(CommandId.ClipboardGroup,
                                 componentContext.CommandManager.Get(CommandId.Paste)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.PublishGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.PostAndPublish)));
            componentContext.CommandManager.Add(new Command(CommandId.ParagraphGroup)); // Has it's own icon
            componentContext.CommandManager.Add(new GroupCommand(CommandId.InsertGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId
                                                                        .InsertPictureFromFile)));

            componentContext.CommandManager.Add(CommandId.InsertTab, null);
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BreaksGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.InsertHorizontalLine)));
            componentContext.CommandManager.Add(
                new GroupCommand(CommandId.TablesGroup,
                                 componentContext.CommandManager.Get(CommandId.InsertTable)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.MediaGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId
                                                                        .InsertPictureFromFile)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.PluginsGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.PluginsGallery)));

            componentContext.CommandManager.Add(new Command(CommandId.BlogProviderTab));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BlogProviderBlogGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.ConfigureWeblog)));
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BlogProviderThemeGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.UpdateWeblogStyle)));

            componentContext.CommandManager.Add(CommandId.PreviewTab, null);

            // Already added PublishGroup
            componentContext.CommandManager.Add(new GroupCommand(CommandId.BrowserGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.UpdateWeblogStyle)));

            // Already added TextEditingGroup
            componentContext.CommandManager.Add(new GroupCommand(CommandId.PreviewGroup,
                                                                 componentContext.CommandManager.Get(
                                                                     CommandId.ClosePreview)));

            componentContext.CommandManager.Add(CommandId.DebugTab, null);
            componentContext.CommandManager.Add(new Command(CommandId.GeneralDebugGroup));
            componentContext.CommandManager.Add(new Command(CommandId.DialogDebugGroup));
            componentContext.CommandManager.Add(new Command(CommandId.TextDebugGroup));
            componentContext.CommandManager.Add(new Command(CommandId.ValidateDebugGroup));

            componentContext.CommandManager.Add(new Command(CommandId.FormatMapGroup));
            componentContext.CommandManager.Add(new Command(CommandId.FormatMapPropertiesGroup));

            componentContext.CommandManager.EndUpdate();
        }

        /// <summary>
        /// Manages the commands.
        /// </summary>
        public void ManageCommands() => this.componentContext.CommandManager.SetEnabled(
            CommandId.SemanticHtmlGallery,
            this.commandSource.CanApplyFormatting(
                CommandId.SemanticHtmlGallery));
    }
}
