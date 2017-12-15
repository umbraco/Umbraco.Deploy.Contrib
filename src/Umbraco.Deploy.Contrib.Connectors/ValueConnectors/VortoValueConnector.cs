using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy ValueConnector for the Vorto property editor
    /// </summary>
    public class VortoValueConnector : IValueConnector
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        internal static string MockPropertyTypeAlias = "mockVortoPropertyTypeAlias";

        public VortoValueConnector(IDataTypeService dataTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            if (dataTypeService == null) throw new ArgumentNullException(nameof(dataTypeService));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            _dataTypeService = dataTypeService;
            _valueConnectorsLazy = valueConnectors;
        }

        public virtual IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.Vorto" };

        // cannot inject ValueConnectorCollection else of course it creates a circular (recursive) dependency,
        // so we have to inject it lazily and use the lazy value when actually needing it
        private ValueConnectorCollection ValueConnectors => _valueConnectorsLazy.Value;

        /// <summary>
        /// Gets the deploy property corresponding to a content property.
        /// </summary>
        /// <param name="property">The content property.</param>
        /// <param name="dependencies">The content dependencies.</param>
        /// <returns>The deploy property value.</returns>
        /// <remarks>
        /// This will iterate through each language of a Vorto property and resolve the values using the ValueConnector
        /// matching the property type being wrapped by Vorto.
        /// In order to do this we need to create fake items with properties to pass in to the underlying IValueConnector
        /// and then make sure all the resolved values are put back on the original item afterwards in the corresponding
        /// language property of the JSON blob.
        /// </remarks>
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            // do various null/empty checks to avoid doing work we don't need to do.
            var value = property.Value as string;
            
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.DetectIsJson() == false)
                return null;

            // try parsing this as a Vorto value
            var vortoValue = JsonConvert.DeserializeObject<VortoValue>(value);
            if (vortoValue?.Values?.ValuePairs == null)
                return null;

            // get the Vorto datatype
            var vortoDataType = _dataTypeService.GetDataTypeDefinitionById(Guid.Parse(vortoValue.DtdGuid));

            // fetch the prevalues containing metadata about what Vorto is wrapping
            var vortoDataTypePrevalueJson = _dataTypeService.GetPreValuesCollectionByDataTypeId(vortoDataType.Id).FormatAsDictionary().FirstOrDefault(x => x.Key == "dataType").Value.Value;
            var vortoDataTypePrevalue = JsonConvert.DeserializeObject<VortoDatatypePrevalue>(vortoDataTypePrevalueJson);

            // get the actual datatype wrapped by Vorto
            var wrappedDataType = _dataTypeService.GetDataTypeDefinitionById(vortoDataTypePrevalue.Guid);

            // ensure the wrapped datatype is found
            if (wrappedDataType == null)
                throw new InvalidOperationException($"Could not resolve the datatype used inside the Vorto property: {property.Alias}");

            // add the wrapped datatype as a dependency
            dependencies.Add(new ArtifactDependency(wrappedDataType.GetUdi(), false, ArtifactDependencyMode.Match));

            // make a property type to use in mock property and get the value connector needed to parse values.
            // we know the value connector is the same for all iterations, as Vorto only wraps the same single datatype for every language.
            var propertyType = new PropertyType(wrappedDataType);
            var valueConnector = ValueConnectors.Get(propertyType);

            // iterate languages and create a mock property to pass through the valueConnector to get a parsedValue.
            foreach (var languageKey in vortoValue.Values.ValuePairs.Keys.ToArray())
            {
                var val = vortoValue.Values.ValuePairs[languageKey];

                var mockProperty = new Property(propertyType, val);

                object parsedValue = valueConnector.GetValue(mockProperty, dependencies);

                // test if the value is a json object (thus could be a nested complex editor)
                // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                var jtokenValue = parsedValue != null && parsedValue.ToString().DetectIsJson() ? JToken.Parse(parsedValue.ToString()) : null;
                if (jtokenValue != null)
                {
                    parsedValue = jtokenValue;
                }
                else if (parsedValue != null)
                {
                    parsedValue = parsedValue.ToString();
                }
                // set the parsed value back onto the original object.
                vortoValue.Values.ValuePairs[languageKey] = parsedValue;
            }
            value = JsonConvert.SerializeObject(vortoValue);
            return value;
        }

        /// <summary>
        /// Sets a content property value using a deploy property.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="value">The deploy property value.</param>
        /// <remarks>
        /// This will loop through all the values stored inside the Vorto property, using the IValueConnector to parse each value.
        /// Due to how the ValueConnectors set the value, we have to create mocked content items and assign the value to it.
        /// Then the mock item is passed to the ValueConnector, matching the property type being wrapped by Vorto and the
        /// ValueConnector will parse the value.
        /// When the ValueConnector has done its work, the parsed value is extracted from the mock content and then assigned back
        /// to the original field inside the Vorto value object instead.
        /// </remarks>
        public void SetValue(IContentBase content, string alias, string value)
        {
            // if there's an empty value we don't need to do more work
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            // fail fast if this isn't JSON
            if (value.DetectIsJson() == false)
                return;

            // try parsing this as a Vorto value
            var vortoValue = JsonConvert.DeserializeObject<VortoValue>(value);
            if (vortoValue == null)
                return;

            // getting the wrapped datatype via the Vorto datatype
            var vortoDataType = _dataTypeService.GetDataTypeDefinitionById(Guid.Parse(vortoValue.DtdGuid));
            var vortoDataTypePrevalueJson = _dataTypeService.GetPreValuesCollectionByDataTypeId(vortoDataType.Id).FormatAsDictionary().FirstOrDefault(x => x.Key == "dataType").Value.Value;
            var vortoDataTypePrevalue = JsonConvert.DeserializeObject<VortoDatatypePrevalue>(vortoDataTypePrevalueJson);

            var wrappedDataType = _dataTypeService.GetDataTypeDefinitionById(vortoDataTypePrevalue.Guid);

            // ensure the wrapped datatype is found
            if (wrappedDataType == null)
                throw new InvalidOperationException($"Could not resolve the datatype used inside the Vorto property: {alias}");

            var propertyType = new PropertyType(wrappedDataType, MockPropertyTypeAlias);
            var valueConnector = ValueConnectors.Get(propertyType);

            // iterate values for each language
            foreach (var languageKey in vortoValue.Values.ValuePairs.Keys.ToArray())
            {
                var val = vortoValue.Values.ValuePairs[languageKey];

                var mockProperty = new Property(propertyType, val);
                var mockContent = new Content("mockContent", -1, new ContentType(-1), new PropertyCollection(new List<Property> {mockProperty}));

                // due to how ValueConnector.SetValue() works, we have to pass the mock item through the connector to have it do its
                // work on parsing the value on the item itself.
                valueConnector.SetValue(mockContent, mockProperty.Alias, val.ToString());

                // we then extract the converted value from the mock item so we can assign it to the inner value object inside the
                // actual Vorto item's value pair for this specific language.
                var convertedValue = mockContent.GetValue(mockProperty.Alias);

                var jtokenValue = convertedValue != null && convertedValue.ToString().DetectIsJson() ? JToken.Parse(convertedValue.ToString()) : null;
                if (jtokenValue != null)
                {
                    convertedValue = jtokenValue;
                }
                else if (convertedValue != null)
                {
                    convertedValue = convertedValue.ToString();
                }
                vortoValue.Values.ValuePairs[languageKey] = convertedValue;
            }
            
            // Vorto does not use formatting when serializing JSON values
            value = JObject.FromObject(vortoValue).ToString(Formatting.None);
            content.SetValue(alias, value);
        }

        /// <summary>
        /// The typed value stored for Vorto properties
        /// </summary>
        /// <example>
        /// Two examples of the JSON stored for a Vorto property (first is a textstring, second is a mntp).
        /// Its values consists of key/value pairs of: "language":"value"
        /// <![CDATA[
        ///    {"values":{"en-US":"english text","da-DK":"danish text"},"dtdGuid":"06640ede-8e43-4b26-9be2-ceb45b1e37a3"}
        ///
        ///    {"values":{"en-US":"1198","da-DK":"1215"},"dtdGuid":"c812b8ac-30a6-4b82-90a8-c2dad1781024"}
        /// ]]>
        /// </example>
        
        internal class VortoValue
        {
            [JsonProperty("values")]
            public Values Values { get; set; }
            [JsonProperty("dtdGuid")]
            public string DtdGuid { get; set; }
        }

        internal class Values
        {
            /// <summary>
            /// The value properties will be serialized to a dictionary
            /// </summary>
            /// <remarks>
            /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
            /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
            /// Vorto stores language/value pairs so these are unknown at compile time and can't be accessed as typed properties.
            /// </remarks>
            [JsonExtensionData]
            public IDictionary<string, object> ValuePairs { get; set; }
        }

        /// <summary>
        /// This defines the wrapped datatype used inside the Vorto property.
        /// </summary>
        internal class VortoDatatypePrevalue
        {
            [JsonProperty("guid")]
            public Guid Guid { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("propertyEditorAlias")]
            public string PropertyEditorAlias { get; set; }
        }
    }
}