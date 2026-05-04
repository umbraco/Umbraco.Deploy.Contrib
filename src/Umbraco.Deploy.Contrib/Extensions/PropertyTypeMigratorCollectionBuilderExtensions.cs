using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Extensions;

public static class PropertyTypeMigratorCollectionBuilderExtensions
{
    /// <summary>
    /// Adds/inserts the legacy property type migrators to allow importing from Umbraco 7.
    /// </summary>
    /// <returns>
    /// The property type migrator collection builder.
    /// </returns>
    /// <remarks>
    /// The legacy migrators are inserted at the beginning of the collection to ensure they run before any other migrators, including default Deploy migrators.
    /// </remarks>
    public static PropertyTypeMigratorCollectionBuilder AddLegacyMigrators(this PropertyTypeMigratorCollectionBuilder propertyTypeMigratorCollectionBuilder)
        => propertyTypeMigratorCollectionBuilder.Insert(
            // Pre-values to a single value or JSON array
            typeof(CheckBoxListPropertyTypeMigrator),
            typeof(DropDownPropertyTypeMigrator),
            typeof(DropDownListFlexiblePropertyTypeMigrator),
            typeof(DropdownlistMultiplePublishKeysPropertyTypeMigrator),
            typeof(DropdownlistPublishingKeysPropertyTypeMigrator),
            typeof(DropDownMultiplePropertyTypeMigrator),
            typeof(RadioButtonListPropertyTypeMigrator));
}
