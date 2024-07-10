using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Grid;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Migrators;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.PropertyEditors.BlockGridConfiguration;

namespace Umbraco.Deploy.Contrib.Migrators;

/// <summary>
/// Migrates the <see cref="DataTypeArtifact" /> to replace the legacy/obsoleted <see cref="Constants.PropertyEditors.Aliases.Grid" /> editor with <see cref="Constants.PropertyEditors.Aliases.BlockGrid" /> and support DTGE grid editors.
/// </summary>
public class ReplaceDocTypeGridEditorDataTypeArtifactMigrator : ReplaceGridDataTypeArtifactMigrator
{
    private readonly IContentTypeService _contentTypeService;

    /// <summary>
    /// Gets or sets a value indicating whether to add the default DTGE grid editor (if not configured in the grid.editors.config.js files).
    /// </summary>
    /// <value>
    ///   <c>true</c> if the default DTGE grid editor is added; otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    protected bool AddDefaultDocTypeGridEditor { get; init; } = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaceDocTypeGridEditorDataTypeArtifactMigrator" /> class.
    /// </summary>
    /// <param name="propertyEditors">The property editors.</param>
    /// <param name="configurationEditorJsonSerializer">The configuration editor JSON serializer.</param>
    /// <param name="contentTypeService">The content type service.</param>
    /// <param name="dataTypeService">The data type service.</param>
    /// <param name="shortStringHelper">The short string helper.</param>
    /// <param name="gridConfig">The grid configuration.</param>
    public ReplaceDocTypeGridEditorDataTypeArtifactMigrator(PropertyEditorCollection propertyEditors, IConfigurationEditorJsonSerializer configurationEditorJsonSerializer, IContentTypeService contentTypeService, IDataTypeService dataTypeService, IShortStringHelper shortStringHelper, IGridConfig gridConfig)
        : base(propertyEditors, configurationEditorJsonSerializer, contentTypeService, dataTypeService, shortStringHelper, gridConfig)
        => _contentTypeService = contentTypeService;

    /// <inheritdoc />
    protected override IEnumerable<(string, BlockGridBlockConfiguration)> MigrateGridEditors(IEnumerable<GridConfigurationLayout> gridLayouts)
    {
        // Migrate regular grid editors (DTGE editors are skipped)
        foreach ((string alias, BlockGridBlockConfiguration blockGridBlockConfiguration) in base.MigrateGridEditors(gridLayouts))
        {
            yield return (alias, blockGridBlockConfiguration);
        }

        // Migrate DTGE editors
        var allElementTypes = GetAllElementTypes().ToList();
        var migratedContentTypeKeys = new HashSet<Guid>();
        foreach (IGridEditorConfig gridEditor in GetGridEditors(gridLayouts).Where(IsDocTypeGridEditor))
        {
            foreach (Guid contentElementTypeKey in MigrateDocTypeGridEditor(gridEditor, allElementTypes))
            {
                // Avoid DocTypeGridEditors returning duplicate block configurations for the same content element type
                if (migratedContentTypeKeys.Add(contentElementTypeKey))
                {
                    yield return (gridEditor.Alias, new BlockGridBlockConfiguration()
                    {
                        ContentElementTypeKey = contentElementTypeKey,
                        Label = gridEditor.Config.TryGetValue("nameTemplate", out var nameTemplateConfig) && nameTemplateConfig is string nameTemplate && !string.IsNullOrEmpty(nameTemplate)
                        ? nameTemplate
                        : gridEditor.NameTemplate,
                        EditorSize = gridEditor.Config.TryGetValue("overlaySize", out var overviewSizeConfig) && overviewSizeConfig is string overviewSize && !string.IsNullOrEmpty(overviewSize)
                        ? overviewSize
                        : null,
                    });
                }
            }
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IGridEditorConfig> GetGridEditors()
    {
        foreach (IGridEditorConfig gridEditor in base.GetGridEditors())
        {
            yield return gridEditor;
        }

        if (AddDefaultDocTypeGridEditor)
        {
            yield return new GridEditor()
            {
                Name = "Doc Type",
                Alias = "docType",
                View = "/App_Plugins/DocTypeGridEditor/Views/doctypegrideditor.html",
                Render = "/App_Plugins/DocTypeGridEditor/Render/DocTypeGridEditor.cshtml",
                Icon = "icon-item-arrangement",
            };
        }
    }

    /// <inheritdoc />
    protected override Guid? MigrateGridEditor(IGridEditorConfig gridEditor)
        // Skip migrating DocTypeGridEditor using base implementation (as that creates a new element type)
        => IsDocTypeGridEditor(gridEditor) is false ? base.MigrateGridEditor(gridEditor) : null;

    /// <summary>
    /// Migrates the DocTypeGridEditor.
    /// </summary>
    /// <param name="gridEditor">The grid editor.</param>
    /// <param name="allElementTypes">All element types.</param>
    /// <returns>
    /// The keys of the content element types of the migrated grid editor.
    /// </returns>
    protected virtual IEnumerable<Guid> MigrateDocTypeGridEditor(IGridEditorConfig gridEditor, IEnumerable<IContentType> allElementTypes)
    {
        if (gridEditor.Config.TryGetValue("allowedDocTypes", out var allowedDocTypesConfig) &&
            allowedDocTypesConfig is JArray allowedDocTypes &&
            allowedDocTypes.Values<string>().WhereNotNull().ToArray() is string[] docTypes &&
            docTypes.Length > 0)
        {
            // Use regex matching
            return allElementTypes.Where(x => docTypes.Any(y => Regex.IsMatch(x.Alias, y))).Select(x => x.Key);
        }

        // Return all
        return allElementTypes.Select(x => x.Key);
    }

    /// <summary>
    /// Gets all element types.
    /// </summary>
    /// <returns>
    /// Returns all element types.
    /// </returns>
    /// <remarks>
    /// The default implementation excludes element types with aliases that start with <c>gridLayout_</c>, <c>gridRow_</c>, <c>gridEditor_</c> or <c>gridSettings_</c>.
    /// </remarks>
    protected virtual IEnumerable<IContentType> GetAllElementTypes()
    {
        static bool IsAllowedElementType(string alias) => !alias.StartsWith("gridLayout_") && !alias.StartsWith("gridRow_") && !alias.StartsWith("gridEditor_") && !alias.StartsWith("gridSettings_");

        return _contentTypeService.GetAllElementTypes().Where(x => IsAllowedElementType(x.Alias));
    }

    /// <summary>
    /// Determines whether the grid editor is the DocTypeGridEditor.
    /// </summary>
    /// <param name="gridEditor">The grid editor.</param>
    /// <returns>
    ///   <c>true</c> if the grid editor is the DocTypeGridEditor; otherwise, <c>false</c>.
    /// </returns>
    protected static bool IsDocTypeGridEditor(IGridEditorConfig gridEditor)
        => gridEditor.View?.Contains("doctypegrideditor", StringComparison.OrdinalIgnoreCase) is true;
}
