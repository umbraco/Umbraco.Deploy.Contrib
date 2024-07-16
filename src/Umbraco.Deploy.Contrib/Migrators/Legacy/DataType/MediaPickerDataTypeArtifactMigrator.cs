using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to update the <see cref="EditorAlias" /> editor configuration.
/// </summary>
[Obsolete("This has been replaced by ReplaceMediaPickerDataTypeArtifactMigrator and DefaultLegacyDataTypeConfigurationArtifactMigrator in Deploy.")]
public class MediaPickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase
{
    private const string EditorAlias = DeployConstants.PropertyEditors.Legacy.Aliases.MediaPicker;

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
        => fromConfiguration;
}
