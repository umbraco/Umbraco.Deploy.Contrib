using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.DropDownListFlexible" /> and the configuration from Umbraco 7 to <see cref="DropDownFlexibleConfiguration" />.
/// </summary>
public abstract class DropDownReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase<DropDownFlexibleConfiguration>
{
    private const string TrueValue = "1";

    /// <summary>
    /// Gets a value indicating whether the configuration allows multiple items to be selected.
    /// </summary>
    /// <value>
    ///   <c>true</c> if multiple items can be selected; otherwise, <c>false</c>.
    /// </value>
    protected abstract bool Multiple { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownReplaceDataTypeArtifactMigratorBase" /> class.
    /// </summary>
    /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    protected DropDownReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(fromEditorAlias, Constants.PropertyEditors.Aliases.DropDownListFlexible, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override DropDownFlexibleConfiguration? MigrateConfigurationObject(IDictionary<string, object> fromConfiguration)
    {
        var toConfiguration = new DropDownFlexibleConfiguration()
        {
            Multiple = Multiple
        };

        foreach (var (key, value) in fromConfiguration)
        {
            if (key == "multiple")
            {
                toConfiguration.Multiple = TrueValue.Equals(value);
            }
            else if (int.TryParse(key, out _) && value?.ToString() is string itemValue)
            {
                toConfiguration.Items.Add(itemValue);
            }
        }

        return toConfiguration;
    }

    /// <inheritdoc />
    protected override IDictionary<string, object> GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
    {
        var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
        configuration["multiple"] = Multiple;

        return configuration;
    }
}
