using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Extensions;

public static class PropertyTypeMigratorCollectionBuilderExtensions
{
    /// <summary>
    /// Adds the legacy property type migrators to allow importing from Umbraco 7.
    /// </summary>
    /// <returns>
    /// The property type migrator collection builder.
    /// </returns>
    public static PropertyTypeMigratorCollectionBuilder AddLegacyMigrators(this PropertyTypeMigratorCollectionBuilder propertyTypeMigratorCollectionBuilder)
        => propertyTypeMigratorCollectionBuilder
            // Pre-values to a single value or JSON array
            .Append<CheckBoxListPropertyTypeMigrator>()
            .Append<DropDownListFlexiblePropertyTypeMigrator>()
            .Append<RadioButtonListPropertyTypeMigrator>();
}
