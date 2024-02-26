using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MultiUrlPicker" />.
    /// </summary>
    public abstract class MultiUrlPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiUrlPickerReplaceDataTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
        /// <param name="propertyEditors">The property editors.</param>
        protected MultiUrlPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MultiUrlPicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);
    }
}
