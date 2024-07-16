using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Deploy.Infrastructure.Artifacts;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the <see cref="FromEditorAlias" /> editor with <see cref="Constants.PropertyEditors.Aliases.MultiNodeTreePicker" /> and update the configuration.
/// </summary>
public class MultiNodeTreePicker2DataTypeArtifactMigrator : LegacyReplaceDataTypeArtifactMigratorBase
{
    private const string FromEditorAlias = "Umbraco.MultiNodeTreePicker2";

    private readonly IContentTypeService _contentTypeService;
    private readonly IMediaTypeService _mediaTypeService;
    private readonly IMemberTypeService _memberTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiNodeTreePicker2DataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    [Obsolete("Please use the constructor taking all parameters. This constructor will be removed in a future version.")]
    public MultiNodeTreePicker2DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : this(
              propertyEditors,
              configurationEditorJsonSerializer,
              StaticServiceProvider.Instance.GetRequiredService<IContentTypeService>(),
              StaticServiceProvider.Instance.GetRequiredService<IMediaTypeService>(),
              StaticServiceProvider.Instance.GetRequiredService<IMemberTypeService>())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiNodeTreePicker2DataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaTypeService">The media type service.</param>
    /// <param name="memberTypeService">The member type service.</param>
    public MultiNodeTreePicker2DataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer, IContentTypeService contentTypeService, IMediaTypeService mediaTypeService, IMemberTypeService memberTypeService)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.MultiNodeTreePicker, propertyEditors, configurationEditorJsonSerializer)
    {
        _contentTypeService = contentTypeService;
        _mediaTypeService = mediaTypeService;
        _memberTypeService = memberTypeService;

        MaxVersion = new SemVersion(3, 0, 0);
    }

    /// <inheritdoc />
    protected override IDictionary<string, object>? MigrateConfiguration(IDictionary<string, object> configuration)
    {
        ReplaceTreeSourceIdUdiWithGuid(ref configuration, "startNode", out string? treeSourceType);
        ReplaceAliasesWithKeys(ref configuration, "filter", treeSourceType?.ToLowerInvariant() switch
        {
            Constants.UdiEntityType.Media => x => _mediaTypeService.Get(x)?.Key,
            Constants.UdiEntityType.Member => x => _memberTypeService.Get(x)?.Key,
            _ => x => _contentTypeService.Get(x)?.Key,
        });
        configuration.Remove("multiPicker");
        ReplaceIntegerWithBoolean(ref configuration, Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes);
        ReplaceIntegerWithBoolean(ref configuration, "showOpenButton");

        return configuration;
    }
}
