using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.Label" /> and the configuration from Umbraco 7 to <see cref="LabelConfiguration" />.
    /// </summary>
    public class NoEditDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<LabelConfiguration>
    {
        private const string FromEditorAlias = "Umbraco.NoEdit";

        /// <summary>
        /// Initializes a new instance of the <see cref="NoEditDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public NoEditDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.Label, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override LabelConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new LabelConfiguration();

            if (fromConfiguration.TryGetValue("umbracoDataValueType", out var umbracoDataValueType) && umbracoDataValueType != null)
            {
                toConfiguration.ValueType = umbracoDataValueType.ToString();
            }

            return toConfiguration;
        }
    }
}
