using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TextArea" /> and the configuration from Umbraco 7 to <see cref="TextAreaConfiguration" />.
    /// </summary>
    public class TextboxMultipleDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<TextAreaConfiguration>
    {
        private const string FromEditorAlias = "Umbraco.TextboxMultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxMultipleDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public TextboxMultipleDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TextArea, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override TextAreaConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new TextAreaConfiguration();

            if (fromConfiguration.TryGetValue("maxChars", out var maxChars) &&
                int.TryParse(maxChars?.ToString(), out var maxCharsValue))
            {
                toConfiguration.MaxChars = maxCharsValue;
            }

            if (fromConfiguration.TryGetValue("rows", out var rows) &&
                int.TryParse(rows?.ToString(), out var rowsValue))
            {
                toConfiguration.Rows = rowsValue;
            }

            return toConfiguration;
        }
    }
}
