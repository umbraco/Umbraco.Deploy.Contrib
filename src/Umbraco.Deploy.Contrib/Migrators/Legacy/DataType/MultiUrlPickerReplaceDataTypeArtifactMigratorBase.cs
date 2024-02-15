using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MultiUrlPicker" /> and the configuration from Umbraco 7 to <see cref="MultiUrlPickerConfiguration" />.
    /// </summary>
    public abstract class MultiUrlPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase<MultiUrlPickerConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiUrlPickerReplaceDataTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
        /// <param name="propertyEditors">The property editors.</param>
        protected MultiUrlPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MultiUrlPicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override MultiUrlPickerConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new MultiUrlPickerConfiguration();

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

            if (fromConfiguration.TryGetValue("ignoreUserStartNodes", out var ignoreUserStartNodes))
            {
                toConfiguration.IgnoreUserStartNodes = TrueValue.Equals(ignoreUserStartNodes);
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override MultiUrlPickerConfiguration GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
            => new MultiUrlPickerConfiguration();
    }
}
