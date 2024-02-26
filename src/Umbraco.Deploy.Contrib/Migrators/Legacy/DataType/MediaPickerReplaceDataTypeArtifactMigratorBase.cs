using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MediaPicker" /> and update the configuration.
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
        protected MediaPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MediaPicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override IDictionary<string, object> MigrateConfiguration(IDictionary<string, object> configuration)
        {
            configuration["multiPicker"] = Multiple;

            if (configuration.TryGetValue("startNodeId", out var startNodeIdValue) &&
                (!(startNodeIdValue?.ToString() is string startNodeId) || !Udi.TryParse(startNodeId, out _)))
            {
                // Remove invalid start node ID
                configuration.Remove("startNodeId");
            }

            return configuration;
        }

        /// <inheritdoc />
        protected override IDictionary<string, object> GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
        {
            var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
            if (configuration != null)
            {
                configuration["multiPicker"] = Multiple;
            }

            return configuration;
        }
    }
}
