using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the property value containing prevalues (seperated by <see cref="Delimiter" />) from Umbraco 7 to a single value or JSON array.
/// </summary>
public abstract class PrevaluePropertyTypeMigratorBase : PropertyTypeMigratorBase
{
    private const string EditorAliasPrefix = PrevalueArtifactMigrator.EditorAliasPrefix;
    private const string Delimiter = ";;";

    private readonly IJsonSerializer _jsonSerializer;

    /// <summary>
    /// Gets a value indicating whether the property type stores multiple prevalues as a JSON array or single value.
    /// </summary>
    /// <value>
    ///   <c>true</c> if multiple prevalues are stored as a JSON array; otherwise, <c>false</c>.
    /// </value>
    protected abstract bool Multiple { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrevaluePropertyTypeMigratorBase" /> class.
    /// </summary>
    /// <param name="editorAlias">The editor alias.</param>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    protected PrevaluePropertyTypeMigratorBase(string editorAlias, IJsonSerializer jsonSerializer)
        : base(EditorAliasPrefix + editorAlias, editorAlias)
        => _jsonSerializer = jsonSerializer;

    /// <inheritdoc />
    public override object? Migrate(IPropertyType propertyType, object? value, IDictionary<string, string> propertyEditorAliases, IContextCache contextCache)
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
