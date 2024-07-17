using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TextArea" />.
/// </summary>
public class TextboxMultipleDataTypeArtifactMigrator : LegacyReplaceDataTypeArtifactMigratorBase
{
    private const string FromEditorAlias = "Umbraco.TextboxMultiple";

    /// <summary>
    /// Initializes a new instance of the <see cref="TextboxMultipleDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public TextboxMultipleDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TextArea, DeployConstants.PropertyEditors.UiAliases.TextArea, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        ReplaceStringWithInteger(ref configuration, "maxChars");
        ReplaceStringWithInteger(ref configuration, "rows");

        return configuration;
    }
}
