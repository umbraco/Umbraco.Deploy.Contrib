using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    public class TupleValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        public TupleValueConnector(IDataTypeService dataTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            Mandate.ParameterNotNull(dataTypeService, nameof(dataTypeService));
            Mandate.ParameterNotNull(valueConnectors, nameof(valueConnectors));

            _dataTypeService = dataTypeService;
            _valueConnectorsLazy = valueConnectors;
        }

        public IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.Tuple" };

        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            // get the property value
            var value = property.Value?.ToString();
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // deserialize it
            var items = JsonConvert.DeserializeObject<List<TupleValueItem>>(value);
            if (items == null || items.Count == 0)
                return null;

            // loop through each value
            foreach (var item in items)
            {
                // get the selected data-type (and ensure it exists)
                var dataType = _dataTypeService.GetDataTypeDefinitionById(item.DataTypeGuid);

                if (dataType == null)
                {
                    LogHelper.Warn<TupleValueConnector>($"Could not resolve the data-type used by the Property List value for: {property.Alias}, with GUID: {item.DataTypeGuid}");
                    continue;
                }

                // add the selected data-type as a dependency
                dependencies.Add(new ArtifactDependency(dataType.GetUdi(), false, ArtifactDependencyMode.Match));

                // make a property-type to use in a mocked Property
                // and get the value-connector needed to parse values (outside the loop, as it's the same for all iterations)
                var propertyType = new PropertyType(dataType);
                var valueConnector = ValueConnectors.Get(propertyType);

                item.Value = valueConnector.GetValue(new Property(propertyType, item.Value), dependencies);
            }

            return JsonConvert.SerializeObject(items);
        }

        public void SetValue(IContentBase content, string alias, string value)
        {
            // take the value
            if (string.IsNullOrWhiteSpace(value))
                return;

            // deserialize it
            var items = JsonConvert.DeserializeObject<List<TupleValueItem>>(value);
            if (items == null || items.Count == 0)
                return;

            // loop through each value
            foreach (var item in items)
            {
                // get the selected data-type (and ensure it exists)
                var dataType = _dataTypeService.GetDataTypeDefinitionById(item.DataTypeGuid);

                if (dataType == null)
                {
                    LogHelper.Warn<TupleValueConnector>($"Could not resolve the data-type used by the Tuple item for: {alias}, with GUID: {item.DataTypeGuid}");
                    continue;
                }

                // make a property-type to use in a mocked Property
                // and get the value-connector needed to parse values (outside the loop, as it's the same for all iterations)
                var propertyType = new PropertyType(dataType, "mockTupleAlias");
                var valueConnector = ValueConnectors.Get(propertyType);

                var mockProperty = new Property(propertyType);
                var mockContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> { mockProperty }));

                // pass it to its own value-connector
                // NOTE: due to how ValueConnector.SetValue() works, we have to pass the mock item
                // through to the connector to have it do its work on parsing the value on the item itself.
                valueConnector.SetValue(mockContent, mockProperty.Alias, item.Value?.ToString());

                // get the value back and assign
                item.Value = mockContent.GetValue(mockProperty.Alias);
            }

            // serialize the JSON values
            content.SetValue(alias, JArray.FromObject(items).ToString(Formatting.None));
        }

        public class TupleValueItem
        {
            [JsonProperty("key")]
            public Guid Key { get; set; }

            [JsonProperty("dtd")]
            public Guid DataTypeGuid { get; set; }

            [JsonProperty("value")]
            public object Value { get; set; }
        }
    }
}