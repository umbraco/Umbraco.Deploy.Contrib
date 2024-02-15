using System.Collections.Generic;
using Semver;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Deploy.Migrators;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="DataTypeArtifact" /> to replace the editor alias with <see cref="Constants.PropertyEditors.Aliases.MediaPicker" /> and the configuration from Umbraco 7 to <see cref="MediaPickerConfiguration" />.
    /// </summary>
    public abstract class MediaPickerReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase<MediaPickerConfiguration>
    {
        private const string TrueValue = "1";

        /// <summary>
        /// Gets a value indicating whether the configuration allows multiple items to be picked.
        /// </summary>
        /// <value>
        ///   <c>true</c> if multiple items can be picked; otherwise, <c>false</c>.
        /// </value>
        protected abstract bool Multiple { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaPickerReplaceDataTypeArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="fromEditorAlias">From editor alias.</param>
        /// <param name="propertyEditors">The property editors.</param>
        protected MediaPickerReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, PropertyEditorCollection propertyEditors)
            : base(fromEditorAlias, Constants.PropertyEditors.Aliases.MediaPicker, propertyEditors)
            => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override MediaPickerConfiguration MigrateConfiguration(IDictionary<string, object> fromConfiguration)
        {
            var toConfiguration = new MediaPickerConfiguration()
            {
                Multiple = Multiple
            };

            if (fromConfiguration.TryGetValue("multiPicker", out var multiPicker))
            {
                toConfiguration.Multiple = TrueValue.Equals(multiPicker);
            }

            if (fromConfiguration.TryGetValue("onlyImages", out var onlyImages))
            {
                toConfiguration.OnlyImages = TrueValue.Equals(onlyImages);
            }

            if (fromConfiguration.TryGetValue("disableFolderSelect", out var disableFolderSelect))
            {
                toConfiguration.DisableFolderSelect = TrueValue.Equals(disableFolderSelect);
            }

            if (fromConfiguration.TryGetValue("startNodeId", out var startNodeId) &&
               Udi.TryParse(startNodeId?.ToString(), out var udi))
            {
                toConfiguration.StartNodeId = udi;
            }

            if (fromConfiguration.TryGetValue("ignoreUserStartNodes", out var ignoreUserStartNodes))
            {
                toConfiguration.IgnoreUserStartNodes = TrueValue.Equals(ignoreUserStartNodes);
            }

            return toConfiguration;
        }

        /// <inheritdoc />
        protected override MediaPickerConfiguration GetDefaultConfiguration(IConfigurationEditor toConfigurationEditor)
        {
            var configuration = base.GetDefaultConfiguration(toConfigurationEditor);
            configuration.Multiple = Multiple;

            return configuration;
        }
    }
}
