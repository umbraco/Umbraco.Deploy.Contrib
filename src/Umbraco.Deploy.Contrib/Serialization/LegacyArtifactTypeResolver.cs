using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Core.Serialization;

namespace Umbraco.Deploy.Contrib.Connectors.Serialization;

/// <summary>
/// Resolves legacy artifact type names from Umbraco 7.
/// </summary>
public sealed class LegacyArtifactTypeResolver : ArtifactTypeResolverBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyArtifactTypeResolver" /> class.
    /// </summary>
    public LegacyArtifactTypeResolver()
        => MaxVersion = new SemVersion(3, 0, 0);

    /// <inheritdoc />
    protected override string ResolveTypeName(string typeName)
    {
        // v2 to v4
        switch (typeName)
        {
            // Content
            case "Umbraco.Deploy.Artifacts.DocumentArtifact":
                typeName = "Umbraco.Deploy.Artifacts.Content.DocumentArtifact";
                break;
            case "Umbraco.Deploy.Artifacts.MediaArtifact":
                typeName = "Umbraco.Deploy.Artifacts.Content.MediaArtifact";
                break;
            case "Umbraco.Deploy.Artifacts.MemberArtifact":
                typeName = "Umbraco.Deploy.Artifacts.Content.MemberArtifact";
                break;
            // Content types
            case "Umbraco.Deploy.Artifacts.DocumentTypeArtifact":
                typeName = "Umbraco.Deploy.Artifacts.ContentType.DocumentTypeArtifact";
                break;
            case "Umbraco.Deploy.Artifacts.MediaTypeArtifact":
                typeName = "Umbraco.Deploy.Artifacts.ContentType.MediaTypeArtifact";
                break;
            case "Umbraco.Deploy.Artifacts.MemberTypeArtifact":
                typeName = "Umbraco.Deploy.Artifacts.ContentType.MemberTypeArtifact";
                break;
            case "Umbraco.Deploy.Artifacts.RelationTypeArtifact":
                typeName = "Umbraco.Deploy.Artifacts.ContentType.RelationTypeArtifact";
                break;
        }

        // Resolve remaining changes (to later versions) using base implementation
        return base.ResolveTypeName(typeName);
    }
}
