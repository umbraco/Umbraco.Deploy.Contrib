using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DocumentArtifact" /> JSON from Umbraco 7 release/expire date to schedule.
/// </summary>
public class DocumentArtifactJsonMigrator : ArtifactJsonMigratorBase<DocumentArtifact>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentArtifactJsonMigrator" /> class.
    /// </summary>
    public DocumentArtifactJsonMigrator()
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    public override JsonNode Migrate(JsonNode artifactJson)
    {
        var schedule = new JsonArray();

        if (artifactJson["ReleaseDate"] is JsonNode releaseDate)
        {
            schedule.Add(new JsonObject()
            {
                ["Date"] = releaseDate.DeepClone(),
                ["Culture"] = string.Empty,
                ["Action"] = nameof(ContentScheduleAction.Release)
            });
        }

        if (artifactJson["ExpireDate"] is JsonNode expireDate)
        {
            schedule.Add(new JsonObject()
            {
                ["Date"] = expireDate.DeepClone(),
                ["Culture"] = string.Empty,
                ["Action"] = nameof(ContentScheduleAction.Expire)
            });
        }

        artifactJson["Schedule"] = schedule;

        return artifactJson;
    }
}
