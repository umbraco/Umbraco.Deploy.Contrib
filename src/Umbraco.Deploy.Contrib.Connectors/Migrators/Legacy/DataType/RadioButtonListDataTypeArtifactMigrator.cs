using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the configuration of the <see cref="Constants.PropertyEditors.Aliases.RadioButtonList" /> editor from Umbraco 7 to <see cref="ValueListConfiguration" />.
    /// </summary>
    public class RadioButtonListDataTypeArtifactMigrator : DataTypeConfigurationArtifactMigratorBase<ValueListConfiguration>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButtonListDataTypeArtifactMigrator" /> class.
        /// </summary>
        public RadioButtonListDataTypeArtifactMigrator()
            : base(Constants.PropertyEditors.Aliases.RadioButtonList)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override DataTypeArtifact Migrate(DataTypeArtifact artifact)
        {
            artifact.DatabaseType = ValueStorageType.Nvarchar;

            return base.Migrate(artifact);
        }

        /// <inheritdoc />
        protected override ValueListConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new ValueListConfiguration();

            foreach (var (key, value) in fromConfiguration)
            {
                if (int.TryParse(key, out var id) && value != null)
                {
                    toConfiguration.Items.Add(new ValueListConfiguration.ValueListItem()
                    {
                        Id = id,
                        Value = value.ToString()
                    });
                }
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override ValueListConfiguration GetDefaultConfiguration()
            => new ValueListConfiguration();
    }
}
