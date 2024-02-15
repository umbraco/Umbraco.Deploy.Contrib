using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Deploy.Contrib.Connectors.GridCellValueConnectors;
using Umbraco.Deploy.ValueConnectors;

namespace Umbraco.Deploy.Contrib.Connectors.ValueConnectors
{
    /// <summary>
    /// A Deploy connector for the NestedContent property editor
    /// </summary>
    public class NestedContentValueConnector : IValueConnector
    {
        private readonly IContentTypeService _contentTypeService;
        private readonly Lazy<ValueConnectorCollection> _valueConnectorsLazy;

        public NestedContentValueConnector(IContentTypeService contentTypeService, Lazy<ValueConnectorCollection> valueConnectors)
        {
            if (contentTypeService == null) throw new ArgumentNullException(nameof(contentTypeService));
            if (valueConnectors == null) throw new ArgumentNullException(nameof(valueConnectors));
            _contentTypeService = contentTypeService;
            _valueConnectorsLazy = valueConnectors;
        }

        // Our.Umbraco.NestedContent is the original NestedContent package
        // Umbraco.NestedContent is Core NestedContent (introduced in v7.7)
        public virtual IEnumerable<string> PropertyEditorAliases => new[] { "Our.Umbraco.NestedContent", "Umbraco.NestedContent" };

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
        /// This will iterate through each row of nested content and determine the Property Type for each cell, then it will
        /// resolve the value from the cell's underlying ValueConnector so that the whole thing can be re-serialized/normalized for transfer.
        /// In order to do this we need to create a fake Property to pass in to the underlying IValueConnector.
        /// </remarks>
        public string GetValue(Property property, ICollection<ArtifactDependency> dependencies)
        {
            var value = property.Value as string;

            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.DetectIsJson() == false)
                return null;

            var nestedContent = new List<NestedContentValue>();
            if (value.Trim().StartsWith("{"))
                nestedContent.Add(JsonConvert.DeserializeObject<NestedContentValue>(value));
            else
                nestedContent.AddRange(JsonConvert.DeserializeObject<NestedContentValue[]>(value));

            if (nestedContent.All(x => x == null))
                return null;

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => _contentTypeService.GetContentType(a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}");
            }

            //Ensure that these content types have dependencies added
            foreach (var contentType in allContentTypes.Values)
            {
                dependencies.Add(new ArtifactDependency(contentType.GetUdi(), false, ArtifactDependencyMode.Match));
            }

            foreach (var row in nestedContent)
            {
                var contentType = allContentTypes[row.ContentTypeAlias];

                foreach (var key in row.PropertyValues.Keys.ToArray())
                {
                    // key is a system property that is added by NestedContent in Core v7.7
                    // see note in NestedContentValue - leave it unchanged
                    if (key == "key")
                        continue;

                    var propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                    if (propertyType == null)
                    {
                        LogHelper.Debug<NestedContentValueConnector>($"No property type found with alias {key} on content type {contentType.Alias}");
                        continue;
                    }

                    // throws if not found - no need for a null check
                    var propValueConnector = ValueConnectors.Get(propertyType);

                    // this should be enough for all other value connectors to work with
                    // as all they should need is the value, and the property type infos
                    var mockProperty = new Property(propertyType, row.PropertyValues[key]);

                    object parsedValue = propValueConnector.GetValue(mockProperty, dependencies);

                    // getting Map image value umb://media/43e7401fb3cd48ceaa421df511ec703c to (nothing) - why?!
                    Umbraco.Core.Logging.LogHelper.Debug<NestedContentValueConnector>("Map " + key + " value '" + row.PropertyValues[key] + "' to '" + parsedValue
                        + "' using " + propValueConnector.GetType() + " for " + propertyType);

                    // test if the value is a json object (thus could be a nested complex editor)
                    // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                    JToken jtokenValue = parsedValue != null && parsedValue.ToString().DetectIsJson() ? JToken.Parse(parsedValue.ToString()) : null;
                    if (jtokenValue != null)
                    {
                        parsedValue = jtokenValue;
                    }
                    else if (parsedValue != null)
                    {
                        parsedValue = parsedValue.ToString();
                    }

                    row.PropertyValues[key] = parsedValue;
                }
            }

