using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Core.Models;
using Umbraco.Deploy.Artifacts.Content;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
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
        public override JToken Migrate(JToken artifactJson)
        {
            var schedule = new JArray();

            if (artifactJson["ReleaseDate"] is JValue releaseDate &&
                releaseDate.Value != null)
            {
                schedule.Add(new JObject()
                {
                    ["Date"] = releaseDate.Value<string>(),
                    ["Culture"] = string.Empty,
                    ["Action"] = nameof(ContentScheduleAction.Release)
                });
            }

            if (artifactJson["ExpireDate"] is JValue expireDate &&
                expireDate.Value != null)
            {
                schedule.Add(new JObject()
                {
                    ["Date"] = expireDate,
                    ["Culture"] = string.Empty,
                    ["Action"] = nameof(ContentScheduleAction.Expire)
                });
            }

            artifactJson["Schedule"] = schedule;

            return artifactJson;
        }
    }
}
