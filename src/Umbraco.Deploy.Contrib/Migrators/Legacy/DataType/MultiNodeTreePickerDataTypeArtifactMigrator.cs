using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
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
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public MultiNodeTreePickerDataTypeArtifactMigrator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(Constants.PropertyEditors.Aliases.MultiNodeTreePicker, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object?>? MigrateConfiguration(IDictionary<string, object?> fromConfiguration)
    {
        if (fromConfiguration.TryGetValue("startNode", out var startNodeValue) &&
            startNodeValue is JObject startNode &&
            startNode["id"] is JValue idValue &&
            (idValue.Value?.ToString() is not string id || !UdiParser.TryParse(id, out _)))
        {
            // Remove invalid start node ID
            startNode.Remove("id");
        }

        return fromConfiguration;
    }
}
