using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Deploy.Artifacts.Content;
using Umbraco.Deploy.Connectors.ServiceConnectors.Wrappers;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="PropertyValueWithSegments" /> using the <see cref="Constants.PropertyEditors.Aliases.RadioButtonList" /> editor from the <see cref="ContentArtifactBase" /> containing prevalues (seperated by <see cref="PrevaluePropertyValueArtifactMigratorBase.Delimiter" />) from Umbraco 7 to a JSON array.
    /// </summary>
    public class RadioButtonListPropertyValueArtifactMigrator : PrevaluePropertyValueArtifactMigratorBase
    {
        /// <inheritdoc />
        protected override bool Multiple => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadioButtonListPropertyValueArtifactMigrator" /> class.
        /// </summary>
        /// <param name="contentTypeService">The content type service.</param>
        /// <param name="mediaTypeService">The media type service.</param>
        /// <param name="memberTypeService">The member type service.</param>
        public RadioButtonListPropertyValueArtifactMigrator(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
            : base(Constants.PropertyEditors.Aliases.RadioButtonList, contentTypeService, mediaTypeService, memberTypeService)
        { }
    }
}
