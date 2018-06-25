using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    public class PropertyListValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;

        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        public PropertyListValueConnector(IDataTypeService dataTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            Mandate.ParameterNotNull(dataTypeService, nameof(dataTypeService));
            Mandate.ParameterNotNull(valueConnectors, nameof(valueConnectors));

            _dataTypeService = dataTypeService;
            _valueConnectorsLazy = valueConnectors;
        }

        public IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.PropertyList" };

        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            // get the property value
            var value = property.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // deserialize it
            var model = JsonConvert.DeserializeObject<PropertyListValue>(value);
            if (model == null)
                return null;

            // get the selected data-type (and ensure it exists)
            var dataType = _dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);

            if (dataType == null)
                throw new InvalidOperationException($"Could not resolve the data-type used by the Property List value for: {property.Alias}");

            // add the selected data-type as a dependency
            dependencies.Add(new ArtifactDependency(dataType.GetUdi(), false, ArtifactDependencyMode.Match));

            // make a property-type to use in a mocked Property
            // and get the value-connector needed to parse values (outside the loop, as it's the same for all iterations)
            var propertyType = new PropertyType(dataType);
            var valueConnector = ValueConnectors.Get(propertyType);

            // loop through each value
            for (int i = 0; i < model.Values.Count; i++)
            {
                // pass it to its own value-connector
                // set the parsed value back onto the original object, (it may be a string representing more json. which is fine)
                model.Values[i] = valueConnector.GetValue(new Property(propertyType, model.Values[i]), dependencies);
            }

            return JsonConvert.SerializeObject(model);
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            // take the value
            if (string.IsNullOrWhiteSpace(value))
                return;

            // deserialize it
            var model = JsonConvert.DeserializeObject<PropertyListValue>(value);
            if (model == null)
                return;

            // get the selected data-type (and ensure it exists)
            var dataType = _dataTypeService.GetDataTypeDefinitionById(model.DataTypeGuid);

            if (dataType == null)
                throw new InvalidOperationException($"Could not resolve the data-type used by the Property List value for: {alias}");

            // make a property-type to use in a mocked Property
            // and get the value-connector needed to parse values (outside the loop, as it's the same for all iterations)
            var propertyType = new PropertyType(dataType, "mockPropertyListAlias");
            var valueConnector = ValueConnectors.Get(propertyType);

            // loop through each value
            for (int i = 0; i < model.Values.Count; i++)
            {
                var item = model.Values[i];

                var mockProperty = new Property(propertyType);
                var mockContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> { mockProperty }));

                // pass it to its own value-connector
                // NOTE: due to how ValueConnector.SetValue() works, we have to pass the mock item
                // through to the connector to have it do its work on parsing the value on the item itself.
                valueConnector.SetValue(mockContent, mockProperty.Alias, item?.ToString());

                // get the value back and assign
                model.Values[i] = mockContent.GetValue(mockProperty.Alias);
            }

            // serialize the JSON values
            content.SetValue(alias, JObject.FromObject(model).ToString());
        }

        public class PropertyListValue
        {
            [JsonProperty("dtd")]
            public Guid DataTypeGuid { get; set; }

            [JsonProperty("values")]
            public List<object> Values { get; set; }
        }
    }
}