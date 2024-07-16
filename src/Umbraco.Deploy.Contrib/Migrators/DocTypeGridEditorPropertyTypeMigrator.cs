using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    protected override BlockItemData? MigrateGridControl(GridValue.GridControl gridControl, BlockGridConfiguration configuration, IContextCache contextCache)
    {
        if (TryDeserialize(gridControl.Value, out DocTypeGridEditorValue? value))
        {
            return MigrateGridControl(value, configuration, contextCache);
        }

        return base.MigrateGridControl(gridControl, configuration, contextCache);
    }

    /// <summary>
    /// Migrates the grid control.
    /// </summary>
    /// <param name="value">The DTGE value.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="contextCache">The context cache.</param>
    /// <returns>
    /// The block item data, or <c>null</c> if migration should be skipped.
    /// </returns>
    protected virtual BlockItemData? MigrateGridControl(DocTypeGridEditorValue value, BlockGridConfiguration configuration, IContextCache contextCache)
    {
        IContentType contentType = GetContentType(value.ContentTypeAlias, configuration, contextCache)
            ?? throw new InvalidOperationException($"Migrating legacy grid failed, because content type with alias '{value.ContentTypeAlias}' could not be found (in the Block Grid configuration).");

        return new BlockItemData()
        {
            Udi = Udi.Create(Constants.UdiEntityType.Element, value.Id),
            ContentTypeKey = contentType.Key,
            RawPropertyValues = value.Value
        };
    }

    private bool TryDeserialize(JToken? value, [NotNullWhen(true)] out DocTypeGridEditorValue? docTypeGridEditorValue)
    {
        try
        {
            docTypeGridEditorValue = value switch
            {
                JObject jsonObject => jsonObject.ToObject<DocTypeGridEditorValue>(),
                JToken jsonToken when jsonToken.Value<string>() is string json && json.DetectIsJson() => _jsonSerializer.Deserialize<DocTypeGridEditorValue>(json),
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
        [JsonProperty("value")]
        public Dictionary<string, object?> Value { get; set; } = new();

        /// <summary>
        /// Gets or sets the content type alias.
        /// </summary>
        /// <value>
        /// The content type alias.
        /// </value>
        [JsonProperty("dtgeContentTypeAlias")]
        public string ContentTypeAlias { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}
