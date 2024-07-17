using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Core.Migrators;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Artifacts.ContentType;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to remove Matryoshka data types (using the <see cref="EditorAlias" /> editor) and
/// <see cref="ContentTypeArtifactBase" /> to replace properties using the removed Matryoshka data types with native groups.
/// </summary>
public sealed class ReplaceMatryoshkaArtifactMigrator : ArtifactMigratorBase<IArtifact>
{
    private const string EditorAlias = "Our.Umbraco.Matryoshka.GroupSeparator";

    private readonly HashSet<Guid> _removedDataTypeKeys = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceMatryoshkaArtifactMigrator" /> class.
    /// </summary>
    public ReplaceMatryoshkaArtifactMigrator()
        // Matryoshka was only supported on v8
        => MaxVersion = new SemVersion(9, 0, 0);

    /// <inheritdoc />
    protected override bool CanMigrateType(Type type)
        => type == typeof(DataTypeArtifact) || type.Inherits<ContentTypeArtifactBase>();

    /// <inheritdoc />
    protected override IArtifact? Migrate(IArtifact artifact)
        => artifact switch
        {
            // Remove Matryoshka data types
            DataTypeArtifact dataTypeArtifact when dataTypeArtifact.EditorAlias == EditorAlias => Migrate(dataTypeArtifact),
            // Replace properties with removed Matryoshka data types with native groups
            ContentTypeArtifactBase contentTypeArtifactBase => Migrate(contentTypeArtifactBase),
            // Ignore other artifacts
            _ => artifact,
        };

    private DataTypeArtifact? Migrate(DataTypeArtifact artifact)
    {
        // Track removed data types by key
        _removedDataTypeKeys.Add(artifact.Key);

        return null;
    }

    private ContentTypeArtifactBase? Migrate(ContentTypeArtifactBase artifact)
    {
        // Remove property types using the removed data types
        artifact.PropertyTypes = artifact.PropertyTypes.Where(x => _removedDataTypeKeys.Contains(x.DataType.Guid) is false).ToArray();

        // Convert property groups to tabs and create new groups when removed data types are found
        var propertyGroups = new List<ContentTypeArtifactBase.PropertyGroup>();
        foreach (var propertyGroup in artifact.PropertyGroups.OrderBy(x => x.SortOrder).ToArray())
        {
            // Convert groups to tabs
            propertyGroup.Type = (short)PropertyGroupType.Tab;

            var propertyTypes = new List<ContentTypeArtifactBase.PropertyType>();
            foreach (var propertyType in propertyGroup.PropertyTypes.OrderByDescending(x => x.SortOrder).ToArray())
            {
                if (_removedDataTypeKeys.Contains(propertyType.DataType.Guid))
                {
                    // Create new group below tab
                    propertyGroups.Add(new ContentTypeArtifactBase.PropertyGroup()
                    {
                        Key = propertyType.Key,
                        Name = propertyType.Name,
                        Alias = propertyGroup.Alias + "/" + propertyType.Alias,
                        PropertyTypes = propertyTypes.ToArray(),
                        SortOrder = propertyType.SortOrder,
                        Type = (short)PropertyGroupType.Group,
                    });

                    propertyTypes.Clear();
                }
                else
                {
                    // Keep property type
                    propertyTypes.Add(propertyType);
                }
            }

            propertyGroup.PropertyTypes = propertyTypes.ToArray();
            propertyGroups.Add(propertyGroup);
        }

        artifact.PropertyGroups = propertyGroups.ToArray();

        return artifact;
    }
}
