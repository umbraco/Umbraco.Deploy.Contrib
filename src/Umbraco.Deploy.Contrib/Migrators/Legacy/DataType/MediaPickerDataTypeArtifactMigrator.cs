using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to update the <see cref="Constants.PropertyEditors.Aliases.MediaPicker" /> editor configuration.
    /// </summary>
    public class MediaPickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerDataTypeArtifactMigrator" /> class.
        /// </summary>
        public MediaPickerDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.MediaPicker)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override IDictionary<string, object> MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            if (fromConfiguration.TryGetValue("startNodeId", out var startNodeIdValue) &&
                (!(startNodeIdValue?.ToString() is string startNodeId) || !Udi.TryParse(startNodeId, out _)))
            {
                // Remove invalid start node ID
                fromConfiguration.Remove("startNodeId");
            }

            return fromConfiguration;
        }
    }
}
