using Umbraco.Core.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.DropDownListFlexible" /> and the configuration from Umbraco 7 to <see cref="DropDownFlexibleConfiguration" />.
    /// </summary>
    public class DropDownDataTypeArtifactMigrator : DropDownReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.DropDown";

        /// <inheritdoc />
        protected override bool Multiple => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public DropDownDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, propertyEditors)
        { }
    }
}
