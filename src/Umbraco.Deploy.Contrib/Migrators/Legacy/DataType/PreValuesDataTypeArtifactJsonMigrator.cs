using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> JSON from Umbraco 7 pre-values to configuration.
    /// </summary>
    public class PreValuesDataTypeArtifactJsonMigrator : ArtifactJsonMigratorBase<DataTypeArtifact>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PreValuesDataTypeArtifactJsonMigrator" /> class.
        /// </summary>
        public PreValuesDataTypeArtifactJsonMigrator()
          => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        public override JToken Migrate(JToken artifactJson)
        {
            var preValues = artifactJson["PreValues"];
            if (preValues != null)
            {
                artifactJson["Configuration"] = preValues;

                preValues.Remove();
            }

            return artifactJson;
        }
    }
}
