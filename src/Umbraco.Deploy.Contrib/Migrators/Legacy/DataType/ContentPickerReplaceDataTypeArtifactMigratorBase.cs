using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.ContentPicker" /> and update the configuration.
/// </summary>
public abstract class ContentPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPickerReplaceDataTypeArtifactMigratorBase" /> class.
    /// </summary>
    /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    protected ContentPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(fromEditorAlias, Constants.PropertyEditors.Aliases.ContentPicker, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object?>? MigrateConfiguration(IDictionary<string, object?> configuration)
    {
        if (configuration.TryGetValue("startNodeId", out var startNodeIdValue) &&
            (startNodeIdValue?.ToString() is not string startNodeIdString || !UdiParser.TryParse(startNodeIdString, out _)))
        {
            // Remove invalid start node id
            configuration.Remove("startNodeId");
        }

        return base.MigrateConfiguration(configuration);
    }
}
