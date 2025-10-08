using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Extensions;

public static class ArtifactMigratorCollectionBuilderExtensions
{
    /// <summary>
    /// Adds/inserts the legacy artifact migrators to allow importing from Umbraco 7.
    /// </summary>
    /// <param name="artifactMigratorCollectionBuilder">The artifact migrator collection builder.</param>
    /// <returns>
    /// The artifact migrator collection builder.
    /// </returns>
    /// <remarks>
    /// The legacy migrators are inserted at the beginning of the collection to ensure they run before any other migrators, including default Deploy migrators.
    /// </remarks>
    public static ArtifactMigratorCollectionBuilder AddLegacyMigrators(this ArtifactMigratorCollectionBuilder artifactMigratorCollectionBuilder)
        => artifactMigratorCollectionBuilder.Insert(
            // Pre-values to configuration
            typeof(PreValuesDataTypeArtifactJsonMigrator),
            // Release/expire dates to schedule
            typeof(DocumentArtifactJsonMigrator),
            // Allowed at root and child content types to permissions
            typeof(ContentTypeArtifactJsonMigrator),
            // Data types
            typeof(CheckBoxListDataTypeArtifactMigrator),
            typeof(ColorPickerAliasDataTypeArtifactMigrator),
            typeof(ContentPicker2DataTypeArtifactMigrator),
            typeof(ContentPickerAliasDataTypeArtifactMigrator),
            typeof(DateDataTypeArtifactMigrator),
            typeof(DropDownFlexibleDataTypeArtifactMigrator), // Ensure this is appended before other dropdown migrators to avoid duplicate migration
            typeof(DropDownDataTypeArtifactMigrator),
            typeof(DropdownlistMultiplePublishKeysDataTypeArtifactMigrator),
            typeof(DropdownlistPublishingKeysDataTypeArtifactMigrator),
            typeof(DropDownMultipleDataTypeArtifactMigrator),
            typeof(MediaPicker2DataTypeArtifactMigrator),
            typeof(MemberPicker2DataTypeArtifactMigrator),
            typeof(MultiNodeTreePicker2DataTypeArtifactMigrator),
            typeof(MultipleMediaPickerDataTypeArtifactMigrator),
            typeof(NoEditDataTypeArtifactMigrator),
            typeof(RadioButtonListDataTypeArtifactMigrator),
            typeof(RelatedLinks2DataTypeArtifactMigrator),
            typeof(RelatedLinksDataTypeArtifactMigrator),
            typeof(TextboxDataTypeArtifactMigrator),
            typeof(TextboxMultipleDataTypeArtifactMigrator),
            typeof(TinyMCEv3DataTypeArtifactMigrator),
            // Add prefixes to pre-value property editor aliases, triggering property type migrators
            typeof(PrevalueArtifactMigrator));
}
