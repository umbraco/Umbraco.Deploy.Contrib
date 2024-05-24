using System.Collections.Generic;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to update the <see cref="Constants.PropertyEditors.Aliases.MultiNodeTreePicker" /> editor configuration.
/// </summary>
public class MultiNodeTreePickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultiNodeTreePickerDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public MultiNodeTreePickerDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> fromConfiguration)
    {
        if (fromConfiguration.TryGetValue("startNode", out var startNodeValue) &&
            startNodeValue is JsonObject startNode &&
            startNode["id"] is JsonValue idValue &&
            (idValue.TryGetValue(out string? id) is false || UdiParser.TryParse(id, out _) is false))
        {
            // Remove invalid start node ID
            startNode.Remove("id");
        }

        return fromConfiguration;
    }
}