            value = JsonConvert.SerializeObject(nestedContent);
            return value;
        }

        /// <summary>
        /// Sets a content property value using a deploy property.
        /// </summary>
        /// <param name="content">The content item.</param>
        /// <param name="alias">The property alias.</param>
        /// <param name="value">The deploy property value.</param>
        /// <remarks>
        /// This is a bit tricky because for each cell of nested content we need to pass the value of it to it's related
        /// IValueConnector because each cell is essentially a Property Type - though a fake one.
        /// So to do this we have to create a fake content item for the underlying IValueConnector to set it's value on and
        /// then we can extract that value from it to put on the main serialized object to set on the real IContentBase item.
        /// </remarks>
        public void SetValue(IContentBase content, string alias, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                content.SetValue(alias, value);
                return;
            }

            if (value.DetectIsJson() == false)
                return;

            var nestedContent = JsonConvert.DeserializeObject<NestedContentValue[]>(value);

            if (nestedContent == null)
                return;

            var allContentTypes = nestedContent.Select(x => x.ContentTypeAlias)
                .Distinct()
                .ToDictionary(a => a, a => _contentTypeService.GetContentType(a));

            //Ensure all of these content types are found
            if (allContentTypes.Values.Any(contentType => contentType == null))
            {
                throw new InvalidOperationException($"Could not resolve these content types for the Nested Content property: {string.Join(",", allContentTypes.Where(x => x.Value == null).Select(x => x.Key))}");
            }

            var mocks = new Dictionary<IContentType, IContent>();

            foreach (var row in nestedContent)
            {
                var contentType = allContentTypes[row.ContentTypeAlias];

                // note
                // the way we do it here, doing content.SetValue() several time on the same content, reduces
                // allocations and should be ok because SetValue does not care about the previous value - would
                // be different for the overloads that manage eg files for uploads (not sure how NestedContent
                // deals with them really)

                // we need a fake content instance to pass in to the value connector, since the value connector
                // wants to SetValue on an object - then we can extract the value back from that object to set
                // it correctly on the real instance
                IContent mockContent;
                if (!mocks.TryGetValue(contentType, out mockContent))
                    mockContent = mocks[contentType] = new Content("NC_" + Guid.NewGuid(), -1, contentType);

                foreach (var key in row.PropertyValues.Keys.ToArray())
                {
                    // key is a system property that is added by NestedContent in Core v7.7
                    // see note in NestedContentValue - leave it unchanged
                    if (key == "key")
                        continue;

                    var propertyType = contentType.CompositionPropertyTypes.FirstOrDefault(x => x.Alias == key);

                    if (propertyType == null)
                    {
                        LogHelper.Debug<NestedContentValueConnector>($"No property type found with alias {key} on content type {contentType.Alias}");
                        continue;
                    }

                    // throws if not found - no need for a null check
                    var propValueConnector = ValueConnectors.Get(propertyType);

                    var rowValue = row.PropertyValues[key];

                    if (rowValue != null)
                    {
                        propValueConnector.SetValue(mockContent, propertyType.Alias, rowValue.ToString());
                        var convertedValue = mockContent.GetValue(propertyType.Alias);
                        if (convertedValue == null)
                        {
                            row.PropertyValues[key] = null;
                        }
                        // integers needs to be converted into strings
                        else if (convertedValue is int)
                        {
                            row.PropertyValues[key] = convertedValue.ToString();
                        }
                        else
                        {
                            // test if the value is a json object (thus could be a nested complex editor)
                            // if that's the case we'll need to add it as a json object instead of string to avoid it being escaped
                            JToken jtokenValue = convertedValue.ToString().DetectIsJson() ? JToken.Parse(convertedValue.ToString()) : null;
                            if (jtokenValue != null)
                            {
                                row.PropertyValues[key] = jtokenValue;
                            }
                            else
                            {
                                row.PropertyValues[key] = convertedValue;
                            }
                        }
                    }
                    else
                    {
                        row.PropertyValues[key] = rowValue;
                    }
                }
            }

            // This statement checks whether we have a single item being stored - and if this is stored inside a LeBlender value.
            // For some reason, LeBlender does not store an array of one object (like normal Nested Content would do) if the Nested
            // Content is inside a LeBlender property. Instead it simply stores an object - therefore we need to save single items
            // using JObject instead of JArray, if the current operation is being run from within a wrapping LeBlender property.
            // Note: NestedContent does not use formatting when serializing JSON values.
            if (nestedContent.Length == 1 && content.Properties[0]?.Alias == LeBlenderGridCellValueConnector.MockPropertyTypeAlias)
                value = JObject.FromObject(nestedContent.FirstOrDefault()).ToString(Formatting.None);
            else
                value = JArray.FromObject(nestedContent).ToString(Formatting.None);

            content.SetValue(alias, value);
        }

        /// <summary>
        /// The typed value stored for Nested Content
        /// </summary>
        /// <example>
        /// An example of the JSON stored for NestedContent is:
        /// <![CDATA[
        ///    [
        ///      {"name":"Content","ncContentTypeAlias":"nC1","text":"Hello","multiText":"world","rTE":"<p>asdfasdfasdfasdf</p>\n<p>asdf</p>\n<p><img style=\"width: 213px; height: 213px;\" src=\"/media/1050/profile_pic_cg_2015.jpg?width=213&amp;height=213\" alt=\"\" rel=\"1087\" data-id=\"1087\" /></p>\n<p>asdf</p>"},
        ///      {"name":"Content","ncContentTypeAlias":"nC1","text":"This is ","multiText":"pretty cool","rTE":""}
        ///    ]
        /// ]]>
        /// </example>
        public class NestedContentValue
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("ncContentTypeAlias")]
            public string ContentTypeAlias { get; set; }

            // starting with v7.7, Core's NestedContent implement "key" as a system property
            // but since we are supporting pre-v7.7 including the NestedContent package, we
            // cannot do it this way - it's all managed "manually" when dealing with
            // PropertyValues.
            //[JsonProperty("key")]
            //public Guid Key { get; set; }

            /// <summary>
            /// The remaining properties will be serialized to a dictionary
            /// </summary>
            /// <remarks>
            /// The JsonExtensionDataAttribute is used to put the non-typed properties into a bucket
            /// http://www.newtonsoft.com/json/help/html/DeserializeExtensionData.htm
            /// NestedContent serializes to string, int, whatever eg
            ///   "stringValue":"Some String","numericValue":125,"otherNumeric":null
            /// </remarks>
            [JsonExtensionData]
            public IDictionary<string, object> PropertyValues { get; set; }
        }
    }
}
