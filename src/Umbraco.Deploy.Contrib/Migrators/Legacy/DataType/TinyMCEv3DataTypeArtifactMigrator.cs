using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TinyMce" />.
    /// </summary>
    public class TinyMCEv3DataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.TinyMCEv3";

        /// <summary>
        /// Initializes a new instance of the <see cref="TinyMCEv3DataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public TinyMCEv3DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TinyMce, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);
    }
}
