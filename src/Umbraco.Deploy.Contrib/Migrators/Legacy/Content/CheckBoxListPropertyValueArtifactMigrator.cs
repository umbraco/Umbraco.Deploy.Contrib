using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Deploy.Artifacts.Content;
using Umbraco.Deploy.Connectors.ServiceConnectors.Wrappers;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="PropertyValueWithSegments" /> using the <see cref="Constants.PropertyEditors.Aliases.CheckBoxList" /> editor from the <see cref="ContentArtifactBase" /> containing prevalues (seperated by <see cref="PrevaluePropertyValueArtifactMigratorBase.Delimiter" />) from Umbraco 7 to a JSON array.
    /// </summary>
    public class CheckBoxListPropertyValueArtifactMigrator : PrevaluePropertyValueArtifactMigratorBase
    {
        /// <inheritdoc />
        protected override bool Multiple => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxListPropertyValueArtifactMigrator" /> class.
        /// </summary>
        /// <param name="contentTypeService">The content type service.</param>
        /// <param name="mediaTypeService">The media type service.</param>
        /// <param name="memberTypeService">The member type service.</param>
        public CheckBoxListPropertyValueArtifactMigrator(IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
            : base(Constants.PropertyEditors.Aliases.CheckBoxList, contentTypeService, mediaTypeService, memberTypeService)
        { }
    }
}
