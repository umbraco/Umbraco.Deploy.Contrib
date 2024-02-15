using System.Collections.Generic;
using Newtonsoft.Json;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.NestedContent" /> editor configuration from Umbraco 7 to <see cref="NestedContentConfiguration" />.
    /// </summary>
    public class NestedContentDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<NestedContentConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="NestedContentDataTypeArtifactMigrator" /> class.
        /// </summary>
        public NestedContentDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.NestedContent)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override NestedContentConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new NestedContentConfiguration();

            if (fromConfiguration.TryGetValue("contentTypes", out var contentTypes) && contentTypes != null)
            {
                toConfiguration.ContentTypes = JsonConvert.DeserializeObject<NestedContentConfiguration.ContentType[]>(contentTypes.ToString());
            }

            if (fromConfiguration.TryGetValue("minItems", out var minItems) &&
                int.TryParse(minItems?.ToString(), out var minItemsValue))
            {
                toConfiguration.MinItems = minItemsValue;
            }

            if (fromConfiguration.TryGetValue("maxItems", out var maxItems) &&
               int.TryParse(maxItems?.ToString(), out var maxItemsValue))
            {
                toConfiguration.MaxItems = maxItemsValue;
            }

            if (fromConfiguration.TryGetValue("confirmDeletes", out var confirmDeletes))
            {
                toConfiguration.ConfirmDeletes = TrueValue.Equals(confirmDeletes);
            }

            if (fromConfiguration.TryGetValue("showIcons", out var showIcons))
            {
                toConfiguration.ShowIcons = TrueValue.Equals(showIcons);
            }

            if (fromConfiguration.TryGetValue("hideLabel", out var hideLabel))
            {
                toConfiguration.HideLabel = TrueValue.Equals(hideLabel);
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override NestedContentConfiguration GetDefaultConfiguration()
            => new NestedContentConfiguration();
    }
}
