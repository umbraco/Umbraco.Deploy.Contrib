using System.Collections.Generic;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MediaPicker3" /> and update the configuration.
/// </summary>
public abstract class MediaPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase
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
        : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MediaPicker3, propertyEditors, configurationEditorJsonSerializer)
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        if (!configuration.ContainsKey("multiPicker"))
        {
            configuration["multiPicker"] = Multiple;
        }

        if (configuration.TryGetValue("startNodeId", out var startNodeIdValue))
        {
            if (startNodeIdValue?.ToString() is not string startNodeId || !UdiParser.TryParse(startNodeId, out GuidUdi? udi))
            {
                // Remove invalid start node ID
                configuration.Remove("startNodeId");
            }
            else
            {
                // Update start node ID to GUID
                configuration["startNodeId"] = udi.Guid;
            }
        }

        return configuration;
    }

    /// <inheritdoc />
    protected override IDictionary<string, object> GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
    {
        var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
        configuration["multiPicker"] = Multiple;

        return configuration;
    }
}
