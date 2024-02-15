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
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.TinyMce" /> and the configuration from Umbraco 7 to <see cref="RichTextConfiguration" />.
    /// </summary>
    public class TinyMCEv3DataTypeArtifactMigrator : ReplaceDataTypeArtifactMigratorBase<RichTextConfiguration>
    {
        private const string FromEditorAlias = "Umbraco.TinyMCEv3";
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="TinyMCEv3DataTypeArtifactMigrator" /> class.
        /// </summary>
        /// <param name="propertyEditors">The property editors.</param>
        public TinyMCEv3DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors)
            : base(FromEditorAlias, Constants.PropertyEditors.Aliases.TinyMce, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override RichTextConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new RichTextConfiguration();

            if (fromConfiguration.TryGetValue("editor", out var editor) && editor != null)
            {
                toConfiguration.Editor = JObject.Parse(editor.ToString());
            }

            if (fromConfiguration.TryGetValue("hideLabel", out var hideLabel))
            {
                toConfiguration.HideLabel = TrueValue.Equals(hideLabel);
            }

            if (fromConfiguration.TryGetValue("ignoreUserStartNodes", out var ignoreUserStartNodes))
            {
                toConfiguration.IgnoreUserStartNodes = TrueValue.Equals(ignoreUserStartNodes);
            }

            return toConfiguration;
        }
    }
}
