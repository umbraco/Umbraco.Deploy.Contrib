using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.DateTime" /> and the configuration from Umbraco 7 to <see cref="DateTimeConfiguration" />.
    /// </summary>
    public class DateDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<DateTimeConfiguration>
    {
        private const string FromEditorAlias = "Umbraco.Date";

        /// <summary>
        /// Initializes a new instance of the <see cref="DateDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public DateDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.DateTime, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override DateTimeConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new DateTimeConfiguration();

            if (fromConfiguration.TryGetValue("format", out var format) && format != null)
            {
                toConfiguration.Format = format.ToString();
            }

            return toConfiguration;
        }
    }
}
