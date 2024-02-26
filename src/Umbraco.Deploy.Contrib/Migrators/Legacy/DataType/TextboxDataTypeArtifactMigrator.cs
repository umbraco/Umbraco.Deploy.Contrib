using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TextBox" />.
    /// </summary>
    public class TextboxDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.Textbox";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public TextboxDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TextBox, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);
    }
}
