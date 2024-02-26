using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.DropDownListFlexible" /> and the configuration from Umbraco 7 to <see cref="DropDownFlexibleConfiguration" />.
    /// </summary>
    public abstract class DropDownReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase<DropDownFlexibleConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Gets a value indicating whether the configuration allows multiple items to be selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if multiple items can be selected; otherwise, <c>false</c>.
        /// </value>
        protected abstract bool Multiple { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownReplaceDataTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
        /// <param name="propertyEditors">The property editors.</param>
        protected DropDownReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.DropDownListFlexible, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override DropDownFlexibleConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new DropDownFlexibleConfiguration()
            {
                Multiple = Multiple
            };

            foreach (var (key, value) in fromConfiguration)
            {
                if (key == "multiple")
                {
                    toConfiguration.Multiple = TrueValue.Equals(value);
                }
                else if (int.TryParse(key, out var id) && value != null)
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
        protected override DropDownFlexibleConfiguration GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
        {
            var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
            if (configuration != null)
            {
                configuration.Multiple = Multiple;
            }

            return configuration;
        }
    }
}
