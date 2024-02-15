using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.MultiNodeTreePicker" /> and the configuration from Umbraco 7 to <see cref="MultiNodePickerConfiguration" />.
    /// </summary>
    public class MultiNodeTreePicker2DataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<MultiNodePickerConfiguration>
    {
        private const string FromEditorAlias = "Umbraco.MultiNodeTreePicker2";
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiNodeTreePicker2DataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public MultiNodeTreePicker2DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.MultiNodeTreePicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override MultiNodePickerConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new MultiNodePickerConfiguration();

            if (fromConfiguration.TryGetValue("startNode", out var startNode) && startNode != null)
            {
                toConfiguration.TreeSource = new MultiNodePickerConfigurationTreeSource();

                var treeSource = JObject.Parse(startNode.ToString());

                if (treeSource.TryGetValue("type", out var type) && type != null)
                {
                    toConfiguration.TreeSource.ObjectType = type.Value<string>();
                }

                if (treeSource.TryGetValue("query", out var query) && query != null)
                {
                    toConfiguration.TreeSource.StartNodeQuery = query.Value<string>();
                }

                if (treeSource.TryGetValue("id", out var id) &&
                    id != null &&
                    Udi.TryParse(id.Value<string>(), out var udi))
                {
                    toConfiguration.TreeSource.StartNodeId = udi;
                }
            }

            if (fromConfiguration.TryGetValue("filter", out var filter) && filter != null)
            {
                toConfiguration.Filter = filter.ToString();
            }

            if (fromConfiguration.TryGetValue("minNumber", out var minNumber) &&
               int.TryParse(minNumber?.ToString(), out var minNumberValue))
            {
                toConfiguration.MinNumber = minNumberValue;
            }

            if (fromConfiguration.TryGetValue("maxNumber", out var maxNumber) &&
               int.TryParse(maxNumber?.ToString(), out var maxNumberValue))
            {
                toConfiguration.MaxNumber = maxNumberValue;
            }

            if (fromConfiguration.TryGetValue("showOpenButton", out var showOpenButton))
            {
                toConfiguration.ShowOpen = TrueValue.Equals(showOpenButton);
            }

            if (fromConfiguration.TryGetValue("ignoreUserStartNodes", out var ignoreUserStartNodes))
            {
                toConfiguration.IgnoreUserStartNodes = TrueValue.Equals(ignoreUserStartNodes);
            }

            return toConfiguration;
        }
    }
}
