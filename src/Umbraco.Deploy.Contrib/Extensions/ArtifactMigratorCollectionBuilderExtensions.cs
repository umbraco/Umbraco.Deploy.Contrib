using Umbraco.Deploy.Contrib.Migrators.Legacy;
using Umbraco.Deploy.Migrators;

namespace Umbraco.Extensions
{
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
                .Append<DocumentTypeArtifactJsonMigrator>()
                .Append<MediaTypeArtifactJsonMigrator>()
                .Append<MemberTypeArtifactJsonMigrator>()
                // Data types
                .Append<CheckBoxListDataTypeArtifactMigrator>()
                .Append<ColorPickerAliasDataTypeArtifactMigrator>()
                .Append<ContentPicker2DataTypeArtifactMigrator>()
                .Append<ContentPickerAliasDataTypeArtifactMigrator>()
                .Append<DateDataTypeArtifactMigrator>()
                .Append<DropDownDataTypeArtifactMigrator>()
                .Append<DropDownFlexibleDataTypeArtifactMigrator>()
                .Append<DropdownlistMultiplePublishKeysDataTypeArtifactMigrator>()
                .Append<DropdownlistPublishingKeysDataTypeArtifactMigrator>()
                .Append<DropDownMultipleDataTypeArtifactMigrator>()
                .Append<GridDataTypeArtifactMigrator>()
                .Append<ImageCropperDataTypeArtifactMigrator>()
                .Append<ListViewDataTypeArtifactMigrator>()
                .Append<MediaPicker2DataTypeArtifactMigrator>()
                .Append<MediaPickerDataTypeArtifactMigrator>()
                .Append<MemberPicker2DataTypeArtifactMigrator>()
                .Append<MultiNodeTreePicker2DataTypeArtifactMigrator>()
                .Append<MultiNodeTreePickerDataTypeArtifactMigrator>()
                .Append<MultipleMediaPickerDataTypeArtifactMigrator>()
                .Append<NestedContentDataTypeArtifactMigrator>()
                .Append<NoEditDataTypeArtifactMigrator>()
                .Append<RadioButtonListDataTypeArtifactMigrator>()
                .Append<RelatedLinks2DataTypeArtifactMigrator>()
                .Append<RelatedLinksDataTypeArtifactMigrator>()
                .Append<TextboxDataTypeArtifactMigrator>()
                .Append<TextboxMultipleDataTypeArtifactMigrator>()
                .Append<TinyMCEv3DataTypeArtifactMigrator>()
                // Property values
                .Append<CheckBoxListPropertyValueArtifactMigrator>()
                .Append<DropDownListFlexiblePropertyValueArtifactMigrator>()
                .Append<RadioButtonListPropertyValueArtifactMigrator>();
    }
}
