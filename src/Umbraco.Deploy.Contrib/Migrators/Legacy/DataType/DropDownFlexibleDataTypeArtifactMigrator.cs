using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the configuration of the <see cref="Constants.PropertyEditors.Aliases.DropDownListFlexible" /> editor from Umbraco 7 to <see cref="DropDownFlexibleConfiguration" />.
/// </summary>
public class DropDownFlexibleDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<DropDownFlexibleConfiguration>
{
    private const string TrueValue = "1";

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownFlexibleDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public DropDownFlexibleDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(Constants.PropertyEditors.Aliases.DropDownListFlexible, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override DropDownFlexibleConfiguration? MigrateConfigurationObject(IDictionary<string, object> fromConfiguration)
    {
        var toConfiguration = new DropDownFlexibleConfiguration()
        {
            Multiple = true
        };

        foreach (var (key, value) in fromConfiguration)
        {
            if (key == "multiple")
            {
                toConfiguration.Multiple = TrueValue.Equals(value);
            }
            else if (int.TryParse(key, out _) && value is string itemValue)
            {
                toConfiguration.Items.Add(itemValue);
            }
        }

        return toConfiguration;
    }

    /// <inheritdoc />
    protected override IDictionary<string, object> GetDefaultConfiguration(IConfigurationEditor configurationEditor)
    {
        var configuration = base.GetDefaultConfiguration(configurationEditor);
        configuration["multiple"] = true;

        return configuration;
    }
}
