using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Deploy.Artifacts.ContentType;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <typeparamref name="T"/> JSON from Umbraco 7 allowed at root and child content types to permissions.
    /// </summary>
    /// <typeparam name="T">The content type artifact.</typeparam>
    public abstract class ContentTypeArtifactJsonMigratorBase<T> : ArtifactJsonMigratorBase<T>
        where T : ContentTypeArtifactBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentTypeArtifactJsonMigratorBase{T}" /> class.
        /// </summary>
        public ContentTypeArtifactJsonMigratorBase()
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        public override JToken Migrate(JToken artifactJson)
        {
            var permissions = new JObject();

            var allowedAtRoot = artifactJson["AllowedAtRoot"];
            if (allowedAtRoot != null)
            {
                permissions["AllowedAtRoot"] = allowedAtRoot.Value<bool>();

                allowedAtRoot.Remove();
            }

            var allowedChildContentTypes = artifactJson["AllowedChildContentTypes"];
            if (allowedChildContentTypes != null)
            {
                permissions["AllowedChildContentTypes"] = allowedChildContentTypes;

                allowedChildContentTypes.Remove();
            }

            artifactJson["Permissions"] = permissions;

            return artifactJson;
        }
    }
}
