using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.GridCellValueConnectors
{
    /// <summary>
    /// A Grid Cell Value Connector for the LeBlender
    /// </summary>
    public class LeBlenderGridCellValueConnector : GridCellValueConnectorBase
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly IMacroParser _macroParser;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        internal static string MockPropertyTypeAlias = "mockLeBlenderPropertyTypeAlias";

        public LeBlenderGridCellValueConnector(IDataTypeService dataTypeService, IMacroParser macroParser, Lazy<ValueConnectorCollection> valueConnectors)
        {
            if (dataTypeService == null) throw new ArgumentNullException(nameof(dataTypeService));
            if (macroParser == null) throw new ArgumentNullException(nameof(macroParser));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            _dataTypeService = dataTypeService;
            _macroParser = macroParser;
            _valueConnectorsLazy = valueConnectors;
        }

        // cannot inject ValueConnectorCollection else of course it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        /// <inheritdoc/>
        public override bool IsConnector(string view)
        {
            if (string.IsNullOrWhiteSpace(view))
                return false;

            return view.Contains("leblender");
        }

        /// <inheritdoc/>
        public override string GetValue(GridValue.GridControl control, Property property, ICollection<ArtifactDependency> dependencies)
        {
            // cancel if there's no values
            if (control.Value == null || control.Value.HasValues == false)
                return null;

            // Often there is only one entry in cell.Value, but with LeBlender 
            // min/max you can easily get 2+ entries so we'll walk them all
            var newItemValue = new List<object>();
            foreach (var properties in control.Value)
            {
                // create object to store resolved properties
                var resolvedProperties = new JObject();
                foreach (var leBlenderPropertyWrapper in properties)
                {
                    if (leBlenderPropertyWrapper.HasValues == false) continue;

                    var leBlenderProperty = leBlenderPropertyWrapper.First.ToObject<LeBlenderGridCellValue>();

                    // get the data type of the property
                    var dataType = _dataTypeService.GetDataTypeDefinitionById(leBlenderProperty.DataTypeGuid);
                    if (dataType == null)
                    {
                        throw new ArgumentNullException(
                            $"Unable to find the data type for editor '{leBlenderProperty.EditorName}' ({leBlenderProperty.EditorAlias} {leBlenderProperty.DataTypeGuid}) referenced by '{property.Alias}'.");
                    }

                    // add the datatype as a dependency
                    dependencies.Add(new ArtifactDependency(dataType.GetUdi(), false, ArtifactDependencyMode.Exist));

                    // if it's null or undefined value there is no need to do any more processing
                    if (leBlenderProperty.Value.Type == JTokenType.Null || leBlenderProperty.Value.Type == JTokenType.Undefined)
                    {
                        resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));
                        continue;
                    }

                    Udi udi;
                    // if the value is an Udi then add it as a dependency
                    if (Udi.TryParse(leBlenderProperty.Value.ToString(), out udi))
                    {
                        dependencies.Add(new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist));
                        resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));
                        continue;
                    }

                    var tempDependencies = new List<Udi>();
                    // try to convert the value with the macro parser - this is mainly for legacy editors
                    var value = _macroParser.ReplaceAttributeValue(leBlenderProperty.Value.ToString(), dataType.PropertyEditorAlias, tempDependencies, Direction.ToArtifact);
                    foreach (var dependencyUdi in tempDependencies)
                    {
                        // if the macro parser was able to convert the value it will mark the Udi as dependency
                        // and we want that added as a artifact dependency
                        dependencies.Add(new ArtifactDependency(dependencyUdi, false, ArtifactDependencyMode.Exist));
                    }

                    // test if the macroparser converted the value to an Udi
                    if (Udi.TryParse(value, out udi))
                    {
                        leBlenderProperty.Value = value;
                        resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));
                        continue;
                    }

                    // if the macro parser didn't convert the value then try to find a value connector that can and convert it
                    var propertyType = new PropertyType(dataType.PropertyEditorAlias, dataType.DatabaseType);
                    propertyType.DataTypeDefinitionId = dataType.Id;
                    var propValueConnector = ValueConnectors.Get(propertyType);
                    if (leBlenderProperty.Value.Type == JTokenType.Array)
                    {
                        // if the value is an array then we should try and convert each item instead of the whole array
                        var array = new JArray();
                        foreach (var child in leBlenderProperty.Value)
                        {
                            var mockProperty = new Property(propertyType, child.Value<object>());
                            var convertedValue = propValueConnector.GetValue(mockProperty, dependencies);
                            array.Add(new JValue(convertedValue));
                        }
                        leBlenderProperty.Value = array;
                    }
                    else
                    {
                        var mockProperty = new Property(propertyType, leBlenderProperty.Value);
                        value = propValueConnector.GetValue(mockProperty, dependencies);
                        leBlenderProperty.Value = value;
                    }

                    resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));
                }
                newItemValue.Add(resolvedProperties);
            }

            return JsonConvert.SerializeObject(newItemValue);
        }

        /// <inheritdoc/>
        public override void SetValue(GridValue.GridControl control, Property property)
        {
            var value = control.Value.ToString();
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            // For some reason the control value isn't properly parsed so we need this extra step to parse it into a JToken
            control.Value = JToken.Parse(control.Value.ToString());

            // cancel if there's no values or it didn't parse
            if (control.Value == null || control.Value.HasValues == false)
                return;

            // Often there is only one entry in cell.Value, but with LeBlender 
            // min/max you can easily get 2+ entries so we'll walk them all
            var newItemValue = new JArray();
            foreach (var properties in control.Value)
            {
                // create object to store resolved properties
                var resolvedProperties = new JObject();
                foreach (var leBlenderPropertyWrapper in properties)
                {
                    if (leBlenderPropertyWrapper.HasValues == false) continue;

                    var leBlenderProperty = leBlenderPropertyWrapper.First.ToObject<LeBlenderGridCellValue>();

                    // get the data type of the property
                    var dataType = _dataTypeService.GetDataTypeDefinitionById(leBlenderProperty.DataTypeGuid);
                    if (dataType == null)
                    {
                        throw new ArgumentNullException(
                            $"Unable to find the data type for editor '{leBlenderProperty.EditorName}' ({leBlenderProperty.EditorAlias} {leBlenderProperty.DataTypeGuid}) referenced by '{property.Alias}'.");
                    }

                    // if it's null or undefined value there is no need to do any more processing
                    if (leBlenderProperty.Value.Type == JTokenType.Null || leBlenderProperty.Value.Type == JTokenType.Undefined)
                    {
                        resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));
                        continue;
                    }

                    Udi udi;
                    // if the value is an Udi then add it as a dependency and move on
                    if (Udi.TryParse(leBlenderProperty.Value.ToString(), out udi))
                    {
                        // if the value was converted with the macro parser it needs to be converted back again
                        // the macro parser should just return the value if it doesn't support the editor
                        leBlenderProperty.Value = _macroParser.ReplaceAttributeValue(leBlenderProperty.Value.ToString(), dataType.PropertyEditorAlias, null, Direction.FromArtifact);
                        resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));
                        continue;
                    }

                    var propertyType = new PropertyType(dataType.PropertyEditorAlias, dataType.DatabaseType, MockPropertyTypeAlias);
                    propertyType.DataTypeDefinitionId = dataType.Id;
                    // get the value connector for the property type
                    var propValueConnector = ValueConnectors.Get(propertyType);
                    // we need to create a mocked property that we can parse in to the value connector
                    var mockProperty = new Property(propertyType);
                    var mockContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> { mockProperty }));

                    if (leBlenderProperty.Value.Type == JTokenType.Array)
                    {
                        // if the value is an array then we should try and convert each item
                        var array = new JArray();
                        foreach (var child in leBlenderProperty.Value)
                        {
                            propValueConnector.SetValue(mockContent, mockProperty.Alias, child.Value<object>().ToString());
                            var convertedValue = mockContent.GetValue(mockProperty.Alias);
                            // if the value stored is json then we should try to deserialize it into an object
                            // because some property editors like the Related Links editor doesn't work unless its done
                            convertedValue = convertedValue is string && convertedValue.ToString().DetectIsJson() ? JsonConvert.DeserializeObject(convertedValue.ToString()) : convertedValue;

                            array.Add(convertedValue);
                        }

                        leBlenderProperty.Value = array;
                    }
                    else
                    {
                        propValueConnector.SetValue(mockContent, mockProperty.Alias, leBlenderProperty.Value.ToString());
                        var convertedValue = mockContent.GetValue(mockProperty.Alias);
                        leBlenderProperty.Value = new JValue(convertedValue);
                    }

                    resolvedProperties.Add(leBlenderProperty.EditorAlias, JObject.FromObject(leBlenderProperty));

                }
                newItemValue.Add(resolvedProperties);
            }

            control.Value = newItemValue;
        }

        public class LeBlenderGridCellValue
        {
            [JsonProperty("value")]
            public JToken Value { get; set; }

            [JsonProperty("dataTypeGuid")]
            public Guid DataTypeGuid { get; set; }

            [JsonProperty("editorAlias")]
            public string EditorAlias { get; set; }

            [JsonProperty("editorName")]
            public string EditorName { get; set; }
        }
    }
}
