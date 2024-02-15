using System.Collections.Generic;
using Newtonsoft.Json;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.MultiNodeTreePicker" /> editor configuration from Umbraco 7 to <see cref="MultiNodePickerConfiguration" />.
    /// </summary>
    public class MultiNodeTreePickerDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<MultiNodePickerConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiNodeTreePickerDataTypeArtifactMigrator" /> class.
        /// </summary>
        public MultiNodeTreePickerDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.MultiNodeTreePicker)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override MultiNodePickerConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new MultiNodePickerConfiguration();

            if (fromConfiguration.TryGetValue("startNode", out var startNode) && startNode != null)
            {
                toConfiguration.TreeSource = JsonConvert.DeserializeObject<MultiNodePickerConfigurationTreeSource>(startNode.ToString());
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

        /// <inheritdoc />
        protected override MultiNodePickerConfiguration GetDefaultConfiguration()
            => new MultiNodePickerConfiguration();
    }
}
