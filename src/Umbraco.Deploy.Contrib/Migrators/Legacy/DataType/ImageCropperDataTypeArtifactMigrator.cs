using System.Collections.Generic;
using Newtonsoft.Json;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.ImageCropper" /> editor configuration from Umbraco 7 to <see cref="ImageCropperConfiguration" />.
    /// </summary>
    public class ImageCropperDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<ImageCropperConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImageCropperDataTypeArtifactMigrator" /> class.
        /// </summary>
        public ImageCropperDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.ImageCropper)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override ImageCropperConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new ImageCropperConfiguration();

            if (fromConfiguration.TryGetValue("crops", out var crops) && crops != null)
            {
                toConfiguration.Crops = JsonConvert.DeserializeObject<ImageCropperConfiguration.Crop[]>(crops.ToString());
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override ImageCropperConfiguration GetDefaultConfiguration()
            => new ImageCropperConfiguration();
    }
}
