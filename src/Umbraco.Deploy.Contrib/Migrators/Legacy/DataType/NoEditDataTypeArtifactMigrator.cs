using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.Label" />.
    /// </summary>
    public class NoEditDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.NoEdit";

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEditDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public NoEditDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.Label, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);
    }
}
