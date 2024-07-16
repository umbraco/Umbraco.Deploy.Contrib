using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.ColorPicker" /> and the configuration from Umbraco 7 to <see cref="ColorPickerConfiguration" />.
/// </summary>
public class ColorPickerAliasDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<ColorPickerConfiguration>
{
    private const string FromEditorAlias = "Umbraco.ColorPickerAlias";
    private const string TrueValue = "1";

    /// <summary>
    /// Initializes a new instance of the <see cref="ColorPickerAliasDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    public ColorPickerAliasDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.ColorPicker, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override ColorPickerConfiguration? MigrateConfigurationObject(IDictionary<string, object> fromConfiguration)
    {
        var toConfiguration = new ColorPickerConfiguration();

        foreach (var (key, value) in fromConfiguration)
        {
            if (key == "useLabel")
            {
                toConfiguration.UseLabel = TrueValue.Equals(value);
            }
            else if (int.TryParse(key, out _) && value is string itemValue)
            {
                var hex = itemValue.ToLowerInvariant();
                if (hex.Length is 3)
                {
                    hex = string.Join(string.Empty, hex.Select(c => $"{c}{c}"));
                }

                toConfiguration.Items.Add(new ColorPickerConfiguration.ColorPickerItem()
                {
                    Label = hex,
                    Value = hex,
                });
            }
        }

        return toConfiguration;
    }
}
