using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Connectors.ValueConnectors.Services;

namespace Umbraco.Deploy.Contrib.Connectors.GridCellValueConnectors
{
    public class DocTypeGridEditorCellValueConnector : IGridCellValueConnector
    {
        private readonly ILogger _logger;
        private readonly IContentTypeService _contentTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        public DocTypeGridEditorCellValueConnector(ILogger logger, IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            _logger = logger;
            _contentTypeService = contentTypeService ?? throw new ArgumentNullException(nameof(contentTypeService));
            _valueConnectorsLazy = valueConnectors ?? throw new ArgumentNullException(nameof(valueConnectors));
        }

        public bool IsConnector(string view) => !string.IsNullOrWhiteSpace(view) && view.Contains("doctypegrideditor");

        public string GetValue(GridValue.GridControl gridControl, ICollection<ArtifactDependency> dependencies)
        {
            // cancel if there's no values
            if (gridControl.Value == null || gridControl.Value.HasValues == false) return null;

            _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - Grid Values: {gridControl.Value}");

            var docTypeGridEditorContent = JsonConvert.DeserializeObject<DocTypeGridEditorValue>(gridControl.Value.ToString());

            // if an 'empty' dtge item has been added - it has no ContentTypeAlias set .. just return and don't throw.
            if (docTypeGridEditorContent == null || string.IsNullOrWhiteSpace(docTypeGridEditorContent.ContentTypeAlias))
            {
                _logger.Debug<DocTypeGridEditorCellValueConnector>("GetValue - DTGE Empty without ContentTypeAlias");
                return null;
            }

            _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - ContentTypeAlias - {docTypeGridEditorContent.ContentTypeAlias}");

            // check if the doc type exist - else abort packaging
            var contentType = _contentTypeService.Get(docTypeGridEditorContent.ContentTypeAlias);

            if (contentType == null)
            {
                _logger.Debug<DocTypeGridEditorCellValueConnector>("GetValue - Missing ContentType");
                throw new InvalidOperationException($"Could not resolve the Content Type for the Doc Type Grid Editor property: {docTypeGridEditorContent.ContentTypeAlias}");
            }

            // add content type as a dependency
            dependencies.Add(new ArtifactDependency(contentType.GetUdi(), false, ArtifactDependencyMode.Match));

            // find all properties
            var propertyTypes = contentType.CompositionPropertyTypes;

            foreach (var propertyType in propertyTypes)
            {
                _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - PropertyTypeAlias - {propertyType.Alias}");

                // test if there's a value for the given property
                if (!TryGetValue(docTypeGridEditorContent, propertyType, out var value))
                {
                    continue;
                }

                // if the value is an Udi then add it as a dependency
                if (AddUdiDependency(dependencies, value))
                {
                    continue;
                }

                // throws if not found - no need for a null check
                var propValueConnector = ValueConnectors.Get(propertyType);

                _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - PropertyValueConnectorAlias - {string.Join(", ", propValueConnector.PropertyEditorAliases)}");
                _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - propertyTypeValue - {value}");

                //properties like MUP / Nested Content are JSON, we need to convert to string for the conversion to artifact
                string parsedValue = propValueConnector.ToArtifact(IsJson(value) ? value.ToString() : value, propertyType, dependencies);

                _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - ParsedValue - {parsedValue}");

                docTypeGridEditorContent.Value[propertyType.Alias] = parsedValue;
            }

            var resolvedValue = JsonConvert.SerializeObject(docTypeGridEditorContent, Formatting.None);

            _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - ResolvedValue - {resolvedValue}");

            return resolvedValue;
        }

        public void SetValue(GridValue.GridControl gridControl)
        {
            // cancel if there's no values
            if (string.IsNullOrWhiteSpace(gridControl.Value.ToString()))
            {
                return;
            }

            // For some reason the control value isn't properly parsed so we need this extra step to parse it into a JToken
            gridControl.Value = JToken.Parse(gridControl.Value.ToString());

            _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - GridControlValue - {gridControl.Value}");

            var docTypeGridEditorContent = JsonConvert.DeserializeObject<DocTypeGridEditorValue>(gridControl.Value.ToString());

            if (docTypeGridEditorContent == null)
            {
                return;
            }

            _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - ContentTypeAlias - {docTypeGridEditorContent.ContentTypeAlias}");

            // check if the doc type exist - else abort packaging
            var contentType = _contentTypeService.Get(docTypeGridEditorContent.ContentTypeAlias);

            if (contentType == null)
            {
                throw new InvalidOperationException($"Could not resolve the Content Type for the Doc Type Grid Editor property: {docTypeGridEditorContent.ContentTypeAlias}");
            }

            _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - ContentType - {contentType}");

            // find all properties
            var propertyTypes = contentType.CompositionPropertyTypes;

            foreach (var propertyType in propertyTypes)
            {
                _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - PropertyEditorAlias -- {propertyType.PropertyEditorAlias}");

                if (!docTypeGridEditorContent.Value.TryGetValue(propertyType.Alias, out object value) || value == null)
                {
                    _logger.Debug<DocTypeGridEditorCellValueConnector>("SetValue - Value Is Null");
                    continue;
                }

                // throws if not found - no need for a null check
                var propValueConnector = ValueConnectors.Get(propertyType);
                var convertedValue = propValueConnector.FromArtifact(value.ToString(), propertyType, "");

                JToken jtokenValue = null;
                if (IsJson(convertedValue))
                {
                    // test if the value is a json object (thus could be a nested complex editor)
                    // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                    jtokenValue = GetJTokenValue(convertedValue);
                }

                if (jtokenValue != null)
                {
                    _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - jtokenValue - {jtokenValue}");
                    docTypeGridEditorContent.Value[propertyType.Alias] = jtokenValue;
                }
                else
                {
                    _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - convertedValue - {convertedValue}");
                    docTypeGridEditorContent.Value[propertyType.Alias] = convertedValue;
                }
            }

            var jtokenObj = JToken.FromObject(docTypeGridEditorContent);
            _logger.Debug<DocTypeGridEditorCellValueConnector>($"SetValue - jtokenObject - {jtokenObj}");
            gridControl.Value = jtokenObj;
        }

        private JToken GetJTokenValue(object value) => value != null && IsJson(value) ? JToken.Parse(value.ToString()) : null;

        private bool IsJson(object value) => value != null && value.ToString().DetectIsJson();

        private bool AddUdiDependency(ICollection<ArtifactDependency> dependencies, object value)
        {
            if (Udi.TryParse(value.ToString(), out var udi))
            {
                _logger.Debug<DocTypeGridEditorCellValueConnector>($"GetValue - Udi Dependency - {udi}");
                dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Match));
                return true;
            }

            return false;
        }

        private bool TryGetValue(DocTypeGridEditorValue docTypeGridEditorContent, PropertyType propertyType, out object objVal)
        {
            if (!docTypeGridEditorContent.Value.TryGetValue(propertyType.Alias, out objVal) || objVal == null)
            {
                _logger.Debug<DocTypeGridEditorCellValueConnector>("GetValue - Value is null");
                return false;
            }

            return true;
        }

        private class DocTypeGridEditorValue
        {
            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("dtgeContentTypeAlias")]
            public string ContentTypeAlias { get; set; }

            [JsonProperty("value")]
            public Dictionary<string, object> Value { get; set; }
        }
    }
}
