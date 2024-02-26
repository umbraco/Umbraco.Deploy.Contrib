using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to update the <see cref="Constants.PropertyEditors.Aliases.MultiNodeTreePicker" /> editor configuration.
    /// </summary>
    public class MultiNodeTreePickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiNodeTreePickerDataTypeArtifactMigrator" /> class.
        /// </summary>
        public MultiNodeTreePickerDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.MultiNodeTreePicker)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override IDictionary<string, object> MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            if (fromConfiguration.TryGetValue("startNode", out var startNodeValue) &&
                startNodeValue is JObject startNode &&
                startNode["id"] is JValue idValue &&
                (!(idValue.Value?.ToString() is string id) || !Udi.TryParse(id, out _)))
            {
                // Remove invalid start node ID
                startNode.Remove("id");
            }

            return fromConfiguration;
        }
    }
}
