using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <inheritdoc />
public abstract class LegacyReplaceDataTypeArtifactMigratorBase : ReplaceDataTypeArtifactMigratorBase
{
    private readonly IConfigurationEditorJsonSerializer _configurationEditorJsonSerializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LegacyReplaceDataTypeArtifactMigratorBase" /> class.
    /// </summary>
    /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
    /// <param name="toEditorAlias">The editor alias to migrate to.</param>
    /// <param name="toEditorUiAlias">The editor UI alias to migrate to.</param>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    protected LegacyReplaceDataTypeArtifactMigratorBase(string fromEditorAlias, string toEditorAlias, string toEditorUiAlias, PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
        : base(fromEditorAlias, toEditorAlias, toEditorUiAlias, propertyEditors, configurationEditorJsonSerializer)
        => _configurationEditorJsonSerializer = configurationEditorJsonSerializer;

    /// <summary>
    /// Replaces the old key with a new key.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="oldKey">The old key.</param>
    /// <param name="newKey">The new key.</param>
    protected static void ReplaceKey(ref IDictionary<string, object> configuration, string oldKey, string newKey)
    {
        if (configuration.Remove(oldKey, out var value))
        {
            configuration[newKey] = value;
        }
    }

    /// <summary>
    /// Replaces the UDI value with the GUID.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    /// <param name="keepInvalid">If set to <c>true</c> keeps the invalid value.</param>
    protected static void ReplaceUdiWithGuid(ref IDictionary<string, object> configuration, string key, bool keepInvalid = false)
    {
        if (configuration.TryGetValue(key, out var value))
        {
            if (value is string udi && UdiParser.TryParse(udi, out GuidUdi? guidUdi))
            {
                configuration[key] = guidUdi.Guid;
            }
            else if (keepInvalid is false && (value is not string guid || Guid.TryParse(guid, out _) is false))
            {
                configuration.Remove(key);
            }
        }
    }

    /// <summary>
    /// Replaces the integer value with a boolean (defaults to false).
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    protected static void ReplaceIntegerWithBoolean(ref IDictionary<string, object> configuration, string key)
    {
        if (configuration.TryGetValue(key, out var value) &&
            value is not bool)
        {
            configuration[key] = value?.ToString()?.ToLowerInvariant() switch
            {
                "1" or "true" => true,
                _ => false,
            };
        }
    }

    /// <summary>
    /// Replaces the string value with an integer.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    /// <param name="keepInvalid">If set to <c>true</c> keeps the invalid value.</param>
    protected static void ReplaceStringWithInteger(ref IDictionary<string, object> configuration, string key, bool keepInvalid = false)
    {
        if (configuration.TryGetValue(key, out var value) &&
            value is not int)
        {
            if (value is string stringValue &&
                int.TryParse(stringValue, out int intValue))
            {
                configuration[key] = intValue;
            }
            else if (keepInvalid is false)
            {
                configuration.Remove(key);
            }
        }
    }

    /// <summary>
    /// Replaces the value list array with a string array.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    protected void ReplaceValueListArrayWithStringArray(ref IDictionary<string, object> configuration, string key)
    {
        if (TryDeserialize(ref configuration, key, out IEnumerable<LegacyValueListItem>? items))
        {
            configuration[key] = items.Select(x => x.Value).ToArray();
        }
    }

    /// <summary>
    /// Replaces the UDI in the tree source ID value with the GUID.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    /// <param name="treeSourceType">The entity type of the tree source.</param>
    protected void ReplaceTreeSourceIdUdiWithGuid(ref IDictionary<string, object> configuration, string key, out string? treeSourceType)
    {
        if (TryDeserialize(ref configuration, key, out LegacyTreeSource? treeSource))
        {
            configuration[key] = new TreeSource()
            {
                Type = treeSource.Type,
                Id = treeSource.Id?.Guid,
                DynamicRoot = treeSource.DynamicRoot,
            };
        }

        treeSourceType = treeSource?.Type;
    }

    /// <summary>
    /// Replaces the aliases with keys.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    /// <param name="getKeyByAlias">The delegate to get the key by alias.</param>
    protected static void ReplaceAliasesWithKeys(ref IDictionary<string, object> configuration, string key, Func<string, Guid?> getKeyByAlias)
    {
        if (configuration.TryGetValue(key, out var value) &&
            value is string stringValue)
        {
            configuration[key] = string.Join(',', stringValue.Split(Constants.CharArrays.Comma).Select(getKeyByAlias).OfType<Guid>());
        }
    }

    /// <summary>
    /// Attempts to JSON deserialize the value.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="configuration">The configuration.</param>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <returns>
    ///   <c>true</c> if the value was deserialized; otherwise, <c>false</c>.
    /// </returns>
    protected bool TryDeserialize<T>(ref IDictionary<string, object> configuration, string key, [NotNullWhen(true)] out T? value)
    {
        if (configuration.TryGetValue(key, out var configurationValue) &&
            configurationValue?.ToString() is string stringValue &&
            stringValue.DetectIsJson())
        {
            value = _configurationEditorJsonSerializer.Deserialize<T>(stringValue);
        }
        else
        {
            value = default;
        }

        return value is not null;
    }

    private sealed class LegacyValueListItem
    {
        public required string Value { get; set; }
    }

    private sealed class LegacyTreeSource
    {
        public required string Type { get; set; }
        public GuidUdi? Id { get; set; }
        public object? DynamicRoot { get; set; }
    }

    private sealed class TreeSource
    {
        public required string Type { get; set; }
        public Guid? Id { get; set; }
        public object? DynamicRoot { get; set; }
    }
}
