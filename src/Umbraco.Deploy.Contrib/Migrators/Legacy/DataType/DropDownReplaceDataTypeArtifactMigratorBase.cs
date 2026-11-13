using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MultipleDropDown" /> and the configuration from Umbraco 7 to <see cref="DropDownFlexibleConfiguration" />.
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
        : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MultipleDropDown, DeployConstants.PropertyEditors.UiAliases.Dropdown, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override DropDownFlexibleConfiguration? MigrateConfigurationObject(IDictionary<string, object> configuration)
    {
        var toConfiguration = new DropDownFlexibleConfiguration();

        foreach (var (key, value) in configuration)
        {
            if (int.TryParse(key, out _) && value?.ToString() is string itemValue)
            {
                toConfiguration.Items.Add(itemValue);
            }
        }

        return toConfiguration;
    }

    /// <inheritdoc />
    protected override IDictionary<string, object> MigrateConfiguration(IDictionary<string, object> configuration, IConfigurationEditor toConfigurationEditor, IConfigurationEditor? fromConfigurationEditor)
    {
        var migratedConfiguration = base.MigrateConfiguration(configuration, toConfigurationEditor, fromConfigurationEditor);

        // Always write an explicit value, because the dropdown migrators that run afterwards treat a missing key as a single value dropdown
        migratedConfiguration["multiple"] = configuration.TryGetValue("multiple", out var multiple)
            ? TrueValue.Equals(multiple)
            : Multiple;

        return migratedConfiguration;
    }

    /// <inheritdoc />
    protected override IDictionary<string, object> GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
    {
        var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
        configuration["multiple"] = Multiple;

        return configuration;
    }
}
