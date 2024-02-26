using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TextArea" />.
    /// </summary>
    public class TextboxMultipleDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.TextboxMultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxMultipleDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public TextboxMultipleDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TextArea, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);
    }
}
