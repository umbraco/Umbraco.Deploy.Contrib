using System.Collections.Generic;
using Newtonsoft.Json;
using Semver;
using Umbraco.Core;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="Constants.PropertyEditors.Aliases.ListView" /> editor configuration from Umbraco 7 to <see cref="ListViewConfiguration" />.
    /// </summary>
    public class ListViewDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<ListViewConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewDataTypeArtifactMigrator" /> class.
        /// </summary>
        public ListViewDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.ListView)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override ListViewConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new ListViewConfiguration();

            if (fromConfiguration.TryGetValue("includeProperties", out var includeProperties) && includeProperties != null)
            {
                toConfiguration.IncludeProperties = JsonConvert.DeserializeObject<ListViewConfiguration.Property[]>(includeProperties.ToString());
            }

            if (fromConfiguration.TryGetValue("layouts", out var layouts) && layouts != null)
            {
                toConfiguration.Layouts = JsonConvert.DeserializeObject<ListViewConfiguration.Layout[]>(layouts.ToString());
            }

            if (fromConfiguration.TryGetValue("orderBy", out var orderBy) && orderBy != null)
            {
                toConfiguration.OrderBy = orderBy.ToString();
            }

            if (fromConfiguration.TryGetValue("orderDirection", out var orderDirection) && orderDirection != null)
            {
                toConfiguration.OrderDirection = orderDirection.ToString();
            }

            if (fromConfiguration.TryGetValue("pageSize", out var pageSize) &&
               int.TryParse(pageSize?.ToString(), out var pageSizeValue))
            {
                toConfiguration.PageSize = pageSizeValue;
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override ListViewConfiguration GetDefaultConfiguration()
            => new ListViewConfiguration();
    }
}
