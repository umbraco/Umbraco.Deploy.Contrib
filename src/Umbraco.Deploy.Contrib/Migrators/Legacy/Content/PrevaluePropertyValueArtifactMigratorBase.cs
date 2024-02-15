using System;
using Newtonsoft.Json;
using Semver;
using Umbraco.Core.Services;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy
{
    /// <summary>
    /// Migrates the <see cref="PropertyValueWithSegments" /> using the specified editor alias from the <see cref="ContentArtifactBase" /> containing prevalues (seperated by <see cref="Delimiter" />) from Umbraco 7 to a JSON array.
    /// </summary>
    public abstract class PrevaluePropertyValueArtifactMigratorBase : PropertyValueArtifactMigratorBase
    {
        private const string Delimiter = ";;";

        /// <summary>
        /// Initializes a new instance of the <see cref="PrevaluePropertyValueArtifactMigratorBase" /> class.
        /// </summary>
        /// <param name="editorAlias">The editor alias.</param>
        /// <param name="contentTypeService">The content type service.</param>
        /// <param name="mediaTypeService">The media type service.</param>
        /// <param name="memberTypeService">The member type service.</param>
        public PrevaluePropertyValueArtifactMigratorBase(string editorAlias, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
           : base(editorAlias, contentTypeService, mediaTypeService, memberTypeService)
           => MaxVersion = new SemVersion(3, 0, 0);

        /// <inheritdoc />
        protected override string Migrate(string value)
            => JsonConvert.SerializeObject(value.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries));
    }
}
