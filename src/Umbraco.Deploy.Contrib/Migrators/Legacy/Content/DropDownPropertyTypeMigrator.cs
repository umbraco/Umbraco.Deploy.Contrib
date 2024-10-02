using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the property value from the legacy <see cref="FromEditorAlias" /> editor containing prevalues (seperated by <see cref="PrevaluePropertyTypeMigratorBase.Delimiter" />) from Umbraco 7 to a JSON array.
/// </summary>
public sealed class DropDownPropertyTypeMigrator : PrevaluePropertyTypeMigratorBase
{
    private const string FromEditorAlias = "Umbraco.DropDown";

    /// <inheritdoc />
    protected override bool Multiple => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="DropDownPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public DropDownPropertyTypeMigrator(IJsonSerializer jsonSerializer)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.DropDownListFlexible, jsonSerializer)
    { }
}
