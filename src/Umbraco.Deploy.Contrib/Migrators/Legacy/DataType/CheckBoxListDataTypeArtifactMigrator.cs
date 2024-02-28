using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.CheckBoxList" /> editor configuration from Umbraco 7 to <see cref="ValueListConfiguration" />.
/// </summary>
public class CheckBoxListDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<ValueListConfiguration>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CheckBoxListDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public CheckBoxListDataTypeArtifactMigrator(IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(Constants.PropertyEditors.Aliases.CheckBoxList, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override ValueListConfiguration? MigrateConfiguration(IDictionary<string, object?> fromConfiguration)
    {
        var toConfiguration = new ValueListConfiguration();

        foreach (var (key, value) in fromConfiguration)
        {
            if (int.TryParse(key, out var id) && value is not null)
            {
                toConfiguration.Items.Add(new ValueListConfiguration.ValueListItem()
                {
                    Id = id,
                    Value = value.ToString()
                });
            }
        }

        return toConfiguration;
    }

    /// <inheritdoc />
    protected override ValueListConfiguration? GetDefaultConfiguration()
        => new ValueListConfiguration();
}
