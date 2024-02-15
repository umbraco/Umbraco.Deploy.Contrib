using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TextBox" /> and the configuration from Umbraco 7 to <see cref="TextboxConfiguration" />.
    /// </summary>
    public class TextboxDataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<TextboxConfiguration>
    {
        private const string FromEditorAlias = "Umbraco.Textbox";

        /// <summary>
        /// Initializes a new instance of the <see cref="TextboxDataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public TextboxDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TextBox, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override TextboxConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new TextboxConfiguration();

            if (fromConfiguration.TryGetValue("maxChars", out var maxChars) &&
                int.TryParse(maxChars?.ToString(), out var maxCharsValue))
            {
                toConfiguration.MaxChars = maxCharsValue;
            }

            return toConfiguration;
        }
    }
}
