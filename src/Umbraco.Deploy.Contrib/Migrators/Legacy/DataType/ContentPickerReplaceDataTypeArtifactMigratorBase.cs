using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.ContentPicker" /> and update the configuration.
    /// </summary>
    public abstract class ContentPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPickerReplaceDataTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
        /// <param name="propertyEditors">The property editors.</param>
        protected ContentPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.ContentPicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override IDictionary<string, object> MigrateConfiguration(IDictionary<string, object> configuration)
        {
            if (configuration.TryGetValue("startNodeId", out var startNodeIdValue) &&
                (!(startNodeIdValue?.ToString() is string startNodeIdString) || !Udi.TryParse(startNodeIdString, out _)))
            {
                // Remove invalid start node id
                configuration.Remove("startNodeId");
            }

            return base.MigrateConfiguration(configuration);
        }
    }
}
