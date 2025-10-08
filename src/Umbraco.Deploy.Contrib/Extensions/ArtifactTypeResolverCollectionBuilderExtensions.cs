using Umbraco.Deploy.Contrib.Connectors.Serialization;
using Umbraco.Deploy.Core.Serialization;

namespace Umbraco.Extensions;

public static class ArtifactTypeResolverCollectionBuilderExtensions
{
    /// <summary>
    /// Adds/inserts the legacy artifact type resolver to allow importing from Umbraco 7.
    /// </summary>
    /// <param name="artifactTypeResolverCollectionBuilder">The artifact type resolver collection builder.</param>
    /// <returns>
    /// The artifact type resolver collection builder.
    /// </returns>
    /// <remarks>
    /// The legacy artifact type resolver is inserted at the beginning of the collection to ensure it runs before any other resolvers.
    /// </remarks>
    public static ArtifactTypeResolverCollectionBuilder AddLegacyTypeResolver(this ArtifactTypeResolverCollectionBuilder artifactTypeResolverCollectionBuilder)
        => artifactTypeResolverCollectionBuilder.Insert<LegacyArtifactTypeResolver>();
}
