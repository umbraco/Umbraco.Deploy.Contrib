using Umbraco.Deploy.Artifacts.ContentType;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DocumentTypeArtifact" /> JSON from Umbraco 7 allowed at root and child content types to permissions.
    /// </summary>
    public class DocumentTypeArtifactJsonMigrator : ContentTypeArtifactJsonMigratorBase<DocumentTypeArtifact>
    { }
}
