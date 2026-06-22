using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Deploy.Core.Migrators;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators;

/// <summary>
/// Migrates the property value when the editor of a property type changed from <see cref="Constants.PropertyEditors.Aliases.Grid" /> to <see cref="Constants.PropertyEditors.Aliases.BlockGrid" /> and supports DocTypeGridEditor.
/// </summary>
public partial class DocTypeGridEditorPropertyTypeMigrator : GridPropertyTypeMigrator
{
    private readonly ILogger<GridPropertyTypeMigrator> _logger;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly PropertyTypeMigratorCollection _propertyTypeMigrators;
    private IDictionary<string, string>? _propertyEditorAliases;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocTypeGridEditorPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaService">The media service.</param>
    [Obsolete("Use the constructor with all parameters. This will be removed in a future version.")]
    public DocTypeGridEditorPropertyTypeMigrator(ILogger<GridPropertyTypeMigrator> logger, IJsonSerializer jsonSerializer, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, IContentTypeService contentTypeService, IMediaService mediaService)
        : this(
              logger,
              jsonSerializer,
              dataTypeService,
              shortStringHelper,
              contentTypeService,
              mediaService,
              StaticServiceProvider.Instance.GetRequiredService<PropertyTypeMigratorCollection>())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DocTypeGridEditorPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaService">The media service.</param>
    /// <param name="propertyTypeMigrators">The property type migrators.</param>
    public DocTypeGridEditorPropertyTypeMigrator(ILogger<GridPropertyTypeMigrator> logger, IJsonSerializer jsonSerializer, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, IContentTypeService contentTypeService, IMediaService mediaService, PropertyTypeMigratorCollection propertyTypeMigrators)
        : base(logger, jsonSerializer, dataTypeService, shortStringHelper, contentTypeService, mediaService)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _propertyTypeMigrators = propertyTypeMigrators;
    }

    /// <inheritdoc />
    public override async Task<object?> MigrateAsync(IPropertyType propertyType, object? value, IDictionary<string, string> propertyEditorAliases, IContextCache contextCache, CancellationToken cancellationToken = default)
    {
        // Workaround: store property editor aliases for use in MigrateGridControl
        _propertyEditorAliases = propertyEditorAliases;

        return await base.MigrateAsync(propertyType, value, propertyEditorAliases, contextCache, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async Task<BlockItemData?> MigrateGridControlAsync(GridValue.GridControl gridControl, BlockGridConfiguration configuration, IContextCache contextCache, CancellationToken cancellationToken = default)
    {
        if (TryDeserialize(gridControl.Value, out DocTypeGridEditorValue? value))
        {
            return await MigrateGridControlAsync(value, configuration, contextCache).ConfigureAwait(false);
        }

        return await base.MigrateGridControlAsync(gridControl, configuration, contextCache, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Migrates the grid control.
    /// </summary>
    /// <param name="value">The DTGE value.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the block item data, or <c>null</c> if migration should be skipped.
    /// </returns>
    protected virtual async Task<BlockItemData?> MigrateGridControlAsync(DocTypeGridEditorValue value, BlockGridConfiguration configuration, IContextCache contextCache) // TODO: Add cancellation token
    {
        IContentType contentType = await GetContentTypeAsync(value.ContentTypeAlias, configuration, contextCache).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Migrating legacy grid failed, because content type with alias '{value.ContentTypeAlias}' could not be found (in the Block Grid configuration).");

        var propertyValues = new List<BlockPropertyValue>(value.Value.Count);

        foreach (IPropertyType propertyType in contentType.CompositionPropertyTypes)
        {
            if (value.Value.TryGetValue(propertyType.Alias, out object? propertyValue))
            {
                if (_propertyEditorAliases is not null &&
                    await _propertyTypeMigrators.TryMigrateAsync(propertyType, value, _propertyEditorAliases, contentType.Alias, contextCache).ConfigureAwait(false) is (true, var migratedValue))
                {
                    LogMigratedProperty(propertyType.Alias, contentType.Alias, propertyType.PropertyEditorAlias, migratedValue);

                    propertyValue = migratedValue;
                }

                propertyValues.Add(new BlockPropertyValue()
                {
                    Alias = propertyType.Alias,
                    Value = propertyValue,
                });
            }
        }

        return new BlockItemData()
        {
            Key = value.Id,
            ContentTypeKey = contentType.Key,
            Values = propertyValues,
        };
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Migrated nested/recursive property {PropertyTypeAlias} on {ContentTypeAlias} to {PropertyEditorAlias}: {Value}.")]
    private partial void LogMigratedProperty(string propertyTypeAlias, string contentTypeAlias, string propertyEditorAlias, object? value);

    private bool TryDeserialize(JsonNode? value, [NotNullWhen(true)] out DocTypeGridEditorValue? docTypeGridEditorValue)
    {
        try
        {
            docTypeGridEditorValue = value switch
            {
                JsonObject jsonObject => jsonObject.Deserialize<DocTypeGridEditorValue>(),
                JsonNode jsonNode when jsonNode.GetValue<string>() is string json && json.DetectIsJson() => _jsonSerializer.Deserialize<DocTypeGridEditorValue>(json),
                _ => null
            };

            return !string.IsNullOrEmpty(docTypeGridEditorValue?.ContentTypeAlias);
        }
        catch (JsonException)
        {
            docTypeGridEditorValue = null;

            return false;
        }
    }

    /// <summary>
    /// The DTGE grid editor value.
    /// </summary>
    protected sealed class DocTypeGridEditorValue
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [JsonPropertyName("value")]
        public Dictionary<string, object?> Value { get; init; } = new();

        /// <summary>
        /// Gets or sets the content type alias.
        /// </summary>
        /// <value>
        /// The content type alias.
        /// </value>
        [JsonPropertyName("dtgeContentTypeAlias")]
        public string ContentTypeAlias { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonPropertyName("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
