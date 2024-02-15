using Umbraco.Core.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.ContentPicker" /> and the configuration from Umbraco 7 to <see cref="ContentPickerConfiguration" />.
    /// </summary>
    public class ContentPickerAliasDataTypeArtifactMigrator : ContentPickerReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.ContentPickerAlias";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPickerAliasDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public ContentPickerAliasDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, propertyEditors)
        { }
    }
}
