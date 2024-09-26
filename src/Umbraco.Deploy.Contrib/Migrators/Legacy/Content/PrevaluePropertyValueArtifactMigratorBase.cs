using System;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Core.Connectors.ServiceConnectors.Wrappers;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;
using Umbraco.Deploy.Infrastructure.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="PropertyValueWithSegments" /> using the specified editor alias from the <see cref="ContentArtifactBase" /> containing prevalues (seperated by <see cref="Delimiter" />) from Umbraco 7 to a single value or JSON array.
/// </summary>
[Obsolete("Migrating property values in an artifact migrator does not support nested/recursive properties. Use the PrevalueArtifactMigrator and property type migrators instead. This class will be removed in a future version.")]
public abstract class PrevaluePropertyValueArtifactMigratorBase : PropertyValueArtifactMigratorBase
{
    private const string Delimiter = ";;";

    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Gets a value indicating whether the property stored multiple prevalues as a JSON array or single value.
    /// </summary>
    /// <value>
    ///   <c>true</c> if multiple prevalues are stored as a JSON array; otherwise, <c>false</c>.
    /// </value>
    protected abstract bool Multiple { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrevaluePropertyValueArtifactMigratorBase" /> class.
    /// </summary>
    /// <param name="editorAlias">The editor alias.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="memberTypeService">The member type service.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public PrevaluePropertyValueArtifactMigratorBase(string editorAlias, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService, IJsonSerializer jsonSerializer)
       : base(editorAlias, contentTypeService, mediaTypeService, memberTypeService)
    {
        _jsonSerializer = jsonSerializer;

        MaxVersion = new SemVersion(3, 0, 0);
    }

    /// <inheritdoc />
    protected override string? Migrate(string? value)
    {
        var values = value?.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
        if (values is null || values.Length == 0)
        {
            return null;
        }

        return Multiple
            ? _jsonSerializer.Serialize(values)
            : values[0];
    }
}
