using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TinyMce" />.
/// </summary>
public class TinyMCEv3DataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase
{
    private const string FromEditorAlias = "Umbraco.TinyMCEv3";

    /// <summary>
    /// Initializes a new instance of the <see cref="TinyMCEv3DataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public TinyMCEv3DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TinyMce, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);
}
