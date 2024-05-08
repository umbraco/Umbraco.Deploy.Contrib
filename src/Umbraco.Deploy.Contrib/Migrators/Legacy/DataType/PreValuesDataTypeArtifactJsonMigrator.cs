using System.Text.Json;
using System.Text.Json.Nodes;
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
    public override JsonNode Migrate(JsonNode artifactJson)
    {
        if (artifactJson["PreValues"] is JsonObject preValues)
        {
            var configuration = new JsonObject();

            foreach (var preValue in preValues)
            {
                var value = preValue.Value;

                // Convert pre-value serialized JSON to actual JSON objects/arrays
                if (value is JsonValue jsonValue &&
                    jsonValue.TryGetValue(out string? json) &&
                    json.DetectIsJson())
                {
                    value = JsonNode.Parse(json);
                }

                configuration.Add(preValue.Key, value);
            }

            artifactJson["Configuration"] = configuration;
        }

        return artifactJson;
    }
}
