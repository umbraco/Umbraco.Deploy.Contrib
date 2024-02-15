using System;
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

            var releaseDate = artifactJson["ReleaseDate"];
            if (releaseDate != null)
            {
                if (DateTime.TryParse(releaseDate.Value<string>(), out var releaseDateValue))
                {
                    schedule.Add(new JObject()
                    {
                        ["Date"] = releaseDateValue,
                        ["Culture"] = string.Empty,
                        ["Action"] = nameof(ContentScheduleAction.Release)
                    });
                }

                releaseDate.Remove();
            }

            var expireDate = artifactJson["ExpireDate"];
            if (expireDate != null)
            {
                if (DateTime.TryParse(expireDate.Value<string>(), out var expireDateValue))
                {
                    schedule.Add(new JObject()
                    {
                        ["Date"] = expireDateValue,
                        ["Culture"] = string.Empty,
                        ["Action"] = nameof(ContentScheduleAction.Expire)
                    });
                }

                expireDate.Remove();
            }

            artifactJson["Schedule"] = schedule;

            return artifactJson;
        }
    }
}
