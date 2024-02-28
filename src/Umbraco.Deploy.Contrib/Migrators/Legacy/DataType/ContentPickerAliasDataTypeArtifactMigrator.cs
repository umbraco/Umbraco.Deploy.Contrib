using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.ContentPicker" /> and the configuration from Umbraco 7 to <see cref="ContentPickerConfiguration" />.
/// </summary>
public class ContentPickerAliasDataTypeArtifactMigrator : ContentPickerReplaceDataTypeArtifactMigratorBase
{
    private const string FromEditorAlias = "Umbraco.ContentPickerAlias";

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentPickerAliasDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public ContentPickerAliasDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(FromEditorAlias, propertyEditors, configurationEditorJsonSerializer)
    { }
}
