using System;
using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;
using Umbraco.Deploy.Core.Migrators;

namespace Umbraco.Extensions;

internal static class OrderedCollectionBuilderExtensions
{
    public static ArtifactMigratorCollectionBuilder Insert(this ArtifactMigratorCollectionBuilder builder, params IEnumerable<Type> types)
        => Insert(builder, 0, types);

    public static ArtifactMigratorCollectionBuilder Insert(this ArtifactMigratorCollectionBuilder builder, int index, params IEnumerable<Type> types)
        => Insert<ArtifactMigratorCollectionBuilder, ArtifactMigratorCollection, IArtifactMigrator>(builder, index, types);

    public static PropertyTypeMigratorCollectionBuilder Insert(this PropertyTypeMigratorCollectionBuilder builder, params IEnumerable<Type> types)
        => Insert(builder, 0, types);

    public static PropertyTypeMigratorCollectionBuilder Insert(this PropertyTypeMigratorCollectionBuilder builder, int index, params IEnumerable<Type> types)
        => Insert<PropertyTypeMigratorCollectionBuilder, PropertyTypeMigratorCollection, IPropertyTypeMigrator>(builder, index, types);

    private static TBuilder Insert<TBuilder, TCollection, TItem>(TBuilder builder, int index, params IEnumerable<Type> types)
        where TBuilder : OrderedCollectionBuilderBase<TBuilder, TCollection, TItem>
        where TCollection : class, IBuilderCollection<TItem>
    {
        foreach (var type in types)
        {
            builder.Insert(index, type);

            // Insert next type after the current one
            index++;
        }

        return builder;
    }
}
