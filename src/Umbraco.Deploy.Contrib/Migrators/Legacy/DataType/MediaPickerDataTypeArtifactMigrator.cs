using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.MediaPicker" /> editor configuration from Umbraco 7 to <see cref="MediaPickerConfiguration" />.
    /// </summary>
    public class MediaPickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<MediaPickerConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerDataTypeArtifactMigrator" /> class.
        /// </summary>
        public MediaPickerDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.MediaPicker)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override MediaPickerConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new MediaPickerConfiguration();

            if (fromConfiguration.TryGetValue("startNodeId", out var startNodeId) &&
               Udi.TryParse(startNodeId?.ToString(), out var udi))
            {
                toConfiguration.StartNodeId = udi;
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override MediaPickerConfiguration GetDefaultConfiguration()
            => new MediaPickerConfiguration();
    }
}
