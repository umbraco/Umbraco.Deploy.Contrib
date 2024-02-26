using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.MultiUrlPicker" /> and the configuration from Umbraco 7 to <see cref="MultiUrlPickerConfiguration" />.
    /// </summary>
    public class RelatedLinksDataTypeArtifactMigrator : MultiUrlPickerReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.RelatedLinks";

        /// <summary>
        /// Initializes a new instance of the <see cref="RelatedLinksDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public RelatedLinksDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, propertyEditors)
        { }
    }
}
