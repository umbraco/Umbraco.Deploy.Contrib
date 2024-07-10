using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Configuration.Grid;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Connectors.DataTypeConfigurationConnectors;
using Umbraco.Deploy.Core;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Deploy.Contrib.DataTypeConfigurationConnectors
{
    /// <summary>
    /// Implements a Grid layout data type configuration connector supporting DocTypeGridEditor.
    /// </summary>
    public class DocTypeGridEditorDataTypeConfigurationConnector : DataTypeConfigurationConnectorBase2
    {
        private readonly IGridConfig _gridConfig;
        private readonly IContentTypeService _contentTypeService;

        /// <inheritdoc />
        public override IEnumerable<string> PropertyEditorAliases { get; } = new[]
        {
            Constants.PropertyEditors.Aliases.Grid
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="DocTypeGridEditorDataTypeConfigurationConnector" /> class.
        /// </summary>
        /// <param name="gridConfig">The grid configuration.</param>
        /// <param name="contentTypeService">The content type service.</param>
        public DocTypeGridEditorDataTypeConfigurationConnector(IGridConfig gridConfig, IContentTypeService contentTypeService)
        {
            _gridConfig = gridConfig;
            _contentTypeService = contentTypeService;
        }

        /// <inheritdoc />
        public override string ToArtifact(IDataType dataType, ICollection<ArtifactDependency> dependencies, IContextCache contextCache)
        {
            if (dataType.ConfigurationAs<GridConfiguration>() is GridConfiguration gridConfiguration &&
                gridConfiguration.Items?.ToObject<GridConfigurationItems>() is GridConfigurationItems gridConfigurationItems)
            {
                // Get all element types (when needed)
                var allElementTypes = new Lazy<IEnumerable<IContentType>>(() => _contentTypeService.GetAll().Where(x => x.IsElement).ToList());

                // Process DTGE editors
                foreach (var gridEditor in GetGridEditors(gridConfigurationItems).Where(IsDocTypeGridEditor))
                {
                    if (gridEditor.Config.TryGetValue("allowedDocTypes", out var allowedDocTypesConfig) &&
                        allowedDocTypesConfig is JArray allowedDocTypes &&
                        allowedDocTypes.Count > 0)
                    {
                        string[] docTypes = allowedDocTypes.Values<string>().WhereNotNull().ToArray();

                        // Use regex matching
                        AddDependencies(dependencies, allElementTypes.Value.Where(x => docTypes.Any(y => Regex.IsMatch(x.Alias, y))));
                    }
                    else
                    {
                        // Add all element types as dependencies and stop processing
                        AddDependencies(dependencies, allElementTypes.Value);
                        break;
                    }
                }
            }

            return base.ToArtifact(dataType, dependencies, contextCache);
        }

        private static void AddDependencies(ICollection<ArtifactDependency> dependencies, IEnumerable<IContentType> contentTypes)
        {
            foreach (var contentType in contentTypes)
            {
                dependencies.Add(new ArtifactDependency(contentType.GetUdi(), true, ArtifactDependencyMode.Exist));
            }
        }

        /// <summary>
        /// Gets the grid editors used within the grid configuration.
        /// </summary>
        /// <param name="gridConfigurationItems">The grid configuration items.</param>
        /// <returns>
        /// The used grid editors (returns all editors if any of the areas allows all editors).
        /// </returns>
        protected virtual IEnumerable<IGridEditorConfig> GetGridEditors(GridConfigurationItems gridConfigurationItems)
        {
            foreach (var gridEditor in _gridConfig.EditorsConfig.Editors)
            {
                if (IsAllowedGridEditor(gridConfigurationItems, gridEditor.Alias))
                {
                    yield return gridEditor;
                }
            }
        }

        /// <summary>
        /// Determines whether the grid editor alias is allowed in the specified grid configuration.
        /// </summary>
        /// <param name="gridConfigurationItems">The grid configuration items.</param>
        /// <param name="alias">The alias.</param>
        /// <returns>
        ///   <c>true</c> if the grid editor alias is allowed in the specified grid configuration; otherwise, <c>false</c>.
        /// </returns>
        protected static bool IsAllowedGridEditor(GridConfigurationItems gridConfigurationItems, string alias)
            => gridConfigurationItems.Layouts.Any(x => x.Areas.Any(y => y.AllowAll || y.Allowed.Contains(alias)));

        /// <summary>
        /// Determines whether the grid editor is the DTGE.
        /// </summary>
        /// <param name="gridEditor">The grid editor.</param>
        /// <returns>
        ///   <c>true</c> if the grid editor is the DTGE; otherwise, <c>false</c>.
        /// </returns>
        protected static bool IsDocTypeGridEditor(IGridEditorConfig gridEditor)
            => !string.IsNullOrEmpty(gridEditor.View) && gridEditor.View.Contains("doctypegrideditor");

        /// <summary>
        /// The grid configuration items.
        /// </summary>
        protected sealed class GridConfigurationItems
        {
            /// <summary>
            /// Gets or sets the row configurations.
            /// </summary>
            /// <value>
            /// The row configurations.
            /// </value>
            [JsonProperty("layouts")]
            public GridConfigurationLayout[] Layouts { get; set; } = Array.Empty<GridConfigurationLayout>();
        }

        /// <summary>
        /// The grid row configuration.
        /// </summary>
        protected sealed class GridConfigurationLayout
        {
            /// <summary>
            /// Gets or sets the areas.
            /// </summary>
            /// <value>
            /// The areas.
            /// </value>
            [JsonProperty("areas")]
            public GridConfigurationLayoutArea[] Areas { get; set; } = Array.Empty<GridConfigurationLayoutArea>();
        }

        /// <summary>
        /// The grid row configuration area.
        /// </summary>
        protected sealed class GridConfigurationLayoutArea
        {
            /// <summary>
            /// Gets or sets a value indicating whether all grid editors are allowed.
            /// </summary>
            /// <value>
            ///   <c>true</c> if all grid editors are allowed; otherwise, <c>false</c>.
            /// </value>
            /// <remarks>
            /// Defaults to <c>true</c>.
            /// </remarks>
            [JsonProperty("allowAll")]
            public bool AllowAll { get; set; } = true;

            /// <summary>
            /// Gets or sets the allowed grid editor aliases.
            /// </summary>
            /// <value>
            /// The allowed grid editor aliases.
            /// </value>
            [JsonProperty("allowed")]
            public string[] Allowed { get; set; } = Array.Empty<string>();
        }
    }
}
