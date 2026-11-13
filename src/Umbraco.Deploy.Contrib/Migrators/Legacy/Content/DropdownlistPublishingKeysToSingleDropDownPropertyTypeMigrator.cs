using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the property value from the legacy <see cref="FromEditorAlias" /> editor containing prevalues (seperated by <see cref="PrevaluePropertyTypeMigratorBase.Delimiter" />) from Umbraco 7 to a JSON array, for data types that were moved to the <see cref="Constants.PropertyEditors.Aliases.SingleDropDown" /> editor.
/// </summary>
public sealed class DropdownlistPublishingKeysToSingleDropDownPropertyTypeMigrator : PrevaluePropertyTypeMigratorBase
{
    private const string FromEditorAlias = "Umbraco.DropdownlistPublishingKeys";

    /// <inheritdoc />
    /// <remarks>
    /// The single dropdown stores its value as a JSON array too, so this picks the format, not the number of values.
    /// </remarks>
    protected override bool Multiple => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="DropdownlistPublishingKeysToSingleDropDownPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public DropdownlistPublishingKeysToSingleDropDownPropertyTypeMigrator(IJsonSerializer jsonSerializer)
        : base(FromEditorAlias, Constants.PropertyEditors.Aliases.SingleDropDown, jsonSerializer)
    { }
}
