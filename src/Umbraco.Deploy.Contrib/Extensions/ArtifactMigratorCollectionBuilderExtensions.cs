using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Extensions;

public static class ArtifactMigratorCollectionBuilderExtensions
{
    /// <summary>
    /// Adds the legacy artifact migrators to allow importing from Umbraco 7.
    /// </summary>
    /// <param name="artifactMigratorCollectionBuilder">The artifact migrator collection builder.</param>
    /// <returns>
    /// The artifact migrator collection builder.
    /// </returns>
    public static ArtifactMigratorCollectionBuilder AddLegacyMigrators(this ArtifactMigratorCollectionBuilder artifactMigratorCollectionBuilder)
        => artifactMigratorCollectionBuilder
            // Pre-values to configuration
            .Append<PreValuesDataTypeArtifactJsonMigrator>()
            // Release/expire dates to schedule
            .Append<DocumentArtifactJsonMigrator>()
            // Allowed at root and child content types to permissions
            .Append<ContentTypeArtifactJsonMigrator>()
            // Data types
            .Append<CheckBoxListDataTypeArtifactMigrator>()
            .Append<ColorPickerAliasDataTypeArtifactMigrator>()
            .Append<ContentPicker2DataTypeArtifactMigrator>()
            .Append<ContentPickerAliasDataTypeArtifactMigrator>()
            .Append<DateDataTypeArtifactMigrator>()
            .Append<DropDownFlexibleDataTypeArtifactMigrator>() // Ensure this is appended before other dropdown migrators to avoid duplicate migration
            .Append<DropDownDataTypeArtifactMigrator>()
            .Append<DropdownlistMultiplePublishKeysDataTypeArtifactMigrator>()
            .Append<DropdownlistPublishingKeysDataTypeArtifactMigrator>()
            .Append<DropDownMultipleDataTypeArtifactMigrator>()
            .Append<MediaPicker2DataTypeArtifactMigrator>()
            .Append<MemberPicker2DataTypeArtifactMigrator>()
            .Append<MultiNodeTreePicker2DataTypeArtifactMigrator>()
            .Append<MultipleMediaPickerDataTypeArtifactMigrator>()
            .Append<NoEditDataTypeArtifactMigrator>()
            .Append<RadioButtonListDataTypeArtifactMigrator>()
            .Append<RelatedLinks2DataTypeArtifactMigrator>()
            .Append<RelatedLinksDataTypeArtifactMigrator>()
            .Append<TextboxDataTypeArtifactMigrator>()
            .Append<TextboxMultipleDataTypeArtifactMigrator>()
            .Append<TinyMCEv3DataTypeArtifactMigrator>()
            // Add prefixes to pre-value property editor aliases, triggering property type migrators
            .Append<PrevalueArtifactMigrator>();
}
