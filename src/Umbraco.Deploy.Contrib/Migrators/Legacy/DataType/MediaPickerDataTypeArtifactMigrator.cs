using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to update the <see cref="EditorAlias" /> editor configuration.
/// </summary>
public class MediaPickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase
{
    private const string EditorAlias = "Umbraco.MediaPicker"; // TODO: Use DeployConstants.PropertyEditors.Legacy.Aliases.MediaPicker once made public

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPickerDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public MediaPickerDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(EditorAlias, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> fromConfiguration)
    {
        if (fromConfiguration.TryGetValue("startNodeId", out var startNodeIdValue) &&
            (startNodeIdValue?.ToString() is not string startNodeId || !UdiParser.TryParse(startNodeId, out _)))
        {
            // Remove invalid start node ID
            fromConfiguration.Remove("startNodeId");
        }

        return fromConfiguration;
    }
}
