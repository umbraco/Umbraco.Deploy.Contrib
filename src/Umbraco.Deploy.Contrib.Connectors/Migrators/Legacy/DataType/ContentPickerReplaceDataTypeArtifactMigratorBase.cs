using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.ContentPicker" /> and the configuration from Umbraco 7 to <see cref="ContentPickerConfiguration" />.
    /// </summary>
    public abstract class ContentPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase<ContentPickerConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentPickerReplaceDataTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
        /// <param name="propertyEditors">The property editors.</param>
        protected ContentPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.ContentPicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override ContentPickerConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new ContentPickerConfiguration();

            if (fromConfiguration.TryGetValue("showOpenButton", out var showOpenButton))
            {
                toConfiguration.ShowOpenButton = TrueValue.Equals(showOpenButton);
            }

            if (fromConfiguration.TryGetValue("startNodeId", out var startNodeId) &&
                Udi.TryParse(startNodeId?.ToString(), out var udi))
            {
                toConfiguration.StartNodeId = udi;
            }

            if (fromConfiguration.TryGetValue("ignoreUserStartNodes", out var ignoreUserStartNodes))
            {
                toConfiguration.IgnoreUserStartNodes = TrueValue.Equals(ignoreUserStartNodes);
            }

            return toConfiguration;
        }
    }
}
