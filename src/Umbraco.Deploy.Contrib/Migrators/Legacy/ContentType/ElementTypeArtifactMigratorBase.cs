using System.Linq;
using Semver;
using Umbraco.Deploy.Artifacts.ContentType;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DocumentTypeArtifact" /> from Umbraco 7 to an element type.
    /// </summary>
    public abstract class ElementTypeArtifactMigratorBase : ArtifactMigratorBase<DocumentTypeArtifact>
    {
        private readonly string[] _elementTypeAliases;

        /// <summary>
        /// Initializes a new instance of the <see cref="ElementTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="elementTypeAliases">The element type aliases.</param>
        protected ElementTypeArtifactMigratorBase(params string[] elementTypeAliases)
        {
            _elementTypeAliases = elementTypeAliases;

            MaxVersion = new SemVersion(3, 0, 0);
        }

        /// <inheritdoc />
        protected override DocumentTypeArtifact Migrate(DocumentTypeArtifact artifact)
        {
            if (_elementTypeAliases.Contains(artifact.Alias))
            {
                artifact.Permissions.IsElementType = true;
            }

            return artifact;
        }
    }
}
