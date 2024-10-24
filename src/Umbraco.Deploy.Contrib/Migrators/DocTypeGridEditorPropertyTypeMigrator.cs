using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators;

/// <summary>
/// Migrates the property value when the editor of a property type changed from <see cref="Constants.PropertyEditors.Aliases.Grid" /> to <see cref="Constants.PropertyEditors.Aliases.BlockGrid" /> and supports DocTypeGridEditor.
/// </summary>
public class DocTypeGridEditorPropertyTypeMigrator : GridPropertyTypeMigrator
{
    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocTypeGridEditorPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="mediaService">The media service.</param>
    public DocTypeGridEditorPropertyTypeMigrator(ILogger<GridPropertyTypeMigrator> logger, IJsonSerializer jsonSerializer, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, IContentTypeService contentTypeService, IMediaService mediaService)
        : base(logger, jsonSerializer, dataTypeService, shortStringHelper, contentTypeService, mediaService)
        => _jsonSerializer = jsonSerializer;

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
    protected virtual async Task<BlockItemData?> MigrateGridControlAsync(DocTypeGridEditorValue value, BlockGridConfiguration configuration, IContextCache contextCache)
    {
        IContentType contentType = await GetContentTypeAsync(value.ContentTypeAlias, configuration, contextCache).ConfigureAwait(false)
            ?? throw new InvalidOperationException($"Migrating legacy grid failed, because content type with alias '{value.ContentTypeAlias}' could not be found (in the Block Grid configuration).");

        return new BlockItemData()
        {
            Key = value.Id,
            ContentTypeKey = contentType.Key,
            Values = value.Value.Select(x => new BlockPropertyValue()
            {
                Alias = x.Key,
                Value = x.Value,
            }).ToList(),
        };
    }

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
        catch (JsonSerializationException)
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
        public Dictionary<string, object?> Value { get; set; } = new();

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
