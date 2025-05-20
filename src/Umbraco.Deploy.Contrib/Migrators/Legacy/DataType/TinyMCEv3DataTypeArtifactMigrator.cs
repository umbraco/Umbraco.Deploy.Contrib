using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.RichText" />.
/// </summary>
public class TinyMCEv3DataTypeArtifactMigrator : LegacyReplaceDataTypeArtifactMigratorBase
{
    private const string FromEditorAlias = "Umbraco.TinyMCEv3";

    /// <summary>
    /// Initializes a new instance of the <see cref="TinyMCEv3DataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public TinyMCEv3DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.RichText, DeployConstants.PropertyEditors.UiAliases.Tiptap, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        ReplaceUdiWithGuid(ref configuration, "mediaParentId");
        ReplaceRichTextEditor(ref configuration);
        ReplaceIntegerWithBoolean(ref configuration, Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes);
        configuration.TryAdd("extensions", new[]
        {
            "Umb.Tiptap.Embed",
            "Umb.Tiptap.Link",
            "Umb.Tiptap.Figure",
            "Umb.Tiptap.Image",
            "Umb.Tiptap.Subscript",
            "Umb.Tiptap.Superscript",
            "Umb.Tiptap.Table",
            "Umb.Tiptap.Underline",
            "Umb.Tiptap.TextAlign",
            "Umb.Tiptap.MediaUpload",
        });
        configuration.TryAdd("toolbar", new string[][][]
        {
            [
                [
                    "Umb.Tiptap.Toolbar.SourceEditor",
                ],
                [
                    "Umb.Tiptap.Toolbar.Bold",
                    "Umb.Tiptap.Toolbar.Italic",
                    "Umb.Tiptap.Toolbar.Underline",
                ],
                [
                    "Umb.Tiptap.Toolbar.TextAlignLeft",
                    "Umb.Tiptap.Toolbar.TextAlignCenter",
                    "Umb.Tiptap.Toolbar.TextAlignRight",
                ],
                [
                    "Umb.Tiptap.Toolbar.BulletList",
                    "Umb.Tiptap.Toolbar.OrderedList",
                ],
                [
                    "Umb.Tiptap.Toolbar.Blockquote",
                    "Umb.Tiptap.Toolbar.HorizontalRule",
                ],
                [
                    "Umb.Tiptap.Toolbar.Link",
                    "Umb.Tiptap.Toolbar.Unlink",
                ],
                [
                    "Umb.Tiptap.Toolbar.MediaPicker",
                    "Umb.Tiptap.Toolbar.EmbeddedMedia",
                ],
            ],
        });
        configuration.TryAdd("maxImageSize", 500);
        configuration.TryAdd("overlaySize", "medium");

        return configuration;
    }

    private void ReplaceRichTextEditor(ref IDictionary<string, object> configuration)
    {
        if (TryDeserialize(ref configuration, "editor", out RichTextEditorConfiguration? richTextEditorConfiguration))
        {
            if (richTextEditorConfiguration.Toolbar is { Length: > 0 })
            {
                // TODO: Map TinyMCE toolbar to Tiptap actions
            }

            if (richTextEditorConfiguration.Stylesheets is { Length: > 0 })
            {
                configuration["stylesheets"] = richTextEditorConfiguration.Stylesheets;
            }

            if (richTextEditorConfiguration.MaxImageSize is not null)
            {
                configuration["maxImageSize"] = richTextEditorConfiguration.MaxImageSize;
            }

            if (richTextEditorConfiguration.Dimensions is not null)
            {
                configuration["dimensions"] = richTextEditorConfiguration.Dimensions;
            }

            configuration.Remove("editor");
        }
    }

    private sealed class RichTextEditorConfiguration
    {
        public string[]? Toolbar { get; set; }
        public string[]? Stylesheets { get; set; }
        public int? MaxImageSize { get; set; }
        public string? Mode { get; set; }
        public EditorDimensions? Dimensions { get; set; }
        public sealed class EditorDimensions
        {
            public int? Width { get; set; }
            public int? Height { get; set; }
        }
    }
}
