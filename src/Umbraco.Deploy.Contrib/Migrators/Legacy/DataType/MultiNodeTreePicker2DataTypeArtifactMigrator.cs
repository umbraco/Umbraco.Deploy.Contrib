using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.MultiNodeTreePicker" /> and update the configuration.
    /// </summary>
    public class MultiNodeTreePicker2DataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.MultiNodeTreePicker2";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiNodeTreePicker2DataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public MultiNodeTreePicker2DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.MultiNodeTreePicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override IDictionary<string, object> MigrateConfiguration(IDictionary<string, object> configuration)
        {
            if (configuration.TryGetValue("startNode", out var startNodeValue) &&
                startNodeValue is JObject startNode &&
                startNode["id"] is JValue idValue &&
                (!(idValue.Value?.ToString() is string id) || !Udi.TryParse(id, out _)))
            {
                // Remove invalid start node ID
                startNode.Remove("id");
            }

            return configuration;
        }
    }
}
