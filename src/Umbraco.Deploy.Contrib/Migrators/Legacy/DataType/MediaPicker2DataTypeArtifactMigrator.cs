using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.MediaPicker" /> and the configuration from Umbraco 7 to <see cref="MediaPickerConfiguration" />.
    /// </summary>
    public class MediaPicker2DataTypeArtifactMigrator : MediaPickerReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.MediaPicker2";

        /// <inheritdoc />
        protected override bool Multiple => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPicker2DataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public MediaPicker2DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, propertyEditors)
        { }
    }
}
