using System;
using System.Text.Json.Nodes;
using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Infrastructure.Artifacts.ContentType;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="ContentTypeArtifactBase" /> JSON from Umbraco 7 allowed at root and child content types to permissions.
/// </summary>
public class ContentTypeArtifactJsonMigrator : ArtifactJsonMigratorBase<ContentTypeArtifactBase>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ContentTypeArtifactJsonMigrator" /> class.
    /// </summary>
    public ContentTypeArtifactJsonMigrator()
        => MaxVersion = new SemVersion(3, 0, 0);

    protected override bool CanMigrateType(Type type)
        => type.Inherits<ContentTypeArtifactBase>();

    /// <inheritdoc />
    public override JsonNode Migrate(JsonNode artifactJson)
    {
        artifactJson["Permissions"] = new JsonObject()
        {
            ["AllowedAtRoot"] = artifactJson["AllowedAtRoot"]?.DeepClone(),
            ["AllowedChildContentTypes"] = artifactJson["AllowedChildContentTypes"]?.DeepClone(),
        };

        return artifactJson;
    }
}
