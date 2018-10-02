using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    public class ArchetypeValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IMacroParser _macroParser;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;
        public virtual IEnumerable<string> PropertyEditorAliases => new[] { "Imulus.Archetype" };

        internal static string MockPropertyTypeAlias = "mockArchetypePropertyTypeAlias";
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        public ArchetypeValueConnector(IDataTypeService dataTypeService, IMacroParser macroParser, Lazy<ValueConnectorCollection> valueConnectors)
        {
            _dataTypeService = dataTypeService;
            _macroParser = macroParser;
            _valueConnectorsLazy = valueConnectors;
        }

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            if (property.Value == null)
                return null;

            var value = property.Value.ToString();

            var prevalues = _dataTypeService.GetPreValuesCollectionByDataTypeId(property.PropertyType.DataTypeDefinitionId).PreValuesAsDictionary;
            PreValue prevalue;
            //Fetch the Prevalues for the current Property's DataType (if its an 'Archetype config')
            if (!prevalues.TryGetValue("archetypeconfig", out prevalue) && !prevalues.TryGetValue("archetypeConfig", out prevalue))
            {
                throw new InvalidOperationException("Could not find Archetype configuration.");
            }
            var archetypePreValue = prevalue == null
                ? null
                : JsonConvert.DeserializeObject<ArchetypePreValue>(prevalue.Value);

            RetrieveAdditionalProperties(ref archetypePreValue);

            var archetype = JsonConvert.DeserializeObject<ArchetypeModel>(value);

            RetrieveAdditionalProperties(ref archetype, archetypePreValue);

            if (archetype == null)
                throw new InvalidOperationException("Could not parse Archetype value.");

            var properties = archetype.Fieldsets.SelectMany(x => x.Properties);

            foreach (var archetypeProperty in properties)
            {
                if (archetypeProperty.DataTypeGuid == null)
                    continue;
                // get the data type of the property
                var dataType = _dataTypeService.GetDataTypeDefinitionById(Guid.Parse(archetypeProperty.DataTypeGuid));
                if (dataType == null)
                {
                    throw new ArgumentNullException(
                        $"Unable to find the data type for editor '{archetypeProperty.PropertyEditorAlias}' ({archetypeProperty.DataTypeGuid}) referenced by '{property.Alias}'.");
                }
                // add the datatype as a dependency
                dependencies.Add(new ArtifactDependency(new GuidUdi(Constants.UdiEntityType.DataType, dataType.Key), false, ArtifactDependencyMode.Exist));

                // if it's null or undefined value there is no need to do any more processing
                if (archetypeProperty.Value == null || archetypeProperty.Value.ToString() == string.Empty)
                {
                    continue;
                }

                Udi udi;
                // if the value is an Udi then add it as a dependency and continue
                if (Udi.TryParse(archetypeProperty.Value.ToString(), out udi))
                {
                    dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));
                    continue;
                }

                var tempDependencies = new List<Udi>();
                // try to convert the value with the macro parser - this is mainly for legacy editors
                var archetypeValue = archetypeProperty.Value != null
                    ? _macroParser.ReplaceAttributeValue(archetypeProperty.Value.ToString(), dataType.PropertyEditorAlias, tempDependencies, Direction.ToArtifact)
                    : null;
                foreach (var dependencyUdi in tempDependencies)
                {
                    // if the macro parser was able to convert the value it will mark the Udi as dependency
                    // and we want that added as a artifact dependency
                    dependencies.Add(new ArtifactDependency(dependencyUdi, false, ArtifactDependencyMode.Exist));
                }

                // test if the macroparser converted the value to an Udi so we can just continue
                if (Udi.TryParse(archetypeValue, out udi))
                {
                    archetypeProperty.Value = archetypeValue;
                    continue;
                }

                // if the macro parser didn't convert the value then try to find a value connector that can and convert it
                var propertyType = new PropertyType(dataType.PropertyEditorAlias, dataType.DatabaseType);
                propertyType.DataTypeDefinitionId = dataType.Id;
                var propValueConnector = ValueConnectors.Get(propertyType);

                var mockProperty = new Property(propertyType, archetypeProperty.Value);

                archetypeValue = propValueConnector.GetValue(mockProperty, dependencies);
                archetypeProperty.Value = archetypeValue;
            }
            value = archetype.SerializeForPersistence();
            return value;
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            if (value.DetectIsJson() == false)
                return;

            var property = content.Properties[alias];
            if (property == null)
                throw new NullReferenceException($"Property not found: '{alias}'.");

            var prevalues = _dataTypeService.GetPreValuesCollectionByDataTypeId(property.PropertyType.DataTypeDefinitionId).FormatAsDictionary();

            PreValue prevalue = null;
            //Fetch the Prevalues for the current Property's DataType (if its an 'Archetype config')
            if (!prevalues.TryGetValue("archetypeconfig", out prevalue) && !prevalues.TryGetValue("archetypeConfig", out prevalue))
            {
                throw new InvalidOperationException("Could not find Archetype configuration.");
            }
            var archetypePreValue = prevalue == null
                ? null
                : JsonConvert.DeserializeObject<ArchetypePreValue>(prevalue.Value);

            RetrieveAdditionalProperties(ref archetypePreValue);

            var archetype = JsonConvert.DeserializeObject<ArchetypeModel>(value);

            if (archetype == null)
                throw new InvalidOperationException("Could not parse Archetype value.");

            RetrieveAdditionalProperties(ref archetype, archetypePreValue);

            var properties = archetype.Fieldsets.SelectMany(x => x.Properties);

            foreach (var archetypeProperty in properties)
            {
                if (archetypeProperty.Value == null || string.IsNullOrEmpty(archetypeProperty.DataTypeGuid))
                    continue;

                // get the data type of the property
                var dataType = _dataTypeService.GetDataTypeDefinitionById(Guid.Parse(archetypeProperty.DataTypeGuid));
                if (dataType == null)
                {
                    throw new ArgumentNullException(
                        $"Unable to find the data type for editor '{archetypeProperty.PropertyEditorAlias}' ({archetypeProperty.DataTypeGuid}) referenced by '{property.Alias}'.");
                }

                // try to convert the value with the macro parser - this is mainly for legacy editors
                var archetypeValue = _macroParser.ReplaceAttributeValue(archetypeProperty.Value.ToString(), dataType.PropertyEditorAlias, null, Direction.FromArtifact);

                Udi udi;
                // test if the macroparser converted the value to an Udi
                if (Udi.TryParse(archetypeValue, out udi))
                {
                    archetypeProperty.Value = archetypeValue;
                    continue;
                }

                // if the macro parser didn't convert the value then try to find a value connector that can and convert it
                var propertyType = new PropertyType(dataType.PropertyEditorAlias, dataType.DatabaseType, MockPropertyTypeAlias);
                propertyType.DataTypeDefinitionId = dataType.Id;
                var propValueConnector = ValueConnectors.Get(propertyType);
                var mockProperty = new Property(propertyType);
                var mockContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> { mockProperty }));
                propValueConnector.SetValue(mockContent, mockProperty.Alias, archetypeProperty.Value.ToString());
                archetypeProperty.Value = mockContent.GetValue(mockProperty.Alias);
            }
            value = archetype.SerializeForPersistence();
            content.SetValue(alias, value);
        }

        private void RetrieveAdditionalProperties(ref ArchetypePreValue preValue)
        {
            if (preValue == null)
                return;

            foreach (var fieldset in preValue.Fieldsets)
            {
                foreach (var property in fieldset.Properties)
                {
                    var dataType = _dataTypeService.GetDataTypeDefinitionById(property.DataTypeGuid);

                    if (dataType == null)
                        throw new NullReferenceException($"Could not find DataType with guid: '{property.DataTypeGuid}'");

                    property.PropertyEditorAlias = dataType.PropertyEditorAlias;
                }
            }
        }

        private void RetrieveAdditionalProperties(ref ArchetypeModel archetype, ArchetypePreValue preValue)
        {
            if (preValue == null)
                return;
            foreach (var fieldset in preValue.Fieldsets)
            {
                var fieldsetAlias = fieldset.Alias;
                foreach (var fieldsetInst in archetype.Fieldsets.Where(x => x.Alias == fieldsetAlias))
                {
                    foreach (var property in fieldset.Properties)
                    {
                        foreach (var propertyInst in fieldsetInst.Properties.Where(x => x.Alias == property.Alias))
                        {
                            propertyInst.DataTypeGuid = property.DataTypeGuid.ToString();
                            Guid dataTypeGuid;
                            if (Guid.TryParse(propertyInst.DataTypeGuid, out dataTypeGuid) == false)
                                throw new InvalidOperationException($"Could not parse DataTypeGuid as a guid: '{propertyInst.DataTypeGuid}'.");
                            var dataTypeDefinition = _dataTypeService.GetDataTypeDefinitionById(dataTypeGuid);
                            if (dataTypeDefinition == null)
                                throw new NullReferenceException($"Could not find DataType with guid: '{dataTypeGuid}'");
                            propertyInst.DataTypeId = dataTypeDefinition.Id;
                            propertyInst.PropertyEditorAlias = property.PropertyEditorAlias;
                        }
                    }
                }
            }
        }

        internal class ArchetypePreValue
        {
            [JsonProperty("showAdvancedOptions")]
            public bool ShowAdvancedOptions { get; set; }

            [JsonProperty("startWithAddButton")]
            public bool StartWithAddButton { get; set; }

            [JsonProperty("hideFieldsetToolbar")]
            [Obsolete("This value is no longer used but is kept to prevent breaking changes.")]
            public bool HideFieldsetToolbar { get; set; }

            [JsonProperty("enableMultipleFieldsets")]
            public bool EnableMultipleFieldsets { get; set; }

            [JsonProperty("enableCollapsing")]
            public bool EnableCollapsing { get; set; }

            [JsonProperty("enableCloning")]
            public bool EnableCloning { get; set; }

            [JsonProperty("enableDisabling")]
            public bool EnableDisabling { get; set; }

            [JsonProperty("enablePublishing")]
            public bool EnablePublishing { get; set; }

            [JsonProperty("enableCrossDragging")]
            public bool EnableCrossDragging { get; set; }

            [JsonProperty("enableMemberGroups")]
            public bool EnableMemberGroups { get; set; }

            [JsonProperty("hideFieldsetControls")]
            public bool HideFieldsetControls { get; set; }

            [JsonProperty("hidePropertyLabel")]
            public bool HidePropertyLabel { get; set; }

            [JsonProperty("maxFieldsets", NullValueHandling = NullValueHandling.Ignore)]
            public int MaxFieldsets { get; set; }

            [JsonProperty("fieldsets")]
            public IEnumerable<ArchetypePreValueFieldset> Fieldsets { get; set; }

            [JsonProperty("fieldsetGroups")]
            public IEnumerable<ArchetypePreValueFieldsetGroup> FieldsetGroups { get; set; }

            [JsonProperty("hidePropertyLabels")]
            public bool HidePropertyLabels { get; set; }

            [JsonProperty("customCssClass")]
            public string CustomCssClass { get; set; }

            [JsonProperty("customCssPath")]
            public string CustomCssPath { get; set; }

            [JsonProperty("customJsPath")]
            public string CustomJsPath { get; set; }

            [JsonProperty("customViewPath")]
            public string CustomViewPath { get; set; }

            [JsonProperty("enableDeepDatatypeRequests")]
            public bool EnableDeepDatatypeRequests { get; set; }

            [JsonProperty("developerMode")]
            public bool DeveloperMode { get; set; }

            [JsonProperty("overrideDefaultPropertyValueConverter")]
            public bool OverrideDefaultPropertyValueConverter { get; set; }
        }

        internal class ArchetypePreValueFieldset
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("remove")]
            public bool Remove { get; set; }

            [JsonProperty("collapse")]
            public bool Collapse { get; set; }

            [JsonProperty("labelTemplate")]
            public string LabelTemplate { get; set; }

            [JsonProperty("icon")]
            public string Icon { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("properties")]
            public IEnumerable<ArchetypePreValueProperty> Properties { get; set; }

            [JsonProperty("group")]
            internal ArchetypePreValueFieldsetGroup Group { get; set; }
        }

        internal class ArchetypePreValueProperty
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("remove")]
            public bool Remove { get; set; }

            [JsonProperty("collapse")]
            public bool Collapse { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("helpText")]
            public string HelpText { get; set; }

            [JsonProperty("dataTypeGuid")]
            public Guid DataTypeGuid { get; set; }

            [JsonProperty("propertyEditorAlias")]
            public string PropertyEditorAlias { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("required")]
            public bool Required { get; set; }

            [JsonProperty("regEx")]
            public string RegEx { get; set; }
        }

        internal class ArchetypePreValueFieldsetGroup
        {
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        [JsonObject]
        internal class ArchetypeModel
        {
            [JsonProperty("fieldsets")]
            public IEnumerable<ArchetypeFieldsetModel> Fieldsets { get; set; }

            /// <summary>
            /// Serializes for persistence. This should be used for serialization as it cleans up the JSON before saving.
            /// </summary>
            /// <returns></returns>
            public string SerializeForPersistence()
            {
                var json = JObject.Parse(JsonConvert.SerializeObject(this, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

                var propertiesToRemove = new[] { "propertyEditorAlias", "dataTypeId", "dataTypeGuid", "hostContentType", "editorState" };

                json.Descendants().OfType<JProperty>()
                  .Where(p => propertiesToRemove.Contains(p.Name))
                  .ToList()
                  .ForEach(x => x.Remove());

                return json.ToString(Formatting.None);
            }
        }

        internal class ArchetypeFieldsetModel
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("disabled")]
            public bool Disabled { get; set; }

            [JsonProperty("properties")]
            public IEnumerable<ArchetypePropertyModel> Properties { get; set; }

            [JsonProperty("id")]
            public Guid Id { get; set; }

            [JsonProperty("releaseDate")]
            public DateTime? ReleaseDate { get; set; }

            [JsonProperty("expireDate")]
            public DateTime? ExpireDate { get; set; }

            [JsonProperty("allowedMemberGroups")]
            public string AllowedMemberGroups { get; set; }
        }

        internal class ArchetypePropertyModel
        {
            [JsonProperty("alias")]
            public string Alias { get; set; }

            [JsonProperty("value")]
            public object Value { get; set; }

            [JsonProperty("propertyEditorAlias")]
            public string PropertyEditorAlias { get; internal set; }

            [JsonProperty("dataTypeId")]
            public int DataTypeId { get; internal set; }

            [JsonProperty("dataTypeGuid")]
            internal string DataTypeGuid { get; set; }

            // container for temporary editor state from the Umbraco backend
            [JsonProperty("editorState")]
            internal UmbracoEditorState EditorState { get; set; }

            [JsonProperty("hostContentType")]
            internal PublishedContentType HostContentType { get; set; }
        }

        internal class UmbracoEditorState
        {
            // container for the names of any files selected for a property in the Umbraco backend
            [JsonProperty("fileNames")]
            public IEnumerable<string> FileNames { get; set; }
        }
    }
}
