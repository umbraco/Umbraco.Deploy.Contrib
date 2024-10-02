using System;
using System.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Semver;
using Umbraco.Deploy.Core.Migrators;
using Umbraco.Deploy.Infrastructure.Artifacts.Content;
using Umbraco.Extensions;

namespace Umbraco.Deploy.Contrib.Migrators.Legacy;

/// <summary>
/// Migrates the <see cref="ContentArtifactBase" /> property editor aliases to add a prefix to Umbraco 7 prevalue editors, triggering property type migrators.
/// </summary>
public class PrevalueArtifactMigrator : ArtifactMigratorBase<ContentArtifactBase>
{
    internal const string EditorAliasPrefix = "MigratePrevalue.";

    private readonly string[] _editorAliases;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrevalueArtifactMigrator" /> class.
    /// </summary>
    public PrevalueArtifactMigrator()
        : this(
              Constants.PropertyEditors.Aliases.CheckBoxList,
              Constants.PropertyEditors.Aliases.DropDownListFlexible,
              Constants.PropertyEditors.Aliases.RadioButtonList)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrevalueArtifactMigrator" /> class.
    /// </summary>
    /// <param name="editorAliases">The editor aliases.</param>
    protected PrevalueArtifactMigrator(params string[] editorAliases)
    {
        _editorAliases = editorAliases;

        MaxVersion = new SemVersion(3, 0, 0);
    }

    /// <inheritdoc />
    protected override bool CanMigrateType(Type type)
        => type.Inherits<ContentArtifactBase>();

    /// <inheritdoc />
    protected override ContentArtifactBase? Migrate(ContentArtifactBase artifact)
    {
        // Add prefix to matching property editor aliases
        artifact.PropertyEditorAliases = artifact.PropertyEditorAliases?.ToDictionary(x => x.Key, x => _editorAliases.Contains(x.Value) ? EditorAliasPrefix + x.Value : x.Value);

        return artifact;
    }
}
