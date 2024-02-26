using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.DropDownListFlexible" /> and the configuration from Umbraco 7 to <see cref="DropDownFlexibleConfiguration" />.
    /// </summary>
    public class DropdownlistMultiplePublishKeysDataTypeArtifactMigrator : DropDownReplaceDataTypeArtifactMigratorBase
    {
        private const string FromEditorAlias = "Umbraco.DropdownlistMultiplePublishKeys";

        /// <inheritdoc />
        protected override bool Multiple => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropdownlistMultiplePublishKeysDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public DropdownlistMultiplePublishKeysDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, propertyEditors)
        { }
    }
}
