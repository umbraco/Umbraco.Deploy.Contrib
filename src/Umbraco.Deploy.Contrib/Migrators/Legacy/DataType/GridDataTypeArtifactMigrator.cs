using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.Grid" /> editor configuration from Umbraco 7 to <see cref="GridConfiguration" />.
    /// </summary>
    public class GridDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<GridConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="GridDataTypeArtifactMigrator" /> class.
        /// </summary>
        public GridDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.Grid)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override GridConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new GridConfiguration();

            if (fromConfiguration.TryGetValue("items", out var items) && items != null)
            {
                toConfiguration.Items = JObject.Parse(items.ToString());
            }

            if (fromConfiguration.TryGetValue("rte", out var rte) && rte != null)
            {
                toConfiguration.Rte = JObject.Parse(rte.ToString());
            }

            if (fromConfiguration.TryGetValue("ignoreUserStartNodes", out var ignoreUserStartNodes))
            {
                toConfiguration.IgnoreUserStartNodes = TrueValue.Equals(ignoreUserStartNodes);
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override GridConfiguration GetDefaultConfiguration()
            => new GridConfiguration();
    }
}
