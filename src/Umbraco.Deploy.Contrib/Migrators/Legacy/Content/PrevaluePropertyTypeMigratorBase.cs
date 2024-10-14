using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the property value containing pre-values (separated by <see cref="Delimiter" />) from Umbraco 7 to a single value or JSON array.
/// </summary>
public abstract class PrevaluePropertyTypeMigratorBase : PropertyTypeMigratorBase
{
    private const string EditorAliasPrefix = PrevalueArtifactMigrator.EditorAliasPrefix;
    private const string Delimiter = ";;";

    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Gets a value indicating whether the property type stores multiple pre-values as a JSON array or single value.
    /// </summary>
    /// <value>
    ///   <c>true</c> if multiple pre-values are stored as a JSON array; otherwise, <c>false</c>.
    /// </value>
    protected abstract bool Multiple { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrevaluePropertyTypeMigratorBase" /> class.
    /// </summary>
    /// <param name="editorAlias">The editor alias (without the prefix added by <see cref="PrevalueArtifactMigrator" />).</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected PrevaluePropertyTypeMigratorBase(string editorAlias, IJsonSerializer jsonSerializer)
        : this(EditorAliasPrefix + editorAlias, editorAlias, jsonSerializer)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrevaluePropertyTypeMigratorBase" /> class.
    /// </summary>
    /// <param name="fromEditorAlias">The editor alias to migrate from.</param>
    /// <param name="toEditorAlias">The editor alias to migrate to.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected PrevaluePropertyTypeMigratorBase(string fromEditorAlias, string toEditorAlias, IJsonSerializer jsonSerializer)
        : base(fromEditorAlias, toEditorAlias)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override Task<object?> MigrateAsync(IPropertyType propertyType, object? value, IDictionary<string, string> propertyEditorAliases, IContextCache contextCache, CancellationToken cancellationToken = default)
        => Task.FromResult(Migrate(propertyType));

    private object? Migrate(object? value)
    {
        if (value is not string stringValue)
        {
            return null;
        }

        var values = stringValue.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries);
        if (values.Length == 0)
        {
            return null;
        }

        return Multiple
            ? _jsonSerializer.Serialize(values)
            : values[0];
    }
}
