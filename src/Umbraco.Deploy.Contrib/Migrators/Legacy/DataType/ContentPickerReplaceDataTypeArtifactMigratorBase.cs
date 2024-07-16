using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.ContentPicker" /> and update the configuration.
/// </summary>
public abstract class ContentPickerReplaceDataTypeArtifactMigratorBase : LegacyReplaceDataTypeArtifactMigratorBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPickerReplaceDataTypeArtifactMigratorBase" /> class.
    /// </summary>
    /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    protected ContentPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(fromEditorAlias, Constants.PropertyEditors.Aliases.ContentPicker, DeployConstants.PropertyEditors.UiAliases.DocumentPicker, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        ReplaceIntegerWithBoolean(ref configuration, Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes);
        ReplaceUdiWithGuid(ref configuration, "startNodeId");
        ReplaceIntegerWithBoolean(ref configuration, "showOpenButton");

        return configuration;
    }
}
