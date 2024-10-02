using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the property value using the <see cref="Constants.PropertyEditors.Aliases.RadioButtonList" /> editor containing prevalues (seperated by <see cref="PrevaluePropertyTypeMigratorBase.Delimiter" />) from Umbraco 7 to a single value.
/// </summary>
public sealed class RadioButtonListPropertyTypeMigrator : PrevaluePropertyTypeMigratorBase
{
    /// <inheritdoc />
    protected override bool Multiple => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="RadioButtonListPropertyTypeMigrator" /> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer.</param>
    public RadioButtonListPropertyTypeMigrator(IJsonSerializer jsonSerializer)
        : base(Constants.PropertyEditors.Aliases.RadioButtonList, jsonSerializer)
    { }
}
