using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Extensions;

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
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.RichText, DeployConstants.PropertyEditors.UiAliases.TinyMce, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        ReplaceUdiWithGuid(ref configuration, "mediaParentId");
        ReplaceRichTextEditor(ref configuration);
        ReplaceIntegerWithBoolean(ref configuration, Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes);
        configuration.TryAdd("toolbar", new[]
        {
            "style",
            "bold",
            "italic",
            "alignleft",
            "aligncenter",
            "alignright",
            "bullist",
            "numlist",
            "outdent",
            "indent",
            "link",
            "sourcecode",
            "umbmediapicker",
            "umbembeddialog"
        });
        configuration.TryAdd("mode", "Classic");
        configuration.TryAdd("maxImageSize", 500);
        configuration.TryAdd("overlaySize", "small");

        return configuration;
    }

    private void ReplaceRichTextEditor(ref IDictionary<string, object> configuration)
    {
        if (TryDeserialize(ref configuration, "editor", out RichTextEditorConfiguration? richTextEditorConfiguration))
        {
            if (richTextEditorConfiguration.Toolbar is { Length: > 0 })
            {
                // Replace ace with sourcecode
                configuration["toolbar"] = richTextEditorConfiguration.Toolbar.Select(x => x == "ace" ? "sourcecode" : x).ToArray();
            }

            if (richTextEditorConfiguration.Stylesheets is { Length: > 0 })
            {
                configuration["stylesheets"] = richTextEditorConfiguration.Stylesheets;
            }

            if (string.IsNullOrEmpty(richTextEditorConfiguration.Mode) is false)
            {
                configuration["mode"] = richTextEditorConfiguration.Mode.ToFirstUpperInvariant();
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
