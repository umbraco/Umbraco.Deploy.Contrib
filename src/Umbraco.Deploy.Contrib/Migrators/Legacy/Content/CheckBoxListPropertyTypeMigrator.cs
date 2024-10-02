using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the property value using the <see cref="Constants.PropertyEditors.Aliases.CheckBoxList" /> editor containing prevalues (seperated by <see cref="PrevaluePropertyTypeMigratorBase.Delimiter" />) from Umbraco 7 to a JSON array.
/// </summary>
public sealed class CheckBoxListPropertyTypeMigrator : PrevaluePropertyTypeMigratorBase
{
    /// <inheritdoc />
    protected override bool Multiple => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="CheckBoxListPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public CheckBoxListPropertyTypeMigrator(IJsonSerializer jsonSerializer)
        : base(Constants.PropertyEditors.Aliases.CheckBoxList, jsonSerializer)
    { }
}
