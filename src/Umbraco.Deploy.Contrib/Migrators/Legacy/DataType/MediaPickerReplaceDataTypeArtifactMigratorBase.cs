using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MediaPicker3" /> and update the configuration.
/// </summary>
public abstract class MediaPickerReplaceDataTypeArtifactMigratorBase : LegacyReplaceDataTypeArtifactMigratorBase
{
    /// <summary>
    /// Gets a value indicating whether the configuration allows multiple items to be picked.
    /// </summary>
    /// <value>
    ///   <c>true</c> if multiple items can be picked; otherwise, <c>false</c>.
    /// </value>
    protected abstract bool Multiple { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaPickerReplaceDataTypeArtifactMigratorBase" /> class.
    /// </summary>
    /// <param name="fromEditorAlias">From editor alias.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    protected MediaPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MediaPicker3, DeployConstants.PropertyEditors.UiAliases.MediaPicker, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        ReplaceKey(ref configuration, "multiPicker", "multiple");
        ReplaceIntegerWithBoolean(ref configuration, "multiple");
        if (!configuration.ContainsKey("multiple"))
        {
            configuration["multiple"] = Multiple;
        }

        ReplaceUdiWithGuid(ref configuration, "startNodeId");
        ReplaceIntegerWithBoolean(ref configuration, Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes);

        return configuration;
    }

    /// <inheritdoc />
    protected override IDictionary<string, object> GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
    {
        var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
        configuration["multiple"] = Multiple;

        return configuration;
    }
}
