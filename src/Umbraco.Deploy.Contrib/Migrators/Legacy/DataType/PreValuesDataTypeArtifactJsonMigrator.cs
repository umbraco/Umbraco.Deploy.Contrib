using System.Linq;
using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> JSON from Umbraco 7 pre-values to configuration objects/arrays.
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
            if (artifactJson["PreValues"] is JObject preValues)
            {
                var configuration = new JObject();

                foreach (var property in preValues.Properties())
                {
                    var propertyValue = property.Value;

                    // Convert pre-value serialized JSON to actual JSON objects/arrays
                    if (propertyValue.Type == JTokenType.String &&
                        propertyValue.Value<string>() is string value)
                    {
                        if (string.IsNullOrEmpty(value))
                        {
                            // Skip empty value
                            continue;
                        }
                        else if (value.DetectIsJson())
                        {
                            propertyValue = JToken.Parse(value);
                        }
                    }

                    configuration.Add(property.Name, propertyValue);
                }

                artifactJson["Configuration"] = configuration;
            }

            return artifactJson;
        }
    }
}
