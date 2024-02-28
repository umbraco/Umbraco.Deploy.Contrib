using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

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
                    propertyValue.Value<string>() is string json &&
                    json.DetectIsJson())
                {
                    propertyValue = JToken.Parse(json);
                }

                configuration.Add(property.Name, propertyValue);
            }

            artifactJson["Configuration"] = configuration;
        }

        return artifactJson;
    }
}
