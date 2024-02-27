using Umbraco.Deploy.Contrib.Connectors.Serialization;
using Umbraco.Deploy.Serialization;

namespace Umbraco.Extensions
{
    public static class ArtifactTypeResolverCollectionBuilderExtensions
    {
        /// <summary>
        /// Adds the legacy artifact type resolver to allow importing from Umbraco 7.
        /// </summary>
        /// <param name="artifactTypeResolverCollectionBuilder">The artifact type resolver collection builder.</param>
        /// <returns>
        /// The artifact type resolver collection builder.
        /// </returns>
        public static ArtifactTypeResolverCollectionBuilder AddLegacyTypeResolver(this ArtifactTypeResolverCollectionBuilder artifactTypeResolverCollectionBuilder)
            => artifactTypeResolverCollectionBuilder.Append<LegacyArtifactTypeResolver>();
    }
}
